
#include <windows.h>
#include <tlhelp32.h>
#include <stdio.h>

#define DOSBOX_MEMSIZE                      0x1001000

#define BONUS_MEM_OFFSET_PLAYER1_LETTERS    0x4C404
#define BONUS_MEM_OFFSET_PLAYER2_LETTERS    0x4C40D
#define BONUS_MEM_OFFSET_RACK               0x4C434
#define BONUS_MEM_OFFSET_BOARD              0x4C444

#define BONUS_NUM_PLAYER_LETTERS            8
#define BONUS_BOARD_SIZE                    0x9C
#define BONUS_BOARD_DIMENTIONS              12

#define CODE_PAGE_862_START                 0x80

void PrintBuffer(const char* title, const unsigned char* buffer, size_t size, int columnLength) 
{
    printf("%s:\n", title);
    for (size_t i = 0; i < size; i++) 
    {
        printf("%02X ", buffer[i]);
        if ((i + 1) % columnLength == 0 || i == size - 1)
        {
            printf("\n");
        }
    }
}

HANDLE GetProcessHandleByName(const wchar_t* pProcName)
{
    PROCESSENTRY32W entry;
    entry.dwSize = sizeof(entry);

    HANDLE snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
    if (snapshot == INVALID_HANDLE_VALUE) 
    {
        return NULL;
    }

    if (Process32FirstW(snapshot, &entry) == TRUE)
    {
        do
        {
            if (_wcsicmp(entry.szExeFile, pProcName) == 0)
            {
                HANDLE hProcess = OpenProcess(PROCESS_ALL_ACCESS, FALSE, entry.th32ProcessID);

                return hProcess;
            }

        } while (Process32NextW(snapshot, &entry) == TRUE);
    }

    CloseHandle(snapshot);
    return NULL;
}

PVOID FindgameMemoryStartEx(char* begin, intptr_t size, HANDLE hProc, SIZE_T targetMemorySize)
{
    MEMORY_BASIC_INFORMATION mbi;
    SIZE_T bytesRead;

    bytesRead = VirtualQueryEx(hProc, (LPCVOID)begin, &mbi, sizeof(mbi));
    if (bytesRead == 0)
    {
        return NULL;
    }

    for (char* curr = begin; curr < begin + size; curr += mbi.RegionSize)
    {
        bytesRead = VirtualQueryEx(hProc, curr, &mbi, sizeof(mbi));
        if ( (bytesRead == 0) || (mbi.AllocationProtect != PAGE_READWRITE) )
        {
            continue;
        }

        if (mbi.RegionSize == targetMemorySize)
        {
            return mbi.BaseAddress;
        }
    }

    return NULL;
}

PVOID FindgameMemoryStart(HANDLE hProc, SIZE_T targetMemorySize)
{
    BOOL IsWow64Proc = FALSE;
    unsigned long long int kernelMemory;

    if (!IsWow64Process(hProc, &IsWow64Proc))
    {
        return NULL;
    }

    kernelMemory = IsWow64Proc ? 0x80000000 : 0x800000000000;

    return FindgameMemoryStartEx(0x0, (intptr_t)kernelMemory, hProc, targetMemorySize);
}

int ReadMemory(HANDLE hProc, PVOID baseAddress, size_t size, PVOID destAddress, size_t destSize)
{
    SIZE_T bytesRead;
    
    if ( (baseAddress == NULL) || (size == 0) || (destAddress == NULL) || (destAddress == 0) || (size < destSize) )
    {
        return -1;
    }

    if ( (ReadProcessMemory(hProc, baseAddress, destAddress, size, &bytesRead) == FALSE) || (bytesRead != size) )
    {
        return -1;
    }

    return 0;
}

void PrintHebrewMapping() 
{
    char* hebrewLetters[] = 
    {
        "Alef", "Bet", "Gimel", "Dalet", "He", "Vav", "Zayin", "Het",
        "Tet", "Yod", "Final Kaf", "Kaf", "Lamed", "Final Mem", "Mem", "Final Nun",
        "Nun", "Samekh", "Ayin", "Final Pe", "Pe", "Final Tsade", "Tsade", "Qof",
        "Resh", "Shin", "Tav"
    };

    int numLetters = sizeof(hebrewLetters) / sizeof(hebrewLetters[0]);

    printf("Hebrew Letter Mapping:\n");
    for (int i = 0; i < numLetters; i++)
    {
        int codePageValue = CODE_PAGE_862_START + i;
        printf("%02X: %-15s", codePageValue, hebrewLetters[i]);
        if ((i + 1) % 3 == 0) 
        {
            printf("\n");
        }
    }
    printf("\n");
}

int main()
{
    HANDLE dosboxHandle     = NULL;
    PVOID  gameMemoryStart  = NULL;

    int    status           = EXIT_SUCCESS;

    char player1Letters[BONUS_NUM_PLAYER_LETTERS];
    char player2Letters[BONUS_NUM_PLAYER_LETTERS];
    char board[BONUS_BOARD_SIZE];

    dosboxHandle = GetProcessHandleByName(L"DOSBox.exe");
    if (dosboxHandle == NULL)
    {
        fprintf(stderr, "Can't find DOSBox process\n");
        status = EXIT_FAILURE;
        goto exit;
    }

    gameMemoryStart = FindgameMemoryStart(dosboxHandle, DOSBOX_MEMSIZE);
    if (gameMemoryStart == NULL)
    {
        fprintf(stderr, "Can't find DOSBox game memory start\n");
        status = EXIT_FAILURE;
        goto exit;
    }

    if (ReadMemory(dosboxHandle, (char*)gameMemoryStart + BONUS_MEM_OFFSET_PLAYER1_LETTERS, 
                    BONUS_NUM_PLAYER_LETTERS, player1Letters, sizeof(player1Letters)) != 0)
    {
        fprintf(stderr, "Can't read player 1 letters\n");
        status = EXIT_FAILURE;
        goto exit;
    }

    if (ReadMemory(dosboxHandle, (char*)gameMemoryStart + BONUS_MEM_OFFSET_PLAYER2_LETTERS,
        BONUS_NUM_PLAYER_LETTERS, player2Letters, sizeof(player2Letters)) != 0)
    {
        fprintf(stderr, "Can't read player 2 letters\n");
        status = EXIT_FAILURE;
        goto exit;
    }

    if (ReadMemory(dosboxHandle, (char*)gameMemoryStart + BONUS_MEM_OFFSET_BOARD,
        BONUS_BOARD_SIZE, board, sizeof(board)) != 0)
    {
        fprintf(stderr, "Can't read board\n");
        status = EXIT_FAILURE;
        goto exit;
    }

    PrintHebrewMapping();

    PrintBuffer("Player 1 Letters", player1Letters, sizeof(player1Letters), BONUS_NUM_PLAYER_LETTERS);
    printf("\n");
    PrintBuffer("Player 2 Letters", player2Letters, sizeof(player2Letters), BONUS_NUM_PLAYER_LETTERS);
    printf("\n");
    PrintBuffer("Board", board, sizeof(board), BONUS_BOARD_DIMENTIONS + 1);

exit:
    if (dosboxHandle != NULL)
    {
        CloseHandle(dosboxHandle);
    }

    return status;
}


