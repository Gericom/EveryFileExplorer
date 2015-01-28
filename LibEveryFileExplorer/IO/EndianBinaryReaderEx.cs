using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using LibEveryFileExplorer.IO.Serialization;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.Collections;

namespace LibEveryFileExplorer.IO
{
	public class EndianBinaryReaderEx : EndianBinaryReader
	{
		public EndianBinaryReaderEx(Stream baseStream)
			: base(baseStream) { }

		public EndianBinaryReaderEx(Stream baseStream, Endianness endianness)
			: base(baseStream, endianness) { }

		public void ReadPadding(int Alignment)
		{
			while ((BaseStream.Position % Alignment) != 0) BaseStream.Position++;
		}

		public Single ReadFixedPoint(bool Sign, int IntPart, int FracPart)
		{
			if (IntPart < 0 || FracPart < 0) throw new ArgumentException("IntPart and FracPart shoulf be greater or equal to 0!");
			int TotalBits = IntPart + FracPart + (Sign ? 1 : 0);
			if (TotalBits > 64 || TotalBits == 0) throw new ArgumentException("Total number of bits should be greater than 0 and smaller or equal to 64!");
			ulong result;
			if (TotalBits <= 8) result = ReadByte();
			else if (TotalBits <= 16) result = ReadUInt16();
			else if (TotalBits <= 32) result = ReadUInt32();
			else result = ReadUInt64();
			result &= ~(~0ul << TotalBits);
			if (Sign) return (((long)result << (64 - TotalBits)) >> (64 - TotalBits)) / (float)(1 << FracPart);
			else return result / (float)(1 << FracPart);
		}

		public void ReadObject(Object obj)
		{
			Type t = obj.GetType();
			MemberInfo[] m = t.FindMembers(MemberTypes.Field | MemberTypes.Property, BindingFlags.Instance | BindingFlags.Public, null, null);
			foreach (MemberInfo f in m)
			{
				Type FieldType;
				if (f.MemberType == MemberTypes.Property) FieldType = ((PropertyInfo)f).PropertyType;
				else FieldType = ((FieldInfo)f).FieldType;
				if (GetAttributeValue<bool>(f, typeof(BinaryIgnoreAttribute), false)) continue;
				if (f.GetCustomAttributes(typeof(BinaryBOMAttribute), true).Length != 0) Endianness = IO.Endianness.BigEndian;
				object Result;
				if (FieldType.IsArray)
				{
					int ArraySize = GetAttributeValue<int>(f, typeof(BinaryFixedSizeAttribute), 0);
					if (ArraySize <= 0) throw new ArgumentException("Array size must be greater than 0!");
					if (FieldType.GetElementType().IsPrimitive)
					{
						switch (FieldType.GetElementType().Name)
						{
							case "Boolean":
								{
									BooleanSize b = GetAttributeValue<BooleanSize>(f, typeof(BinaryBooleanSizeAttribute), BooleanSize.U32);
									Result = new Boolean[ArraySize];
									switch (b)
									{
										case BooleanSize.U8:
											for (int i = 0; i < ArraySize; i++)
											{
												((Boolean[])Result)[i] = ReadByte() == 1;
											}
											break;
										case BooleanSize.U16:
											for (int i = 0; i < ArraySize; i++)
											{
												((Boolean[])Result)[i] = ReadUInt16() == 1;
											}
											break;
										case BooleanSize.U32:
											for (int i = 0; i < ArraySize; i++)
											{
												((Boolean[])Result)[i] = ReadUInt32() == 1;
											}
											break;
										default: throw new Exception("Invalid BooleanSize Value!");
									}
									break;
								}
							case "Byte": Result = ReadBytes(ArraySize); break;
							case "SByte": Result = ReadSBytes(ArraySize); break;
							case "Int16": Result = ReadInt16s(ArraySize); break;
							case "UInt16": Result = ReadUInt16s(ArraySize); break;
							case "Int32": Result = ReadInt32s(ArraySize); break;
							case "UInt32": Result = ReadUInt32s(ArraySize); break;
							case "Int64": Result = ReadInt64s(ArraySize); break;
							case "UInt64": Result = ReadUInt64s(ArraySize); break;
							//Unsupported
							case "IntPtr":
							case "UIntPtr":
								goto default;
							case "Char": Result = ReadChars(GetAttributeValue<Encoding>(f, typeof(BinaryStringEncodingAttribute), Encoding.ASCII), ArraySize); break;
							case "Double": Result = ReadDoubles(ArraySize); break;
							case "Single":
								{
									uint Format = GetAttributeValue<uint>(f, typeof(BinaryFixedPointAttribute), 0);
									if (Format == 0) Result = ReadSingles(ArraySize);
									else
									{
										bool Sign = (Format >> 14) == 1;
										int IntPart = (int)((Format >> 7) & 0x7F);
										int FracPart = (int)(Format & 0x7F);
										Result = new Single[ArraySize];
										for (int i = 0; i < ArraySize; i++)
										{
											((Single[])Result)[i] = ReadFixedPoint(Sign, IntPart, FracPart);
										}
									}
									break;
								}
							default:
								throw new Exception(t.Name + " is not supported!");
						}
					}
					else
					{
						Type elementt = FieldType.GetElementType();
						Result = Array.CreateInstance(elementt, ArraySize);
						for (int i = 0; i < ArraySize; i++)
						{
							((Array)Result).SetValue(elementt.InvokeMember("", BindingFlags.CreateInstance, null, null, new object[] { this }), i);
						}
					}
				}
				else
				{
					if (FieldType.IsEnum) FieldType = FieldType.GetEnumUnderlyingType();
					if (FieldType.IsPrimitive)
					{
						switch (FieldType.Name)
						{
							case "Boolean":
								{
									BooleanSize b = GetAttributeValue<BooleanSize>(f, typeof(BinaryBooleanSizeAttribute), BooleanSize.U32);
									switch (b)
									{
										case BooleanSize.U8: Result = ReadByte() == 1; break;
										case BooleanSize.U16: Result = ReadUInt16() == 1; break;
										case BooleanSize.U32: Result = ReadUInt32() == 1; break;
										default: throw new Exception("Invalid BooleanSize Value!");
									}
									break;
								}
							case "Byte": Result = ReadByte(); break;
							case "SByte": Result = ReadSByte(); break;
							case "Int16": Result = ReadInt16(); break;
							case "UInt16": Result = ReadUInt16(); break;
							case "Int32": Result = ReadInt32(); break;
							case "UInt32": Result = ReadUInt32(); break;
							case "Int64": Result = ReadInt64(); break;
							case "UInt64": Result = ReadUInt64(); break;
							//Unsupported
							case "IntPtr":
							case "UIntPtr":
								goto default;
							case "Char": Result = ReadChar(GetAttributeValue<Encoding>(f, typeof(BinaryStringEncodingAttribute), Encoding.ASCII)); break;
							case "Double": Result = ReadDouble(); break;
							case "Single":
								{
									uint Format = GetAttributeValue<uint>(f, typeof(BinaryFixedPointAttribute), 0);
									if (Format == 0) Result = ReadSingle();
									else
									{
										bool Sign = (Format >> 14) == 1;
										int IntPart = (int)((Format >> 7) & 0x7F);
										int FracPart = (int)(Format & 0x7F);
										Result = ReadFixedPoint(Sign, IntPart, FracPart);
									}
									break;
								}
							default:
								throw new Exception(t.Name + " is not supported!");
						}
					}
					else if (FieldType.Name == "String")
					{
						if (GetAttributeValue<bool>(f, typeof(BinaryStringNTAttribute), false)) Result = ReadStringNT(GetAttributeValue<Encoding>(f, typeof(BinaryStringEncodingAttribute), Encoding.ASCII));
						else
						{
							int length = GetAttributeValue<int>(f, typeof(BinaryFixedSizeAttribute), 0);
							if (length <= 0) throw new ArgumentException("String size must be greater than 0!");
							Result = ReadString(GetAttributeValue<Encoding>(f, typeof(BinaryStringEncodingAttribute), Encoding.ASCII), length);
						}
					}
					else if (FieldType.Name == "Vector2")
					{
						uint Format = GetAttributeValue<uint>(f, typeof(BinaryFixedPointAttribute), 0);
						if (Format == 0) Result = ReadVector2();
						else
						{
							bool Sign = (Format >> 14) == 1;
							int IntPart = (int)((Format >> 7) & 0x7F);
							int FracPart = (int)(Format & 0x7F);
							Result = new Vector2(ReadFixedPoint(Sign, IntPart, FracPart), ReadFixedPoint(Sign, IntPart, FracPart));
						}
					}
					else if (FieldType.Name == "Vector3")
					{
						uint Format = GetAttributeValue<uint>(f, typeof(BinaryFixedPointAttribute), 0);
						if (Format == 0) Result = ReadVector3();
						else
						{
							bool Sign = (Format >> 14) == 1;
							int IntPart = (int)((Format >> 7) & 0x7F);
							int FracPart = (int)(Format & 0x7F);
							Result = new Vector3(ReadFixedPoint(Sign, IntPart, FracPart), ReadFixedPoint(Sign, IntPart, FracPart), ReadFixedPoint(Sign, IntPart, FracPart));
						}
					}
					else if (FieldType.Name == "Vector4")
					{
						uint Format = GetAttributeValue<uint>(f, typeof(BinaryFixedPointAttribute), 0);
						if (Format == 0) Result = ReadVector4();
						else
						{
							bool Sign = (Format >> 14) == 1;
							int IntPart = (int)((Format >> 7) & 0x7F);
							int FracPart = (int)(Format & 0x7F);
							Result = new Vector4(ReadFixedPoint(Sign, IntPart, FracPart), ReadFixedPoint(Sign, IntPart, FracPart), ReadFixedPoint(Sign, IntPart, FracPart), ReadFixedPoint(Sign, IntPart, FracPart));
						}
					}
					else
					{
						//new T(this);
						Result = FieldType.InvokeMember("", BindingFlags.CreateInstance, null, null, new object[] { this });
					}
				}
				if (f.MemberType == MemberTypes.Property) ((PropertyInfo)f).SetValue(obj, Result, null);
				else ((FieldInfo)f).SetValue(obj, Result);
				byte[] bsig = GetAttributeValue<byte[]>(f, typeof(BinaryByteArraySignatureAttribute), null);
				if (bsig != null && Result is byte[])
				{
					if (!bsig.SequenceEqual((byte[])Result)) throw new SignatureNotCorrectException("{ " + BitConverter.ToString((byte[])Result, 0, ((byte[])Result).Length).Replace("-", ", ") + " }", "{ " + BitConverter.ToString(bsig, 0, bsig.Length).Replace("-", ", ") + " }", BaseStream.Position - ((byte[])Result).Length);
				}
				else
				{
					string ssig = GetAttributeValue<string>(f, typeof(BinaryStringSignatureAttribute), null);
					if (ssig != null && Result is string)
					{
						if (!ssig.Equals(Result)) throw new SignatureNotCorrectException((string)Result, ssig, BaseStream.Position - ssig.Length);
					}
				}
				if (f.GetCustomAttributes(typeof(BinaryBOMAttribute), true).Length != 0)
				{
					uint LittleEndian = GetAttributeValue<uint>(f, typeof(BinaryBOMAttribute), 0);
					if (Convert.ToUInt32(Result).Equals(LittleEndian)) Endianness = IO.Endianness.LittleEndian;
					else Endianness = IO.Endianness.BigEndian;
				}
			}
		}

		public T GetAttributeValue<T>(MemberInfo Field, Type Attribute, T DefaultValue)
		{
			var att = Field.GetCustomAttributes(Attribute, true);
			if (att.Length == 0) return DefaultValue;
			return (T)((BinaryAttribute)att[0]).Value;
		}
	}
}
