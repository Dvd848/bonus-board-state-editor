using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace bonus_cheat
{

    public enum AllocationProtectEnum : uint
    {
        PAGE_EXECUTE = 0x00000010,
        PAGE_EXECUTE_READ = 0x00000020,
        PAGE_EXECUTE_READWRITE = 0x00000040,
        PAGE_EXECUTE_WRITECOPY = 0x00000080,
        PAGE_NOACCESS = 0x00000001,
        PAGE_READONLY = 0x00000002,
        PAGE_READWRITE = 0x00000004,
        PAGE_WRITECOPY = 0x00000008,
        PAGE_GUARD = 0x00000100,
        PAGE_NOCACHE = 0x00000200,
        PAGE_WRITECOMBINE = 0x00000400
    }

    public enum StateEnum : uint
    {
        MEM_COMMIT = 0x1000,
        MEM_FREE = 0x10000,
        MEM_RESERVE = 0x2000
    }

    public enum TypeEnum : uint
    {
        MEM_IMAGE = 0x1000000,
        MEM_MAPPED = 0x40000,
        MEM_PRIVATE = 0x20000
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MEMORY_BASIC_INFORMATION
    {
        public IntPtr BaseAddress;
        public IntPtr AllocationBase;
        public AllocationProtectEnum AllocationProtect;
        public IntPtr RegionSize;
        public StateEnum State;
        public AllocationProtectEnum Protect;
        public TypeEnum Type;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MEMORY_BASIC_INFORMATION64
    {
        public ulong BaseAddress;
        public ulong AllocationBase;
        public int AllocationProtect;
        public int __alignment1;
        public ulong RegionSize;
        public int State;
        public int Protect;
        public int Type;
        public int __alignment2;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SYSTEM_INFO
    {
        public ushort processorArchitecture;
        ushort reserved;
        public uint pageSize;
        public IntPtr minimumApplicationAddress;
        public IntPtr maximumApplicationAddress;
        public IntPtr activeProcessorMask;
        public uint numberOfProcessors;
        public uint processorType;
        public uint allocationGranularity;
        public ushort processorLevel;
        public ushort processorRevision;
    }


    internal class ProcessMemoryWriter
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

        [DllImport("kernel32.dll")]
        static extern void GetSystemInfo(out SYSTEM_INFO lpSystemInfo);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory
        (IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint dwSize, ref uint lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory
        (IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint dwSize, ref uint lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        static extern uint GetLastError();

        Process process;
        public ProcessMemoryWriter(string processName)
        {
            this.process = GetProcessByName(processName);
        }

        public List<IntPtr> LocateWritableMemoryRangeBySize(uint size)
        {
            List <IntPtr> res = new List<IntPtr> ();

            SYSTEM_INFO sysInfo;
            GetSystemInfo(out sysInfo);

            IntPtr MaxAddress = sysInfo.maximumApplicationAddress;
            IntPtr currentAddress = sysInfo.minimumApplicationAddress;

            do
            {
                MEMORY_BASIC_INFORMATION memInfo;
                int result = VirtualQueryEx(this.process.Handle, currentAddress, out memInfo, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION)));
                if (result == 0)
                {
                    break;
                }

                if (memInfo.RegionSize.ToInt64() == size && memInfo.AllocationProtect == AllocationProtectEnum.PAGE_READWRITE)
                {
                    res.Add (memInfo.BaseAddress);
                    Debug.WriteLine(String.Format("Found base address 0x{0:X} of size 0x{1:X} for process handle 0x{2:X}", memInfo.BaseAddress,
                        size, this.process.Handle));
                }

                currentAddress = new IntPtr(memInfo.BaseAddress.ToInt64() + memInfo.RegionSize.ToInt64()); ;
            } while (currentAddress.CompareTo(MaxAddress) < 0);

            return res;
        }

        public byte[] ReadMemory(IntPtr address, uint size)
        {
            uint bytesRead = 0;
            byte[] buffer = new byte[size];

            if ( (ReadProcessMemory(this.process.Handle, address, buffer, size, ref bytesRead) == false) || (bytesRead != size) )
            {
                throw new Exception(String.Format("Could not read {0} bytes from address 0x{1:X}", size, address));
            }

            return buffer;
        }

        public void WriteMemory(IntPtr address, byte[] buffer)
        {
            uint bytesWritten = 0;

            if ((WriteProcessMemory(this.process.Handle, address, buffer, (uint)buffer.Length, ref bytesWritten) == false) || (bytesWritten != buffer.Length))
            {
                uint error = GetLastError();
                throw new Exception(String.Format("Could not write {0} bytes to address 0x{1:X} (Error: 0x{2:X})", buffer.Length, address, error));
            }

        }

        private static Process GetProcessByName(string name)
        {
            Process[] processes = Process.GetProcessesByName(name);
            if (processes.Length == 0)
            {
                throw new InvalidOperationException(String.Format("Can't find process '{0}'", name));
            }
            else if (processes.Length > 1)
            {
                throw new InvalidOperationException(String.Format("Too many instances of '{0}'", name));
            }

            return processes[0];

        }
    }
}
