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

namespace WindowsFormsApplication1
{
    public partial class Form7 : Form
    {
        public Form7()
        {
            InitializeComponent();
        }
        string window = "hm";
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
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

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
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

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
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
            SaveFileDialog saveFft = new SaveFileDialog();
            Directory.CreateDirectory(Application.StartupPath + "\\PLP");
            saveFft.InitialDirectory = Application.StartupPath + "\\PLP";
            saveFft.Filter = "Текстовый документ (*.txt)|*.txt";
            double[] source = WAV.GetNormalizedData(Form1.newWav);
            List<List<double>> plpList = SignalProcessing.PLP(source, (int)Form1.newWav.sampleRate, Convert.ToInt32(textBox1.Text),
                Convert.ToInt32(textBox1.Text) - Convert.ToInt32(textBox2.Text), window, 
                (int)Math.Ceiling(SignalProcessing.hzToBark(Form1.newWav.sampleRate/2))+1,
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
            if (saveFft.ShowDialog(this) == DialogResult.OK)
            {
                string FileName = saveFft.FileName;
                System.IO.File.WriteAllLines(FileName, text);
            }
            this.Close();
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (Convert.ToInt32(textBox1.Text) < 256)
                textBox1.Text = 256.ToString();
            int len = Form1.newWav.LeftChData.Length;
            int pow = 0;
            while (len > 1)
            {
                len = len / 2;
                pow++;
            }
            len = (int)Math.Pow(2, pow);
            if (Convert.ToInt32(textBox1.Text) > len)
                textBox1.Text = len.ToString();
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            if (Convert.ToInt32(textBox1.Text) < Convert.ToInt32(textBox2.Text))
                textBox2.Text = (Convert.ToInt32(textBox1.Text) * 3 / 4).ToString();
        }

        private void Form7_Load(object sender, EventArgs e)
        {
            Form1.cantBlockWindow = true;
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

    }
}
