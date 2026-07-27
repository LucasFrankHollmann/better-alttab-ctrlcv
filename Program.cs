using System.Runtime.InteropServices;
using System.Text;

const string WindowClassName = "TrayAppWindowClass";
const int WM_DESTROY = 0x0002;
const int WM_USER = 0x0400;
const int WM_TRAYICON = WM_USER + 1;
const int NIF_MESSAGE = 0x00000001;
const int NIF_ICON = 0x00000002;
const int NIF_TIP = 0x00000004;
const int NIM_ADD = 0x00000000;
const int NIM_DELETE = 0x00000002;
const int NIM_MODIFY = 0x00000001;
const int WM_LBUTTONUP = 0x0202;
const int WM_RBUTTONUP = 0x0205;
const int WM_COMMAND = 0x0111;
const int MF_STRING = 0x00000000;
const int MF_SEPARATOR = 0x00000800;
const int IDM_EXIT = 1000;

var hInstance = GetModuleHandle(null);
var iconHandle = LoadIcon(IntPtr.Zero, IDI_APPLICATION);
var wndClass = new WNDCLASS
{
    lpfnWndProc = WndProc,
    lpszClassName = WindowClassName,
    hInstance = hInstance,
    hCursor = LoadCursor(IntPtr.Zero, IDC_ARROW),
};

RegisterClass(ref wndClass);

var hwnd = CreateWindowEx(
    0,
    WindowClassName,
    "AltTab CtrlCV Tray App",
    0,
    0,
    0,
    0,
    0,
    IntPtr.Zero,
    IntPtr.Zero,
    hInstance,
    IntPtr.Zero);

var nid = new NOTIFYICONDATA
{
    cbSize = Marshal.SizeOf<NOTIFYICONDATA>(),
    hWnd = hwnd,
    uID = 1,
    uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP,
    uCallbackMessage = WM_TRAYICON,
    hIcon = iconHandle,
    szTip = "AltTab CtrlCV - clique para abrir"
};

Shell_NotifyIcon(NIM_ADD, ref nid);

try
{
    var msg = new MSG();
    while (GetMessage(out msg, IntPtr.Zero, 0, 0) != 0)
    {
        TranslateMessage(ref msg);
        DispatchMessage(ref msg);
    }
}
finally
{
    Shell_NotifyIcon(NIM_DELETE, ref nid);
}

IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
{
    return msg switch
    {
        WM_TRAYICON when lParam == (IntPtr)WM_LBUTTONUP => MessageBox(hWnd, "AltTab CtrlCV está ativo na bandeja.", "Tray App", MB_OK),
        WM_TRAYICON when lParam == (IntPtr)WM_RBUTTONUP => ShowContextMenu(hWnd),
        WM_COMMAND when wParam.ToInt32() == IDM_EXIT => PostMessage(hWnd, WM_DESTROY, IntPtr.Zero, IntPtr.Zero),
        WM_DESTROY => PostQuitMessage(0),
        _ => DefWindowProc(hWnd, msg, wParam, lParam),
    };
}

void ShowContextMenu(IntPtr hWnd)
{
    var hMenu = CreatePopupMenu();
    AppendMenu(hMenu, MF_STRING, IDM_EXIT, "Sair");
    SetForegroundWindow(hWnd);
    TrackPopupMenu(hMenu, 0x0000 | 0x0020, 0, 0, 0, hWnd, IntPtr.Zero);
    DestroyMenu(hMenu);
}

[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
static extern IntPtr CreateWindowEx(uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle,
    int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
static extern ushort RegisterClass([In] ref WNDCLASS lpWndClass);

[DllImport("user32.dll", SetLastError = true)]
static extern IntPtr LoadIcon(IntPtr hInstance, string lpIconName);

[DllImport("user32.dll", SetLastError = true)]
static extern IntPtr LoadCursor(IntPtr hInstance, string lpCursorName);

[DllImport("shell32.dll", SetLastError = true)]
static extern bool Shell_NotifyIcon(int dwMessage, [In] ref NOTIFYICONDATA lpdata);

[DllImport("user32.dll", SetLastError = true)]
static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

[DllImport("user32.dll")] static extern bool TranslateMessage([In] ref MSG lpMsg);
[DllImport("user32.dll")] static extern IntPtr DispatchMessage([In] ref MSG lpMsg);
[DllImport("user32.dll")] static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);
[DllImport("user32.dll", SetLastError = true)]
static extern bool PostQuitMessage(int nExitCode);
[DllImport("user32.dll", SetLastError = true)]
static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
[DllImport("user32.dll", SetLastError = true)]
static extern IntPtr CreatePopupMenu();
[DllImport("user32.dll", SetLastError = true)]
static extern bool AppendMenu(IntPtr hMenu, uint uFlags, uint uIDNewItem, string lpNewItem);
[DllImport("user32.dll", SetLastError = true)]
static extern bool DestroyMenu(IntPtr hMenu);
[DllImport("user32.dll", SetLastError = true)]
static extern bool TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);
[DllImport("user32.dll", SetLastError = true)]
static extern bool SetForegroundWindow(IntPtr hWnd);
[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
static extern IntPtr GetModuleHandle(string lpModuleName);

[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
static extern int MessageBox(IntPtr hWnd, string lpText, string lpCaption, uint uType);

const uint MB_OK = 0x00000000;
static readonly IntPtr IDC_ARROW = (IntPtr)32512;
static readonly string IDI_APPLICATION = "IDI_APPLICATION";

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
struct WNDCLASS
{
    public uint style;
    public IntPtr lpfnWndProc;
    public int cbClsExtra;
    public int cbWndExtra;
    public IntPtr hInstance;
    public IntPtr hIcon;
    public IntPtr hCursor;
    public IntPtr hbrBackground;
    public string lpszMenuName;
    public string lpszClassName;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
struct NOTIFYICONDATA
{
    public int cbSize;
    public IntPtr hWnd;
    public uint uID;
    public uint uFlags;
    public uint uCallbackMessage;
    public IntPtr hIcon;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string szTip;
    public uint dwState;
    public uint dwStateMask;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string szInfo;
    public uint uTimeoutOrVersion;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string szInfoTitle;
    public uint dwInfoFlags;
    public Guid guidItem;
    public IntPtr hBalloonIcon;
}

[StructLayout(LayoutKind.Sequential)]
struct POINT { public int x; public int y; }

[StructLayout(LayoutKind.Sequential)]
struct MSG
{
    public IntPtr hWnd;
    public uint message;
    public IntPtr wParam;
    public IntPtr lParam;
    public uint time;
    public POINT pt;
    public uint lPrivate;
}
