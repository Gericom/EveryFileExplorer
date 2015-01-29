using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Collections;

namespace MarioKart.UI.MapViewer
{
	public abstract class RenderGroup
	{
		//public abstract bool Createable { get; }
		public abstract bool Interactable { get; }

		public void Render()
		{
			Render(false, -1);
		}

		public abstract void Render(bool Picking, int PickingId);

		public virtual Object GetEntry(int Index) { throw new NotImplementedException(); }

		public virtual Vector3 GetPosition(int Index) { throw new NotImplementedException(); }
		public virtual void SetPosition(int Index, Vector3 Position, bool ValidY = false) { throw new NotImplementedException(); }
	}
}
