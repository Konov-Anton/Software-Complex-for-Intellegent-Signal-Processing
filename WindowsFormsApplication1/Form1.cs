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
    public partial class Form1 : Form
    {
        public static WAV newWav;
        public static bool WorkSpaceCreated = false;       
        int difWidth;
        int difHeight;
        Graphics g;
        int mid;
        Point PickOutFrom=new Point(-1,-1);
        Point PickOutBefore=new Point (-1,-1);
        bool MouseMoves = false;
        bool CursorPainted = false;
        Bitmap bmp;
        double pos = 0;
        //int count;

        public Form1()
        {
            InitializeComponent();
            bmp = new Bitmap(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height);
            g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            pictureBox1.Image = bmp;
        }
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
                if (MDIParent1.FileNames[MDIParent1.currentFile] != "")
                {
                    mid = pictureBox1.Height / 2;
                    double dur = Math.Round((double)Form1.newWav.wavData.Length / (double)Form1.newWav.fmtAvgBPS, 3);
                    double step = (double)(this.Width) / newWav.sampleRate / 10.0;
                    e.Graphics.DrawLine(System.Drawing.Pens.Gray, pictureBox1.Location.X, mid, pictureBox1.Width, mid);
                    e.Graphics.DrawLine(System.Drawing.Pens.Black, pictureBox1.Location.X, pictureBox1.Height - 5, pictureBox1.Width, pictureBox1.Height - 5);
                    float x = (float)pictureBox1.Location.X;
                    int upperBound;
                    int emptyPart=0;
                    if (pos * newWav.LeftChData.Length + newWav.sampleRate*10 > newWav.LeftChData.Length - 1)
                    {
                        upperBound = newWav.LeftChData.Length - 1;
                        emptyPart = (int)(pos * newWav.LeftChData.Length + newWav.sampleRate * 10);
                       
                    }
                    else
                        upperBound = (int)(pos * newWav.LeftChData.Length + newWav.sampleRate * 10);
                    for (int i = (int)(pos*newWav.LeftChData.Length); i < upperBound; i=i+1)
                    {
                        if (i % (newWav.sampleRate * 10) == 0)
                        {
                            e.Graphics.DrawLine(System.Drawing.Pens.Black, (float)x, pictureBox1.Height - 5, (float)x, pictureBox1.Height - 12);
                            double time = i / newWav.sampleRate;
                            string duration = WAV.GetDuration(time);
                            System.Drawing.Font drawFont = new System.Drawing.Font("Arial", 8);
                            System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
                            e.Graphics.DrawString(duration, drawFont, drawBrush, x, pictureBox1.Height - 20);
                            drawFont.Dispose();
                            drawBrush.Dispose();
                        }
                        else
                            if (i % newWav.sampleRate == 0)
                            {
                                e.Graphics.DrawLine(System.Drawing.Pens.Black, (float)x, pictureBox1.Height - 5, (float)x, pictureBox1.Height - 8);
                            }

                        e.Graphics.DrawLine(System.Drawing.Pens.Blue, (float)x, (float)(mid + newWav.LeftChData[i]/512), (float)(x + step), (float)(mid + newWav.LeftChData[i + 1]/512));
                        x = (float)(x + step);
                    }
                    if (pos * newWav.LeftChData.Length + newWav.sampleRate * 10 > newWav.LeftChData.Length - 1)
                    {
                        e.Graphics.FillRectangle(System.Drawing.Brushes.Gray, (float)x, pictureBox1.Location.Y, emptyPart - x, pictureBox1.Height);
                    }

                }
                    
            //if (CursorPainted == true)
            //{
            //    g.DrawLine(System.Drawing.Pens.Gray, PickOutFrom.X + MDIParent1.difBtwMainAndWorkSpace, pictureBox1.Location.Y, PickOutFrom.X + MDIParent1.difBtwMainAndWorkSpace, pictureBox1.Height);
            //}
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Location = new Point(MDIParent1.windowX, MDIParent1.windowY);
            MDIParent1.difBtwMainAndWorkSpace -= pictureBox1.Location.X;
            WorkSpaceCreated = true;
            string[] splitted = MDIParent1.FileNames[MDIParent1.currentFile].Split('\\');
            this.Text = splitted[splitted.Length-1];
            difWidth = this.Width - (pictureBox1.Location.X + pictureBox1.Width);
            difHeight = this.Height - (pictureBox1.Location.Y + pictureBox1.Height);
            LoadSoundFile();
            pictureBox1.Show();
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            pictureBox1.Width = this.Width - difWidth;
            pictureBox1.Height = this.Height - difHeight;
            hScrollBar1.Width = this.Width;
            pictureBox1.Refresh();
            if (CursorPainted == true)
            {
                g.DrawLine(System.Drawing.Pens.Gray, PickOutFrom.X + MDIParent1.difBtwMainAndWorkSpace, pictureBox1.Location.Y, PickOutFrom.X + MDIParent1.difBtwMainAndWorkSpace, pictureBox1.Height);
            }
        }

        private void pictureBox1_Resize(object sender, EventArgs e)
        {
           //pictureBox1.Refresh();           
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadSoundFile();
            pictureBox1.Show();
        }

        private void LoadSoundFile ()
        {
                try
                {
                    newWav = WAV.Reading(MDIParent1.FileNames[MDIParent1.currentFile]);
                }
                catch (Exception ex) 
                {
                    MessageBox.Show("Не удалось загрузить файл: " + ex.Message);
                }
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (MDIParent1.closeOrChange == true)
            {
                MDIParent1.currentFile = MDIParent1.FileNames.FindIndex(0, MDIParent1.FileNames.Count, FindName);
                string[] splitted = MDIParent1.FileNames[MDIParent1.currentFile].Split('\\');
                string name = splitted[splitted.Length - 1];
                MDIParent1.FileNames.Remove(MDIParent1.FileNames[MDIParent1.currentFile]);
                MDIParent1.listBox1.Items.Remove(name);
                MDIParent1.visualRepresentationShowing = false;
                if (MDIParent1.currentFile != 0)
                    MDIParent1.currentFile--;
            }
        }
        private void Form1_Activated(object sender, EventArgs e)
        {
            //if (MDIParent1.WorkSpaces.Count > 0)
            //{
               
                MDIParent1.currentFile = MDIParent1.FileNames.FindIndex(0, MDIParent1.FileNames.Count, FindName);
                newWav = WAV.Reading(MDIParent1.FileNames[MDIParent1.currentFile]);
                this.Refresh();
                //count = 0;
                //foreach (Form1 o in MDIParent1.WorkSpaces)
                //{
                //    MDIParent1.currentFile = -1;
                //    for (int i = 0; i < MDIParent1.WorkSpaces.Count; i++)
                //    {
                //        MDIParent1.currentFile = MDIParent1.FileNames.FindIndex(0, MDIParent1.FileNames.Count, FindName);
                //        if (MDIParent1.currentFile!=-1)
                //            break;

                //    }
                //    count++;
                //    newWav = WAV.Reading(MDIParent1.FileNames[MDIParent1.currentFile]);
                //    o.Refresh();
                //}
            //}
        }
        private bool FindName(string FileName)
        {
            if (FileName.Contains(this.Text))
                return true;
            else
                return false;
        }


        public static void MaxHeight(Form1 f)
        {
            f.pictureBox1.Height = f.Height-38;
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            MouseMoves = true;
            PickOutFrom = Control.MousePosition;
            if (CursorPainted == false)
            {
                g.DrawLine(System.Drawing.Pens.Gray, PickOutFrom.X + MDIParent1.difBtwMainAndWorkSpace, pictureBox1.Location.Y, PickOutFrom.X + MDIParent1.difBtwMainAndWorkSpace, pictureBox1.Height);
                pictureBox1.Image = bmp;
                CursorPainted = true;
            }
            else
            {
                g.Clear(Color.White);
                g.DrawLine(System.Drawing.Pens.Gray, PickOutFrom.X + MDIParent1.difBtwMainAndWorkSpace, pictureBox1.Location.Y, PickOutFrom.X + MDIParent1.difBtwMainAndWorkSpace, pictureBox1.Height);
                pictureBox1.Image = bmp;
            }
            
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            MouseMoves = false;
        }
        private void Form1_LocationChanged(object sender, EventArgs e)
        {
            MDIParent1.difBtwMainAndWorkSpace = 0 - this.Location.X - 8;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            PickOutBefore = Control.MousePosition;
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            pos = hScrollBar1.Value/100.0;
            pictureBox1.Refresh();           
        }
    }
}
