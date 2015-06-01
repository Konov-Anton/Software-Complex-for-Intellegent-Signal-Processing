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
    public partial class Pictures : Form
    {
        public Pictures()
        {
            InitializeComponent();
        }
        List<double> pitch = new List<double>();
        List<double> formants = new List<double>();
        List<double[]> F = new List<double[]>();
        List<double> H = new List<double>();
        string window = "bm";

        private void Pictures_Load(object sender, EventArgs e)
        {
            Form1.cantBlockWindow = true;
            this.Location = new Point(0,300);
            if(MDIParent1.pictureMode == 1)
            {
                this.Text = "Спектрограмма";
            }
            if (MDIParent1.pictureMode == 2)
            {
                this.Text = "Частота основного тона";
                SaveFileDialog saveFft = new SaveFileDialog();
                saveFft.InitialDirectory = Application.StartupPath;
                saveFft.Filter = "Текстовый документ (*.txt)|*.txt";
                double[] source = WAV.GetNormalizedData(Form1.newWav);
                pitch = SignalProcessing.Pitch(source, 1024, 256, (int)Form1.newWav.sampleRate);
                string text = "";
                for (int i = 0; i < pitch.Count; i++)
                {
                    text += pitch[i] + "\r\n";
                }
                if (saveFft.ShowDialog(this) == DialogResult.OK)
                {
                    string FileName = saveFft.FileName;
                    System.IO.File.WriteAllText(FileName, text);
                }
            }
            if  (MDIParent1.pictureMode == 3)
            {
                this.Text = "Форманты";
                SaveFileDialog saveFft = new SaveFileDialog();
                saveFft.InitialDirectory = Application.StartupPath;
                saveFft.Filter = "Текстовый документ (*.txt)|*.txt";
                double[] source = WAV.GetNormalizedData(Form1.newWav);
                string text = "";
                for (int k = 0; k + 1024 < source.Length; k = k + 768)
                {
                    double[] x = new double[1024];
                    Array.Copy(source, k, x, 0, 1024);
                    double[] lpc = SignalProcessing.Durbin(x, (int)Form1.newWav.sampleRate / 1000);
                    Complex[] roots = SignalProcessing.ComputeRoots(lpc);
                    List<double> angz = new List<double>();
                    List<double> freqs = new List<double>();
                    List<double> bandwidths = new List<double>();
                    
                    int j = 0;
                    for (int i = 0; i < roots.Length; i++)
                    {
                        if (roots[i].imaginary >= 0)
                        {
                            freqs.Add(Math.Atan2(roots[i].imaginary, roots[i].real) * (Form1.newWav.sampleRate / (2 * Math.PI)));
                            bandwidths.Add((-1 * (double)Form1.newWav.sampleRate / (Math.PI)) * Math.Log(Math.Sqrt(roots[i].real * roots[i].real + roots[i].imaginary * roots[i].imaginary)));
                            //if (Math.Sqrt(roots[i].real * roots[i].real + roots[i].imaginary * roots[i].imaginary) >= 0.7)
                            
                            if (freqs[j] > 90) // && bandwidths.Last() < 400)
                                formants.Add(freqs[j]);

                            j++;
                        }
                    }
                    formants.Sort();
                    double[] temp = new double[4];
                    if (formants.Count>0)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (formants[0] < 600)
                                temp[i] = formants[i];
                            else
                                temp[i] = 0;

                            text += formants[i].ToString() + " ";
                        }
                    }
                    formants.Clear();
                    F.Add(temp);
                    text += "\r\n";
                }
                if (saveFft.ShowDialog(this) == DialogResult.OK)
                {
                    string FileName = saveFft.FileName;
                    System.IO.File.WriteAllText(FileName, text);
                }
            }

            if (MDIParent1.pictureMode == 5)
            {
                this.Text = "Передаточная функция";
                for (float i=0; i<Filter.order; i++)
                {
                    Complex sum = new Complex(0,0);
                    for (int j=0; j<Filter.order; j++)
                    {
                        sum.real += Filter.weights[j] * Math.Cos(-2 * Math.PI * i * j / Filter.order);
                        sum.imaginary += Filter.weights[j] * Math.Sin(-2 * Math.PI * i * j / Filter.order);                       
                    } 
                    double s = Math.Sqrt(sum.real * sum.real + sum.imaginary * sum.imaginary);
                    H.Add(s);
                }
            }
            pictureBox1.Refresh();
        }
        bool scaleDone = false;

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if(MDIParent1.pictureMode == 1)
            {
                System.Drawing.Font smallFont = new System.Drawing.Font("Arial", 7);
                System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);

                int nframes = (int)Math.Floor((decimal)(Form1.newWav.LeftChData.Length - 1024) / 256) + 1;
                double[] data = WAV.GetNormalizedData(Form1.newWav);
                List<List<Complex>> fft = Fourier.FftWithWindow(data, 1024, 256);
                if (Form1.newWav.channels == 2)
                    nframes = fft.Count;
           
                Complex[][] fft1 = new Complex[nframes][];
                int k = 0;
                foreach (List<Complex> list in fft)
                {
                    fft1[k] = list.ToArray();
                    k++;
                }
                float stepx = pictureBox1.Width / (float)nframes;

                float x = 30;
                e.Graphics.DrawLine(System.Drawing.Pens.Gray, 0, pictureBox1.Height - 14, pictureBox1.Width, pictureBox1.Height - 14);
                e.Graphics.DrawLine(System.Drawing.Pens.Gray, 29, pictureBox1.Height, 29, pictureBox1.Location.Y);
                    for (int m = 0; m < nframes; m++)
                    {
                        double[] magn = Fourier.GetMagnitude(fft1[m]);
                        double[] globalMagnitudes = new double[magn.Length / 2];
                        Array.Copy(magn, globalMagnitudes, globalMagnitudes.Length);
                        for (int i = 0; i < globalMagnitudes.Length; i++)                       // нормировка (нужна ли)
                        {
                            globalMagnitudes[i] = (double)(2 * globalMagnitudes[i] / 1024);
                        }
                        double maxMagn = 0;
                        int indexOfMax = 0;
                        for (int i = 0; i < globalMagnitudes.Length; i++)
                        {
                            if (globalMagnitudes[i] > maxMagn)
                            {
                                maxMagn = globalMagnitudes[i];
                                indexOfMax = i;
                            }
                        }
                        double[] freqs = new double[globalMagnitudes.Length];
                        for (int i = 0; i < globalMagnitudes.Length; i++)
                        {
                            freqs[i] = (i * Form1.newWav.sampleRate) / (float)(globalMagnitudes.Length * 2);
                        }
                        float stepy = (float)pictureBox1.Height / (freqs.Length - 1);               
                        float y = pictureBox1.Height;
                        if (maxMagn > 0)
                        {
                            double intensity = 255 / maxMagn;
                            for (int j = 0; j < freqs.Length - 1; j++)
                            {
                                int color = 255 - (int)(globalMagnitudes[j] * intensity);
                                //if (color > 64 && color < 192)
                                //    color -= 60;
                                Brush br = new SolidBrush(Color.FromArgb(color, color, color));
                                e.Graphics.FillRectangle(br, x, y-15, stepx, stepy);
                                if(scaleDone == false)
                                {
                                    scaleDone = true;
                                    double temp = freqs.Length / (Form1.newWav.sampleRate / 2 / 1000);
                                    for (int i=0; i<Form1.newWav.sampleRate/1000 + 1; i++)
                                    {
                                        e.Graphics.DrawLine(System.Drawing.Pens.Gray, 29, (float)(pictureBox1.Height - stepy * i * temp), 
                                            22,(float)(pictureBox1.Height -  stepy * i * temp));
                                        double str = i * 1000;
                                        e.Graphics.DrawString(str.ToString(), smallFont, drawBrush, 2, (float)((float)(pictureBox1.Height - stepy * i * temp)));
                                    }
                                }
                                y -= stepy;
                            }
                        }
                        if (m%50 == 0)
                        {
                            e.Graphics.DrawLine(System.Drawing.Pens.Gray, x, pictureBox1.Height - 14, x, pictureBox1.Height - 4);
                            e.Graphics.DrawString(m.ToString(), smallFont, drawBrush, x+1, (float)(pictureBox1.Height - 10));
                        }
                        x += stepx;

                }
            }
            if(MDIParent1.pictureMode == 2)
            {
                System.Drawing.Font smallFont = new System.Drawing.Font("Arial", 7);
                System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);

                e.Graphics.DrawLine(System.Drawing.Pens.Gray, 0, pictureBox1.Height - 14, pictureBox1.Width, pictureBox1.Height - 14);
                e.Graphics.DrawLine(System.Drawing.Pens.Gray, 29, pictureBox1.Height, 29, pictureBox1.Location.Y);
                float x = 30;
                float stepy = (pictureBox1.Height-14)/500f;
                float stepx = (float)pictureBox1.Width / pitch.Count;
                for (int i = 0; i < 5;i++ )
                {
                    e.Graphics.DrawLine(System.Drawing.Pens.Gray, 29, pictureBox1.Height - 14 - 100 * stepy * i, 22, pictureBox1.Height - 14 - 100 * stepy * i);
                    e.Graphics.DrawString((100*i).ToString(), smallFont, drawBrush, 10, pictureBox1.Height - 14 - 100 * stepy * i);
                }
                for (int i = 0; i < pitch.Count; i++)
                {
                    float y = (float)pitch[i] * stepy;
                    e.Graphics.FillRectangle(drawBrush, x, pictureBox1.Height - (y + 14), 1, 1);
                    if (i != pitch.Count - 1)
                        e.Graphics.DrawLine(System.Drawing.Pens.Black, x, pictureBox1.Height - y - 14, x + stepx, pictureBox1.Height - 14 - (float)pitch[i + 1] * stepy);
                    if (i % 50 == 0)
                    {
                        e.Graphics.DrawLine(System.Drawing.Pens.Gray, x, pictureBox1.Height - 14, x, pictureBox1.Height - 4);
                        e.Graphics.DrawString(i.ToString(), smallFont, drawBrush, x + 1, (float)(pictureBox1.Height - 10));
                    }
                    x += stepx;
                }
            }
            if(MDIParent1.pictureMode == 3)
            {
                System.Drawing.Font smallFont = new System.Drawing.Font("Arial", 7);
                System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
                e.Graphics.DrawLine(System.Drawing.Pens.Gray, 0, pictureBox1.Height - 14, pictureBox1.Width, pictureBox1.Height - 14);
                e.Graphics.DrawLine(System.Drawing.Pens.Gray, 29, pictureBox1.Height, 29, pictureBox1.Location.Y);
                float x = 30;
                float stepy = (pictureBox1.Height - 14) / 5000f;
                float stepx = (float)(pictureBox1.Width-40) / F.Count;
                for (int i = 0; i < 5; i++)
                {
                    e.Graphics.DrawLine(System.Drawing.Pens.Gray, 29, pictureBox1.Height - 14 - 1000 * stepy * i, 22, pictureBox1.Height - 14 - 1000 * stepy * i);
                    e.Graphics.DrawString((1000 * i).ToString(), smallFont, drawBrush, 10, pictureBox1.Height - 14 - 1000 * stepy * i);
                }
                for (int i = 0; i < F.Count; i++)
                {
                    float y;
                    for (int j = 0; j <F[i].Length; j++)
                    {
                        y = (float)F[i][j] * stepy;
                        e.Graphics.FillRectangle(drawBrush, x, pictureBox1.Height - (y + 14), 1, 1);
                        if (i != F.Count - 1 && F[i+1][0]!=0 && F[i][0]!=0)
                        {
                            if (j==0)
                                e.Graphics.DrawLine(System.Drawing.Pens.Black, x, pictureBox1.Height - y - 14, x + stepx, pictureBox1.Height - 14 - (float)F[i + 1][j] * stepy);
                            if (j == 1)
                                e.Graphics.DrawLine(System.Drawing.Pens.Yellow, x, pictureBox1.Height - y - 14, x + stepx, pictureBox1.Height - 14 - (float)F[i + 1][j] * stepy);
                            if (j == 2)
                                e.Graphics.DrawLine(System.Drawing.Pens.Red, x, pictureBox1.Height - y - 14, x + stepx, pictureBox1.Height - 14 - (float)F[i + 1][j] * stepy);
                            if (j == 3)
                                e.Graphics.DrawLine(System.Drawing.Pens.Blue, x, pictureBox1.Height - y - 14, x + stepx, pictureBox1.Height - 14 - (float)F[i + 1][j] * stepy);
                        }
                    }
                    if (i % 50 == 0)
                    {
                        e.Graphics.DrawLine(System.Drawing.Pens.Gray, x, pictureBox1.Height - 14, x, pictureBox1.Height - 4);
                        e.Graphics.DrawString(i.ToString(), smallFont, drawBrush, x + 1, (float)(pictureBox1.Height - 10));
                    }
                    x += stepx;
                }
            }
            if(MDIParent1.pictureMode == 5)
            {
                System.Drawing.Font smallFont = new System.Drawing.Font("Arial", 7);
                System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
                e.Graphics.DrawLine(System.Drawing.Pens.Gray, 0, pictureBox1.Height - 14, pictureBox1.Width , pictureBox1.Height - 14);
                e.Graphics.DrawLine(System.Drawing.Pens.Gray, 30, pictureBox1.Height, 30, 0);
                e.Graphics.DrawString("0", smallFont, drawBrush, 23, (float)(pictureBox1.Height - 12));
                e.Graphics.DrawString("|H(w)|", smallFont, drawBrush, 32, (float)(8));
                e.Graphics.DrawString("F, Гц", smallFont, drawBrush, pictureBox1.Width - 30, (float)(pictureBox1.Height - 26));
                float x = 30;
                float stepy = (pictureBox1.Height - 50) / (float)H.Max();
                float stepx = (float)(pictureBox1.Width - 40) / ((H.Count - 1) / 2);

                e.Graphics.DrawLine(System.Drawing.Pens.Gray, 30, pictureBox1.Height - (float)H.Max() * stepy - 14, 23, pictureBox1.Height - (float)H.Max() * stepy - 14);
                e.Graphics.DrawLine(System.Drawing.Pens.Gray, 30, pictureBox1.Height - (float)H.Max() * stepy / 2 - 14, 23, pictureBox1.Height - (float)H.Max() * stepy / 2 - 14);
                e.Graphics.DrawString("1", smallFont, drawBrush, 15, (float)(pictureBox1.Height - (float)H.Max() * stepy - 18));
                e.Graphics.DrawString("0.5", smallFont, drawBrush, 5, (float)(pictureBox1.Height - (float)H.Max() * stepy / 2 - 18));

                for (int i = 0; i < (H.Count - 1) / 2; i++)
                {
                    e.Graphics.DrawLine(System.Drawing.Pens.Blue, x, pictureBox1.Height - 14 - (float)(H[i] * stepy), x + stepx, pictureBox1.Height - 14 - (float)(H[i + 1] * stepy));
                    x += stepx;
                }
                int s = (int)Form1.newWav.sampleRate/ 2 /1000;
                float stepp = (pictureBox1.Width - 50) / s;
                for (int i = 0; i < s; i++)
                {
                    e.Graphics.DrawLine(System.Drawing.Pens.Gray, 30 + stepp * (i + 1), pictureBox1.Height - 14, 30 + stepp * (i + 1), pictureBox1.Height - 9);
                    e.Graphics.DrawString(((i + 1) * 1000).ToString(), smallFont, drawBrush, 30 + stepp * (i + 1) - 4, (float)(pictureBox1.Height - 9));
                }
            }
        }

        private void Pictures_FormClosed(object sender, FormClosedEventArgs e)
        {
            Form1.cantBlockWindow = false;
        }
        
    }
}
