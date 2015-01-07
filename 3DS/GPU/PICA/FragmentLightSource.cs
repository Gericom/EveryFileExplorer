using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Collections;

namespace _3DS.GPU.PICA
{
	public class FragmentLightSource
	{
		public Vector4 Ambient = new Vector4(0, 0, 0, 1);
		public Vector4 Diffuse = new Vector4(1, 1, 1, 1);//default white for light0, black for others
		public Vector4 Specular0 = new Vector4(1, 1, 1, 1);//default white for light0, black for others
		public Vector4 Specular1 = new Vector4(1, 1, 1, 1);//default white for light0, black for others
		public Vector4 Position = new Vector4(0, 0, 1, 0);
		public Vector3 SpotDirection = new Vector3(0, 0, -1);

		public Boolean Shadowed = false;
		public Boolean SpotEnabled = false;
		public Boolean DistanceAttenuationEnabled = false;
		public Boolean TwoSideDiffuse = false;
	}
}
