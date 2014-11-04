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

				/*switch (PrimitiveType)
				{
					case AnimationType.Vector2Animation:
						var v = new Single[2][];
						if ((Flags & 1) != 0) v[0] = new float[] { er.ReadSingle() };//constant
						else if ((Flags & 4) != 0) er.ReadUInt32();//nothing = empty reference

						Data = v;
						break;
				}*/

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
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
