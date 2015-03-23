using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class MDIParent1 : Form
    {
        private int childFormNumber = 0;
        public static List<string> FileNames = new List<string> { };
        public static int currentFile=0;
        Form1 newMDIChildForm1;
        public static bool visualRepresentationShowing = false;
        Form2 newMDIChildForm2;
        public static int windowX = 0;
        public static int windowY = 0;
        public static bool changesHaveBeenMade = false;
        public static bool Exit = false;
        public static Size mainWindowSize;
        public static int difBtwMainAndWorkSpace;
        public static bool fileContainerNotCreatedYet = true;
        public static bool closeOrChange = true;
        bool hideContextMenuStripForFileContainer = true;
        public MDIParent1()
        {
            InitializeComponent();
            mainWindowSize = this.Size;
        }

        private void OpenFile(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            openFileDialog.InitialDirectory = "D:/Учебный год 2013-2014 мать его за ногу/материалы по курсовой работе/1мин/30сек";
            openFileDialog.Filter = "Аудио файлы (*.wav)|*.wav|Все файлы (*.*)|*.*";
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                if (FileNames.Contains(openFileDialog.FileName) == false)
                {
                    if (FileNames.Count != 0)
                        currentFile++;
                    FileNames.Add(openFileDialog.FileName);
                    closeToolStripMenuItem.Enabled = true;
                    fileInfoToolStripMenuItem.Enabled = true;
                    spectrumToolStripMenuItem.Enabled = true;
                    scaleToolStripMenuItem.Enabled = true;
                    maximizeTheWidthToolStripMenuItem.Enabled = true;
                    minimizeToolStripMenuItem.Enabled = true;
                    maximizeTheHeightToolStripMenuItem.Enabled = true;
                    if (fileContainerNotCreatedYet == true)
                    {
                        listView1.Height = this.Height - 87;
                        listView1.Show();
                        fileContainerNotCreatedYet = false;
                    }
                    string[] splitted = openFileDialog.FileName.Split('\\');
                    string name = splitted[splitted.Length - 1];
                    if (name.Length > 33)
                    {
                        listView1.Items.Add(name.Substring(0,30) + "...");
                        
                    }
                    else
                    {
                        int spaceCount = 57 - name.Length;
                        string[] array = new string[spaceCount];
                        for (int i=0; i<spaceCount; i++)
                        {
                            array[i] = " ";
                        }
                        string tail = String.Concat(array);
                        listView1.Items.Add(name+tail);
                    }
                    if (visualRepresentationShowing == false)
                    {
                        newMDIChildForm1 = new Form1();
                        newMDIChildForm1.MdiParent = this;
                        newMDIChildForm1.Location = new Point(windowX, windowY);
                        newMDIChildForm1.Show();
                        visualRepresentationShowing = true;
                    }
                    else
                    {
                        closeOrChange = false;
                        newMDIChildForm1.Close();
                        newMDIChildForm1 = new Form1();
                        newMDIChildForm1.MdiParent = this;
                        newMDIChildForm1.Location = new Point(windowX, windowY);
                        newMDIChildForm1.Show();
                        visualRepresentationShowing = true;
                        closeOrChange = true;
                    }
                    difBtwMainAndWorkSpace = this.Location.X;
                }
                else
                {
                    MessageBox.Show("Файл с таким именем уже открыт", "Интелектуальная обработка сигналов",
                                         MessageBoxButtons.OK);
                }
            }           
        }

        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            saveFileDialog.Filter = "Аудио файлы (*.wav)|*.wav|Все файлы (*.*)|*.*";
            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string FileName = saveFileDialog.FileName;
                WAV.Writing2(Form1.newWav, FileName);
            }
        }

        private void ExitToolsStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CutToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void PasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void CascadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
        }

        private void TileVerticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileVertical);
        }

        private void TileHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void ArrangeIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.ArrangeIcons);
        }

        private void CloseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form childForm in MdiChildren)
            {
                childForm.Close();
            }
        }


        private void fileInfoToolStripMenuItem_CheckStateChanged(object sender, EventArgs e)
        {
            if (Exit == false || FileNames.Count != 0)
            {
                if (FileNames[currentFile] != "")
                {
                    if (fileInfoToolStripMenuItem.Checked == true)
                    {
                        newMDIChildForm2 = new Form2();
                        newMDIChildForm2.DesktopLocation = new Point(300, 300);
                        newMDIChildForm2.MdiParent = this;

                        newMDIChildForm2.Show();
                    }
                    else
                    {
                        newMDIChildForm2.Close();
                        fileInfoToolStripMenuItem.Checked = false;
                    }
                }
            }
            else
                newMDIChildForm2.Close();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (newMDIChildForm1 != null)
            {
                newMDIChildForm1.Close();
                Form1.WorkSpaceCreated = false;
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {           
            WAV.Writing(Form1.newWav, FileNames[currentFile]);
        }

        private void MDIParent1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (FileNames.Count != 0)
            {
                Exit = true;
                if (changesHaveBeenMade == true)
                {
                    string[] splitted = MDIParent1.FileNames[MDIParent1.currentFile].Split('\\');
                    string name = splitted[splitted.Length - 1];
                    var response = MessageBox.Show("Сохранить изменения в " + name + "?", "Интелектуальная обработка сигналов",
                                 MessageBoxButtons.YesNoCancel,
                                 MessageBoxIcon.Question);
                    if (response == System.Windows.Forms.DialogResult.No)
                        e.Cancel = false;
                    else if (response == System.Windows.Forms.DialogResult.Yes)
                    {
                        WAV.Writing(WAV.Reading(FileNames[currentFile]), FileNames[currentFile]);
                        e.Cancel = false;
                    }
                    else
                    {
                        e.Cancel = true;
                        Exit = false;
                    }
                }
            }
        }

        private void maximizeTheWidthToolStripMenuItem_Click(object sender, EventArgs e)
        {
            newMDIChildForm1.Location = new Point(0,newMDIChildForm1.Location.Y);
            newMDIChildForm1.Width = this.Width-listView1.Width-20;
        }

        private void maximizeTheHeightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            newMDIChildForm1.Location = new Point(newMDIChildForm1.Location.X, 0);
            newMDIChildForm1.Height = this.Bounds.Height - 92;
            Form1.MaxHeight(newMDIChildForm1);
        }

        private void minimizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            newMDIChildForm1.Location = new Point(0, newMDIChildForm1.Location.Y);
            newMDIChildForm1.Width = 600;
            newMDIChildForm1.Height = 100;
        }

        private void MDIParent1_Load(object sender, EventArgs e)
        {
            this.Width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            this.Height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
        }

        private void MDIParent1_Resize(object sender, EventArgs e)
        {
            if (fileContainerNotCreatedYet == false)
                listView1.Height = this.Bounds.Height - 87;
        }

        public static void SetFileInfoUncheckedValue ()
        {
            fileInfoToolStripMenuItem.Checked = false;
        }
        //private void listView1_SelectedIndexChanged_1(object sender, EventArgs e)
        //{
        //    if (listView1.SelectedIndices[0] >= 0)
        //    {
        //        if (listView1.SelectedIndices[0] != currentFile)
        //        {
        //            MDIParent1.currentFile = listView1.SelectedIndices[0];
        //            if (visualRepresentationShowing == false)
        //            {
        //                newMDIChildForm1 = new Form1();
        //                newMDIChildForm1.MdiParent = this;
        //                newMDIChildForm1.Location = new Point(windowX, windowY);
        //                newMDIChildForm1.Show();
        //                visualRepresentationShowing = true;
        //            }
        //            else
        //            {
        //                closeOrChange = false;
        //                newMDIChildForm1.Close();
        //                newMDIChildForm1 = new Form1();
        //                newMDIChildForm1.MdiParent = this;
        //                newMDIChildForm1.Location = new Point(windowX, windowY);
        //                newMDIChildForm1.Show();
        //                visualRepresentationShowing = true;
        //                closeOrChange = true;
        //            }
        //        }
        //    }
        //}

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                if (listView1.SelectedIndices[0] >= 0)
                {
                    if (listView1.SelectedIndices[0] != currentFile || (listView1.SelectedIndices[0]==0 && currentFile==0))
                    {
                        MDIParent1.currentFile = listView1.SelectedIndices[0];
                        if (visualRepresentationShowing == false)
                        {
                            newMDIChildForm1 = new Form1();
                            newMDIChildForm1.MdiParent = this;
                            newMDIChildForm1.Location = new Point(windowX, windowY);
                            newMDIChildForm1.Show();
                            visualRepresentationShowing = true;
                        }
                        else
                        {
                            closeOrChange = false;
                            newMDIChildForm1.Close();
                            newMDIChildForm1 = new Form1();
                            newMDIChildForm1.MdiParent = this;
                            newMDIChildForm1.Location = new Point(windowX, windowY);
                            newMDIChildForm1.Show();
                            visualRepresentationShowing = true;
                            closeOrChange = true;
                        }
                    }
                }
            }
        }

        private void listView1_ItemMouseHover(object sender, ListViewItemMouseHoverEventArgs e)
        {
            if (e.Item != null)
            {
                    e.Item.ToolTipText = FileNames[e.Item.Index];
            }
        }

        private void listView1_ItemActivate(object sender, EventArgs e)
        {
            
        }

        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                {
                    if (listView1.FocusedItem != null)
                    {
                        if (e.X < listView1.FocusedItem.Position.X + listView1.FocusedItem.Bounds.Width &&
                            e.Y < listView1.FocusedItem.Position.Y + listView1.FocusedItem.Bounds.Height)
                        {
                            listView1.ContextMenuStrip = contextMenuStrip1;
                            hideContextMenuStripForFileContainer = false;
                        }
                        else
                            if (hideContextMenuStripForFileContainer == false)
                            {
                                listView1.ContextMenuStrip = null;
                                hideContextMenuStripForFileContainer = true;
                            }
                        listView1.FocusedItem.Focused = false;
                    }
                }       
        }

        private void closeAllExceptThisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int i = 0;
            while (FileNames.Count > 1)
            {
                if (i != currentFile)
                {
                    FileNames.Remove(FileNames[i]);
                    listView1.Items.Remove(listView1.Items[i]);
                    i--;
                    if (i < currentFile)
                    {
                        currentFile--;                       
                    }
                }
                i++;
            }
            
        }

        //private void openInNewWindowToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    MDIParent1 newMainWindow = new MDIParent1();
        //    newMainWindow.Text = "Интеллектуальная обработка сигналов (побочное окно)";
        //    newMainWindow.Show();
        //    listView1.Items.Add(FileNames[currentFile]);
        //    listView1.Show();
        //}

    }
}

