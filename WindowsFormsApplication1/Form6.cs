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
    public partial class Form6 : Form
    {
        public Form6()
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

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
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
            saveFft.Filter = "Текстовый документ (*.txt)|*.txt"; //|Все файлы (*.*)|*.*";
            double[] normalizedData = WAV.GetNormalizedData(Form1.newWav);
            int frameLength = Convert.ToInt32(textBox1.Text);
            int frameInc = Convert.ToInt32(textBox1.Text) - Convert.ToInt32(textBox2.Text);

            List<double[]> a = new List<double[]>();
            if (MDIParent1.modeOfLpcWindow == 2)
            {
                a = SignalProcessing.GetCC(normalizedData, frameLength, frameInc, Convert.ToInt32(textBox4.Text), Convert.ToInt32(textBox3.Text), window);
                Directory.CreateDirectory(Application.StartupPath + "\\CLPC");
                saveFft.InitialDirectory = Application.StartupPath + "\\CLPC";
            }
            else
            {
                a = SignalProcessing.GetLPC(normalizedData, frameLength, frameInc, Convert.ToInt32(textBox3.Text), window);
                Directory.CreateDirectory(Application.StartupPath + "\\LPC");
                saveFft.InitialDirectory = Application.StartupPath + "\\LPC";
            }
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

           
            if (saveFft.ShowDialog(this) == DialogResult.OK)
            {               
                string FileName = saveFft.FileName;
                System.IO.File.WriteAllLines(FileName, text);
            }
            this.Close();
        }

        private void Form6_Load(object sender, EventArgs e)
        {
            Form1.cantBlockWindow = true;
            if (MDIParent1.modeOfLpcWindow == 2)
            {
                label5.Show();
                textBox4.Show();
            }
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

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

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
