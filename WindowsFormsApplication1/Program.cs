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
    public class Complex
    {
        public double real = 0.0;
        public double imaginary = 0.0;
        public Complex()
        {
        }

        public Complex(double real, double im)
        {
            this.real = real;
            this.imaginary = im;
        }

        public string ToString()
        {
            string complexValue = real.ToString() + " " + imaginary.ToString() + "i";
            return complexValue;
        }

        public static Complex FromPolarToRectangular(double r, double radians)
        {
            Complex complexValue = new Complex(r * Math.Cos(radians), r * Math.Sin(radians));
            return complexValue;
        }

        public static Complex operator + (Complex a, Complex b)
        {
            Complex sum = new Complex(a.real + b.real, a.imaginary + b.imaginary);
            return sum;
        }

        public static Complex operator - (Complex a, Complex b)
        {
            Complex dif = new Complex(a.real - b.real, a.imaginary - b.imaginary);
            return dif;
        }

        public static Complex operator * (Complex a, Complex b)
        {
            Complex prod = new Complex(a.real * b.real - a.imaginary * b.imaginary,
                a.real * b.imaginary + a.imaginary * b.real);
            return prod;
        }

        public static Complex operator / (Complex a, Complex b)
        {
            Complex quo = new Complex((a.real * b.real + a.imaginary * b.imaginary) / (b.real * b.real + b.imaginary * b.imaginary),
                (b.real * a.imaginary - b.imaginary * a.real) / (b.real * b.real + b.imaginary * b.imaginary));
            return quo;
        }
        public double ComplexAbs (Complex a)
        {
            return (Math.Sqrt(a.real * a.real + a.imaginary * a.imaginary));
        }

        public double GetPhase (Complex a)
        {
            return Math.Atan(a.imaginary / a.real);
        }
    }

    class Coefficients
    {
        public static List<List<double>> MFCC (double[] source, int frameLength, int frameInc,
           int mfccSize, int filtersNumber, int sampleRate, int minFreq, int maxFreq, string wnd)
        {
            mfccSize = mfccSize + 1; //потому что выкинем первый
            List<List<double>> result = new List<List<double>>();
            for (int i = 0; i + frameLength < source.Length + 1 ; i += frameInc)
            {
                double[] normalizedData = new double[frameLength];
                normalizedData[0] = 0;
                for (int j = 1; j < normalizedData.Length; j++)
                    normalizedData[j] = normalizedData[j] - normalizedData[j - 1];
                Array.Copy(source, i, normalizedData, 0, frameLength);
                if (wnd == "hm")
                    for (int j = 0; j < normalizedData.Length; j++)
                    {
                        normalizedData[j] = normalizedData[j] * (0.53836 - 0.46164 * Math.Cos((2 * Math.PI * j) / (normalizedData.Length - 1)));
                    }
                if (wnd == "hn")
                    for (int j = 0; j < normalizedData.Length; j++)
                    {
                        normalizedData[j] = normalizedData[j] * (0.5 * (1 - Math.Cos((2 * Math.PI * j) / (normalizedData.Length - 1))));
                    }
                if (wnd == "bm")
                    for (int j = 0; j < normalizedData.Length; j++)
                    {
                        normalizedData[j] = normalizedData[j] * (0.42 - 0.5 * Math.Cos((2 * Math.PI * j) / (normalizedData.Length - 1)) + 0.08 * Math.Cos((4 * Math.PI * j) / (normalizedData.Length - 1)));
                    }
                Complex[] forFft = Fourier.PrepareToFFT(normalizedData);
                Complex[] fftCoefs = Fourier.FFT(forFft);
                //Complex[] fftCoefsHalf = new Complex[fftCoefs.Length / 2];
                //Array.Copy(fftCoefs, fftCoefsHalf, fftCoefsHalf.Length);         
                //double[] magnitudes = Fourier.GetMagnitude(fftCoefsHalf);
                double[] magnitudes = Fourier.GetMagnitude(fftCoefs);
                double minFreqMel = freqToMel(minFreq);
                double maxFreqMel = freqToMel(maxFreq);
                double[] melFreqs = new double [filtersNumber + 2]; 
                for (int j = 0; j < filtersNumber + 2; j++)
                {
                    melFreqs[j] = minFreqMel + j * (maxFreqMel - minFreqMel) / (filtersNumber + 1);
                }
                double[] freqs = new double[melFreqs.Length];
                for (int j = 0; j < melFreqs.Length; j++)
                {
                    freqs[j] = melToFreq(melFreqs[j]);
                }
                int[] gcps = new int[freqs.Length];
                for (int j = 0; j < freqs.Length; j++)
                {
                    gcps[j] = (int)Math.Floor((frameLength + 1) * freqs[j] / sampleRate);
                }
                double [,] filters = new double [mfccSize, frameLength];
                for (int m = 1; m < mfccSize + 1; m++)
                    for (int k = 0; k < frameLength; k++)
                    {
                        if (gcps[m - 1] <= k && k <= gcps[m])
                        {
                            filters[m - 1, k] = (k - gcps[m - 1]) / (gcps[m] - gcps[m - 1]);
                        }
                        else
                            if (gcps[m] <= k && k <= gcps[m + 1])
                            {
                                filters[m - 1, k] = (gcps[m + 1] - k) / (gcps[m + 1] - gcps[m]);
                            }
                            else
                                filters[m - 1, k] = 0;
                    }
                double[] S = new double[mfccSize];
                for (int m = 0; m < mfccSize; m++)
                {
                    double sum = 0;
                    for (int k = 0; k < frameLength; k++)
                    {
                        sum += magnitudes[k] * magnitudes[k] * filters[m, k];
                    }
                    S[m] = Math.Log(sum);
                }
                double[] mfcc = new double[mfccSize];
                for (int l = 0; l < mfccSize; l++)
                {
                    double sum = 0;
                    for (int m = 0; m < mfccSize; m++)
                        sum += S[m] * Math.Cos(Math.PI * l * (m + 0.5) / mfccSize);
                    mfcc[l] = sum;
                }
                List<double> mfccList = new List<double>();
                mfccList = mfcc.ToList();
                result.Add(mfccList);
                mfccList.Remove(mfccList[0]);
            }
            return result;
        }

        public static double freqToMel (double freq)
        {
            return 1127 * Math.Log(1 + freq / 700, Math.E);
        }

        public static double melToFreq (double mel)
        {
            return 700 * (Math.Exp(mel / 1127) - 1);
        }
    }
    class Fourier
    {
        public static Complex[] FFT(Complex[] x)
        {
            if (x.Length == 1)
            {
                return new Complex[] {x[0]};
            }
            if (x.Length == 2)
            {
                return new Complex[] {x[0] + x[1], x[0] - x[1] };
            }
            int N = x.Length;
            Complex[] X = new Complex[N];
            Complex[] even, Even, odd, Odd;

            even = new Complex[N / 2];
            odd = new Complex[N / 2];

            for (int i = 0; i < N / 2; i++)
            {
                even[i] = x[2 * i];
                odd[i] = x[2 * i + 1];
            } 
            Even = FFT(even);
            Odd = FFT(odd);

            for (int i = 0; i < N / 2; i++)
            {
                Complex arg = Complex.FromPolarToRectangular(1, -2 * Math.PI * i / N);
                X[i] = Even[i] + Odd[i] * arg;
                X[i + N / 2] = Even[i] - Odd[i] * arg;
            }            
            return X;    
        }

        public static List<List<Complex>> FftWithWindow(double[] source, int frameLength, int increment)
        {
            List<List<Complex>> result = new List<List<Complex>>();
            for (int i = 0; i + frameLength < source.Length + 1; i += increment)
            {
                double[] normalizedData = new double[frameLength];
                Array.Copy(source, i, normalizedData, 0, frameLength);
                for (int j = 0; j < normalizedData.Length; j++)
                {
                    normalizedData[j] = normalizedData[j] * (0.53836 - 0.46164 * Math.Cos((2 * Math.PI * j) / (normalizedData.Length - 1)));
                }
                Complex[] forFft = Fourier.PrepareToFFT(normalizedData);
                Complex[] fftCoefs = Fourier.FFT(forFft);
                List<Complex> fftList = new List<Complex>();
                fftList = fftCoefs.ToList();
                result.Add(fftList);
            }
            return result;
        }

        public static double[] GetMagnitude (Complex[] fftCoefs)
        {
            double[] magn = new double[fftCoefs.Length];
            for (int i = 0; i < fftCoefs.Length; i++)
            {
                magn[i] = Math.Sqrt(fftCoefs[i].real * fftCoefs[i].real + fftCoefs[i].imaginary * fftCoefs[i].imaginary);
            }
            return magn;
        }
        public static Complex[] PrepareToFFT(double[] data)
        {
            int len = data.Length;
            int pow = 0;
            while(len>1)
            {
                len = len / 2;
                pow++;
            }
            if (data.Length - Math.Pow(2, pow) > Math.Pow(2, pow + 1) - data.Length)
                len = (int)Math.Pow(2, pow + 1);
            else
                len = (int)Math.Pow(2, pow);
            Complex[] complexData = new Complex[data.Length];
            for (int i = 0 ; i < data.Length; i++)
            {
                complexData[i] = new Complex(data[i], 0);
            }
            Complex[] forFft = new Complex[len];
            if (len > data.Length)
            {
                Array.Copy(complexData, forFft, data.Length);
                for (int i = data.Length; i < len; i++)
                    forFft[i] = new Complex(0, 0);
            }
            else
            {
                Array.Copy(complexData, forFft, len);
            }
            return forFft;
        }
    }
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
                            currentFile.LeftChData = new double[samples];
                            currentFile.RightChData = new double[samples];
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
                LeftChData[i] = bytesToCountValue(curFile.wavData[j], curFile.wavData[j + 1]);
                j += 2;
                i++;
            }
            //for (int k = 0; k < 1000; k++)
            //    Console.Write(LeftCh[k] + " ");
            return LeftChData;
        }


        public static double[] GetNormalizedData(WAV curFile)
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
                data[0, i] = bytesToCountValue(curFile.wavData[j], curFile.wavData[j + 1]);
                j += 2;
                data[1, i] = bytesToCountValue(curFile.wavData[j], curFile.wavData[j + 1]);
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

        static double bytesToCountValue(byte firstByte, byte secondByte)
        {
            //short s = (short)((secondByte << 8) | firstByte);
            //return s / 32768.000;
            short countValue = (short)((secondByte << 8) | firstByte);
            return countValue;
        }

        public static double bytesToDouble(byte firstByte, byte secondByte)
        {
            short s = (short)((secondByte << 8) | firstByte);
            return s / 32768.000;
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
