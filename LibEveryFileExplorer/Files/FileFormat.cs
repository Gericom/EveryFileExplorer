using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Drawing;

namespace LibEveryFileExplorer.Files
{
	internal interface FileFormatBase { }

	public abstract class FileFormat<T> : FileFormatBase where T : FileFormatIdentifier, new()
	{
		private static T _identifier = new T();
		public static T Identifier { get { return _identifier; } }
	}

	public abstract class FileFormatIdentifier
	{
		protected const String Category_Animations = "Animations";
		protected const String Category_Archives = "Archives";
		protected const String Category_Audio = "Audio";
		protected const String Category_Cells = "Cells";
		protected const String Category_Fonts = "Fonts";
		protected const String Category_Graphics = "Graphics";
		protected const String Category_Layouts = "Layouts";
		protected const String Category_Models = "Models";
		protected const String Category_Palettes = "Palettes";
		protected const String Category_Particles = "Particles";
		protected const String Category_Roms = "Roms";
		protected const String Category_Screens = "Screens";
		protected const String Category_Shaders = "Shaders";
		protected const String Category_Strings = "Strings";
		protected const String Category_Textures = "Textures";
		protected const String Category_Videos = "Videos";
		protected const String Category_Movies = "Movies";
                protected const String Category_Sound = "Sound";
		protected const String Category_Text = "Text";

		public abstract String GetCategory();
		public abstract String GetFileDescription();
		public abstract String GetFileFilter();
		public virtual Bitmap GetIcon() { return null; }
		public virtual Bitmap GetTreeIcon() { return null; }
		public abstract FormatMatch IsFormat(EFEFile File);
	}

	public enum FormatMatch
	{
		No,
		Extension,
		Content
	}
}
