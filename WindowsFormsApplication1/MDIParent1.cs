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
    public partial class MDIParent1 : Form
    {
        private int childFormNumber = 0;
        public static List<string> FileNames = new List<string> { };
        public static int currentFile=0;
        public static bool visualRepresentationShowing = false;
        public static Form1 newMDIChildForm1;
        public static Form2 newMDIChildForm2;
        public static Form4 spectrumWindow;
        public static int windowX = 0;
        public static int windowY = 0;
        public static bool changesHaveBeenMade = false;
        public static bool Exit = false;
        public static Size mainWindowSize;
        public static int difBtwMainAndWorkSpace;
        public static bool fileContainerNotCreatedYet = true;
        public static bool closeOrChange = true;
        bool hideContextMenuStripForFileContainer = true;
        public static double[] globalMagnitudes;
        public static int modeOfLpcWindow;
        public static int classificationMode;
        public static int filterMode;
        public static int pictureMode;
        public static int identificationTask;
        public static bool specialCase1 = false;
        WAV bufferCut = new WAV();
        public MDIParent1()
        {
            InitializeComponent();
            mainWindowSize = this.Size;
        }

        private void OpenFile(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            openFileDialog.InitialDirectory = "D:/Учебный год 2013-2014 мать его за ногу/материалы по курсовой работе/1мин/30сек";
            openFileDialog.Filter = "Аудио файлы (*.wav)|*.wav";
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                if (FileNames.Contains(openFileDialog.FileName) == false)
                {
                    if (FileNames.Count != 0)
                        currentFile++;
                    FileNames.Add(openFileDialog.FileName);
                    closeToolStripMenuItem.Enabled = true;
                    fileInfoToolStripMenuItem.Enabled = true;
                    spectrumToolStripMenuItem.Enabled = true;
                    scaleToolStripMenuItem.Enabled = true;
                    maximizeTheWidthToolStripMenuItem.Enabled = true;
                    minimizeToolStripMenuItem.Enabled = true;
                    maximizeTheHeightToolStripMenuItem.Enabled = true;
                    спектрограммаToolStripMenuItem.Enabled = true;
                    silenceRemovalToolStripMenuItem.Enabled = true;
                    lowPassFilterToolStripMenuItem.Enabled = true;
                    highPassFilterToolStripMenuItem.Enabled = true;
                    bandPassFilterToolStripMenuItem.Enabled = true;
                    полоснозаграждающийФильтрToolStripMenuItem.Enabled = true;
                    movingAverageFilterToolStripMenuItem.Enabled = true;
                    fFTToolStripMenuItem.Enabled = true;
                    lPCToolStripMenuItem.Enabled = true;
                    cepstralToolStripMenuItem.Enabled = true;
                    mFCCToolStripMenuItem.Enabled = true;
                    pLPToolStripMenuItem.Enabled = true;
                    pitchToolStripMenuItem.Enabled = true;
                    formantsToolStripMenuItem.Enabled = true;
                    if (fileContainerNotCreatedYet == true)
                    {
                        listView1.Height = this.Height - 87;
                        listView1.Show();
                        fileContainerNotCreatedYet = false;
                    }
                    string[] splitted = openFileDialog.FileName.Split('\\');
                    string name = splitted[splitted.Length - 1];
                    if (name.Length > 33)
                    {
                        listView1.Items.Add(name.Substring(0,30) + "...");
                        
                    }
                    else
                    {
                        int spaceCount = 57 - name.Length;
                        string[] array = new string[spaceCount];
                        for (int i=0; i<spaceCount; i++)
                        {
                            array[i] = " ";
                        }
                        string tail = String.Concat(array);
                        listView1.Items.Add(name+tail);
                    }
                    if (visualRepresentationShowing == false)
                    {
                        newMDIChildForm1 = new Form1();
                        newMDIChildForm1.MdiParent = this;
                        newMDIChildForm1.Location = new Point(windowX, windowY);
                        newMDIChildForm1.Show();
                        visualRepresentationShowing = true;
                    }
                    else
                    {
                        closeOrChange = false;
                        newMDIChildForm1.Close();
                        newMDIChildForm1 = new Form1();
                        newMDIChildForm1.MdiParent = this;
                        newMDIChildForm1.Location = new Point(windowX, windowY);
                        newMDIChildForm1.Show();
                        visualRepresentationShowing = true;
                        closeOrChange = true;
                    }
                    difBtwMainAndWorkSpace = this.Location.X;
                }
                else
                {
                    MessageBox.Show("Файл с таким именем уже открыт", "Интелектуальная обработка сигналов",
                                         MessageBoxButtons.OK);
                }
            }           
        }

        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            saveFileDialog.Filter = "Аудио файлы (*.wav)|*.wav|Все файлы (*.*)|*.*";
            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string FileName = saveFileDialog.FileName;
                WAV.Writing2(Form1.newWav, FileName);
            }
        }

        private void ExitToolsStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FileNames.Count == 0)
                MessageBox.Show("Нет открытых файлов");
            else
            {
                //copy
                changesHaveBeenMade = true;
                int c = Form1.countUnderCursor;
                int end = Form1.countUnderCursorEnd;
                if (Form1.countUnderCursor % 2 == 1)
                    c++;
                if (Form1.countUnderCursorEnd % 2 == 1)
                    end++;
                bufferCut = CreateBuffer(Form1.newWav);
                bufferCut.LeftChData = new double[end - c];
                for (int i = c; i < end; i++)
                    bufferCut.LeftChData[i - c] = Form1.newWav.LeftChData[i];
                
                //cut
                WAV buffer = CreateBuffer(Form1.newWav);
                buffer.LeftChData = new double[Form1.newWav.LeftChData.Length - bufferCut.LeftChData.Length];
                for(int i = 0; i<c; i++)
                    buffer.LeftChData[i]=Form1.newWav.LeftChData[i];
                for(int i=c; i<buffer.LeftChData.Length; i++)
                    buffer.LeftChData[i]=Form1.newWav.LeftChData[end+i-c];

                 buffer.wavData = WAV.ComputeBytesOfWavData(buffer.LeftChData, true);
                    Form1.buf[0] = Form1.newWav;
                    Form1.buf[1] = buffer;
                    Form1.newWav = CreateBuffer(buffer);
                MDIParent1.newMDIChildForm1.Width += 1;
            }
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FileNames.Count == 0)
                MessageBox.Show("Нет открытых файлов");
            else
            {
                int c = Form1.countUnderCursor;
                int end = Form1.countUnderCursorEnd;
                if (Form1.countUnderCursor % 2 == 1)
                    c++;
                if (Form1.countUnderCursorEnd % 2 == 1)
                    end++;
                bufferCut = CreateBuffer(Form1.newWav);
                bufferCut.LeftChData = new double[end - c];
                for (int i = c; i < end; i++)
                    bufferCut.LeftChData[i-c] = Form1.newWav.LeftChData[i];
                MDIParent1.newMDIChildForm1.Width += 1;
            }
        }

        private void PasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FileNames.Count == 0)
                MessageBox.Show("Нет открытых файлов");
            else
            {
                if (bufferCut != null)
                {
                    changesHaveBeenMade = true;
                    int c = Form1.countUnderCursor;
                    if (Form1.countUnderCursor % 2 == 1)
                        c++;
                    WAV buffer = CreateBuffer(Form1.newWav);
                    buffer.LeftChData = new double[Form1.newWav.LeftChData.Length + bufferCut.LeftChData.Length];
                    for (int i = 0; i < c; i++)
                        buffer.LeftChData[i] = Form1.newWav.LeftChData[i];
                    for (int i = c; i < c + bufferCut.LeftChData.Length; i++)
                        buffer.LeftChData[i] = bufferCut.LeftChData[i - c];
                    for (int i = c + bufferCut.LeftChData.Length; i < buffer.LeftChData.Length; i++)
                        buffer.LeftChData[i] = Form1.newWav.LeftChData[i - bufferCut.LeftChData.Length];
                    
                    buffer.wavData = WAV.ComputeBytesOfWavData(buffer.LeftChData, true);
                    Form1.buf[0] = Form1.newWav;
                    Form1.buf[1] = buffer;
                    Form1.newWav = CreateBuffer(buffer);
                    MDIParent1.newMDIChildForm1.Width += 1;
                }
            }
        }


        private void CloseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form childForm in MdiChildren)
            {
                childForm.Close();
            }
        }


        private void fileInfoToolStripMenuItem_CheckStateChanged(object sender, EventArgs e)
        {
            if (MDIParent1.FileNames.Count == 0)
            {
                MessageBox.Show("Нет открытых файлов");
            }
            if (Exit == false && FileNames.Count != 0)
            {
                if (FileNames[currentFile] != "")
                {
                    if (fileInfoToolStripMenuItem.Checked == true)
                    {
                        newMDIChildForm2 = new Form2();
                        newMDIChildForm2.DesktopLocation = new Point(300, 300);
                        newMDIChildForm2.MdiParent = this;

                        newMDIChildForm2.Show();
                    }
                    else
                    {
                        newMDIChildForm2.Close();
                        fileInfoToolStripMenuItem.Checked = false;
                    }
                }
            }
            if (Exit == true)
                newMDIChildForm2.Close();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (newMDIChildForm1 != null)
            {
                newMDIChildForm1.Close();
                Form1.WorkSpaceCreated = false;
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {           
            WAV.Writing(Form1.newWav, FileNames[currentFile]);
        }

        private void MDIParent1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (FileNames.Count != 0)
            {
                Exit = true;
            }
        }

        private void maximizeTheWidthToolStripMenuItem_Click(object sender, EventArgs e)
        {
            newMDIChildForm1.Location = new Point(0,newMDIChildForm1.Location.Y);
            newMDIChildForm1.Width = this.Width-listView1.Width-20;
        }

        private void maximizeTheHeightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            newMDIChildForm1.Location = new Point(newMDIChildForm1.Location.X, 0);
            newMDIChildForm1.Height = this.Bounds.Height - 92;
            Form1.MaxHeight(newMDIChildForm1);
        }

        private void minimizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            newMDIChildForm1.Location = new Point(0, newMDIChildForm1.Location.Y);
            newMDIChildForm1.Width = 600;
            newMDIChildForm1.Height = 100;
        }

        private void MDIParent1_Load(object sender, EventArgs e)
        {
            this.Width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            this.Height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
        }

        private void MDIParent1_Resize(object sender, EventArgs e)
        {
            if (fileContainerNotCreatedYet == false)
                listView1.Height = this.Bounds.Height - 87;
        }

        public static void SetSpectrumUncheckedValue()
        {
            spectrumToolStripMenuItem.Checked = false;
        }

        public static bool spectrumCheckState()
        {
            return spectrumToolStripMenuItem.Checked;
        }

        public static void SetSpectrumCheckedValue()
        {
            spectrumToolStripMenuItem.Checked = true;
        }

        public static void SetFileInfoUncheckedValue()
        {
            fileInfoToolStripMenuItem.Checked = false;
        }

        public static void SetFileInfoCheckedValue()
        {
            fileInfoToolStripMenuItem.Checked = true;
        }
        public static bool FileInfoCheckState ()
        {
            return fileInfoToolStripMenuItem.Checked;
        }
        

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                if (listView1.SelectedIndices[0] >= 0)
                {
                    if (listView1.SelectedIndices[0] != currentFile || (listView1.SelectedIndices[0]==0 && currentFile==0))
                    {
                        MDIParent1.currentFile = listView1.SelectedIndices[0];
                        if (visualRepresentationShowing == false)
                        {
                            newMDIChildForm1 = new Form1();
                            newMDIChildForm1.MdiParent = this;
                            newMDIChildForm1.Location = new Point(windowX, windowY);
                            newMDIChildForm1.Show();
                            visualRepresentationShowing = true;
                        }
                        else
                        {
                            closeOrChange = false;
                            newMDIChildForm1.Close();
                            newMDIChildForm1 = new Form1();
                            newMDIChildForm1.MdiParent = this;
                            newMDIChildForm1.Location = new Point(windowX, windowY);
                            newMDIChildForm1.Show();
                            visualRepresentationShowing = true;
                            closeOrChange = true;
                        }
                    }
                }
            }
        }

        private void listView1_ItemMouseHover(object sender, ListViewItemMouseHoverEventArgs e)
        {
            if (e.Item != null)
            {
                    e.Item.ToolTipText = FileNames[e.Item.Index];
            }
        }

        private void listView1_ItemActivate(object sender, EventArgs e)
        {
            
        }

        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                {
                    if (listView1.FocusedItem != null)
                    {
                        if (e.X < listView1.FocusedItem.Position.X + listView1.FocusedItem.Bounds.Width &&
                            e.Y < listView1.FocusedItem.Position.Y + listView1.FocusedItem.Bounds.Height)
                        {
                            listView1.ContextMenuStrip = contextMenuStrip1;
                            hideContextMenuStripForFileContainer = false;
                        }
                        else
                            if (hideContextMenuStripForFileContainer == false)
                            {
                                listView1.ContextMenuStrip = null;
                                hideContextMenuStripForFileContainer = true;
                            }
                        listView1.FocusedItem.Focused = false;
                    }
                }       
        }

        private void closeAllExceptThisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int i = 0;
            while (FileNames.Count > 1)
            {
                if (i != currentFile)
                {
                    FileNames.Remove(FileNames[i]);
                    listView1.Items.Remove(listView1.Items[i]);
                    i--;
                    if (i < currentFile)
                    {
                        currentFile--;                       
                    }
                }
                i++;
            }
            
        }

        private void fFTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FileNames.Count == 0)
                MessageBox.Show("Нет открытых файлов");
            else
            {
                SaveFileDialog saveFft = new SaveFileDialog();
                saveFft.InitialDirectory = Application.StartupPath;
                saveFft.Filter = "Текстовый документ (*.txt)|*.txt|Все файлы (*.*)|*.*";
                double[] normalizedData = WAV.GetNormalizedData(Form1.newWav);

                Complex[] forFft = Fourier.PrepareToFFT(normalizedData);
                for (int j = 0; j < normalizedData.Length; j++)
                {
                    normalizedData[j] = normalizedData[j] * (0.53836 - 0.46164 * Math.Cos((2 * Math.PI * j) / (normalizedData.Length - 1)));
                }
                Complex[] fftCoefs = Fourier.FFT(forFft);
                //double[] magn = Fourier.GetMagnitude(fftCoefs);

                string[] text = new string[fftCoefs.Length];
                for (int i = 0; i < fftCoefs.Length; i++)
                {
                    text[i] = fftCoefs[i].ToString();
                    //text[i] = magn[i].ToString();
                }

                if (saveFft.ShowDialog(this) == DialogResult.OK)
                {
                    string FileName = saveFft.FileName;
                    System.IO.File.WriteAllLines(FileName, text);
                }
            }
        }
        private void spectrumToolStripMenuItem_CheckStateChanged(object sender, EventArgs e)
        {
            if (FileNames.Count == 0)
                MessageBox.Show("Нет открытых файлов");
            if (Exit == false && FileNames.Count != 0)
            {
                if (FileNames[currentFile] != "")
                {
                    if (spectrumToolStripMenuItem.Checked == true)
                    {
                        double[] normalizedData = WAV.GetNormalizedData(Form1.newWav);
                        for (int j = 0; j < normalizedData.Length; j++)
                        {
                            normalizedData[j] = normalizedData[j] * (0.53836 - 0.46164 * Math.Cos((2 * Math.PI * j) / (normalizedData.Length - 1)));
                        }
                        Complex[] forFft = Fourier.PrepareToFFT(normalizedData);
                        Complex[] fftCoefs = Fourier.FFT(forFft);
                        double[] magn = Fourier.GetMagnitude(fftCoefs);
                        globalMagnitudes = new double[magn.Length / 2];
                        Array.Copy(magn, globalMagnitudes, globalMagnitudes.Length);
                        for (int i = 0; i < globalMagnitudes.Length; i++)                       // нормировка (нужна ли)
                        {
                            globalMagnitudes[i] = (double)(2 * globalMagnitudes[i] / 2048);
                        }
                        globalMagnitudes[0] /= 2;
                        spectrumWindow = new Form4();
                        spectrumWindow.DesktopLocation = new Point(300, 600);
                        spectrumWindow.MdiParent = this;

                        spectrumWindow.Show();
                    }
                    else
                    {
                        spectrumWindow.Close();
                        spectrumToolStripMenuItem.Checked = false;
                    }
                }
            }
            if(Exit == true)
                spectrumWindow.Close();
        }

        private void mFCCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FileNames.Count == 0)
                MessageBox.Show("Нет открытых файлов");
            else
            {
                Form5 mfccWindow = new Form5();
                mfccWindow.Show();
            }
        }

        private void lPCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FileNames.Count == 0)
                MessageBox.Show("Нет открытых файлов");
            else
            {
                modeOfLpcWindow = 1;
                Form6 lpcWindow = new Form6();
                lpcWindow.Show();
            }
        }

        private void cepstralToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FileNames.Count == 0)
                MessageBox.Show("Нет открытых файлов");
            else
            {
                modeOfLpcWindow = 2;
                Form6 lpcWindow = new Form6();
                lpcWindow.Show();
            }
        }

        private void pLPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FileNames.Count == 0)
                MessageBox.Show("Нет открытых файлов");
            else
            {
                Form7 plpWindow = new Form7();
                plpWindow.Show();
            }
        }

        private void pitchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FileNames.Count == 0)
                MessageBox.Show("Нет открытых файлов");
            else
            {
                Pictures form = new Pictures();
                pictureMode = 2;
                form.Show();
            }
        }

        private void formantsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FileNames.Count == 0)
                MessageBox.Show("Нет открытых файлов");
            else
            {
                Pictures form = new Pictures();
                pictureMode = 3;
                form.Show();
            }
        }

        private void vectorQuantinizationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            identificationTask = 1;
            classificationMode = 1;
            Form8 VQWindow = new Form8();
            VQWindow.Show();
        }

        private void vectorQuantinizationToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            identificationTask = 2;
            classificationMode = 1;
            Form8 VQWindow = new Form8();
            VQWindow.Show();
        }

        private void gaussianMixtureModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            identificationTask = 1;
            classificationMode = 2;
            Form8 VQWindow = new Form8();
            VQWindow.Show();
        }

        private void gaussianMixtureModelToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            identificationTask = 2;
            classificationMode = 2;
            Form8 VQWindow = new Form8();
            VQWindow.Show();
        }

        private void supportVectorMachineToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            identificationTask = 2;
            classificationMode = 3;
            Form8 VQWindow = new Form8();
            VQWindow.Show();
        }

        private void lowPassFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FileNames.Count == 0)
                MessageBox.Show("Нет открытых файлов");
            else
            {
                filterMode = 1;
                Filter filterWindow = new Filter();
                filterWindow.Show();
            }
        }

        private void highPassFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FileNames.Count == 0)
                MessageBox.Show("Нет открытых файлов");
            else
            {
                filterMode = 2;
                Filter filterWindow = new Filter();
                filterWindow.Show();
            }
        }

        private void bandPassFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FileNames.Count == 0)
                MessageBox.Show("Нет открытых файлов");
            else
            {
                filterMode = 3;
                Filter filterWindow = new Filter();
                filterWindow.Show();
            }
        }

        private void movingAverageFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FileNames.Count == 0)
                MessageBox.Show("Нет открытых файлов");
            else
            {
                filterMode = 4;
                Filter filterWindow = new Filter();
                filterWindow.Show();
            }
        }

        private void batchProcessingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            batchProcessing BPwindow = new batchProcessing();
            BPwindow.Show();
        }

        private void silenceRemovalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FileNames.Count == 0)
                MessageBox.Show("Нет открытых файлов");
            else
            {
                changesHaveBeenMade = true;
                WAV buffer = CreateBuffer(Form1.newWav);
                buffer.LeftChData = SignalProcessing.SilenceDetection(WAV.GetNormalizedData(Form1.newWav), 1024);
                buffer.wavData = WAV.ComputeBytesOfWavData(buffer.LeftChData);
                Form1.buf[0] = Form1.newWav;
                Form1.buf[1] = buffer;
                Form1.newWav = CreateBuffer(buffer);
                for (int i = 0; i < Form1.newWav.LeftChData.Length; i++)
                    Form1.newWav.LeftChData[i] *= 32768;
                MDIParent1.newMDIChildForm1.Width += 1;
            }
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MDIParent1.changesHaveBeenMade == true)
            {
                Form1.newWav = Form1.buf[0];
                changesHaveBeenMade = false;
            }

        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.newWav = Form1.buf[1];
            changesHaveBeenMade = true;
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

        private void спектрограммаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FileNames.Count == 0)
                MessageBox.Show("Нет открытых файлов");
            else
            {
                Pictures form = new Pictures();
                pictureMode = 1;
                form.Show();
            }
        }

        private void машинаОпорныхВекторовToolStripMenuItem_Click(object sender, EventArgs e)
        {
            identificationTask = 2;
            classificationMode = 3;
            Form8 wnd = new Form8();          
            wnd.Show();
        }

        private void supportVectorMachineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            identificationTask = 1;
            classificationMode = 3;
            Form8 VQWindow = new Form8();
            VQWindow.Show();
        }


        private void векторноеКвантованиеToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            identificationTask = 3;
            classificationMode = 1;
            Form8 VQWindow = new Form8();
            VQWindow.Show();
        }

        private void смесьГауссовскихРаспределенийToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            identificationTask = 3;
            classificationMode = 2;
            Form8 VQWindow = new Form8();
            VQWindow.Show();
        }

        private void методОпорныхВекторовToolStripMenuItem_Click(object sender, EventArgs e)
        {
            identificationTask = 3;
            classificationMode = 3;
            Form8 VQWindow = new Form8();
            VQWindow.Show();
        }

        private void векторноеКвантованиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            identificationTask = 2;
            classificationMode = 1;
            Form8 VQWindow = new Form8();
            VQWindow.Show();
        }

        private void смесьГауссовскихРаспределенийToolStripMenuItem_Click(object sender, EventArgs e)
        {
            identificationTask = 2;
            classificationMode = 2;
            Form8 VQWindow = new Form8();
            VQWindow.Show();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FileNames.Count == 0)
                MessageBox.Show("Нет открытых файлов");
            else
            {
                specialCase1 = true;
                newMDIChildForm1.Refresh();
            }
        }

        private void pasteSilenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FileNames.Count == 0)
                MessageBox.Show("Нет открытых файлов");
            else
            {
                changesHaveBeenMade = true;
                int c = Form1.countUnderCursor;
                if (Form1.countUnderCursor % 2 == 1)
                    c++;
                WAV buffer = CreateBuffer(Form1.newWav);
                buffer.LeftChData = new double[Form1.newWav.LeftChData.Length + 10000];
                for (int i = 0; i < c; i++)
                    buffer.LeftChData[i] = Form1.newWav.LeftChData[i];
                for (int i = c; i < c + 10000; i++)
                   buffer.LeftChData[i] = 0;
                for (int i = c + 10000; i < buffer.LeftChData.Length; i++)
                    buffer.LeftChData[i] = Form1.newWav.LeftChData[i - 10000];
                Form1.buf[0] = Form1.newWav;
                Form1.buf[1] = buffer;
                Form1.newWav = CreateBuffer(buffer);
                for (int i = 0; i < Form1.newWav.LeftChData.Length; i++)
                    Form1.newWav.LeftChData[i] *= 32768;
                MDIParent1.newMDIChildForm1.Width += 1;
            }
        }

        private void полоснозаграждающийФильтрToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FileNames.Count == 0)
                MessageBox.Show("Нет открытых файлов");
            else
            {
                filterMode = 5;
                Filter filterWindow = new Filter();
                filterWindow.Show();
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Программный комплекс интеллектуальной обработки речевых сигналов. Сделано в КФУ, 2015 год");
        }
    }
}

