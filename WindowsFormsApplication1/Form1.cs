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
using System.Timers;
using System.Diagnostics;
using System.Threading;

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
        int cursorPositionOnAxis=0;
        Point PickOutBefore=new Point (-1,-1);
        bool movementFromRightToLeft = false;
        bool areaIsSelected = false;
        int areaPaintingMode = 0;
        Bitmap bmp;
        double pos = 0;
        bool leftMouseButtonPressed = false;
        int countUnderCursor = 0;
        int countUnderCursorEnd = 0;
        int lastCountOfArea = 0;
        bool firstlyPlayed = true;
        SoundPlayer playingNow = new SoundPlayer();
        bool isPlayingNow = false;
        int iii = 0;
        int cursorPos=0;
        System.Timers.Timer timer = new System.Timers.Timer();
        int scrollBarDif;
        bool scrollingLimitIsReached = false;
        int playingPosition = 0;
        int endOfPlaying;
        bool stubbornPlacePassed = false;
        int A, B;
        int residue = 0;
        Size formSize;
        double verticalScale = 1;
        bool mouseWheel = false;
        int timeScaleMode = 0;
        int scalingRatio = 1;
        int skipNcounts = 4;
        double durOfFile;
        bool paintBoldPoints = false;
        int TimeAxisDetailization = 1;
        int something = 0;
        int something2 = 0;
        bool positionChanged = false;
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
            if (MDIParent1.FileNames.Count > 0)
            {
                if (MDIParent1.FileNames[MDIParent1.currentFile] != "")
                {
                    int countsUnderScrollbar = (int)(pos * newWav.LeftChData.Length);
                    if (areaPaintingMode == 1)
                    {
                        float length1 = (float)((countUnderCursorEnd - countUnderCursor) * step);
                        g.Clear(Color.White);
                        e.Graphics.FillRectangle(System.Drawing.Brushes.Black, (float)(countUnderCursor * step - countsUnderScrollbar * step),
                        pictureBox1.Location.Y,
                        length1,
                        pictureBox1.Height - 20);
                    }
                    if (areaPaintingMode == 2)
                    {
                        float length1 = (float)((countUnderCursor - countUnderCursorEnd) * step);
                        g.Clear(Color.White);
                        e.Graphics.FillRectangle(System.Drawing.Brushes.Black, (float)(countUnderCursorEnd * step - countsUnderScrollbar * step),
                        pictureBox1.Location.Y,
                        length1,
                        pictureBox1.Height - 20);
                    }
                    mid = pictureBox1.Height / 2;
                    double dur = Math.Round((double)Form1.newWav.wavData.Length / (double)Form1.newWav.fmtAvgBPS, 3);
                    switch (timeScaleMode)
                    {
                        case 0:
                            skipNcounts = 4;
                            TimeAxisDetailization = 1;
                            break;
                        case 1:
                            skipNcounts = 4;
                            TimeAxisDetailization = 2;
                            break;
                        case 2:
                            skipNcounts = 4;
                            TimeAxisDetailization = 3;
                            break;
                        case 3:
                            skipNcounts = 2;
                            TimeAxisDetailization = 4;
                            break;
                        case 4:
                            skipNcounts = 1;
                            TimeAxisDetailization = 5;
                            break;
                        case 5:
                            skipNcounts = 1;
                            TimeAxisDetailization = 6;
                            break;
                        case 6:
                            skipNcounts = 1;
                            TimeAxisDetailization = 7;
                            break;
                        case 7:
                            skipNcounts = 1;
                            TimeAxisDetailization = 8;
                            break;
                        case 8:
                            skipNcounts = 1;
                            TimeAxisDetailization = 9;
                            break;
                        case 9:
                            skipNcounts = 1;
                            TimeAxisDetailization = 10;
                            break;
                        case 10:
                            skipNcounts = 1;
                            TimeAxisDetailization = 10;
                            break;
                        case 11:
                            skipNcounts = 1;
                            TimeAxisDetailization = 10;
                            break;
                        case 12:
                            skipNcounts = 1;
                            TimeAxisDetailization = 11;
                            break;
                        case 13:
                            skipNcounts = 1;
                            TimeAxisDetailization = 12;
                            break;

                    }
                    step = (double)(this.Width) * scalingRatio / newWav.sampleRate / 10.0;
                    e.Graphics.DrawLine(System.Drawing.Pens.Gray, pictureBox1.Location.X, mid, pictureBox1.Width, mid);
                    e.Graphics.DrawLine(System.Drawing.Pens.Black, pictureBox1.Location.X, pictureBox1.Height - 5, pictureBox1.Width, pictureBox1.Height - 5);
                    float x = (float)pictureBox1.Location.X;
                    float xTimeAxis = (float)pictureBox1.Location.X;
                    int upperBound;
                    int emptyPart = 0;

                    if (pos * newWav.LeftChData.Length + newWav.sampleRate * 10 / scalingRatio > newWav.LeftChData.Length - 1)
                    {
                        upperBound = newWav.LeftChData.Length - 1;
                        emptyPart = (int)(pos * newWav.LeftChData.Length + newWav.sampleRate * 10 / scalingRatio);
                    }
                    else
                    {
                        upperBound = (int)(pos * newWav.LeftChData.Length + newWav.sampleRate * 10 / scalingRatio);
                    }
                    bool paint = true;

                    if (timeScaleMode < 5)
                    {
                        if (mouseWheel == true)
                        {
                            pos = (countUnderCursor - (PickOutFrom.X + MDIParent1.difBtwMainAndWorkSpace - MDIParent1.listView1.Width) / step) / newWav.LeftChData.Length;
                        }
                        for (int i = (int)(pos * newWav.LeftChData.Length), l = (int)(pos * newWav.LeftChData.Length); paint; i = i + skipNcounts, l = l + 1)
                        {
                            if (l == countUnderCursor)
                            {
                                e.Graphics.DrawLine(System.Drawing.Pens.Gray, (float)((countUnderCursor - (int)(pos * newWav.LeftChData.Length)) * step),
                                    pictureBox1.Location.Y,
                                    (float)((countUnderCursor - pos * newWav.LeftChData.Length) * step),
                                    pictureBox1.Height);
                            }
                            if (l % (newWav.sampleRate * 5) == 0)
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
                                    if (TimeAxisDetailization >= 2)
                                    {
                                        double time = l / newWav.sampleRate;
                                        string duration = WAV.GetDuration(time);
                                        System.Drawing.Font drawFont = new System.Drawing.Font("Arial", 8);
                                        System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
                                        e.Graphics.DrawString(duration, drawFont, drawBrush, xTimeAxis, pictureBox1.Height - 20);
                                        drawFont.Dispose();
                                        drawBrush.Dispose();
                                    }
                                }
                            if (TimeAxisDetailization == 3)
                            {
                                if (l % (newWav.sampleRate / 5) == 0)
                                {
                                    e.Graphics.DrawLine(System.Drawing.Pens.Black, (float)xTimeAxis, pictureBox1.Height - 5, (float)xTimeAxis, pictureBox1.Height - 8);
                                }
                            }
                            if (TimeAxisDetailization == 4 && l % (newWav.sampleRate / 20) == 0)
                            {
                                e.Graphics.DrawLine(System.Drawing.Pens.Black, (float)xTimeAxis, pictureBox1.Height - 5, (float)xTimeAxis, pictureBox1.Height - 8);
                                if (l % (newWav.sampleRate / 2) == 0 && (l / (newWav.sampleRate / 2) % 2 == 1))
                                {
                                    double time = (double)l / newWav.sampleRate / 10;
                                    string duration = WAV.GetDuration(time);
                                    System.Drawing.Font drawFont = new System.Drawing.Font("Arial", 6);
                                    System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
                                    e.Graphics.DrawString(duration, drawFont, drawBrush, xTimeAxis, pictureBox1.Height - 20);
                                    drawFont.Dispose();
                                    drawBrush.Dispose();

                                }
                            }

                            if ((TimeAxisDetailization == 5) && l % (newWav.sampleRate / 20) == 0)
                            {
                                e.Graphics.DrawLine(System.Drawing.Pens.Black, (float)xTimeAxis, pictureBox1.Height - 5, (float)xTimeAxis, pictureBox1.Height - 8);
                                if (l % (newWav.sampleRate / 5) == 0 && (l / (newWav.sampleRate / 5) % 5 != 0))
                                {
                                    double time = (double)l / newWav.sampleRate / 10;
                                    string duration = WAV.GetDuration(time);
                                    System.Drawing.Font drawFont = new System.Drawing.Font("Arial", 6);
                                    System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
                                    e.Graphics.DrawString(duration, drawFont, drawBrush, xTimeAxis, pictureBox1.Height - 20);
                                    drawFont.Dispose();
                                    drawBrush.Dispose();
                                }
                            }

                            if (i < upperBound - skipNcounts + 1)
                            {
                                e.Graphics.DrawLine(System.Drawing.Pens.Blue, (float)x, (float)(mid + newWav.LeftChData[i] / 512 * verticalScale),
                                    (float)(x + skipNcounts * step), (float)(mid + newWav.LeftChData[i + skipNcounts] / 512 * verticalScale));

                                x = (float)(x + skipNcounts * step);
                            }
                            else
                                if (l >= upperBound)
                                    paint = false;
                            xTimeAxis = (float)(xTimeAxis + step);
                        }
                        if (pos * newWav.LeftChData.Length + newWav.sampleRate * 10 / scalingRatio > newWav.LeftChData.Length - 1)
                        {
                            e.Graphics.FillRectangle(System.Drawing.Brushes.Gray, (float)x, pictureBox1.Location.Y, emptyPart - x, pictureBox1.Height);
                        }
                    }
                    if (timeScaleMode > 4)
                    {
                        int begin;
                        if (mouseWheel == true)
                        {
                            pos = (countUnderCursor - (PickOutFrom.X + MDIParent1.difBtwMainAndWorkSpace - MDIParent1.listView1.Width) / step) / newWav.LeftChData.Length;
                        }

                        e.Graphics.DrawLine(System.Drawing.Pens.Gray, pictureBox1.Location.X, mid, pictureBox1.Width, mid);
                        if (positionChanged == false)
                        {
                            begin = (int)(pos * newWav.LeftChData.Length) + something - something2;
                        }
                        else
                        {
                            begin = (int)(pos * newWav.LeftChData.Length);
                            something = 0;
                        }
                        int end;
                        if ((int)(begin + newWav.sampleRate * 10 / scalingRatio) > newWav.LeftChData.Length)
                            end = newWav.LeftChData.Length - 1;
                        else
                            end = (int)(begin + newWav.sampleRate * 10 / scalingRatio);
                        for (int i = begin, l = begin; i < end; i++, l++)
                        {
                            if (l == countUnderCursor)
                            {
                                e.Graphics.DrawLine(System.Drawing.Pens.Gray, (float)((countUnderCursor - (int)(pos * newWav.LeftChData.Length)) * step),
                                    pictureBox1.Location.Y,
                                    (float)((countUnderCursor - (int)pos * newWav.LeftChData.Length) * step),
                                    pictureBox1.Height);
                            }
                            if (l % (newWav.sampleRate * 5) == 0)
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
                                    if (TimeAxisDetailization >= 2)
                                    {
                                        double time = l / newWav.sampleRate;
                                        string duration = WAV.GetDuration(time);
                                        System.Drawing.Font drawFont = new System.Drawing.Font("Arial", 8);
                                        System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
                                        e.Graphics.DrawString(duration, drawFont, drawBrush, xTimeAxis, pictureBox1.Height - 20);
                                        drawFont.Dispose();
                                        drawBrush.Dispose();
                                    }
                                }

                            if (TimeAxisDetailization == 6 && l % (newWav.sampleRate / 20) == 0)
                            {
                                e.Graphics.DrawLine(System.Drawing.Pens.Black, (float)xTimeAxis, pictureBox1.Height - 5, (float)xTimeAxis, pictureBox1.Height - 8);
                                if (l % (newWav.sampleRate / 5) == 0 && (l / (newWav.sampleRate / 5) % 5 != 0))
                                {
                                    double time = (double)l / newWav.sampleRate / 10;
                                    string duration = WAV.GetDuration(time);
                                    System.Drawing.Font drawFont = new System.Drawing.Font("Arial", 6);
                                    System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
                                    e.Graphics.DrawString(duration, drawFont, drawBrush, xTimeAxis, pictureBox1.Height - 20);
                                    drawFont.Dispose();
                                    drawBrush.Dispose();
                                }
                            }
                            if (TimeAxisDetailization == 6 && l % (newWav.sampleRate / 20) == 0)
                            {
                                if (l % (newWav.sampleRate / 10) == 0 && (l / (newWav.sampleRate / 10) % 10 != 0))
                                {
                                    double time = (double)l / newWav.sampleRate / 10;
                                    string duration = WAV.GetDuration(time);
                                    System.Drawing.Font drawFont = new System.Drawing.Font("Arial", 6);
                                    System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
                                    e.Graphics.DrawString(duration, drawFont, drawBrush, xTimeAxis, pictureBox1.Height - 20);
                                    drawFont.Dispose();
                                    drawBrush.Dispose();
                                }
                            }
                            if (TimeAxisDetailization == 7 && l % (newWav.sampleRate / 20) == 0)
                            {
                                e.Graphics.DrawLine(System.Drawing.Pens.Black, (float)xTimeAxis, pictureBox1.Height - 5, (float)xTimeAxis, pictureBox1.Height - 8);
                                if (l % (newWav.sampleRate / 20) == 0 && (l / (newWav.sampleRate / 20) % 20 != 0))
                                {
                                    double time = (double)l / newWav.sampleRate / 10;
                                    string duration = WAV.GetDuration(time);
                                    System.Drawing.Font drawFont = new System.Drawing.Font("Arial", 6);
                                    System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
                                    e.Graphics.DrawString(duration, drawFont, drawBrush, xTimeAxis, pictureBox1.Height - 20);
                                    drawFont.Dispose();
                                    drawBrush.Dispose();
                                }
                            }
                            if (TimeAxisDetailization == 8 && l % (newWav.sampleRate / 100) == 0)
                            {
                                e.Graphics.DrawLine(System.Drawing.Pens.Black, (float)xTimeAxis, pictureBox1.Height - 5, (float)xTimeAxis, pictureBox1.Height - 8);
                                if (l % (newWav.sampleRate / 20) == 0 && (l / (newWav.sampleRate / 20) % 20 != 0))
                                {
                                    double time = (double)l / newWav.sampleRate / 10;
                                    string duration = WAV.GetDuration(time);
                                    System.Drawing.Font drawFont = new System.Drawing.Font("Arial", 6);
                                    System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
                                    e.Graphics.DrawString(duration, drawFont, drawBrush, xTimeAxis, pictureBox1.Height - 20);
                                    drawFont.Dispose();
                                    drawBrush.Dispose();
                                }
                            }
                            if ((TimeAxisDetailization == 9 || TimeAxisDetailization == 10) && l % (newWav.sampleRate / 100) == 0)
                            {
                                e.Graphics.DrawLine(System.Drawing.Pens.Black, (float)xTimeAxis, pictureBox1.Height - 5, (float)xTimeAxis, pictureBox1.Height - 8);
                                if (l % (newWav.sampleRate / 100) == 0 && (l / (newWav.sampleRate / 100) % 100 != 0))
                                {
                                    double time = (double)l / newWav.sampleRate / 10;
                                    string duration = WAV.GetDuration(time);
                                    System.Drawing.Font drawFont = new System.Drawing.Font("Arial", 6);
                                    System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
                                    e.Graphics.DrawString(duration, drawFont, drawBrush, xTimeAxis, pictureBox1.Height - 20);
                                    drawFont.Dispose();
                                    drawBrush.Dispose();
                                }
                            }
                            //if (Math.Abs((int)(pos * newWav.LeftChData.Length + l) - cursorPositionOnAxis) < 1)
                            //if (l == cursorPositionOnAxis)
                            //{
                            //if (PickOutFrom.X + MDIParent1.difBtwMainAndWorkSpace - MDIParent1.listView1.Width > pos * newWav.LeftChData.Length * step)
                            //{
                            //    e.Graphics.DrawLine(System.Drawing.Pens.Gray,
                            //        (float)(PickOutFrom.X + MDIParent1.difBtwMainAndWorkSpace - MDIParent1.listView1.Width - pos * newWav.LeftChData.Length * step),
                            //        pictureBox1.Location.Y,
                            //        (float)(PickOutFrom.X + MDIParent1.difBtwMainAndWorkSpace - MDIParent1.listView1.Width - pos * newWav.LeftChData.Length * step),
                            //        pictureBox1.Height);
                            //    pictureBox1.Image = bmp;

                            //}
                            //else
                            //{
                            //    e.Graphics.DrawLine(System.Drawing.Pens.Gray,
                            //        (float)(PickOutFrom.X + MDIParent1.difBtwMainAndWorkSpace - MDIParent1.listView1.Width + pos * newWav.LeftChData.Length * step),
                            //        pictureBox1.Location.Y,
                            //        (float)(PickOutFrom.X + MDIParent1.difBtwMainAndWorkSpace - MDIParent1.listView1.Width + pos * newWav.LeftChData.Length * step),
                            //        pictureBox1.Height);
                            //    pictureBox1.Image = bmp;
                            //}
                            //}
                            e.Graphics.DrawLine(System.Drawing.Pens.Blue, (float)x, (float)(mid + newWav.LeftChData[i] / 512 * verticalScale),
                                (float)(x + skipNcounts * step), (float)(mid + newWav.LeftChData[i + skipNcounts] / 512 * verticalScale));

                            if (TimeAxisDetailization == 11)
                            {
                                RectangleF pointValue = new RectangleF((float)(x - 1), (float)(mid + newWav.LeftChData[i] / 512 * verticalScale - 1), 2f, 2f);
                                e.Graphics.FillRectangle(System.Drawing.Brushes.Blue, pointValue);
                            }

                            if (TimeAxisDetailization == 12)
                            {
                                RectangleF pointValue = new RectangleF((float)(x - 2), (float)(mid + newWav.LeftChData[i] / 512 * verticalScale - 2), 4f, 4f);
                                e.Graphics.FillRectangle(System.Drawing.Brushes.Blue, pointValue);
                            }

                            x = (float)(x + skipNcounts * step);

                            xTimeAxis = (float)(xTimeAxis + step);
                        }
                        emptyPart = (int)(pos * newWav.LeftChData.Length + newWav.sampleRate * 10 / scalingRatio / 3);
                        if (pos * newWav.LeftChData.Length + (int)(newWav.sampleRate * 10 / scalingRatio) > newWav.LeftChData.Length - 1)
                        {
                            e.Graphics.FillRectangle(System.Drawing.Brushes.Gray, (float)x, pictureBox1.Location.Y, emptyPart - x, pictureBox1.Height);
                        }
                    }
                    if (mouseWheel == true)
                    {
                        g.Clear(Color.White);
                        g.DrawLine(System.Drawing.Pens.Gray, (float)(countUnderCursor * step - (int)(pos * newWav.LeftChData.Length) * step),
                            pictureBox1.Location.Y,
                            (float)(countUnderCursor * step - (int)(pos * newWav.LeftChData.Length) * step),
                            pictureBox1.Height - 5);
                        pictureBox1.Image = bmp;
                    }
                    if (areaIsSelected == true && mouseWheel == true)
                    {
                        g.Clear(Color.White);
                        if (PickOutBefore.X > PickOutFrom.X)
                        {
                            float length = (float)((countUnderCursorEnd - countUnderCursor) * step);
                            if (movementFromRightToLeft == true)
                            {
                                g.Clear(Color.White);
                                g.FillRectangle(System.Drawing.Brushes.Black, (float)(countUnderCursor * step - (int)(pos * newWav.LeftChData.Length) * step),
                                    pictureBox1.Location.Y,
                                    length,
                                    pictureBox1.Height - 20);
                                pictureBox1.Image = bmp;
                            }
                            else
                            {
                                g.FillRectangle(System.Drawing.Brushes.Black, (float)(countUnderCursor * step - (int)(pos * newWav.LeftChData.Length) * step),
                                    pictureBox1.Location.Y,
                                    length,
                                    pictureBox1.Height - 20);
                                movementFromRightToLeft = true;
                                pictureBox1.Image = bmp;
                            }

                        }
                        else
                        {
                            float length = (float)((countUnderCursor - countUnderCursorEnd) * step);
                            if (movementFromRightToLeft == false)
                            {
                                g.FillRectangle(System.Drawing.Brushes.Black, (float)(countUnderCursorEnd * step - (int)(pos * newWav.LeftChData.Length) * step),
                                    pictureBox1.Location.Y,
                                    length,
                                    pictureBox1.Height - 20);
                                pictureBox1.Image = bmp;
                                movementFromRightToLeft = true;
                            }
                            else
                            {
                                g.Clear(Color.White);
                                g.FillRectangle(System.Drawing.Brushes.Black, (float)(countUnderCursorEnd * step - (int)(pos * newWav.LeftChData.Length) * step),
                                    pictureBox1.Location.Y,
                                    length,
                                    pictureBox1.Height - 20);
                                pictureBox1.Image = bmp;
                            }
                        }
                    }
                    mouseWheel = false;
                    if (Form5.cantBlockWindow == false)
                        pictureBox1.Focus();
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
            formSize = this.Size;
            string[] splitted = MDIParent1.FileNames[MDIParent1.currentFile].Split('\\');
            this.Text = splitted[splitted.Length-1];
            difWidth = this.Width - (pictureBox1.Location.X + pictureBox1.Width);
            difHeight = this.Height - (pictureBox1.Location.Y + pictureBox1.Height);
            LoadSoundFile();
            durOfFile = (double)(newWav.LeftChData.Length / newWav.sampleRate);           
            scrollBarDif = (int)(1000/durOfFile);
            hScrollBar1.LargeChange = scrollBarDif-1;
            g.DrawLine(System.Drawing.Pens.Gray, pictureBox1.Location.X, pictureBox1.Location.Y, pictureBox1.Location.X, pictureBox1.Height);
            pictureBox1.Image = bmp;
            pictureBox1.Show();
            if (MDIParent1.FileInfoCheckState())
            {
                MDIParent1.newMDIChildForm2.Dispose();
                MDIParent1.newMDIChildForm2 = new Form2();
                MDIParent1.newMDIChildForm2.DesktopLocation = new Point(300, 300);
                MDIParent1.newMDIChildForm2.MdiParent = MDIParent1.ActiveForm;
                MDIParent1.newMDIChildForm2.Show();
                MDIParent1.SetFileInfoCheckedValue();
            }
            if (MDIParent1.spectrumCheckState())
            {
                MDIParent1.spectrumWindow.Dispose();
                double[] normalizedData = WAV.GetNormalizedData(Form1.newWav);
                Complex[] forFft = Fourier.PrepareToFFT(normalizedData);
                Complex[] fftCoefs = Fourier.FFT(forFft);
                double[] magn = Fourier.GetMagnitude(fftCoefs);
                MDIParent1.globalMagnitudes = new double[magn.Length / 2];
                Array.Copy(magn, MDIParent1.globalMagnitudes, MDIParent1.globalMagnitudes.Length);
                for (int i = 0; i < MDIParent1.globalMagnitudes.Length; i++)                       // нормировка (нужна ли)
                {
                    MDIParent1.globalMagnitudes[i] = (double)(2 * MDIParent1.globalMagnitudes[i] / 2048);
                }
                MDIParent1.globalMagnitudes[0] /= 2;
                MDIParent1.spectrumWindow = new Form4();
                MDIParent1.spectrumWindow.DesktopLocation = new Point(300, 300);
                MDIParent1.spectrumWindow.MdiParent = MDIParent1.ActiveForm;
                MDIParent1.spectrumWindow.Show();
                MDIParent1.SetSpectrumCheckedValue();
            }

        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            verticalScale = (double)(this.Size.Height) / formSize.Height;
            pictureBox1.Width = this.Width - difWidth;
            pictureBox1.Height = this.Height - difHeight;
            hScrollBar1.Width = this.Width;
            pictureBox1.Refresh();
            g.DrawLine(System.Drawing.Pens.Gray, PickOutFrom.X + MDIParent1.difBtwMainAndWorkSpace - MDIParent1.listView1.Width, pictureBox1.Location.Y, 
                PickOutFrom.X + MDIParent1.difBtwMainAndWorkSpace - MDIParent1.listView1.Width , pictureBox1.Height);
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
                if (MDIParent1.spectrumWindow != null)
                MDIParent1.spectrumWindow.Close();
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
            if (Form5.cantBlockWindow == false)
                pictureBox1.Focus();
            if (MDIParent1.Exit == false)
            {
                MDIParent1.currentFile = MDIParent1.FileNames.FindIndex(0, MDIParent1.FileNames.Count, FindName);
                newWav = WAV.Reading(MDIParent1.FileNames[MDIParent1.currentFile]);
 //               this.Refresh();
            }
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

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            leftMouseButtonPressed = false;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            pictureBox1.Focus();
            areaPaintingMode = 0;
            leftMouseButtonPressed = true;
            PickOutFrom = Control.MousePosition;
            cursorPositionOnAxis = CountUnderCursor(PickOutFrom.X);
            textBox1.Text += CountUnderCursor(PickOutFrom.X) + "   ";
            PickOutBefore = new Point(-1, -1);
            g.Clear(Color.White);

            countUnderCursor = CountUnderCursor(PickOutFrom.X);
            g.DrawLine(System.Drawing.Pens.Gray, (float)(countUnderCursor * step - (int)(pos * newWav.LeftChData.Length)*step), pictureBox1.Location.Y,
            (float)(countUnderCursor * step - (int)(pos * newWav.LeftChData.Length) * step), pictureBox1.Height);

            pictureBox1.Image = bmp;
            areaIsSelected = false;
            firstlyPlayed = true;
            if (isPlayingNow == true)
            {
                button1.PerformClick();
            }
            OnMouseMove(e);
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
                isPlayingNow = false;
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
                countUnderCursorEnd = CountUnderCursor(PickOutBefore.X);
                int countsUnderScrollbar = (int)(pos * newWav.LeftChData.Length);
                if (PickOutBefore.X > PickOutFrom.X)
                {
                    areaPaintingMode = 1;
                    float length = (float)((countUnderCursorEnd - countUnderCursor) * step);
                    if (movementFromRightToLeft == true)
                    {                       
                        g.Clear(Color.White);
                        g.FillRectangle(System.Drawing.Brushes.Black, (float)(countUnderCursor * step - countsUnderScrollbar * step),
                            pictureBox1.Location.Y,
                            length,
                            pictureBox1.Height - 20);
                        pictureBox1.Image = bmp;
                    }
                    else
                    {
                        g.FillRectangle(System.Drawing.Brushes.Black, (float)(countUnderCursor * step - countsUnderScrollbar * step),
                            pictureBox1.Location.Y,
                            length,
                            pictureBox1.Height - 20);
                        movementFromRightToLeft = true;
                        pictureBox1.Image = bmp;
                    }
                }
                else
                {
                    areaPaintingMode = 2;
                    float length = (float)((countUnderCursor - countUnderCursorEnd) * step);
                    if (movementFromRightToLeft == false)
                    {
                        g.FillRectangle(System.Drawing.Brushes.Black, (float)(countUnderCursorEnd * step - countsUnderScrollbar * step),
                            pictureBox1.Location.Y,
                            length,
                            pictureBox1.Height - 20);
                        pictureBox1.Image = bmp;
                        movementFromRightToLeft = true;
                    }
                    else 
                    {
                        g.Clear(Color.White);
                        g.FillRectangle(System.Drawing.Brushes.Black, (float)(countUnderCursorEnd * step - countsUnderScrollbar * step),
                            pictureBox1.Location.Y,
                            length,
                            pictureBox1.Height - 20);
                        pictureBox1.Image = bmp;
                    }
                }

                lastCountOfArea = (int)((PickOutBefore.X + MDIParent1.difBtwMainAndWorkSpace - MDIParent1.listView1.Width - pictureBox1.Location.X) / step);
                areaIsSelected = true;
            }
        }

        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            timeScaleMode += e.Delta / 120;
            if (timeScaleMode > 13)
                timeScaleMode = 13;
            else
                if (timeScaleMode < 0)
                    timeScaleMode = 0;
                else
                    mouseWheel = true;
            scalingRatio = (int)Math.Pow(2, timeScaleMode);
            scrollBarDif = (int)(1000 / durOfFile / scalingRatio);
            if (timeScaleMode > 7)
                hScrollBar1.LargeChange = 1;
            else
                if (timeScaleMode > 3)
                    hScrollBar1.LargeChange = scrollBarDif + 1;
                else
                    hScrollBar1.LargeChange = scrollBarDif;

            something = 0;
            textBox1.Text += timeScaleMode + " ";
            textBox1.Text += scrollBarDif + "   ";
            pictureBox1.Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (countUnderCursor < newWav.LeftChData.Length)
            {
                iii = 0;
                A = CountUnderCursor(pictureBox1.Width);
                endOfPlaying = newWav.LeftChData.Length;
                if (PickOutBefore.X != -1)
                {
                    endOfPlaying = CountUnderCursor (PickOutBefore.X);
                }
                if (firstlyPlayed == true)
                {
                    TrimWavFile(countUnderCursor, endOfPlaying);
                    playingNow.SoundLocation = Application.StartupPath + @"\" + "buffer";
                    playingNow.Load();
                    firstlyPlayed = false;
                    isPlayingNow = true;
                    timer.Interval = 300;
                    timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                }
                cursorPos = PickOutFrom.X + MDIParent1.difBtwMainAndWorkSpace - MDIParent1.listView1.Width;
                //cursorPos = (int)(pos * newWav.LeftChData.Length) + (int)( / step); ;
                if (cursorPos < 0)
                    cursorPos = 0;
                playingPosition = (int)(pos * newWav.LeftChData.Length) + (int)((PickOutFrom.X + 
                    MDIParent1.difBtwMainAndWorkSpace - MDIParent1.listView1.Width - pictureBox1.Location.X) / step);
                hScrollBar1.Enabled = false;
                playingNow.Play();
                timer.Start();
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            playingNow.Stop();
            playingNow.Dispose();
            timer.Stop();
            isPlayingNow = false;
            hScrollBar1.Enabled = true;
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

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            if (Form5.cantBlockWindow == false)
                MDIParent1.listView1.Focus();
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if (isPlayingNow == true)
            {
                if (playingPosition > endOfPlaying)
                {
                    isPlayingNow = false;

                    hScrollBar1.Enabled = true;
                    setCursorBack();
                    playingNow.Dispose();
                    timer.Close();
                }
                else
                {
                    double newCursorPos = cursorPos + iii * 14700 * step;
                    g.Clear(Color.White);
                    g.DrawLine(System.Drawing.Pens.Gray, (float)newCursorPos,
                         pictureBox1.Location.Y, (float)newCursorPos, pictureBox1.Height);
                    pictureBox1.Image = bmp;
                    int countUnderMovingCursor = (int)(newCursorPos / step);
                    playingPosition += 14700 - residue;
                    residue = 0;
                    if (countUnderMovingCursor > newWav.sampleRate * 10 / (Math.Pow(2,timeScaleMode)))
                    {
                        cursorPos = 0;
                        if (stubbornPlacePassed == false)
                        {
                            if (hScrollBar1.Value + scrollBarDif < 100)
                            {
                                hScrollBar1.Value += scrollBarDif;
                                B = CountUnderCursor(pictureBox1.Location.X);
                                cursorPos = Math.Abs(B - A);
                                residue = cursorPos;
                                A = CountUnderCursor(pictureBox1.Width);
                            }
                            else
                            {
                                if (hScrollBar1.Value < 100)
                                {
                                    hScrollBar1.Value = hScrollBar1.Value + scrollBarDif;
                                }
                            }
                            iii = 0;
                            stubbornPlacePassed = true;
                        }
                        else
                        {
                            stubbornPlacePassed = false;
                        }
                    }
                    iii++;
                }
            }
        }

        private void setCursorBack()
        {
            g.Clear(Color.White);
            if (PickOutFrom.X + MDIParent1.difBtwMainAndWorkSpace - MDIParent1.listView1.Width > 0)
                g.DrawLine(System.Drawing.Pens.Gray, PickOutFrom.X + MDIParent1.difBtwMainAndWorkSpace - MDIParent1.listView1.Width,
                 pictureBox1.Location.Y, PickOutFrom.X + MDIParent1.difBtwMainAndWorkSpace - MDIParent1.listView1.Width, pictureBox1.Height);
            else
            {
                hScrollBar1.Value = 0;
                g.DrawLine(System.Drawing.Pens.Gray, pictureBox1.Location.X,
                    pictureBox1.Location.Y, pictureBox1.Location.X, pictureBox1.Height);
            }
            pictureBox1.Image = bmp;
        }
        private void hScrollBar1_ValueChanged(object sender, EventArgs e)
        {
            g.Clear(Color.White);
            pos = hScrollBar1.Value / 100.0;
            textBox1.Text += hScrollBar1.Value + "  " ;          
            pictureBox1.Refresh(); 
            
        }

        private int CountUnderCursor (float position)
        {
            int cuc = (int)Math.Round((pos * newWav.LeftChData.Length) + (int)Math.Round((position +
                    MDIParent1.difBtwMainAndWorkSpace - MDIParent1.listView1.Width - pictureBox1.Location.X) / step),0);
            return cuc;
        }

        private void pictureBox1_MouseHover_1(object sender, EventArgs e)
        {
            if (Form5.cantBlockWindow == false)
                pictureBox1.Focus();
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
           if (e.Type == ScrollEventType.SmallIncrement)
           {
               something2 = 0;
               something += (int)(newWav.sampleRate * 10 / scalingRatio / 3);
               if (timeScaleMode > 4 && something < newWav.LeftChData.Length / 100)
                   hScrollBar1.SmallChange = 0;
               pictureBox1.Refresh();
           }
           if (e.Type == ScrollEventType.SmallDecrement)
           {
               if ((int)(pos * newWav.LeftChData.Length + (int)(newWav.sampleRate * 10 / scalingRatio )) < newWav.LeftChData.Length)
               {
                   if ((int)(pos * newWav.LeftChData.Length) > (int)(newWav.sampleRate * 10 / scalingRatio / 3))
                   {
                       something2 += (int)(newWav.sampleRate * 10 / scalingRatio / 3);
                       if (timeScaleMode > 4 && something < newWav.LeftChData.Length / 100)
                           hScrollBar1.SmallChange = 0;
                       pictureBox1.Refresh();
                   }
               }
               else
               {
                   pos = 0.99;
                   pictureBox1.Refresh();
               }
           }
           if (e.Type == ScrollEventType.ThumbTrack)
           {
               something2 = 0;
               something = 0;
           }
        }
    }
}
