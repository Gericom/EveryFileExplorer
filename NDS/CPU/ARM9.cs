using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.IO;

namespace NDS.CPU
{
	public class ARM9
	{
		public static byte[] Decompress(byte[] Compressed, uint RamAddress, uint EntryAddress, uint AutoloadDone)
		{
			ARM9Context c = new ARM9Context();
			c.Registers[15] = EntryAddress;
			Array.Copy(Compressed, 0, c.Memory, RamAddress, Compressed.Length);
			uint stopaddress;
			while (true)
			{
				ExecuteInstruction(c);
				if (c.Registers[15] == AutoloadDone)
				{
					//Now we know the address of do_autoload + 4, which is MIi_UncompressBackward + 8
					stopaddress = c.Registers[14] - 4;
					break;
				}
			}
			c = new ARM9Context();
			c.Registers[15] = EntryAddress;
			Array.Copy(Compressed, 0, c.Memory, RamAddress, Compressed.Length);
			uint length = 0;
			uint compoffsaddr = 0;
			while (true)
			{
				ExecuteInstruction(c);
				if (c.Registers[15] == stopaddress - 4)
				{
					//look if the file is compressed (R0 = 0 means the data is not compressed!)
					if (c.Registers[0] == 0) return Compressed;
					length = c.Registers[0] + IOUtil.ReadU32LE(c.Memory, (int)c.Registers[0] - 4) - RamAddress;
					compoffsaddr = c.Registers[1] + 0x14 - RamAddress;
				}
				//we're done decompressing!
				if (c.Registers[15] == stopaddress) break;
			}
			byte[] result = new byte[length];
			Array.Copy(c.Memory, RamAddress, result, 0, length);
			IOUtil.WriteU32LE(result, (int)compoffsaddr, 0);
			return result;
		}

		private class ARM9Context
		{
			public uint[] Registers = new uint[16];
			public byte[] Memory = new byte[0x8000000];
			public bool C;
			public bool N;
			public bool V;
			public bool Z;
		}

		private static void ExecuteInstruction(ARM9Context c)
		{
			uint inst = IOUtil.ReadU32LE(c.Memory, (int)c.Registers[15]);
			uint condition = inst >> 28;
			if (EvaluateCondition(c, condition))
			{
				switch ((inst >> 25) & 7)
				{
					case 0:
					case 1:
						DataProc(c, inst);
						c.Registers[15] += 4;
						break;
					case 0x2:
					case 0x3:
						SingleDataTrans(c, inst);
						c.Registers[15] += 4;
						break;
					case 0x4:
						BlockDataTrans(c, inst);
						c.Registers[15] += 4;
						break;
					case 0x5://101
						Branch(c, inst);
						break;
					case 0x7:
						c.Registers[15] += 4;
						break;//ignore mcr, mrc and that kind of crap
					default:
						c.Registers[15] += 4;
						break;
				}
			}
			else c.Registers[15] += 4;
		}

		private static UInt32 Shift(UInt32 ShiftType, UInt32 Value, UInt32 NrBits, ref bool Carry)
		{
			switch (ShiftType)
			{
				case 0:
					if (NrBits > 0) Carry = ((Value >> (32 - (int)NrBits)) & 1) == 1;
					return Value << (int)NrBits;
				case 1:
					if (NrBits > 0) Carry = ((Value >> ((int)NrBits - 1)) & 1) == 1;
					else Carry = ((Value >> 31) & 1) == 1;
					return Value >> (int)NrBits;
				case 2:
					if (NrBits > 0)
					{
						Carry = ((Value >> ((int)NrBits - 1)) & 1) == 1;
						return (uint)(((int)Value) >> (int)NrBits);
					}
					else
					{
						Carry = ((Value >> 31) & 1) == 1;
						return ((Value >> 31) & 1) * 0xFFFFFFFF;
					}
				case 3:
					if (NrBits > 0)
					{
						Carry = ((Value >> ((int)NrBits - 1)) & 1) == 1;
						return (Value >> (int)NrBits) | (Value << (32 - (int)NrBits));
					}
					else
					{
						uint tmp = ((Carry ? 1u : 0u) << 31) | (Value >> 1);
						Carry = (Value & 1) == 1;
						return tmp;
					}
			}
			return 0xFFFFFFFF;
		}

		private static void DataProc(ARM9Context c, uint Instruction)
		{
			uint result = 0;
			uint Rn;
			if ((Instruction & 0x0FFFFF00) == 0x12FFF00)//bx, blx
			{
				uint op = (Instruction >> 4) & 0xF;
				Rn = Instruction & 0xF;
				if (op == 1)
				{
					c.Registers[15] = c.Registers[Rn] - 4;
				}
				else if (op == 3)
				{
					c.Registers[14] = c.Registers[15] + 4;
					c.Registers[15] = c.Registers[Rn] - 4;
				}
				else
				{
					//shouldn't happen!
				}
				return;
			}
			bool Shift_C = c.C;

			bool I = ((Instruction >> 25) & 1) == 1;
			uint Opcode = (Instruction >> 21) & 0xF;
			bool S = ((Instruction >> 20) & 1) == 1;
			Rn = (Instruction >> 16) & 0xF;
			uint Rd = (Instruction >> 12) & 0xF;
			uint Op2 = 0;
			if (I)
			{
				uint Is = (Instruction >> 8) & 0xF;
				uint nn = Instruction & 0xFF;

				Op2 = (nn >> (int)(Is * 2)) | (nn << (32 - (int)(Is * 2)));
			}
			else
			{
				uint ShiftType = (Instruction >> 5) & 0x3;
				bool R = ((Instruction >> 4) & 1) == 1;
				uint Rm = Instruction & 0xF;

				if (!R)
				{
					uint Is = (Instruction >> 7) & 0x1F;
					Op2 = Shift(ShiftType, c.Registers[Rm], Is, ref Shift_C);
				}
				else
				{
					uint Rs = (Instruction >> 8) & 0xF;
					uint Reserved = (Instruction >> 7) & 1;
					Op2 = Shift(ShiftType, c.Registers[Rm], c.Registers[Rs] & 0xFF, ref Shift_C);
				}
			}

			switch (Opcode)
			{
				case 0x2://sub
					result = c.Registers[Rd] = c.Registers[Rn] - Op2;
					break;
				case 0x4://add
					result = c.Registers[Rd] = c.Registers[Rn] + Op2;
					break;
				case 0x8:
					if (S == false)//MRS
					{
						break;
					}
					else//TST
					{
						result = c.Registers[Rn] & Op2;
					}
					break;
				case 0x9:
					if (S == false)//MSR
					{
						break;
					}
					else//TEQ
					{
						result = c.Registers[Rn] ^ Op2;
					}
					break;
				case 0xA://compare
					result = c.Registers[Rn] - Op2;
					break;
				case 0xC://OR logical 
					result = c.Registers[Rd] = c.Registers[Rn] | Op2;
					break;
				case 0xD://move
					result = c.Registers[Rd] = Op2;
					break;
				case 0xE://bit clear 
					result = c.Registers[Rd] = c.Registers[Rn] & ~Op2;
					break;
				default:
					break;
			}

			if (S)
			{
				bool V = c.V;//tmp
				bool C = c.C;//tmp
				bool Z = result == 0;
				bool N = (result >> 31) == 1;
				if (Rd != 15)
				{
					switch (Opcode)
					{
						case 0:
						case 1:
						case 8:
						case 9:
						case 12:
						case 13:
						case 14:
						case 15:
							C = Shift_C;
							break;
						//case 2:
						case 3:
						case 4:
						case 5:
						case 6:
						case 7:
						//case 10:
						case 11:
							C = !(Op2 > c.Registers[Rn]);
							break;
						case 0x2:
						case 0xA:
							C = !(Op2 > c.Registers[Rn]);
							V = ((c.Registers[Rn] < 0 && Op2 >= 0) || (c.Registers[Rn] >= 0 && Op2 < 0)) && ((c.Registers[Rn] < 0 && result >= 0) || (c.Registers[Rn] >= 0 && result < 0));
							break;
					}
					c.C = C;
					c.N = N;
					c.V = V;
					c.Z = Z;
				}
			}

		}

		private static void SingleDataTrans(ARM9Context c, UInt32 Instruction)
		{
			bool I = ((Instruction >> 25) & 1) == 1;
			bool P = ((Instruction >> 24) & 1) == 1;
			bool U = ((Instruction >> 23) & 1) == 1;
			bool B = ((Instruction >> 22) & 1) == 1;

			bool T = false;
			bool W = false;

			if (P) W = ((Instruction >> 21) & 1) == 1;
			else T = ((Instruction >> 21) & 1) == 1;

			bool L = ((Instruction >> 20) & 1) == 1;

			uint Rn = (Instruction >> 16) & 0xF;
			uint Rd = (Instruction >> 12) & 0xF;

			uint Offset;
			if (I)
			{
				uint Is = (Instruction >> 7) & 0x1F;
				uint ShiftType = (Instruction >> 5) & 0x3;
				uint Reserved = (Instruction >> 4) & 1;
				uint Rm = Instruction & 0xF;
				bool Shift_C = c.C;
				Offset = Shift(ShiftType, c.Registers[Rm], Is, ref Shift_C);
			}
			else
			{
				Offset = Instruction & 0xFFF;
			}

			uint MemoryOffset = c.Registers[Rn];
			if (Rn == 15) MemoryOffset += 8;
			if (P)
			{
				if (U) MemoryOffset += Offset;
				else MemoryOffset -= Offset;
				if (W) c.Registers[Rn] = MemoryOffset;
			}
			if (L)
			{
				if (B)
				{
					try
					{
						c.Registers[Rd] = c.Memory[MemoryOffset];
					}
					catch { }
				}
				else c.Registers[Rd] = IOUtil.ReadU32LE(c.Memory, (int)MemoryOffset);
			}
			else
			{
				if (Rd == 15)
				{
					if (B) c.Memory[MemoryOffset] = (byte)((c.Registers[Rd] + 12) & 0xFF);
					else Array.Copy(BitConverter.GetBytes(c.Registers[Rd] + 12), 0, c.Memory, MemoryOffset, 4);
				}
				else
				{
					if (B) c.Memory[MemoryOffset] = (byte)(c.Registers[Rd] & 0xFF);
					else Array.Copy(BitConverter.GetBytes(c.Registers[Rd]), 0, c.Memory, MemoryOffset, 4);
				}
			}
			if (!P)
			{
				if (U) MemoryOffset += Offset;
				else MemoryOffset -= Offset;
				c.Registers[Rn] = MemoryOffset;
			}
		}

		private static void BlockDataTrans(ARM9Context c, UInt32 Instruction)
		{
			bool P = ((Instruction >> 24) & 1) == 1;
			bool U = ((Instruction >> 23) & 1) == 1;
			bool S = ((Instruction >> 22) & 1) == 1;
			bool W = ((Instruction >> 21) & 1) == 1;
			bool L = ((Instruction >> 20) & 1) == 1;
			uint Rn = (Instruction >> 16) & 0xF;
			uint RList = Instruction & 0xFFFF;

			if (!U && ((RList >> 15) & 1) == 1)
			{
				//TODO!
			}

			uint offs = c.Registers[Rn];
			for (int i = 0; i < 15; i++)
			{
				int reg;
				if (U) reg = i;
				else reg = 14 - i;
				if (((RList >> reg) & 1) == 1)
				{
					if (P)
					{
						if (U) offs += 4;
						else offs -= 4;
					}
					if (L) c.Registers[reg] = IOUtil.ReadU32LE(c.Memory, (int)offs);
					else IOUtil.WriteU32LE(c.Memory, (int)offs, c.Registers[reg]);
					if (!P)
					{
						if (U) offs += 4;
						else offs -= 4;
					}
				}
			}
			if (U && ((RList >> 15) & 1) == 1)
			{
				//TODO!
			}
			if (W) c.Registers[Rn] = offs;
		}

		private static void Branch(ARM9Context c, UInt32 Instruction)
		{
			uint Opcode = (Instruction >> 24) & 1;
			int nn = (int)((Instruction & 0xFFFFFF) << 8) >> 8;
			if (Opcode == 1) 
				c.Registers[14] = c.Registers[15] + 4;
			c.Registers[15] = (uint)((int)c.Registers[15] + 8 + nn * 4);
		}

		private static bool EvaluateCondition(ARM9Context c, uint condition)
		{
			switch (condition)
			{
				case 0x0: return c.Z == true;
				case 0x1: return c.Z == false;
				case 0x2: return c.C == true;
				case 0x3: return c.C == false;
				case 0x4: return c.N == true;
				case 0x5: return c.N == false;
				case 0x6: return c.V == true;
				case 0x7: return c.V == false;
				case 0x8: return c.C == true && c.Z == false;
				case 0x9: return c.C == false || c.Z == true;
				case 0xA: return c.N == c.V;
				case 0xB: return c.N != c.V;
				case 0xC: return c.Z == false && c.N == c.V;
				case 0xD: return c.Z == true || c.N != c.V;
				case 0xE: return true;
				case 0xF: return false;
			}
			//shouldn't happen!
			return false;
		}
	}
}
