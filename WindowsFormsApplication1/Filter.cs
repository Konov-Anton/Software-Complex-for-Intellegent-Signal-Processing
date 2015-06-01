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
    public partial class Filter : Form
    {
        public Filter()
        {
            InitializeComponent();
        }

        public static double[] weights;
        public static int order;
        string window = "bm";

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text != "" && textBox1.Text != "-")
            {
                if (Convert.ToDouble(textBox1.Text) < 0)
                    textBox1.Text = "0";
             
                if (Convert.ToDouble(textBox1.Text) > 0.5)
                    textBox1.Text = "0.5";
                
                
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (textBox2.Text != "")
            {
                double ratio = 4 / Convert.ToDouble(textBox2.Text);
                if (checkBox1.Checked == true)
                    textBox3.Text = (ratio * Form1.newWav.sampleRate).ToString();
            }
        }
        

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (textBox3.Text != "")
            {
                double ratio = Convert.ToDouble(textBox3.Text) / Form1.newWav.sampleRate;
                if (checkBox2.Checked == true)
                    textBox2.Text = ((int)Math.Ceiling(4 / ratio)).ToString();
            }
        }

        private void Filter_Load(object sender, EventArgs e)
        {
            Form1.cantBlockWindow = true;
            if (MDIParent1.filterMode == 1)
                this.Text = "Фильтр нижних частот";
            if (MDIParent1.filterMode == 2)
                this.Text = "Фильтр высоких частот";
            if (MDIParent1.filterMode == 3)
            {
                this.Text = "Полосовой фильтр";
                label3.Show();
                textBox4.Show();
                label3.Text = "Частота среза НЧ";
                label1.Text = "Частота среза ВЧ";
            }
            if (MDIParent1.filterMode == 4)
            {
                this.Text = "Фильтр скользящего среднего";
                label1.Hide();
                label2.Hide();
                label18.Hide();
                textBox1.Hide();
                listBox1.Hide();

            }
            if (MDIParent1.filterMode == 5)
            {
                this.Text = "Полосно-заграждающий фильтр";
                label3.Show();
                textBox4.Show();
                label3.Text = "Нижняя частота полосы";
                label1.Text = "Верхняя частота полосы";
            }
            double ratio = 4 / Convert.ToDouble(textBox2.Text);
            textBox3.Text = (ratio * Form1.newWav.sampleRate).ToString();
        }

        private void Filter_FormClosed(object sender, FormClosedEventArgs e)
        {
            Form1.cantBlockWindow = false;
            if (checkBox3.Checked == true)
                Form1.cantBlockWindow = true;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                textBox2.Enabled = true;
                checkBox2.Checked = false;
            }
            else
            {
                textBox2.Enabled = false;
            }
            
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true)
            {
                textBox3.Enabled = true;
                checkBox1.Checked = false;
            }
            else
            {
                textBox3.Enabled = false;
            }
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            if (textBox2.Text != "" && textBox2.Text != "-")
            {
                if (Convert.ToDouble(textBox2.Text) < 10)
                    textBox2.Text = "10";
                if (Convert.ToDouble(textBox2.Text) > 16000)
                    textBox2.Text = "16000";
                double ratio = 4 / Convert.ToDouble(textBox2.Text);
                textBox3.Text = (ratio * Form1.newWav.sampleRate).ToString();
            }
        }

        private void textBox3_Leave(object sender, EventArgs e)
        {
            if (textBox3.Text != "" && textBox3.Text != "-")
            {
                if (Convert.ToDouble(textBox3.Text) < 0)
                    textBox3.Text = "10";
                if (Convert.ToDouble(textBox3.Text) > Form1.newWav.sampleRate / 4)
                    textBox3.Text = (Form1.newWav.sampleRate / 4).ToString();
                double ratio = Convert.ToDouble(textBox3.Text) / Form1.newWav.sampleRate;
                textBox2.Text = ((int)Math.Ceiling(4 / ratio)).ToString();
            }
        }

        private void textBox3_KeyPress_1(object sender, KeyPressEventArgs e)
        {
            if (Char.IsDigit(e.KeyChar))
            {
            }
            else
                if (Char.IsControl(e.KeyChar) && textBox3.Text!="")
                {
                }
                    else
                    {
                        e.Handled = true;
                    }
        }

        private void textBox2_KeyPress_1(object sender, KeyPressEventArgs e)
        {
            if (Char.IsDigit(e.KeyChar))
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

        private void button1_Click(object sender, EventArgs e)
        {
            MDIParent1.changesHaveBeenMade = true;
            if (MDIParent1.filterMode == 1)
            {
                WAV buffer = CreateBuffer(Form1.newWav);
                double Fc;
                if (textBox1.Text != "")
                    Fc = Convert.ToDouble(textBox1.Text);
                else 
                    Fc = 0.15;
                order = Convert.ToInt32(textBox2.Text);
                if (order % 2 == 0)
                    order++;
                buffer.LeftChData = Filters.LowPassFilter(buffer.LeftChData, Fc, order, window, out weights);
                buffer.wavData = WAV.ComputeBytesOfWavData(buffer.LeftChData);
                Form1.buf[0] = Form1.newWav;
                Form1.buf[1] = buffer;
                Form1.newWav = CreateBuffer(buffer);
                for (int i = 0; i < Form1.newWav.LeftChData.Length; i++)
                    Form1.newWav.LeftChData[i] *= 32768;
                MDIParent1.newMDIChildForm1.Width += 1;
                if (checkBox3.Checked == true)
                {
                    Form1.cantBlockWindow = true;
                    Pictures form = new Pictures();
                    MDIParent1.pictureMode = 5;
                    form.Show();
                }
                this.Close();
            }
            if (MDIParent1.filterMode == 2)
            {
                WAV buffer = CreateBuffer(Form1.newWav);
                double Fc;
                if (textBox1.Text != "")
                    Fc = Convert.ToDouble(textBox1.Text);
                else
                    Fc = 0.15;
                order = Convert.ToInt32(textBox2.Text);
                if (order % 2 == 0)
                    order++;
                buffer.LeftChData = Filters.HighPassFilter(buffer.LeftChData, Fc, order, window, out weights);
                buffer.wavData = WAV.ComputeBytesOfWavData(buffer.LeftChData);
                Form1.buf[0] = Form1.newWav;
                Form1.buf[1] = buffer;
                Form1.newWav = CreateBuffer(buffer);
                for (int i = 0; i < Form1.newWav.LeftChData.Length; i++)
                    Form1.newWav.LeftChData[i] *= 32768;
                MDIParent1.newMDIChildForm1.Width += 1;
                if (checkBox3.Checked == true)
                {
                    Form1.cantBlockWindow = true;
                    Pictures form = new Pictures();
                    MDIParent1.pictureMode = 5;
                    form.Show();
                }
                this.Close();
            }
            if (MDIParent1.filterMode == 3)
            {
                WAV buffer = CreateBuffer(Form1.newWav);
                double FcHigh;
                double FcLow;
                if (textBox1.Text != "")
                    FcHigh = Convert.ToDouble(textBox1.Text);
                else
                    FcHigh = 0.3;
                if (textBox4.Text != "")
                    FcLow = Convert.ToDouble(textBox4.Text);
                else
                    FcLow = 0.2;
                order = Convert.ToInt32(textBox2.Text);
                if (order % 2 == 0)
                    order++;
                
                buffer.LeftChData = Filters.BandPassFilter(buffer.LeftChData, FcLow, FcHigh, order, window, out weights);
                buffer.wavData = WAV.ComputeBytesOfWavData(buffer.LeftChData);
                Form1.buf[0] = Form1.newWav;
                Form1.buf[1] = buffer;
                Form1.newWav = CreateBuffer(buffer);
                for (int i = 0; i < Form1.newWav.LeftChData.Length; i++)
                    Form1.newWav.LeftChData[i] *= 32768;
                MDIParent1.newMDIChildForm1.Width += 1;
                if (checkBox3.Checked == true)
                {
                    Form1.cantBlockWindow = true;
                    Pictures form = new Pictures();
                    MDIParent1.pictureMode = 5;
                    form.Show();
                }
                this.Close();
            }
            if (MDIParent1.filterMode == 4)
            {
                checkBox3.Hide();
                WAV buffer = CreateBuffer(Form1.newWav);
                order = Convert.ToInt32(textBox2.Text);
                if (order % 2 == 0)
                    order++;
                weights = new double[order];
                for (int i = 0; i < order; i++)
                    weights[i] = (double)1 / order;
                buffer.LeftChData = Filters.MovingAverageFilter(buffer.LeftChData, order);
                buffer.wavData = WAV.ComputeBytesOfWavData(buffer.LeftChData);
                Form1.buf[0] = Form1.newWav;
                Form1.buf[1] = buffer;
                Form1.newWav = CreateBuffer(buffer);
                for (int i = 0; i < Form1.newWav.LeftChData.Length; i++)
                    Form1.newWav.LeftChData[i] *= 32768;
                MDIParent1.newMDIChildForm1.Width += 1;
                if (checkBox3.Checked == true)
                {
                    Form1.cantBlockWindow = true;
                    Pictures form = new Pictures();
                    MDIParent1.pictureMode = 5;
                    form.Show();
                }
                this.Close();
            }
            if (MDIParent1.filterMode == 5)
            {
                WAV buffer = CreateBuffer(Form1.newWav);
                double FcHigh;
                double FcLow;
                if (textBox1.Text != "")
                    FcHigh = Convert.ToDouble(textBox1.Text);
                else
                    FcHigh = 0.3;
                if (textBox4.Text != "")
                    FcLow = Convert.ToDouble(textBox4.Text);
                else
                    FcLow = 0.2;
                order = Convert.ToInt32(textBox2.Text);
                if (order % 2 == 0)
                    order++;

                buffer.LeftChData = Filters.BandStopFilter(buffer.LeftChData, FcLow, FcHigh, order, window, out weights);
                buffer.wavData = WAV.ComputeBytesOfWavData(buffer.LeftChData);
                Form1.buf[0] = Form1.newWav;
                Form1.buf[1] = buffer;
                Form1.newWav = CreateBuffer(buffer);
                for (int i = 0; i < Form1.newWav.LeftChData.Length; i++)
                    Form1.newWav.LeftChData[i] *= 32768;
                MDIParent1.newMDIChildForm1.Width += 1;
                if (checkBox3.Checked == true)
                {
                    Form1.cantBlockWindow = true;
                    Pictures form = new Pictures();
                    MDIParent1.pictureMode = 5;
                    form.Show();
                }
                this.Close();
            }
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
                buffer.wavData = new byte[newWav.wavData.Length/2];
                int k=0;
                for (int i=0; i<newWav.wavData.Length; i=i+4)
                {
                    if (i % 4 == 0)
                    {
                        buffer.wavData[k] = newWav.wavData[i];
                        buffer.wavData[k+1] = newWav.wavData[i+1];
                        k=k+2;
                    }
                }
            }
            Array.Copy(newWav.LeftChData, buffer.LeftChData, newWav.LeftChData.Length);
           
            buffer.dataSize = buffer.LeftChData.Length * buffer.channels * buffer.bitDepth / 8;
            buffer.fileSize = (uint)(buffer.dataSize + 36);
            WAV.Writing2(buffer, Application.StartupPath + @"\" + "buffer");
            return buffer;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == 0)
                window = "hm";
            if (listBox1.SelectedIndex == 1)
                window = "hn";
            if (listBox1.SelectedIndex == 2)
                window = "bm";
            if (listBox1.SelectedIndex == 3)
                window = "rect";
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

    }
}
