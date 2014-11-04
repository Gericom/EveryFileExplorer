using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LibEveryFileExplorer.Files;
using System.Drawing;
using LibEveryFileExplorer.Collections;
using Tao.OpenGl;

namespace _3DS.NintendoWare.LYT
{
	public class mat1
	{
		public mat1(EndianBinaryReader er)
		{
			long startpos = er.BaseStream.Position;
			Signature = er.ReadString(Encoding.ASCII, 4);
			if (Signature != "mat1") throw new SignatureNotCorrectException(Signature, "mat1", er.BaseStream.Position - 4);
			SectionSize = er.ReadUInt32();
			NrMaterials = er.ReadUInt32();
			MaterialEntryOffsets = er.ReadUInt32s((int)NrMaterials);
			Materials = new MaterialEntry[NrMaterials];
			for (int i = 0; i < NrMaterials; i++)
			{
				er.BaseStream.Position = startpos + MaterialEntryOffsets[i];
				Materials[i] = new MaterialEntry(er);
			}
			er.BaseStream.Position = startpos + SectionSize;
		}
		public String Signature;
		public UInt32 SectionSize;
		public UInt32 NrMaterials;
		public UInt32[] MaterialEntryOffsets;

		public MaterialEntry[] Materials;
		public class MaterialEntry
		{
			public MaterialEntry(EndianBinaryReader er)
			{
				Name = er.ReadString(Encoding.ASCII, 20).Replace("\0", "");
				BufferColor = er.ReadColor8();
				ConstColors = new Color[6];
				ConstColors[0] = er.ReadColor8();
				ConstColors[1] = er.ReadColor8();
				ConstColors[2] = er.ReadColor8();
				ConstColors[3] = er.ReadColor8();
				ConstColors[4] = er.ReadColor8();
				ConstColors[5] = er.ReadColor8();
				Flags = er.ReadUInt32();
				//Material Flag:
				//  0-1: Nr texMap
				//  2-3: Nr texMatrix
				//  4-5: Nr texCoordGen
				//  6-8: Nr tevStage
				//    9: Has alphaCompare
				//   10: Has blendMode
				//   11: Use Texture Only
				//   12: Separate Blend Mode
				//   14: Has Indirect Parameter
				//15-16: Nr projectionTexGenParameter
				//   17: Has Font Shadow Parameter
				TexMaps = new TexMap[Flags & 3];
				for (int i = 0; i < (Flags & 3); i++)
				{
					TexMaps[i] = new TexMap(er);
				}
				TexMatrices = new TexMatrix[(Flags >> 2) & 3];
				for (int i = 0; i < ((Flags >> 2) & 3); i++)
				{
					TexMatrices[i] = new TexMatrix(er);
				}
				TexCoordGens = new TexCoordGen[(Flags >> 4) & 3];
				for (int i = 0; i < ((Flags >> 4) & 3); i++)
				{
					TexCoordGens[i] = new TexCoordGen(er);
				}
				TevStages = new TevStage[(Flags >> 6) & 7];
				for (int i = 0; i < ((Flags >> 6) & 7); i++)
				{
					TevStages[i] = new TevStage(er);
				}
				if (((Flags >> 9) & 1) == 1) AlphaTest = new AlphaCompare(er);
				if (((Flags >> 10) & 1) == 1) ColorBlendMode = new BlendMode(er);
				if (((Flags >> 12) & 1) == 1) AlphaBlendMode = new BlendMode(er);
				//Some more things
			}

			public String Name;
			public Color BufferColor;//?
			public Color[] ConstColors;
			public UInt32 Flags;

			public TexMap[] TexMaps;
			public class TexMap
			{
				public enum WrapMode
				{
					Clamp = 0,
					Repeat = 1,
					Mirror = 2
				}
				public enum FilterMode
				{
					Near = 0,
					Linear = 1
				}
				public TexMap(EndianBinaryReader er)
				{
					TexIndex = er.ReadUInt16();
					byte tmp = er.ReadByte();
					WrapS = (WrapMode)(tmp & 3);
					MinFilter = (FilterMode)((tmp >> 2) & 3);
					tmp = er.ReadByte();
					WrapT = (WrapMode)(tmp & 3);
					MagFilter = (FilterMode)((tmp >> 2) & 3);
				}
				public UInt16 TexIndex;
				public WrapMode WrapS;
				public FilterMode MinFilter;
				public WrapMode WrapT;
				public FilterMode MagFilter;
			}

			public TexMatrix[] TexMatrices;
			public class TexMatrix
			{
				public TexMatrix(EndianBinaryReader er)
				{
					Translation = er.ReadVector2();
					Rotation = er.ReadSingle();
					Scale = er.ReadVector2();
				}
				public Vector2 Translation;
				public Single Rotation;
				public Vector2 Scale;
			}

			public TexCoordGen[] TexCoordGens;
			public class TexCoordGen
			{
				public enum TexCoordSource
				{
					Tex0 = 0,
					Tex1 = 1,
					Tex2 = 2,
					OrthogonalProjection = 3,
					PaneBasedProjection = 4,
					PerspectiveProjection = 5
				}
				public TexCoordGen(EndianBinaryReader er)
				{
					Unknown1 = er.ReadByte();
					Source = (TexCoordSource)er.ReadByte();
					Unknown2 = er.ReadUInt16();
				}
				public byte Unknown1;
				public TexCoordSource Source;
				public UInt16 Unknown2;
			}

			public TevStage[] TevStages;
			public class TevStage
			{
				public enum TevSource
				{
					Tex0 = 0,
					Tex1 = 1,
					Tex2 = 2,
					Tex3 = 3,
					Constant = 4,
					Primary = 5,
					Previous = 6,
					Register = 7
				}
				public enum TevColorOp
				{
					RGB = 0,
					InvRGB = 1,
					Alpha = 2,
					InvAlpha = 3,
					RRR = 4,
					InvRRR = 5,
					GGG = 6,
					InvGGG = 7,
					BBB = 8,
					InvBBB = 9
				}
				public enum TevAlphaOp
				{
					Alpha = 0,
					InvAlpha = 1,
					R = 2,
					InvR = 3,
					G = 4,
					InvG = 5,
					B = 6,
					InvB = 7
				}
				public enum TevMode
				{
					Replace = 0,
					Modulate = 1,
					Add = 2,
					AddSigned = 3,
					Interpolate = 4,
					Subtract = 5,
					AddMult = 6,
					MultAdd = 7,
					Overlay = 8,
					Indirect = 9,
					BlendIndirect = 10,
					EachIndirect = 11
				}
				public enum TevScale
				{
					Scale1,
					Scale2,
					Scale4
				}
				public TevStage(EndianBinaryReader er)
				{
					uint tmp = er.ReadUInt32();
					ColorSources = new TevSource[] { (TevSource)(tmp & 0xF), (TevSource)((tmp >> 4) & 0xF), (TevSource)((tmp >> 8) & 0xF) };
					ColorOperators = new TevColorOp[] { (TevColorOp)((tmp >> 12) & 0xF), (TevColorOp)((tmp >> 16) & 0xF), (TevColorOp)((tmp >> 20) & 0xF) };
					ColorMode = (TevMode)((tmp >> 24) & 0xF);
					ColorScale = (TevScale)((tmp >> 28) & 0x3);
					ColorSavePrevReg = ((tmp >> 30) & 0x1) == 1;
					tmp = er.ReadUInt32();
					AlphaSources = new TevSource[] { (TevSource)(tmp & 0xF), (TevSource)((tmp >> 4) & 0xF), (TevSource)((tmp >> 8) & 0xF) };
					AlphaOperators = new TevAlphaOp[] { (TevAlphaOp)((tmp >> 12) & 0xF), (TevAlphaOp)((tmp >> 16) & 0xF), (TevAlphaOp)((tmp >> 20) & 0xF) };
					AlphaMode = (TevMode)((tmp >> 24) & 0xF);
					AlphaScale = (TevScale)((tmp >> 28) & 0x3);
					AlphaSavePrevReg = ((tmp >> 30) & 0x1) == 1;

					ConstColors = er.ReadUInt32();
				}
				public TevSource[] ColorSources;
				public TevColorOp[] ColorOperators;
				public TevMode ColorMode;
				public TevScale ColorScale;
				public Boolean ColorSavePrevReg;

				public byte ColorUnknown;
				public TevSource[] AlphaSources;
				public TevAlphaOp[] AlphaOperators;
				public TevMode AlphaMode;
				public TevScale AlphaScale;
				public Boolean AlphaSavePrevReg;

				public UInt32 ConstColors;
			}
			public AlphaCompare AlphaTest;
			public class AlphaCompare
			{
				public enum AlphaFunction
				{
					Never = 0,
					Less = 1,
					LEqual = 2,
					Equal = 3,
					NEqual = 4,
					GEqual = 5,
					Greater = 6,
					Always = 7
				}
				public AlphaCompare(EndianBinaryReader er)
				{
					AlphaFunc = (AlphaFunction)er.ReadUInt32();
					Reference = er.ReadSingle();
				}
				public AlphaFunction AlphaFunc;
				public Single Reference;

				private readonly int[] GlAlphaFunc =
				{
					Gl.GL_NEVER,
					Gl.GL_LESS,
					Gl.GL_LEQUAL,
					Gl.GL_EQUAL,
					Gl.GL_NOTEQUAL,
					Gl.GL_GEQUAL,
					Gl.GL_GREATER,
					Gl.GL_ALWAYS
				};

				public void Apply()
				{
					Gl.glEnable(Gl.GL_ALPHA_TEST);
					Gl.glAlphaFunc(GlAlphaFunc[(int)AlphaFunc], Reference);
					Gl.glEnable(Gl.GL_ALPHA_TEST);
				}
			}
			public BlendMode ColorBlendMode;
			public BlendMode AlphaBlendMode;
			public class BlendMode
			{
				public enum BlendOp
				{
					None = 0,
					Add = 1,
					Subtract = 2,
					ReverseSubtract = 3,
					SelectMin = 4,
					SelectMax = 5
				}
				public enum BlendSource
				{
					V0 = 0,
					V1_0 = 1,
					DstClr = 2,
					InvDstClr = 3,
					SrcAlpha = 4,
					InvSrcAlpha = 5,
					DstAlpha = 6,
					InvDstAlpha = 7
				}
				public enum BlendDestination
				{
					V0 = 0,
					V1_0 = 1,
					SrcClr = 2,
					InvSrcClr = 3,
					SrcAlpha = 4,
					InvSrcAlpha = 5,
					DstAlpha = 6,
					InvDstAlpha = 7
				}
				public enum BlendLogicOp
				{
					None = 0,
					NoOp = 1,
					Clear = 2,
					Set = 3,
					Copy = 4,
					InvCopy = 5,
					Inv = 6,
					And = 7,
					Nand = 8,
					Or = 9,
					Nor = 10,
					Xor = 11,
					Equiv = 12,
					RevAnd = 13,
					InvAnd = 14,
					RevOr = 15,
					InvOr = 16
				}
				public BlendMode(EndianBinaryReader er)
				{
					BlendOperator = (BlendOp)er.ReadByte();
					SourceFactor = (BlendSource)er.ReadByte();
					DestinationFactor = (BlendDestination)er.ReadByte();
					LogicOperator = (BlendLogicOp)er.ReadByte();
				}
				public BlendOp BlendOperator;
				public BlendSource SourceFactor;
				public BlendDestination DestinationFactor;
				public BlendLogicOp LogicOperator;

				private readonly int[] GlBlendEq =
				{
					0,
					Gl.GL_FUNC_ADD,
					Gl.GL_FUNC_SUBTRACT,
					Gl.GL_FUNC_REVERSE_SUBTRACT,
					Gl.GL_MIN,
					Gl.GL_MAX
				};

				private readonly int[] GlBlendSrc =
				{
					Gl.GL_ZERO,
					Gl.GL_ONE,
					Gl.GL_DST_COLOR,
					Gl.GL_ONE_MINUS_DST_COLOR,
					Gl.GL_SRC_ALPHA,
					Gl.GL_ONE_MINUS_SRC_ALPHA,
					Gl.GL_DST_ALPHA,
					Gl.GL_ONE_MINUS_DST_ALPHA
				};

				private readonly int[] GlBlendDst =
				{
					Gl.GL_ZERO,
					Gl.GL_ONE,
					Gl.GL_SRC_COLOR,
					Gl.GL_ONE_MINUS_SRC_COLOR,
					Gl.GL_SRC_ALPHA,
					Gl.GL_ONE_MINUS_SRC_ALPHA,
					Gl.GL_DST_ALPHA,
					Gl.GL_ONE_MINUS_DST_ALPHA
				};

				public void Apply()
				{
					if (BlendOperator == BlendOp.None) Gl.glDisable(Gl.GL_BLEND);
					else
					{
						Gl.glEnable(Gl.GL_BLEND);
						Gl.glBlendEquation(GlBlendEq[(int)BlendOperator]);
						Gl.glBlendFunc(GlBlendSrc[(int)SourceFactor], GlBlendDst[(int)DestinationFactor]);
						Gl.glEnable(Gl.GL_BLEND);
					}
				}
			}

			public CLYTShader GlShader { get; private set; }

			public void SetupShader()
			{
				GlShader = new CLYTShader(this, new int[TexMaps.Length]);
				GlShader.Compile();
			}

			public void ApplyAlphaCompareBlendMode()
			{
				if (AlphaTest != null) AlphaTest.Apply();
				else
				{
					Gl.glEnable(Gl.GL_ALPHA_TEST);
					Gl.glAlphaFunc(Gl.GL_ALWAYS, 0);
					Gl.glEnable(Gl.GL_ALPHA_TEST);
				}
				if (ColorBlendMode != null) ColorBlendMode.Apply();
				else
				{
					Gl.glEnable(Gl.GL_BLEND);
					Gl.glBlendEquation(Gl.GL_FUNC_ADD);
					Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
					Gl.glEnable(Gl.GL_BLEND);
				}
			}

			public override string ToString()
			{
				return Name;
			}
		}
	}
}
