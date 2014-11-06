using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace EveryFileExplorer
{
	class Win32MessageHelper
	{
		public const int HWND_BROADCAST = 0xffff;
		public const int WM_COPYDATA = 0x004A;

		[DllImport("user32")]
		static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, ref COPYDATASTRUCT lParam);

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
	}
}
