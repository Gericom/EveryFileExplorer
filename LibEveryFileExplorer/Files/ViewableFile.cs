using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibEveryFileExplorer.Files;

namespace LibEveryFileExplorer.Files
{
	public class ViewableFile
	{
		public ViewableFile(EFEFile File, Type Format, bool CreateNew = false)
		{
			if (!Format.GetInterfaces().Contains(typeof(IViewable))) throw new ArgumentException("This format is not viewable!");
			this.File = File;
			if (CreateNew) FileFormat = Format.InvokeMember("", System.Reflection.BindingFlags.CreateInstance, null, null, new object[0]);
			else FileFormat = Format.InvokeMember("", System.Reflection.BindingFlags.CreateInstance, null, null, new object[] { File.Data });
			Dialog = FileFormat.GetDialog();
			Dialog.Tag = this;
			Dialog.Text = File.Name;
			Dialog.FormClosing += new FormClosingEventHandler(Dialog_FormClosing);
		}

		void Dialog_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (e.CloseReason == CloseReason.MdiFormClosing) EveryFileExplorerUtil.DisableFileDependencyDialog();
			if (DialogClosing != null) e.Cancel = !DialogClosing.Invoke(this);
		}

		public void ShowDialog(Form Parent)
		{
			Dialog.MdiParent = Parent;
			Dialog.Show();
		}

		public EFEFile File { get; private set; }
		public dynamic FileFormat { get; private set; }
		public Form Dialog { get; private set; }
		public delegate bool DialogClosingEventHandler(ViewableFile VFile);
		public event DialogClosingEventHandler DialogClosing;
	}
}
