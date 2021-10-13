using System;
using System.Runtime.InteropServices;
using System.Text;

namespace libxputty.DllImports {
  public static class Kernel32dll {

    private const string KERNEL32 = "kernel32.dll";

    #region --- Console management --------------------------------------------
    [DllImport(KERNEL32)]
    public static extern bool AllocConsole();
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool AttachConsole(uint dwProcessId);
    [DllImport(KERNEL32, SetLastError = true, ExactSpelling = true)]
    public static extern bool FreeConsole();
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern uint GetConsoleProcessList(out uint[] ProcessList, uint ProcessCount);
    #endregion --- Console management -----------------------------------------

    #region --- Console Window --------------------------------------------
    [DllImport(KERNEL32)]
    public static extern IntPtr GetConsoleWindow();
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool SetConsoleWindowInfo(IntPtr hConsoleOutput, bool bAbsolute, [In] ref SMALL_RECT lpConsoleWindow);
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern COORD GetLargestConsoleWindowSize(IntPtr hConsoleOutput);
    #endregion --- Console Window -----------------------------------------

    #region --- Console std handle --------------------------------------------
    [DllImport(KERNEL32)]
    public static extern IntPtr GetStdHandle(int handle);
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool SetStdHandle(uint nStdHandle, IntPtr hHandle);
    #endregion --- Console std handle -----------------------------------------

    #region --- Screen buffer --------------------------------------------
    [DllImport(KERNEL32)]
    public static extern IntPtr CreateConsoleScreenBuffer(UInt32 dwDesiredAccess, UInt32 dwShareMode, IntPtr secutiryAttributes, UInt32 flags, IntPtr screenBufferData);
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool ScrollConsoleScreenBuffer(IntPtr hConsoleOutput, [In] ref SMALL_RECT lpScrollRectangle, IntPtr lpClipRectangle, COORD dwDestinationOrigin, [In] ref CHAR_INFO lpFill);
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool SetConsoleActiveScreenBuffer(IntPtr hConsoleOutput);
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool SetConsoleScreenBufferInfoEx(IntPtr hConsoleOutput, CONSOLE_SCREEN_BUFFER_INFO_EX ConsoleScreenBufferInfoEx);
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool SetConsoleScreenBufferSize(IntPtr hConsoleOutput, COORD dwSize);
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool GetConsoleScreenBufferInfo(IntPtr hConsoleOutput, out CONSOLE_SCREEN_BUFFER_INFO lpConsoleScreenBufferInfo);
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool GetConsoleScreenBufferInfoEx(IntPtr hConsoleOutput, ref CONSOLE_SCREEN_BUFFER_INFO_EX ConsoleScreenBufferInfo);
    #endregion --- Screen buffer -----------------------------------------

    #region --- Console alias(es) --------------------------------------------
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool AddConsoleAlias(string Source, string Target, string ExeName);
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool GetConsoleAlias(string Source, out StringBuilder TargetBuffer, uint TargetBufferLength, string ExeName);
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern uint GetConsoleAliases(StringBuilder[] lpTargetBuffer, uint targetBufferLength, string lpExeName);
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern uint GetConsoleAliasesLength(string ExeName);
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern uint GetConsoleAliasExes(out StringBuilder ExeNameBuffer, uint ExeNameBufferLength);
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern uint GetConsoleAliasExesLength();
    #endregion --- Console alias(es) -----------------------------------------

    #region --- Console Ctrl events --------------------------------------------
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);

    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate HandlerRoutine, bool Add);
    // Delegate type to be used as the Handler Routine for SCCH
    public delegate bool ConsoleCtrlDelegate(CtrlTypes CtrlType);
    #endregion --- Console Ctrl events -----------------------------------------

    #region --- Console input --------------------------------------------
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool FlushConsoleInputBuffer(IntPtr hConsoleInput);
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool PeekConsoleInput(IntPtr hConsoleInput, [Out] INPUT_RECORD[] lpBuffer, uint nLength, out uint lpNumberOfEventsRead);
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool ReadConsole(IntPtr hConsoleInput, [Out] StringBuilder lpBuffer, uint nNumberOfCharsToRead, out uint lpNumberOfCharsRead, IntPtr lpReserved);
    [DllImport(KERNEL32, EntryPoint = "ReadConsoleInputW", CharSet = CharSet.Unicode)]
    public static extern bool ReadConsoleInput(IntPtr hConsoleInput, [Out] INPUT_RECORD[] lpBuffer, uint nLength, out uint lpNumberOfEventsRead);
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool ReadConsoleOutput(IntPtr hConsoleOutput, [Out] CHAR_INFO[] lpBuffer, COORD dwBufferSize, COORD dwBufferCoord, ref SMALL_RECT lpReadRegion);
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool ReadConsoleOutputAttribute(IntPtr hConsoleOutput, [Out] ushort[] lpAttribute, uint nLength, COORD dwReadCoord, out uint lpNumberOfAttrsRead);
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool ReadConsoleOutputCharacter(IntPtr hConsoleOutput, [Out] StringBuilder lpCharacter, uint nLength, COORD dwReadCoord, out uint lpNumberOfCharsRead);
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool GetNumberOfConsoleInputEvents(IntPtr hConsoleInput, out uint lpcNumberOfEvents);
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool GetNumberOfConsoleMouseButtons(ref uint lpNumberOfMouseButtons);
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool GetConsoleSelectionInfo(CONSOLE_SELECTION_INFO ConsoleSelectionInfo);

    #endregion --- Console input -----------------------------------------

    #region --- Console output --------------------------------------------
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool WriteConsole(IntPtr hConsoleOutput, string lpBuffer, uint nNumberOfCharsToWrite, out uint lpNumberOfCharsWritten, IntPtr lpReserved);
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool WriteConsoleInput(IntPtr hConsoleInput, INPUT_RECORD[] lpBuffer, uint nLength, out uint lpNumberOfEventsWritten);
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool WriteConsoleOutput(IntPtr hConsoleOutput, CHAR_INFO[] lpBuffer, COORD dwBufferSize, COORD dwBufferCoord, ref SMALL_RECT lpWriteRegion);
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool WriteConsoleOutputAttribute(IntPtr hConsoleOutput, ushort[] lpAttribute, uint nLength, COORD dwWriteCoord, out uint lpNumberOfAttrsWritten);
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool WriteConsoleOutputCharacter(IntPtr hConsoleOutput, string lpCharacter, uint nLength, COORD dwWriteCoord, out uint lpNumberOfCharsWritten);
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool FillConsoleOutputAttribute(IntPtr hConsoleOutput, ushort wAttribute, uint nLength, COORD dwWriteCoord, out uint lpNumberOfAttrsWritten);
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool FillConsoleOutputCharacter(IntPtr hConsoleOutput, char cCharacter, uint nLength, COORD dwWriteCoord, out uint lpNumberOfCharsWritten);
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool SetConsoleTextAttribute(IntPtr hConsoleOutput, ushort wAttributes);
    #endregion --- Console output -----------------------------------------

    #region --- Console cursor --------------------------------------------
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool GetConsoleCursorInfo(IntPtr hConsoleOutput, out CONSOLE_CURSOR_INFO lpConsoleCursorInfo);

    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool SetConsoleCursorInfo(IntPtr hConsoleOutput, [In] ref CONSOLE_CURSOR_INFO lpConsoleCursorInfo);

    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool SetConsoleCursorPosition(IntPtr hConsoleOutput, COORD dwCursorPosition);
    #endregion --- Console cursor -----------------------------------------

    #region --- Console fonts --------------------------------------------
    [DllImport(KERNEL32, SetLastError = true)]
    static extern bool GetCurrentConsoleFont(IntPtr hConsoleOutput, bool bMaximumWindow, out CONSOLE_FONT_INFO lpConsoleCurrentFont);

    [DllImport(KERNEL32, SetLastError = true)]
    static extern bool GetCurrentConsoleFontEx(IntPtr ConsoleOutput, bool MaximumWindow, out CONSOLE_FONT_INFO_EX ConsoleCurrentFont);

    [DllImport(KERNEL32, SetLastError = true)]
    static extern COORD GetConsoleFontSize(IntPtr hConsoleOutput, Int32 nFont);

    [DllImport(KERNEL32, SetLastError = true)]
    static extern bool SetCurrentConsoleFontEx(IntPtr ConsoleOutput, bool MaximumWindow, CONSOLE_FONT_INFO_EX ConsoleCurrentFontEx);
    #endregion --- Console fonts -----------------------------------------

    #region --- Console code pages --------------------------------------------
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern uint GetConsoleCP();

    [DllImport(KERNEL32, SetLastError = true)]
    public static extern uint GetConsoleOutputCP();

    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool SetConsoleCP(uint wCodePageID);

    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool SetConsoleOutputCP(uint wCodePageID);
    #endregion --- Console code pages -----------------------------------------

    #region --- Console title --------------------------------------------
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern uint GetConsoleOriginalTitle(out StringBuilder ConsoleTitle, uint Size);
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern uint GetConsoleTitle([Out] StringBuilder lpConsoleTitle, uint nSize);
    [DllImport(KERNEL32)]
    public static extern bool SetConsoleTitle(string lpConsoleTitle);
    #endregion --- Console title -----------------------------------------

    #region --- Console history --------------------------------------------
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool GetConsoleHistoryInfo(out CONSOLE_HISTORY_INFO ConsoleHistoryInfo);
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool SetConsoleHistoryInfo(CONSOLE_HISTORY_INFO ConsoleHistoryInfo);
    #endregion --- Console history -----------------------------------------

    #region --- Console display mode --------------------------------------------
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool GetConsoleDisplayMode(out uint ModeFlags);

    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool SetConsoleDisplayMode(IntPtr ConsoleOutput, uint Flags, out COORD NewScreenBufferDimensions);
    #endregion --- Console display mode -----------------------------------------

    #region --- Console mode --------------------------------------------
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);
    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
    #endregion --- Console mode -----------------------------------------

    #region --- Data structures --------------------------------------------
    [StructLayout(LayoutKind.Sequential)]
    public struct COORD {
      public short X;
      public short Y;
    }

    public struct SMALL_RECT {
      public short Left;
      public short Top;
      public short Right;
      public short Bottom;
    }

    public struct CONSOLE_SCREEN_BUFFER_INFO {
      public COORD dwSize;
      public COORD dwCursorPosition;
      public short wAttributes;
      public SMALL_RECT srWindow;
      public COORD dwMaximumWindowSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CONSOLE_SCREEN_BUFFER_INFO_EX {
      public uint cbSize;
      public COORD dwSize;
      public COORD dwCursorPosition;
      public short wAttributes;
      public SMALL_RECT srWindow;
      public COORD dwMaximumWindowSize;

      public ushort wPopupAttributes;
      public bool bFullscreenSupported;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
      public COLORREF[] ColorTable;

      public static CONSOLE_SCREEN_BUFFER_INFO_EX Create() {
        return new CONSOLE_SCREEN_BUFFER_INFO_EX { cbSize = 96 };
      }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct COLORREF {
      public uint ColorDWORD;

      public COLORREF(System.Drawing.Color color) {
        ColorDWORD = (uint)color.R + (((uint)color.G) << 8) + (((uint)color.B) << 16);
      }

      public System.Drawing.Color GetColor() {
        return System.Drawing.Color.FromArgb((int)(0x000000FFU & ColorDWORD),
           (int)(0x0000FF00U & ColorDWORD) >> 8, (int)(0x00FF0000U & ColorDWORD) >> 16);
      }

      public void SetColor(System.Drawing.Color color) {
        ColorDWORD = (uint)color.R + (((uint)color.G) << 8) + (((uint)color.B) << 16);
      }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CONSOLE_FONT_INFO {
      public int nFont;
      public COORD dwFontSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct CONSOLE_FONT_INFO_EX {
      public uint cbSize;
      public uint nFont;
      public COORD dwFontSize;
      public ushort FontFamily;
      public ushort FontWeight;
      fixed char FaceName[LF_FACESIZE];

      const int LF_FACESIZE = 32;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct INPUT_RECORD {
      [FieldOffset(0)]
      public ushort EventType;
      [FieldOffset(4)]
      public KEY_EVENT_RECORD KeyEvent;
      [FieldOffset(4)]
      public MOUSE_EVENT_RECORD MouseEvent;
      [FieldOffset(4)]
      public WINDOW_BUFFER_SIZE_RECORD WindowBufferSizeEvent;
      [FieldOffset(4)]
      public MENU_EVENT_RECORD MenuEvent;
      [FieldOffset(4)]
      public FOCUS_EVENT_RECORD FocusEvent;
    };

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    public struct KEY_EVENT_RECORD {
      [FieldOffset(0), MarshalAs(UnmanagedType.Bool)]
      public bool bKeyDown;
      [FieldOffset(4), MarshalAs(UnmanagedType.U2)]
      public ushort wRepeatCount;
      [FieldOffset(6), MarshalAs(UnmanagedType.U2)]
      //public VirtualKeys wVirtualKeyCode;
      public ushort wVirtualKeyCode;
      [FieldOffset(8), MarshalAs(UnmanagedType.U2)]
      public ushort wVirtualScanCode;
      [FieldOffset(10)]
      public char UnicodeChar;
      [FieldOffset(12), MarshalAs(UnmanagedType.U4)]
      //public ControlKeyState dwControlKeyState;
      public uint dwControlKeyState;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MOUSE_EVENT_RECORD {
      public COORD dwMousePosition;
      public uint dwButtonState;
      public uint dwControlKeyState;
      public uint dwEventFlags;
    }

    public struct WINDOW_BUFFER_SIZE_RECORD {
      public COORD dwSize;

      public WINDOW_BUFFER_SIZE_RECORD(short x, short y) {
        dwSize = new COORD();
        dwSize.X = x;
        dwSize.Y = y;
      }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MENU_EVENT_RECORD {
      public uint dwCommandId;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FOCUS_EVENT_RECORD {
      public uint bSetFocus;
    }

    //CHAR_INFO struct, which was a union in the old days
    // so we want to use LayoutKind.Explicit to mimic it as closely
    // as we can
    [StructLayout(LayoutKind.Explicit)]
    public struct CHAR_INFO {
      [FieldOffset(0)]
      char UnicodeChar;
      [FieldOffset(0)]
      char AsciiChar;
      [FieldOffset(2)] //2 bytes seems to work properly
      UInt16 Attributes;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CONSOLE_CURSOR_INFO {
      uint Size;
      bool Visible;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CONSOLE_HISTORY_INFO {
      ushort cbSize;
      ushort HistoryBufferSize;
      ushort NumberOfHistoryBuffers;
      uint dwFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CONSOLE_SELECTION_INFO {
      uint Flags;
      COORD SelectionAnchor;
      SMALL_RECT Selection;

      // Flags values:
      const uint CONSOLE_MOUSE_DOWN = 0x0008; // Mouse is down
      const uint CONSOLE_MOUSE_SELECTION = 0x0004; //Selecting with the mouse
      const uint CONSOLE_NO_SELECTION = 0x0000; //No selection
      const uint CONSOLE_SELECTION_IN_PROGRESS = 0x0001; //Selection has begun
      const uint CONSOLE_SELECTION_NOT_EMPTY = 0x0002; //Selection rectangle is not empty
    }

    // Enumerated type for the control messages sent to the handler routine
    public enum CtrlTypes : uint {
      CTRL_C_EVENT = 0,
      CTRL_BREAK_EVENT,
      CTRL_CLOSE_EVENT,
      CTRL_LOGOFF_EVENT = 5,
      CTRL_SHUTDOWN_EVENT
    }
    #endregion --- Data structures -----------------------------------------
  }
}
