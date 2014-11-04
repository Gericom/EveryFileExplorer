using System.Text;
using System.Runtime.InteropServices;
using LibEveryFileExplorer.Collections;

namespace System.IO
{
	public sealed class EndianBinaryReader : IDisposable
	{
		private bool disposed;
		private byte[] buffer;

		public Stream BaseStream { get; private set; }
		public Endianness Endianness { get; set; }
		public Endianness SystemEndianness { get { return BitConverter.IsLittleEndian ? Endianness.LittleEndian : Endianness.BigEndian; } }

		private bool Reverse { get { return SystemEndianness != Endianness; } }

		public EndianBinaryReader(Stream baseStream)
			: this(baseStream, Endianness.BigEndian)
		{ }

		public EndianBinaryReader(Stream baseStream, Endianness endianness)
		{
			if (baseStream == null) throw new ArgumentNullException("baseStream");
			if (!baseStream.CanRead) throw new ArgumentException("baseStream");

			BaseStream = baseStream;
			Endianness = endianness;
		}

		~EndianBinaryReader()
		{
			Dispose(false);
		}

		private void FillBuffer(int bytes, int stride)
		{
			if (buffer == null || buffer.Length < bytes)
				buffer = new byte[bytes];

			BaseStream.Read(buffer, 0, bytes);

			if (Reverse)
				for (int i = 0; i < bytes; i += stride)
				{
					Array.Reverse(buffer, i, stride);
				}
		}

		public byte ReadByte()
		{
			FillBuffer(1, 1);

			return buffer[0];
		}

		public byte[] ReadBytes(int count)
		{
			byte[] temp;

			FillBuffer(count, 1);
			temp = new byte[count];
			Array.Copy(buffer, 0, temp, 0, count);
			return temp;
		}

		public sbyte ReadSByte()
		{
			FillBuffer(1, 1);

			unchecked
			{
				return (sbyte)buffer[0];
			}
		}

		public sbyte[] ReadSBytes(int count)
		{
			sbyte[] temp;

			temp = new sbyte[count];
			FillBuffer(count, 1);

			unchecked
			{
				for (int i = 0; i < count; i++)
				{
					temp[i] = (sbyte)buffer[i];
				}
			}

			return temp;
		}

		public char ReadChar(Encoding encoding)
		{
			int size;

			size = GetEncodingSize(encoding);
			FillBuffer(size, size);
			return encoding.GetChars(buffer, 0, size)[0];
		}

		public char[] ReadChars(Encoding encoding, int count)
		{
			int size;

			size = GetEncodingSize(encoding);
			FillBuffer(size * count, size);
			return encoding.GetChars(buffer, 0, size * count);
		}

		private static int GetEncodingSize(Encoding encoding)
		{
			if (encoding == Encoding.UTF8 || encoding == Encoding.ASCII)
				return 1;
			else if (encoding == Encoding.Unicode || encoding == Encoding.BigEndianUnicode)
				return 2;

			return 1;
		}

		public string ReadStringNT(Encoding encoding)
		{
			string text;

			text = "";

			do
			{
				text += ReadChar(encoding);
			} while (!text.EndsWith("\0", StringComparison.Ordinal));

			return text.Remove(text.Length - 1);
		}

		public string ReadString(Encoding encoding, int count)
		{
			return new string(ReadChars(encoding, count));
		}

		public double ReadDouble()
		{
			const int size = sizeof(double);
			FillBuffer(size, size);
			return BitConverter.ToDouble(buffer, 0);
		}

		public double[] ReadDoubles(int count)
		{
			const int size = sizeof(double);
			double[] temp;

			temp = new double[count];
			FillBuffer(size * count, size);

			for (int i = 0; i < count; i++)
			{
				temp[i] = BitConverter.ToDouble(buffer, size * i);
			}
			return temp;
		}

		public Single ReadSingle()
		{
			const int size = sizeof(Single);
			FillBuffer(size, size);
			return BitConverter.ToSingle(buffer, 0);
		}

		public Single[] ReadSingles(int count)
		{
			const int size = sizeof(Single);
			Single[] temp;

			temp = new Single[count];
			FillBuffer(size * count, size);

			for (int i = 0; i < count; i++)
			{
				temp[i] = BitConverter.ToSingle(buffer, size * i);
			}
			return temp;
		}

		public Vector2 ReadVector2()
		{
			return new Vector2(ReadSingle(), ReadSingle());
		}

		public Vector3 ReadVector3()
		{
			return new Vector3(ReadSingle(), ReadSingle(), ReadSingle());
		}

		public Vector4 ReadVector4()
		{
			return new Vector4(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
		}

		public Single ReadFx16()
		{
			return ReadInt16() / 4096f;
		}

		public Vector3 ReadVecFx16()
		{
			return new Vector3(ReadFx16(), ReadFx16(), ReadFx16());
		}

		public Single ReadFx32()
		{
			return ReadInt32() / 4096f;
		}

		public Single[] ReadFx32s(int count)
		{
			Single[] result = new float[count];
			for (int i = 0; i < count; i++)
			{
				result[i] = ReadInt32() / 4096f;
			}
			return result;
		}

		public Vector3 ReadVecFx32()
		{
			return new Vector3(ReadFx32(), ReadFx32(), ReadFx32());
		}

		public Int32 ReadInt32()
		{
			const int size = sizeof(Int32);
			FillBuffer(size, size);
			return BitConverter.ToInt32(buffer, 0);
		}

		public Int32[] ReadInt32s(int count)
		{
			const int size = sizeof(Int32);
			Int32[] temp;

			temp = new Int32[count];
			FillBuffer(size * count, size);

			for (int i = 0; i < count; i++)
			{
				temp[i] = BitConverter.ToInt32(buffer, size * i);
			}
			return temp;
		}

		public Int64 ReadInt64()
		{
			const int size = sizeof(Int64);
			FillBuffer(size, size);
			return BitConverter.ToInt64(buffer, 0);
		}

		public Int64[] ReadInt64s(int count)
		{
			const int size = sizeof(Int64);
			Int64[] temp;

			temp = new Int64[count];
			FillBuffer(size * count, size);

			for (int i = 0; i < count; i++)
			{
				temp[i] = BitConverter.ToInt64(buffer, size * i);
			}
			return temp;
		}

		public Int16 ReadInt16()
		{
			const int size = sizeof(Int16);
			FillBuffer(size, size);
			return BitConverter.ToInt16(buffer, 0);
		}

		public Int16[] ReadInt16s(int count)
		{
			const int size = sizeof(Int16);
			Int16[] temp;

			temp = new Int16[count];
			FillBuffer(size * count, size);

			for (int i = 0; i < count; i++)
			{
				temp[i] = BitConverter.ToInt16(buffer, size * i);
			}
			return temp;
		}

		public UInt16 ReadUInt16()
		{
			const int size = sizeof(UInt16);
			FillBuffer(size, size);
			return BitConverter.ToUInt16(buffer, 0);
		}

		public UInt16[] ReadUInt16s(int count)
		{
			const int size = sizeof(UInt16);
			UInt16[] temp;

			temp = new UInt16[count];
			FillBuffer(size * count, size);

			for (int i = 0; i < count; i++)
			{
				temp[i] = BitConverter.ToUInt16(buffer, size * i);
			}
			return temp;
		}

		public UInt32 ReadUInt32()
		{
			const int size = sizeof(UInt32);
			FillBuffer(size, size);
			return BitConverter.ToUInt32(buffer, 0);
		}

		public UInt32[] ReadUInt32s(int count)
		{
			const int size = sizeof(UInt32);
			UInt32[] temp;

			temp = new UInt32[count];
			FillBuffer(size * count, size);

			for (int i = 0; i < count; i++)
			{
				temp[i] = BitConverter.ToUInt32(buffer, size * i);
			}
			return temp;
		}

		public UInt64 ReadUInt64()
		{
			const int size = sizeof(UInt64);
			FillBuffer(size, size);
			return BitConverter.ToUInt64(buffer, 0);
		}

		public UInt64[] ReadUInt64s(int count)
		{
			const int size = sizeof(UInt64);
			UInt64[] temp;

			temp = new UInt64[count];
			FillBuffer(size * count, size);

			for (int i = 0; i < count; i++)
			{
				temp[i] = BitConverter.ToUInt64(buffer, size * i);
			}
			return temp;
		}

		public void Close()
		{
			Dispose();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					if (BaseStream != null)
					{
						BaseStream.Close();
					}
				}

				buffer = null;

				disposed = true;
			}
		}
	}

	public enum Endianness
	{
		BigEndian,
		LittleEndian,
	}
}
