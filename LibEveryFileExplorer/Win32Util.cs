using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace LibEveryFileExplorer
{
	public class Win32Util
	{
		public const int HWND_BROADCAST = 0xffff;
		public const int WM_COPYDATA = 0x004A;

		private const int GWL_STYLE = -16;
		private const int GWL_EXSTYLE = -20;

		private const int WS_BORDER = 0x00800000;
		private const int WS_EX_CLIENTEDGE = 0x00000200;

		public const int SW_RESTORE = 0x09;

		private const uint SWP_NOSIZE = 0x0001;
		private const uint SWP_NOMOVE = 0x0002;
		private const uint SWP_NOZORDER = 0x0004;
		private const uint SWP_NOREDRAW = 0x0008;
		private const uint SWP_NOACTIVATE = 0x0010;
		private const uint SWP_FRAMECHANGED = 0x0020;
		private const uint SWP_SHOWWINDOW = 0x0040;
		private const uint SWP_HIDEWINDOW = 0x0080;
		private const uint SWP_NOCOPYBITS = 0x0100;
		private const uint SWP_NOOWNERZORDER = 0x0200;
		private const uint SWP_NOSENDCHANGING = 0x0400;

		[DllImport("kernel32.dll")]
		public static extern bool AllocConsole();

		[DllImport("kernel32.dll")]
		public static extern bool AttachConsole(int pid);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool FreeConsole();

		[DllImport("Shlwapi.dll", CharSet = CharSet.Auto)]
		public static extern long StrFormatByteSize(long fileSize, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder buffer, int bufferSize);

		[DllImport("user32", CharSet = CharSet.Auto)]
		private static extern int GetWindowLong(IntPtr hWnd, int Index);

		[DllImport("user32")]
		private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, ref COPYDATASTRUCT lParam);

		[DllImport("user32", CharSet = CharSet.Auto)]
		private static extern int SetWindowLong(IntPtr hWnd, int Index, int Value);

		[DllImport("user32", ExactSpelling = true)]
		private static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

		[DllImport("user32.dll")]
		public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

		[DllImport("uxtheme.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
		public static extern int SetWindowTheme(IntPtr hWnd, string appName, string partList);

		[StructLayout(LayoutKind.Sequential)]
		private struct COPYDATASTRUCT
		{
			public IntPtr dwData;
			public int cbData;
			[MarshalAs(UnmanagedType.LPWStr)]
			public String lpData;
		}

		public static bool SendString(IntPtr Handle, String Data)
		{
			COPYDATASTRUCT c = new COPYDATASTRUCT() { lpData = Data, cbData = Data.Length * 2 + 2 };
			return SendMessage(Handle, WM_COPYDATA, IntPtr.Zero, ref c) == IntPtr.Zero;
		}

		public static String GetStringFromMessage(Message m)
		{
			COPYDATASTRUCT c = (COPYDATASTRUCT)m.GetLParam(typeof(COPYDATASTRUCT));
			return c.lpData;
		}

		public static void SetMDIBorderStyle(MdiClient mdiClient, BorderStyle value)
		{
			// Get styles using Win32 calls
			int style = GetWindowLong(mdiClient.Handle, GWL_STYLE);
			int exStyle = GetWindowLong(mdiClient.Handle, GWL_EXSTYLE);

			// Add or remove style flags as necessary.
			switch (value)
			{
				case BorderStyle.Fixed3D:
					exStyle |= WS_EX_CLIENTEDGE;
					style &= ~WS_BORDER;
					break;

				case BorderStyle.FixedSingle:
					exStyle &= ~WS_EX_CLIENTEDGE;
					style |= WS_BORDER;
					break;

				case BorderStyle.None:
					style &= ~WS_BORDER;
					exStyle &= ~WS_EX_CLIENTEDGE;
					break;
			}

			// Set the styles using Win32 calls
			SetWindowLong(mdiClient.Handle, GWL_STYLE, style);
			SetWindowLong(mdiClient.Handle, GWL_EXSTYLE, exStyle);

			// Update the non-client area.
			SetWindowPos(mdiClient.Handle, IntPtr.Zero, 0, 0, 0, 0,
				SWP_NOACTIVATE | SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER |
				SWP_NOOWNERZORDER | SWP_FRAMECHANGED);
		}
	}
}
