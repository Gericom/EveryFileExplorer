using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Tao.OpenGl;

namespace _3DS.NintendoWare.LYT1
{
	public class CLYTShader
	{
		public int[] Textures;
		private mat1.MaterialEntry Material;
		public CLYTShader(mat1.MaterialEntry Material, int[] Textures)
		{
			this.Textures = Textures;
			this.Material = Material;
		}

		public void Enable()
		{
			Gl.glUseProgram(program);
			//setup uniforms

			Gl.glUniform4f(Gl.glGetUniformLocation(program, "colorreg"), Material.BufferColor.R / 255f, Material.BufferColor.G / 255f, Material.BufferColor.B / 255f, Material.BufferColor.A / 255f);

			for (int i = 0; i < 6; i++)
			{
				Gl.glUniform4f(Gl.glGetUniformLocation(program, "const" + i), Material.ConstColors[i].R / 255f, Material.ConstColors[i].G / 255f, Material.ConstColors[i].B / 255f, Material.ConstColors[i].A / 255f);
			}
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

			//vert_ss.AppendFormat("vec4 diffuse = {0};\n", GetVec4(Material.MaterialColor.DiffuseU32));
			//vert_ss.AppendFormat("vec4 ambient = {0};\n", GetVec4(Material.MaterialColor.Ambient));
			//vert_ss.AppendFormat("vec4 spec1 = {0};\n", GetVec4(Material.MaterialColor.Specular0U32));
			//vert_ss.AppendFormat("vec4 spec2 = {0};\n", GetVec4(Material.MaterialColor.Specular1U32));

			vert_ss.AppendLine("void main()");
			vert_ss.AppendLine("{");
			{
				vert_ss.AppendLine("gl_FrontColor = gl_Color;");

				//if (Material.Tex0 != null) vert_ss.AppendFormat("gl_TexCoord[0] = gl_TextureMatrix[0] * gl_MultiTexCoord{0};\n", Material.TextureCoordiators[0].SourceCoordinate);
				//if (Material.Tex1 != null) vert_ss.AppendFormat("gl_TexCoord[1] = gl_TextureMatrix[1] * gl_MultiTexCoord{0};\n", Material.TextureCoordiators[1].SourceCoordinate);
				//if (Material.Tex2 != null) vert_ss.AppendFormat("gl_TexCoord[2] = gl_TextureMatrix[2] * gl_MultiTexCoord{0};\n", Material.TextureCoordiators[2].SourceCoordinate);
				int i = 0;
				foreach (var v in Material.TexCoordGens)
				{
					if ((int)v.Source < 4)
					{
						vert_ss.AppendFormat("gl_TexCoord[{0}] = gl_TextureMatrix[{0}] * gl_MultiTexCoord{1};\n", i, (int)v.Source);
					}
					i++;
				}

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

			string[] p0 =
			{
				"texture2D(textures0, gl_TexCoord[0].st)",
				"texture2D(textures1, gl_TexCoord[1].st)",
				"texture2D(textures2, gl_TexCoord[2].st)",
				"texture2D(textures3, gl_TexCoord[3].st)",
				"{0}",
				"gl_Color",
				"previous",
				"colorreg"
			};

			string[] oc =
			{
				"{0}.rgb",
				"vec3(1.0) - {0}.rgb",
				"{0}.aaa",
				"vec3(1.0) - {0}.aaa",
				"{0}.rrr",
				"vec3(1.0) - {0}.rrr",
				"{0}.ggg",
				"vec3(1.0) - {0}.ggg",
				"{0}.bbb",
				"vec3(1.0) - {0}.bbb"
			};

			string[] oa =
			{
				"{0}.a",
				"(1.0 - {0}.a)",
				"{0}.r",
				"(1.0 - {0}.r)",
				"{0}.g",
				"(1.0 - {0}.g)",
				"{0}.b",
				"(1.0 - {0}.b)"
			};

			string[] comb_c =
			{
				"j1.rgb",
				"j1.rgb * j2.rgb",
				"j1.rgb + j2.rgb",
				"j1.rgb + j2.rgb - vec3(0.5)",
				"j1.rgb * j3.rgb + j2.rgb * (vec3(1.0) - j3.rgb)",
				"j1.rgb - j2.rgb",
				"clamp(j1.rgb + j2.rgb, 0.0, 1.0) * j3.rgb",
				"(j1.rgb * j2.rgb) + j3.rgb",
				"j1.rgb",
				"j1.rgb",
				"j1.rgb",
				"j1.rgb"
			};

			string[] comb_a =
			{
				"j1.a",
				"j1.a * j2.a",
				"j1.a + j2.a",
				"j1.a + j2.a - vec3(0.5)",
				"j1.a * j3.a + j2.a * (vec3(1.0) - j3.a)",
				"j1.a - j2.a",
				"clamp(j1.a + j2.a, 0.0, 1.0) * j3.a",
				"(j1.a * j2.a) + j3.a",
				"j1.a",
				"j1.a",
				"j1.a",
				"j1.a"
			};

			// generate fragment shader code
			//{
			StringBuilder frag_ss = new StringBuilder();
			//frag_ss += "uniform sampler2D tex;";
			// uniforms
			for (uint i = 0; i != sampler_count; ++i)
				frag_ss.AppendFormat("uniform sampler2D textures{0};\n", i);

			frag_ss.AppendLine("uniform vec4 colorreg;");

			for (int i = 0; i < 6; i++) frag_ss.AppendFormat("uniform vec4 const{0};\n", i);


			frag_ss.AppendLine("void main()");
			frag_ss.AppendLine("{");
			{
				if (Material.TexMaps.Length == 0 && Material.TevStages.Length == 0) frag_ss.AppendLine("gl_FragColor = gl_Color;");
				else if (Material.TexMaps.Length == 1 && Material.TevStages.Length == 0) frag_ss.AppendLine("gl_FragColor = texture2D(textures0, gl_TexCoord[0].st) * gl_Color;");
				else
				{
					frag_ss.AppendLine("vec4 j1;");
					frag_ss.AppendLine("vec4 j2;");
					frag_ss.AppendLine("vec4 j3;");
					frag_ss.AppendLine("vec4 previous;");
					foreach (var tev in Material.TevStages)
					{
						uint colorconst = (tev.ConstColors & 0xF) - 1;
						uint alphaconst = ((tev.ConstColors >> 4) & 0xF) - 1;
						frag_ss.AppendFormat("j1 = vec4({0}, {1});\n",
							string.Format(oc[(int)tev.ColorOperators[0]], string.Format(p0[(int)tev.ColorSources[0]], string.Format("const{0}", colorconst))),
							string.Format(oa[(int)tev.AlphaOperators[0]], string.Format(p0[(int)tev.AlphaSources[0]], string.Format("const{0}", alphaconst))));

						frag_ss.AppendFormat("j2 = vec4({0}, {1});\n",
							string.Format(oc[(int)tev.ColorOperators[1]], string.Format(p0[(int)tev.ColorSources[1]], string.Format("const{0}", colorconst))),
							string.Format(oa[(int)tev.AlphaOperators[1]], string.Format(p0[(int)tev.AlphaSources[1]], string.Format("const{0}", alphaconst))));
						frag_ss.AppendFormat("j3 = vec4({0}, {1});\n",
							string.Format(oc[(int)tev.ColorOperators[2]], string.Format(p0[(int)tev.ColorSources[2]], string.Format("const{0}", colorconst))),
							string.Format(oa[(int)tev.AlphaOperators[2]], string.Format(p0[(int)tev.AlphaSources[2]], string.Format("const{0}", alphaconst))));

						frag_ss.AppendFormat("previous = vec4({0},{1});\n", comb_c[(int)tev.ColorMode], comb_a[(int)tev.AlphaMode]);
					}
					frag_ss.AppendLine("gl_FragColor = previous;");
				}
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

			Gl.glUniform4f(Gl.glGetUniformLocation(program, "colorreg"), Material.BufferColor.R / 255f, Material.BufferColor.G / 255f, Material.BufferColor.B / 255f, Material.BufferColor.A / 255f);

			for (int i = 0; i < 6; i++)
			{
				Gl.glUniform4f(Gl.glGetUniformLocation(program, "const" + i), Material.ConstColors[i].R / 255f, Material.ConstColors[i].G / 255f, Material.ConstColors[i].B / 255f, Material.ConstColors[i].A / 255f);
			}
		}

		private String GetVec4(Color c)
		{
			return String.Format("vec4({0}, {1}, {2}, {3})", (c.R / 255f).ToString().Replace(",", "."), (c.G / 255f).ToString().Replace(",", "."), (c.B / 255f).ToString().Replace(",", "."), (c.A / 255f).ToString().Replace(",", "."));
		}
		public int program = 0, fragment_shader = 0, vertex_shader = 0;
	}
}
