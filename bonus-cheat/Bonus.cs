using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bonus_cheat
{
    internal class Bonus
    {
        const string DOSBOX_PROCESS_NAME = "DOSBox";
        const uint DOSBOX_MEMSIZE = 0x1001000;

        const int BONUS_MEM_OFFSET_PLAYER1_LETTERS = 0x4C404;
        const int BONUS_MEM_OFFSET_PLAYER2_LETTERS = 0x4C40D;
        const int BONUS_MEM_OFFSET_RACK = 0x4C434;
        const int BONUS_MEM_OFFSET_BOARD = 0x4C444;

        const int BONUS_NUM_PLAYER_LETTERS = 8;
        const int BONUS_BOARD_SIZE = 0x9b;

        public const int BONUS_BOARD_DIMENTION = 12;

        const int CODE_PAGE_862_START = 0x80;
        const int CODE_PAGE_862_END = 0x9A;

        private ProcessMemoryWriter memoryWriter;
        private IntPtr baseAddress;

        private static readonly char[] CodePage862ToUTF8Mapping = new char[]
        {
            // Custom mapping from Code Page 862 to UTF-8
            'א', 'ב', 'ג', 'ד', 'ה', 'ו', 'ז', 'ח', 'ט', 'י', 'ך', 'כ', 'ל', 'ם', 'מ', 'ן',
            'נ', 'ס', 'ע', 'ף', 'פ', 'ץ', 'צ', 'ק', 'ר', 'ש', 'ת'
        };

        public enum Player
        {
            Player1,
            Player2
        }

        public Bonus()
        {
            this.memoryWriter = new ProcessMemoryWriter(DOSBOX_PROCESS_NAME);
            List<IntPtr> baseAddresses = this.memoryWriter.LocateWritableMemoryRangeBySize(DOSBOX_MEMSIZE);
            if (baseAddresses.Count != 1)
            {
                throw new Exception("Can't identify memory range for DOSBox memory");
            }

            this.baseAddress = baseAddresses[0];

            /*
            byte[] p1letters = this.memoryWriter.ReadMemory(baseAddress + BONUS_MEM_OFFSET_PLAYER1_LETTERS, BONUS_NUM_PLAYER_LETTERS);
            byte[] p2letters = this.memoryWriter.ReadMemory(baseAddress + BONUS_MEM_OFFSET_PLAYER2_LETTERS, BONUS_NUM_PLAYER_LETTERS);
            byte[] rack = this.memoryWriter.ReadMemory(baseAddress + BONUS_MEM_OFFSET_RACK, BONUS_NUM_PLAYER_LETTERS);

            byte[] board = this.memoryWriter.ReadMemory(baseAddress + BONUS_MEM_OFFSET_BOARD, BONUS_BOARD_SIZE);

            char[] letters = getPlayerLetters(Player.Player1);
            char[,] boardArr = getBoard();
            */

            //this.memoryWriter.WriteMemory(baseAddress + BONUS_MEM_OFFSET_PLAYER1_LETTERS, new byte[]{ 0x90, 0x91});

        }

        public char[] getPlayerLetters(Player player)
        {
            char[] res = new char[BONUS_NUM_PLAYER_LETTERS];
            int offset = getPlayerMemOffset(player);

            byte[] letters = this.memoryWriter.ReadMemory(baseAddress + offset, BONUS_NUM_PLAYER_LETTERS);
            
            for (var i = 0; i < res.Length; i++)
            {
                res[i] = ConvertCodePage862ToUTF8(letters[i]);
            }

            return res;
        }


        public void setPlayerLetter(Player player, int index, char letter)
        {
            if (index < 0 || index > BONUS_NUM_PLAYER_LETTERS)
            {
                throw new ArgumentException("Invalid index for setting player letter");
            }

            byte b = ConvertUTF8ToCodePage862(letter);

            this.memoryWriter.WriteMemory(baseAddress + getPlayerMemOffset(player) + index, new byte[] { b });
        }
        public char[] getRackLetters()
        {
            char[] res = new char[BONUS_NUM_PLAYER_LETTERS];

            byte[] letters = this.memoryWriter.ReadMemory(baseAddress + BONUS_MEM_OFFSET_RACK, BONUS_NUM_PLAYER_LETTERS);

            for (var i = 0; i < res.Length; i++)
            {
                res[i] = ConvertCodePage862ToUTF8(letters[i]);
            }

            return res;
        }

        public char[,] getBoard()
        {
            char[,] res = new char[BONUS_BOARD_DIMENTION, BONUS_BOARD_DIMENTION];

            byte[] letters = this.memoryWriter.ReadMemory(baseAddress + BONUS_MEM_OFFSET_BOARD, BONUS_BOARD_SIZE);

            int index = 0;
            for (var r = 0; r < BONUS_BOARD_DIMENTION; r++)
            {
                for (var c = 0; c < BONUS_BOARD_DIMENTION; c++)
                {
                    res[r,c] = ConvertCodePage862ToUTF8(letters[index]);
                    index++;
                }
                index++;
            }

            return res;
        }

        public void setBoardLetter(int row, int col, char letter)
        {
            if ( (row < 0 || row > BONUS_BOARD_DIMENTION) || (col < 0 || col > BONUS_BOARD_DIMENTION) )
            {
                throw new ArgumentException("Invalid index for setting board letter");
            }

            byte b = ConvertUTF8ToCodePage862(letter);
            int index = row * BONUS_BOARD_DIMENTION + col;
            index = index + (index / BONUS_BOARD_DIMENTION);

            Debug.Assert(this.memoryWriter.ReadMemory(baseAddress + BONUS_MEM_OFFSET_BOARD + index, 1)[0] != 0x0);

            this.memoryWriter.WriteMemory(baseAddress + BONUS_MEM_OFFSET_BOARD + index, new byte[] { b });
        }

        private int getPlayerMemOffset(Player player)
        {
            return new Dictionary<Player, int>
            {
                { Player.Player1, BONUS_MEM_OFFSET_PLAYER1_LETTERS },
                { Player.Player2, BONUS_MEM_OFFSET_PLAYER2_LETTERS }
            }[player];
        }

        private static char ConvertCodePage862ToUTF8(byte codePage862Byte)
        {
            if (codePage862Byte == 0x20)
            {
                return ' ';
            }
            else if (codePage862Byte == 0x30)
            {
                return '#';
            }

            if (codePage862Byte < CODE_PAGE_862_START || codePage862Byte > CODE_PAGE_862_END)
            {
                throw new ArgumentException("Invalid Code Page 862 byte");
            }

            return CodePage862ToUTF8Mapping[codePage862Byte - CODE_PAGE_862_START];
        }

        private static byte ConvertUTF8ToCodePage862(char utf8Char)
        {
            if (utf8Char == ' ')
            {
                return 0x20;
            }

            int index = Array.IndexOf(CodePage862ToUTF8Mapping, utf8Char);
            if (index == -1)
            {
                throw new ArgumentException("Invalid UTF-8 character");
            }

            // Get the corresponding Code Page 862 byte
            return (byte)(index + CODE_PAGE_862_START);
        }
    }
}
