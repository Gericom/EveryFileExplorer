using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibEveryFileExplorer.Files;
using EveryFileExplorer.Plugins;
using EveryFileExplorer.Files;
using LibEveryFileExplorer;
using System.Runtime.InteropServices;
using System.Reflection;
using LibEveryFileExplorer.Compression;
using LibEveryFileExplorer.Projects;
using System.IO;
using EveryFileExplorer.Properties;
using System.Diagnostics;
using NAudio.Wave;

namespace EveryFileExplorer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Win32Util.SetWindowTheme(treeView1.Handle, "explorer", null);
        }

        private String PendingPath = null;
        public Form1(String Path)
            : this()
        {
            if (Path.Length < 1 || !System.IO.File.Exists(Path)) return;
            PendingPath = Path;
        }

        private ImageList ProjectTreeIL = null;
        private ProjectBase Project = null;

        private void Form1_Load(object sender, EventArgs e)
        {
            ProjectTreeIL = new ImageList();
            ProjectTreeIL.ColorDepth = ColorDepth.Depth32Bit;
            ProjectTreeIL.ImageSize = new Size(16, 16);
            ProjectTreeIL.Images.Add(Resources.document);
            ProjectTreeIL.Images.Add(Resources.folder_open);
            treeView1.ImageList = ProjectTreeIL;

            for (int i = 0; i < this.Controls.Count; i++)
            {
                MdiClient mdiClient = this.Controls[i] as MdiClient;
                if (mdiClient != null)
                {
                    Win32Util.SetMDIBorderStyle(mdiClient, BorderStyle.None);
                }
            }
            menuNew.MenuItems.Clear();
            foreach (Plugin p in Program.PluginManager.Plugins)
            {
                List<Type> creatables = new List<Type>();
                foreach (Type t in p.FileFormatTypes)
                {
                    if (t.GetInterfaces().Contains(typeof(IEmptyCreatable))) creatables.Add(t);
                }
                if (creatables.Count > 0)
                {
                    MenuItem m = menuNew.MenuItems.Add(p.Name);
                    foreach (Type t in creatables)
                    {
                        MenuItem ii = m.MenuItems.Add(((dynamic)new StaticDynamic(t)).Identifier.GetFileDescription());
                        ii.Click += new EventHandler(CreateNew_Click);
                        ii.Tag = t;
                    }
                }
            }
            menuFileNew.MenuItems.Clear();
            foreach (Plugin p in Program.PluginManager.Plugins)
            {
                List<Type> creatables = new List<Type>();
                foreach (Type t in p.FileFormatTypes)
                {
                    if (t.GetInterfaces().Contains(typeof(IFileCreatable))) creatables.Add(t);
                }
                if (creatables.Count > 0)
                {
                    MenuItem m = menuFileNew.MenuItems.Add(p.Name);
                    foreach (Type t in creatables)
                    {
                        MenuItem ii = m.MenuItems.Add(((dynamic)new StaticDynamic(t)).Identifier.GetFileDescription());
                        ii.Click += new EventHandler(CreateFileNew_Click);
                        ii.Tag = t;
                    }
                }
            }
            menuProjectNew.MenuItems.Clear();
            foreach (Plugin p in Program.PluginManager.Plugins)
            {
                if (p.ProjectTypes.Length > 0)
                {
                    MenuItem m = menuProjectNew.MenuItems.Add(p.Name);
                    foreach (Type t in p.ProjectTypes)
                    {
                        MenuItem ii = m.MenuItems.Add(((dynamic)new StaticDynamic(t)).Identifier.GetProjectDescription());
                        ii.Click += new EventHandler(CreateNewProject_Click);
                        ii.Tag = t;
                    }
                }
            }
            menuCompression.MenuItems.Clear();
            foreach (Plugin p in Program.PluginManager.Plugins)
            {
                if (p.CompressionTypes.Length != 0)
                {
                    MenuItem m = menuCompression.MenuItems.Add(p.Name);
                    foreach (Type t in p.CompressionTypes)
                    {
                        MenuItem ii = m.MenuItems.Add(((dynamic)new StaticDynamic(t)).Identifier.GetCompressionDescription());
                        var dec = ii.MenuItems.Add("Decompress...");
                        dec.Click += new EventHandler(Decompress_Click);
                        dec.Tag = t;
                        if (t.GetInterfaces().Contains(typeof(ICompressable)))
                        {
                            var comp = ii.MenuItems.Add("Compress...");
                            comp.Click += new EventHandler(Compress_Click);
                            comp.Tag = t;
                        }
                    }
                }
            }
            if (PendingPath != null) Program.FileManager.OpenFile(new EFEDiskFile(PendingPath));
            PendingPath = null;
        }

        void Compress_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "All Files (*.*)|*.*";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK
                && openFileDialog1.FileName.Length > 0)
            {
                saveFileDialog1.Filter = "All Files (*.*)|*.*";
                saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(openFileDialog1.FileName) + "_comp" + Path.GetExtension(openFileDialog1.FileName);
                if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK
                    && saveFileDialog1.FileName.Length > 0)
                {
                    Type comp = (Type)((MenuItem)sender).Tag;
                    ICompressable c = (ICompressable)comp.InvokeMember("", BindingFlags.CreateInstance, null, null, new object[0]);
                    byte[] result = null;
                    try
                    {
                        result = c.Compress(File.ReadAllBytes(openFileDialog1.FileName));
                    }
                    catch (Exception ee)
                    {
                        MessageBox.Show("An error occured while trying to compress:\n" + ee);
                        return;
                    }
                    File.Create(saveFileDialog1.FileName).Close();
                    File.WriteAllBytes(saveFileDialog1.FileName, result);
                }
            }
        }

        void Decompress_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "All Files (*.*)|*.*";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK
                && openFileDialog1.FileName.Length > 0)
            {
                Type comp = (Type)((MenuItem)sender).Tag;
                CompressionFormatBase c = (CompressionFormatBase)comp.InvokeMember("", BindingFlags.CreateInstance, null, null, new object[0]);
                byte[] result = null;
                try
                {
                    result = c.Decompress(File.ReadAllBytes(openFileDialog1.FileName));
                }
                catch (Exception ee)
                {
                    MessageBox.Show("An error occured while trying to decompress! The file might not be in this compression type or not compressed at all!");
                    return;
                }
                saveFileDialog1.Filter = "All Files (*.*)|*.*";
                saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(openFileDialog1.FileName) + "_dec" + Path.GetExtension(openFileDialog1.FileName);
                if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK
                    && saveFileDialog1.FileName.Length > 0)
                {
                    File.Create(saveFileDialog1.FileName).Close();
                    File.WriteAllBytes(saveFileDialog1.FileName, result);
                }
            }
        }

        void CreateNewProject_Click(object sender, EventArgs e)
        {
            ProjectBase p = (ProjectBase)((Type)((MenuItem)sender).Tag).InvokeMember("", BindingFlags.CreateInstance, null, null, new object[0]);
            if (p.CreateNew())
            {
                Project = p;
                EnableProjectMode();
            }
        }

        public void BringMDIWindowToFront(Form Dialog)
        {
            ActivateMdiChild(Dialog);
            Dialog.BringToFront();
        }
        bool opening = false;
        private void OpenFile(object sender, EventArgs e)
        {
            if (opening) return;
            opening = true;
            String Filter = "";
            List<String> Filters = new List<string>();
            Filters.Add("all files (*.*)|*.*");
            List<String> Extenstions = new List<string>();
            String AllSupportedFilesA = "All Supported Files (";
            String AllSupportedFilesB = "|";
            foreach (Plugin p in Program.PluginManager.Plugins)
            {
                foreach (Type t in p.FileFormatTypes)
                {
                    if (t.GetInterfaces().Contains(typeof(IViewable)))
                    {
                        dynamic d = new StaticDynamic(t);
                        String FilterString;
                        try
                        {
                            FilterString = d.Identifier.GetFileFilter();
                        }
                        catch (NotImplementedException)
                        {
                            continue;
                        }
                        if (FilterString == null || FilterString.Length == 0) continue;
                        if (!Filters.Contains(FilterString.ToLower()))
                        {
                            string[] strArray = FilterString.Split(new char[] { '|' });
                            if ((strArray == null) || ((strArray.Length % 2) != 0)) continue;
                            string[] q = FilterString.Split('|');
                            for (int i = 1; i < q.Length; i += 2)
                            {
                                foreach (string f in q[i].Split(';'))
                                {
                                    if (!Extenstions.Contains(f.ToLower()))
                                    {
                                        Extenstions.Add(f.ToLower());
                                        if (!AllSupportedFilesA.EndsWith("(")) AllSupportedFilesA += ", ";
                                        AllSupportedFilesA += f.ToLower();
                                        if (!AllSupportedFilesB.EndsWith("|")) AllSupportedFilesB += ";";
                                        AllSupportedFilesB += f.ToLower();
                                    }
                                }
                            }
                            Filters.Add(FilterString.ToLower());
                            if (Filter != "") Filter += "|";
                            Filter += FilterString;
                        }
                    }
                }
            }
            if (Filter != "")
            {
                Filter = AllSupportedFilesA + ")" + AllSupportedFilesB + "|" + Filter;
                Filter += "|";
            }
            Filter += "All Files (*.*)|*.*";
            openFileDialog1.Filter = Filter;
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK
                && openFileDialog1.FileName.Length > 0)
            {
                foreach (String s in openFileDialog1.FileNames)
                {
                    Program.FileManager.OpenFile(new EFEDiskFile(s));
                }
            }
            opening = false;
        }

        private void menuTile_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileVertical);
        }

        private void menuCascade_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
        }

        private void Form1_MdiChildActivate(object sender, EventArgs e)
        {
            if (ActiveMdiChild != null)
            {
                var v = Program.FileManager.GetViewableFileFromWindow(ActiveMdiChild);
                dynamic FileFormat = v.FileFormat;
                menuSave.Enabled = buttonSave.Enabled = FileFormat is IWriteable && (v.File.CompressionFormat == null || v.File.CompressionFormat is ICompressable);
                menuSaveAs.Enabled = FileFormat is IWriteable | FileFormat is IConvertable;
                menuClose.Enabled = true;
            }
            else
            {
                menuClose.Enabled = false;
                menuSaveAs.Enabled = menuSave.Enabled = buttonSave.Enabled = false;
            }
        }

        private void menuExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void SaveFile(object sender, EventArgs e)
        {
            ViewableFile file = Program.FileManager.GetViewableFileFromWindow(ActiveMdiChild);
            if (!(file.FileFormat is IWriteable))
            {
                MessageBox.Show("This format is not saveable!");
                return;
            }
            if (file.File is EFEDiskFile && ((EFEDiskFile)file.File).Path == null)
            {
                saveFileDialog1.Filter = file.FileFormat.GetSaveDefaultFileFilter();
                saveFileDialog1.Title = "Save";
                saveFileDialog1.FileName = file.File.Name;
                if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK
                && saveFileDialog1.FileName.Length > 0)
                {
                    ((EFEDiskFile)file.File).Path = saveFileDialog1.FileName;
                    file.File.Name = System.IO.Path.GetFileName(saveFileDialog1.FileName);
                    file.Dialog.Text = file.File.Name;
                }
                else return;
            }
            try
            {
                byte[] data = file.FileFormat.Write();
                file.File.Data = data;
                file.File.Save();
            }
            catch (Exception ee)
            {
                MessageBox.Show("An error occurred while trying to save:\n" + ee);
            }
        }

        private void menuClose_Click(object sender, EventArgs e)
        {
            ActiveMdiChild.Close();
        }

        private void menuSaveAs_Click(object sender, EventArgs e)
        {
            String Filter = "";
            ViewableFile file = Program.FileManager.GetViewableFileFromWindow(ActiveMdiChild);
            if (file.FileFormat is IWriteable) Filter += file.FileFormat.GetSaveDefaultFileFilter();
            if (file.FileFormat is IConvertable)
            {
                if (Filter.Length > 0) Filter += "|";
                Filter += file.FileFormat.GetConversionFileFilters();
            }
            saveFileDialog1.Filter = Filter;
            saveFileDialog1.Title = "Save As";
            saveFileDialog1.FileName = file.File.Name;
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK
                && saveFileDialog1.FileName.Length > 0)
            {
                if (file.FileFormat is IWriteable && saveFileDialog1.FilterIndex == 1)
                {
                    try
                    {
                        byte[] data = file.FileFormat.Write();
                        if (data != null)
                        {
                            System.IO.File.Create(saveFileDialog1.FileName).Close();
                            System.IO.File.WriteAllBytes(saveFileDialog1.FileName, data);
                        }
                    }
                    catch { }
                }
                else
                {
                    if (!file.FileFormat.Convert(saveFileDialog1.FilterIndex - (file.FileFormat is IWriteable ? 2 : 1), saveFileDialog1.FileName))
                    {
                        MessageBox.Show("An error occured while trying to convert!");
                    }
                }
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files) Program.FileManager.OpenFile(new EFEDiskFile(file));
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        void CreateNew_Click(object sender, EventArgs e)
        {
            Program.FileManager.CreateEmptyFileWithType((Type)(((MenuItem)sender).Tag));
        }

        void CreateFileNew_Click(object sender, EventArgs e)
        {
            Program.FileManager.CreateFileFromFileWithType((Type)(((MenuItem)sender).Tag));
        }

        private void EnableProjectMode()
        {
            //Close all files
            Program.FileManager.CloseAllFiles();

            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();
            treeView1.Nodes.AddRange(Project.GetProjectTree());
            treeView1.EndUpdate();

            menuProject.Visible = panel2.Visible = splitter1.Visible = menuCloseProject.Enabled = true;
            menuNew.Enabled = menuFileNew.Enabled = menuOpen.Enabled = buttonOpen.Enabled = false;
        }

        private void DisableProjectMode()
        {
            Program.FileManager.CloseAllFiles();
            Project = null;
            menuProject.Visible = splitter1.Visible = panel2.Visible = menuCloseProject.Enabled = false;
            menuNew.Enabled = menuFileNew.Enabled = menuOpen.Enabled = buttonOpen.Enabled = true;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Win32Util.WM_COPYDATA)
            {
                if (WindowState == FormWindowState.Minimized) Win32Util.ShowWindowAsync(Handle, Win32Util.SW_RESTORE);
                TopMost = true;
                TopMost = false;
                String path = Win32Util.GetStringFromMessage(m);
                if (path.Length < 1 || !System.IO.File.Exists(path)) return;
                Program.FileManager.OpenFile(new EFEDiskFile(path));
                return;
            }
            base.WndProc(ref m);
        }

        private void menuItem1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Every File Explorer Project (*.efeproj)|*.efeproj";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK
                && openFileDialog1.FileName.Length > 0)
            {
                Type t = ProjectFile.GetProjectType(File.ReadAllBytes(openFileDialog1.FileName));
                Project = (ProjectBase)t.InvokeMember("", BindingFlags.CreateInstance, null, null, new object[] { openFileDialog1.FileName });
                EnableProjectMode();
            }
        }

        private void menuCloseProject_Click(object sender, EventArgs e)
        {
            DisableProjectMode();
        }

        private void menuItem3_Click(object sender, EventArgs e)
        {
            Project.Build();
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            String Path = Project.ProjectDir + "\\" + e.Node.FullPath;
            if ((new FileInfo(Path).Attributes & FileAttributes.Directory) != 0) return;
            Program.FileManager.OpenFile(new EFEDiskFile(Path));
        }

        private void menuItem1_Click_1(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo("cmd") { WorkingDirectory = Path.GetDirectoryName(Application.ExecutablePath) });
        }

        private void menuItem10_Click(object sender, EventArgs e)
        {
            //I use to some of the YATA-PLUS's code:https://github.com/exelix11/YATA-PLUS/edit/master/Form1.cs
            st.StartInfo.FileName = @"Plugins\vgmstream\vgmstream.exe";
            try
            {
                OpenFileDialog opn = new OpenFileDialog();
                opn.Filter =
                    "Supported files (*.bcstm;*.bcwav;*.bfwav;*.brstm;*.nus3bank;*.strm;*.swav)|*.bcstm;*.bcwav;*.bfwav;*.brstm;*.nus3bank;*.strm;*.swav" +
                    "|CTR Stream (*.bcstm)|*.bcstm" +
                    "|CTR Wave (*.bcwav)|*.bcwav" +
                    "|Cafe Wave (*.bfwav)|*.bfwav" +
                    "|Brstm Audio File (*.brstm)|*.brstm" +
                    "|Nus3bank Audio File (*.nus3bank)|*.nus3bank" +
                    "|Nitro Sound Stream(*.strm)|*.strm" +
                    "|Nitro Sound Wave(*.swav)|*.swav" +
                    "|All Files (*.*)|*.*";
                opn.Title = "Open file";
                opn.Multiselect = true;
                if (opn.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (opn.FileNames.Length == 1)
                    {
                        AudioTOWav(opn.FileName);
                    }
                    else
                    {
                        for (int i = 0; i < opn.FileNames.Length; i++)
                        {
                            Process prc = new Process();
                            prc.StartInfo.FileName = @"Plugins\vgmstream\test.exe";
                            prc.StartInfo.Arguments = "-o \"" + opn.FileNames[i] + ".wav\" " + "\"" + opn.FileNames[i] + "\"";
                            prc.StartInfo.CreateNoWindow = true;
                            prc.StartInfo.UseShellExecute = false;
                            prc.Start();
                            prc.WaitForExit();
                        }
                        MessageBox.Show("Batch Conversion Completed");
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }

            void AudioTOWav(string input)
            {
                st.StartInfo.FileName = @"Plugins\vgmstream\vgmstream.exe";
                {
                }
                SaveFileDialog sv = new SaveFileDialog();
                sv.Filter = "Wave File (*.wav)|*.wav";
                sv.Title = "Save file";
                if (sv.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Process prc = new Process();
                    prc.StartInfo.FileName = @"Plugins\vgmstream\test.exe";
                    prc.StartInfo.Arguments = "-o \"" + sv.FileName + "\" " + "\"" + input + "\"";
                    prc.StartInfo.CreateNoWindow = true;
                    prc.StartInfo.UseShellExecute = false;
                    prc.Start();
                    prc.WaitForExit();
                    if (File.Exists(sv.FileName)) MessageBox.Show("Conversion Completed");
                }
            }
        }

        System.Diagnostics.Process st = new System.Diagnostics.Process();

        private void menuItem12_Click(object sender, EventArgs e)
        {
            //I use to some of the YATA-PLUS's code:https://github.com/exelix11/YATA-PLUS/edit/master/Form1.cs
            st.StartInfo.FileName = @"Plugins\ctr_WaveConverter32\ctr_WaveConverter32.exe";
            try
            {
                OpenFileDialog opn = new OpenFileDialog();
                opn.Filter = "Wave File (*.wav)|*.wav";
                opn.Title = "Select a WAV file";
                opn.Multiselect = true;
                if (opn.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (opn.FileNames.Length == 1)
                    {
                        Wav2CWAV(opn.FileName);
                    }
                    else
                    {
                        int error = 0;
                        for (int i = 0; i < opn.FileNames.Length; i++)
                        {
                            if (!APP_not_Optimize_Cwavs)
                            {
                                Debug.Print("Optimizing CWAV: " + Path.GetTempPath() + Path.GetFileName(opn.FileNames[i]) + ".tmp.wav");
                                WaveFormat New = new WaveFormat(APP_opt_samples, 8, 1);
                                WaveStream Original = new WaveFileReader(opn.FileNames[i]);
                                WaveFormatConversionStream stream = new WaveFormatConversionStream(New, Original);
                                if (System.IO.File.Exists(Path.GetTempPath() + Path.GetFileName(opn.FileNames[i]) + ".tmp.wav")) File.Delete(Path.GetTempPath() + Path.GetFileName(opn.FileNames[i]) + ".tmp.wav");
                                WaveFileWriter.CreateWaveFile(Path.GetTempPath() + Path.GetFileName(opn.FileNames[i]) + ".tmp.wav", stream);
                                stream.Dispose();
                                Original.Dispose();
                            }
                            Process prc = new Process();
                            prc.StartInfo.FileName = @"Plugins\ctr_WaveConverter32\ctr_WaveConverter32.exe";
                            if (!APP_not_Optimize_Cwavs) prc.StartInfo.Arguments = "-o \"" + opn.FileNames[i] + ".bcwav\" \"" + Path.GetTempPath() + Path.GetFileName(opn.FileNames[i]) + ".tmp.wav\""; else prc.StartInfo.Arguments = "-o \"" + opn.FileNames[i] + ".bcwav\" \"" + opn.FileNames[i] + "\"";
                            Debug.Print("Converting CWAV: " + Path.GetTempPath() + Path.GetFileName(opn.FileNames[i]) + ".tmp.wav");
                            prc.Start();
                            prc.WaitForExit();
                            if (System.IO.File.Exists(Path.GetTempPath() + Path.GetFileName(opn.FileNames[i]) + ".tmp.wav")) File.Delete(Path.GetTempPath() + Path.GetFileName(opn.FileNames[i]) + ".tmp.wav");
                            if (!File.Exists(opn.FileNames[i] + ".bcwav")) error++;
                        }
                        if (error != 0) MessageBox.Show("Convert Completed"); else MessageBox.Show("Batch Convert Completed");
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
        }

        public static bool APP_not_Optimize_Cwavs = false;
        public static int APP_opt_samples = 11025;

        void Wav2CWAV(string input)
        {
            SaveFileDialog sv = new SaveFileDialog();
            sv.Filter = "CTR Wave (*.bcwav)|*.bcwav";
            sv.Title = "Save the CWAV file";
            if (sv.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!APP_not_Optimize_Cwavs)
                {
                    Debug.Print("Optimizing CWAV: " + Path.GetTempPath() + Path.GetFileName(input) + ".tmp.wav");
                    WaveFormat New = new WaveFormat(APP_opt_samples, 8, 1);
                    WaveStream Original = new WaveFileReader(input);
                    WaveFormatConversionStream stream = new WaveFormatConversionStream(New, Original);
                    if (System.IO.File.Exists(Path.GetTempPath() + Path.GetFileName(input) + ".tmp.wav")) File.Delete(Path.GetTempPath() + Path.GetFileName(input) + ".tmp.wav");
                    WaveFileWriter.CreateWaveFile(Path.GetTempPath() + Path.GetFileName(input) + ".tmp.wav", stream);
                    stream.Dispose();
                    Original.Dispose();
                }
                Process prc = new Process();
                prc.StartInfo.FileName = @"Plugins\ctr_WaveConverter32\ctr_WaveConverter32.exe";
                if (!APP_not_Optimize_Cwavs) prc.StartInfo.Arguments = "-o \"" + sv.FileName + "\" \"" + Path.GetTempPath() + Path.GetFileName(input) + ".tmp.wav\""; else prc.StartInfo.Arguments = "-o \"" + sv.FileName + "\" \"" + input + "\"";
                prc.Start();
                prc.WaitForExit();
                if (System.IO.File.Exists(Path.GetTempPath() + Path.GetFileName(input) + ".tmp.wav")) File.Delete(Path.GetTempPath() + Path.GetFileName(input) + ".tmp.wav");
                if (File.Exists(sv.FileName)) MessageBox.Show("Convert Completed"); else MessageBox.Show("An Error while converting the file, run the command in the cmd to check the output");
            }
        }
    }
}
