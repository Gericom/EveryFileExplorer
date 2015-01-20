using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibEveryFileExplorer.IO
{
	[Serializable]
	public class SignatureNotCorrectException : Exception
	{
		public string BadSignature { get; private set; }
		public string CorrectSignature { get; private set; }
		public long Offset { get; private set; }
		public SignatureNotCorrectException(string BadSignature, string CorrectSignature, long Offset)
			: base("Signature '" + BadSignature + "' at 0x" + Offset.ToString("X8") + " does not match '" + CorrectSignature + "'.")
		{
			this.BadSignature = BadSignature;
			this.CorrectSignature = CorrectSignature;
			this.Offset = Offset;
		}
	}
}
