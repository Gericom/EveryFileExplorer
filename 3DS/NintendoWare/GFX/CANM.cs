using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LibEveryFileExplorer.Files;

namespace _3DS.NintendoWare.GFX
{
	public class CANM
	{
		public CANM(EndianBinaryReader er)
		{
			Signature = er.ReadString(Encoding.ASCII, 4);
			if (Signature != "CANM") throw new SignatureNotCorrectException(Signature, "CANM", er.BaseStream.Position);
			Revision = er.ReadUInt32();
			NameOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
			TargetAnimationGroupNameOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
			LoopMode = er.ReadUInt32();
			FrameSize = er.ReadSingle();
			NrMemberAnimations = er.ReadUInt32();
			MemberAnimationDictOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
			NrUserDataEntries = er.ReadUInt32();
			UserDataOffset = er.ReadUInt32();

			long curpos = er.BaseStream.Position;
			er.BaseStream.Position = NameOffset;
			Name = er.ReadStringNT(Encoding.ASCII);
			er.BaseStream.Position = TargetAnimationGroupNameOffset;
			TargetAnimationGroupName = er.ReadStringNT(Encoding.ASCII);
			er.BaseStream.Position = MemberAnimationDictOffset;
			MemberAnimationDictionary = new DICT(er);

			MemberAnimations = new MemberAnimationData[NrMemberAnimations];
			for (int i = 0; i < NrMemberAnimations; i++)
			{
				er.BaseStream.Position = MemberAnimationDictionary[i].DataOffset;
				MemberAnimations[i] = new MemberAnimationData(er);
			}

			er.BaseStream.Position = curpos;
		}

		public String Signature;
		public UInt32 Revision;
		public UInt32 NameOffset;
		public UInt32 TargetAnimationGroupNameOffset;
		public UInt32 LoopMode;
		public Single FrameSize;
		public UInt32 NrMemberAnimations;
		public UInt32 MemberAnimationDictOffset;
		public UInt32 NrUserDataEntries;
		public UInt32 UserDataOffset;

		public String Name;
		public String TargetAnimationGroupName;

		public DICT MemberAnimationDictionary;

		public MemberAnimationData[] MemberAnimations;
		public class MemberAnimationData
		{
			public enum AnimationType
			{
				FloatAnimation,
				IntAnimation,
				BoolAnimation,
				Vector2Animation,
				Vector3Animation,
				TransformAnimation,
				RgbaColorAnimation,
				TextureAnimation,
				BakedTransformAnimation,
				FullBakedAnimation
			}
			public MemberAnimationData(EndianBinaryReader er)
			{
				Flags = er.ReadUInt32();
				PathOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
				PrimitiveType = (AnimationType)er.ReadUInt32();

				switch (PrimitiveType)
				{
					case AnimationType.Vector2Animation:
						var v = new Single[2][];
						if ((Flags & 1) != 0) v[0] = new float[] { er.ReadSingle() };//constant
						else if ((Flags & 4) != 0) er.ReadUInt32();//nothing = empty reference
						else
						{
							long offs = er.BaseStream.Position + er.ReadUInt32();
							long curpos_ = er.BaseStream.Position;
							er.BaseStream.Position = offs;
							FloatAnimationCurve a = new FloatAnimationCurve(er);
							er.BaseStream.Position = curpos_;
						}

						if ((Flags & 2) != 0) v[1] = new float[] { er.ReadSingle() };//constant
						else if ((Flags & 8) != 0) er.ReadUInt32();//nothing = empty reference
						else
						{
							long offs = er.BaseStream.Position + er.ReadUInt32();
							long curpos_ = er.BaseStream.Position;
							er.BaseStream.Position = offs;
							FloatAnimationCurve a = new FloatAnimationCurve(er);
							er.BaseStream.Position = curpos_;
						}

						//Data = v;
						break;
				}

				long curpos = er.BaseStream.Position;
				er.BaseStream.Position = PathOffset;
				Path = er.ReadStringNT(Encoding.ASCII);
				er.BaseStream.Position = curpos;
			}
			public UInt32 Flags;
			public UInt32 PathOffset;
			public AnimationType PrimitiveType;

			//public Array[] Data;

			public String Path;

			public class AnimationCurve
			{
				public AnimationCurve(EndianBinaryReader er)
				{
					StartFrame = er.ReadSingle();
					EndFrame = er.ReadSingle();
					PreRepeatMethod = er.ReadByte();
					PostRepeatMethod = er.ReadByte();
					Padding = er.ReadUInt16();
					Flags = er.ReadUInt32();
				}
				public Single StartFrame;
				public Single EndFrame;
				public Byte PreRepeatMethod;
				public Byte PostRepeatMethod;
				public UInt16 Padding;
				public UInt32 Flags;
			}

			public class FloatAnimationCurve : AnimationCurve
			{
				public FloatAnimationCurve(EndianBinaryReader er)
					: base(er)
				{
					NrSegments = er.ReadUInt32();
					SegmentOffsets = new uint[NrSegments];
					for (int i = 0; i < NrSegments; i++)
						SegmentOffsets[i] = (uint)er.BaseStream.Position + er.ReadUInt32();
					long curpos = er.BaseStream.Position;
					Segments = new FloatSegment[NrSegments];
					for (int i = 0; i < NrSegments; i++)
					{
						er.BaseStream.Position = SegmentOffsets[i];
						Segments[i] = new FloatSegment(er);
					}
					er.BaseStream.Position = curpos;
				}

				public UInt32 NrSegments;
				public UInt32[] SegmentOffsets;

				public FloatSegment[] Segments;
				public class FloatSegment
				{
					public enum QuantizationType : uint
					{
						Hermite128,
						Hermite64,//
						Hermite48,//
						UnifiedHermite96,//
						UnifiedHermite48,//
						UnifiedHermite32,//
						StepLinear64,
						StepLinear32//
					}

					public FloatSegment(EndianBinaryReader er)
					{
						StartFrame = er.ReadSingle();
						EndFrame = er.ReadSingle();
						Flags = er.ReadUInt32();
						if ((Flags & 1) == 1) SingleKeyValue = er.ReadSingle();
						else
						{
							NrKeys = er.ReadUInt32();
							Speed = er.ReadSingle();

							uint interp = (Flags >> 2) & 7;
							QuantizationType quanti = (QuantizationType)((Flags >> 5) & 7);
							if (quanti != QuantizationType.Hermite128 && quanti != QuantizationType.StepLinear64)
							{
								if (quanti != QuantizationType.UnifiedHermite96)
								{
									Scale = er.ReadSingle();
									Offset = er.ReadSingle();
									FrameScale = er.ReadSingle();
								}
							}
							Keys = new Key[NrKeys];
							for (int i = 0; i < NrKeys; i++)
							{
								Key k = new Key();
								switch (quanti)
								{
									case QuantizationType.Hermite128:
										{
											k.Frame = er.ReadSingle();
											k.Value = er.ReadSingle();
											k.InSlope = er.ReadSingle();
											k.OutSlope = er.ReadSingle();
											break;
										}
									case QuantizationType.Hermite64:
										{
											UInt32 FrameValue = er.ReadUInt32();
											k.InSlope = (float)er.ReadInt16() * 0.00390625f;
											k.OutSlope = (float)er.ReadInt16() * 0.00390625f;
											break;
										}
									case QuantizationType.Hermite48:
										{
											byte[] FrameValue = er.ReadBytes(3);
											byte[] InOutSlope = er.ReadBytes(3);
											break;
										}
									case QuantizationType.UnifiedHermite96:
										{
											k.Frame = er.ReadSingle();
											k.Value = er.ReadSingle();
											k.InSlope = k.OutSlope = er.ReadSingle();
											break;
										}
									case QuantizationType.UnifiedHermite48:
										{
											k.Frame = (float)er.ReadUInt16() * 0.03125f;
											k.Value = er.ReadInt16();
											k.InSlope = k.OutSlope = (float)er.ReadInt16() * 0.00390625f;
											break;
										}
									case QuantizationType.UnifiedHermite32:
										{
											k.Frame = er.ReadByte();
											byte[] ValueSlope = er.ReadBytes(3);
											break;
										}
									case QuantizationType.StepLinear64:
										{
											k.Frame = er.ReadSingle();
											k.Value = er.ReadSingle();
											break;
										}
									case QuantizationType.StepLinear32:
										{
											UInt32 FrameValue = er.ReadUInt32();
											k.Frame = FrameValue & 0xFFF;
											k.Value = ((int)FrameValue) >> 12;
											break;
										}
								}
								Keys[i] = k;
							}
						}
					}
					public Single StartFrame;
					public Single EndFrame;
					public UInt32 Flags;

					//(Flags & 1) == 1
					public Single SingleKeyValue;
					//(Flags & 1) == 0
					public UInt32 NrKeys;
					public Single Speed;//1 / duration

					public Single Scale;
					public Single Offset;
					public Single FrameScale;

					public Key[] Keys;
					public class Key
					{
						public Single Frame;
						public Single Value;
						public Single InSlope;
						public Single OutSlope;
					}
				}
			}
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
