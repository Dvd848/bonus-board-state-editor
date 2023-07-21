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

        public const int BONUS_NUM_PLAYER_LETTERS = 8;
        const int BONUS_BOARD_SIZE = 0x9b;

        public const int BONUS_BOARD_DIMENTION = 12;

        const int CODE_PAGE_862_START = 0x80;
        const int CODE_PAGE_862_END = 0x9A;

        private ProcessMemoryWriter memoryWriter;
        private IntPtr baseAddress;

        private char[] letters;

        private static readonly char[] CodePage862ToUTF8Mapping = new char[]
        {
            // Custom mapping from Code Page 862 to UTF-8
            'א', 'ב', 'ג', 'ד', 'ה', 'ו', 'ז', 'ח', 'ט', 'י', 'ך', 'כ', 'ל', 'ם', 'מ', 'ן',
            'נ', 'ס', 'ע', 'ף', 'פ', 'ץ', 'צ', 'ק', 'ר', 'ש', 'ת'
        };

        private static readonly ISet<char> BonusInvalidLetters = new HashSet<char>
        {
            'ך', 'ם', 'ן', 'ף', 'ץ'
        };

        public enum Entity
        {
            Player1,
            Player2,
            Rack
        }

        public enum SpecialChars
        {
            Space = ' ',
            Blocked = '#'
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

            letters = new char[CodePage862ToUTF8Mapping.Length - BonusInvalidLetters.Count];
            int index = 0;

            foreach (var letter in CodePage862ToUTF8Mapping)
            {
                if (!BonusInvalidLetters.Contains(letter))
                {
                    letters[index++] = letter;
                }
            }
        }

        public char[] GetEntityLetters(Entity entity)
        {
            char[] res = new char[BONUS_NUM_PLAYER_LETTERS];
            int offset = GetEntityMemOffset(entity);

            byte[] letters = this.memoryWriter.ReadMemory(baseAddress + offset, BONUS_NUM_PLAYER_LETTERS);
            
            for (var i = 0; i < res.Length; i++)
            {
                res[i] = ConvertCodePage862ToUTF8(letters[i]);
            }

            return res;
        }


        public void SetEntityLetter(Entity entity, int index, char letter)
        {
            if (index < 0 || index > BONUS_NUM_PLAYER_LETTERS)
            {
                throw new ArgumentException("Invalid index for setting entity letter");
            }

            byte b = ConvertUTF8ToCodePage862(letter);

            this.memoryWriter.WriteMemory(baseAddress + GetEntityMemOffset(entity) + index, new byte[] { b });
        }

        public char[,] GetBoard()
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

        public void SetBoardLetter(int row, int col, char letter)
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

        public char[] Letters
        {
            get
            {
                return this.letters;
            }
        }

        private int GetEntityMemOffset(Entity player)
        {
            return new Dictionary<Entity, int>
            {
                { Entity.Player1, BONUS_MEM_OFFSET_PLAYER1_LETTERS },
                { Entity.Player2, BONUS_MEM_OFFSET_PLAYER2_LETTERS },
                { Entity.Rack,    BONUS_MEM_OFFSET_RACK }
            }[player];
        }

        private static char ConvertCodePage862ToUTF8(byte codePage862Byte)
        {
            if (codePage862Byte == 0x20)
            {
                return ((char)SpecialChars.Space);
            }
            else if ( (codePage862Byte >= 0x30) && (codePage862Byte <= 0x34) )
            {
                return ((char)SpecialChars.Blocked);
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

            return (byte)(index + CODE_PAGE_862_START);
        }
    }
}
