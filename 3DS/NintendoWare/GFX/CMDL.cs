using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LibEveryFileExplorer.Files;
using System.Drawing;
using Tao.OpenGl;
using LibEveryFileExplorer.IO;
using LibEveryFileExplorer._3D;
using LibEveryFileExplorer.Collections;

namespace _3DS.NintendoWare.GFX
{
	public class CMDL
	{
		public CMDL(EndianBinaryReader er)
		{
			Type = er.ReadUInt32();
			Signature = er.ReadString(Encoding.ASCII, 4);
			if (Signature != "CMDL") throw new SignatureNotCorrectException(Signature, "CMDL", er.BaseStream.Position);
			Revision = er.ReadUInt32();
			NameOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
			Unknown2 = er.ReadUInt32();
			Unknown3 = er.ReadUInt32();
			Flags = er.ReadUInt32();
			IsBranchVisible = er.ReadUInt32() == 1;
			NrChildren = er.ReadUInt32();
			Unknown7 = er.ReadUInt32();
			NrAnimationGroupDescriptions = er.ReadUInt32();
			AnimationGroupDescriptionsDictOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
			Scale = er.ReadVector3();
			Rotation = er.ReadVector3();
			Translation = er.ReadVector3();
			LocalMatrix = er.ReadSingles(4 * 3);
			WorldMatrix = er.ReadSingles(4 * 3);
			NrMeshes = er.ReadUInt32();
			MeshOffsetsOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
			NrMaterials = er.ReadUInt32();
			MaterialsDictOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
			NrShapes = er.ReadUInt32();
			ShapeOffsetsOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
			NrMeshNodes = er.ReadUInt32();
			MeshNodeVisibilitiesDictOffset = er.ReadUInt32();
			if (MeshNodeVisibilitiesDictOffset != 0) MeshNodeVisibilitiesDictOffset += (UInt32)er.BaseStream.Position - 4;
			Unknown23 = er.ReadUInt32();
			Unknown24 = er.ReadUInt32();
			Unknown25 = er.ReadUInt32();
			if ((Type & 0x80) != 0) SkeletonInfoSOBJOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();

			long curpos = er.BaseStream.Position;
			er.BaseStream.Position = NameOffset;
			Name = er.ReadStringNT(Encoding.ASCII);
			er.BaseStream.Position = AnimationGroupDescriptionsDictOffset;
			AnimationInfoDict = new DICT(er);
			er.BaseStream.Position = MeshOffsetsOffset;
			MeshOffsets = new UInt32[NrMeshes];
			for (int i = 0; i < NrMeshes; i++)
			{
				MeshOffsets[i] = (UInt32)er.BaseStream.Position + er.ReadUInt32();
			}
			er.BaseStream.Position = MaterialsDictOffset;
			MaterialsDict = new DICT(er);

			Materials = new MTOB[NrMaterials];
			for (int i = 0; i < NrMaterials; i++)
			{
				er.BaseStream.Position = MaterialsDict[i].DataOffset;
				Materials[i] = new MTOB(er);
			}

			er.BaseStream.Position = ShapeOffsetsOffset;
			ShapeOffsets = new UInt32[NrShapes];
			for (int i = 0; i < NrShapes; i++)
			{
				ShapeOffsets[i] = (UInt32)er.BaseStream.Position + er.ReadUInt32();
			}

			if (MeshNodeVisibilitiesDictOffset != 0)
			{
				er.BaseStream.Position = MeshNodeVisibilitiesDictOffset;
				MeshNodeVisibilitiesDict = new DICT(er);
			}

			AnimationGroupDescriptions = new GraphicsAnimationGroup[NrAnimationGroupDescriptions];
			for (int i = 0; i < NrAnimationGroupDescriptions; i++)
			{
				er.BaseStream.Position = AnimationInfoDict[i].DataOffset;
				AnimationGroupDescriptions[i] = new GraphicsAnimationGroup(er);
			}

			Meshes = new Mesh[NrMeshes];
			for (int i = 0; i < NrMeshes; i++)
			{
				er.BaseStream.Position = MeshOffsets[i];
				Meshes[i] = new Mesh(er);
			}

			Shapes = new SeparateDataShape[NrShapes];
			for (int i = 0; i < NrShapes; i++)
			{
				er.BaseStream.Position = ShapeOffsets[i];
				Shapes[i] = new SeparateDataShape(er);
			}

			MeshNodeVisibilities = new MeshNodeVisibilityCtr[NrMeshNodes];
			for (int i = 0; i < NrMeshNodes; i++)
			{
				er.BaseStream.Position = MeshNodeVisibilitiesDict[i].DataOffset;
				MeshNodeVisibilities[i] = new MeshNodeVisibilityCtr(er);
			}

			if ((Type & 0x80) != 0)
			{
				er.BaseStream.Position = SkeletonInfoSOBJOffset;
				Skeleton = new SkeletonCtr(er);
			}

			er.BaseStream.Position = curpos;
		}
		public void Write(EndianBinaryWriter er, CGFXWriterContext c)
		{
			long basepos = er.BaseStream.Position;
			er.Write(Type);
			er.Write(Signature, Encoding.ASCII, false);
			er.Write(Revision);
			c.WriteStringReference(Name, er);
			er.Write(Unknown2);
			er.Write(Unknown3);
			er.Write(Flags);
			er.Write((uint)(IsBranchVisible ? 1 : 0));
			er.Write(NrChildren);
			er.Write(Unknown7);
			er.Write(NrAnimationGroupDescriptions);
			long anmgrpdescdictoffs = er.BaseStream.Position;
			er.Write((uint)0);
			er.WriteVector3(Scale);
			er.WriteVector3(Rotation);
			er.WriteVector3(Translation);
			er.Write(LocalMatrix, 0, 4 * 3);
			er.Write(WorldMatrix, 0, 4 * 3);

			er.Write(NrMeshes);
			long mshoffsoffs = er.BaseStream.Position;
			er.Write((uint)0);

			er.Write(NrMaterials);
			long matdictoffs = er.BaseStream.Position;
			er.Write((uint)0);

			er.Write(NrShapes);
			long shpoffsoffs = er.BaseStream.Position;
			er.Write((uint)0);

			er.Write(NrMeshNodes);
			long mshnodedictoffs = er.BaseStream.Position;
			er.Write((uint)0);

			er.Write(Unknown23);
			er.Write(Unknown24);
			er.Write(Unknown25);

			long skeletonoffs = er.BaseStream.Position;
			if (Skeleton != null) er.Write((uint)0);

			//seems to be padded to 8 bytes
			while ((er.BaseStream.Position % 8) != 0) er.Write((byte)0);

			long meshoffs = er.BaseStream.Position;
			er.BaseStream.Position = mshoffsoffs;
			er.Write((uint)(meshoffs - mshoffsoffs));
			er.BaseStream.Position = meshoffs;
			er.Write(new uint[NrMeshes], 0, (int)NrMeshes);

			long shpoffs = er.BaseStream.Position;
			er.BaseStream.Position = shpoffsoffs;
			er.Write((uint)(shpoffs - shpoffsoffs));
			er.BaseStream.Position = shpoffs;
			er.Write(new uint[NrShapes], 0, (int)NrShapes);

			long anmgrpdict = er.BaseStream.Position;
			er.BaseStream.Position = anmgrpdescdictoffs;
			er.Write((uint)(anmgrpdict - anmgrpdescdictoffs));
			er.BaseStream.Position = anmgrpdict;
			AnimationInfoDict.Write(er, c);

			long matdict = er.BaseStream.Position;
			er.BaseStream.Position = matdictoffs;
			er.Write((uint)(matdict - matdictoffs));
			er.BaseStream.Position = matdict;
			MaterialsDict.Write(er, c);

			long mshnoddict = er.BaseStream.Position;
			if (NrMeshNodes != 0 && MeshNodeVisibilitiesDict != null)
			{
				er.BaseStream.Position = mshnodedictoffs;
				er.Write((uint)(mshnoddict - mshnodedictoffs));
				er.BaseStream.Position = mshnoddict;
				MeshNodeVisibilitiesDict.Write(er, c);
			}

			for (int i = 0; i < NrAnimationGroupDescriptions; i++)
			{
				long curpos = er.BaseStream.Position;
				long bpos = er.BaseStream.Position = anmgrpdict + 0x1C + i * 0x10 + 0xC;
				er.Write((uint)(curpos - bpos));
				er.BaseStream.Position = curpos;
				AnimationGroupDescriptions[i].Write(er, c);
			}

			for (int i = 0; i < NrMeshes; i++)
			{
				while ((er.BaseStream.Position % 8) != 0) er.Write((byte)0);
				long curpos = er.BaseStream.Position;
				long bpos = er.BaseStream.Position = meshoffs + i * 4;
				er.Write((uint)(curpos - bpos));
				er.BaseStream.Position = curpos;
				Meshes[i].Write(er, c, basepos);
			}

			for (int i = 0; i < NrMaterials; i++)
			{
				long curpos = er.BaseStream.Position;
				long bpos = er.BaseStream.Position = matdict + 0x1C + i * 0x10 + 0xC;
				er.Write((uint)(curpos - bpos));
				er.BaseStream.Position = curpos;
				Materials[i].Write(er, c);
			}

			for (int i = 0; i < NrShapes; i++)
			{
				long curpos = er.BaseStream.Position;
				long bpos = er.BaseStream.Position = shpoffs + i * 4;
				er.Write((uint)(curpos - bpos));
				er.BaseStream.Position = curpos;
				Shapes[i].Write(er, c);
			}

			for (int i = 0; i < NrMeshNodes; i++)
			{
				long curpos = er.BaseStream.Position;
				long bpos = er.BaseStream.Position = mshnoddict + 0x1C + i * 0x10 + 0xC;
				er.Write((uint)(curpos - bpos));
				er.BaseStream.Position = curpos;
				MeshNodeVisibilities[i].Write(er, c);
			}

			if (Skeleton != null)
			{
				throw new NotImplementedException();
				long curpos = er.BaseStream.Position;
				er.BaseStream.Position = skeletonoffs;
				er.Write((uint)(curpos - skeletonoffs));
				er.BaseStream.Position = curpos;
				//Skeleton.Write(er, c);
			}

			//Mesh
			//Shape
			//animgroupdesc dict
			//materials dict
			//MeshNodeVisibilities Dict

			//animgroupdesc dict entries
			//mesh entries
			//material dict entries
			//shape entries
			//MeshNodeVisibilities Dict entries

			//skeleton
		}
		public UInt32 Type;//?
		public String Signature;
		public UInt32 Revision;
		public UInt32 NameOffset;
		public UInt32 Unknown2;//userdata nr entries
		public UInt32 Unknown3;//userdata dict offset
		public UInt32 Flags;
		public Boolean IsBranchVisible;
		public UInt32 NrChildren;
		public UInt32 Unknown7;//children
		public UInt32 NrAnimationGroupDescriptions;
		public UInt32 AnimationGroupDescriptionsDictOffset;
		public Vector3 Scale;
		public Vector3 Rotation;
		public Vector3 Translation;
		public Single[] LocalMatrix;//4x3
		public Single[] WorldMatrix;//4x3
		public UInt32 NrMeshes;
		public UInt32 MeshOffsetsOffset;
		public UInt32 NrMaterials;
		public UInt32 MaterialsDictOffset;
		public UInt32 NrShapes;
		public UInt32 ShapeOffsetsOffset;
		public UInt32 NrMeshNodes;
		public UInt32 MeshNodeVisibilitiesDictOffset;
		public UInt32 Unknown23;
		public UInt32 Unknown24;
		public UInt32 Unknown25;
		public UInt32 SkeletonInfoSOBJOffset;

		public UInt32[] MeshOffsets;
		public UInt32[] ShapeOffsets;

		public String Name;
		public DICT AnimationInfoDict;
		public DICT MaterialsDict;
		public DICT MeshNodeVisibilitiesDict;

		public GraphicsAnimationGroup[] AnimationGroupDescriptions;

		public Mesh[] Meshes;
		public SeparateDataShape[] Shapes;

		public SkeletonCtr Skeleton;

		public class GraphicsAnimationGroup
		{
			[Flags]
			public enum GraphicsAnimGroupFlags : uint
			{
				IsTransform = 1
			}
			public enum GraphicsMemberType : uint
			{
				None = 0,
				Bone = 1,
				Material = 2,
				Model = 3,
				Light = 4,
				Camera = 5,
				Fog = 6
			}
			public enum AnimGroupEvaluationTiming : uint
			{
				BeforeWorldUpdate = 0,
				AfterSceneCulling = 1
			}

			public GraphicsAnimationGroup(EndianBinaryReader er)
			{
				Type = er.ReadUInt32();
				Flags = (GraphicsAnimGroupFlags)er.ReadUInt32();
				NameOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
				MemberType = (GraphicsMemberType)er.ReadUInt32();
				NrMembers = er.ReadUInt32();
				MemberInfoDICTOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
				NrBlendOperations = er.ReadUInt32();
				BlendOperationArrayOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
				EvaluationTiming = (AnimGroupEvaluationTiming)er.ReadUInt32();

				long curpos = er.BaseStream.Position;
				er.BaseStream.Position = NameOffset;
				Name = er.ReadStringNT(Encoding.ASCII);
				er.BaseStream.Position = MemberInfoDICTOffset;
				MemberInfoDICT = new DICT(er);
				er.BaseStream.Position = BlendOperationArrayOffset;
				BlendOperations = er.ReadUInt32s((int)NrBlendOperations);

				AnimationGroupMembers = new AnimationGroupMember[NrMembers];
				for (int i = 0; i < NrMembers; i++)
				{
					er.BaseStream.Position = MemberInfoDICT[i].DataOffset;
					AnimationGroupMembers[i] = AnimationGroupMember.FromStream(er);
				}
				er.BaseStream.Position = curpos;
			}
			public void Write(EndianBinaryWriter er, CGFXWriterContext c)
			{
				er.Write(Type);
				er.Write((uint)Flags);
				c.WriteStringReference(Name, er);
				er.Write((uint)MemberType);

				er.Write(NrMembers);
				long mbrinfdictoffs = er.BaseStream.Position;
				er.Write((uint)0);

				er.Write(NrBlendOperations);
				long blndopoffs = er.BaseStream.Position;
				er.Write((uint)0);

				er.Write((uint)EvaluationTiming);

				long curpos = er.BaseStream.Position;
				er.BaseStream.Position = blndopoffs;
				er.Write((uint)(curpos - blndopoffs));
				er.BaseStream.Position = curpos;
				er.Write(BlendOperations, 0, BlendOperations.Length);

				long mbrinfodict = curpos = er.BaseStream.Position;
				er.BaseStream.Position = mbrinfdictoffs;
				er.Write((uint)(curpos - mbrinfdictoffs));
				er.BaseStream.Position = curpos;
				MemberInfoDICT.Write(er, c);

				for (int i = 0; i < AnimationGroupMembers.Length; i++)
				{
					curpos = er.BaseStream.Position;
					long bpos = er.BaseStream.Position = mbrinfodict + 0x1C + i * 0x10 + 0xC;
					er.Write((uint)(curpos - bpos));
					er.BaseStream.Position = curpos;
					AnimationGroupMembers[i].Write(er, c);
				}
			}

			public UInt32 Type;
			public GraphicsAnimGroupFlags Flags;
			public UInt32 NameOffset;
			public GraphicsMemberType MemberType;
			public UInt32 NrMembers;
			public UInt32 MemberInfoDICTOffset;
			public UInt32 NrBlendOperations;
			public UInt32 BlendOperationArrayOffset;
			public AnimGroupEvaluationTiming EvaluationTiming;

			public UInt32[] BlendOperations;

			public String Name;
			public DICT MemberInfoDICT;

			public AnimationGroupMember[] AnimationGroupMembers;
			public class AnimationGroupMember
			{
				public enum AnimationGroupMemberType : uint
				{
					MeshNodeVisibilityMember = 0x00080000,
					MeshMember = 0x01000000,
					TextureSamplerMember = 0x02000000,
					BlendOperationMember = 0x04000000,
					MaterialColorMember = 0x08000000,
					ModelMember = 0x10000000,
					TextureMapperMember = 0x20000000,
					BoneMember = 0x40000000,
					TextureCoordinatorMember = 0x80000000
				}
				public AnimationGroupMember(EndianBinaryReader er)
				{
					Type = (AnimationGroupMemberType)er.ReadUInt32();
					PathOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
					MemberOffset = er.ReadUInt32();
					BlendOperationIndex = er.ReadUInt32();
					ObjectType = er.ReadUInt32();
					MemberType = er.ReadUInt32();
					ResMaterialPtr = er.ReadUInt32();
					//Type Specific Stuff
					long curpos = er.BaseStream.Position;
					er.BaseStream.Position = PathOffset;
					Path = er.ReadStringNT(Encoding.ASCII);
					er.BaseStream.Position = curpos;
				}
				public virtual void Write(EndianBinaryWriter er, CGFXWriterContext c)
				{
					er.Write((uint)Type);
					c.WriteStringReference(Path, er);
					er.Write(MemberOffset);
					er.Write(BlendOperationIndex);
					er.Write(ObjectType);
					er.Write(MemberType);
					er.Write(ResMaterialPtr);
				}

				public AnimationGroupMemberType Type;
				public UInt32 PathOffset;
				public UInt32 MemberOffset;
				public UInt32 BlendOperationIndex;
				public UInt32 ObjectType;
				public UInt32 MemberType;
				public UInt32 ResMaterialPtr;

				public String Path;

				public static AnimationGroupMember FromStream(EndianBinaryReader er)
				{
					AnimationGroupMemberType Type = (AnimationGroupMemberType)er.ReadUInt32();
					er.BaseStream.Position -= 4;
					switch (Type)
					{
						case AnimationGroupMemberType.MeshNodeVisibilityMember:
							return new MeshNodeVisibilityMember(er);
						case AnimationGroupMemberType.MeshMember:
							return new MeshMember(er);
						case AnimationGroupMemberType.TextureSamplerMember:
							return new TextureSamplerMember(er);
						case AnimationGroupMemberType.BlendOperationMember:
							return new BlendOperationMember(er);
						case AnimationGroupMemberType.MaterialColorMember:
							return new MaterialColorMember(er);
						case AnimationGroupMemberType.ModelMember:
							return new ModelMember(er);
						case AnimationGroupMemberType.TextureMapperMember:
							return new TextureMapperMember(er);
						case AnimationGroupMemberType.BoneMember:
							return new BoneMember(er);
						case AnimationGroupMemberType.TextureCoordinatorMember:
							return new TextureCoordinatorMember(er);
					}
					return new AnimationGroupMember(er);
				}

				public override string ToString()
				{
					return Path;
				}
			}

			public class MeshNodeVisibilityMember : AnimationGroupMember
			{
				public MeshNodeVisibilityMember(EndianBinaryReader er)
					: base(er)
				{
					NodeNameOffset = (uint)er.BaseStream.Position + er.ReadUInt32();
					long curpos = er.BaseStream.Position;
					er.BaseStream.Position = NodeNameOffset;
					NodeName = er.ReadStringNT(Encoding.ASCII);
					er.BaseStream.Position = curpos;
				}
				public override void Write(EndianBinaryWriter er, CGFXWriterContext c)
				{
					base.Write(er, c);
					c.WriteStringReference(NodeName, er);
					er.Write(ObjectType);
				}

				public UInt32 NodeNameOffset;

				public String NodeName;
			}

			public class MeshMember : AnimationGroupMember
			{
				public MeshMember(EndianBinaryReader er)
					: base(er)
				{
					MeshIndex = er.ReadUInt32();
				}
				public override void Write(EndianBinaryWriter er, CGFXWriterContext c)
				{
					base.Write(er, c);
					er.Write(MeshIndex);
					er.Write(ObjectType);
				}
				public UInt32 MeshIndex;
			}

			public class TextureSamplerMember : AnimationGroupMember
			{
				public TextureSamplerMember(EndianBinaryReader er)
					: base(er)
				{
					MaterialNameOffset = (uint)er.BaseStream.Position + er.ReadUInt32();
					TextureMapperIndex = er.ReadUInt32();
					long curpos = er.BaseStream.Position;
					er.BaseStream.Position = MaterialNameOffset;
					MaterialName = er.ReadStringNT(Encoding.ASCII);
					er.BaseStream.Position = curpos;
				}
				public override void Write(EndianBinaryWriter er, CGFXWriterContext c)
				{
					base.Write(er, c);
					c.WriteStringReference(MaterialName, er);
					er.Write(TextureMapperIndex);
				}
				public UInt32 MaterialNameOffset;
				public UInt32 TextureMapperIndex;

				public String MaterialName;
			}

			public class BlendOperationMember : AnimationGroupMember
			{
				public BlendOperationMember(EndianBinaryReader er)
					: base(er)
				{
					MaterialNameOffset = (uint)er.BaseStream.Position + er.ReadUInt32();
					long curpos = er.BaseStream.Position;
					er.BaseStream.Position = MaterialNameOffset;
					MaterialName = er.ReadStringNT(Encoding.ASCII);
					er.BaseStream.Position = curpos;
				}
				public override void Write(EndianBinaryWriter er, CGFXWriterContext c)
				{
					base.Write(er, c);
					c.WriteStringReference(MaterialName, er);
					er.Write(ObjectType);
				}
				public UInt32 MaterialNameOffset;

				public String MaterialName;
			}

			public class MaterialColorMember : AnimationGroupMember
			{
				public MaterialColorMember(EndianBinaryReader er)
					: base(er)
				{
					MaterialNameOffset = (uint)er.BaseStream.Position + er.ReadUInt32();
					long curpos = er.BaseStream.Position;
					er.BaseStream.Position = MaterialNameOffset;
					MaterialName = er.ReadStringNT(Encoding.ASCII);
					er.BaseStream.Position = curpos;
				}
				public override void Write(EndianBinaryWriter er, CGFXWriterContext c)
				{
					base.Write(er, c);
					c.WriteStringReference(MaterialName, er);
					er.Write(ObjectType);
				}
				public UInt32 MaterialNameOffset;

				public String MaterialName;
			}

			public class ModelMember : AnimationGroupMember
			{
				public ModelMember(EndianBinaryReader er)
					: base(er) { }

				public override void Write(EndianBinaryWriter er, CGFXWriterContext c)
				{
					base.Write(er, c);
					er.Write(ObjectType);
				}
			}

			public class TextureMapperMember : AnimationGroupMember
			{
				public TextureMapperMember(EndianBinaryReader er)
					: base(er)
				{
					MaterialNameOffset = (uint)er.BaseStream.Position + er.ReadUInt32();
					TextureMapperIndex = er.ReadUInt32();
					long curpos = er.BaseStream.Position;
					er.BaseStream.Position = MaterialNameOffset;
					MaterialName = er.ReadStringNT(Encoding.ASCII);
					er.BaseStream.Position = curpos;
				}
				public override void Write(EndianBinaryWriter er, CGFXWriterContext c)
				{
					base.Write(er, c);
					c.WriteStringReference(MaterialName, er);
					er.Write(TextureMapperIndex);
				}
				public UInt32 MaterialNameOffset;
				public UInt32 TextureMapperIndex;

				public String MaterialName;
			}

			public class BoneMember : AnimationGroupMember
			{
				public BoneMember(EndianBinaryReader er)
					: base(er)
				{
					BoneNameOffset = (uint)er.BaseStream.Position + er.ReadUInt32();
					long curpos = er.BaseStream.Position;
					er.BaseStream.Position = BoneNameOffset;
					BoneName = er.ReadStringNT(Encoding.ASCII);
					er.BaseStream.Position = curpos;
				}
				public override void Write(EndianBinaryWriter er, CGFXWriterContext c)
				{
					base.Write(er, c);
					c.WriteStringReference(BoneName, er);
					er.Write(ObjectType);
				}
				public UInt32 BoneNameOffset;

				public String BoneName;
			}

			public class TextureCoordinatorMember : AnimationGroupMember
			{
				public TextureCoordinatorMember(EndianBinaryReader er)
					: base(er)
				{
					MaterialNameOffset = (uint)er.BaseStream.Position + er.ReadUInt32();
					TextureCoordinatorIndex = er.ReadUInt32();
					long curpos = er.BaseStream.Position;
					er.BaseStream.Position = MaterialNameOffset;
					MaterialName = er.ReadStringNT(Encoding.ASCII);
					er.BaseStream.Position = curpos;
				}
				public override void Write(EndianBinaryWriter er, CGFXWriterContext c)
				{
					base.Write(er, c);
					c.WriteStringReference(MaterialName, er);
					er.Write(TextureCoordinatorIndex);
				}
				public UInt32 MaterialNameOffset;
				public UInt32 TextureCoordinatorIndex;

				public String MaterialName;
			}

			public override string ToString()
			{
				return Name;
			}
		}

		public MeshNodeVisibilityCtr[] MeshNodeVisibilities;
		public class MeshNodeVisibilityCtr
		{
			public MeshNodeVisibilityCtr(EndianBinaryReader er)
			{
				NameOffset = (uint)er.BaseStream.Position + er.ReadUInt32();
				Visible = er.ReadUInt32() == 1;
				long curpos = er.BaseStream.Position;
				er.BaseStream.Position = NameOffset;
				Name = er.ReadStringNT(Encoding.ASCII);
				er.BaseStream.Position = curpos;
			}
			public void Write(EndianBinaryWriter er, CGFXWriterContext c)
			{
				c.WriteStringReference(Name, er);
				er.Write((uint)(Visible ? 1 : 0));
			}
			public UInt32 NameOffset;
			public Boolean Visible;

			public String Name;

			public override string ToString()
			{
				return Name;
			}
		}


		public MTOB[] Materials;
		public class MTOB
		{
			[Flags]
			public enum MaterialFlags : uint
			{
				FragmentLightEnabled = 1,
				VertexLightEnabled = 2,
				HemiSphereLightEnabled = 4,
				HemiSphereOcclusionEnabled = 8,
				FogEnabled = 16,
				ParticleMaterialEnabled = 32
			}

			public MTOB(EndianBinaryReader er)
			{
				Type = er.ReadUInt32();
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "MTOB") throw new SignatureNotCorrectException(Signature, "MTOB", er.BaseStream.Position);
				Revision = er.ReadUInt32();
				NameOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
				Unknown2 = er.ReadUInt32();
				Unknown3 = er.ReadUInt32();
				Flags = (MaterialFlags)er.ReadUInt32();
				TexCoordConfig = er.ReadUInt32();
				TranslucencyKind = er.ReadUInt32();
				MaterialColor = new MaterialColorCtr(er);
				Rasterization = new RasterizationCtr(er);
				FragmentOperation = new FragmentOperationCtr(er);
				NrActiveTextureCoordiators = er.ReadUInt32();
				TextureCoordiators = new TextureCoordinatorCtr[3];
				TextureCoordiators[0] = new TextureCoordinatorCtr(er);
				TextureCoordiators[1] = new TextureCoordinatorCtr(er);
				TextureCoordiators[2] = new TextureCoordinatorCtr(er);
				TexMapper0Offset = er.ReadUInt32();
				if (TexMapper0Offset != 0) TexMapper0Offset += (UInt32)er.BaseStream.Position - 4;
				TexMapper1Offset = er.ReadUInt32();
				if (TexMapper1Offset != 0) TexMapper1Offset += (UInt32)er.BaseStream.Position - 4;
				TexMapper2Offset = er.ReadUInt32();
				if (TexMapper2Offset != 0) TexMapper2Offset += (UInt32)er.BaseStream.Position - 4;
				ProcTexMapperOffset = er.ReadUInt32();
				if (ProcTexMapperOffset != 0) ProcTexMapperOffset += (UInt32)er.BaseStream.Position - 4;
				ShaderOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
				FragmentShaderOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
				ShaderProgramDescriptionIndex = er.ReadUInt32();
				NrShaderParameters = er.ReadUInt32();
				ShaderParametersOffsetArrayOffset = er.ReadUInt32();
				LightSetIndex = er.ReadUInt32();
				FogIndex = er.ReadUInt32();
				ShadingParameterHash = er.ReadUInt32();
				ShaderParametersHash = er.ReadUInt32();
				TextureCoordinatorsHash = er.ReadUInt32();
				TextureSamplersHash = er.ReadUInt32();
				TextureMappersHash = er.ReadUInt32();
				MaterialColorHash = er.ReadUInt32();
				RasterizationHash = er.ReadUInt32();
				FragmentLightingHash = er.ReadUInt32();
				FragmentLightingTableHash = er.ReadUInt32();
				FragmentLightingTableParametersHash = er.ReadUInt32();
				TextureCombinersHash = er.ReadUInt32();
				AlphaTestHash = er.ReadUInt32();
				FragmentOperationHash = er.ReadUInt32();
				MaterialId = er.ReadUInt32();

				long curpos = er.BaseStream.Position;
				er.BaseStream.Position = NameOffset;
				Name = er.ReadStringNT(Encoding.ASCII);
				if (TexMapper0Offset != 0)
				{
					er.BaseStream.Position = TexMapper0Offset;
					Tex0 = new TexInfo(er);
				}
				if (TexMapper1Offset != 0)
				{
					er.BaseStream.Position = TexMapper1Offset;
					Tex1 = new TexInfo(er);
				}
				if (TexMapper2Offset != 0)
				{
					er.BaseStream.Position = TexMapper2Offset;
					Tex2 = new TexInfo(er);
				}
				/*if (TexMapper3Offset != 0)
				{
					er.BaseStream.Position = Tex3Offset;
					Tex3 = new TexInfo(er);
				}*/
				//TODO: Procedural Texture Mapper
				er.BaseStream.Position = ShaderOffset;
				Shader = new SHDR(er);
				er.BaseStream.Position = FragmentShaderOffset;
				FragShader = new FragmentShader(er);
				er.BaseStream.Position = curpos;
			}
			private readonly int[] WrapMapper = { 2, 3, 0, 1 };
			private readonly int[] TexCombSrcMapper = { 0x84c0, 0x84c1, 0x84c2, 0x84c3, 0x8576, 0x8577, 0x8578, 0x8579, 0x6210, 0x6211 };
			public void Write(EndianBinaryWriter er, CGFXWriterContext c)
			{
				er.Write(Type);
				er.Write(Signature, Encoding.ASCII, false);
				er.Write(Revision);
				c.WriteStringReference(Name, er);
				er.Write(Unknown2);
				er.Write(Unknown3);
				er.Write((uint)Flags);
				er.Write(TexCoordConfig);
				er.Write(TranslucencyKind);
				MaterialColor.Write(er);
				Rasterization.Write(er);
				FragmentOperation.Write(er);
				er.Write(NrActiveTextureCoordiators);
				for (int i = 0; i < 3; i++)
				{
					TextureCoordiators[i].Write(er);
				}
				long offs = er.BaseStream.Position;
				er.Write((uint)0);//tex0
				er.Write((uint)0);//tex1
				er.Write((uint)0);//tex2
				er.Write((uint)0);//proctex
				er.Write((uint)0);//shader
				er.Write((uint)0);//fragshader
				er.Write(ShaderProgramDescriptionIndex);
				er.Write(NrShaderParameters);
				er.Write((uint)0);//ShaderParametersOffsetArrayOffset
				er.Write(LightSetIndex);
				er.Write(FogIndex);
				er.Write(CGFXWriterContext.CalcHash(BitConverter.GetBytes((uint)((uint)Flags | 0x20))));//ShadingParameterHash
				byte[] result = new byte[NrShaderParameters * 4];
				//TODO: Read the shaderparameters, and put in array for hash
				er.Write(CGFXWriterContext.CalcHash(result));//ShaderParametersHash;

				List<byte> lresult;/* = new List<byte>();
				lresult.AddRange(BitConverter.GetBytes(TexCoordConfig));
				for (int i = 0; i < /*3/NrActiveTextureCoordiators; i++)
				{
					lresult.AddRange(BitConverter.GetBytes(TextureCoordiators[i].SourceCoordinate));
					lresult.AddRange(BitConverter.GetBytes(TextureCoordiators[i].MappingMethod));
					lresult.AddRange(BitConverter.GetBytes(TextureCoordiators[i].ReferenceCamera));
					lresult.AddRange(BitConverter.GetBytes(TextureCoordiators[i].MatrixMode));
					lresult.AddRange(BitConverter.GetBytes(TextureCoordiators[i].Scale.X));
					lresult.AddRange(BitConverter.GetBytes(TextureCoordiators[i].Scale.Y));
					lresult.AddRange(BitConverter.GetBytes(TextureCoordiators[i].Rotate));
					lresult.AddRange(BitConverter.GetBytes(TextureCoordiators[i].Translate.X));
					lresult.AddRange(BitConverter.GetBytes(TextureCoordiators[i].Translate.Y));
					lresult.AddRange(BitConverter.GetBytes(false));
					lresult.AddRange(BitConverter.GetBytes(TextureCoordiators[i].Matrix[0]));
					lresult.AddRange(BitConverter.GetBytes(TextureCoordiators[i].Matrix[1]));
					lresult.AddRange(BitConverter.GetBytes(TextureCoordiators[i].Matrix[2]));
					lresult.AddRange(BitConverter.GetBytes(TextureCoordiators[i].Matrix[3]));
					lresult.AddRange(BitConverter.GetBytes(TextureCoordiators[i].Matrix[4]));
					lresult.AddRange(BitConverter.GetBytes(TextureCoordiators[i].Matrix[5]));
					lresult.AddRange(BitConverter.GetBytes(TextureCoordiators[i].Matrix[6]));
					lresult.AddRange(BitConverter.GetBytes(TextureCoordiators[i].Matrix[7]));
					lresult.AddRange(BitConverter.GetBytes(TextureCoordiators[i].Matrix[8]));
					lresult.AddRange(BitConverter.GetBytes(TextureCoordiators[i].Matrix[9]));
					lresult.AddRange(BitConverter.GetBytes(TextureCoordiators[i].Matrix[10]));
					lresult.AddRange(BitConverter.GetBytes(TextureCoordiators[i].Matrix[11]));
					lresult.AddRange(BitConverter.GetBytes(TexCoordConfig));
				}*/
				er.Write((uint)0);//TextureCoordinatorsHash;

				lresult = new List<byte>();
				lresult.AddRange(BitConverter.GetBytes(TexCoordConfig));
				if (Tex0 != null && Tex0.Sampler is TexInfo.StandardTextureSamplerCtr)
				{
					var v = Tex0.Sampler as TexInfo.StandardTextureSamplerCtr;
					lresult.AddRange(BitConverter.GetBytes(v.BorderColor.X));
					lresult.AddRange(BitConverter.GetBytes(v.BorderColor.Y));
					lresult.AddRange(BitConverter.GetBytes(v.BorderColor.Z));
					lresult.AddRange(BitConverter.GetBytes(v.BorderColor.W));
					lresult.AddRange(BitConverter.GetBytes(WrapMapper[(Tex0.Unknown12 >> 12) & 0xF]));
					lresult.AddRange(BitConverter.GetBytes(WrapMapper[(Tex0.Unknown12 >> 8) & 0xF]));
					lresult.AddRange(BitConverter.GetBytes(0f));
					lresult.AddRange(BitConverter.GetBytes(v.LodBias));
					lresult.AddRange(BitConverter.GetBytes(v.MinFilter));
					lresult.AddRange(BitConverter.GetBytes((uint)1));
				}
				if (Tex1 != null && Tex1.Sampler is TexInfo.StandardTextureSamplerCtr)
				{
					var v = Tex1.Sampler as TexInfo.StandardTextureSamplerCtr;
					lresult.AddRange(BitConverter.GetBytes(v.BorderColor.X));
					lresult.AddRange(BitConverter.GetBytes(v.BorderColor.Y));
					lresult.AddRange(BitConverter.GetBytes(v.BorderColor.Z));
					lresult.AddRange(BitConverter.GetBytes(v.BorderColor.W));
					lresult.AddRange(BitConverter.GetBytes(WrapMapper[(Tex1.Unknown12 >> 12) & 0xF]));
					lresult.AddRange(BitConverter.GetBytes(WrapMapper[(Tex1.Unknown12 >> 8) & 0xF]));
					lresult.AddRange(BitConverter.GetBytes(0f));
					lresult.AddRange(BitConverter.GetBytes(v.LodBias));
					lresult.AddRange(BitConverter.GetBytes(v.MinFilter));
					lresult.AddRange(BitConverter.GetBytes((uint)1));
				}
				if (Tex2 != null && Tex2.Sampler is TexInfo.StandardTextureSamplerCtr)
				{
					var v = Tex2.Sampler as TexInfo.StandardTextureSamplerCtr;
					lresult.AddRange(BitConverter.GetBytes(v.BorderColor.X));
					lresult.AddRange(BitConverter.GetBytes(v.BorderColor.Y));
					lresult.AddRange(BitConverter.GetBytes(v.BorderColor.Z));
					lresult.AddRange(BitConverter.GetBytes(v.BorderColor.W));
					lresult.AddRange(BitConverter.GetBytes(WrapMapper[(Tex2.Unknown12 >> 12) & 0xF]));
					lresult.AddRange(BitConverter.GetBytes(WrapMapper[(Tex2.Unknown12 >> 8) & 0xF]));
					lresult.AddRange(BitConverter.GetBytes(0f));
					lresult.AddRange(BitConverter.GetBytes(v.LodBias));
					lresult.AddRange(BitConverter.GetBytes(v.MinFilter));
					lresult.AddRange(BitConverter.GetBytes((uint)1));
				}
				er.Write(CGFXWriterContext.CalcHash(lresult.ToArray()));//TextureSamplersHash;
				er.Write((uint)0);
				er.Write(MaterialColor.GetHash());
				er.Write(Rasterization.GetHash());
				lresult = new List<byte>();
				lresult.AddRange(BitConverter.GetBytes((uint)FragShader.FragmentLighting.Flags));
				lresult.AddRange(BitConverter.GetBytes(FragShader.FragmentLighting.LayerConfig));
				lresult.AddRange(BitConverter.GetBytes(FragShader.FragmentLighting.FresnelConfig));
				lresult.AddRange(BitConverter.GetBytes(FragShader.FragmentLighting.BumpTextureIndex));
				lresult.AddRange(BitConverter.GetBytes(FragShader.FragmentLighting.BumpMode));
				lresult.AddRange(BitConverter.GetBytes(FragShader.FragmentLighting.IsBumpRenormalize));
				//lresult.AddRange(BitConverter.GetBytes((Flags & MaterialFlags.FragmentLightEnabled) != 0));
				er.Write(CGFXWriterContext.CalcHash(lresult.ToArray()));//FragmentLightingHash;
				er.Write((uint)0);
				er.Write(FragShader.FragmentLightingTable.GetHash());//FragmentLightingTableParametersHash;
				/*lresult = new List<byte>();
				lresult.AddRange(BitConverter.GetBytes(FragShader.BufferColor.X));
				lresult.AddRange(BitConverter.GetBytes(FragShader.BufferColor.Y));
				lresult.AddRange(BitConverter.GetBytes(FragShader.BufferColor.Z));
				lresult.AddRange(BitConverter.GetBytes(FragShader.BufferColor.W));
				for (int i = 0; i < 6; i++)
				{
					lresult.AddRange(BitConverter.GetBytes(FragShader.TextureCombiners[i].Constant));
					lresult.AddRange(BitConverter.GetBytes(TexCombSrcMapper[FragShader.TextureCombiners[i].ScaleRgb & 0xF]));
					lresult.AddRange(BitConverter.GetBytes(TexCombSrcMapper[(FragShader.TextureCombiners[i].ScaleRgb >> 4) & 0xF]));
					lresult.AddRange(BitConverter.GetBytes(TexCombSrcMapper[(FragShader.TextureCombiners[i].ScaleRgb >> 8) & 0xF]));
				}*/
				er.Write((uint)0);//TextureCombinersHash;
				er.Write(FragShader.AlphaTest.GetHash());//AlphaTestHash;
				er.Write(FragmentOperation.GetHash());//FragmentOperationHash;
				er.Write(MaterialId);
				if (Tex0 != null)
				{
					long curpos = er.BaseStream.Position;
					er.BaseStream.Position = offs + 0;
					er.Write((uint)(curpos - (offs + 0)));
					er.BaseStream.Position = curpos;
					Tex0.Write(er, c);
				}
				if (Tex1 != null)
				{
					long curpos = er.BaseStream.Position;
					er.BaseStream.Position = offs + 4;
					er.Write((uint)(curpos - (offs + 4)));
					er.BaseStream.Position = curpos;
					Tex1.Write(er, c);
				}
				if (Tex2 != null)
				{
					long curpos = er.BaseStream.Position;
					er.BaseStream.Position = offs + 8;
					er.Write((uint)(curpos - (offs + 8)));
					er.BaseStream.Position = curpos;
					Tex2.Write(er, c);
				}
				if (ProcTex != null)
				{
					long curpos = er.BaseStream.Position;
					er.BaseStream.Position = offs + 12;
					er.Write((uint)(curpos - (offs + 12)));
					er.BaseStream.Position = curpos;
					//currently unsupported!
					//ProcTex.Write(er, c);
				}
				if (Shader != null)
				{
					long curpos = er.BaseStream.Position;
					er.BaseStream.Position = offs + 16;
					er.Write((uint)(curpos - (offs + 16)));
					er.BaseStream.Position = curpos;
					Shader.Write(er, c);
				}
				if (FragShader != null)
				{
					long curpos = er.BaseStream.Position;
					er.BaseStream.Position = offs + 20;
					er.Write((uint)(curpos - (offs + 20)));
					er.BaseStream.Position = curpos;
					FragShader.Write(er, c);
				}
			}
			public UInt32 Type;
			public String Signature;
			public UInt32 Revision;
			public UInt32 NameOffset;
			public UInt32 Unknown2;
			public UInt32 Unknown3;
			public MaterialFlags Flags;
			public UInt32 TexCoordConfig;
			public UInt32 TranslucencyKind;
			public MaterialColorCtr MaterialColor;
			public RasterizationCtr Rasterization;
			public FragmentOperationCtr FragmentOperation;
			public UInt32 NrActiveTextureCoordiators;
			public TextureCoordinatorCtr[] TextureCoordiators;
			public UInt32 TexMapper0Offset;
			public UInt32 TexMapper1Offset;
			public UInt32 TexMapper2Offset;
			public UInt32 ProcTexMapperOffset;
			public UInt32 ShaderOffset;
			public UInt32 FragmentShaderOffset;
			public UInt32 ShaderProgramDescriptionIndex;//Shader Related, index into unknown 2 array of binary shader (SHDR)			
			public UInt32 NrShaderParameters;
			public UInt32 ShaderParametersOffsetArrayOffset;
			public UInt32 LightSetIndex;
			public UInt32 FogIndex;
			public UInt32 ShadingParameterHash;
			public UInt32 ShaderParametersHash;
			public UInt32 TextureCoordinatorsHash;
			public UInt32 TextureSamplersHash;
			public UInt32 TextureMappersHash;
			public UInt32 MaterialColorHash;
			public UInt32 RasterizationHash;
			public UInt32 FragmentLightingHash;
			public UInt32 FragmentLightingTableHash;
			public UInt32 FragmentLightingTableParametersHash;
			public UInt32 TextureCombinersHash;
			public UInt32 AlphaTestHash;
			public UInt32 FragmentOperationHash;
			public UInt32 MaterialId;

			public String Name;
			public TexInfo Tex0;
			public TexInfo Tex1;
			public TexInfo Tex2;
			public TexInfo ProcTex;//unsupported for now!

			public class MaterialColorCtr
			{
				public MaterialColorCtr(EndianBinaryReader er)
				{
					Emission = er.ReadVector4();
					Ambient = er.ReadVector4();
					Diffuse = er.ReadVector4();
					Specular0 = er.ReadVector4();
					Specular1 = er.ReadVector4();
					Constant0 = er.ReadVector4();
					Constant1 = er.ReadVector4();
					Constant2 = er.ReadVector4();
					Constant3 = er.ReadVector4();
					Constant4 = er.ReadVector4();
					Constant5 = er.ReadVector4();

					EmissionU32 = er.ReadColor8();
					AmbientU32 = er.ReadColor8();
					DiffuseU32 = er.ReadColor8();
					Specular0U32 = er.ReadColor8();
					Specular1U32 = er.ReadColor8();
					Constant0U32 = er.ReadColor8();
					Constant1U32 = er.ReadColor8();
					Constant2U32 = er.ReadColor8();
					Constant3U32 = er.ReadColor8();
					Constant4U32 = er.ReadColor8();
					Constant5U32 = er.ReadColor8();

					CommandCache = er.ReadUInt32();
				}
				public void Write(EndianBinaryWriter er)
				{
					er.WriteVector4(Emission);
					er.WriteVector4(Ambient);
					er.WriteVector4(Diffuse);
					er.WriteVector4(Specular0);
					er.WriteVector4(Specular1);
					er.WriteVector4(Constant0);
					er.WriteVector4(Constant1);
					er.WriteVector4(Constant2);
					er.WriteVector4(Constant3);
					er.WriteVector4(Constant4);
					er.WriteVector4(Constant5);

					er.WriteColor8(EmissionU32);
					er.WriteColor8(AmbientU32);
					er.WriteColor8(DiffuseU32);
					er.WriteColor8(Specular0U32);
					er.WriteColor8(Specular1U32);
					er.WriteColor8(Constant0U32);
					er.WriteColor8(Constant1U32);
					er.WriteColor8(Constant2U32);
					er.WriteColor8(Constant3U32);
					er.WriteColor8(Constant4U32);
					er.WriteColor8(Constant5U32);

					er.Write(CommandCache);
				}
				public uint GetHash()
				{
					byte[] Result = new byte[0xA4];
					int offs = 0;
					CGFXWriterContext.WriteFloatColorRGB(Result, offs, Emission);
					offs += 0xC;
					CGFXWriterContext.WriteFloatColorRGBA(Result, offs, Ambient);
					offs += 0x10;
					CGFXWriterContext.WriteFloatColorRGBA(Result, offs, Diffuse);
					offs += 0x10;
					CGFXWriterContext.WriteFloatColorRGB(Result, offs, Specular0);
					offs += 0xC;
					CGFXWriterContext.WriteFloatColorRGB(Result, offs, Specular1);
					offs += 0xC;
					CGFXWriterContext.WriteFloatColorRGBA(Result, offs, Constant0);
					offs += 0x10;
					CGFXWriterContext.WriteFloatColorRGBA(Result, offs, Constant1);
					offs += 0x10;
					CGFXWriterContext.WriteFloatColorRGBA(Result, offs, Constant2);
					offs += 0x10;
					CGFXWriterContext.WriteFloatColorRGBA(Result, offs, Constant3);
					offs += 0x10;
					CGFXWriterContext.WriteFloatColorRGBA(Result, offs, Constant4);
					offs += 0x10;
					CGFXWriterContext.WriteFloatColorRGBA(Result, offs, Constant5);
					offs += 0x10;
					return CGFXWriterContext.CalcHash(Result);
				}
				public Vector4 Emission;//R,G,B,A singles
				public Vector4 Ambient;//and vertex color scale
				public Vector4 Diffuse;
				public Vector4 Specular0;
				public Vector4 Specular1;
				public Vector4 Constant0;
				public Vector4 Constant1;
				public Vector4 Constant2;
				public Vector4 Constant3;
				public Vector4 Constant4;
				public Vector4 Constant5;

				public Color EmissionU32;//U32
				public Color AmbientU32;
				public Color DiffuseU32;
				public Color Specular0U32;
				public Color Specular1U32;
				public Color Constant0U32;
				public Color Constant1U32;
				public Color Constant2U32;
				public Color Constant3U32;
				public Color Constant4U32;
				public Color Constant5U32;

				public UInt32 CommandCache;
			}

			public class RasterizationCtr
			{
				[Flags]
				public enum RasterizationFlags : uint
				{
					PolygonOffsetEnabled = 1
				}
				public RasterizationCtr(EndianBinaryReader er)
				{
					Flags = (RasterizationFlags)er.ReadUInt32();
					CullingMode = er.ReadUInt32();
					PolygonOffsetUnit = er.ReadSingle();
					Command1 = er.ReadUInt32();
					Command2 = er.ReadUInt32();
				}
				public void Write(EndianBinaryWriter er)
				{
					er.Write((uint)Flags);
					er.Write(CullingMode);
					er.Write(PolygonOffsetUnit);
					er.Write(Command1);
					er.Write(Command2);
				}
				public uint GetHash()
				{
					byte[] Result = new byte[0x10];
					IOUtil.WriteU32LE(Result, 0, (uint)Flags);
					IOUtil.WriteU32LE(Result, 4, Command1);
					IOUtil.WriteU32LE(Result, 8, Command2);
					IOUtil.WriteSingleLE(Result, 12, PolygonOffsetUnit);
					return CGFXWriterContext.CalcHash(Result);
				}
				public RasterizationFlags Flags;
				public UInt32 CullingMode;
				public Single PolygonOffsetUnit;
				public UInt32 Command1;
				public UInt32 Command2;
			}

			public class FragmentOperationCtr
			{
				public FragmentOperationCtr(EndianBinaryReader er)
				{
					DepthOperation = new DepthOperationCtr(er);
					BlendOperation = new BlendOperationCtr(er);
					StencilOperation = new StencilOperationCtr(er);
				}
				public void Write(EndianBinaryWriter er)
				{
					DepthOperation.Write(er);
					BlendOperation.Write(er);
					StencilOperation.Write(er);
				}
				private readonly uint[] DepthOpTransform = { 0, 1, 4, 7, 2, 3, 6, 5 };
				private readonly uint[] BlendLogicOpTransform = { 0, 8, 6, 1, 3, 4, 2, 5, 10, 9, 11, 12, 13, 14, 7, 15 };
				public uint GetHash()
				{
					int blendmode;
					if (BlendOperation.Command4 != 0) blendmode = 3;
					else if (((BlendOperation.Command3 >> 16) & 0xF) == 1 && ((BlendOperation.Command3 >> 20) & 0xF) == 0
						&& ((BlendOperation.Command3 >> 24) & 0xF) == 1 && ((BlendOperation.Command3 >> 8) & 0xFF) == 0) blendmode = 0;
					else if ((((BlendOperation.Command3 >> 16) & 0xF) != 1 || ((BlendOperation.Command3 >> 20) & 0xF) != 0)
						&& ((BlendOperation.Command3 >> 24) & 0xF) == 1 && ((BlendOperation.Command3 >> 8) & 0xFF) == 0) blendmode = 2;
					else blendmode = 1;


					byte[] Result = new byte[0x4C];
					IOUtil.WriteU32LE(Result, 0, (uint)DepthOperation.Flags);
					IOUtil.WriteU32LE(Result, 4, DepthOpTransform[(DepthOperation.Command1 >> 4) & 0xF]);
					IOUtil.WriteU32LE(Result, 8, 0);
					IOUtil.WriteU32LE(Result, 12, (uint)blendmode);///*BlendOperation.Mode*/(uint)((BlendOperation.Command1 >> 8) & 0xFF));
					CGFXWriterContext.WriteFloatColorRGBA(Result, 16, BlendOperation.BlendColor);
					if (blendmode != 3) IOUtil.WriteU32LE(Result, 32, 1);
					else IOUtil.WriteU32LE(Result, 32, BlendLogicOpTransform[BlendOperation.Command4]);

					if (blendmode == 0)
					{
						IOUtil.WriteU32LE(Result, 36, (uint)BlendOperationCtr.GlFactor[6]);
						IOUtil.WriteU32LE(Result, 40, (uint)BlendOperationCtr.GlFactor[7]);
						IOUtil.WriteU32LE(Result, 44, (uint)BlendOperationCtr.GlEquation[0]);
						IOUtil.WriteU32LE(Result, 48, (uint)BlendOperationCtr.GlFactor[1]);
						IOUtil.WriteU32LE(Result, 52, (uint)BlendOperationCtr.GlFactor[0]);
						IOUtil.WriteU32LE(Result, 56, (uint)BlendOperationCtr.GlEquation[0]);
					}
					else if (blendmode == 1)
					{
						IOUtil.WriteU32LE(Result, 36, (uint)BlendOperationCtr.GlFactor[(BlendOperation.Command3 >> 16) & 0xF]);
						IOUtil.WriteU32LE(Result, 40, (uint)BlendOperationCtr.GlFactor[(BlendOperation.Command3 >> 20) & 0xF]);
						IOUtil.WriteU32LE(Result, 44, (uint)BlendOperationCtr.GlEquation[(BlendOperation.Command3 >> 0) & 0xFF]);
						IOUtil.WriteU32LE(Result, 48, (uint)BlendOperationCtr.GlFactor[1]);
						IOUtil.WriteU32LE(Result, 52, (uint)BlendOperationCtr.GlFactor[0]);
						IOUtil.WriteU32LE(Result, 56, (uint)BlendOperationCtr.GlEquation[0]);
					}
					else if (blendmode == 2)
					{
						IOUtil.WriteU32LE(Result, 36, (uint)BlendOperationCtr.GlFactor[(BlendOperation.Command3 >> 16) & 0xF]);
						IOUtil.WriteU32LE(Result, 40, (uint)BlendOperationCtr.GlFactor[(BlendOperation.Command3 >> 20) & 0xF]);
						IOUtil.WriteU32LE(Result, 44, (uint)BlendOperationCtr.GlEquation[(BlendOperation.Command3 >> 0) & 0xFF]);
						IOUtil.WriteU32LE(Result, 48, (uint)BlendOperationCtr.GlFactor[(BlendOperation.Command3 >> 24) & 0xF]);
						IOUtil.WriteU32LE(Result, 52, (uint)BlendOperationCtr.GlFactor[(BlendOperation.Command3 >> 28) & 0xF]);
						IOUtil.WriteU32LE(Result, 56, (uint)BlendOperationCtr.GlEquation[(BlendOperation.Command3 >> 8) & 0xFF]);
					}
					else
					{
						IOUtil.WriteU32LE(Result, 36, (uint)BlendOperationCtr.GlFactor[6]);
						IOUtil.WriteU32LE(Result, 40, (uint)BlendOperationCtr.GlFactor[7]);
						IOUtil.WriteU32LE(Result, 44, (uint)BlendOperationCtr.GlEquation[0]);
						IOUtil.WriteU32LE(Result, 48, (uint)BlendOperationCtr.GlFactor[1]);
						IOUtil.WriteU32LE(Result, 52, (uint)BlendOperationCtr.GlFactor[0]);
						IOUtil.WriteU32LE(Result, 56, (uint)BlendOperationCtr.GlEquation[0]);
					}
					IOUtil.WriteU32LE(Result, 60, StencilOperation.Command1);
					IOUtil.WriteU32LE(Result, 64, StencilOperation.Command2);
					IOUtil.WriteU32LE(Result, 68, StencilOperation.Command3);
					IOUtil.WriteU32LE(Result, 72, StencilOperation.Command4);
					return CGFXWriterContext.CalcHash(Result);
				}
				public DepthOperationCtr DepthOperation;
				public class DepthOperationCtr
				{
					[Flags]
					public enum DepthFlags : uint
					{
						TestEnabled = 1,
						MaskEnabled = 2
					}
					public DepthOperationCtr(EndianBinaryReader er)
					{
						Flags = (DepthFlags)er.ReadUInt32();
						Command1 = er.ReadUInt32();
						Command2 = er.ReadUInt32();
						Command3 = er.ReadUInt32();
						Command4 = er.ReadUInt32();
					}
					public void Write(EndianBinaryWriter er)
					{
						er.Write((uint)Flags);
						er.Write(Command1);
						er.Write(Command2);
						er.Write(Command3);
						er.Write(Command4);
					}
					public DepthFlags Flags;
					public UInt32 Command1;
					public UInt32 Command2;
					public UInt32 Command3;
					public UInt32 Command4;
				}
				public BlendOperationCtr BlendOperation;
				public class BlendOperationCtr
				{
					public BlendOperationCtr(EndianBinaryReader er)
					{
						Mode = er.ReadUInt32();
						BlendColor = er.ReadVector4();
						Command1 = er.ReadUInt32();
						Command2 = er.ReadUInt32();
						Command3 = er.ReadUInt32();
						Command4 = er.ReadUInt32();
						Command5 = er.ReadUInt32();
						Command6 = er.ReadUInt32();
					}
					public void Write(EndianBinaryWriter er)
					{
						er.Write((uint)Mode);
						er.WriteVector4(BlendColor);
						er.Write(Command1);
						er.Write(Command2);
						er.Write(Command3);
						er.Write(Command4);
						er.Write(Command5);
						er.Write(Command6);
					}
					public UInt32 Mode;
					public Vector4 BlendColor;//4 singles
					public UInt32 Command1;
					//00E4 MMMM MMMM 0000 0000
					//MMMM MMMM = Blend Mode
					public UInt32 Command2;
					public UInt32 Command3;
					//ADAD ASAS CDCD CSCS AFAF AFAF CFCF CFCF
					//ADAD = Alpha Destination
					//ASAS = Alpha Source
					//CDCD = Color Destination
					//CSCS = Color Source
					//AFAF AFAF = Alpha Function
					//CFCF CFCF = Color Function
					public UInt32 Command4;
					public UInt32 Command5;
					public UInt32 Command6;

					public static readonly int[] GlFactor =
					{
						Gl.GL_ZERO,
						Gl.GL_ONE,
						Gl.GL_SRC_COLOR,
						Gl.GL_ONE_MINUS_SRC_COLOR,
						Gl.GL_DST_COLOR,
						Gl.GL_ONE_MINUS_DST_COLOR,
						Gl.GL_SRC_ALPHA,
						Gl.GL_ONE_MINUS_SRC_ALPHA,
						Gl.GL_DST_ALPHA,
						Gl.GL_ONE_MINUS_DST_ALPHA,
						Gl.GL_CONSTANT_COLOR,
						Gl.GL_ONE_MINUS_CONSTANT_COLOR,
						Gl.GL_CONSTANT_ALPHA,
						Gl.GL_ONE_MINUS_CONSTANT_ALPHA,
						Gl.GL_SRC_ALPHA_SATURATE
					};

					public static readonly int[] GlEquation =
					{
						Gl.GL_FUNC_ADD,
						Gl.GL_FUNC_SUBTRACT,
						Gl.GL_FUNC_REVERSE_SUBTRACT,
						Gl.GL_MIN,
						Gl.GL_MAX
					};

					public void GlApply()
					{
						uint AD = (Command3 >> 28) & 0xF;
						uint AS = (Command3 >> 24) & 0xF;
						uint CD = (Command3 >> 20) & 0xF;
						uint CS = (Command3 >> 16) & 0xF;
						uint AF = (Command3 >> 8) & 0xFF;
						uint CF = (Command3 >> 0) & 0xFF;
						if (((Command1 >> 8) & 0xFF) == 1)
						{
							Gl.glEnable(Gl.GL_BLEND);
							Gl.glBlendColor(BlendColor.X, BlendColor.Y, BlendColor.Z, BlendColor.W);
							Gl.glBlendFuncSeparate(GlFactor[CS], GlFactor[CD], GlFactor[AS], GlFactor[AD]);
							Gl.glBlendEquationSeparate(GlEquation[CF], GlEquation[AF]);
							Gl.glEnable(Gl.GL_BLEND);
						}
						else
						{
							Gl.glDisable(Gl.GL_BLEND);
						}
					}
				}
				public StencilOperationCtr StencilOperation;
				public class StencilOperationCtr
				{
					public StencilOperationCtr(EndianBinaryReader er)
					{
						Command1 = er.ReadUInt32();
						Command2 = er.ReadUInt32();
						Command3 = er.ReadUInt32();
						Command4 = er.ReadUInt32();
					}
					public void Write(EndianBinaryWriter er)
					{
						er.Write(Command1);
						er.Write(Command2);
						er.Write(Command3);
						er.Write(Command4);
					}
					public UInt32 Command1;
					public UInt32 Command2;
					public UInt32 Command3;
					public UInt32 Command4;
				}
			}

			public class TextureCoordinatorCtr
			{
				public TextureCoordinatorCtr(EndianBinaryReader er)
				{
					SourceCoordinate = er.ReadUInt32();
					MappingMethod = er.ReadUInt32();
					ReferenceCamera = er.ReadInt32();
					MatrixMode = er.ReadUInt32();
					Scale = new Vector2(er.ReadSingle(), er.ReadSingle());
					Rotate = er.ReadSingle();
					Translate = new Vector2(er.ReadSingle(), er.ReadSingle());
					Unknown3 = er.ReadUInt32();
					Matrix = er.ReadSingles(4 * 3);
				}
				public void Write(EndianBinaryWriter er)
				{
					er.Write(SourceCoordinate);
					er.Write(MappingMethod);
					er.Write(ReferenceCamera);
					er.Write(MatrixMode);
					er.WriteVector2(Scale);
					er.Write(Rotate);
					er.WriteVector2(Translate);
					er.Write(Unknown3);
					er.Write(Matrix, 0, 4 * 3);
				}
				public UInt32 SourceCoordinate;
				public UInt32 MappingMethod;//0 = UV, 1 = Cube, 2 = Sphere, 3 = Projection, 4 = Shadow
				public Int32 ReferenceCamera;
				public UInt32 MatrixMode;
				public Vector2 Scale;
				public Single Rotate;
				public Vector2 Translate;
				public UInt32 Unknown3;
				public Single[] Matrix;//4*3
			}

			public class TexInfo
			{
				public TexInfo(EndianBinaryReader er)
				{
					Type = er.ReadUInt32();
					DynamicAllocator = er.ReadUInt32();
					TXOBOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
					SamplerOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
					Unknown4 = er.ReadUInt32();
					Unknown5 = er.ReadUInt16();
					Unknown6 = er.ReadUInt16();
					Unknown7 = er.ReadUInt32();
					Unknown8 = er.ReadUInt16();
					Unknown9 = er.ReadUInt16();
					Height = er.ReadUInt16();
					Width = er.ReadUInt16();
					Unknown12 = er.ReadUInt32();
					Unknown13 = er.ReadUInt32();
					Unknown14 = er.ReadUInt32();
					Unknown15 = er.ReadUInt32();
					Unknown16 = er.ReadUInt32();
					Unknown17 = er.ReadUInt32();
					Unknown18 = er.ReadUInt32();
					Unknown19 = er.ReadUInt32();
					Unknown20 = er.ReadUInt32();
					CommandSizeToSend = er.ReadUInt32();

					long curpos = er.BaseStream.Position;
					er.BaseStream.Position = TXOBOffset;
					TextureObject = TXOB.FromStream(er);
					er.BaseStream.Position = SamplerOffset;
					Sampler = TextureSamplerCtr.FromStream(er);
					er.BaseStream.Position = curpos;
				}
				public void Write(EndianBinaryWriter er, CGFXWriterContext c)
				{
					long basepos = er.BaseStream.Position;
					er.Write(Type);
					er.Write(DynamicAllocator);
					long offs = er.BaseStream.Position;
					er.Write((uint)0);
					er.Write((uint)0);
					er.Write(Unknown4);
					er.Write(Unknown5);
					er.Write(Unknown6);
					er.Write(Unknown7);
					er.Write(Unknown8);
					er.Write(Unknown9);
					er.Write(Height);
					er.Write(Width);
					er.Write(Unknown12);
					er.Write(Unknown13);
					er.Write(Unknown14);
					er.Write(Unknown15);
					er.Write(Unknown16);
					er.Write(Unknown17);
					er.Write(Unknown18);
					er.Write(Unknown19);
					er.Write(Unknown20);
					er.Write(CommandSizeToSend);

					long curpos = er.BaseStream.Position;
					er.BaseStream.Position = offs;
					er.Write((uint)(curpos - offs));
					er.BaseStream.Position = curpos;
					TextureObject.Write(er, c);

					curpos = er.BaseStream.Position;
					er.BaseStream.Position = offs + 4;
					er.Write((uint)(curpos - (offs + 4)));
					er.BaseStream.Position = curpos;
					Sampler.Write(er, basepos);
				}
				public UInt32 Type;//0
				public UInt32 DynamicAllocator;//4
				public UInt32 TXOBOffset;//8
				public UInt32 SamplerOffset;//C
				public UInt32 Unknown4;//10
				public UInt16 Unknown5;//14
				public UInt16 Unknown6;//16
				public UInt32 Unknown7;//18
				public UInt16 Unknown8;//1C
				public UInt16 Unknown9;//1E
				public UInt16 Height;//20  Command 0x000F0082
				public UInt16 Width;//22
				public UInt32 Unknown12;//24  Command 0x000F0083?
				public UInt32 Unknown13;//28
				public UInt32 Unknown14;//2C  Command 0x000F0085?
				public UInt32 Unknown15;//30
				public UInt32 Unknown16;//34
				public UInt32 Unknown17;//38
				public UInt32 Unknown18;//3C
				public UInt32 Unknown19;//40
				public UInt32 Unknown20;//44
				public UInt32 CommandSizeToSend;//48

				//The types are like this:
				//Pixel Based Texture Mapper = 0x80000000
				//Procedural Texture Mapper = 0x40000000

				public TXOB TextureObject;
				/*public TXOB TextureObject;
				public class TXOB
				{
					public TXOB(EndianBinaryReader er)
					{
						Type = er.ReadUInt32();
						Signature = er.ReadString(Encoding.ASCII, 4);
						if (Signature != "TXOB") throw new SignatureNotCorrectException(Signature, "TXOB", er.BaseStream.Position);
						Revision = er.ReadUInt32();
						NameOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
						Unknown2 = er.ReadUInt32();
						Unknown3 = er.ReadUInt32();
						LinkedTextureNameOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
						LinkedTextureOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();

						long curpos = er.BaseStream.Position;
						er.BaseStream.Position = NameOffset;
						Name = er.ReadStringNT(Encoding.ASCII);
						er.BaseStream.Position = LinkedTextureNameOffset;
						LinkedTextureName = er.ReadStringNT(Encoding.ASCII);
						er.BaseStream.Position = curpos;
					}
					public void Write(EndianBinaryWriter er, CGFXWriterContext c)
					{
						er.Write(Type);
						er.Write(Signature, Encoding.ASCII, false);
						er.Write(Revision);
						c.WriteStringReference(Name, er);
						er.Write(Unknown2);
						er.Write(Unknown3);
						c.WriteStringReference(LinkedTextureName, er);
						er.Write((uint)0);//TODO: Texture Offset
					}
					public UInt32 Type;
					public String Signature;
					public UInt32 Revision;
					public UInt32 NameOffset;
					public UInt32 Unknown2;
					public UInt32 Unknown3;
					public UInt32 LinkedTextureNameOffset;
					public UInt32 LinkedTextureOffset;

					public String Name;
					public String LinkedTextureName;

					public override string ToString()
					{
						return Name;
					}

					//The types are like this:
					//Image Texture = 0x20000011
					//Cube Texture = 0x20000009
					//Reference Texture = 0x20000004 (this structure)
					//Procedural Texture = 0x20000002
					//Shadow Texture = 0x20000021
				}*/

				public TextureSamplerCtr Sampler;
				public class TextureSamplerCtr
				{
					public TextureSamplerCtr(EndianBinaryReader er)
					{
						Type = er.ReadUInt32();
						OwnerOffset = (uint)(er.BaseStream.Position + er.ReadInt32());
						MinFilter = er.ReadUInt32();
					}
					public UInt32 Type;
					public UInt32 OwnerOffset;
					public UInt32 MinFilter;
					public static TextureSamplerCtr FromStream(EndianBinaryReader er)
					{
						UInt32 Type = er.ReadUInt32();
						er.BaseStream.Position -= 4;
						switch (Type)
						{
							case 0x80000000:
								return new StandardTextureSamplerCtr(er);
						}
						return new TextureSamplerCtr(er);
					}
					public virtual void Write(EndianBinaryWriter er, long OwnerOffset)
					{
						er.Write(Type);
						er.Write((int)(OwnerOffset - er.BaseStream.Position));
						er.Write(MinFilter);
					}
				}

				public class StandardTextureSamplerCtr : TextureSamplerCtr
				{
					public StandardTextureSamplerCtr(EndianBinaryReader er)
						: base(er)
					{
						BorderColor = er.ReadVector4();
						LodBias = er.ReadSingle();
					}
					public override void Write(EndianBinaryWriter er, long OwnerOffset)
					{
						base.Write(er, OwnerOffset);
						er.WriteVector4(BorderColor);
						er.Write(LodBias);
					}
					public Vector4 BorderColor;
					public Single LodBias;
				}
			}
			public SHDR Shader;
			public class SHDR
			{
				public SHDR(EndianBinaryReader er)
				{
					Type = er.ReadUInt32();
					Signature = er.ReadString(Encoding.ASCII, 4);
					if (Signature != "SHDR") throw new SignatureNotCorrectException(Signature, "SHDR", er.BaseStream.Position);
					Revision = er.ReadUInt32();
					NameOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
					Unknown2 = er.ReadUInt32();
					Unknown3 = er.ReadUInt32();
					LinkedShaderNameOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
					Unknown4 = er.ReadUInt32();


					long curpos = er.BaseStream.Position;
					er.BaseStream.Position = NameOffset;
					Name = er.ReadStringNT(Encoding.ASCII);
					er.BaseStream.Position = LinkedShaderNameOffset;
					LinkedShaderName = er.ReadStringNT(Encoding.ASCII);
					er.BaseStream.Position = curpos;
				}
				public void Write(EndianBinaryWriter er, CGFXWriterContext c)
				{
					er.Write(Type);
					er.Write(Signature, Encoding.ASCII, false);
					er.Write(Revision);
					c.WriteStringReference(Name, er);
					er.Write(Unknown2);
					er.Write(Unknown3);
					c.WriteStringReference(LinkedShaderName, er);
					er.Write(Unknown4);
				}
				public UInt32 Type;
				public String Signature;
				public UInt32 Revision;
				public UInt32 NameOffset;
				public UInt32 Unknown2;
				public UInt32 Unknown3;
				public UInt32 LinkedShaderNameOffset;
				public UInt32 Unknown4;

				public String Name;
				public String LinkedShaderName;

				public override string ToString()
				{
					return Name;
				}
			}

			public FragmentShader FragShader;
			public class FragmentShader
			{
				public FragmentShader(EndianBinaryReader er)
				{
					BufferColor = er.ReadVector4();
					FragmentLighting = new FragmentLightingCtr(er);
					FragmentLightingTableOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
					TextureCombiners = new TextureCombinerCtr[6];
					for (int i = 0; i < 6; i++) TextureCombiners[i] = new TextureCombinerCtr(er);
					AlphaTest = new AlphaTestCtr(er);
					BufferCommand1 = er.ReadUInt32();
					BufferCommand2 = er.ReadUInt32();
					BufferCommand3 = er.ReadUInt32();
					BufferCommand4 = er.ReadUInt32();
					BufferCommand5 = er.ReadUInt32();
					BufferCommand6 = er.ReadUInt32();

					long curpos = er.BaseStream.Position;
					er.BaseStream.Position = FragmentLightingTableOffset;
					FragmentLightingTable = new FragmentLightingTableCtr(er);
					er.BaseStream.Position = curpos;
				}
				public void Write(EndianBinaryWriter er, CGFXWriterContext c)
				{
					er.WriteVector4(BufferColor);
					FragmentLighting.Write(er);
					long tableoff = er.BaseStream.Position;
					er.Write((uint)0);
					for (int i = 0; i < 6; i++) TextureCombiners[i].Write(er);
					AlphaTest.Write(er);
					er.Write(BufferCommand1);
					er.Write(BufferCommand2);
					er.Write(BufferCommand3);
					er.Write(BufferCommand4);
					er.Write(BufferCommand5);
					er.Write(BufferCommand6);
					if (FragmentLightingTable != null)
					{
						long curpos = er.BaseStream.Position;
						er.BaseStream.Position = tableoff;
						er.Write((uint)(curpos - tableoff));
						er.BaseStream.Position = curpos;
						FragmentLightingTable.Write(er, c);
					}
				}
				public Vector4 BufferColor;//4 Singles
				public FragmentLightingCtr FragmentLighting;
				public UInt32 FragmentLightingTableOffset;
				public TextureCombinerCtr[] TextureCombiners;//6
				public AlphaTestCtr AlphaTest;
				public UInt32 BufferCommand1;
				public UInt32 BufferCommand2;
				public UInt32 BufferCommand3;
				public UInt32 BufferCommand4;
				public UInt32 BufferCommand5;
				public UInt32 BufferCommand6;

				public class FragmentLightingCtr
				{
					[Flags]
					public enum FragmentLightingFlags : uint
					{
						ClampHighLight = 1,
						UseDistribution0 = 2,
						UseDistribution1 = 4,
						UseGeometricFactor0 = 8,
						UseGeometricFactor1 = 16,
						UseReflection = 32
					}

					public FragmentLightingCtr(EndianBinaryReader er)
					{
						Flags = (FragmentLightingFlags)er.ReadUInt32();
						LayerConfig = er.ReadUInt32();
						FresnelConfig = er.ReadUInt32();
						BumpTextureIndex = er.ReadUInt32();
						BumpMode = er.ReadUInt32();
						IsBumpRenormalize = er.ReadUInt32() == 1;
					}

					public void Write(EndianBinaryWriter er)
					{
						er.Write((uint)Flags);
						er.Write(LayerConfig);
						er.Write(FresnelConfig);
						er.Write(BumpTextureIndex);
						er.Write(BumpMode);
						er.Write((uint)(IsBumpRenormalize ? 1 : 0));
					}

					public FragmentLightingFlags Flags;
					public UInt32 LayerConfig;
					public UInt32 FresnelConfig;
					public UInt32 BumpTextureIndex;
					public UInt32 BumpMode;
					public Boolean IsBumpRenormalize;
				}

				public FragmentLightingTableCtr FragmentLightingTable;
				public class FragmentLightingTableCtr
				{
					public FragmentLightingTableCtr(EndianBinaryReader er)
					{
						ReflectanceRSamplerOffset = er.ReadUInt32();
						if (ReflectanceRSamplerOffset != 0) ReflectanceRSamplerOffset += (UInt32)er.BaseStream.Position - 4;
						ReflectanceGSamplerOffset = er.ReadUInt32();
						if (ReflectanceGSamplerOffset != 0) ReflectanceGSamplerOffset += (UInt32)er.BaseStream.Position - 4;
						ReflectanceBSamplerOffset = er.ReadUInt32();
						if (ReflectanceBSamplerOffset != 0) ReflectanceBSamplerOffset += (UInt32)er.BaseStream.Position - 4;
						Distribution0SamplerOffset = er.ReadUInt32();
						if (Distribution0SamplerOffset != 0) Distribution0SamplerOffset += (UInt32)er.BaseStream.Position - 4;
						Distribution1SamplerOffset = er.ReadUInt32();
						if (Distribution1SamplerOffset != 0) Distribution1SamplerOffset += (UInt32)er.BaseStream.Position - 4;
						FresnelSamplerOffset = er.ReadUInt32();
						if (FresnelSamplerOffset != 0) FresnelSamplerOffset += (UInt32)er.BaseStream.Position - 4;

						if (ReflectanceRSamplerOffset != 0)
						{
							er.BaseStream.Position = ReflectanceRSamplerOffset;
							ReflectanceRSampler = new LightingLookupTableCtr(er);
						}
						if (ReflectanceGSamplerOffset != 0)
						{
							er.BaseStream.Position = ReflectanceGSamplerOffset;
							ReflectanceGSampler = new LightingLookupTableCtr(er);
						}
						if (ReflectanceBSamplerOffset != 0)
						{
							er.BaseStream.Position = ReflectanceBSamplerOffset;
							ReflectanceBSampler = new LightingLookupTableCtr(er);
						}
						if (Distribution0SamplerOffset != 0)
						{
							er.BaseStream.Position = Distribution0SamplerOffset;
							Distribution0Sampler = new LightingLookupTableCtr(er);
						}
						if (Distribution1SamplerOffset != 0)
						{
							er.BaseStream.Position = Distribution1SamplerOffset;
							Distribution1Sampler = new LightingLookupTableCtr(er);
						}
						if (FresnelSamplerOffset != 0)
						{
							er.BaseStream.Position = FresnelSamplerOffset;
							FresnelSampler = new LightingLookupTableCtr(er);
						}
					}
					public void Write(EndianBinaryWriter er, CGFXWriterContext c)
					{
						long offs = er.BaseStream.Position;
						er.Write((uint)0);
						er.Write((uint)0);
						er.Write((uint)0);
						er.Write((uint)0);
						er.Write((uint)0);
						er.Write((uint)0);
						if (ReflectanceRSampler != null)
						{
							long curpos = er.BaseStream.Position;
							er.BaseStream.Position = offs + 0;
							er.Write((uint)(curpos - (offs + 0)));
							er.BaseStream.Position = curpos;
							ReflectanceRSampler.Write(er, c);
						}
						if (ReflectanceGSampler != null)
						{
							long curpos = er.BaseStream.Position;
							er.BaseStream.Position = offs + 4;
							er.Write((uint)(curpos - (offs + 4)));
							er.BaseStream.Position = curpos;
							ReflectanceGSampler.Write(er, c);
						}
						if (ReflectanceBSampler != null)
						{
							long curpos = er.BaseStream.Position;
							er.BaseStream.Position = offs + 8;
							er.Write((uint)(curpos - (offs + 8)));
							er.BaseStream.Position = curpos;
							ReflectanceBSampler.Write(er, c);
						}
						if (Distribution0Sampler != null)
						{
							long curpos = er.BaseStream.Position;
							er.BaseStream.Position = offs + 12;
							er.Write((uint)(curpos - (offs + 12)));
							er.BaseStream.Position = curpos;
							Distribution0Sampler.Write(er, c);
						}
						if (Distribution1Sampler != null)
						{
							long curpos = er.BaseStream.Position;
							er.BaseStream.Position = offs + 16;
							er.Write((uint)(curpos - (offs + 16)));
							er.BaseStream.Position = curpos;
							Distribution1Sampler.Write(er, c);
						}
						if (FresnelSampler != null)
						{
							long curpos = er.BaseStream.Position;
							er.BaseStream.Position = offs + 20;
							er.Write((uint)(curpos - (offs + 20)));
							er.BaseStream.Position = curpos;
							FresnelSampler.Write(er, c);
						}
					}
					public UInt32 ReflectanceRSamplerOffset;
					public UInt32 ReflectanceGSamplerOffset;
					public UInt32 ReflectanceBSamplerOffset;
					public UInt32 Distribution0SamplerOffset;
					public UInt32 Distribution1SamplerOffset;
					public UInt32 FresnelSamplerOffset;

					public LightingLookupTableCtr ReflectanceRSampler;
					public LightingLookupTableCtr ReflectanceGSampler;
					public LightingLookupTableCtr ReflectanceBSampler;
					public LightingLookupTableCtr Distribution0Sampler;
					public LightingLookupTableCtr Distribution1Sampler;
					public LightingLookupTableCtr FresnelSampler;
					public class LightingLookupTableCtr
					{
						public LightingLookupTableCtr(EndianBinaryReader er)
						{
							InputCommand = er.ReadUInt32();
							ScaleCommand = er.ReadUInt32();
							SamplerOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();

							long curpos = er.BaseStream.Position;
							er.BaseStream.Position = SamplerOffset;
							Sampler = new ReferenceLookupTableCtr(er);
							er.BaseStream.Position = curpos;
						}
						public void Write(EndianBinaryWriter er, CGFXWriterContext c)
						{
							er.Write(InputCommand);
							er.Write(ScaleCommand);
							er.Write((uint)4);
							Sampler.Write(er, c);
						}
						public UInt32 InputCommand;
						public UInt32 ScaleCommand;
						public UInt32 SamplerOffset;

						public ReferenceLookupTableCtr Sampler;
						public class ReferenceLookupTableCtr
						{
							public ReferenceLookupTableCtr(EndianBinaryReader er)
							{
								Type = er.ReadUInt32();
								BinaryPathOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
								TableNameOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
								TargetLUTOffset = er.ReadUInt32();
								if (TargetLUTOffset != 0) TargetLUTOffset += (UInt32)er.BaseStream.Position - 4;

								long curpos = er.BaseStream.Position;
								er.BaseStream.Position = BinaryPathOffset;
								BinaryPath = er.ReadStringNT(Encoding.ASCII);
								er.BaseStream.Position = TableNameOffset;
								TableName = er.ReadStringNT(Encoding.ASCII);
								er.BaseStream.Position = curpos;
							}
							public void Write(EndianBinaryWriter er, CGFXWriterContext c)
							{
								er.Write(Type);
								c.WriteStringReference(BinaryPath, er);
								c.WriteStringReference(TableName, er);
								er.Write((uint)0);//TargetLUTOffset
							}
							public UInt32 Type;
							public UInt32 BinaryPathOffset;
							public UInt32 TableNameOffset;
							public UInt32 TargetLUTOffset;

							public String BinaryPath;
							public String TableName;
						}
					}
					public UInt32 GetHash()
					{
						List<byte> lresult = new List<byte>();
						if (ReflectanceRSampler != null)
						{
							lresult.AddRange(BitConverter.GetBytes(ReflectanceRSampler.InputCommand));
							lresult.AddRange(BitConverter.GetBytes(ReflectanceRSampler.ScaleCommand));
						}
						if (ReflectanceGSampler != null)
						{
							lresult.AddRange(BitConverter.GetBytes(ReflectanceGSampler.InputCommand));
							lresult.AddRange(BitConverter.GetBytes(ReflectanceGSampler.ScaleCommand));
						}
						if (ReflectanceBSampler != null)
						{
							lresult.AddRange(BitConverter.GetBytes(ReflectanceBSampler.InputCommand));
							lresult.AddRange(BitConverter.GetBytes(ReflectanceBSampler.ScaleCommand));
						}
						if (Distribution0Sampler != null)
						{
							lresult.AddRange(BitConverter.GetBytes(Distribution0Sampler.InputCommand));
							lresult.AddRange(BitConverter.GetBytes(Distribution0Sampler.ScaleCommand));
						}
						if (Distribution1Sampler != null)
						{
							lresult.AddRange(BitConverter.GetBytes(Distribution1Sampler.InputCommand));
							lresult.AddRange(BitConverter.GetBytes(Distribution1Sampler.ScaleCommand));
						}
						if (FresnelSampler != null)
						{
							lresult.AddRange(BitConverter.GetBytes(FresnelSampler.InputCommand));
							lresult.AddRange(BitConverter.GetBytes(FresnelSampler.ScaleCommand));
						}
						return CGFXWriterContext.CalcHash(lresult.ToArray());
					}
				}

				public class TextureCombinerCtr
				{
					public TextureCombinerCtr(EndianBinaryReader er)
					{
						Constant = er.ReadUInt32();
						SrcRgb = er.ReadUInt16();
						SrcAlpha = er.ReadUInt16();
						Address = er.ReadUInt32();
						Operands = er.ReadUInt32();
						CombineRgb = er.ReadUInt16();
						CombineAlpha = er.ReadUInt16();
						ConstRgba = er.ReadColor8();
						ScaleRgb = er.ReadUInt16();
						ScaleAlpha = er.ReadUInt16();
					}
					public void Write(EndianBinaryWriter er)
					{
						er.Write(Constant);
						er.Write(SrcRgb);
						er.Write(SrcAlpha);
						er.Write(Address);
						er.Write(Operands);
						er.Write(CombineRgb);
						er.Write(CombineAlpha);
						er.WriteColor8(ConstRgba);
						er.Write(ScaleRgb);
						er.Write(ScaleAlpha);
					}
					public UInt32 Constant;
					public UInt16 SrcRgb;
					public UInt16 SrcAlpha;
					public UInt32 Address;
					public UInt32 Operands;
					public UInt16 CombineRgb;
					public UInt16 CombineAlpha;
					public Color ConstRgba;
					public UInt16 ScaleRgb;
					public UInt16 ScaleAlpha;
				}

				public class AlphaTestCtr
				{
					public AlphaTestCtr(EndianBinaryReader er)
					{
						Command1 = er.ReadUInt32();
						Command2 = er.ReadUInt32();
					}
					public void Write(EndianBinaryWriter er)
					{
						er.Write(Command1);
						er.Write(Command2);
					}
					public UInt32 Command1;
					public UInt32 Command2;
					private readonly int[] GlAlphaFunc =
							{
								Gl.GL_NEVER,
								Gl.GL_ALWAYS,
								Gl.GL_EQUAL,
								Gl.GL_NOTEQUAL,
								Gl.GL_LESS,
								Gl.GL_LEQUAL,
								Gl.GL_GREATER,
								Gl.GL_GEQUAL
							};
					public void GlApply()
					{
						if ((Command1 & 1) == 1)
						{
							Gl.glEnable(Gl.GL_ALPHA_TEST);
							uint func = (Command1 >> 4) & 0xF;
							uint reference = Command1 >> 8;
							Gl.glAlphaFunc(GlAlphaFunc[func], (reference - 0.5f) / 255f);
							Gl.glEnable(Gl.GL_ALPHA_TEST);
						}
						else Gl.glDisable(Gl.GL_ALPHA_TEST);
					}

					private readonly uint[] ReverseFunc =
					{
						0, 1, 4, 7, 2, 3, 6, 5
					};
					public uint GetHash()
					{
						byte[] Result = new byte[9];
						Result[0] = (byte)(Command1 & 1);
						IOUtil.WriteU32LE(Result, 1, ReverseFunc[(Command1 >> 4) & 0xF]);
						IOUtil.WriteSingleLE(Result, 5, (uint)((Command1 >> 8) - 0.5f) / 255f);
						return CGFXWriterContext.CalcHash(Result);
					}
				}
			}

			public override string ToString()
			{
				return Name;
			}
		}

		public class Mesh
		{
			public Mesh(EndianBinaryReader er)
			{
				Type = er.ReadUInt32();
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "SOBJ") throw new SignatureNotCorrectException(Signature, "SOBJ", er.BaseStream.Position);
				Revision = er.ReadUInt32();
				NameOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
				Unknown2 = er.ReadUInt32();
				Unknown3 = er.ReadUInt32();
				ShapeIndex = er.ReadUInt32();
				MaterialIndex = er.ReadUInt32();
				OwnerModelOffset = (uint)(er.ReadInt32() + (UInt32)er.BaseStream.Position);
				IsVisible = er.ReadByte() == 1;
				RenderPriority = er.ReadByte();
				MeshNodeVisibilityIndex = er.ReadInt16();
				Unknown8 = er.ReadUInt32();
				Unknown9 = er.ReadUInt32();
				Unknown10 = er.ReadUInt32();
				Unknown11 = er.ReadUInt32();
				Unknown12 = er.ReadUInt32();
				Unknown13 = er.ReadUInt32();
				Unknown14 = er.ReadUInt32();
				Unknown15 = er.ReadUInt32();
				Unknown16 = er.ReadUInt32();
				Unknown17 = er.ReadUInt32();
				Unknown18 = er.ReadUInt32();
				Unknown19 = er.ReadUInt32();
				Unknown20 = er.ReadUInt32();
				Unknown21 = er.ReadUInt32();
				Unknown22 = er.ReadUInt32();
				Unknown23 = er.ReadUInt32();
				Unknown24 = er.ReadUInt32();
				Unknown25 = er.ReadUInt32();
				MeshNodeNameOffset = er.ReadUInt32();
				if (MeshNodeNameOffset != 0) MeshNodeNameOffset += (UInt32)er.BaseStream.Position - 4;
				Unknown27 = er.ReadUInt32();
				Unknown28 = er.ReadUInt32();
				Unknown29 = er.ReadUInt32();
				Unknown30 = er.ReadUInt32();
				//Unknown31 = er.ReadUInt32();

				long curpos = er.BaseStream.Position;
				er.BaseStream.Position = NameOffset;
				Name = er.ReadStringNT(Encoding.ASCII);
				er.BaseStream.Position = MeshNodeNameOffset;
				MeshNodeName = er.ReadStringNT(Encoding.ASCII);
				er.BaseStream.Position = curpos;
			}
			public void Write(EndianBinaryWriter er, CGFXWriterContext c, long OwnerOffset)
			{
				er.Write(Type);
				er.Write(Signature, Encoding.ASCII, false);
				er.Write(Revision);
				c.WriteStringReference(Name, er);
				er.Write(Unknown2);
				er.Write(Unknown3);
				er.Write(ShapeIndex);
				er.Write(MaterialIndex);
				er.Write((int)(OwnerOffset - er.BaseStream.Position));
				er.Write((byte)(IsVisible ? 1 : 0));
				er.Write(RenderPriority);
				er.Write(MeshNodeVisibilityIndex);
				er.Write(Unknown8);
				er.Write(Unknown9);
				er.Write(Unknown10);
				er.Write(Unknown11);
				er.Write(Unknown12);
				er.Write(Unknown13);
				er.Write(Unknown14);
				er.Write(Unknown15);
				er.Write(Unknown16);
				er.Write(Unknown17);
				er.Write(Unknown18);
				er.Write(Unknown19);
				er.Write(Unknown20);
				er.Write(Unknown21);
				er.Write(Unknown22);
				er.Write(Unknown23);
				er.Write(Unknown24);
				er.Write(Unknown25);
				c.WriteStringReference(MeshNodeName, er);
				er.Write(Unknown27);
				er.Write(Unknown28);
				er.Write(Unknown29);
				er.Write(Unknown30);
			}
			public UInt32 Type;
			public String Signature;
			public UInt32 Revision;
			public UInt32 NameOffset;
			public UInt32 Unknown2;//userdata nr entries
			public UInt32 Unknown3;//userdata dict offset
			public UInt32 ShapeIndex;
			public UInt32 MaterialIndex;
			public UInt32 OwnerModelOffset;
			public Boolean IsVisible;
			public Byte RenderPriority;
			public Int16 MeshNodeVisibilityIndex;
			public UInt32 Unknown8;
			public UInt32 Unknown9;
			public UInt32 Unknown10;
			public UInt32 Unknown11;
			public UInt32 Unknown12;//Might be a single
			public UInt32 Unknown13;
			public UInt32 Unknown14;
			public UInt32 Unknown15;//Might be a single
			public UInt32 Unknown16;//Might be a single
			public UInt32 Unknown17;
			public UInt32 Unknown18;
			public UInt32 Unknown19;
			public UInt32 Unknown20;//Might be a single
			public UInt32 Unknown21;
			public UInt32 Unknown22;
			public UInt32 Unknown23;
			public UInt32 Unknown24;
			public UInt32 Unknown25;
			public UInt32 MeshNodeNameOffset;
			public UInt32 Unknown27;
			public UInt32 Unknown28;
			public UInt32 Unknown29;
			public UInt32 Unknown30;
			//public UInt32 Unknown31;

			public String Name;
			public String MeshNodeName;

			public override string ToString()
			{
				return Name;
			}
		}

		public class SeparateDataShape
		{
			[Flags]
			public enum ShapeFlags : uint
			{
				HasBeenSetup = 1
			}
			public enum DataType : uint
			{
				GL_BYTE = 0x1400,
				GL_UNSIGNED_BYTE = 0x1401,
				GL_SHORT = 0x1402,//might also be unsigned short
				GL_FLOAT = 0x1406
			}
			public SeparateDataShape(EndianBinaryReader er)
			{
				Type = er.ReadUInt32();
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "SOBJ") throw new SignatureNotCorrectException(Signature, "SOBJ", er.BaseStream.Position);
				Revision = er.ReadUInt32();
				NameOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
				Unknown2 = er.ReadUInt32();
				Unknown3 = er.ReadUInt32();
				Flags = (ShapeFlags)er.ReadUInt32();
				OrientedBoundingBoxOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
				PositionOffset = er.ReadVector3();
				NrPrimitiveSets = er.ReadUInt32();
				PrimitiveSetOffsetsArrayOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
				BaseAddress = er.ReadUInt32();
				NrVertexAttributes = er.ReadUInt32();
				VertexAttributeOffsetsArrayOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
				BlendShapeOffset = er.ReadUInt32();
				if (BlendShapeOffset != 0) BlendShapeOffset += (UInt32)er.BaseStream.Position - 4;

				long curpos = er.BaseStream.Position;
				er.BaseStream.Position = NameOffset;
				Name = er.ReadStringNT(Encoding.ASCII);
				er.BaseStream.Position = OrientedBoundingBoxOffset;
				BoundingBox = BoundingVolume.FromStream(er);

				er.BaseStream.Position = PrimitiveSetOffsetsArrayOffset;
				PrimitiveSetOffsets = new uint[NrPrimitiveSets];
				for (int i = 0; i < NrPrimitiveSets; i++)
				{
					PrimitiveSetOffsets[i] = (UInt32)er.BaseStream.Position + er.ReadUInt32();
				}

				er.BaseStream.Position = VertexAttributeOffsetsArrayOffset;
				VertexAttributeOffsets = new uint[NrVertexAttributes];
				for (int i = 0; i < NrVertexAttributes; i++)
				{
					VertexAttributeOffsets[i] = (UInt32)er.BaseStream.Position + er.ReadUInt32();
				}

				PrimitiveSets = new PrimitiveSet[NrPrimitiveSets];
				for (int i = 0; i < NrPrimitiveSets; i++)
				{
					er.BaseStream.Position = PrimitiveSetOffsets[i];
					PrimitiveSets[i] = new PrimitiveSet(er);
				}

				VertexAttributes = new VertexAttributeCtr[NrVertexAttributes];
				if (NrVertexAttributes != 0)
				{
					for (int i = 0; i < NrVertexAttributes; i++)
					{
						er.BaseStream.Position = VertexAttributeOffsets[i];
						VertexAttributes[i] = VertexAttributeCtr.FromStream(er);
					}
				}

				if (BlendShapeOffset != 0)
				{
					er.BaseStream.Position = BlendShapeOffset;
					BlendShape = new BlendShapeCtr(er);
				}

				er.BaseStream.Position = curpos;
			}
			public void Write(EndianBinaryWriter er, CGFXWriterContext c)
			{
				er.Write(Type);
				er.Write(Signature, Encoding.ASCII, false);
				er.Write(Revision);
				c.WriteStringReference(Name, er);
				er.Write(Unknown2);
				er.Write(Unknown3);
				er.Write((uint)Flags);
				long bboffs = er.BaseStream.Position;
				er.Write((uint)0);
				er.WriteVector3(PositionOffset);
				er.Write(NrPrimitiveSets);
				long primsetoffsarrayoffs = er.BaseStream.Position;
				er.Write((uint)0);
				er.Write(BaseAddress);
				er.Write(NrVertexAttributes);
				long vtxattroffsarrayoffs = er.BaseStream.Position;
				er.Write((uint)0);
				long blendshapeoffs = er.BaseStream.Position;
				er.Write((uint)0);

				long primsetoffsarray = er.BaseStream.Position;
				er.BaseStream.Position = primsetoffsarrayoffs;
				er.Write((uint)(primsetoffsarray - primsetoffsarrayoffs));
				er.BaseStream.Position = primsetoffsarray;
				er.Write(new uint[NrPrimitiveSets], 0, (int)NrPrimitiveSets);

				long vtxattroffsarray = er.BaseStream.Position;
				er.BaseStream.Position = vtxattroffsarrayoffs;
				er.Write((uint)(vtxattroffsarray - vtxattroffsarrayoffs));
				er.BaseStream.Position = vtxattroffsarray;
				er.Write(new uint[NrVertexAttributes], 0, (int)NrVertexAttributes);

				long curpos = er.BaseStream.Position;
				er.BaseStream.Position = bboffs;
				er.Write((uint)(curpos - bboffs));
				er.BaseStream.Position = curpos;
				BoundingBox.Write(er);

				for (int i = 0; i < NrPrimitiveSets; i++)
				{
					curpos = er.BaseStream.Position;
					er.BaseStream.Position = primsetoffsarray + 4 * i;
					er.Write((uint)(curpos - (primsetoffsarray + 4 * i)));
					er.BaseStream.Position = curpos;
					PrimitiveSets[i].Write(er, c);
				}

				for (int i = 0; i < NrVertexAttributes; i++)
				{
					curpos = er.BaseStream.Position;
					er.BaseStream.Position = vtxattroffsarray + 4 * i;
					er.Write((uint)(curpos - (vtxattroffsarray + 4 * i)));
					er.BaseStream.Position = curpos;
					VertexAttributes[i].Write(er, c);
				}

				if (BlendShape != null)
				{
					curpos = er.BaseStream.Position;
					er.BaseStream.Position = blendshapeoffs;
					er.Write((uint)(curpos - blendshapeoffs));
					er.BaseStream.Position = curpos;
					BlendShape.Write(er);
				}
			}
			public UInt32 Type;
			public String Signature;
			public UInt32 Revision;
			public UInt32 NameOffset;
			public UInt32 Unknown2;//userdata nr entries //0x10
			public UInt32 Unknown3;//userdata dict offset
			public ShapeFlags Flags;
			public UInt32 OrientedBoundingBoxOffset;
			public Vector3 PositionOffset;//0x20
			public UInt32 NrPrimitiveSets;
			public UInt32 PrimitiveSetOffsetsArrayOffset;//0x30
			public UInt32 BaseAddress;
			public UInt32 NrVertexAttributes;
			public UInt32 VertexAttributeOffsetsArrayOffset;
			public UInt32 BlendShapeOffset;

			public UInt32[] PrimitiveSetOffsets;
			public UInt32[] VertexAttributeOffsets;

			public String Name;

			public BoundingVolume BoundingBox;

			public PrimitiveSet[] PrimitiveSets;
			public class PrimitiveSet
			{
				public PrimitiveSet(EndianBinaryReader er)
				{
					NrRelatedBones = er.ReadUInt32();
					RelatedBonesArrayOffset = er.ReadUInt32();
					if (RelatedBonesArrayOffset != 0) RelatedBonesArrayOffset += (UInt32)er.BaseStream.Position - 4;
					SkinningMode = er.ReadUInt32();
					NrPrimitives = er.ReadUInt32();
					PrimitiveOffsetsArrayOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();

					long curpos = er.BaseStream.Position;
					if (RelatedBonesArrayOffset != 0)
					{
						er.BaseStream.Position = RelatedBonesArrayOffset;
						RelatedBones = er.ReadUInt32s((int)NrRelatedBones);
					}
					er.BaseStream.Position = PrimitiveOffsetsArrayOffset;
					PrimitiveOffsets = new uint[NrPrimitives];
					for (int i = 0; i < NrPrimitives; i++)
					{
						PrimitiveOffsets[i] = (UInt32)er.BaseStream.Position + er.ReadUInt32();
					}
					Primitives = new Primitive[NrPrimitives];
					for (int i = 0; i < NrPrimitives; i++)
					{
						er.BaseStream.Position = PrimitiveOffsets[i];
						Primitives[i] = new Primitive(er);
					}
				}
				public void Write(EndianBinaryWriter er, CGFXWriterContext c)
				{
					er.Write(NrRelatedBones);
					if (NrRelatedBones != 0) er.Write((uint)0x10);
					else er.Write((uint)0);
					er.Write(SkinningMode);
					er.Write(NrPrimitives);
					if (NrRelatedBones != 0 && NrPrimitives != 0) er.Write((uint)(4 + NrRelatedBones * 4));
					else if (NrRelatedBones == 0 && NrPrimitives != 0) er.Write((uint)4);
					else er.Write((uint)0);

					er.Write(RelatedBones, 0, (int)NrRelatedBones);
					long primoffsarray = er.BaseStream.Position;
					er.Write(new uint[NrPrimitives], 0, (int)NrPrimitives);

					for (int i = 0; i < NrPrimitives; i++)
					{
						long curpos = er.BaseStream.Position;
						er.BaseStream.Position = primoffsarray + 4 * i;
						er.Write((uint)(curpos - (primoffsarray + 4 * i)));
						er.BaseStream.Position = curpos;
						Primitives[i].Write(er, c);
					}
				}
				public UInt32 NrRelatedBones;
				public UInt32 RelatedBonesArrayOffset;
				public UInt32 SkinningMode;
				public UInt32 NrPrimitives;
				public UInt32 PrimitiveOffsetsArrayOffset;
				//bones array
				//unknown1 array

				public UInt32[] RelatedBones;
				public UInt32[] PrimitiveOffsets;

				public Primitive[] Primitives;
				public class Primitive
				{
					public Primitive(EndianBinaryReader er)
					{
						NrIndexStreams = er.ReadUInt32();
						IndexStreamOffsetsArrayOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
						NrBufferObjects = er.ReadUInt32();
						BufferObjectArrayOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
						Flags = er.ReadUInt32();
						CommandAllocator = er.ReadUInt32();

						long curpos = er.BaseStream.Position;
						er.BaseStream.Position = IndexStreamOffsetsArrayOffset;
						IndexStreamOffsets = new uint[NrIndexStreams];
						for (int i = 0; i < NrIndexStreams; i++)
						{
							IndexStreamOffsets[i] = (UInt32)er.BaseStream.Position + er.ReadUInt32();
						}
						er.BaseStream.Position = BufferObjectArrayOffset;
						BufferObjects = er.ReadUInt32s((int)NrBufferObjects);

						IndexStreams = new IndexStreamCtr[NrIndexStreams];
						for (int i = 0; i < NrIndexStreams; i++)
						{
							er.BaseStream.Position = IndexStreamOffsets[i];
							IndexStreams[i] = new IndexStreamCtr(er);
						}
					}
					public void Write(EndianBinaryWriter er, CGFXWriterContext c)
					{
						er.Write(NrIndexStreams);
						if (NrIndexStreams != 0) er.Write((uint)0x14);
						else er.Write((uint)0);
						er.Write(NrBufferObjects);
						if (NrIndexStreams != 0 && NrBufferObjects != 0) er.Write((uint)(12 + NrIndexStreams * 4));
						else if (NrIndexStreams == 0 && NrBufferObjects != 0) er.Write((uint)12);
						else er.Write((uint)0);
						er.Write(Flags);
						er.Write(CommandAllocator);

						long idxstrmarray = er.BaseStream.Position;
						er.Write(new uint[NrIndexStreams], 0, (int)NrIndexStreams);
						er.Write(BufferObjects, 0, (int)NrBufferObjects);

						for (int i = 0; i < NrIndexStreams; i++)
						{
							long curpos = er.BaseStream.Position;
							er.BaseStream.Position = idxstrmarray + 4 * i;
							er.Write((uint)(curpos - (idxstrmarray + 4 * i)));
							er.BaseStream.Position = curpos;
							IndexStreams[i].Write(er, c);
						}
					}
					public UInt32 NrIndexStreams;
					public UInt32 IndexStreamOffsetsArrayOffset;
					public UInt32 NrBufferObjects;
					public UInt32 BufferObjectArrayOffset;
					public UInt32 Flags;
					public UInt32 CommandAllocator;
					//face info array
					//unknown 2 array

					public UInt32[] IndexStreamOffsets;
					public UInt32[] BufferObjects;

					public IndexStreamCtr[] IndexStreams;
					public class IndexStreamCtr
					{
						public IndexStreamCtr(EndianBinaryReader er)
						{
							FormatType = er.ReadUInt32();
							PrimitiveMode = er.ReadByte();
							IsVisible = er.ReadByte() == 1;
							er.ReadUInt16();//padding
							FaceDataLength = er.ReadUInt32();
							FaceDataOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
							BufferObject = er.ReadUInt32();
							LocationFlag = er.ReadUInt32();
							CommandCache = er.ReadUInt32();
							CommandCacheSize = er.ReadUInt32();
							LocationAddress = er.ReadUInt32();
							MemoryArea = er.ReadUInt32();
							BoundingBoxOffset = er.ReadUInt32();
							if (BoundingBoxOffset != 0) BoundingBoxOffset += (UInt32)er.BaseStream.Position - 4;

							long curpos = er.BaseStream.Position;
							er.BaseStream.Position = FaceDataOffset;
							FaceData = er.ReadBytes((int)FaceDataLength);
							if (BoundingBoxOffset != 0)
							{
								er.BaseStream.Position = BoundingBoxOffset;
								BoundingBox = BoundingVolume.FromStream(er);
							}
							er.BaseStream.Position = curpos;
						}
						public void Write(EndianBinaryWriter er, CGFXWriterContext c)
						{
							er.Write(FormatType);
							er.Write(PrimitiveMode);
							er.Write((byte)(IsVisible ? 1 : 0));
							er.Write((ushort)0);//padding
							er.Write(FaceDataLength);
							c.WriteDataReference(FaceData, er);
							er.Write(BufferObject);
							er.Write(LocationFlag);
							er.Write(CommandCache);
							er.Write(CommandCacheSize);
							er.Write(LocationAddress);
							er.Write(MemoryArea);
							if (BoundingBox != null)
							{
								er.Write((uint)4);
								BoundingBox.Write(er);
							}
							else er.Write((uint)0);
						}
						public UInt32 FormatType;
						public Byte PrimitiveMode;
						public Boolean IsVisible;
						public UInt32 FaceDataLength;
						public UInt32 FaceDataOffset;
						public UInt32 BufferObject;
						public UInt32 LocationFlag;
						public UInt32 CommandCache;
						public UInt32 CommandCacheSize;
						public UInt32 LocationAddress;
						public UInt32 MemoryArea;
						public UInt32 BoundingBoxOffset;

						public BoundingVolume BoundingBox;

						public byte[] FaceData;

						public Vector3[] GetFaceData()
						{
							Vector3[] res;
							int offset = 0;
							if (FormatType == 0x1403)
							{
								int count = (int)(FaceDataLength / 2 / 3);
								res = new Vector3[count];
								for (int i = 0; i < count; i++)
								{
									res[i] = new Vector3(IOUtil.ReadU16LE(FaceData, offset), IOUtil.ReadU16LE(FaceData, offset + 2), IOUtil.ReadU16LE(FaceData, offset + 4));
									offset += 6;
								}
							}
							else if (FormatType == 0x1401)
							{
								int count = (int)(FaceDataLength / 3);
								res = new Vector3[count];
								for (int i = 0; i < count; i++)
								{
									res[i] = new Vector3(FaceData[offset], FaceData[offset + 1], FaceData[offset + 2]);
									offset += 3;
								}
							}
							else
							{
								res = new Vector3[0];
								//res = null;
							}
							return res;
						}
					}
				}
			}
			public VertexAttributeCtr[] VertexAttributes;
			public class VertexAttributeCtr
			{
				public enum VertexAttributeUsage : uint
				{
					Position,
					Normal,
					Tangent,
					Color,
					TextureCoordinate0,
					TextureCoordinate1,
					TextureCoordinate2,
					BoneIndex,
					BoneWeight,
					UserAttribute0,
					UserAttribute1,
					UserAttribute2,
					UserAttribute3,
					UserAttribute4,
					UserAttribute5,
					UserAttribute6,
					UserAttribute7,
					UserAttribute8,
					UserAttribute9,
					UserAttribute10,
					UserAttribute11,
					Interleave,
					Quantity
				}
				[Flags]
				public enum VertexAttributeFlags : uint
				{
					VertexParam = 1,
					Interleave = 2
				}
				public VertexAttributeCtr(EndianBinaryReader er)
				{
					Type = er.ReadUInt32();
					Usage = (VertexAttributeUsage)er.ReadUInt32();
					Flags = (VertexAttributeFlags)er.ReadUInt32();
				}
				public virtual void Write(EndianBinaryWriter er, CGFXWriterContext c)
				{
					er.Write(Type);
					er.Write((uint)Usage);
					er.Write((uint)Flags);
				}
				public UInt32 Type;
				public VertexAttributeUsage Usage;
				public VertexAttributeFlags Flags;

				public static VertexAttributeCtr FromStream(EndianBinaryReader er)
				{
					UInt32 Type = er.ReadUInt32();
					er.BaseStream.Position -= 4;
					if (Type == 0x40000002) return new InterleavedVertexStreamCtr(er);
					else if (Type == 0x80000000) return new VertexParamAttributeCtr(er);
					else
					{
						return new VertexAttributeCtr(er);
					}
				}

				public virtual void GetVertexData(Polygon Destination, PrimitiveSet PrimitiveSet, CMDL Model)
				{

				}
			}
			public class InterleavedVertexStreamCtr : VertexAttributeCtr
			{
				public InterleavedVertexStreamCtr(EndianBinaryReader er)
					: base(er)
				{
					BufferObject = er.ReadUInt32();
					LocationFlag = er.ReadUInt32();
					VertexStreamLength = er.ReadUInt32();
					VertexStreamOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
					LocationAddress = er.ReadUInt32();//VertexStreamOffset seems to be used when this one is 0...
					MemoryArea = er.ReadUInt32();
					VertexDataEntrySize = er.ReadUInt32();
					NrVertexStreams = er.ReadUInt32();
					VertexStreamsOffsetArrayOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();

					long curpos = er.BaseStream.Position;
					er.BaseStream.Position = VertexStreamsOffsetArrayOffset;
					VertexStreamsOffsetArray = new uint[NrVertexStreams];
					for (int i = 0; i < NrVertexStreams; i++)
					{
						VertexStreamsOffsetArray[i] = (UInt32)er.BaseStream.Position + er.ReadUInt32();
					}

					er.BaseStream.Position = VertexStreamOffset;
					VertexStream = er.ReadBytes((int)VertexStreamLength);

					VertexStreams = new VertexStreamCtr[NrVertexStreams];
					for (int i = 0; i < NrVertexStreams; i++)
					{
						er.BaseStream.Position = VertexStreamsOffsetArray[i];
						VertexStreams[i] = new VertexStreamCtr(er);
					}
				}
				public override void Write(EndianBinaryWriter er, CGFXWriterContext c)
				{
					base.Write(er, c);
					er.Write(BufferObject);
					er.Write(LocationFlag);
					er.Write(VertexStreamLength);
					c.WriteDataReference(VertexStream, er);
					er.Write(LocationAddress);
					er.Write(MemoryArea);
					er.Write(VertexDataEntrySize);
					er.Write(NrVertexStreams);
					if (NrVertexStreams != 0) er.Write((uint)4);
					else er.Write((uint)0);

					long vtxstrmarray = er.BaseStream.Position;
					er.Write(new uint[NrVertexStreams], 0, (int)NrVertexStreams);

					for (int i = 0; i < NrVertexStreams; i++)
					{
						long curpos = er.BaseStream.Position;
						er.BaseStream.Position = vtxstrmarray + 4 * i;
						er.Write((uint)(curpos - (vtxstrmarray + 4 * i)));
						er.BaseStream.Position = curpos;
						VertexStreams[i].Write(er, c);
					}
				}
				public UInt32 BufferObject;
				public UInt32 LocationFlag;
				public UInt32 VertexStreamLength;
				public UInt32 VertexStreamOffset;
				public UInt32 LocationAddress;
				public UInt32 MemoryArea;
				public UInt32 VertexDataEntrySize;//Stride
				public UInt32 NrVertexStreams;//Nr Attributes
				public UInt32 VertexStreamsOffsetArrayOffset;

				public UInt32[] VertexStreamsOffsetArray;
				public byte[] VertexStream;
				public VertexStreamCtr[] VertexStreams;
				public class VertexStreamCtr : VertexAttributeCtr
				{
					public VertexStreamCtr(EndianBinaryReader er)
						: base(er)
					{
						BufferObject = er.ReadUInt32();
						LocationFlag = er.ReadUInt32();
						VertexStreamLength = er.ReadUInt32();
						VertexStreamOffset = er.ReadUInt32();
						LocationAddress = er.ReadUInt32();
						MemoryArea = er.ReadUInt32();
						FormatType = (DataType)er.ReadUInt32();
						NrComponents = er.ReadUInt32();
						Scale = er.ReadSingle();
						Offset = er.ReadUInt32();
					}
					public override void Write(EndianBinaryWriter er, CGFXWriterContext c)
					{
						base.Write(er, c);
						er.Write(BufferObject);
						er.Write(LocationFlag);
						er.Write(VertexStreamLength);
						//Not supported yet
						//if (BufferDataLength != 0) c.WriteDataReference(VertexStream, er);
						/*else*/
						er.Write((uint)0);
						er.Write(LocationAddress);
						er.Write(MemoryArea);
						er.Write((uint)FormatType);
						er.Write(NrComponents);
						er.Write(Scale);
						er.Write(Offset);
					}
					public UInt32 BufferObject;
					public UInt32 LocationFlag;//0x10
					public UInt32 VertexStreamLength;
					public UInt32 VertexStreamOffset;
					public UInt32 LocationAddress;
					public UInt32 MemoryArea;//0x20
					public DataType FormatType;
					public UInt32 NrComponents;//For example XYZ = 3, ST = 2, RGBA = 4
					public Single Scale;
					public UInt32 Offset;//0x30
				}

				public override void GetVertexData(Polygon Destination, PrimitiveSet PrimitiveSet, CMDL Model)
				{
					var p = Destination;
					int Vertices = (int)(VertexStreamLength / VertexDataEntrySize);
					int Offset = 0;
					byte[] VertexData = VertexStream;
					foreach (var v in VertexStreams)
					{
						switch (v.Usage)
						{
							case VertexAttributeUsage.Position: p.Vertex = new Vector3[Vertices]; break;
							case VertexAttributeUsage.Normal: p.Normals = new Vector3[Vertices]; break;
							//case VertexAttributeUsage.Tangent: break;
							case VertexAttributeUsage.Color: p.Colors = new Color[Vertices]; break;
							case VertexAttributeUsage.TextureCoordinate0: p.TexCoords = new Vector2[Vertices]; break;
							case VertexAttributeUsage.TextureCoordinate1: p.TexCoords2 = new Vector2[Vertices]; break;
							case VertexAttributeUsage.TextureCoordinate2: p.TexCoords3 = new Vector2[Vertices]; break;
							case VertexAttributeUsage.BoneIndex: break;
							case VertexAttributeUsage.BoneWeight: break;
							default:
								break;
						}
					}
					for (int i = 0; i < Vertices; i++)
					{
						byte[] bones = new byte[0];
						foreach (var v in VertexStreams)
						{
							float[] Vars = new float[v.NrComponents];
							int offs_ = 0;
							for (int q = 0; q < v.NrComponents; q++)
							{
								switch (v.FormatType)
								{
									case DataType.GL_BYTE:
										Vars[q] = (sbyte)VertexData[Offset + (int)v.Offset + offs_] * v.Scale;
										offs_++;
										break;
									case DataType.GL_UNSIGNED_BYTE:
										Vars[q] = VertexData[Offset + (int)v.Offset + offs_] * v.Scale;
										offs_++;
										break;
									case DataType.GL_SHORT:
										Vars[q] = IOUtil.ReadS16LE(VertexData, Offset + (int)v.Offset + offs_) * v.Scale;
										offs_ += 2;
										break;
									case DataType.GL_FLOAT:
										Vars[q] = BitConverter.ToSingle(VertexData, Offset + (int)v.Offset + offs_);
										offs_ += 4;
										break;
								}
							}
							switch (v.Usage)
							{
								case VertexAttributeUsage.Position:
									p.Vertex[i] = new Vector3(Vars[0], Vars[1], Vars[2]);
									if (PrimitiveSet.RelatedBones != null && PrimitiveSet.RelatedBones.Length == 1) p.Vertex[i] = p.Vertex[i] * Model.Skeleton.GetMatrix((int)PrimitiveSet.RelatedBones[0]);//SkeletonCtr.TransformByMtx(p.Vertex[i], Model.Skeleton.GetMatrix((int)PrimitiveSet.RelatedBones[0]));
									break;
								case VertexAttributeUsage.Normal:
									p.Normals[i] = new Vector3(Vars[0], Vars[1], Vars[2]);
									break;
								case VertexAttributeUsage.Tangent:
									{
										Vector3 unk = new Vector3(Vars[0], Vars[1], Vars[2]);
									}
									break;
								case VertexAttributeUsage.Color:
									if (Vars[0] < 0) Vars[0] = 0;
									if (Vars[0] > 1) Vars[0] = 1;
									if (Vars[1] < 0) Vars[1] = 0;
									if (Vars[1] > 1) Vars[1] = 1;
									if (Vars[2] < 0) Vars[2] = 0;
									if (Vars[2] > 1) Vars[2] = 1;
									if (Vars.Length > 3 && Vars[3] < 0) Vars[3] = 0;
									if (Vars.Length > 3 && Vars[3] > 1) Vars[3] = 1;
									if (v.FormatType == DataType.GL_BYTE || v.FormatType == DataType.GL_UNSIGNED_BYTE)
										if (Vars.Length > 3) p.Colors[i] = Color.FromArgb((int)(Vars[3] * 255), (int)(Vars[0] * 255), (int)(Vars[1] * 255), (int)(Vars[2] * 255));
										else p.Colors[i] = Color.FromArgb(255, (int)(Vars[0] * 255), (int)(Vars[1] * 255), (int)(Vars[2] * 255));
									else p.Colors[i] = Color.White;
									break;
								case VertexAttributeUsage.TextureCoordinate0:
									p.TexCoords[i] = new Vector2(Vars[0], Vars[1]);
									break;
								case VertexAttributeUsage.TextureCoordinate1:
									p.TexCoords2[i] = new Vector2(Vars[0], Vars[1]);
									break;
								case VertexAttributeUsage.TextureCoordinate2:
									p.TexCoords3[i] = new Vector2(Vars[0], Vars[1]);
									break;
								case VertexAttributeUsage.BoneIndex:
									{
										if (v.NrComponents == 1)
										{
											byte boneidx = VertexData[Offset + v.Offset];
											p.Vertex[i] = p.Vertex[i] * Model.Skeleton.GetMatrix((int)PrimitiveSet.RelatedBones[boneidx]); //SkeletonCtr.TransformByMtx(p.Vertex[i], Model.Skeleton.GetMatrix((int)PrimitiveSet.RelatedBones[boneidx]));//Vector3.Transform(p.Vertex[i], Model.Skeleton.GetMatrix((int)PrimitiveSets[0].RelatedBones[boneidx]));
										}
										else if (v.NrComponents == 2)
										{
											bones = new byte[v.NrComponents];
											for (int q = 0; q < v.NrComponents; q++)
											{
												bones[q] = VertexData[Offset + v.Offset + q];
											}
										}
										else
										{

										}
									}
									break;
								case VertexAttributeUsage.BoneWeight:
									{
										if (bones.Length != 0)
										{
											//this doesn't work correct!
											/*Vector3 dst = new Vector3(0, 0, 0);
											for (int j = 0; j < v.NrComponents; j++)
											{
												dst += p.Vertex[i] * Model.Skeleton.GetMatrix((int)PrimitiveSet.RelatedBones[bones[j]]);// SkeletonCtr.TransformByMtx(p.Vertex[i], Model.Skeleton.GetMatrix((int)PrimitiveSet.RelatedBones[bones[j]])) * Vars[j];//Add4(dst, Mult4(Model.Skeleton.GetMatrix((int)PrimitiveSet.RelatedBones[bones[j]]), ((float)weights[j] * v.Scale)));
											}*/
											//no transformations when no animation is used!
											//p.Vertex[i] = dst;
										}
										else
										{

										}
									}
									break;
								default:
									break;
							}
						}
						Offset += (int)VertexDataEntrySize;
					}
				}
			}
			public class VertexParamAttributeCtr : VertexAttributeCtr
			{
				public VertexParamAttributeCtr(EndianBinaryReader er)
					: base(er)
				{
					FormatType = (DataType)er.ReadUInt32();
					NrComponents = er.ReadUInt32();
					Scale = er.ReadSingle();
					NrAttributes = er.ReadUInt32();
					AttributeArrayOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
					long curpos = er.BaseStream.Position;
					er.BaseStream.Position = AttributeArrayOffset;
					Attributes = er.ReadSingles((int)NrAttributes);
					er.BaseStream.Position = curpos;
				}
				public override void Write(EndianBinaryWriter er, CGFXWriterContext c)
				{
					base.Write(er, c);
					er.Write((uint)FormatType);
					er.Write(NrComponents);
					er.Write(Scale);
					er.Write(NrAttributes);
					if (NrAttributes != 0) er.Write((uint)4);
					else er.Write((uint)0);
					er.Write(Attributes, 0, (int)NrAttributes);
				}
				public DataType FormatType;
				public UInt32 NrComponents;//For example XYZ = 3, ST = 2, RGBA = 4
				public Single Scale;
				public UInt32 NrAttributes;
				public UInt32 AttributeArrayOffset;

				public Single[] Attributes;

				public override void GetVertexData(Polygon Destination, PrimitiveSet PrimitiveSet, CMDL Model)
				{
					int count = Destination.Vertex.Length;
					switch (Usage)
					{
						//case VertexAttributeUsage.Position: Destination.Vertex = new Vector3[Vertices]; break;
						case VertexAttributeUsage.Tangent:
							break;
						case VertexAttributeUsage.Normal:
							Destination.Normals = new Vector3[count];
							for (int i = 0; i < count; i++)
							{
								Destination.Normals[i] = new Vector3(Attributes[0], Attributes[1], Attributes[2]);
							}
							break;
						//case VertexAttributeUsage.Tangent: break;
						case VertexAttributeUsage.Color:
							Destination.Colors = new Color[count];
							for (int i = 0; i < count; i++)
							{
								if (NrAttributes > 3) Destination.Colors[i] = Color.FromArgb((int)(Attributes[3] * 255), (int)(Attributes[0] * 255), (int)(Attributes[1] * 255), (int)(Attributes[2] * 255));
								else Destination.Colors[i] = Color.FromArgb(255, (int)(Attributes[0] * 255), (int)(Attributes[1] * 255), (int)(Attributes[2] * 255));
							}
							break;
						case VertexAttributeUsage.TextureCoordinate0:
							Destination.TexCoords = new Vector2[count];
							for (int i = 0; i < count; i++)
							{
								Destination.TexCoords[i] = new Vector2(Attributes[0], Attributes[1]);
							}
							break;
						case VertexAttributeUsage.TextureCoordinate1:
							Destination.TexCoords2 = new Vector2[count];
							for (int i = 0; i < count; i++)
							{
								Destination.TexCoords2[i] = new Vector2(Attributes[0], Attributes[1]);
							}
							break;
						case VertexAttributeUsage.TextureCoordinate2:
							Destination.TexCoords3 = new Vector2[count];
							for (int i = 0; i < count; i++)
							{
								Destination.TexCoords3[i] = new Vector2(Attributes[0], Attributes[1]);
							}
							break;
						case VertexAttributeUsage.BoneIndex:
							Matrix34 mtx = Model.Skeleton.GetMatrix((int)Attributes[0]);
							for (int i = 0; i < count; i++)
							{
								Destination.Vertex[i] *= mtx;
							}
							break;
						case VertexAttributeUsage.BoneWeight: break;//tmp
						default:
							break;
					}
				}
			}
			public BlendShapeCtr BlendShape;
			public class BlendShapeCtr
			{
				public BlendShapeCtr(EndianBinaryReader er)
				{
					Unknown1 = er.ReadUInt32();
					Unknown2 = er.ReadUInt32();
					Unknown3 = er.ReadUInt32();
					Unknown4 = er.ReadUInt32();
					Unknown5 = er.ReadUInt32();
				}
				public void Write(EndianBinaryWriter er)
				{
					er.Write(Unknown1);
					er.Write(Unknown2);
					er.Write(Unknown3);
					er.Write(Unknown4);
					er.Write(Unknown5);
				}
				public UInt32 Unknown1;
				public UInt32 Unknown2;
				public UInt32 Unknown3;
				public UInt32 Unknown4;
				public UInt32 Unknown5;
			}

			public Polygon GetVertexData(CMDL Model)
			{
				Polygon p = new Polygon();
				foreach (var v in VertexAttributes)
				{
					v.GetVertexData(p, PrimitiveSets[0], Model);
				}
				return p;
			}

			public override string ToString()
			{
				return Name;
			}
		}

		public class SkeletonCtr
		{
			public enum SkeletonScalingRule : uint
			{
				Standard = 0,
				Maya = 1,
				Softimage = 2
			}
			[Flags]
			public enum SkeletonFlags : uint
			{
				IsModelCoordinate = 1,
				IsTranslateAnimationEnabled = 2
			}

			public SkeletonCtr(EndianBinaryReader er)
			{
				Type = er.ReadUInt32();
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "SOBJ") throw new SignatureNotCorrectException(Signature, "SOBJ", er.BaseStream.Position);
				Revision = er.ReadUInt32();
				NameOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
				Unknown2 = er.ReadUInt32();
				Unknown3 = er.ReadUInt32();
				NrBones = er.ReadUInt32();
				BoneDictionaryOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
				RootBoneOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
				ScalingRule = (SkeletonScalingRule)er.ReadUInt32();
				Flags = (SkeletonFlags)er.ReadUInt32();

				long curpos = er.BaseStream.Position;
				er.BaseStream.Position = NameOffset;
				Name = er.ReadStringNT(Encoding.ASCII);
				er.BaseStream.Position = BoneDictionaryOffset;
				BoneDictionary = new DICT(er);
				Bones = new Bone[NrBones];
				for (int i = 0; i < NrBones; i++)
				{
					er.BaseStream.Position = BoneDictionary[i].DataOffset;
					Bones[i] = new Bone(er);
				}
				er.BaseStream.Position = curpos;
			}
			public UInt32 Type;
			public String Signature;
			public UInt32 Revision;
			public UInt32 NameOffset;
			public UInt32 Unknown2;
			public UInt32 Unknown3;
			public UInt32 NrBones;
			public UInt32 BoneDictionaryOffset;
			public UInt32 RootBoneOffset;
			public SkeletonScalingRule ScalingRule;
			public SkeletonFlags Flags;

			public String Name;

			public DICT BoneDictionary;
			public Bone[] Bones;
			public class Bone
			{
				[Flags]
				public enum BoneFlags : uint
				{
					IsIdentity = 1 << 0,
					IsTranslateZero = 1 << 1,
					IsRotateZero = 1 << 2,
					IsScaleOne = 1 << 3,
					IsUniformScale = 1 << 4,
					IsSegmentScaleCompensate = 1 << 5,
					IsNeedRendering = 1 << 6,
					IsLocalMatrixCalculate = 1 << 7,
					IsWorldMatrixCalculate = 1 << 8,
					HasSkinningMatrix = 1 << 9
				}

				public enum BBMode : uint
				{
					Off,
					World,
					WorldViewpoint,
					Screen,
					ScreenViewpoint,
					YAxial,
					YAxialViewpoint
				}
				public Bone(EndianBinaryReader er)
				{
					NameOffset = (UInt32)er.BaseStream.Position + er.ReadUInt32();
					Flags = (BoneFlags)er.ReadUInt32();
					JointID = er.ReadUInt32();
					ParentID = er.ReadInt32();
					if (ParentID == -1) ParentOffset = er.ReadUInt32();
					else ParentOffset = (UInt32)(er.BaseStream.Position + er.ReadInt32());
					ChildOffset = er.ReadUInt32();
					if (ChildOffset != 0) ChildOffset += (UInt32)(er.BaseStream.Position - 4);
					PreviousSiblingOffset = er.ReadUInt32();
					if (PreviousSiblingOffset != 0) PreviousSiblingOffset += (UInt32)(er.BaseStream.Position - 4);
					NextSiblingOffset = er.ReadUInt32();
					if (NextSiblingOffset != 0) NextSiblingOffset += (UInt32)(er.BaseStream.Position - 4);
					Scale = new Vector3(er.ReadSingle(), er.ReadSingle(), er.ReadSingle());
					Rotation = new Vector3(er.ReadSingle(), er.ReadSingle(), er.ReadSingle());
					Translation = new Vector3(er.ReadSingle(), er.ReadSingle(), er.ReadSingle());
					LocalMatrix = er.ReadSingles(4 * 3);
					WorldMatrix = er.ReadSingles(4 * 3);
					InverseBaseMatrix = er.ReadSingles(4 * 3);
					BillboardMode = (BBMode)er.ReadUInt32();
					Unknown6 = er.ReadUInt32();
					Unknown7 = er.ReadUInt32();

					long curpos = er.BaseStream.Position;
					er.BaseStream.Position = NameOffset;
					Name = er.ReadStringNT(Encoding.ASCII);
					er.BaseStream.Position = curpos;
				}
				public UInt32 NameOffset;
				public BoneFlags Flags;
				public UInt32 JointID;
				public Int32 ParentID;//-1 if no parent
				public UInt32 ParentOffset;//0 if no parent. Offset is signed and self relative
				public UInt32 ChildOffset;//0 if none
				public UInt32 PreviousSiblingOffset;//0 if none
				public UInt32 NextSiblingOffset;//0 if none
				public Vector3 Scale;
				public Vector3 Rotation;
				public Vector3 Translation;
				public Single[] LocalMatrix;//4x3
				public Single[] WorldMatrix;//4x3
				public Single[] InverseBaseMatrix;//4x3
				public BBMode BillboardMode;
				public UInt32 Unknown6;
				public UInt32 Unknown7;

				public String Name;

				public Matrix34 GetMatrix()
				{
					Matrix34 mtx = Matrix34.Identity;

					float RxSin = (float)Math.Sin(Rotation.X);
					float RxCos = (float)Math.Cos(Rotation.X);
					float RySin = (float)Math.Sin(Rotation.Y);
					float RyCos = (float)Math.Cos(Rotation.Y);
					float RzSin = (float)Math.Sin(Rotation.Z);
					float RzCos = (float)Math.Cos(Rotation.Z);
					mtx[2, 0] = -RySin;
					mtx[0, 0] = RzCos * RyCos;
					mtx[1, 0] = RzSin * RyCos;
					mtx[2, 1] = RyCos * RxSin;
					mtx[2, 2] = RyCos * RxCos;
					mtx[0, 1] = (RxSin * RzCos * RySin) - RxCos * RzSin;
					mtx[1, 2] = (RxCos * RzSin * RySin) - RxSin * RzCos;
					mtx[0, 2] = (RxCos * RzCos * RySin) + RxSin * RzSin;
					mtx[1, 1] = (RxSin * RzSin * RySin) + RxCos * RzCos;

					mtx.Row0 *= new Vector4(Scale, 1);
					mtx.Row1 *= new Vector4(Scale, 1);
					mtx.Row2 *= new Vector4(Scale, 1);

					mtx[0, 3] = Translation.X;
					mtx[1, 3] = Translation.Y;
					mtx[2, 3] = Translation.Z;
					return mtx;
				}

				public override string ToString()
				{
					return Name;
				}
			}

			public Matrix34 GetMatrix(int JointID)
			{
				int Joint = -1;
				for (int i = 0; i < NrBones; i++)
				{
					if (Bones[i].JointID == JointID) { Joint = i; break; }
				}
				if (Joint == -1) return Matrix34.Identity;
				Matrix34 curmtx = Bones[Joint].GetMatrix();
				Matrix34 parent = GetMatrix(Bones[Joint].ParentID);
				if ((Bones[Joint].Flags & Bone.BoneFlags.HasSkinningMatrix) != 0)
				{
					//Something special?
				}
				return curmtx * parent;
			}

			public override string ToString()
			{
				return Name;
			}
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
