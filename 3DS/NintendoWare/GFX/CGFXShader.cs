using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tao.OpenGl;
using System.Drawing;
using LibEveryFileExplorer.Collections;

namespace _3DS.NintendoWare.GFX
{
	public class CGFXShader
	{
		public int[] Textures;
		private CMDL.MTOB Material;
		public CGFXShader(CMDL.MTOB Material, int[] Textures)
		{
			this.Textures = Textures;
			this.Material = Material;
		}

		public void Enable()
		{
			Gl.glUseProgram(program);
			//setup uniforms
			/*for (int i = 0; i < 3; i++)
			{
				String ss = "color_register" + i;
				Gl.glUniform4f(Gl.glGetUniformLocation(program, ss), g_color_registers[i][0], g_color_registers[i][1], g_color_registers[i][2], g_color_registers[i][3]);
			}
			for (int i = 0; i < 1; i++)
			{
				String ss = "matColor";
				Gl.glUniform4f(Gl.glGetUniformLocation(program, ss), MatColor[0], MatColor[1], MatColor[2], MatColor[3]);
			}
			for (int i = 0; i < 4; i++)
			{
				String ss = "color_const" + i;
				Gl.glUniform4f(Gl.glGetUniformLocation(program, ss), g_color_consts[i][0], g_color_consts[i][1], g_color_consts[i][2], g_color_consts[i][3]);
			}*/
			// TODO: cache value of GetUniformLocation
			//Gl.glUniform4fv(Gl.glGetUniformLocation(program, "registers"), 3, new float[] { g_color_registers[0][0], g_color_registers[0][1], g_color_registers[0][2], g_color_registers[0][3], g_color_registers[1][0], g_color_registers[1][1], g_color_registers[1][2], g_color_registers[1][3], g_color_registers[2][0], g_color_registers[2][1], g_color_registers[2][2], g_color_registers[2][3] });
		}
		public void Disable()
		{
			//Gl.glDeleteProgram(program);
			//Gl.glDeleteShader(vertex_shader);
			//Gl.glDeleteShader(fragment_shader);
			// TODO: cache value of GetUniformLocation
			//Gl.glUniform4fv(Gl.glGetUniformLocation(program, "registers"), 3, g_color_registers[0]);
		}

		//Fragment Lighting Primary Color:
		//Color = SUM((dmp_FragmentLightSource[i].diffuse * dmp_FragmentMaterial.diffuse * DOT(dmp_FragmentLightSource[i].position, NormalVector) * SdwAttPr[i] + dmp_FragmentMaterial.ambient * dmp_FragmentLightSource[i].ambient) * Spot[i] * DistAtt[i]) + dmp_FragmentMaterial.emission + dmp_FragmentMaterial.ambient * dmp_FragmentLighting.ambient
		//Color = SUM((vec3(1.0) * mDiff * DOT(

		public void Compile()
		{
			// w.e good for now
			uint sampler_count = (uint)Textures.Length;
			//if (sampler_count == 0)
			//{
			//	sampler_count = 1;
			//}
			// generate vertex/fragment shader code
			//{
			StringBuilder vert_ss = new StringBuilder();
			//String vert_ss = "";

			vert_ss.AppendFormat("vec4 diffuse = {0};\n", GetVec4(Material.MaterialColor.DiffuseU32));
			vert_ss.AppendFormat("vec4 ambient = {0};\n", GetVec4(Material.MaterialColor.Ambient));
			vert_ss.AppendFormat("vec4 spec1 = {0};\n", GetVec4(Material.MaterialColor.Specular0U32));
			vert_ss.AppendFormat("vec4 spec2 = {0};\n", GetVec4(Material.MaterialColor.Specular1U32));

			vert_ss.AppendLine("void main()");
			vert_ss.AppendLine("{");
			{
				//if (Material.NrActiveTextureCoordiators != 0)
				//{
					vert_ss.AppendLine("gl_FrontColor = gl_Color * ambient;");
					//vert_ss.AppendLine("gl_BackColor = gl_Color * ambient;");
					//vert_ss.AppendLine("gl_FrontSecondaryColor = gl_Color * ambient;");
					//vert_ss.AppendLine("gl_BackSecondaryColor = gl_Color * ambient;");
				//}
				//else
				//{
				//	vert_ss.AppendLine("gl_FrontColor = diffuse * gl_Color * ambient;");
					//vert_ss.AppendLine("gl_BackColor = diffuse * gl_Color * ambient;");
					//vert_ss.AppendLine("gl_FrontSecondaryColor = diffuse * gl_Color * ambient;");
					//vert_ss.AppendLine("gl_BackSecondaryColor = diffuse * gl_Color * ambient;");
				//}

				//vert_ss.AppendLine("gl_FrontColor = vec4(gl_Color.rgb, 1.0);");

				if (Material.Tex0 != null) vert_ss.AppendFormat("gl_TexCoord[0] = gl_TextureMatrix[0] * gl_MultiTexCoord{0};\n", Material.TextureCoordiators[0].SourceCoordinate);
				if (Material.Tex1 != null) vert_ss.AppendFormat("gl_TexCoord[1] = gl_TextureMatrix[1] * gl_MultiTexCoord{0};\n", Material.TextureCoordiators[1].SourceCoordinate);
				if (Material.Tex2 != null) vert_ss.AppendFormat("gl_TexCoord[2] = gl_TextureMatrix[2] * gl_MultiTexCoord{0};\n", Material.TextureCoordiators[2].SourceCoordinate);

				vert_ss.AppendLine("gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;");
			}
			vert_ss.AppendLine("}");

			// create/compile vertex shader
			vertex_shader = Gl.glCreateShader(Gl.GL_VERTEX_SHADER);

			{
				var vert_src_str = vert_ss.ToString();
				//const GLchar* vert_src = vert_src_str.c_str();
				Gl.glShaderSource(vertex_shader, 1, new string[] { vert_src_str }, new int[] { vert_src_str.Length });
			}

			//}	// done generating vertex shader

			Gl.glCompileShader(vertex_shader);


			string[] constant = 
			{
				"const0",
				"const1",
				"const2",
				"const3",
				"const4",
				"const5",
				"emission",
				"ambient",
				"diffuse",
				"spec1",
				"spec2"
			};

			/*
			 * Confirmed by intermediate file:
			 * 0x00 - PrimaryColor
			 * 0x01 - FragmentPrimaryColor
			 * 0x02 - FragmentSecondaryColor
			 * 0x03 - Texture0
			*/

			string[] p0 =
			{
				"gl_Color",
				"vec4(0.5, 0.5, 0.5, 1.0)",
				"vec4(0.5, 0.5, 0.5, 1.0)",
				//"gl_Color",
				//"gl_Color",
				"texture2D(textures0, gl_TexCoord[0].st)",
				"texture2D(textures1, gl_TexCoord[1].st)",
				"texture2D(textures2, gl_TexCoord[2].st)",
				"texture2D(textures3, gl_TexCoord[3].st)",
				"const0",//?
				"const1",//?
				"const2",//?
				"const3",//?
				"const4",//?
				"const5",//?
				"bufin",
				"{0}",
				"previous"
			};

			string[] c_p1 = 
			{
				"{0}.rgb",//0
				"vec3(1.0) - {0}.rgb",//1
				"{0}.aaa",//2
				"vec3(1.0) - {0}.aaa",//3
				"{0}.rrr",//4
				"vec3(1.0) - {0}.rrr",//5
				"{0}.rgb",//?
				"{0}.rgb",//?
				"{0}.ggg",//8
				"vec3(1.0) - {0}.ggg",//9
				"{0}.rgb",//?
				"{0}.rgb",//?
				"{0}.bbb",//c
				"vec3(1.0) - {0}.bbb",//d
				"{0}.rgb",//?
				"{0}.rgb",//?
			};

			string[] a_p1 = 
			{
				"{0}.a",
				"1.0 - {0}.a",
				"{0}.r",
				"1.0 - {0}.r",
				"{0}.g",
				"1.0 - {0}.g",
				"{0}.b",
				"1.0 - {0}.b",
				"{0}.a",//unknown 8
				"{0}.a",//unknown 9
				"{0}.a",//unknown A
				"{0}.a",//unknown B
				"{0}.a",//unknown C
				"{0}.a",//unknown D
				"{0}.a",//unknown E
				"{0}.a"//unknown F
			};

			string[] c_p2 =
			{
				"j1.rgb",
				"j1.rgb * j2.rgb",
				"j1.rgb + j2.rgb",
				"j1.rgb + j2.rgb - vec3(0.5)",
				"j1.rgb * j3.rgb + j2.rgb * (vec3(1.0) - j3.rgb)",
				"j1.rgb - j2.rgb",
				"vec3(4 * ((j1.r - 0.5) * (j1.r - 0.5) + (j1.g - 0.5) * (j2.g - 0.5) + (j1.b - 0.5) * (j2.b - 0.5)))",
				"vec4(4 * ((j1.r - 0.5) * (j1.r - 0.5) + (j1.g - 0.5) * (j2.g - 0.5) + (j1.b - 0.5) * (j2.b - 0.5)))",
				"(j1.rgb * j2.rgb) + j3.rgb",//"j1.rgb * j2.aaa + j3.rgb" //"(vec3(1.0) - ((vec3(1.0) - j1.rgb) + (vec3(1.0) - j2.rgb))) + j3.rgb",//"j2.rgb * j1.rgb + j3.rgb * (vec3(1.0) - j1.rgb)",//"j1.rgb * j2.rgb + j3.rgb * (vec3(1.0) - j2.rgb)",//"j1.rgb * j2.rgb + j3.rgb * (vec3(1.0) - j2.rgb)"//"j2.rgb + j1.rgb - (vec3(1.0) - j3.rgb)",//Unknown 8
				"clamp(j1.rgb + j2.rgb, 0.0, 1.0) * j3.rgb"//"j2.rgb + (j1.rgb * j3.rgb)"//"j3.rgb * j1.rgb + j2.rgb - vec3(0.1)"//"j2.rgb * j3.rgb + j1.rgb - vec3(0.5)"//"j1.rgb * j2.rgb + j3.rgb * (vec3(1.0) - j2.rgb)"//"j1.rgb + j3.rgb * j2.rgb"//"j3.rgb"//Unknown 9
			};

			string[] a_p2 =
			{
				"j1.a",
				"j1.a * j2.a",
				"j1.a + j2.a",
				"j1.a + j2.a - 0.5",
				"j1.a * j3.a + j2.a * (1.0 - j3.a)",
				"j1.a - j2.a",
				"j1.a",//unknown
				"j1.a",//unknown
				"(j1.a * j2.a) + j3.a",//"j2.a * j1.a + j3.a * (1.0 - j1.a)",//Unknown 8
				"clamp(j1.a + j2.a, 0.0, 1.0) * j3.a"//"(1.0 - j2.a) + (j1.a * j3.a)"//"j2.a - (j1.a + j3.a)"//"j3.a * j1.a + j2.a - 0.1"//"j1.a * j2.a + j3.a * (1.0 - j2.a)"//Unknown 9
			};

			string[] scale =
			{
				//"1.0",
				//"1.0",
				//"1.0"
				"1.0",
				"2.0",
				"4.0"
			};

			// generate fragment shader code
			//{
			StringBuilder frag_ss = new StringBuilder();
			//frag_ss += "uniform sampler2D tex;";
			// uniforms
			for (uint i = 0; i != sampler_count; ++i)
				frag_ss.AppendFormat("uniform sampler2D textures{0};\n", i);

			frag_ss.AppendFormat("vec4 const0 = {0};\n", GetVec4(Material.MaterialColor.Constant0U32));
			frag_ss.AppendFormat("vec4 const1 = {0};\n", GetVec4(Material.MaterialColor.Constant1U32));
			frag_ss.AppendFormat("vec4 const2 = {0};\n", GetVec4(Material.MaterialColor.Constant2U32));
			frag_ss.AppendFormat("vec4 const3 = {0};\n", GetVec4(Material.MaterialColor.Constant3U32));
			frag_ss.AppendFormat("vec4 const4 = {0};\n", GetVec4(Material.MaterialColor.Constant4U32));
			frag_ss.AppendFormat("vec4 const5 = {0};\n", GetVec4(Material.MaterialColor.Constant5U32));
			frag_ss.AppendFormat("vec4 diffuse = {0};\n", GetVec4(Material.MaterialColor.DiffuseU32));
			frag_ss.AppendFormat("vec4 ambient = {0};\n", GetVec4(Material.MaterialColor.Ambient));
			frag_ss.AppendFormat("vec4 spec1 = {0};\n", GetVec4(Material.MaterialColor.Specular0U32));
			frag_ss.AppendFormat("vec4 spec2 = {0};\n", GetVec4(Material.MaterialColor.Specular1U32));
			frag_ss.AppendFormat("vec4 emission = {0};\n", GetVec4(Material.MaterialColor.EmissionU32));
			frag_ss.AppendFormat("vec4 unk1 = vec4(1.0);\n");
			frag_ss.AppendFormat("vec4 unk2 = vec4(0.0);\n");
			frag_ss.AppendFormat("vec4 unk3 = {0};\n", GetVec4(Material.FragShader.BufferColor));

			if (Material.MaterialColor.Constant0U32 ==Color.FromArgb(0, 0, 0, 0) && Material.MaterialColor.Constant1U32 ==Color.FromArgb(0, 0, 0, 0) && Material.MaterialColor.Constant2U32 ==Color.FromArgb(0, 0, 0, 0) && Material.MaterialColor.Constant3U32 ==Color.FromArgb(0, 0, 0, 0) && Material.MaterialColor.Constant4U32 ==Color.FromArgb(0, 0, 0, 0) && Material.MaterialColor.Constant5U32 == Color.FromArgb(0, 0, 0, 0))
			{
				frag_ss.AppendLine("const0 = const1 = const2 = const3 = const4 = const5 = vec4(0, 0, 0, 1);");
			}

			frag_ss.AppendFormat("vec4 buf0 = unk3;\n");
			frag_ss.AppendFormat("vec4 buf1;\n");
			frag_ss.AppendFormat("vec4 buf2;\n");
			frag_ss.AppendFormat("vec4 buf3;\n");
			frag_ss.AppendFormat("vec4 buf4;\n");
			//frag_ss.AppendFormat("vec4 unk3 = gl_Color;\n");
			//frag_ss.AppendFormat("vec4 unk3 = vec4(1.0);\n");

			frag_ss.AppendLine("void main()");
			frag_ss.AppendLine("{");
			{
				frag_ss.AppendLine("vec4 previous = vec4(1.0);");
				frag_ss.AppendLine("vec4 i1c;");
				frag_ss.AppendLine("vec4 i2c;");
				frag_ss.AppendLine("vec4 i3c;");
				frag_ss.AppendLine("vec4 i1a;");
				frag_ss.AppendLine("vec4 i2a;");
				frag_ss.AppendLine("vec4 i3a;");

				frag_ss.AppendLine("vec4 j1;");
				frag_ss.AppendLine("vec4 j2;");
				frag_ss.AppendLine("vec4 j3;");
				frag_ss.AppendLine("vec4 ConstRgba;");
				frag_ss.AppendLine("vec4 bufin;");
				for (int i = 0; i < 6; i++)
				{
					if (i > 0 && i < 5)
					{
						if (((Material.FragShader.BufferCommand3 >> (i + 7)) & 1) == 1)
							frag_ss.AppendFormat("buf{0}.rgb = previous.rgb;\n", i);
						else frag_ss.AppendFormat("buf{0}.rgb = buf{1}.rgb;\n", i, i - 1);
						if (((Material.FragShader.BufferCommand3 >> (i + 11)) & 1) == 1)
							frag_ss.AppendFormat("buf{0}.a = previous.a;\n", i);
						else frag_ss.AppendFormat("buf{0}.a = buf{1}.a;\n", i, i - 1);
					}

					frag_ss.AppendFormat("ConstRgba = {0};\n", GetVec4(Material.FragShader.TextureCombiners[i].ConstRgba));
					int c_input1 = (Material.FragShader.TextureCombiners[i].SrcRgb >> 0) & 0xF;
					int c_input2 = (Material.FragShader.TextureCombiners[i].SrcRgb >> 4) & 0xF;
					int c_input3 = (Material.FragShader.TextureCombiners[i].SrcRgb >> 8) & 0xF;

					int a_input1 = (Material.FragShader.TextureCombiners[i].SrcAlpha >> 0) & 0xF;
					int a_input2 = (Material.FragShader.TextureCombiners[i].SrcAlpha >> 4) & 0xF;
					int a_input3 = (Material.FragShader.TextureCombiners[i].SrcAlpha >> 8) & 0xF;

					//String cons;
					///*if (Material.Shader.TexEnvSlotInfos[i].ConstRgba.ToArgb() == Color.Black.ToArgb())*/ cons = "const" + (Material.Shader.TexEnvSlotInfos[i].Unknown3 & 0xF);
					//else cons = "ConstRgba";
					if (i > 0) frag_ss.AppendFormat("bufin = buf{0};\n", i - 1);

					frag_ss.AppendFormat("i1c = {0};\n", String.Format(p0[c_input1], constant[Material.FragShader.TextureCombiners[i].Constant]));
					frag_ss.AppendFormat("i2c = {0};\n", String.Format(p0[c_input2], constant[Material.FragShader.TextureCombiners[i].Constant]));
					frag_ss.AppendFormat("i3c = {0};\n", String.Format(p0[c_input3], constant[Material.FragShader.TextureCombiners[i].Constant]));

					frag_ss.AppendFormat("i1a = {0};\n", String.Format(p0[a_input1], constant[Material.FragShader.TextureCombiners[i].Constant]));
					frag_ss.AppendFormat("i2a = {0};\n", String.Format(p0[a_input2], constant[Material.FragShader.TextureCombiners[i].Constant]));
					frag_ss.AppendFormat("i3a = {0};\n", String.Format(p0[a_input3], constant[Material.FragShader.TextureCombiners[i].Constant]));

					//frag_ss.AppendFormat("i1 = vec4({0}.rgb, {1}.a);\n", String.Format(p0[c_input1], Material.FragShader.TextureCombiners[i].Unknown3 & 0xF), String.Format(p0[a_input1], Material.FragShader.TextureCombiners[i].Unknown3 & 0xF));
					//frag_ss.AppendFormat("i2 = vec4({0}.rgb, {1}.a);\n", String.Format(p0[c_input2], Material.FragShader.TextureCombiners[i].Unknown3 & 0xF), String.Format(p0[a_input2], Material.FragShader.TextureCombiners[i].Unknown3 & 0xF));
					//frag_ss.AppendFormat("i3 = vec4({0}.rgb, {1}.a);\n", String.Format(p0[c_input3], Material.FragShader.TextureCombiners[i].Unknown3 & 0xF), String.Format(p0[a_input3], Material.FragShader.TextureCombiners[i].Unknown3 & 0xF));

					uint c_p1_1 = (Material.FragShader.TextureCombiners[i].Operands >> 0) & 0xF;
					uint c_p1_2 = (Material.FragShader.TextureCombiners[i].Operands >> 4) & 0xF;
					uint c_p1_3 = (Material.FragShader.TextureCombiners[i].Operands >> 8) & 0xF;

					uint a_p1_1 = (Material.FragShader.TextureCombiners[i].Operands >> 12) & 0xF;
					uint a_p1_2 = (Material.FragShader.TextureCombiners[i].Operands >> 16) & 0xF;
					uint a_p1_3 = (Material.FragShader.TextureCombiners[i].Operands >> 20) & 0xF;

					frag_ss.AppendFormat("j1 = vec4({0}, {1});\n", String.Format(c_p1[c_p1_1], "i1c"), String.Format(a_p1[a_p1_1], "i1a"));
					frag_ss.AppendFormat("j2 = vec4({0}, {1});\n", String.Format(c_p1[c_p1_2], "i2c"), String.Format(a_p1[a_p1_2], "i2a"));
					frag_ss.AppendFormat("j3 = vec4({0}, {1});\n", String.Format(c_p1[c_p1_3], "i3c"), String.Format(a_p1[a_p1_3], "i3a"));

					if (Material.FragShader.TextureCombiners[i].CombineRgb == 7) frag_ss.AppendFormat("previous = {0};\n", c_p2[Material.FragShader.TextureCombiners[i].CombineRgb]);
					else frag_ss.AppendFormat("previous = clamp(vec4(({0}) * vec3({2}, {2}, {2}), ({1}) * {3}), 0.0, 1.0);\n", c_p2[Material.FragShader.TextureCombiners[i].CombineRgb], a_p2[Material.FragShader.TextureCombiners[i].CombineAlpha], scale[Material.FragShader.TextureCombiners[i].ScaleRgb], scale[Material.FragShader.TextureCombiners[i].ScaleAlpha]);
					//else frag_ss.AppendFormat("previous = clamp(vec4(({0}) * vec3({2}), ({1}) * {3}), 0.0, 1.0);\n", c_p2[Material.Shader.TexEnvSlotInfos[i].CombineRgb], a_p2[Material.Shader.TexEnvSlotInfos[i].CombineAlpha], Material.Shader.TexEnvSlotInfos[i].Unknown2Rgb + 1, Material.Shader.TexEnvSlotInfos[i].Unknown2Alpha + 1);
				}
				frag_ss.AppendLine("gl_FragColor = previous;");
				//frag_ss.AppendLine("gl_FragColor = texture2D(textures0, gl_TexCoord[0].st) * vec4(gl_Color.rgb, 1.0);");
			}
			frag_ss.AppendLine("}");

			//std::cout << frag_ss.str() << '\n';

			// create/compile fragment shader
			fragment_shader = Gl.glCreateShader(Gl.GL_FRAGMENT_SHADER);
			{
				var frag_src_str = frag_ss.ToString();
				Gl.glShaderSource(fragment_shader, 1, new String[] { frag_src_str }, new int[] { frag_src_str.Length });
			}

			//}	// done generating fragment shader

			Gl.glCompileShader(fragment_shader);

			// check compile status of both shaders
			//{
			int vert_compiled = 0;
			int frag_compiled = 0;

			Gl.glGetShaderiv(vertex_shader, Gl.GL_COMPILE_STATUS, out vert_compiled);
			Gl.glGetShaderiv(fragment_shader, Gl.GL_COMPILE_STATUS, out frag_compiled);

			if (vert_compiled == 0)
			{
				//std::cout << "Failed to compile vertex shader\n";
			}

			if (frag_compiled == 0)
			{
				//std::cout << "Failed to compile fragment shader\n";
			}

			// create program, attach shaders
			program = Gl.glCreateProgram();
			Gl.glAttachShader(program, vertex_shader);
			Gl.glAttachShader(program, fragment_shader);

			// link program, check link status
			Gl.glLinkProgram(program);
			int link_status;
			Gl.glGetProgramiv(program, Gl.GL_LINK_STATUS, out link_status);

			if (link_status == 0)
			{
				//std::cout << "Failed to link program!\n";
			}

			Gl.glUseProgram(program);

			// set uniforms
			for (uint i = 0; i != sampler_count; ++i)
			{
				String ss = "textures" + i;
				Gl.glUniform1i(Gl.glGetUniformLocation(program, ss), (int)i);
			}
		}

		private String GetVec4(Color c)
		{
			return String.Format("vec4({0}, {1}, {2}, {3})", (c.R / 255f).ToString().Replace(",", "."), (c.G / 255f).ToString().Replace(",", "."), (c.B / 255f).ToString().Replace(",", "."), (c.A / 255f).ToString().Replace(",", "."));
		}
		private String GetVec4(Vector4 c)
		{
			return String.Format("vec4({0}, {1}, {2}, {3})", (c.X).ToString().Replace(",", "."), (c.Y).ToString().Replace(",", "."), (c.Z).ToString().Replace(",", "."), (c.W).ToString().Replace(",", "."));
		}
		public int program = 0, fragment_shader = 0, vertex_shader = 0;

	}
}