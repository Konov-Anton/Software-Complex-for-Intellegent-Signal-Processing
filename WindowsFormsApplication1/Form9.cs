using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace WindowsFormsApplication1
{
    public partial class batchProcessing : Form
    {
        public batchProcessing()
        {
            InitializeComponent();
        }

        int stillBusy1 = 0;
        string window = "hm";
        List<string> paths = new List<string>();
        double[] weights;

        private void batchProcessing_Load(object sender, EventArgs e)
        {
            Directory.CreateDirectory(Application.StartupPath + "\\Пакетная обработка");
            textBox12.Text = Application.StartupPath + "\\Пакетная обработка";
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked == true)
            {
                stillBusy1++;
                checkBox1.Checked = false;
                checkBox1.Enabled = false;
                checkBox2.Checked = false;
                checkBox2.Enabled = false;

                label4.Enabled = true;
                label5.Enabled = true;
                label6.Enabled = true;
                label7.Enabled = true;
                label18.Enabled = true;

                textBox1.Enabled = true;
                textBox2.Enabled = true;
                textBox3.Enabled = true;
                textBox4.Enabled = true;

                listBox1.Enabled = true;
            }
            else
            {                
                checkBox1.Enabled = true;
                checkBox2.Enabled = true;

                if (stillBusy1 < 2)
                {
                    label4.Enabled = false;
                    label5.Enabled = false;
                    label6.Enabled = false;
                    label7.Enabled = false;
                    label18.Enabled = false;

                    textBox1.Enabled = false;
                    textBox2.Enabled = false;
                    textBox3.Enabled = false;
                    textBox4.Enabled = false;

                    listBox1.Enabled = false;
                }
                stillBusy1--;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                stillBusy1++;

                label4.Enabled = true;

                label6.Enabled = true;
                label7.Enabled = true;
                label18.Enabled = true;

                textBox1.Enabled = true;

                textBox3.Enabled = true;
                textBox4.Enabled = true;

                listBox1.Enabled = true;
            }
            else
            {
                if (stillBusy1 < 3)
                {
                    label4.Enabled = false;

                    textBox1.Enabled = false;
                }
                if (stillBusy1 < 2)
                {
                    label6.Enabled = false;
                    label7.Enabled = false;
                    label18.Enabled = false;

                    textBox3.Enabled = false;
                    textBox4.Enabled = false;

                    listBox1.Enabled = false;
                }
                stillBusy1--;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true)
            {
                stillBusy1++;

                label5.Enabled = true;
                label6.Enabled = true;
                label7.Enabled = true;
                label18.Enabled = true;

                textBox2.Enabled = true;
                textBox3.Enabled = true;
                textBox4.Enabled = true;

                listBox1.Enabled = true;
            }
            else
            {
                if (stillBusy1 < 3)
                {
                    label5.Enabled = false;

                    textBox2.Enabled = false;
                }
                if (stillBusy1 < 2)
                {
                    label6.Enabled = false;
                    label7.Enabled = false;
                    label18.Enabled = false;

                    textBox3.Enabled = false;
                    textBox4.Enabled = false;

                    listBox1.Enabled = false;
                }
                stillBusy1--;
            }
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked == true)
            {
                stillBusy1++;

                label7.Enabled = true;
                label18.Enabled = true;

                textBox4.Enabled = true;

                listBox1.Enabled = true;
            }
            else
            {
                if (stillBusy1 < 2)
                {
                    label7.Enabled = false;
                    label18.Enabled = false;

                    textBox4.Enabled = false;

                    listBox1.Enabled = false;
                }
                stillBusy1--;
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsDigit(e.KeyChar))
            {
            }
            else
                if (e.KeyChar == '.')
                {
                }
                else
                    if (Char.IsControl(e.KeyChar) && textBox1.Text != "")
                    {
                    }
                    else
                    {
                        e.Handled = true;
                    }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsDigit(e.KeyChar))
            {
            }
            else
                if (e.KeyChar == '.')
                {
                }
                else
                    if (Char.IsControl(e.KeyChar) && textBox2.Text != "")
                    {
                    }
                    else
                    {
                        e.Handled = true;
                    }
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsDigit(e.KeyChar))
            {
            }
            else
                if (Char.IsControl(e.KeyChar) && textBox3.Text != "")
                {
                }
                else
                {
                    e.Handled = true;
                }
        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsDigit(e.KeyChar))
            {
            }
            else
                if (Char.IsControl(e.KeyChar) && textBox4.Text != "")
                {
                }
                else
                {
                    e.Handled = true;
                }
        }

        private void textBox4_Leave(object sender, EventArgs e)
        {
            if (textBox4.Text != "" && textBox4.Text != "-")
            {
                if (Convert.ToDouble(textBox4.Text) < 10)
                    textBox4.Text = "10";
                if (Convert.ToDouble(textBox4.Text) > 16000)
                    textBox4.Text = "16000";
            }
        }

        private void textBox3_Leave(object sender, EventArgs e)
        {
            if (textBox3.Text != "" && textBox3.Text != "-")
            {
                if (Convert.ToDouble(textBox3.Text) < 0)
                    textBox3.Text = "10";
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog br = new FolderBrowserDialog();
            if (br.ShowDialog(this) == DialogResult.OK)
            {
                textBox12.Text = br.SelectedPath;
            }            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            for(int p=0; p<paths.Count; p++)
            {
                WAV temp = WAV.Reading(paths[p]);
                if(checkBox1.Checked == true)
                {
                    WAV buffer = CreateBuffer(temp);
                    double Fc = Convert.ToDouble(textBox1.Text);
                    int order = Convert.ToInt32(textBox4.Text);
                    if (order % 2 == 0)
                        order++;
                    buffer.LeftChData = Filters.LowPassFilter(buffer.LeftChData, Fc, order, window, out weights);
                    buffer.wavData = WAV.ComputeBytesOfWavData(buffer.LeftChData);
                    temp = CreateBuffer(buffer);
                    for (int i = 0; i < temp.LeftChData.Length; i++)
                        temp.LeftChData[i] *= 32768;
                }
                if(checkBox2.Checked == true)
                {
                    WAV buffer = CreateBuffer(temp);
                    double Fc = Convert.ToDouble(textBox2.Text);
                    int order = Convert.ToInt32(textBox4.Text);
                    if (order % 2 == 0)
                        order++;
                    buffer.LeftChData = Filters.HighPassFilter(buffer.LeftChData, Fc, order, window, out weights);
                    buffer.wavData = WAV.ComputeBytesOfWavData(buffer.LeftChData);
                    temp = CreateBuffer(buffer);
                    for (int i = 0; i < temp.LeftChData.Length; i++)
                        temp.LeftChData[i] *= 32768;
                }
                if(checkBox3.Checked == true)
                {
                    WAV buffer = CreateBuffer(temp);
                    double FcHigh = Convert.ToDouble(textBox2.Text);
                    double FcLow = Convert.ToDouble(textBox1.Text);
                    int order = Convert.ToInt32(textBox4.Text);
                    if (order % 2 == 0)
                        order++;
                    buffer.LeftChData = Filters.BandPassFilter(buffer.LeftChData, FcLow, FcHigh, order, window, out weights);
                    buffer.wavData = WAV.ComputeBytesOfWavData(buffer.LeftChData);
                    temp = CreateBuffer(buffer);
                    for (int i = 0; i < temp.LeftChData.Length; i++)
                        temp.LeftChData[i] *= 32768;
                }
                if(checkBox4.Checked == true)
                {
                    WAV buffer = CreateBuffer(temp);
                    int order = Convert.ToInt32(textBox4.Text);
                    if (order % 2 == 0)
                        order++;
                    buffer.LeftChData = Filters.MovingAverageFilter(buffer.LeftChData, order);
                    buffer.wavData = WAV.ComputeBytesOfWavData(buffer.LeftChData);
                    temp = CreateBuffer(buffer);
                    for (int i = 0; i < temp.LeftChData.Length; i++)
                        temp.LeftChData[i] *= 32768;
                }
                if(checkBox5.Checked == true)
                {

                }
                if(checkBox7.Checked == true)
                {
                    string path = Application.StartupPath + "\\Пакетная обработка" + "\\Коэффициенты\\FFT";
                    Directory.CreateDirectory(path);
                    double[] normalizedData = WAV.GetNormalizedData(temp);

                    Complex[] forFft = Fourier.PrepareToFFT(normalizedData);
                    for (int j = 0; j < normalizedData.Length; j++)
                    {
                        normalizedData[j] = normalizedData[j] * (0.53836 - 0.46164 * Math.Cos((2 * Math.PI * j) / (normalizedData.Length - 1)));
                    }
                    Complex[] fftCoefs = Fourier.FFT(forFft);

                    string[] text = new string[fftCoefs.Length];
                    for (int i = 0; i < fftCoefs.Length; i++)
                    {
                        text[i] = fftCoefs[i].ToString();
                    }
                    System.IO.File.WriteAllLines(path+"\\" + listBox4.Items[p], text);
                }
                if(checkBox9.Checked == true)
                {
                    double[] normalizedData = WAV.GetNormalizedData(temp);
                    int frameLength = Convert.ToInt32(textBox7.Text);
                    int frameInc = Convert.ToInt32(textBox7.Text) - Convert.ToInt32(textBox6.Text);

                    List<double[]> a = new List<double[]>();

                    a = SignalProcessing.GetLPC(normalizedData, frameLength, frameInc, Convert.ToInt32(textBox8.Text), window);
                    string path = Application.StartupPath + "\\Пакетная обработка" + "\\Коэффициенты\\LPC";
                    Directory.CreateDirectory(path);
                    string[] text = new string[a.Count];
                    for (int i = 0; i < a.Count; i++)
                    {
                        for (int j = 0; j < a[i].Length; j++)
                        {
                            if (j != a[i].Length - 1)
                                text[i] += Math.Round(a[i][j], 3) + ", ";
                            else
                                text[i] += Math.Round(a[i][j], 3);
                        }
                    }
                    string[] splitted = paths[p].Split('\\');
                    string name = splitted[splitted.Length - 1];
                    System.IO.File.WriteAllLines(path + "\\" + name+".txt", text);
                }
                if(checkBox6.Checked == true)
                {
                    double[] normalizedData = WAV.GetNormalizedData(temp);
                    int frameLength = Convert.ToInt32(textBox7.Text);
                    int frameInc = Convert.ToInt32(textBox7.Text) - Convert.ToInt32(textBox6.Text);

                    List<double[]> a = new List<double[]>();

                    a = SignalProcessing.GetCC(normalizedData, frameLength, frameInc, Convert.ToInt32(textBox8.Text), Convert.ToInt32(textBox8.Text), window);
                    string path = Application.StartupPath + "\\Пакетная обработка" + "\\Коэффициенты\\CLPC";
                    Directory.CreateDirectory(path);
                    string[] text = new string[a.Count];
                    for (int i = 0; i < a.Count; i++)
                    {
                        for (int j = 0; j < a[i].Length; j++)
                        {
                            if (j != a[i].Length - 1)
                                text[i] += Math.Round(a[i][j], 3) + ", ";
                            else
                                text[i] += Math.Round(a[i][j], 3);
                        }
                    }
                    string[] splitted = paths[p].Split('\\');
                    string name = splitted[splitted.Length - 1];
                    System.IO.File.WriteAllLines(path + "\\" + name + ".txt", text);
                }
                if(checkBox10.Checked == true)
                {
                    double[] source = WAV.GetNormalizedData(temp);
                    List<List<double>> mffcList = SignalProcessing.MFCC(source, Convert.ToInt32(textBox7.Text),
                        Convert.ToInt32(textBox7.Text) - Convert.ToInt32(textBox6.Text),
                        Convert.ToInt32(textBox8.Text),
                        26,
                        (int)temp.sampleRate,
                        0,
                        (int)temp.sampleRate/2,
                        window);
                    string[] text = new string[mffcList.Count];
                    for (int i = 0; i < mffcList.Count; i++)
                    {
                        for (int j = 0; j < mffcList[i].Count; j++)
                        {
                            if (j != mffcList[i].Count - 1)
                                text[i] += Math.Round(mffcList[i][j], 3) + ", ";
                            else
                                text[i] += Math.Round(mffcList[i][j], 3);
                        }
                    }
                    string path = Application.StartupPath + "\\Пакетная обработка" + "\\Коэффициенты\\MFCC";
                    Directory.CreateDirectory(path);
                    string[] splitted = paths[p].Split('\\');
                    string name = splitted[splitted.Length - 1];
                    System.IO.File.WriteAllLines(path + "\\" + name + ".txt", text);
                }
                if(checkBox8.Checked == true)
                {
                    double[] source = WAV.GetNormalizedData(Form1.newWav);
                    List<List<double>> plpList = SignalProcessing.PLP(source, (int)Form1.newWav.sampleRate, Convert.ToInt32(textBox1.Text),
                        Convert.ToInt32(textBox1.Text) - Convert.ToInt32(textBox2.Text), window,
                        (int)Math.Ceiling(SignalProcessing.hzToBark(Form1.newWav.sampleRate / 2)) + 1,
                        Convert.ToInt32(textBox3.Text));
                    string[] text = new string[plpList.Count];

                    for (int i = 0; i < plpList.Count; i++)
                    {
                        for (int j = 0; j < plpList[i].Count; j++)
                        {
                            if (j != plpList[i].Count - 1)
                                text[i] += Math.Round(plpList[i][j], 3) + ", ";
                            else
                                text[i] += Math.Round(plpList[i][j], 3);
                        }
                    }
                    string path = Application.StartupPath + "\\Пакетная обработка" + "\\Коэффициенты\\MFCC";
                    Directory.CreateDirectory(path);
                    string[] splitted = paths[p].Split('\\');
                    string name = splitted[splitted.Length - 1];
                    System.IO.File.WriteAllLines(path + "\\" + name + ".txt", text);
                }
                if(checkBox12.Checked==true)
                {
                    double[] source = WAV.GetNormalizedData(Form1.newWav);
                    List<double> pitch = SignalProcessing.Pitch(source, 1024, 256, (int)Form1.newWav.sampleRate);
                    string[] text = new string[pitch.Count];
                    for (int i = 0; i < pitch.Count; i++)
                    {
                        text[i] = pitch[i].ToString();
                    }
                    string path = Application.StartupPath + "\\Пакетная обработка" + "\\Коэффициенты\\ЧОТ";
                    Directory.CreateDirectory(path);
                    string[] splitted = paths[p].Split('\\');
                    string name = splitted[splitted.Length - 1];
                    System.IO.File.WriteAllLines(path + "\\" + name + ".txt", text);
                }
                if(checkBox11.Checked == true)
                {
                    List<double> formants = new List<double>();
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

                                if (freqs[j] > 90) 
                                    formants.Add(freqs[j]);

                                j++;
                            }
                        }
                        formants.Sort();
                        double[] temp2 = new double[4];
                        if (formants.Count > 0)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                if (formants[0] < 600)
                                    temp2[i] = formants[i];
                                else
                                    temp2[i] = 0;

                                text += formants[i].ToString() + " ";
                            }
                        }
                        formants.Clear();
                        //F.Add(temp);
                        text += "\r\n";
                    }
                    string path = Application.StartupPath + "\\Пакетная обработка" + "\\Коэффициенты\\Форманты";
                    Directory.CreateDirectory(path);
                    string[] splitted = paths[p].Split('\\');
                    string name = splitted[splitted.Length - 1];
                    System.IO.File.WriteAllText(path + "\\" + name + ".txt", text);
                }
                string pathF = Application.StartupPath + "\\Пакетная обработка" + "\\Файлы";
                string[] splittedF = paths[p].Split('\\');
                    string nameF = splittedF[splittedF.Length - 1];
                    Directory.CreateDirectory(pathF);
                    WAV.Writing2(temp, Application.StartupPath + "\\Пакетная обработка" + "\\Файлы\\" + nameF);
            }


            //в самом конце
            if (checkBox18.Checked == true)
                Process.Start(textBox12.Text);
        }

        public WAV CreateBuffer(WAV newWav)
        {
            WAV buffer = new WAV();
            buffer.LeftChData = new double[newWav.LeftChData.Length];
            if (newWav.channels == 1)
            {
                buffer.wavData = newWav.wavData;
                Array.Copy(newWav.wavData, buffer.wavData, newWav.wavData.Length);
            }
            buffer.chunkID = newWav.chunkID;
            buffer.riffType = newWav.riffType;
            buffer.fmtID = newWav.fmtID;
            buffer.fmtSize = newWav.fmtSize;
            buffer.fmtCode = newWav.fmtCode;
            buffer.channels = 1;
            buffer.sampleRate = newWav.sampleRate;
            buffer.fmtAvgBPS = newWav.fmtAvgBPS;
            if (newWav.channels == 2)
                buffer.fmtAvgBPS /= 2;
            buffer.fmtBlockAlign = newWav.fmtBlockAlign;
            if (newWav.channels == 2)
                buffer.fmtBlockAlign /= 2;
            buffer.bitDepth = newWav.bitDepth;
            buffer.dataID = newWav.dataID;

            if (newWav.channels == 2)
            {
                buffer.wavData = new byte[newWav.wavData.Length / 2];
                int k = 0;
                for (int i = 0; i < newWav.wavData.Length; i = i + 4)
                {
                    if (i % 4 == 0)
                    {
                        buffer.wavData[k] = newWav.wavData[i];
                        buffer.wavData[k + 1] = newWav.wavData[i + 1];
                        k = k + 2;
                    }
                }
            }
            Array.Copy(newWav.LeftChData, buffer.LeftChData, newWav.LeftChData.Length);

            buffer.dataSize = buffer.LeftChData.Length * buffer.channels * buffer.bitDepth / 8;
            buffer.fileSize = (uint)(buffer.dataSize + 36);
            WAV.Writing2(buffer, Application.StartupPath + @"\" + "buffer");
            return buffer;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "D:/";
            openFileDialog.Filter = "Аудио файлы (*.wav)|*.wav"; 
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                paths.Add(openFileDialog.FileName);
                string[] splitted = openFileDialog.FileName.Split('\\');
                string name = splitted[splitted.Length - 1];
                if (name.Length > 33)
                {
                    listBox4.Items.Add(name.Substring(0, 30) + "...");
                }
                else
                {
                    int spaceCount = 57 - name.Length;
                    string[] array = new string[spaceCount];
                    for (int i = 0; i < spaceCount; i++)
                    {
                        array[i] = " ";
                    }
                    string tail = String.Concat(array);
                    listBox4.Items.Add(name + tail);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox4.Items.Count != 0)
            {
                if (listBox4.SelectedIndex == -1)
                {
                    listBox4.Items.Remove(listBox4.Items[listBox4.Items.Count - 1]);
                }
                else
                {
                    listBox4.Items.Remove(listBox4.Items[listBox4.SelectedIndex]);
                }
            }
        }

    }
}
