using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace WindowsFormsApplication1
{
    public partial class Form8 : Form
    {
        public Form8()
        {
            InitializeComponent();
        }
        List<List<string>> learningFiles = new List<List<string>>();
        List<string> testFiles = new List<string>();
        List<double[][]> codebooks = new List<double[][]>();
        List<GaussianMixtureModel[]> GMMmodels = new List<GaussianMixtureModel[]>();
        double[] weights;
        double[][] multiclassweights;
        double threshold;
        double[] thresholds;
        string window = "hm";
        double[][] SVMtrainingData;
        bool modelsWasLoad = false;
        private void button5_Click(object sender, EventArgs e)
        {
            if (textBox2.Text != "")
            {
                List<string> list = new List<string>();
                learningFiles.Add(list);
                listBox1.Items.Add(textBox2.Text);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count != 0)
            {
                if(listBox1.SelectedIndex == -1)
                {
                    listBox1.SelectedIndex = 0;
                }
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.InitialDirectory = "D:/";
                openFileDialog.Filter = "Аудио файлы (*.wav)|*.wav";
                if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    learningFiles[listBox1.SelectedIndex].Add(openFileDialog.FileName);
                    string[] splitted = openFileDialog.FileName.Split('\\');
                    string name = splitted[splitted.Length - 1];
                    if (name.Length > 33)
                    {
                        listView1.Items.Add(name.Substring(0, 30) + "...");
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
                        listView1.Items.Add(name + tail);
                    }
                }
            }
        }

        private void listView1_ItemMouseHover(object sender, ListViewItemMouseHoverEventArgs e)
        {
            if (e.Item != null)
            {
                e.Item.ToolTipText = learningFiles[listBox1.SelectedIndex][e.Item.Index];                
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (modelsWasLoad == false)
            {
                if (listBox1.SelectedIndex != -1)
                {
                    listView1.Clear();
                    for (int i = 0; i < learningFiles[listBox1.SelectedIndex].Count; i++)
                    {
                        string[] splitted = learningFiles[listBox1.SelectedIndex][i].Split('\\');
                        string name = splitted[splitted.Length - 1];
                        if (name.Length > 33)
                        {
                            listView1.Items.Add(name.Substring(0, 30) + "...");
                        }
                        else
                        {
                            int spaceCount = 57 - name.Length;
                            string[] array = new string[spaceCount];
                            for (int j = 0; j < spaceCount; j++)
                            {
                                array[j] = " ";
                            }
                            string tail = String.Concat(array);
                            listView1.Items.Add(name + tail);
                        }
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button4.Enabled = true;
            if (MDIParent1.classificationMode == 1)
            {
                int classes = learningFiles.Count;
                List<List<List<double>>> coefs = new List<List<List<double>>>();

                for (int l = 0; l < classes; l++)
                {
                    Stopwatch SW = new Stopwatch();
                    SW.Start();
                    //textBox1.Text += "Начато извлечение характеристик сигналов для языка" + (l + 1).ToString() + "\n";
                    List<List<double>> coefsOneLang = new List<List<double>>();
                    coefs.Add(coefsOneLang);
                    int vectorIndex = 0;
                    for (int f = 0; f < learningFiles[l].Count; f++)
                    {
                        WAV file = WAV.Reading(learningFiles[l][f]);
                        double[] source = WAV.GetNormalizedData(file);
                        if (listBox6.SelectedIndex == 1 || listBox6.SelectedIndex == -1)
                        {
                            List<List<double>> mffcList = SignalProcessing.MFCC(source, Convert.ToInt32(textBox7.Text),
                                Convert.ToInt32(textBox7.Text) - Convert.ToInt32(textBox8.Text),
                                Convert.ToInt32(textBox6.Text),
                                26,
                                (int)file.sampleRate,
                                0,
                                (int)file.sampleRate / 2,
                                window);

                            for (int i = 0; i < mffcList.Count; i++)
                            {
                                List<double> vector = new List<double>();
                                coefs[l].Add(vector);
                                coefs[l][vectorIndex] = mffcList[i];
                                vectorIndex++;
                            }
                        }
                        if (listBox6.SelectedIndex == 0)
                        {
                            List<double[]> lpcList = SignalProcessing.GetCC(source, Convert.ToInt32(textBox7.Text),
                                Convert.ToInt32(textBox7.Text) - Convert.ToInt32(textBox8.Text),
                                Convert.ToInt32(textBox6.Text), Convert.ToInt32(textBox6.Text), window);
                            for (int i=0; i<lpcList.Count; i++)
                            {
                                List<double> vector = new List<double>();
                                coefs[l].Add(vector);
                                List<double> list = lpcList[i].ToList();
                                list.Remove(list[0]);
                                coefs[l][vectorIndex] = list;
                                vectorIndex++;
                            }
                                
                        }
                        if (listBox6.SelectedIndex == 2)
                        {
                            List<List<double>> plpList = SignalProcessing.PLP(source, (int)file.sampleRate,
                                Convert.ToInt32(textBox7.Text),
                                Convert.ToInt32(textBox7.Text) - Convert.ToInt32(textBox8.Text),
                                window,
                                27,
                                Convert.ToInt32(textBox6.Text));
                            for (int i = 0; i < plpList.Count; i++)
                            {
                                List<double> vector = new List<double>();
                                coefs[l].Add(vector);
                                plpList[i].Remove(plpList[i][0]);
                                coefs[l][vectorIndex] = plpList[i];
                                vectorIndex++;
                            }
                        }
                    }
                    SW.Stop();
                    if(MDIParent1.identificationTask == 1)
                        textBox1.Text += "Извлечение характеристик сигналов для диктора " + listBox1.Items[l].ToString() + " завершено за " + SW.Elapsed.Minutes + " минут " + SW.Elapsed.Seconds + " секунд " + SW.Elapsed.Milliseconds + " милисекунд" + "\r\n";
                    if (MDIParent1.identificationTask == 2)
                        textBox1.Text += "Извлечение характеристик сигналов для языка " + listBox1.Items[l].ToString() + " завершено за " + SW.Elapsed.Minutes + " минут " + SW.Elapsed.Seconds + " секунд " + SW.Elapsed.Milliseconds + " милисекунд" + "\r\n";
                    if (MDIParent1.identificationTask == 3)
                    {
                        if (l==0)
                            textBox1.Text += "Извлечение характеристик сигналов для мужского пола " + "завершено за " + SW.Elapsed.Minutes + " минут " + SW.Elapsed.Seconds + " секунд " + SW.Elapsed.Milliseconds + " милисекунд" + "\r\n";
                        else
                            textBox1.Text += "Извлечение характеристик сигналов для женского пола " + "завершено за " + SW.Elapsed.Minutes + " минут " + SW.Elapsed.Seconds + " секунд " + SW.Elapsed.Milliseconds + " милисекунд" + "\r\n";
                    }
                    double[][] data = new double[coefs[l].Count][]; //количество векторов
                    for (int i = 0; i < data.Length; i++)
                        data[i] = coefs[l][i].ToArray();
                    Stopwatch SW2 = new Stopwatch();
                    SW2.Start();
                    int[] clustering;
                    double[][] codebook = VectorQuantinization.CreateCodebook(data,
                        Convert.ToInt32(textBox5.Text),
                        Convert.ToInt32(textBox6.Text),
                        Convert.ToInt32(textBox3.Text),
                        out clustering);
                    SW2.Stop();
                    //textBox1.Text += "Обучение модели для языка" + (l + 1).ToString() + "завершено за " + SW2.Elapsed.Minutes + " минут " + SW2.Elapsed.Seconds + " секунд" + SW2.Elapsed.Milliseconds + " милисекунд" + "\r\n";
                    if (MDIParent1.identificationTask == 1)
                        textBox1.Text += "Обучение модели для диктора " + listBox1.Items[l].ToString() + " завершено за " + SW2.Elapsed.Minutes + " минут " + SW2.Elapsed.Seconds + " секунд " + SW2.Elapsed.Milliseconds + " милисекунд" + "\r\n";
                    if (MDIParent1.identificationTask == 2)
                        textBox1.Text += "Обучение модели для языка " + listBox1.Items[l].ToString() + " завершено за " + SW2.Elapsed.Minutes + " минут " + SW2.Elapsed.Seconds + " секунд " + SW2.Elapsed.Milliseconds + " милисекунд" + "\r\n";
                    if (MDIParent1.identificationTask == 3)
                    {
                        if (l == 0)
                            textBox1.Text += "Обучение модели для мужского пола " + "завершено за " + SW2.Elapsed.Minutes + " минут " + SW2.Elapsed.Seconds + " секунд " + SW2.Elapsed.Milliseconds + " милисекунд" + "\r\n";
                        else
                            textBox1.Text += "Обучение модели для женского пола " + "завершено за " + SW2.Elapsed.Minutes + " минут " + SW2.Elapsed.Seconds + " секунд " + SW2.Elapsed.Milliseconds + " милисекунд" + "\r\n";
                    }
                    codebooks.Add(codebook);

                    //сохранение модели
                    if (checkBox1.Checked == true)
                    {
                        int cl = Convert.ToInt32(textBox5.Text);
                        Directory.CreateDirectory(Application.StartupPath + "\\Модели\\VQ");
                        string path = Application.StartupPath + "\\Модели\\VQ" + "\\" + listBox1.Items[l] + ".txt";
                        string[] lines = new string[cl];
                        for (int i=0; i<cl; i++)
                            for (int j=0; j<codebook[i].Length; j++)
                            {
                                if (j != codebook[i].Length - 1)
                                    lines[i] += codebook[i][j] + " ";
                                else
                                    lines[i] += codebook[i][j];
                            }
                        File.WriteAllLines(path, lines);
                    }
                }
            }
            if (MDIParent1.classificationMode == 2)
            {
                int classes = learningFiles.Count;
                
                List<List<List<double>>> coefs = new List<List<List<double>>>();

                for (int l = 0; l < classes; l++)
                {
                    Stopwatch SW = new Stopwatch();
                    SW.Start();
                    //textBox1.Text += "Начато извлечение характеристик сигналов для языка" + (l + 1).ToString() + "\n";
                    List<List<double>> coefsOneLang = new List<List<double>>();
                    coefs.Add(coefsOneLang);
                    int vectorIndex = 0;
                    for (int f = 0; f < learningFiles[l].Count; f++)
                    {
                        WAV file = WAV.Reading(learningFiles[l][f]);
                        double[] source = WAV.GetNormalizedData(file);
                        if (listBox6.SelectedIndex == 1 || listBox6.SelectedIndex == -1)
                        {
                            List<List<double>> mffcList = SignalProcessing.MFCC(source, Convert.ToInt32(textBox7.Text),
                                Convert.ToInt32(textBox7.Text) - Convert.ToInt32(textBox8.Text),
                                Convert.ToInt32(textBox6.Text),
                                26,
                                (int)file.sampleRate,
                                0,
                                (int)file.sampleRate / 2,
                                window);

                            for (int i = 0; i < mffcList.Count; i++)
                            {
                                List<double> vector = new List<double>();
                                coefs[l].Add(vector);
                                coefs[l][vectorIndex] = mffcList[i];
                                vectorIndex++;
                            }
                        }
                        if (listBox6.SelectedIndex == 0)
                        {
                            List<double[]> lpcList = SignalProcessing.GetCC(source, Convert.ToInt32(textBox7.Text),
                                Convert.ToInt32(textBox7.Text) - Convert.ToInt32(textBox8.Text),
                                Convert.ToInt32(textBox6.Text), Convert.ToInt32(textBox6.Text), window);
                            for (int i = 0; i < lpcList.Count; i++)
                            {
                                List<double> vector = new List<double>();
                                coefs[l].Add(vector);
                                List<double> list = lpcList[i].ToList();
                                list.Remove(list[0]);
                                coefs[l][vectorIndex] = list;
                                vectorIndex++;
                            }
                        }
                        if (listBox6.SelectedIndex == 2)
                        {
                            List<List<double>> plpList = SignalProcessing.PLP(source, (int)file.sampleRate,
                                Convert.ToInt32(textBox7.Text),
                                Convert.ToInt32(textBox7.Text) - Convert.ToInt32(textBox8.Text),
                                window,
                                27,
                                Convert.ToInt32(textBox6.Text));
                            for (int i = 0; i < plpList.Count; i++)
                            {
                                List<double> vector = new List<double>();
                                coefs[l].Add(vector);
                                plpList.Remove(plpList[0]);
                                coefs[l][vectorIndex] = plpList[i];
                                vectorIndex++;
                            }
                        }
                    }
                    SW.Stop();
                    //textBox1.Text += "Извлечение характеристик сигналов для языка" + (l + 1).ToString() + "завершено за " + SW.Elapsed.Minutes + " минут " + SW.Elapsed.Seconds + " секунд" + "\r\n";
                    if (MDIParent1.identificationTask == 1)
                        textBox1.Text += "Извлечение характеристик сигналов для диктора " + listBox1.Items[l].ToString() + " завершено за " + SW.Elapsed.Minutes + " минут " + SW.Elapsed.Seconds + " секунд" + "\r\n";
                    if (MDIParent1.identificationTask == 2)
                        textBox1.Text += "Извлечение характеристик сигналов для языка " + listBox1.Items[l].ToString() + " завершено за " + SW.Elapsed.Minutes + " минут " + SW.Elapsed.Seconds + " секунд" + "\r\n";
                    if (MDIParent1.identificationTask == 3)
                    {
                        if (l == 0)
                            textBox1.Text += "Извлечение характеристик сигналов для мужского пола " + "завершено за " + SW.Elapsed.Minutes + " минут " + SW.Elapsed.Seconds + " секунд" + "\r\n";
                        else
                            textBox1.Text += "Извлечение характеристик сигналов для женского пола " + "завершено за " + SW.Elapsed.Minutes + " минут " + SW.Elapsed.Seconds + " секунд" + "\r\n";
                    }
                    double[][] data = new double[coefs[l].Count][]; //количество векторов
                    for (int i = 0; i < data.Length; i++)
                        data[i] = coefs[l][i].ToArray();
                    Stopwatch SW2 = new Stopwatch();
                    SW2.Start();
                    GaussianMixtureModel[] modelComponents = GaussianMixtureModel.Training(data,
                        Convert.ToInt32(textBox9.Text),
                        Convert.ToDouble(textBox13.Text),
                        Convert.ToInt32(textBox4.Text));
                    SW2.Stop();
                    //textBox1.Text += "Обучение модели для языка " + (l + 1).ToString() + " завершено за " + SW2.Elapsed.Minutes + " минут " + SW2.Elapsed.Seconds + " секунд" + "\r\n";
                    if (MDIParent1.identificationTask == 1)
                        textBox1.Text += "Обучение модели для диктора " + listBox1.Items[l].ToString() + " завершено за " + SW2.Elapsed.Hours + " часов" + SW2.Elapsed.Minutes + " минут " + SW2.Elapsed.Seconds + " секунд" + "\r\n";
                    if (MDIParent1.identificationTask == 2)
                        textBox1.Text += "Обучение модели для языка " + listBox1.Items[l].ToString() + " завершено за " + SW2.Elapsed.Hours + " часов" + SW2.Elapsed.Minutes + " минут " + SW2.Elapsed.Seconds + " секунд" + "\r\n";
                    if (MDIParent1.identificationTask == 3)
                    {
                        if (l == 0)
                            textBox1.Text += "Обучение модели для мужского пола " + "завершено за " + SW2.Elapsed.Hours + " часов" + SW2.Elapsed.Minutes + " минут " + SW2.Elapsed.Seconds + " секунд" + "\r\n";
                        else
                            textBox1.Text += "Обучение модели для женского пола " + "завершено за " + SW2.Elapsed.Hours + " часов" + SW2.Elapsed.Minutes + " минут " + SW2.Elapsed.Seconds + " секунд" + "\r\n";
                    }
                    GMMmodels.Add(modelComponents);

                    //сохранение модели
                    if (checkBox1.Checked == true)
                    {
                        int order = Convert.ToInt32(textBox9.Text);
                        Directory.CreateDirectory(Application.StartupPath + "\\Модели\\GMM");
                        string path = Application.StartupPath + "\\Модели\\GMM" + "\\" + listBox1.Items[l] + ".txt";
                        int len = modelComponents[0].means.Length;
                        string[] lines = new string[order * (2 + len) + 1]; // средние и веса в строчку + высота матрицы ковариации + порядок модели
                        int stringCounter = 0;
                        lines[stringCounter] = order + "";
                        stringCounter++;
                        for (int j = 0; j < order; j++)
                        {
                            for (int i=0; i<len; i++)
                            {
                                if (i == len - 1)
                                    lines[stringCounter] += modelComponents[j].means[i];
                                else
                                    lines[stringCounter] += modelComponents[j].means[i] + " ";
                            }
                            stringCounter++;

                            for (int x=0; x<len; x++)
                            {
                                for (int y=0; y<len; y++)
                                {
                                    if (y != len - 1)
                                        lines[stringCounter] += modelComponents[j].covariance[x][y] + " ";
                                    else
                                        lines[stringCounter] += modelComponents[j].covariance[x][y];
                                }
                                stringCounter++;
                            }

                            lines[stringCounter] = modelComponents[j].pi + "";
                            stringCounter++;
                        }
                        File.WriteAllLines(path, lines);
                    }
                }
            }
            if(MDIParent1.classificationMode == 3)
            {
                int classes = learningFiles.Count;
                
                List<List<double>> coefs = new List<List<double>>();
                List<int> labelsList = new List<int>();
                int vectorIndex = 0;
                
                for (int l = 0; l < classes; l++)
                {
                    Stopwatch SW = new Stopwatch();
                    SW.Start();
                    //textBox1.Text += "Начато извлечение характеристик сигналов для языка" + (l + 1).ToString() + "\n";
                    
                    for (int f = 0; f < learningFiles[l].Count; f++)
                    {
                        WAV file = WAV.Reading(learningFiles[l][f]);
                        double[] source = WAV.GetNormalizedData(file);
                        if (listBox6.SelectedIndex == 1 || listBox6.SelectedIndex == -1)
                        {
                            List<List<double>> mffcList = SignalProcessing.MFCC(source, Convert.ToInt32(textBox7.Text),
                                Convert.ToInt32(textBox7.Text) - Convert.ToInt32(textBox8.Text),
                                Convert.ToInt32(textBox6.Text),
                                26,
                                (int)file.sampleRate,
                                0,
                                (int)file.sampleRate / 2,
                                window);

                            for (int i = 0; i < mffcList.Count; i++)
                            {
                                List<double> vector = new List<double>();
                                coefs.Add(vector);
                                coefs[vectorIndex] = mffcList[i];
                                if (classes < 3)
                                {
                                    if (l == 0)
                                        labelsList.Add(-1);
                                    else
                                        labelsList.Add(1);
                                }
                                else
                                {
                                    labelsList.Add(l);
                                }
                                //labelsList.Add(l);
                                vectorIndex++;
                            }
                        }
                        if (listBox6.SelectedIndex == 0)
                        {
                            List<double[]> lpcList = SignalProcessing.GetCC(source, Convert.ToInt32(textBox7.Text),
                                Convert.ToInt32(textBox7.Text) - Convert.ToInt32(textBox8.Text),
                                Convert.ToInt32(textBox6.Text), Convert.ToInt32(textBox6.Text), window);
                            for (int i = 0; i < lpcList.Count; i++)
                            {
                                List<double> vector = new List<double>();
                                coefs.Add(vector);
                                List<double> list = lpcList[i].ToList();
                                list.Remove(list[0]);
                                coefs[vectorIndex] = list;
                                if (classes < 3)
                                {
                                    if (l == 0)
                                        labelsList.Add(-1);
                                    else
                                        labelsList.Add(1);
                                }
                                else
                                {
                                    labelsList.Add(l);
                                }
                                vectorIndex++;
                            }
                        }
                        if (listBox6.SelectedIndex == 2)
                        {
                            List<List<double>> plpList = SignalProcessing.PLP(source, (int)file.sampleRate,
                                Convert.ToInt32(textBox7.Text),
                                Convert.ToInt32(textBox7.Text) - Convert.ToInt32(textBox8.Text),
                                window,
                                27,
                                Convert.ToInt32(textBox6.Text));
                            for (int i = 0; i < plpList.Count; i++)
                            {
                                List<double> vector = new List<double>();
                                coefs.Add(vector);
                                plpList.Remove(plpList[0]);
                                coefs[vectorIndex] = plpList[i];
                                if (classes < 3)
                                {
                                    if (l == 0)
                                        labelsList.Add(-1);
                                    else
                                        labelsList.Add(1);
                                }
                                else
                                {
                                    labelsList.Add(l);
                                }
                                //labelsList.Add(l);
                                vectorIndex++;
                            }
                        }
                    }
                    SW.Stop();
                    //textBox1.Text += "Извлечение характеристик сигналов для языка" + (l + 1).ToString() + "завершено за " + SW.Elapsed.Minutes + " минут " + SW.Elapsed.Seconds + " секунд" + "\r\n";
                    if (MDIParent1.identificationTask == 1)
                        textBox1.Text += "Извлечение характеристик сигналов для диктора " + listBox1.Items[l].ToString() + " завершено за " + SW.Elapsed.Minutes + " минут " + SW.Elapsed.Seconds + " секунд" + "\r\n";
                    if (MDIParent1.identificationTask == 2)
                        textBox1.Text += "Извлечение характеристик сигналов для языка " + listBox1.Items[l].ToString() + " завершено за " + SW.Elapsed.Minutes + " минут " + SW.Elapsed.Seconds + " секунд" + "\r\n";
                    if (MDIParent1.identificationTask == 3)
                    {
                        if (l == 0)
                            textBox1.Text += "Извлечение характеристик сигналов для мужского пола " + "завершено за " + SW.Elapsed.Minutes + " минут " + SW.Elapsed.Seconds + " секунд" + "\r\n";
                        else
                            textBox1.Text += "Извлечение характеристик сигналов для женского пола " + "завершено за " + SW.Elapsed.Minutes + " минут " + SW.Elapsed.Seconds + " секунд" + "\r\n";
                    }
                }
                    SVMtrainingData = new double[coefs.Count][]; //количество векторов
                    double max = Double.MinValue;
                    for (int i = 0; i < coefs.Count; i++)
                    {
                        if (max < coefs[i].Max())
                            max = coefs[i].Max();
                    }
                    for (int i = 0; i < coefs.Count; i++)
                    {
                        for (int j = 0; j < coefs[i].Count; j++)
                        {
                            if (coefs[i][j] < 0)
                                coefs[i][j] /= -max;
                            if (coefs[i][j] >= 0)
                                coefs[i][j] /= max;
                        }
                        SVMtrainingData[i] = coefs[i].ToArray();
                    }
                    int[] labels = labelsList.ToArray();
                    Stopwatch SW2 = new Stopwatch();
                    SW2.Start();
                    if (classes > 2)
                        multiclassweights = SupportVectorMachine.MultiClassesTraining(SVMtrainingData, labels, classes, Convert.ToDouble(textBox14.Text), out thresholds);
                    else
                        weights = SupportVectorMachine.Training(SVMtrainingData, labels, Convert.ToDouble(textBox14.Text), out threshold);
                    SW2.Stop();
                    textBox1.Text += "Обучение модели " + "завершено за " + SW2.Elapsed.Hours+ "часов" +  SW2.Elapsed.Minutes + " минут " + SW2.Elapsed.Seconds + " секунд " + SW2.Elapsed.Milliseconds + " милисекунд" + "\r\n";
                    
                    //сохранение модели
                    if (checkBox1.Checked == true)
                    {
                        if (classes > 2)
                        {
                            int len = SVMtrainingData.Length;
                            Directory.CreateDirectory(Application.StartupPath + "\\Модели\\SVM");
                            string path = Application.StartupPath + "\\Модели\\SVM" + "\\" + listBox1.Items[0] + ".txt";
                            string[] lines = new string[(multiclassweights.Length + 1) * len + multiclassweights.Length];
                            for (int i = 0; i < len; i++)
                                for (int j = 0; j < SVMtrainingData[i].Length; j++)
                                {
                                    if (j != SVMtrainingData[i].Length - 1)
                                        lines[i] += SVMtrainingData[i][j] + " ";
                                    else
                                        lines[i] += SVMtrainingData[i][j];
                                }
                            for (int p = 0; p < multiclassweights.Length; p++)
                                for (int i = len; i < 2 * len; i++)
                                {
                                    lines[i+p*len] += multiclassweights[p][i - len];
                                }
                            for (int p = 0; p < multiclassweights.Length; p++)
                                lines[(multiclassweights.Length + 1) * len + p] = thresholds[p] + "";
                            File.WriteAllLines(path, lines);
                        }
                        else
                        {
                            int len = SVMtrainingData.Length;
                            Directory.CreateDirectory(Application.StartupPath + "\\Модели\\SVM");
                            string path = Application.StartupPath + "\\Модели\\SVM" + "\\" + listBox1.Items[0] + ".txt";
                            string[] lines = new string[2 * len + 1];
                            for (int i = 0; i < len; i++)
                                for (int j = 0; j < SVMtrainingData[i].Length; j++)
                                {
                                    if (j != SVMtrainingData[i].Length - 1)
                                        lines[i] += SVMtrainingData[i][j] + " ";
                                    else
                                        lines[i] += SVMtrainingData[i][j];
                                }
                            for (int i = len; i < 2 * len; i++)
                            {
                                lines[i] += weights[i - len];
                            }
                            lines[2 * len] = threshold + "";
                            File.WriteAllLines(path, lines);
                        }
                    }
            }
            
        }

        private void listBox7_SelectedIndexChanged(object sender, EventArgs e)
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

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "D:/";
            openFileDialog.Filter = "Аудио файлы (*.wav)|*.wav";
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                testFiles.Add(openFileDialog.FileName);
                string[] splitted = openFileDialog.FileName.Split('\\');
                string name = splitted[splitted.Length - 1];
                if (name.Length > 33)
                {
                    listBox5.Items.Add(name.Substring(0, 30) + "...");
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
                    listBox5.Items.Add(name + tail);
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (MDIParent1.classificationMode == 1)
            {
                int classes = learningFiles.Count;
                for (int f = 0; f < listBox5.Items.Count; f++)
                {
                    WAV file = WAV.Reading(testFiles[f]);
                    double[] source = WAV.GetNormalizedData(file);
                    double[][] test; 
                    if (listBox6.SelectedIndex == -1 || listBox6.SelectedIndex == 1)
                    {
                        List<List<double>> mffcList = SignalProcessing.MFCC(source, Convert.ToInt32(textBox7.Text),
                                    Convert.ToInt32(textBox7.Text) - Convert.ToInt32(textBox8.Text),
                                    Convert.ToInt32(textBox6.Text),
                                    26,
                                    (int)file.sampleRate,
                                    0,
                                    (int)file.sampleRate / 2,
                                    window);
                        test = new double[mffcList.Count][];
                        for (int i = 0; i < test.Length; i++)
                            test[i] = mffcList[i].ToArray();
                    }
                    else
                        if(listBox6.SelectedIndex == 0)
                        {
                            List<double[]> lpcList = SignalProcessing.GetCC(source, Convert.ToInt32(textBox7.Text),
                                    Convert.ToInt32(textBox7.Text) - Convert.ToInt32(textBox8.Text),
                                    Convert.ToInt32(textBox6.Text), Convert.ToInt32(textBox6.Text), window);
                            test = new double[lpcList.Count][];
                            for (int i = 0; i < lpcList.Count; i++)
                            {
                                double[] temp = new double[lpcList[i].Length - 1];
                                Array.Copy(lpcList[i], 1, temp, 0, temp.Length);
                                test[i] = temp;
                            }
                        }
                        else
                            {
                                List<List<double>> plpList = SignalProcessing.PLP(source, (int)file.sampleRate,
                                        Convert.ToInt32(textBox7.Text),
                                        Convert.ToInt32(textBox7.Text) - Convert.ToInt32(textBox8.Text),
                                        window,
                                        27,
                                        Convert.ToInt32(textBox6.Text));
                                test = new double[plpList.Count][];
                                for (int i = 0; i < plpList.Count; i++)
                                {
                                    plpList[i].Remove(plpList[i][0]);
                                    test[i] = plpList[i].ToArray();
                                }
                            }
                    double[][][] cbs = codebooks.ToArray();
                    Stopwatch SW = new Stopwatch();
                    SW.Start();
                    int result = VectorQuantinization.VQIdentification(cbs, test);
                    SW.Stop();
                    textBox1.Text += "Распознавание файла " + listBox5.Items[f] + " завершено за " + SW.Elapsed.Minutes + " минут " + SW.Elapsed.Seconds + " секунд " + SW.Elapsed.Milliseconds + " милисекунд" + "\r\n";    
                    if (MDIParent1.identificationTask == 1)
                        textBox1.Text += "Файл " + listBox5.Items[f] + " отнесен к диктору " + listBox1.Items[result].ToString() + "\r\n";
                    if (MDIParent1.identificationTask == 2)
                        textBox1.Text += "Файл " + listBox5.Items[f] + " отнесен к языку " + listBox1.Items[result].ToString() + "\r\n";
                    if (MDIParent1.identificationTask == 3)
                        textBox1.Text += "Файл " + listBox5.Items[f] + " отнесен к полу " + listBox1.Items[result].ToString() + "\r\n";
                }
            }
            if (MDIParent1.classificationMode == 2)
            {
                int classes = learningFiles.Count;
                for (int f = 0; f < listBox5.Items.Count; f++)
                {
                    WAV file = WAV.Reading(testFiles[f]);
                    double[] source = WAV.GetNormalizedData(file);
                    double[][] test;
                    if (listBox6.SelectedIndex == -1 || listBox6.SelectedIndex == 1)
                    {
                        List<List<double>> mffcList = SignalProcessing.MFCC(source, Convert.ToInt32(textBox7.Text),
                                    Convert.ToInt32(textBox7.Text) - Convert.ToInt32(textBox8.Text),
                                    Convert.ToInt32(textBox6.Text),
                                    26,
                                    (int)file.sampleRate,
                                    0,
                                    (int)file.sampleRate / 2,
                                    window);
                        test = new double[mffcList.Count][];
                        for (int i = 0; i < test.Length; i++)
                            test[i] = mffcList[i].ToArray();
                    }
                    else
                        if (listBox6.SelectedIndex == 0)
                        {
                            List<double[]> lpcList = SignalProcessing.GetCC(source, Convert.ToInt32(textBox7.Text),
                                    Convert.ToInt32(textBox7.Text) - Convert.ToInt32(textBox8.Text),
                                    Convert.ToInt32(textBox6.Text), Convert.ToInt32(textBox6.Text), window);
                            test = new double[lpcList.Count][];
                            for (int i = 0; i < lpcList.Count; i++)
                            {
                                double[] temp = new double[lpcList[i].Length - 1];
                                Array.Copy(lpcList[i], 1, temp, 0, temp.Length);
                                test[i] = temp;
                            }
                        }
                            else
                            {
                                List<List<double>> plpList = SignalProcessing.PLP(source, (int)file.sampleRate,
                                        Convert.ToInt32(textBox7.Text),
                                        Convert.ToInt32(textBox7.Text) - Convert.ToInt32(textBox8.Text),
                                        window,
                                        27,
                                        Convert.ToInt32(textBox6.Text));
                                test = new double[plpList.Count][];
                                for (int i = 0; i < plpList.Count; i++)
                                {
                                    plpList[i].Remove(plpList[i][0]);
                                    test[i] = plpList[i].ToArray();
                                }
                            }
                    GaussianMixtureModel[][] models = GMMmodels.ToArray();
                    Stopwatch SW = new Stopwatch();
                    SW.Start();
                    int result = GaussianMixtureModel.Classification(test, models);
                    SW.Stop();
                    textBox1.Text += "Распознавание файла " + listBox5.Items[f] + " завершено за " + SW.Elapsed.Seconds + " секунд " + SW.Elapsed.Milliseconds + " милисекунд" + "\r\n";
                    if (MDIParent1.identificationTask == 1)
                        textBox1.Text += "Файл " + listBox5.Items[f] + " отнесен к диктору " + listBox1.Items[result].ToString() + "\r\n";
                    if (MDIParent1.identificationTask == 2)
                        textBox1.Text += "Файл " + listBox5.Items[f] + " отнесен к языку " + listBox1.Items[result].ToString() + "\r\n";
                    if (MDIParent1.identificationTask == 3)
                        textBox1.Text += "Файл " + listBox5.Items[f] + " отнесен к полу " + listBox1.Items[result].ToString() + "\r\n";
                }
            }
            if (MDIParent1.classificationMode == 3)
            {
                int classes = learningFiles.Count;
                for (int f = 0; f < listBox5.Items.Count; f++)
                {
                    WAV file = WAV.Reading(testFiles[f]);
                    double[] source = WAV.GetNormalizedData(file);
                    double[][] test;
                    if(listBox6.SelectedIndex == -1 || listBox6.SelectedIndex == 1)
                    { 
                        List<List<double>> mffcList = SignalProcessing.MFCC(source, Convert.ToInt32(textBox7.Text),
                                    Convert.ToInt32(textBox7.Text) - Convert.ToInt32(textBox8.Text),
                                    Convert.ToInt32(textBox6.Text),
                                    26,
                                    (int)file.sampleRate,
                                    0,
                                    (int)file.sampleRate / 2,
                                    window);
                        test = new double[mffcList.Count][];
                        double max = Double.MinValue;
                        for (int i = 0; i < test.Length; i++)
                        {
                            if (max < mffcList[i].Max())
                                max = mffcList[i].Max();
                        }
                        for (int i = 0; i < test.Length; i++)
                        {
                            for (int j = 0; j < mffcList[i].Count; j++)
                            {
                                if (mffcList[i][j] < 0)
                                    mffcList[i][j] /= -max;
                                if (mffcList[i][j] >= 0)
                                    mffcList[i][j] /= max;
                            }
                            test[i] = mffcList[i].ToArray();
                        }
                    }
                    else
                        if (listBox6.SelectedIndex == 0)
                        {
                            List<double[]> lpcList = SignalProcessing.GetCC(source, Convert.ToInt32(textBox7.Text),
                                    Convert.ToInt32(textBox7.Text) - Convert.ToInt32(textBox8.Text),
                                    Convert.ToInt32(textBox6.Text), Convert.ToInt32(textBox6.Text), window);
                            test = new double[lpcList.Count][];
                            double max = Double.MinValue;
                            for (int i = 0; i < test.Length; i++)
                            {
                                if (max < lpcList[i].Max())
                                    max = lpcList[i].Max();
                            }
                            for (int i = 0; i < test.Length; i++)
                            {
                                for (int j = 1; j < lpcList[i].Length; j++)
                                {
                                    if (lpcList[i][j] < 0)
                                        lpcList[i][j] /= -max;
                                    if (lpcList[i][j] >= 0)
                                        lpcList[i][j] /= max;
                                }

                                double[] temp = new double[lpcList[i].Length - 1];
                                Array.Copy(lpcList[i], 1, temp, 0, temp.Length);

                                test[i] = temp;
                            }
                        }
                            else
                            {
                                List<List<double>> plpList = SignalProcessing.PLP(source, (int)file.sampleRate,
                                        Convert.ToInt32(textBox7.Text),
                                        Convert.ToInt32(textBox7.Text) - Convert.ToInt32(textBox8.Text),
                                        window,
                                        27,
                                        Convert.ToInt32(textBox6.Text));
                                test = new double[plpList.Count][];
                                double max = Double.MinValue;
                                for (int i = 0; i < test.Length; i++)
                                {
                                    if (max < plpList[i].Max())
                                        max = plpList[i].Max();
                                }
                                for (int i = 0; i < test.Length; i++)
                                {
                                    for (int j = 0; j < plpList[i].Count; j++)
                                    {
                                        if (plpList[i][j] < 0)
                                            plpList[i][j] /= -max;
                                        if (plpList[i][j] >= 0)
                                            plpList[i][j] /= max;
                                    }
                                    test[i] = plpList[i].ToArray();
                                }
                                for (int i = 0; i < plpList.Count; i++)
                                {
                                    plpList[i].Remove(plpList[i][0]);
                                    test[i] = plpList[i].ToArray();
                                }
                            }
                    Stopwatch SW = new Stopwatch();
                    SW.Start();
                    int result = 0;
                    if (classes < 3)
                        result = SupportVectorMachine.Classification(test,SVMtrainingData,weights,threshold);
                    else
                        result = SupportVectorMachine.MultiClassesClassification(test, SVMtrainingData, multiclassweights, thresholds, classes);
                    //int result = SupportVectorMachine.MultiClassesClassification(test, SVMtrainingData, multiclassweights, thresholds, 4);
                    SW.Stop();
                    if (result == -1)
                        result = 0;
                    textBox1.Text += "Распознавание файла " + listBox5.Items[f] + " завершено за " + SW.Elapsed.Seconds + " секунд " + SW.Elapsed.Milliseconds + " милисекунд" + "\r\n";
                    if (MDIParent1.identificationTask == 1)
                        textBox1.Text += "Файл " + listBox5.Items[f] + " отнесен к диктору " + listBox1.Items[result].ToString() + "\r\n";
                    if (MDIParent1.identificationTask == 2)
                        textBox1.Text += "Файл " + listBox5.Items[f] + " отнесен к языку " + listBox1.Items[result].ToString() + "\r\n";
                    if (MDIParent1.identificationTask == 3)
                        textBox1.Text += "Файл " + listBox5.Items[f] + " отнесен к полу " + listBox1.Items[result].ToString() + "\r\n";
                }
            }
        }

        private void Form8_Load(object sender, EventArgs e)
        {
            listBox1.Focus();
            if (MDIParent1.classificationMode == 3)
            {
                button9.Hide();
            }
            if(MDIParent1.identificationTask == 1)
            {
                groupBox4.Text = "Дикторы";
                button5.Text = "Добавить диктора";
                button7.Text = "Удалить диктора";

                if (MDIParent1.classificationMode == 1)
                {
                    this.Text = "Идентификация дикторов (векторное квантование)";
                    groupBox1.Show();
                }
                if (MDIParent1.classificationMode == 2)
                {
                    this.Text = "Идентификация дикторов (модель гауссовских смесей)";
                    groupBox2.Location = new Point(666, 48);
                    groupBox2.Show();
                }
                if (MDIParent1.classificationMode == 3)
                {
                    this.Text = "Идентификация дикторов (машина опорных векторов)";
                    groupBox3.Location = new Point(666, 48);
                    groupBox3.Show();
                }
            }
            if (MDIParent1.identificationTask == 2)
            {
                groupBox4.Text = "Языки";
                button5.Text = "Добавить язык";
                button7.Text = "Удалить язык";

                if (MDIParent1.classificationMode == 1)
                {
                    this.Text = "Идентификация языка (векторное квантование)";
                    groupBox1.Show();
                }
                if (MDIParent1.classificationMode == 2)
                {
                    this.Text = "Идентификация языка (модель гауссовских смесей)";
                    groupBox2.Location = new Point(666, 48);
                    groupBox2.Show();
                }
                if (MDIParent1.classificationMode == 3)
                {
                    this.Text = "Идентификация языка (машина опорных векторов)";
                    groupBox3.Location = new Point(666, 48);
                    groupBox3.Show();
                }
            }
            if (MDIParent1.identificationTask == 3)
            {
                button5.Hide();
                button7.Hide();
                textBox2.Hide();
                listBox1.Items.Add("Мужской");
                List<string> listM = new List<string>();
                learningFiles.Add(listM);
                listBox1.Items.Add("Женский");
                List<string> listF = new List<string>();
                learningFiles.Add(listF);

                if (MDIParent1.classificationMode == 1)
                {
                    this.Text = "Идентификация пола (векторное квантование)";
                    groupBox1.Show();
                }
                if (MDIParent1.classificationMode == 2)
                {
                    this.Text = "Идентификация пола (модель гауссовских смесей)";
                    groupBox2.Location = new Point(666, 48);
                    groupBox2.Show();
                }
                if (MDIParent1.classificationMode == 3)
                {
                    this.Text = "Идентификация пола (машина опорных векторов)";
                    groupBox3.Location = new Point(666, 48);
                    groupBox3.Show();
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count != 0)
            {
                if (listBox1.SelectedIndex == -1)
                {
                    if(modelsWasLoad == false)
                        learningFiles.Remove(learningFiles[0]);
                    listBox1.Items.Remove(listBox1.Items[listBox1.Items.Count - 1]);
                    listView1.Clear();
                }
                else
                { 
                    if(modelsWasLoad == false)
                        learningFiles.Remove(learningFiles[listBox1.SelectedIndex]);
                    listBox1.Items.Remove(listBox1.Items[listBox1.SelectedIndex]);
                    listView1.Clear();
                }
                if (MDIParent1.classificationMode == 3 && listBox1.Items.Count != 2)
                {
                    button5.Enabled = true;
                    button9.Enabled = true;
                }
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count != 0)
            {
                if (listView1.SelectedIndices.Count == 0)
                {
                    int index = listView1.Items.Count - 1;
                    listView1.Items.Remove(listView1.Items[listView1.Items.Count - 1]);
                    learningFiles[listBox1.SelectedIndex].Remove(learningFiles[listBox1.SelectedIndex][index]);
                }
                else
                {
                    int index = listView1.SelectedIndices[0];
                    listView1.Items.Remove(listView1.Items[listView1.SelectedIndices[0]]);
                    learningFiles[listBox1.SelectedIndex].Remove(learningFiles[listBox1.SelectedIndex][index]);
                }
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            modelsWasLoad = true;
            button1.Enabled = false;
            button2.Enabled = false;
            button6.Enabled = false;
            button8.Enabled = false;
            checkBox1.Enabled = false;
            button4.Enabled = true;
            button5.Hide();
            button7.Hide();
            textBox2.Hide();
            
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (MDIParent1.classificationMode == 1)
            {
                openFileDialog.InitialDirectory = Application.StartupPath + "\\Модели\\VQ";
                openFileDialog.Filter = "Текстовый документ (*.txt)|*.txt";
                if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    string[] lines = File.ReadAllLines(openFileDialog.FileName);
                    double[][] codebook = new double[lines.Length][];
                    for (int i=0; i<lines.Length; i++)
                    {
                        string[] temp = lines[i].Split(' ');
                        codebook[i] = new double[temp.Length];
                        for (int j = 0; j < temp.Length; j++)
                            codebook[i][j] = Convert.ToDouble(temp[j]);
                    }
                    codebooks.Add(codebook);
                    string[] splitted = openFileDialog.FileName.Split('\\');
                    string name = splitted[splitted.Length - 1];
                    listBox1.Items.Add(name);
                    if (MDIParent1.identificationTask == 3)
                    {
                        if (listBox1.SelectedIndex == -1)
                            listBox1.SelectedIndex = 0;
                        listBox1.Items.Remove(listBox1.Items[listBox1.SelectedIndex]);
                    }
                }                
            }
            if (MDIParent1.classificationMode == 2)
            {
                openFileDialog.InitialDirectory = Application.StartupPath + "\\Модели\\GMM";
                openFileDialog.Filter = "Текстовый документ (*.txt)|*.txt";
                if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    string[] lines = File.ReadAllLines(openFileDialog.FileName);
                    int order = Convert.ToInt32(lines[0]);
                    int counter = 1;
                    GaussianMixtureModel[] modelsComponents = new GaussianMixtureModel[order];
                    for (int i = 0; i < order; i++ )
                        modelsComponents[i] = new GaussianMixtureModel();
                    for (int i = 0; i < order; i++)
                    {
                        //среднее                        
                        string[] temp = lines[counter].Split(' ');
                        modelsComponents[i].means = new double[temp.Length];
                        for (int j = 0; j < temp.Length; j++)
                            modelsComponents[i].means[j] = Convert.ToDouble(temp[j]);
                        counter++;

                        //ковариация
                        modelsComponents[i].covariance = new double[temp.Length][];
                        for (int j = 0; j < temp.Length; j++)
                        {
                            temp = lines[counter].Split(' ');
                            modelsComponents[i].covariance[j] = new double[temp.Length];
                            for (int k = 0; k < temp.Length; k++)
                            {
                                modelsComponents[i].covariance[j][k] = Convert.ToDouble(temp[k]);
                            }
                            counter++;
                        }

                        modelsComponents[i].pi = Convert.ToDouble(lines[counter]);
                        counter++;
                        //веса                       
                    }
                    GMMmodels.Add(modelsComponents);
                    string[] splitted = openFileDialog.FileName.Split('\\');
                    string name = splitted[splitted.Length - 1];
                    listBox1.Items.Add(name);
                    if (MDIParent1.identificationTask == 3)
                    {
                        if (listBox1.SelectedIndex == -1)
                            listBox1.SelectedIndex = 0;
                        listBox1.Items.Remove(listBox1.Items[listBox1.SelectedIndex]);
                    }
                }                
            }
            if(MDIParent1.classificationMode == 3)
            {
                button9.Enabled = false;
                openFileDialog.InitialDirectory = Application.StartupPath + "\\Модели\\SVM";
                openFileDialog.Filter = "Текстовый документ (*.txt)|*.txt";

                if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    string[] lines = File.ReadAllLines(openFileDialog.FileName);
                    int len = (lines.Length - 1) / 2;
                    SVMtrainingData = new double[len][];
                    for (int i = 0; i < len; i++)
                    {                        
                        string[] temp = lines[i].Split(' ');
                        SVMtrainingData[i] = new double[temp.Length];
                        for (int j = 0; j < temp.Length; j++)
                            SVMtrainingData[i][j] = Convert.ToDouble(temp[j]);
                    }
                    weights = new double[len];
                    for (int i = len; i < 2 * len; i++)
                    {
                        weights[i - len] = Convert.ToDouble(lines[i]);
                    }
                    threshold = Convert.ToDouble(lines[2 * len]);
                }

                string[] splitted = openFileDialog.FileName.Split('\\');
                string name = splitted[splitted.Length - 1];
                listBox1.Items.Add(name);
                if (MDIParent1.identificationTask == 3)
                {
                    listBox1.Items.Remove("Мужской");
                    listBox1.Items.Remove("Женский");
                }
            }
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }
    }
}
