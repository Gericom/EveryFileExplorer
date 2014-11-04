using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibEveryFileExplorer.Files
{
	[Serializable]
	public class SignatureNotCorrectException : Exception
	{
		public string BadSignature { get; private set; }
		public string CorrectSignature { get; private set; }
		public long Offset { get; private set; }
		public SignatureNotCorrectException(string BadSignature, string CorrectSignature, long Offset)
		{
			this.BadSignature = BadSignature;
			this.CorrectSignature = CorrectSignature;
			this.Offset = Offset;
		}
		public SignatureNotCorrectException(string message) : base(message) { }
		public SignatureNotCorrectException(string message, Exception inner) : base(message, inner) { }
		protected SignatureNotCorrectException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }

		public override string ToString()
		{
			return "Signature '" + BadSignature + "' at 0x" + Offset.ToString("X8") + " does not match '" + CorrectSignature + "'.";
		}
	}
}
