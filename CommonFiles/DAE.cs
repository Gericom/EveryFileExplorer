using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Xml;
using System.IO;

using anyUri = System.String;
using boolean = System.Boolean;
using dateTime = System.DateTime;
using @float = System.Double;
using ID = System.String;
using IDREF = System.String;
using ListOfFloats = System.Collections.Generic.List<System.Double>;
using ListOfNames = System.Collections.Generic.List<System.String>;
using ListOfUints = System.Collections.Generic.List<System.UInt64>;
using NCName = System.String;
using NMTOKEN = System.String;
using token = System.String;
using @uint = System.UInt64;
using unsignedByte = System.Byte;
using unsignedInt = System.UInt32;
using URIFragmentType = System.String;
using VersionType = System.String;
using System.Drawing;
using System.Globalization;
using LibEveryFileExplorer.Files;
using System.Windows.Forms;

namespace CommonFiles
{
	public class DAE : FileFormat<DAE.DAEIdentifier>, IWriteable
	{
		public DAE()
		{
			Content = new COLLADA();
			Content.asset.contributor.Add(new DAE.asset._contributor() { authoring_tool = "Every File Explorer" });
		}

		public DAE(byte[] Data)
		{
			Content = COLLADA.FromByteArray(Data);
		}

		public String GetSaveDefaultFileFilter()
		{
			return "COLLADA DAE File (*.dae)|*.dae";
		}

		public byte[] Write()
		{
			return Content.Write();
		}

		public COLLADA Content;

		public class accessor
		{
			public accessor()
			{
				param = new List<param>();
			}
			[XmlAttribute]
			public @uint count;
			[XmlAttribute]
			[DefaultValue(0)]
			public @uint offset;
			[XmlAttribute]
			public anyUri source;
			[XmlAttribute]
			public @uint stride;

			[XmlElement("param")]
			public List<param> param;
		}
		public class asset
		{
			public asset()
			{
				contributor = new List<_contributor>();
				unit = new _unit("meter", 1);
			}
			[XmlElement("contributor")]
			public List<_contributor> contributor;
			public class _contributor
			{
				public string author;
				public string authoring_tool;
				public string comments;
				public string copyright;
				public anyUri source_data;
			}
			[XmlElement(IsNullable = false)]
			public dateTime created;
			public string keywords;
			[XmlElement(IsNullable = false)]
			public dateTime modified;
			public string revision;
			public string subject;
			public string title;
			public _unit unit;
			public class _unit
			{
				public _unit() { }
				public _unit(string name, float meter)
				{
					this.name = name;
					this.meter = meter;
				}
				[XmlAttribute]
				public @float meter;
				[XmlAttribute]
				public NMTOKEN name;
			}
			[DefaultValue(UpAxisType.Y_UP)]
			public UpAxisType up_axis = UpAxisType.Y_UP;
		}
		public class bind_material
		{
			public bind_material()
			{
				param = new List<param>();
				technique = new List<technique>();
				extra = new List<extra>();
			}
			[XmlElement("param")]
			public List<param> param;

			[XmlElement("technique_common")]
			public _technique_common technique_common;
			[XmlType("technique_commonA")]
			public class _technique_common
			{
				public _technique_common()
				{
					instance_material = new List<instance_material>();
				}
				[XmlElement("instance_material")]
				public List<instance_material> instance_material;
			}

			[XmlElement("technique")]
			public List<technique> technique;

			[XmlElement("extra")]
			public List<extra> extra;
		}
		[Serializable]
		//[XmlRoot(ElementName = "COLLADA", IsNullable = false)]
		public class COLLADA
		{
			public COLLADA()
			{
				asset = new asset();
				asset.created = DateTime.Now;
				extra = new List<extra>();
			}
			[XmlElement(IsNullable = false)]
			public asset asset;

			public library_controllers library_controllers;

			public library_geometries library_geometries;

			public library_effects library_effects;

			public library_images library_images;

			public library_materials library_materials;

			public library_visual_scenes library_visual_scenes;

			public _scene scene;
			public class _scene
			{
				public _scene()
				{
					instance_physics_scene = new List<InstanceWithExtra>();
					extra = new List<extra>();
				}
				[XmlElement("instance_physics_scene")]
				public List<InstanceWithExtra> instance_physics_scene;

				public InstanceWithExtra instance_visual_scene;

				[XmlElement("extra")]
				public List<extra> extra;
			}

			[XmlElement("extra")]
			public List<extra> extra;

			public byte[] Write()//string FileName)
			{
				this.asset.modified = DateTime.Now;

				//Create our own namespaces for the output
				XmlSerializerNamespaces ns = new XmlSerializerNamespaces();

				//Add an empty namespace and empty value
				ns.Add("", "");

				XmlSerializer SerializerObj = new XmlSerializer(typeof(COLLADA));


				MemoryStream mm = new MemoryStream();

				TextWriter WriteFileStream = new StreamWriter(mm);//FileName);
				SerializerObj.Serialize(WriteFileStream, this, ns);

				byte[] b = mm.ToArray();

				// Cleanup
				WriteFileStream.Close();

				XmlDocument d = new XmlDocument();
				d.Load(new MemoryStream(b));//FileName);
				d.DocumentElement.SetAttribute("xmlns", "http://www.collada.org/2005/11/COLLADASchema");
				d.DocumentElement.SetAttribute("version", "1.4.1");
				MemoryStream m = new MemoryStream();
				d.Save(m);
				b = m.ToArray();
				m.Close();
				return b;
				//d.Save(FileName);
			}
			public static COLLADA FromByteArray(byte[] Data)
			{
				XmlSerializer SerializerObj = new XmlSerializer(typeof(COLLADA));
				/*XmlDocument d = new XmlDocument();
				d.Load(FileName);
				d.DocumentElement.RemoveAllAttributes();
				d.RemoveChild(d.ChildNodes[0]);
				MemoryStream m = new MemoryStream();
				d.Save(m);*/
				String txt = Encoding.ASCII.GetString(Data); //File.ReadAllText(FileName);
				txt = txt.Replace("<COLLADA xmlns=\"http://www.collada.org/2005/11/COLLADASchema\" version=\"1.4.1\">", "<COLLADA>");
				txt = txt.Replace("<COLLADA version=\"1.4.1\" xmlns=\"http://www.collada.org/2008/03/COLLADASchema\">", "<COLLADA>");
				TextReader ReadFileStream = new StringReader(txt);//new StreamReader(FileName);
				ReadFileStream.ReadLine();
				object c = SerializerObj.Deserialize(ReadFileStream);
				ReadFileStream.Close();
				return (COLLADA)c;
			}
		}
		public class common_color_or_texture_type
		{
			public _color color;
			public class _color : IXmlSerializable
			{
				[XmlAttribute]
				public NCName sid;

				public Color content;

				public System.Xml.Schema.XmlSchema GetSchema()
				{
					throw new NotImplementedException();
				}

				public void ReadXml(XmlReader reader)
				{
					sid = reader.GetAttribute("sid");

					string inner = reader.ReadInnerXml();
					inner = inner.Replace(".", CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator);
					string[] s = inner.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

					//CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
					//ci.NumberFormat.CurrencyDecimalSeparator = ".";

					content = Color.FromArgb((int)(@float.Parse(s[3]) * 255), (int)(@float.Parse(s[0]) * 255), (int)(@float.Parse(s[1]) * 255), (int)(@float.Parse(s[2]) * 255));
				}

				public void WriteXml(XmlWriter writer)
				{
					if (sid != null && sid != "") writer.WriteAttributeString("sid", sid);

					string result = (content.R / 255f).ToString() + " " + (content.G / 255f).ToString() + " " + (content.B / 255f).ToString() + " " + (content.A / 255f).ToString();
					result = result.Replace(",", ".");
					writer.WriteRaw(result);
				}
			}
			public _param param;
			[XmlType("paramA")]
			public class _param
			{
				[XmlAttribute]
				public NCName @ref;
			}
			public _texture texture;
			public class _texture
			{
				[XmlAttribute]
				public NCName texture;
				[XmlAttribute]
				public NCName texcoord;

				public extra extra;
			}
		}
		public class common_float_or_param_type
		{
			public _float @float;
			public class _float : IXmlSerializable
			{
				[XmlAttribute]
				public NCName sid;

				public float content;

				public System.Xml.Schema.XmlSchema GetSchema()
				{
					throw new NotImplementedException();
				}

				public void ReadXml(XmlReader reader)
				{
					sid = reader.GetAttribute("sid");

					string inner = reader.ReadInnerXml();
					content = float.Parse(inner);
				}

				public void WriteXml(XmlWriter writer)
				{
					if (sid != null && sid != "") writer.WriteAttributeString("sid", sid);

					writer.WriteRaw(content.ToString());
				}
			}
			public _param param;
			[XmlType("paramB")]
			public class _param
			{
				[XmlAttribute]
				public NCName @ref;
			}
		}
		public class common_newparam_type
		{
			[XmlAttribute]
			public NCName sid;

			public NCName semantic;

			[XmlElement("float", typeof(@float))]
			[XmlElement("surface", typeof(fx_surface_common))]
			[XmlElement("sampler2D", typeof(fx_sampler2D_common))]
			public object choice;

			/*
			public @float @float;
			//float2
			//float3
			//float4
			public fx_surface_common surface;
			public fx_sampler2D_common sampler2D;
			*/
		}
		public class common_transparent_type : common_color_or_texture_type
		{
			[XmlAttribute]
			[DefaultValue(fx_opaque_enum.A_ONE)]
			public fx_opaque_enum opaque;
		}
		public class controller
		{
			[XmlAttribute]
			public ID id;
			[XmlAttribute]
			public NCName name;

			public asset asset;

			public skin skin;
			//morph

			[XmlElement("extra")]
			public List<extra> extra;
		}
		public class effect
		{
			public effect()
			{
				annotate = new List<fx_annotate_common>();
				image = new List<image>();
				newparam = new List<fx_newparam_common>();
				extra = new List<extra>();
			}
			[XmlAttribute]
			public ID id;
			[XmlAttribute]
			public NCName name;

			public asset asset;

			[XmlElement("annotate")]
			public List<fx_annotate_common> annotate;

			[XmlElement("image")]
			public List<image> image;

			[XmlElement("newparam")]
			public List<fx_newparam_common> newparam;

			//profile_GLSL
			public profile_COMMON profile_COMMON;
			//profile_CG
			//profile_GLES

			[XmlElement("extra")]
			public List<extra> extra;
		}
		public class extra
		{
			[XmlAttribute]
			public ID id;
			[XmlAttribute]
			public NCName name;
			[XmlAttribute]
			public NMTOKEN type;

			public asset asset;

			//TODO: How to implement this? (XmlAnyElement and xmlnode?)
		}
		public class float4x4 : IXmlSerializable
		{
			public float4x4()
			{
				content = new @float[4][];
				content[0] = new @float[4];
				content[1] = new @float[4];
				content[2] = new @float[4];
				content[3] = new @float[4];
				content[0][0] = 1;
				content[1][1] = 1;
				content[2][2] = 1;
				content[3][3] = 1;
			}

			public @float[][] content;

			public System.Xml.Schema.XmlSchema GetSchema()
			{
				throw new NotImplementedException();
			}

			public void ReadXml(XmlReader reader)
			{
				string inner = reader.ReadInnerXml();
				inner = inner.Replace(".", CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator);
				string[] s = inner.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				int i = 0;
				for (int y = 0; y < 4; y++)
				{
					for (int x = 0; x < 4; x++)
					{
						content[x][y] = @float.Parse(s[i++]);
					}
				}
			}

			public void WriteXml(XmlWriter writer)
			{
				string result = "";
				for (int y = 0; y < 4; y++)
				{
					for (int x = 0; x < 4; x++)
					{
						result += content[x][y] + " ";
					}
				}
				result.Trim();
				result = result.Replace(",", ".");
				writer.WriteRaw(result);
			}
		}
		public class fx_annotate_common
		{
			[XmlAttribute]
			public NCName name;

			[XmlElement("float", typeof(@float))]
			[XmlElement("string", typeof(string))]
			public object choice;

			/*
			//bool
			//bool2
			//bool3
			//bool4
			//int
			//int2
			//int3
			//int4
			public @float @float;
			//float2
			//float3
			//float4
			//float2x2
			//float3x3
			//float4x4
			public string @string;
			*/
		}
		public class fx_newparam_common
		{
			public fx_newparam_common()
			{
				annotate = new List<fx_annotate_common>();
			}
			[XmlAttribute]
			public NCName sid;

			[XmlElement("annotate")]
			public List<fx_annotate_common> annotate;

			public NCName semantic;

			public fx_modifier_enum_common modifier;

			[XmlElement("float", typeof(@float))]
			[XmlElement("surface", typeof(fx_surface_common))]
			[XmlElement("sampler2D", typeof(fx_sampler2D_common))]
			public object choice;

			/*
			//bool
			//bool2
			//bool3
			//bool4
			//int
			//int2
			//int3
			//int4
			public @float @float;
			//float2
			//float3
			//float4
			//float1x1
			//float1x2
			//float1x3
			//float1x4
			//float2x1
			//float2x2
			//float2x3
			//float2x4
			//float3x1
			//float3x2
			//float3x3
			//float3x4
			//float4x1
			//float4x2
			//float4x3
			//float4x4
			public fx_surface_common surface;
			//sampler1D
			public fx_sampler2D_common sampler2D;
			//sampler3D
			//samplerCUBE
			//samplerRECT
			//samplerDEPTH
			//enum
			*/
		}
		public class fx_sampler2D_common
		{
			public fx_sampler2D_common()
			{
				extra = new List<extra>();
			}
			public NCName source;
			[DefaultValue(fx_sampler_wrap_common.WRAP)]
			public fx_sampler_wrap_common wrap_s = fx_sampler_wrap_common.WRAP;
			[DefaultValue(fx_sampler_wrap_common.WRAP)]
			public fx_sampler_wrap_common wrap_t = fx_sampler_wrap_common.WRAP;
			[DefaultValue(fx_sampler_filter_common.NONE)]
			public fx_sampler_filter_common minfilter;
			[DefaultValue(fx_sampler_filter_common.NONE)]
			public fx_sampler_filter_common magfilter;
			[DefaultValue(fx_sampler_filter_common.NONE)]
			public fx_sampler_filter_common mipfilter;
			//border_color
			[DefaultValue(255)]
			public unsignedByte mipmap_maxlevel = 255;
			[DefaultValue(0)]
			public float mipmap_bias;

			[XmlElement("extra")]
			public List<extra> extra;
		}
		public class fx_surface_common
		{
			public fx_surface_common()
			{
				init_from = new List<fx_surface_init_from_common>();
				extra = new List<extra>();
			}
			[XmlAttribute]
			public fx_surface_type_enum type;

			//init_as_null
			//init_as_target
			//init_cube
			//init_volume
			//init_planar
			[XmlElement("init_from")]
			public List<fx_surface_init_from_common> init_from;

			public token format;
			public fx_surface_common format_hint;

			//size
			//viewport_ratio

			[DefaultValue(0)]
			public unsignedInt mip_levels;
			[DefaultValue(false)]
			public boolean mipmap_generate;

			[XmlElement("extra")]
			public List<extra> extra;
		}
		public class fx_surface_format_hint_common
		{
			public fx_surface_format_hint_common()
			{
				extra = new List<extra>();
			}
			public fx_surface_format_hint_channels_enum channels;
			public fx_surface_format_hint_range_enum range;
			public fx_surface_format_hint_precision_enum precision;
			public fx_surface_format_hint_option_enum option;

			[XmlElement("extra")]
			public List<extra> extra;
		}
		public class fx_surface_init_from_common
		{
			[XmlAttribute]
			[DefaultValue(0)]
			public unsignedInt mip;
			[XmlAttribute]
			[DefaultValue(0)]
			public unsignedInt slice;
			[XmlAttribute]
			[DefaultValue(fx_surface_face_enum.POSITIVE_X)]
			public fx_surface_face_enum face;

			[XmlText]
			public IDREF content;
		}
		public class geometry
		{
			public geometry()
			{
				extra = new List<extra>();
			}
			[XmlAttribute]
			public ID id;
			[XmlAttribute]
			public NCName name;

			public asset asset;

			//convex_mesh
			public mesh mesh;
			//spline

			[XmlElement("extra")]
			public List<extra> extra;
		}
		public class image
		{
			public image()
			{
				extra = new List<extra>();
			}
			[XmlAttribute]
			public ID id;
			[XmlAttribute]
			public NCName name;
			[XmlAttribute]
			public token format;
			[XmlAttribute]
			[DefaultValue(0)]
			public @uint height;
			[XmlAttribute]
			[DefaultValue(0)]
			public @uint width;
			[XmlAttribute]
			[DefaultValue(0)]
			public @uint depth;

			public asset asset;

			//data - ListOfHexBinary
			public anyUri init_from;

			[XmlElement("extra")]
			public List<extra> extra;
		}
		public class InputLocal
		{
			[XmlAttribute]
			public NMTOKEN semantic;
			[XmlAttribute]
			public URIFragmentType source;
		}
		public class InputLocalOffset
		{
			[XmlAttribute]
			public @uint offset;
			[XmlAttribute]
			public NMTOKEN semantic;
			[XmlAttribute]
			public URIFragmentType source;
			[XmlAttribute]
			[DefaultValue(0)]
			public @uint set;
		}
		public class instance_controller
		{
			public instance_controller()
			{
				skeleton = new List<anyUri>();
				extra = new List<extra>();
			}
			[XmlAttribute]
			public anyUri url;
			[XmlAttribute]
			public NCName sid;
			[XmlAttribute]
			public NCName name;

			[XmlElement("skeleton")]
			public List<anyUri> skeleton;

			public bind_material bind_material;

			[XmlElement("extra")]
			public List<extra> extra;
		}
		public class instance_effect
		{
			[XmlAttribute]
			public anyUri url;
			[XmlAttribute]
			public NCName sid;
			[XmlAttribute]
			public NCName name;

			[XmlElement("technique_hint")]
			public List<_technique_hint> technique_hint;
			public class _technique_hint
			{
				[XmlAttribute]
				public NCName platform;
				[XmlAttribute]
				public NCName profile;
				[XmlAttribute]
				public NCName @ref;
			}

			[XmlElement("setparam")]
			public List<_setparam> setparam;
			public class _setparam
			{
				public token @ref;
				//fx_basic_type_common
			}

			[XmlElement("extra")]
			public List<extra> extra;
		}
		public class instance_geometry
		{
			public instance_geometry()
			{
				extra = new List<extra>();
			}
			[XmlAttribute]
			public anyUri url;//links to geometry like this: #[Id of geometry object here]
			[XmlAttribute]
			public NCName sid;
			[XmlAttribute]
			public NCName name;

			public bind_material bind_material;

			[XmlElement("extra")]
			public List<extra> extra;
		}
		public class instance_material
		{
			public instance_material()
			{
				bind = new List<_bind>();
				bind_vertex_input = new List<_bind_vertex_input>();
				extra = new List<extra>();
			}
			[XmlAttribute]
			public NCName symbol;
			[XmlAttribute]
			public anyUri target;
			[XmlAttribute]
			public NCName sid;
			[XmlAttribute]
			public NCName name;

			[XmlElement("bind")]
			public List<_bind> bind;
			public class _bind
			{
				[XmlAttribute]
				public NCName semantic;
				[XmlAttribute]
				public token target;
			}

			[XmlElement("bind_vertex_input")]
			public List<_bind_vertex_input> bind_vertex_input;
			public class _bind_vertex_input
			{
				[XmlAttribute]
				public NCName semantic;
				[XmlAttribute]
				public NCName input_semantic;
				[XmlAttribute]
				public @uint input_set;
			}

			[XmlElement("extra")]
			public List<extra> extra;
		}
		public class InstanceWithExtra
		{
			public InstanceWithExtra()
			{
				extra = new List<extra>();
			}
			[XmlAttribute]
			public anyUri url;
			[XmlAttribute]
			public NCName sid;
			[XmlAttribute]
			public NCName name;

			[XmlElement("extra")]
			public List<extra> extra;
		}
		public class library_controllers
		{
			public library_controllers()
			{
				controller = new List<controller>();
				extra = new List<extra>();
			}
			[XmlAttribute]
			public ID id;
			[XmlAttribute]
			public NCName name;

			public asset asset;

			[XmlElement("controller")]
			public List<controller> controller;

			[XmlElement("extra")]
			public List<extra> extra;
		}
		public class library_effects
		{
			public library_effects()
			{
				effect = new List<effect>();
				extra = new List<extra>();
			}
			[XmlAttribute]
			public ID id;
			[XmlAttribute]
			public NCName name;

			public asset asset;

			[XmlElement("effect")]
			public List<effect> effect;

			[XmlElement("extra")]
			public List<extra> extra;
		}
		public class library_geometries
		{
			public library_geometries()
			{
				geometry = new List<geometry>();
				extra = new List<extra>();
			}
			[XmlAttribute]
			public ID id;
			[XmlAttribute]
			public NCName name;

			public asset asset;

			[XmlElement("geometry")]
			public List<geometry> geometry;

			[XmlElement("extra")]
			public List<extra> extra;
		}
		public class library_images
		{
			public library_images()
			{
				image = new List<image>();
				extra = new List<extra>();
			}
			[XmlAttribute]
			public ID id;
			[XmlAttribute]
			public NCName name;

			public asset asset;

			[XmlElement("image")]
			public List<image> image;

			[XmlElement("extra")]
			public List<extra> extra;
		}
		public class library_materials
		{
			public library_materials()
			{
				material = new List<material>();
				extra = new List<extra>();
			}
			[XmlAttribute]
			public ID id;
			[XmlAttribute]
			public NCName name;

			public asset asset;

			[XmlElement("material")]
			public List<material> material;

			[XmlElement("extra")]
			public List<extra> extra;
		}
		public class library_visual_scenes
		{
			public library_visual_scenes()
			{
				visual_scene = new List<visual_scene>();
				extra = new List<extra>();
			}
			[XmlAttribute]
			public ID id;
			[XmlAttribute]
			public NCName name;

			public asset asset;

			[XmlElement("visual_scene")]
			public List<visual_scene> visual_scene;

			[XmlElement("extra")]
			public List<extra> extra;
		}
		public class material
		{
			public material()
			{
				extra = new List<extra>();
			}
			[XmlAttribute]
			public ID id;
			[XmlAttribute]
			public NCName name;

			public asset asset;

			public instance_effect instance_effect;

			[XmlElement("extra")]
			public List<extra> extra;
		}
		public class matrix : IXmlSerializable
		{
			public matrix()
			{
				content = new @float[4][];
				content[0] = new @float[4];
				content[1] = new @float[4];
				content[2] = new @float[4];
				content[3] = new @float[4];
				content[0][0] = 1;
				content[1][1] = 1;
				content[2][2] = 1;
				content[3][3] = 1;
			}
			[XmlAttribute]
			public NCName sid;

			public @float[][] content;

			public System.Xml.Schema.XmlSchema GetSchema()
			{
				throw new NotImplementedException();
			}

			public void ReadXml(XmlReader reader)
			{
				sid = reader.GetAttribute("sid");

				string inner = reader.ReadInnerXml();
				inner = inner.Replace(".", CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator);
				string[] s = inner.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				int i = 0;
				for (int y = 0; y < 4; y++)
				{
					for (int x = 0; x < 4; x++)
					{
						content[x][y] = @float.Parse(s[i++]);
					}
				}
			}

			public void WriteXml(XmlWriter writer)
			{
				if (sid != null && sid != "") writer.WriteAttributeString("sid", sid);

				string result = "";
				for (int y = 0; y < 4; y++)
				{
					for (int x = 0; x < 4; x++)
					{
						result += content[x][y] + " ";
					}
				}
				result.Trim();
				result = result.Replace(",", ".");
				writer.WriteRaw(result);
			}
		}
		public class mesh
		{
			public mesh()
			{
				source = new List<source>();
				triangles = new List<triangles>();
				extra = new List<extra>();
			}
			[XmlElement("source")]
			public List<source> source;

			public vertices vertices;

			//lines
			//linestrips
			//polygons
			//polylist
			[XmlElement("triangles")]
			public List<triangles> triangles;
			//trifans
			//tristrips

			[XmlElement("extra")]
			public List<extra> extra;
		}
		public class node
		{
			public node()
			{
				instance_geometry = new List<DAE.instance_geometry>();
				_node = new List<node>();
				extra = new List<extra>();
			}
			public node(string name)
				: this()
			{
				this.name = name;
			}
			[XmlAttribute]
			public ID id;
			[XmlAttribute]
			public NCName name;
			[XmlAttribute]
			public NCName sid;
			[XmlAttribute]
			[DefaultValue(NodeType.NODE)]
			public NodeType type = NodeType.NODE;
			[XmlAttribute]
			[DefaultValue(null)]
			public ListOfNames layer;

			public asset asset;

			//choice (unbounded):
			//	lookat
			public matrix matrix;
			//	rotate
			//	scale
			//	skew
			//	translate

			//instance_camera
			[XmlElement("instance_controller")]
			public List<instance_controller> instance_controller;
			[XmlElement("instance_geometry")]
			public List<instance_geometry> instance_geometry;
			//instance_light
			//instance_node

			[XmlElement("node")]
			public List<node> _node;

			[XmlElement("extra")]
			public List<extra> extra;
		}
		public class p : IXmlSerializable
		{
			public p()
			{
				content = new ListOfUints();
			}

			public ListOfUints content;

			public System.Xml.Schema.XmlSchema GetSchema()
			{
				throw new NotImplementedException();
			}

			public void ReadXml(XmlReader reader)
			{
				string inner = reader.ReadInnerXml();
				string[] s = inner.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string ss in s)
				{
					content.Add(@uint.Parse(ss));
				}
			}

			public void WriteXml(XmlWriter writer)
			{
				string result = "";
				foreach (@uint ss in content)
				{
					result += ss + " ";
				}
				result.Trim();
				writer.WriteRaw(result);
			}
		}
		public class param
		{
			[XmlAttribute]
			public NCName name;
			[XmlAttribute]
			public NCName sid;
			[XmlAttribute]
			public NMTOKEN semantic;
			[XmlAttribute]
			public NMTOKEN type;

			[XmlText]
			public string content;
		}
		public class profile_COMMON
		{
			public profile_COMMON()
			{
				extra = new List<extra>();
			}
			[XmlAttribute]
			public ID id;

			public asset asset;

			[XmlElement("image")]
			public List<image> image;
			[XmlElement("newparam")]
			public List<common_newparam_type> newparam;

			public _technique technique;
			public class _technique
			{
				public _technique()
				{
					extra = new List<extra>();
				}
				[XmlAttribute]
				public ID id;
				[XmlAttribute]
				public NCName sid;

				public asset asset;

				public image image;
				public common_newparam_type newparam;

				//constant
				public _lambert lambert;
				public class _lambert
				{
					public common_color_or_texture_type emission;
					public common_color_or_texture_type ambient;
					public common_color_or_texture_type diffuse;
					public common_color_or_texture_type reflective;
					public common_float_or_param_type reflectivity;
					public common_transparent_type transparent;
					public common_float_or_param_type transparency;
					public common_float_or_param_type index_of_refraction;
				}
				public _phong phong;
				public class _phong
				{
					public common_color_or_texture_type emission;
					public common_color_or_texture_type ambient;
					public common_color_or_texture_type diffuse;
					public common_color_or_texture_type specular;
					public common_float_or_param_type shininess;
					public common_color_or_texture_type reflective;
					public common_float_or_param_type reflectivity;
					public common_transparent_type transparent;
					public common_float_or_param_type transparency;
					public common_float_or_param_type index_of_refraction;
				}
				//blinn

				[XmlElement("extra")]
				public List<extra> extra;
			}

			[XmlElement("extra")]
			public List<extra> extra;
		}
		public class skin
		{
			public skin()
			{
				_source = new List<source>();
				extra = new List<extra>();
			}
			[XmlAttribute]
			public anyUri source;

			public float4x4 bind_shape_matrix;

			[XmlElement("source")]
			public List<source> _source;

			public _joints joints;
			public class _joints
			{
				public _joints()
				{
					input = new List<InputLocal>();
					extra = new List<extra>();
				}
				[XmlElement("input")]
				public List<InputLocal> input;

				[XmlElement("extra")]
				public List<extra> extra;
			}
			public _vertex_weights vertex_weights;
			public class _vertex_weights
			{
				public _vertex_weights()
				{
					input = new List<InputLocalOffset>();
					extra = new List<extra>();
				}
				[XmlAttribute]
				public @uint count;

				[XmlElement("input")]
				public List<InputLocalOffset> input;

				public _vcount vount;
				public class _vcount
				{
					public _vcount()
					{
						content = new ListOfUints();
					}

					public ListOfUints content;

					public System.Xml.Schema.XmlSchema GetSchema()
					{
						throw new NotImplementedException();
					}

					public void ReadXml(XmlReader reader)
					{
						string inner = reader.ReadInnerXml();
						string[] s = inner.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
						foreach (string ss in s)
						{
							content.Add(@uint.Parse(ss));
						}
					}

					public void WriteXml(XmlWriter writer)
					{
						string result = "";
						foreach (@uint ss in content)
						{
							result += ss + " ";
						}
						result.Trim();
						writer.WriteRaw(result);
					}
				}
				public _v v;
				public class _v
				{
					public _v()
					{
						content = new ListOfUints();
					}

					public ListOfUints content;

					public System.Xml.Schema.XmlSchema GetSchema()
					{
						throw new NotImplementedException();
					}

					public void ReadXml(XmlReader reader)
					{
						string inner = reader.ReadInnerXml();
						string[] s = inner.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
						foreach (string ss in s)
						{
							content.Add(@uint.Parse(ss));
						}
					}

					public void WriteXml(XmlWriter writer)
					{
						string result = "";
						foreach (@uint ss in content)
						{
							result += ss + " ";
						}
						result.Trim();
						writer.WriteRaw(result);
					}
				}
				[XmlElement("extra")]
				public List<extra> extra;
			}
			[XmlElement("extra")]
			public List<extra> extra;
		}
		public class source
		{
			public source()
			{
				technique = new List<technique>();
			}
			[XmlAttribute]
			public ID id;
			[XmlAttribute]
			public NCName name;

			public asset asset;

			//IDREF_array

			public _Name_array Name_array;
			public class _Name_array : IXmlSerializable
			{
				public _Name_array()
				{
					content = new ListOfNames();
				}
				[XmlAttribute]
				public ID id;
				[XmlAttribute]
				public NCName name;
				[XmlAttribute]
				public @uint count;

				public ListOfNames content;

				public System.Xml.Schema.XmlSchema GetSchema()
				{
					throw new NotImplementedException();
				}

				public void ReadXml(XmlReader reader)
				{
					id = reader.GetAttribute("id");
					name = reader.GetAttribute("name");
					string q = reader.GetAttribute("count");
					if (q != null) count = @uint.Parse(q);

					string inner = reader.ReadInnerXml();
					string[] s = inner.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
					content.AddRange(s);
				}

				public void WriteXml(XmlWriter writer)
				{
					if (id != null && id != "") writer.WriteAttributeString("id", id);
					if (name != null && name != "") writer.WriteAttributeString("name", name);
					writer.WriteAttributeString("count", count.ToString());

					if (content.Count != 0)
					{
						string result = "";
						foreach (string ss in content)
						{
							result += ss + " ";
						}
						result.Trim();
						writer.WriteRaw(result);
					}
				}
			}

			//bool_array

			public _float_array float_array;
			public class _float_array : IXmlSerializable
			{
				public _float_array()
				{
					content = new ListOfFloats();
				}
				[XmlAttribute]
				public ID id;
				[XmlAttribute]
				public NCName name;
				[XmlAttribute]
				public @uint count;
				[XmlAttribute]
				public short digits;
				[XmlAttribute]
				public short magnitude;

				public ListOfFloats content;

				public System.Xml.Schema.XmlSchema GetSchema()
				{
					throw new NotImplementedException();
				}

				public void ReadXml(XmlReader reader)
				{
					id = reader.GetAttribute("id");
					name = reader.GetAttribute("name");
					string q = reader.GetAttribute("count");
					if (q != null) count = @uint.Parse(q);
					q = reader.GetAttribute("digits");
					if (q != null) digits = short.Parse(q);
					q = reader.GetAttribute("magnitude");
					if (q != null) magnitude = short.Parse(q);

					string inner = reader.ReadInnerXml();
					inner = inner.Replace(".", CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator);
					string[] s = inner.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
					foreach (string ss in s)
					{
						content.Add(@float.Parse(ss));
					}
				}

				public void WriteXml(XmlWriter writer)
				{
					count = (@uint)content.Count;
					if (id != null && id != "") writer.WriteAttributeString("id", id);
					if (name != null && name != "") writer.WriteAttributeString("name", name);
					writer.WriteAttributeString("count", count.ToString());
					if (digits != 0) writer.WriteAttributeString("digits", digits.ToString());
					if (magnitude != 0) writer.WriteAttributeString("magnitude", magnitude.ToString());

					if (content.Count != 0)
					{
						string result = "";
						foreach (@float ss in content)
						{
							result += ss + " ";
						}
						result.Trim();
						result = result.Replace(",", ".");
						writer.WriteRaw(result);
					}
				}
			}

			//int_array

			[XmlElement("technique_common")]
			public _technique_common technique_common;
			[XmlType("technique_commonB")]
			public class _technique_common
			{
				public accessor accessor;
			}

			[XmlElement("technique")]
			public List<technique> technique;
		}
		public class technique
		{
			[XmlAttribute]
			public NMTOKEN profile;

			//TODO: How to implement this? (XmlAnyElement and xmlnode?)
		}
		public class triangles
		{
			public triangles()
			{
				input = new List<InputLocalOffset>();
				p = new List<p>();
				extra = new List<extra>();
			}
			[XmlAttribute]
			public NCName name;
			[XmlAttribute]
			public @uint count;
			[XmlAttribute]
			public NCName material;

			[XmlElement("input")]
			public List<InputLocalOffset> input;

			[XmlElement("p")]
			public List<p> p;

			[XmlElement("extra")]
			public List<extra> extra;
		}
		public class vertices
		{
			public vertices()
			{
				input = new List<InputLocal>();
				extra = new List<extra>();
			}
			[XmlAttribute]
			public ID id;
			[XmlAttribute]
			public NCName name;

			[XmlElement("input")]
			public List<InputLocal> input;

			[XmlElement("extra")]
			public List<extra> extra;
		}
		public class visual_scene
		{
			public visual_scene()
			{
				node = new List<node>();
				evaluate_scene = new List<_evaluate_scene>();
				extra = new List<extra>();
			}
			public visual_scene(string id)
				: this()
			{
				this.id = id;
			}
			[XmlAttribute]
			public ID id;
			[XmlAttribute]
			public NCName name;

			public asset asset;
			[XmlElement("node")]
			public List<node> node;
			[XmlElement("evaluate_scene")]
			public List<_evaluate_scene> evaluate_scene;
			public class _evaluate_scene
			{
				public _evaluate_scene()
				{
					render = new List<_render>();
				}
				[XmlAttribute]
				public NCName name;

				public List<_render> render;
				public class _render
				{
					public _render()
					{
						layer = new List<NCName>();
					}
					[XmlAttribute]
					public anyUri camera_node;

					[XmlElement("layer")]
					public List<NCName> layer;
					public instance_effect instance_effect;
				}
			}
			[XmlElement("extra")]
			public List<extra> extra;
		}
		public enum fx_modifier_enum_common
		{
			CONST,
			UNIFORM,
			VARYING,
			STATIC,
			VOLATILE,
			EXTERN,
			SHARED
		}
		public enum fx_opaque_enum
		{
			A_ONE,
			RGB_ZERO
		}
		public enum fx_sampler_filter_common
		{
			NONE,
			NEAREST,
			LINEAR,
			NEAREST_MIPMAP_NEAREST,
			LINEAR_MIPMAP_NEAREST,
			NEAREST_MIPMAP_LINEAR,
			LINEAR_MIPMAP_LINEAR
		}
		public enum fx_sampler_wrap_common
		{
			NONE,
			WRAP,
			MIRROR,
			CLAMP,
			BORDER
		}
		public enum fx_surface_face_enum
		{
			POSITIVE_X,
			NEGATIVE_X,
			POSITIVE_Y,
			NEGATIVE_Y,
			POSITIVE_Z,
			NEGATIVE_Z
		}
		public enum fx_surface_format_hint_channels_enum
		{
			RGB,
			RGBA,
			L,
			LA,
			D,
			XYZ,
			XYZW
		}
		public enum fx_surface_format_hint_precision_enum
		{
			LOW,
			MID,
			HIGH
		}
		public enum fx_surface_format_hint_range_enum
		{
			SNORM,
			UNORM,
			SINT,
			UINT,
			FLOAT
		}
		public enum fx_surface_format_hint_option_enum
		{
			SRGB_GAMMA,
			NORMALIZED3,
			NORMALIZED4,
			COMPRESSABLE
		}
		public enum fx_surface_type_enum
		{
			UNTYPED,
			[XmlEnum("1D")]
			_1D,
			[XmlEnum("2D")]
			_2D,
			[XmlEnum("3D")]
			_3D,
			RECT,
			CUBE,
			DEPTH
		}
		public enum NodeType
		{
			JOINT,
			NODE
		}
		public enum UpAxisType
		{
			X_UP,
			Y_UP,
			Z_UP
		}

		public class DAEIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Models;
			}

			public override string GetFileDescription()
			{
				return "COLLADA DAE File (DAE)";
			}

			public override string GetFileFilter()
			{
				return "COLLADA DAE File (*.dae)|*.dae";
			}

			public override Bitmap GetIcon()
			{
				return null;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				String s;
				try
				{
					s = Encoding.ASCII.GetString(File.Data);
				}
				catch { return FormatMatch.No; }
				if(s.StartsWith("<?xml") && s.Contains("<COLLADA xmlns=\"http://www.collada.org/2005/11/COLLADASchema\" version=\"")) return FormatMatch.Content;
				return FormatMatch.No;
			}

		}
	}
}
