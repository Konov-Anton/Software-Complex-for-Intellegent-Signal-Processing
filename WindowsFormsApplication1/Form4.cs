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
    public partial class Form4 : Form
    {
        public Form4()
        {
            InitializeComponent();
        }

        double maxMagn = 0;
        int indexOfMax = 0;
        bool scaleDone = false;
        private void Form4_Load(object sender, EventArgs e)
        {
            this.Location = new Point(0, 300);
            pictureBox1.Show();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            System.Drawing.Font drawFont = new System.Drawing.Font("Arial", 7);
            System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
            float[] freqs = new float[MDIParent1.globalMagnitudes.Length];
            double step = ((double)this.Width - 90) / MDIParent1.globalMagnitudes.Length ;
            float x = 30f;
            e.Graphics.DrawLine(System.Drawing.Pens.Gray, x, pictureBox1.Location.Y, x, pictureBox1.Height);
            for (int i = 0; i < MDIParent1.globalMagnitudes.Length; i++ )
            {
                freqs[i] = (i * Form1.newWav.sampleRate) / (float)(MDIParent1.globalMagnitudes.Length * 2);
            }
            e.Graphics.DrawLine(System.Drawing.Pens.Gray, pictureBox1.Location.X, pictureBox1.Height - 15, pictureBox1.Width, pictureBox1.Height - 15);
            
            for (int i = 0; i < MDIParent1.globalMagnitudes.Length; i++)
            {
                if (MDIParent1.globalMagnitudes[i] > maxMagn)
                {
                    maxMagn = MDIParent1.globalMagnitudes[i];
                    indexOfMax = i;
                }
            }
            double coef = 1000.0;
            if (maxMagn * 1000 > pictureBox1.Height - 15)
            {
                coef = (pictureBox1.Height - 15) / (maxMagn * 1000);
                coef = 0.9 * coef * 1000;
            }
            e.Graphics.DrawLine(System.Drawing.Pens.Gray, 10, (float)(pictureBox1.Height - maxMagn * coef - 15),
                30, (float)(pictureBox1.Height - maxMagn * coef - 15));
            string sgnMaxMagn = Math.Round(maxMagn, 3).ToString();
            e.Graphics.DrawString(sgnMaxMagn.ToString(), drawFont, drawBrush, pictureBox1.Location.X, (float)(pictureBox1.Height - maxMagn * coef - 15));
            for (int i = 0; i < MDIParent1.globalMagnitudes.Length-1; i++)
            {  
                e.Graphics.DrawLine(System.Drawing.Pens.Blue, x, (float)(pictureBox1.Height - MDIParent1.globalMagnitudes[i] * coef - 15),
                    (float)(x + step), (float)(pictureBox1.Height - MDIParent1.globalMagnitudes[i + 1] * coef - 15));
                x = (float)(x + step);
            }
            x -= (float)step;
            double k = (x-30) / (Form1.newWav.sampleRate / 2);
            int j = (int)Math.Floor((double)(Form1.newWav.sampleRate / 2) / 5000);
            
            for (int i = 0; i < j + 1; i++)
            {              
                e.Graphics.DrawLine(System.Drawing.Pens.Gray, (float)(k * i * 5000) + 30,
                    (float)(pictureBox1.Height - 15), (float)(k * i * 5000) +30, pictureBox1.Height - 5);
                int sgn = i*5000;
                string str = sgn.ToString();
                e.Graphics.DrawString(str+"Гц", drawFont, drawBrush, (float)(k * i * 5000)+30, (float)(pictureBox1.Height - 15));
            }
            e.Graphics.DrawLine(System.Drawing.Pens.Gray, x, (float)(pictureBox1.Height - 15), x, (float)(pictureBox1.Height - 5));
            int sgnSR = (int)Form1.newWav.sampleRate / 2;
            string str2 = sgnSR.ToString();
            e.Graphics.DrawString(str2 + "Гц", drawFont, drawBrush, (float)(x), (float)(pictureBox1.Height - 15));
            pictureBox2.Show();
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            System.Drawing.Font drawFont = new System.Drawing.Font("Arial", 7);
            System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
            double step = (pictureBox2.Width - 30) / 30;
            double coef = 500.0;
            if (maxMagn * 500 > pictureBox2.Height - 15)
            {
                coef = (pictureBox2.Height - 15) / (maxMagn * 500);
                coef = 0.9 * coef * 500;
            }
            float x = 30;
            int left = indexOfMax - 15;
            e.Graphics.DrawLine(System.Drawing.Pens.Gray, x, pictureBox2.Location.Y, x, pictureBox2.Height);
            e.Graphics.DrawLine(System.Drawing.Pens.Gray, 0, pictureBox2.Height - 15, pictureBox2.Width, pictureBox2.Height - 15);
            e.Graphics.DrawLine(System.Drawing.Pens.Gray, 10, (float)(pictureBox2.Height - maxMagn * coef - 15),
                (float)(30 + step * (indexOfMax - left)), (float)(pictureBox2.Height - maxMagn * coef - 15));
            string sgnMaxMagn = Math.Round(maxMagn, 3).ToString();
            e.Graphics.DrawString(sgnMaxMagn, drawFont, drawBrush, 5, (float)(pictureBox2.Height - maxMagn * coef - 15));

           
            if (indexOfMax - 15 < 0)
                left = 0;
            for (int i = left; i < indexOfMax + 15; i++)
            {
                e.Graphics.DrawLine(System.Drawing.Pens.Blue, x, (float)(pictureBox2.Height - MDIParent1.globalMagnitudes[i] * coef - 15),
                    (float)(x + step), (float)(pictureBox2.Height - MDIParent1.globalMagnitudes[i + 1] * coef - 15));
                e.Graphics.DrawLine(System.Drawing.Pens.Gray, x, pictureBox2.Height - 15, x, pictureBox2.Height - 10);
                if (i == left + 1 || i == indexOfMax + 14)
                {
                    System.Drawing.Font smallFont = new System.Drawing.Font("Arial", 5);
                    e.Graphics.DrawString(i.ToString(), smallFont, drawBrush, x - 5, (float)(pictureBox2.Height - 10));
                }
                if (i == indexOfMax)
                {
                    string str = ((indexOfMax * Form1.newWav.sampleRate) / (float)(MDIParent1.globalMagnitudes.Length * 2)).ToString();
                    e.Graphics.DrawString(str + " Гц", drawFont, drawBrush, (float)(30 + step * (indexOfMax - left)), (float)(pictureBox2.Height - maxMagn * coef - 15));                    e.Graphics.DrawLine(System.Drawing.Pens.Gray, x, (float)(pictureBox2.Height - MDIParent1.globalMagnitudes[i] * coef - 15),
                        x, pictureBox2.Height - 15);
                    System.Drawing.Font smallFont = new System.Drawing.Font("Arial", 5);
                    e.Graphics.DrawString(i.ToString(), smallFont, drawBrush, x - 5, (float)(pictureBox2.Height - 10));
                }
                x = (float)(x + step);
            }           
        }

        private void Form4_FormClosed(object sender, FormClosedEventArgs e)
        {
            MDIParent1.SetSpectrumUncheckedValue();
        }

        private void Form4_Resize(object sender, EventArgs e)
        {
            pictureBox1.Refresh();
            pictureBox2.Location = new Point (this.Width - 272,0);
        }


    }
}
