using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
namespace WindowsFormsApplication1
{
    public class WAV
    {
        public uint chunkID;
        public uint fileSize;
        public uint riffType;
        public uint fmtID;
        public uint fmtSize;
        public UInt16 fmtCode;
        public UInt16 channels;
        public uint sampleRate;
        public uint fmtAvgBPS;
        public UInt16 fmtBlockAlign;
        public UInt16 bitDepth;
        public int dataID;
        public int dataSize;
        public byte[] wavData;
        public double[] LeftChData;
        public double[] RightChData;

        public static WAV Reading(string path)
        {
            WAV currentFile = new WAV();
            BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));
            currentFile.chunkID = reader.ReadUInt32();
            currentFile.fileSize = reader.ReadUInt32();
            currentFile.riffType = reader.ReadUInt32();
            currentFile.fmtID = reader.ReadUInt32();
            currentFile.fmtSize = reader.ReadUInt32();
            currentFile.fmtCode = reader.ReadUInt16();
            currentFile.channels = reader.ReadUInt16();
            currentFile.sampleRate = reader.ReadUInt32();
            currentFile.fmtAvgBPS = reader.ReadUInt32();
            currentFile.fmtBlockAlign = reader.ReadUInt16();
            currentFile.bitDepth = reader.ReadUInt16();
            currentFile.dataID = reader.ReadInt32();
            currentFile.dataSize = reader.ReadInt32();


            //if (fmtSize == 18)
            //{
            //    // Read any extra values
            //    int fmtExtraSize = reader.ReadInt16();
            //    reader.ReadBytes(fmtExtraSize);
            //    Console.WriteLine("fmtExtraSize:" + fmtExtraSize);
            //}

            currentFile.wavData = reader.ReadBytes(currentFile.dataSize);  //8 bit sound mono
            if (currentFile.bitDepth == 8)
            {
                if (currentFile.channels == 1)
                {
                    currentFile.LeftChData = GetWavData8bit1chan(currentFile);
                }
                else
                    if (currentFile.channels == 2)
                    {
                        double[,] temp = GetWavData8bit2chan(currentFile);
                        int samples = currentFile.wavData.Length / 2;
                        for (int i = 0; i < samples; i++)
                        {
                            currentFile.LeftChData[i] = temp[0, i];
                            currentFile.RightChData[i] = temp[1, i];
                        }
                    }
            }
            else
                if (currentFile.bitDepth == 16)
                {
                    if (currentFile.channels == 1)
                    {
                        currentFile.LeftChData = GetWavData16bit1chan(currentFile);
                    }
                    else
                        if (currentFile.channels == 2)
                        {
                            double[,] temp = GetWavData16bit2chan(currentFile);
                            int samples = currentFile.wavData.Length / 4;
                            for (int i = 0; i < samples; i++)
                            {
                                currentFile.LeftChData[i] = temp[0, i];
                                currentFile.RightChData[i] = temp[1, i];
                            }
                        }
                }

            reader.Close();


            //for (int i = 0; i < byteArray.Length; i++)
            //    Console.Write(byteArray[i] + " ");

            return currentFile;
        }

        public static void PrintWavHeaders(WAV curFile)
        {
            Console.WriteLine("ChunkID:" + curFile.chunkID);
            Console.WriteLine("file size:" + curFile.fileSize);
            Console.WriteLine("riff type:" + curFile.riffType);
            Console.WriteLine("fmtID:" + curFile.fmtID);
            Console.WriteLine("fmt size:" + curFile.fmtSize);
            Console.WriteLine("fmt code:" + curFile.fmtCode);
            Console.WriteLine("channels:" + curFile.channels);
            Console.WriteLine("sample rate:" + curFile.sampleRate);
            Console.WriteLine("bytes per second:" + curFile.fmtAvgBPS);
            Console.WriteLine("align:" + curFile.fmtBlockAlign);
            Console.WriteLine("дискретность:" + curFile.bitDepth);

            Console.WriteLine("dataID:" + curFile.dataID);
            Console.WriteLine("data size:" + curFile.dataSize);
        }

        public static void PrintWavData(WAV curFile)
        {
            Console.WriteLine("Данные:");
            for (int i = 0; i < 100; i++)
            {
                Console.Write(curFile.LeftChData[i] + " ");
            }
        }
        public static double[] GetWavData16bit1chan(WAV curFile)
        {
            int samples = curFile.wavData.Length / 2; //16 bit  mono
            double[] LeftChData = new double[samples];
            int i = 0;
            int j = 0;
            while (i < samples)
            {
                LeftChData[i] = bytesToDouble(curFile.wavData[j], curFile.wavData[j + 1]);
                j += 2;
                i++;
            }
            //for (int k = 0; k < 1000; k++)
            //    Console.Write(LeftCh[k] + " ");
            return LeftChData;
        }
        public static double[,] GetWavData16bit2chan(WAV curFile)
        {
            int samples = curFile.wavData.Length / 4; //16 bit stereo
            double[,] data = new double[2, samples];

            int i = 0;
            int j = 0;
            while (i < samples)
            {
                data[0, i] = bytesToDouble(curFile.wavData[j], curFile.wavData[j + 1]);
                j += 2;
                data[1, i] = bytesToDouble(curFile.wavData[j], curFile.wavData[j + 1]);
                j += 2;
                i++;
            }
            return data;
        }
        public static double[] GetWavData8bit1chan(WAV curFile)
        {
            int samples = curFile.wavData.Length;
            double[] LeftChData = new double[samples];
            for (int i = 0; i < samples; i++)
                LeftChData[i] = curFile.wavData[i];   // 8 bit mono
            return LeftChData;
        }
        public static double[,] GetWavData8bit2chan(WAV curFile)  //8 bit stereo
        {
            int samples = curFile.wavData.Length / 2;
            double[,] data = new double[2, samples];
            int i = 0;
            int j = 0;
            while (i < samples)
            {
                data[0, i] = curFile.wavData[j];
                j++;
                data[1, i] = curFile.wavData[j];
                i++; j++;
            }
            return data;
        }


        public static void Writing(WAV curFile, string previous_name)
        {
            DateTime dt = DateTime.Now;
            string strFile = previous_name.Substring(0, previous_name.Length - 4) + dt.ToString("_yyyy_MM_dd_HH_mm") + ".wav";
            BinaryWriter writer = new BinaryWriter(new FileStream(strFile, FileMode.Create, FileAccess.Write,
                                               FileShare.Write));
            writer.Write(curFile.chunkID);
            writer.Write(curFile.fileSize);
            writer.Write(curFile.riffType);
            writer.Write(curFile.fmtID);
            writer.Write(curFile.fmtSize);
            writer.Write(curFile.fmtCode);
            writer.Write(curFile.channels);
            writer.Write(curFile.sampleRate);
            writer.Write(curFile.fmtAvgBPS);
            writer.Write(curFile.fmtBlockAlign);
            writer.Write(curFile.bitDepth);
            writer.Write(curFile.dataID);
            writer.Write(curFile.dataSize);
            //Console.WriteLine("\nПозиция записи: {0}\n", writer.BaseStream.Position);

            for (int i = 0; i < curFile.wavData.Length; i++)
            {
                writer.Write(curFile.wavData[i]);
            }

            writer.Close();
        }

        public static void Writing2(WAV curFile, string newname)
        {
            DateTime dt = DateTime.Now;
            string strFile = newname;
            BinaryWriter writer = new BinaryWriter(new FileStream(strFile, FileMode.Create, FileAccess.Write,
                                               FileShare.Write));
            writer.Write(curFile.chunkID);
            writer.Write(curFile.fileSize);
            writer.Write(curFile.riffType);
            writer.Write(curFile.fmtID);
            writer.Write(curFile.fmtSize);
            writer.Write(curFile.fmtCode);
            writer.Write(curFile.channels);
            writer.Write(curFile.sampleRate);
            writer.Write(curFile.fmtAvgBPS);
            writer.Write(curFile.fmtBlockAlign);
            writer.Write(curFile.bitDepth);
            writer.Write(curFile.dataID);
            writer.Write(curFile.dataSize);
            //Console.WriteLine("\nПозиция записи: {0}\n", writer.BaseStream.Position);

            for (int i = 0; i < curFile.wavData.Length; i++)
            {
                writer.Write(curFile.wavData[i]);
            }

            writer.Close();
        }

        static double bytesToDouble(byte firstByte, byte secondByte)
        {
            //short s = (short)((secondByte << 8) | firstByte);
            //return s / 32768.000;
            short countValue = (short)((secondByte << 8) | firstByte);
            return countValue;
        }

        public static string GetDuration (double dur)
        {
            string duration = "";
            int hours = (int)(dur/3600);
            string hoursStr = hours+"";
            if (hoursStr.Length==1)
                hoursStr = "0"+hoursStr;
            int minutes = (int)(dur/60)-hours*60;
            string minutesStr = minutes+"";
            if (minutesStr.Length==1)
                minutesStr = "0"+minutesStr;
            double seconds = Math.Round((double)(dur)-hours*3600-minutes*60,3);
            string secondsStr = seconds+"";
            if (secondsStr.Length==1)
                secondsStr = "0"+secondsStr;
            string [] temp = secondsStr.Split('.');
            secondsStr = String.Join("", temp);
            duration = hoursStr+":"+minutesStr+":"+secondsStr;
            return duration;
        }
    }

    static class Program
    {
    

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MDIParent1());
        }
    }
}
