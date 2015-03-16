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
        bool changesHaveBeenMade = false;
        public static bool Exit = false;
        public static Size mainWindowSize;
        public static int difBtwMainAndWorkSpace;
        bool fileContainerNotCreatedYet = true;
        public static bool closeOrChange = true;
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
                    информацияОФайлеToolStripMenuItem.Enabled = true;
                    спектрToolStripMenuItem.Enabled = true;
                    масштабToolStripMenuItem.Enabled = true;
                    максимизироватьПоШиринеToolStripMenuItem.Enabled = true;
                    минимизироватьToolStripMenuItem.Enabled = true;
                    максимизироватьПоВысотеToolStripMenuItem.Enabled = true;
                    if (fileContainerNotCreatedYet == true)
                    {
                        listBox1.Height = this.Height - 87;
                        listBox1.Show();
                        fileContainerNotCreatedYet = false;
                    }
                    string[] splitted = openFileDialog.FileName.Split('\\');
                    listBox1.Items.Add(splitted[splitted.Length - 1]);   
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


        private void информацияОФайлеToolStripMenuItem_CheckStateChanged(object sender, EventArgs e)
        {
            if (Exit == false)
            {
                if (FileNames[currentFile] != "")
                {
                    if (информацияОФайлеToolStripMenuItem.Checked == true)
                    {
                        newMDIChildForm2 = new Form2();
                        newMDIChildForm2.DesktopLocation = new Point(300, 300);
                        newMDIChildForm2.MdiParent = this;

                        newMDIChildForm2.Show();
                    }
                    else
                    {
                        newMDIChildForm2.Close();
                        информацияОФайлеToolStripMenuItem.Checked = false;
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

        private void максимизироватьПоШиринеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            newMDIChildForm1.Location = new Point(0,newMDIChildForm1.Location.Y);
            newMDIChildForm1.Width = this.Width-listBox1.Width-20;
        }

        private void максимизироватьПоВысотеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            newMDIChildForm1.Location = new Point(newMDIChildForm1.Location.X, 0);
            newMDIChildForm1.Height = this.Bounds.Height - 92;
            Form1.MaxHeight(newMDIChildForm1);
        }

        private void минимизироватьToolStripMenuItem_Click(object sender, EventArgs e)
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
                listBox1.Height = this.Bounds.Height - 87;
        }

        public static void SetFileInfoUncheckedValue ()
        {
            информацияОФайлеToolStripMenuItem.Checked = false;
        }

    }
}
