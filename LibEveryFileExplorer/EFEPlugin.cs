using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibEveryFileExplorer
{
	public abstract class EFEPlugin
	{
		/// <summary>
		/// Function which is called just after loading all plugins. (so everything is loaded already)
		/// Use it for debugging and initialization.
		/// </summary>
		public abstract void OnLoad();
	}
}
