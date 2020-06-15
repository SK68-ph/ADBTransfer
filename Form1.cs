using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace ADBTransfer
{
    public partial class Form1 : Form
    {
        bool foundDevice = false;
        Process compiler = new Process();
        public Form1()
        {
            InitializeComponent();
        }

        //Transfer files from listview to device storage.
        public int pushFiles(string path)
        {
            compiler.StartInfo.FileName = "adb.exe";
            compiler.StartInfo.Arguments = "push " + path.Trim() + " /storage/emulated/0/Download/";
            compiler.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            compiler.StartInfo.UseShellExecute = false;
            compiler.StartInfo.RedirectStandardOutput = false;
            compiler.StartInfo.RedirectStandardError = false;
            compiler.Start();
            compiler.WaitForExit();
            return compiler.ExitCode;
        }

        //Scan for available device for transfer
        public bool scanDevice()
        {
            compiler.StartInfo.FileName = "adb.exe";
            //compiler.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            compiler.StartInfo.UseShellExecute = false;
            compiler.StartInfo.RedirectStandardOutput = true;
            compiler.StartInfo.Arguments = "devices";
            compiler.Start();
            string output = compiler.StandardOutput.ReadToEnd().Trim();
            compiler.WaitForExit();
            //check if theres device id
            if (output.Length > 24)
            {
                compiler = new Process();
                foundDevice = true;
                return true;
            }
            foundDevice = false;
            return false;
        }

        // Save file details(filename,filesize,filepath) to listview collection
        public void addRowListView(string path)
        {
            ListViewItem lvi = new ListViewItem();
            lvi.Text = (Path.GetFileName(path));
            long length = new FileInfo(path).Length / 1024;
            lvi.SubItems.Add(length.ToString() + " KB");
            lvi.SubItems.Add("Waiting...");
            lvi.SubItems.Add(path);
            //check if there is a duplicate in listview
            bool dupe = false;
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                if (listView1.Items[i].Text.Equals(lvi.Text))
                {
                    dupe = true;
                }
            }
            //if theres a duplicate = skip.
            if (!dupe)
            {
                listView1.Items.Add(lvi);
            }
            
        }

        //Display file details to listview.
        private void btnAddfile_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "Sample File.file";
            if (openFileDialog1.ShowDialog().Equals(DialogResult.OK))
            {
                for (int i = 0; i < openFileDialog1.FileNames.Length; i++)
                {
                    addRowListView(openFileDialog1.FileNames[i]);
                }
            }
        }

        private void btnAddFolder_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog().Equals(DialogResult.OK))
            {
                string[] filePaths = Directory.GetFiles(folderBrowserDialog1.SelectedPath);
                foreach (var file in filePaths)
                {
                    addRowListView(file);
                }
            }
        }

        private void btnPush_Click(object sender, EventArgs e)
        {
            // recheck if device is still found.
            if (foundDevice == false) scanDevice();

            if (foundDevice)
            {
                lblStatus.Text = "Pushing...";
                //loop through listview then start pushing files
                for (int i = 0; i < listView1.Items.Count; i++)
                {
                    //if file is already done then skip.
                    if (!listView1.Items[i].SubItems[2].Text.Equals("Done."))
                    {
                        int errorCode = pushFiles(listView1.Items[i].SubItems[3].Text);
                        if (errorCode == 0)
                        {
                            listView1.Items[i].SubItems[2].Text = "Done.";
                        }
                        else if (errorCode != 0)
                        {
                            listView1.Items[i].SubItems[2].Text = "ERROR(" + errorCode + ")";
                        }
                        else
                        {
                            listView1.Items[i].SubItems[2].Text = "Failed...";
                        }
                    }

                }
                lblStatus.Text = "Waiting...";
            }
            else
            {
                lblStatus.Text = "Device not found!";
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!File.Exists(Path.Combine(Environment.CurrentDirectory, "adb.exe")))
            {
                MessageBox.Show("Adb not found, please install adb and copy exe to current directory of program");
                Application.Exit();
            }
            else
            {
                //Initial scan for available device.
                if (!scanDevice())
                {
                    foundDevice = false;
                    lblStatus.Text = "Device not found!";
                }
                else
                {
                    foundDevice = true;
                    lblStatus.Text = "Waiting...";
                }
            }
            
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Clear rows of listview.
            listView1.Items.Clear();
        }

        //Drag and drop functions
        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }
        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            //Save dropped files path info
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                addRowListView(file);
            }
        }

    }
}
