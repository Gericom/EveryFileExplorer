using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Collections;

namespace NDS.NitroSystem.G3D
{
	public class Util
	{
		public static Matrix44 G3DPivotToMatrix(float[] ab, int pv, int neg)
		{
			float[] data = new float[16];
			data[15] = 1.0F;
			float one = 1.0F;
			float a = ab[0];
			float b = ab[1];
			float a2 = a;
			float b2 = b;
			switch (neg)
			{
				case 1: // '\001'
				case 3: // '\003'
				case 5: // '\005'
				case 7: // '\007'
				case 9: // '\t'
				case 11: // '\013'
				case 13: // '\r'
				case 15: // '\017'
					one = -1F;
					// fall through
					goto case 2;
				case 2: // '\002'
				case 4: // '\004'
				case 6: // '\006'
				case 8: // '\b'
				case 10: // '\n'
				case 12: // '\f'
				case 14: // '\016'
				default:
					switch (neg)
					{
						case 2: // '\002'
						case 3: // '\003'
						case 6: // '\006'
						case 7: // '\007'
						case 10: // '\n'
						case 11: // '\013'
						case 14: // '\016'
						case 15: // '\017'
							b2 = -b2;
							// fall through
							goto case 4;
						case 4: // '\004'
						case 5: // '\005'
						case 8: // '\b'
						case 9: // '\t'
						case 12: // '\f'
						case 13: // '\r'
						default:
							switch (neg)
							{
								case 4: // '\004'
								case 5: // '\005'
								case 6: // '\006'
								case 7: // '\007'
								case 12: // '\f'
								case 13: // '\r'
								case 14: // '\016'
								case 15: // '\017'
									a2 = -a2;
									// fall through
									goto case 8;
								case 8: // '\b'
								case 9: // '\t'
								case 10: // '\n'
								case 11: // '\013'
								default:
									switch (pv)
									{
										case 0: // '\0'
											data[0] = one;
											data[5] = a;
											data[6] = b;
											data[9] = b2;
											data[10] = a2;
											break;

										case 1: // '\001'
											data[1] = one;
											data[4] = a;
											data[6] = b;
											data[8] = b2;
											data[10] = a2;
											break;

										case 2: // '\002'
											data[2] = one;
											data[4] = a;
											data[5] = b;
											data[8] = b2;
											data[9] = a2;
											break;

										case 3: // '\003'
											data[4] = one;
											data[1] = a;
											data[2] = b;
											data[9] = b2;
											data[10] = a2;
											break;

										case 4: // '\004'
											data[5] = one;
											data[0] = a;
											data[2] = b;
											data[8] = b2;
											data[10] = a2;
											break;

										case 5: // '\005'
											data[6] = one;
											data[0] = a;
											data[1] = b;
											data[8] = b2;
											data[9] = a2;
											break;

										case 6: // '\006'
											data[8] = one;
											data[1] = a;
											data[2] = b;
											data[5] = b2;
											data[6] = a2;
											break;

										case 7: // '\007'
											data[9] = one;
											data[0] = a;
											data[2] = b;
											data[4] = b2;
											data[6] = a2;
											break;

										case 8: // '\b'
											data[10] = one;
											data[0] = a;
											data[1] = b;
											data[4] = b2;
											data[5] = a2;
											break;

										case 9: // '\t'
											data[0] = -a;
											break;
									}
									break;
							}
							break;
					}
					break;
			}
			return new Matrix44(data);
		}
	}
}
