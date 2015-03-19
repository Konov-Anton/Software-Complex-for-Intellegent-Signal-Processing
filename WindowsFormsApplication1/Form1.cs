using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;
using System.Media;

namespace WindowsFormsApplication1
{  
    public partial class Form1 : Form
    {
        public static WAV newWav;
        static WAV buffer;
        public static bool WorkSpaceCreated = false;       
        int difWidth;
        int difHeight;
        Graphics g;
        int mid;
        double step;
        Point PickOutFrom=new Point(-1,-1);
        Point PickOutBefore=new Point (-1,-1);
        bool areaIsSelected = false;
        Bitmap bmp;
        double pos = 0;
        bool leftMouseButtonPressed = false;
        int countUnderCursor = 0;
        int lastCountOfArea = 0;
        bool firstlyPlayed = true;
        SoundPlayer playingNow = new SoundPlayer();
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
                    step = (double)(this.Width) / newWav.sampleRate / 10.0;
                    e.Graphics.DrawLine(System.Drawing.Pens.Gray, pictureBox1.Location.X, mid, pictureBox1.Width, mid);
                    e.Graphics.DrawLine(System.Drawing.Pens.Black, pictureBox1.Location.X, pictureBox1.Height - 5, pictureBox1.Width, pictureBox1.Height - 5);
                    float x = (float)pictureBox1.Location.X;
                    float xTimeAxis = (float)pictureBox1.Location.X;
                    int upperBound;
                    int emptyPart=0;
                    if (pos * newWav.LeftChData.Length + newWav.sampleRate*10 > newWav.LeftChData.Length - 1)
                    {
                        upperBound = newWav.LeftChData.Length - 1;
                        emptyPart = (int)(pos * newWav.LeftChData.Length + newWav.sampleRate * 10);
                       
                    }
                    else
                        upperBound = (int)(pos * newWav.LeftChData.Length + newWav.sampleRate * 10);
                    bool paint = true;
                    for (int i = (int)(pos * newWav.LeftChData.Length), l = (int)(pos * newWav.LeftChData.Length); paint; i = i + 4, l=l+1)
                        {
                            if (l % (newWav.sampleRate * 10) == 0)
                            {
                                e.Graphics.DrawLine(System.Drawing.Pens.Black, (float)xTimeAxis, pictureBox1.Height - 5, (float)xTimeAxis, pictureBox1.Height - 12);
                                double time = l / newWav.sampleRate;
                                string duration = WAV.GetDuration(time);
                                System.Drawing.Font drawFont = new System.Drawing.Font("Arial", 8);
                                System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
                                e.Graphics.DrawString(duration, drawFont, drawBrush, xTimeAxis, pictureBox1.Height - 20);
                                drawFont.Dispose();
                                drawBrush.Dispose();
                            }
                            else
                                if (l % newWav.sampleRate == 0)
                                {
                                    e.Graphics.DrawLine(System.Drawing.Pens.Black, (float)xTimeAxis, pictureBox1.Height - 5, (float)xTimeAxis, pictureBox1.Height - 8);
                                }
                            if (i < upperBound - 3)
                            {
                                e.Graphics.DrawLine(System.Drawing.Pens.Blue, (float)x, (float)(mid + newWav.LeftChData[i] / 512), (float)(x + 4 * step), (float)(mid + newWav.LeftChData[i + 4] / 512));
                                x = (float)(x + 4 * step);
                            }
                            else
                                if (l >= upperBound)
                                    paint = false;
                            xTimeAxis = (float)(xTimeAxis + step);
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
            g.DrawLine(System.Drawing.Pens.Gray, PickOutFrom.X + MDIParent1.difBtwMainAndWorkSpace - MDIParent1.listView1.Width, pictureBox1.Location.Y, PickOutFrom.X + MDIParent1.difBtwMainAndWorkSpace - MDIParent1.listView1.Width, pictureBox1.Height);
            pictureBox1.Image = bmp;
            pictureBox1.Show();
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            pictureBox1.Width = this.Width - difWidth;
            pictureBox1.Height = this.Height - difHeight;
            hScrollBar1.Width = this.Width;
            pictureBox1.Refresh();
            g.DrawLine(System.Drawing.Pens.Gray, PickOutFrom.X + MDIParent1.difBtwMainAndWorkSpace, pictureBox1.Location.Y, PickOutFrom.X + MDIParent1.difBtwMainAndWorkSpace, pictureBox1.Height);
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
                MDIParent1.listView1.Items.Remove(MDIParent1.listView1.Items[MDIParent1.currentFile]);
                MDIParent1.visualRepresentationShowing = false;
                if (MDIParent1.currentFile != 0)
                    MDIParent1.currentFile--;
                if (MDIParent1.FileNames.Count == 0)
                {
                    MDIParent1.listView1.Hide();
                    MDIParent1.fileContainerNotCreatedYet = true;
                }
                if (MDIParent1.FileNames.Count == 0)
                    MDIParent1.closeToolStripMenuItem.Enabled = false;
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
            //PickOutFrom = Control.MousePosition;
            //if (CursorPainted == false)
            //{
            //    g.DrawLine(System.Drawing.Pens.Gray, PickOutFrom.X + MDIParent1.difBtwMainAndWorkSpace - MDIParent1.listView1.Width, pictureBox1.Location.Y, PickOutFrom.X + MDIParent1.difBtwMainAndWorkSpace - MDIParent1.listView1.Width, pictureBox1.Height);
            //    pictureBox1.Image = bmp;
            //    CursorPainted = true;
            //}
            //else
            //{
            //    g.Clear(Color.White);
            //    g.DrawLine(System.Drawing.Pens.Gray, PickOutFrom.X + MDIParent1.difBtwMainAndWorkSpace - MDIParent1.listView1.Width, pictureBox1.Location.Y, PickOutFrom.X + MDIParent1.difBtwMainAndWorkSpace - MDIParent1.listView1.Width, pictureBox1.Height);
            //    pictureBox1.Image = bmp;
            //}
            
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            leftMouseButtonPressed = false;
            pictureBox1.Image = bmp;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            pictureBox1.Focus(); 
            leftMouseButtonPressed = true;
            PickOutFrom = Control.MousePosition;
            g.Clear(Color.White);
            g.DrawLine(System.Drawing.Pens.Gray, PickOutFrom.X + MDIParent1.difBtwMainAndWorkSpace - MDIParent1.listView1.Width, pictureBox1.Location.Y, PickOutFrom.X + MDIParent1.difBtwMainAndWorkSpace - MDIParent1.listView1.Width, pictureBox1.Height);
            countUnderCursor = (int)(pos * newWav.LeftChData.Length) + (int)((PickOutFrom.X + MDIParent1.difBtwMainAndWorkSpace - MDIParent1.listView1.Width - pictureBox1.Location.X) / step);
            areaIsSelected = false;
            firstlyPlayed = true;
            OnMouseMove(e);
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            g.Clear(Color.White);
            pos = hScrollBar1.Value/100.0;
            pictureBox1.Refresh();           
        }
        private void Form1_LocationChanged(object sender, EventArgs e)
        {
            MDIParent1.difBtwMainAndWorkSpace = 0 - this.Location.X - 8;
        }



        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
                if (MDIParent1.changesHaveBeenMade == true)
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
                        WAV.Writing(WAV.Reading(MDIParent1.FileNames[MDIParent1.currentFile]), MDIParent1.FileNames[MDIParent1.currentFile]);
                        e.Cancel = false;
                    }
                    else
                    {
                        e.Cancel = true;
                    }
                }
                playingNow.Stop();
                if (System.IO.File.Exists(Application.StartupPath + @"\" + "buffer"))
                {
                    try
                    {
                        System.IO.File.Delete(Application.StartupPath + @"\" + "buffer");
                    }
                    catch (System.IO.IOException ex)
                    {
                        Console.WriteLine(ex.Message);
                        return;
                    }
                }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (leftMouseButtonPressed == true)
            {
                if (areaIsSelected == false)
                    g.Clear(Color.White);
                PickOutBefore = Control.MousePosition;
                if (PickOutBefore.X > PickOutFrom.X)
                {
                    g.FillRectangle(System.Drawing.Brushes.Black, PickOutFrom.X + MDIParent1.difBtwMainAndWorkSpace - MDIParent1.listView1.Width,
                        pictureBox1.Location.Y,
                        Math.Abs(PickOutBefore.X - PickOutFrom.X),
                        pictureBox1.Height - 20);
                    pictureBox1.Image = bmp;
                }
                else
                {
                    g.FillRectangle(System.Drawing.Brushes.Black, PickOutBefore.X + MDIParent1.difBtwMainAndWorkSpace - MDIParent1.listView1.Width,
                        pictureBox1.Location.Y,
                        Math.Abs(PickOutBefore.X - PickOutFrom.X),
                        pictureBox1.Height - 20);
                    pictureBox1.Image = bmp;
                }
                lastCountOfArea = (int)((PickOutBefore.X + MDIParent1.difBtwMainAndWorkSpace - MDIParent1.listView1.Width - pictureBox1.Location.X) / step);
                areaIsSelected = true;                
            }
        }

        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (countUnderCursor < newWav.LeftChData.Length)
            {
                if (firstlyPlayed == true)
                {
                    TrimWavFile(countUnderCursor, newWav.LeftChData.Length);
                    playingNow.SoundLocation = Application.StartupPath + @"\" + "buffer";
                    playingNow.Load();
                    firstlyPlayed = false;
                }
                playingNow.Play();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            playingNow.Stop();
        }

 
        public static void TrimWavFile(int startPos, int endPos)
        {
            buffer = new WAV();
            buffer.LeftChData = new double[endPos - startPos];
            buffer.wavData = new byte[(endPos-startPos)*2];
            buffer.chunkID = newWav.chunkID;
            buffer.riffType = newWav.riffType;
            buffer.fmtID = newWav.fmtID;
            buffer.fmtSize = newWav.fmtSize;
            buffer.fmtCode = newWav.fmtCode;
            buffer.channels = newWav.channels;
            buffer.sampleRate = newWav.sampleRate;
            buffer.fmtAvgBPS = newWav.fmtAvgBPS;
            buffer.fmtBlockAlign = newWav.fmtBlockAlign;
            buffer.bitDepth = newWav.bitDepth;
            buffer.dataID = newWav.dataID;
            if (startPos % 2 == 1)
                startPos++;
            Array.Copy(newWav.LeftChData, startPos, buffer.LeftChData, 0, endPos - startPos-1);
            Array.Copy(newWav.wavData, startPos*2, buffer.wavData, 0, (endPos - startPos)*2-1);
            buffer.dataSize = buffer.LeftChData.Length * buffer.bitDepth / 8;
            buffer.fileSize = (uint)(buffer.dataSize + 36);
            WAV.Writing2(buffer, Application.StartupPath + @"\" + "buffer");
        }  
    }
}
