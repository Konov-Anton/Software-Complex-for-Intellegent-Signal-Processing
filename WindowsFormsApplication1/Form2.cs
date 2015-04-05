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
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            string[] splitted = MDIParent1.FileNames[MDIParent1.currentFile].Split('\\');
            string path = String.Join("\\",splitted, 0, splitted.Length-1);
            this.Text = "Свойства " + splitted[splitted.Length - 1];
            this.Location = new Point(300, 300);
          
            string[] row0 = { "Расположение", path };
            string[] row1 = { "Название", splitted[splitted.Length - 1] };
            string channels="";
            if (Form1.newWav.channels == 1)
                channels = Form1.newWav.channels + " (Моно)";
            else if (Form1.newWav.channels == 2)
                channels = Form1.newWav.channels + " (Стерео)";
            string[] row2 = { "Каналы", channels};
            string[] row3 = { "Разрешающая способность", Form1.newWav.bitDepth+" бит" };
            string[] row4 = { "Частота сэмплирования", Form1.newWav.sampleRate + "" };
            string[] row5 = { "Выравнивание блока", Form1.newWav.fmtBlockAlign + " байта на выборку" };
            string avgkbps = Form1.newWav.fmtAvgBPS*8/1024 + "";
            string[] row6 = { "Скорость передачи данных", avgkbps + " кбит/с"};
            string fmtCode="";
            if (Form1.newWav.fmtCode == 1)
                fmtCode = "not compressed";
            string[] row7 = { "Тип компрессии", fmtCode };
            string duration = "";
            double dur = (double)Form1.newWav.wavData.Length/(double)Form1.newWav.fmtAvgBPS;
            duration = WAV.GetDuration(dur);
            string size = Form1.newWav.wavData.Length*8/Form1.newWav.channels/Form1.newWav.bitDepth+" сэмплов";
            string[] row8 = { "Длительность", duration + "    " + size};
            double fileSize = (double)Math.Round((decimal)Form1.newWav.fileSize/1024/1024, 2);
            string[] row9 = { "Размер файла", fileSize+" Мб"};
            dataGridView1.Rows.Add(row0);
            dataGridView1.Rows.Add(row1);
            dataGridView1.Rows.Add(row2);
            dataGridView1.Rows.Add(row3);
            dataGridView1.Rows.Add(row4);
            dataGridView1.Rows.Add(row5);
            dataGridView1.Rows.Add(row6);
            dataGridView1.Rows.Add(row7);
            dataGridView1.Rows.Add(row8);
            dataGridView1.Rows.Add(row9);
        }

        private void Form2_Resize(object sender, EventArgs e)
        {
            dataGridView1.Columns[1].Width = this.Width - 279;
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            MDIParent1.SetFileInfoUncheckedValue();           
        }
    }
}
