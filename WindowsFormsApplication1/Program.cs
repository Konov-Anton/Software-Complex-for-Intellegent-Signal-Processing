using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using PolyLib;

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
            real = Math.Round(real, 3);
            imaginary = Math.Round(imaginary, 3);
            string complexValue = "";
            if (imaginary > 0)
                complexValue = real.ToString() + " + " + imaginary.ToString() + "i";
            else
            {
                imaginary = -imaginary;
                complexValue = real.ToString() + " - " + imaginary.ToString() + "i";
            }
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

    class SignalProcessing
    {
        public static List<List<double>> MFCC(double[] source, int frameLength, int frameInc,
           int mfccSize, int filtersNumber, int sampleRate, int minFreq, int maxFreq, string wnd)
        {
            mfccSize = mfccSize + 1; //потому что выкинем первый
            List<List<double>> result = new List<List<double>>();
            for (int i = 0; i + frameLength < source.Length + 1; i += frameInc)
            {
                double[] normalizedData = new double[frameLength];
                Array.Copy(source, i, normalizedData, 0, frameLength);
                normalizedData[0] = 0;
                for (int j = 1; j < normalizedData.Length; j++)
                    normalizedData[j] = normalizedData[j] - normalizedData[j - 1];                
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
                double[] melFreqs = new double[filtersNumber + 2];
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
                double[,] filters = new double[mfccSize, frameLength];
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
                if (Double.IsNaN(mfccList[1]) != true)
                    result.Add(mfccList);
                mfccList.Remove(mfccList[0]);
            }
            return result;
        }

        public static double freqToMel(double freq)
        {
            return 1127 * Math.Log(1 + freq / 700, Math.E);
        }

        public static double melToFreq(double mel)
        {
            return 700 * (Math.Exp(mel / 1127) - 1);
        }

        public static List<double[]> GetLPC (double[] source, int frameLength, int inc, int order, string window)
        {
            List<double[]> result = new List<double[]>();
            for (int i=0; i + frameLength < source.Length + 1; i += inc)
            {
                double[] frame = new double[frameLength];
                Array.Copy(source, i, frame, 0, frameLength);

                if (window == "hm")
                    for (int j = 0; j < frame.Length; j++)
                    {
                        frame[j] = frame[j] * (0.53836 - 0.46164 * Math.Cos((2 * Math.PI * j) / (source.Length - 1)));
                    }
                if (window == "hn")
                    for (int j = 0; j < frame.Length; j++)
                    {
                        frame[j] = frame[j] * (0.5 * (1 - Math.Cos((2 * Math.PI * j) / (source.Length - 1))));
                    }
                if (window == "bm")
                    for (int j = 0; j < frame.Length; j++)
                    {
                        frame[j] = frame[j] * (0.42 - 0.5 * Math.Cos((2 * Math.PI * j) / (source.Length - 1)) + 0.08 * Math.Cos((4 * Math.PI * j) / (source.Length - 1)));
                    }
                double[] c = Durbin(frame, order);
                if (Double.IsNaN(c[1]) != true)
                    result.Add(c);               
            }
            return result;
        }

        public static List<double[]> GetCC (double[] source, int frameLength, int inc, int orderCC, int orderLPC, string window)
        {
            List<double[]> result = GetLPC(source, frameLength, inc, orderLPC, window);
            for (int i=0; i<result.Count; i++)
            {
                result[i] = Cepstral(result[i], orderCC);
            }
            return result;
        }

        public static double[] Durbin(double[] source, int p)
        {
            double error;
            double[] r = new double[p + 1];  //{0.1482, 0.05, 0.017, -0.0323, - 0.0629, 0.0035, - 0.0087};
            double[] k = new double[p + 1];
            double[] a = new double[p + 1];

            for (int i = 0; i < p + 1; i++)
            {
                r[i] = 0;
                for (int n = 0; n < source.Length - i; n++)
                {
                    r[i] = r[i] + source[n] * source[n + i];
                }
            }
            //Complex[] forFft = Fourier.PrepareToFFT(source);
            //alglib.complex[] f = new alglib.complex[source.Length];
            //for (int i = 0; i < f.Length; i++)
            //{
            //    f[i] = new alglib.complex(forFft[i].real, forFft[i].imaginary);
            //}
            //alglib.fftc1d(ref f);
            //for (int i = 0; i < f.Length; i++)
            //    f[i] = Math.Pow(alglib.math.abscomplex(f[i]), 2);
            //alglib.fftr1dinv(f, out r);
            //for (int i = 0; i < r.Length; i++)
            //    r[i] = r[i] / source.Length;

            error = r[0];
            a[0] = 1;
            for (int j = 1; j < p + 1;j++ )
            {
                double[] aj = new double[j + 1];
                aj[0] = 1;
                k[j-1] = r[j];
                for (int i = 2; i <= j; i++)
                    k[j-1]+= a[i - 1] * r[j - i + 1];
                k[j - 1] = -k[j-1] / error;
                for (int i = 2; i <= j; i++)
                    aj[i - 1] = a[i - 1] + k[j - 1] * a[j - i + 1];
                aj[j] = k[j - 1];
                error = error * (1 - k[j - 1] * k[j - 1]);
                for (int i = 0; i < aj.Length; i++)
                    a[i] = aj[i];
            }
            return a;
        }

        public static double[] Cepstral (double[] a, int order)
        {
            double [] c = new double [order+1];
            if (order+1 >= a.Length)
            {
                for (int i = 0; i < a.Length; i++)
                {
                    double sum = 0;
                    for (int k = 1; k < i + 1; k++)
                        sum += ((double)k / (i+1)) * c[k-1] * a[i - k];
                    c[i] = a[i] + sum;
                }
                for (int i = a.Length; i < order+1; i++)
                {
                    for (int k = i - a.Length + 1; k < i; k++)
                        c[i] += ((double)k / (i+1)) * c[k-1] * a[i - k];
                }
            }
            else
            {
                for (int i = 0; i < order+1; i++)
                {
                    double sum = 0;
                    for (int k = 1; k < i + 1; k++)
                        sum += ((double)k / (i+1)) * c[k-1] * a[i - k];
                    c[i] = a[i] + sum;
                }
            }
            return c;
        }

        public static List<List<double>> PLP(double[] source, int sampleRate, int frameLength, int frameInc, string window, int filters, int order)
        {
            List<List<double>> result = new List<List<double>>();
            for (int i = 0; i + frameLength < source.Length + 1; i += frameInc)
            {
                double[] normalizedData = new double[frameLength];
                Array.Copy(source, i, normalizedData, 0, frameLength);
                if (window == "hm")
                    for (int j = 0; j < normalizedData.Length; j++)
                    {
                        normalizedData[j] = normalizedData[j] * (0.53836 - 0.46164 * Math.Cos((2 * Math.PI * j) / (normalizedData.Length - 1)));
                    }
                if (window == "hn")
                    for (int j = 0; j < normalizedData.Length; j++)
                    {
                        normalizedData[j] = normalizedData[j] * (0.5 * (1 - Math.Cos((2 * Math.PI * j) / (normalizedData.Length - 1))));
                    }
                if (window == "bm")
                    for (int j = 0; j < normalizedData.Length; j++)
                    {
                        normalizedData[j] = normalizedData[j] * (0.42 - 0.5 * Math.Cos((2 * Math.PI * j) / (normalizedData.Length - 1)) + 0.08 * Math.Cos((4 * Math.PI * j) / (normalizedData.Length - 1)));
                    }
                Complex[] forFft = Fourier.PrepareToFFT(normalizedData);
                Complex[] fftCoefs = Fourier.FFT(forFft);
                double[] magnitudes = Fourier.GetMagnitude(fftCoefs);
                double[] magnitudesHalf = new double[magnitudes.Length / 2 + 1];
                Array.Copy(magnitudes, magnitudesHalf, magnitudesHalf.Length);
                for (int j = 0; j < magnitudesHalf.Length; j++)
                    magnitudesHalf[j] = Math.Pow(magnitudesHalf[j], 2);
                double[,] wts = fftToBark(frameLength,sampleRate,filters);
                double[] aspectrum = new double[filters];
                for (int j=0; j<filters; j++)
                    for(int k=0; k<frameLength/2+1; k++)
                    {
                        aspectrum[j] += wts[j, k] * magnitudesHalf[k];
                    }
                double[] postSpectrum = equalLoudnessMagic(aspectrum, sampleRate);
                double[] lpc = Durbin(postSpectrum, order);
                double[] cepstra = Cepstral(lpc, order);
                List<double> p = cepstra.ToList();
                if (Double.IsNaN(cepstra[1]) != true)
                    result.Add(cepstra.ToList());
            }
            return result;
        }

        public static double[] equalLoudnessMagic(double[] something, int sampleRate)
        {
            int filters = something.Length;
            double[] result = new double[filters];
            double barkFreqMax = hzToBark(sampleRate / 2);
            double[] pw = new double[filters];
            for (int i=0; i<filters;i++)
            {
                pw[i] = barkToHz(i * (barkFreqMax / (filters - 1)));
                pw[i] = (Math.Pow(pw[i], 2) + 1.44 * Math.Pow(10, 6) * Math.Pow(pw[i], 4)) /
                    (Math.Pow(Math.Pow(pw[i], 2) + 1.6 * Math.Pow(10, 5), 2) * (Math.Pow(pw[i], 2) + 9.61 * Math.Pow(10, 6)));
                pw[i] = Math.Pow(pw[i], 0.33);
            }
            for (int i = 0; i < filters; i++)
                result[i] = something[i] * pw[i];
            return result;
        }

        public static double hzToBark (double freq)
        {
            return 6 * Math.Log(freq/600 +Math.Sqrt(1 + Math.Pow(freq/600,2)));
        }

        public static double barkToHz (double barkFreq)
        {
            return 600 * Math.Sinh(barkFreq / 6);
        }

        public static double[,] fftToBark(int frameLength, int sampleRate, int filters)
        {
            double[,] result = new double[filters, frameLength / 2 + 1];
            double minfreq = 0;
            double maxfreq = sampleRate / 2;
            double minBark = hzToBark(minfreq);
            double maxBark = hzToBark(maxfreq);
            double stepBark = (maxBark - minBark) / (filters - 1);
            double[] binBark = new double[frameLength / 2 + 1];
            double[] lowFreq = new double[frameLength / 2 + 1];
            double[] highFreq = new double[frameLength / 2 + 1];
            for (int i = 0; i < binBark.Length; i++)
            {
                binBark[i] = hzToBark(i * sampleRate / frameLength);
            }
            double[] min = new double [binBark.Length];
            for (int i=0; i<filters; i++)
            {
                double midBark = minBark + i * stepBark;
                for (int j=0; j<binBark.Length;j++)
                {
                    lowFreq[j] = binBark[j] - midBark - 0.5;
                    highFreq[j] = binBark[j] - midBark + 0.5;                    
                }
                for (int k=0; k<lowFreq.Length; k++)
                {
                    if (highFreq[i] < -2.5 * lowFreq[i])
                        min[i] = highFreq[i];
                    else
                        min[i] = -2.5 * lowFreq[i];
                    if (min[i] > 0)
                        min[i] = 0;
                }
                for (int j=0; j<frameLength/2+1;j++)
                {
                    result[i,j] = Math.Pow(10,min[j]);
                }
            }
            return result;
        }

        public static List<double> Pitch(double[] source, int frameLength, int frameInc, int sampleRate)
        {
            List<double> result = new List<double>();
            for (int i = 0; i + frameLength < source.Length + 1; i=i+frameInc)
            {
                double[] normalizedData = new double[frameLength];
                Array.Copy(source, i, normalizedData, 0, frameLength);
                for (int j = 0; j < normalizedData.Length; j++)
                {
                    normalizedData[j] = normalizedData[j] * (0.53836 - 0.46164 * Math.Cos((2 * Math.PI * j) / (normalizedData.Length - 1)));
                }
                double[] autoCor = new double [frameLength];
                double max = 0;
                int pitch = 0;
                int start = (int)Math.Floor(sampleRate / 330.0);
                int end = (int)Math.Floor(sampleRate / 60.0);
                //for (int k = 0; k < frameLength; k++)
                for (int k = start; k < end; k++)
                {
                    for (int m = 0; m < frameLength - k - 1; m++)                    
                    {
                        autoCor[k] += source[i + m] * source[i + m + k];
                    }
                    if (autoCor[k] > max && k > 20)
                    {
                        max = autoCor[k];
                        pitch = k;
                    }
                }
                    result.Add((double)sampleRate/pitch);
            }
            return result;
        }

        public static double[] SilenceDetection (double[] source, int frameLength)
        {
            int nframes = source.Length/frameLength;
            List<double> result = new List<double>();
            bool[] vocalized = new bool[nframes];            
            double[] energies = new double[nframes];
            double eThreshold = Ethreshold(source, frameLength, out energies);
              
            double[] zcr = new double[nframes];
            zcr = ZeroCrossingRate(source, frameLength);
            double zcrThreshold = 0.35;//UpdateZcrThr(source, zcr, 0, 10);
            int firstPause=0; 
            int M=0;
            bool flag = false;
            for (int i=10; i<nframes; i++)
            {
                if (energies[i] > eThreshold)
                    if (zcr[i] < zcrThreshold)
                    {
                        flag = false;
                        M = 0;
                        for (int j = i * frameLength; j < (i + 1) * frameLength; j++)
                            result.Add(source[j]);
                    }
                    else
                    {
                        if (flag==false)
                        {
                            firstPause = i;
                            flag=true;
                        }
                        M++;
                        eThreshold = UpdateEThr(source, energies, firstPause, M);
                        //zcrThreshold = UpdateZcrThr(source, zcr, firstPause, M);
                        vocalized[i] = false;
                        
                    }
                else
                {
                    if (flag == false)
                    {
                        firstPause = i;
                        flag = true;
                    }
                    M++;
                    UpdateEThr(source, energies, firstPause, M);
                    //zcrThreshold = UpdateZcrThr(source, zcr, firstPause, M);
                    vocalized[i] = false;
                }
            }
            double[] resultAr = result.ToArray();
            return resultAr;
        }

        public static double Ethreshold (double[] source, int frameLength, out double[] energies)
        {
            int nframes = source.Length/frameLength;
            energies = new double [nframes];
            int pos=0;
            for (int i=0; i<nframes; i++)
            {
                for (int j=pos; j< pos+frameLength; j++)
                {
                    energies[i] += source[j] * source[j];// *(0.53836 - 0.46164 * Math.Cos((2 * Math.PI * (pos + frameLength - j)) / (frameLength - 1)));
                }
                pos = pos + frameLength;
            }
            double mean = 0;
            for (int i=0; i<10; i++)
            {
                if (mean < energies[i])
                    mean = energies[i];
            }
            mean = mean / 25;
            return mean;
        }
        
        public static double UpdateEThr(double[] source, double[] energies, int i, int M)
        {
            double thr = 0;
            for (int j = i; j < i + M; j++)
            {
                if (energies[j] > thr)
                    thr = energies[j];
            }
            thr /= 25;
            return thr;
        }

        public static double UpdateZcrThr(double[] source, double[] zcr, int i, int M)
        {
            double thr = 0;
            for (int j = i; j < i + M ; j++)
            {
                if (zcr[j] > thr)
                    thr = zcr[j];
            }
            thr *= 5;
            return thr;
        }

        public static double[] ZeroCrossingRate (double[]source, int frameLength)
        {
            int nframes = source.Length/frameLength;
            double[] zcr = new double[nframes];
            int pos = 1;
            for (int i = 0; i < nframes; i++)
            {
                for (int j = pos; j < pos + frameLength - 1; j++)
                {
                    zcr[i] += Math.Abs(Math.Sign(source[j]) - Math.Sign(source[j-1]));// *(0.53836 - 0.46164 * Math.Cos((2 * Math.PI * (pos + frameLength - j)) / (frameLength - 1)));
                }
                zcr[i] = zcr[i] / frameLength;
                pos = pos + frameLength;
            }
            return zcr;
        }

        public static Complex[] ComputeRoots (double[] polynom)
        {
            Array.Reverse(polynom);
            Polynomial p = new Polynomial(polynom);
            PolyLib.Complex[] roots = p.Roots();
            Complex[] result = new Complex[roots.Length];
            for (int i=0;i<roots.Length;i++)
            {
                result[i] = new Complex();
                result[i].real = roots[i].Re;
                result[i].imaginary = roots[i].Im;
            }
            return result;
        }
    }

    class VectorQuantinization
    {
        public static double[][] CreateMatrix(int clusters, int attributes)
        {
            double[][] matrix = new double[clusters][];
            for (int i = 0; i < clusters; i++)
                matrix[i] = new double[attributes];
            return matrix;
        }

        public static void ComputeMeans (double[][] source, int[] clustering, ref double[][] means)
        {
            for (int i = 0; i < means.Length; i++)          //i - номер кластера
                for (int j = 0; j < means[i].Length; j++)   //j - номер аттрибута
                    means[i][j] = 0.0;
            int[] clusterCounts = new int[means.Length];
            for (int i=0; i<source.Length;i++)
            {
                int cluster = clustering[i];
                clusterCounts[cluster]++;
                for (int j = 0; j < source[i].Length; j++)
                    means[cluster][j] += source[i][j]; 
            }
            for (int i = 0; i < means.Length; i++)
                for (int j = 0; j < means[i].Length; j++)
                    means[i][j] /= clusterCounts[i];
        }

        public static double[] ComputeCenter(double[][] source, int[] clustering, int cluster, double[][] means)
        {
            double[] center = new double[means[0].Length]; //кол-ву аттрибутов
            double minDistortion = double.MaxValue;
            for (int i=0; i<source.Length; i++)
            {
                int clust = clustering[i];
                if (clust != cluster) continue;
                double distortion = ComputeDistortion(source[i], means[cluster]);
                if (distortion < minDistortion)
                {
                    minDistortion = distortion;
                    for (int j = 0; j < center.Length; j++)
                        center[j] = source[i][j];
                }
            }
            return center;
        }

        public static double ComputeDistortion(double[] vector1,double[] vector2)
        {
            double sumDistortion = 0.0;
            for (int i = 0; i < vector1.Length; i++)
                sumDistortion += Math.Pow(vector1[i] - vector2[i], 2);
            sumDistortion = Math.Sqrt(sumDistortion);
            return sumDistortion;
        }

        public static void UpdateCenters (double[][] source, int[] clustering, double[][]means, double[][] centers)
        {
            for (int i=0; i<centers.Length; i++)
            {
                double[] center = ComputeCenter(source, clustering, i, means);
                centers[i] = center;
            }
        }

        public static bool Distribution (double[][] source, int[] clustering, double[][] centers)
        {
            bool changed = false;
            double[] distortions = new double[centers.Length]; //массив расстояний до центра каждого кластера
            for (int i=0; i<source.Length; i++)
            {
                for (int j = 0; j < centers.Length; j++)
                    distortions[j] = ComputeDistortion(source[i], centers[j]);
                int newCluster = IndexOfMin(distortions);
                if (newCluster != clustering[i])
                {
                    changed = true;
                    clustering[i] = newCluster;
                }
            }
            return changed;
        }

        public static int IndexOfMin (double[] array)
        {
            int indexOfMin = 0;
            double minEl = array[0];
            for (int i=1; i<array.Length; i++)
            {
                if(array[i] < minEl)
                {
                    minEl = array[i];
                    indexOfMin = i;
                }
            }
            return indexOfMin;
        }

        public static int[] InitializeClusters(int numVectors, int clusters, int randomSeed)
        {
            Random random = new Random(randomSeed);
            int[] clustering = new int[numVectors];
            for (int i=0; i<clusters; i++)
                clustering[i] = i;          //в каждом кластере теперь как минимум 1 вектор
            for (int i=clusters; i<clustering.Length; i++)
                clustering[i] = random.Next(0,clusters); //остальные вектора раскидываем случайно
            return clustering;
        }

        public static List<List<double[]>> ReturnClusters(double[][] source, int[] clustering, int numClusters, int attributes) 
        {  
            List<List<double[]>> clusters = new List<List<double[]>>();
            for (int i = 0; i < numClusters; i++)
            {
                List<double[]> temp = new List<double[]>();
                clusters.Add(temp);
            }
            for (int i=0; i<clustering.Length; i++)
            {
                clusters[clustering[i]].Add(source[i]);
            }
            return clusters;
        }

        public static double[][] CreateCodebook (double[][] source, int clusters, int attributes, int maxIteration, out int[] clustering)
        {
            bool changed = true;
            int iter = 0;
            int numVectors = source.Length;
            clustering = InitializeClusters(numVectors, clusters, 0);
            double[][] means = CreateMatrix(clusters, attributes);
            double[][] centers = CreateMatrix(clusters, attributes);
            ComputeMeans(source, clustering, ref means);
            UpdateCenters(source, clustering, means, centers);
            while (changed == true && iter < maxIteration)   //пока центры меняются и не превышено максимальное количество итераций
            {
                changed = Distribution(source, clustering, centers);
                ComputeMeans(source, clustering, ref means);
                UpdateCenters(source, clustering, means, centers);
                iter++;
            }
            return centers;
        }

        public static double CompareTestWithCodeWords (double[][] codebook, double[][] test)
        {
            int vectors = test.Length;
            int order = test[0].Length;
            int clusters = codebook.Length;
            double[][] distortions = CreateMatrix(vectors, clusters);
            for (int i = 0; i < vectors; i++)
                for (int j = 0; j < clusters; j++)
                    for (int k = 0; k < order; k++)
                        distortions[i][j] += distortions[i][j] + Math.Sqrt(Math.Pow(test[i][k]-codebook[j][k],2));
            double[] minDistortions = new double[vectors];
            for (int i = 0; i < vectors; i++)
                minDistortions[i] = distortions[i].Min(); //вектор наблюдения относится к тому кластеру,
                                                          //от которого расстояние наименьшее, и это расстояние нас и интересует
            double totalDistortion = 0;
            for (int i=0; i<vectors; i++)
            {
                totalDistortion += minDistortions[i];
            }
            totalDistortion /= vectors;
            return totalDistortion;
        }

        public static int VQIdentification (double[][][] codebooks, double[][] test)
        {
            int languages = codebooks.Length;
            double[] distortions = new double[languages];
            for (int i=0; i<languages; i++)
            {
                distortions[i] = CompareTestWithCodeWords(codebooks[i], test);
            }
            return IndexOfMin(distortions);
        }
    }

    public class GaussianMixtureModel
    {
        public double[] means;
        public double[][] covariance;
        public double pi;

        public static double[][] InitializeCovarianceMatrix(double[][] cluster, double[] means)
        {
            int rows = cluster.GetLength(0);
            int cols = cluster[0].Length;
            double[][] cov = new double[cols][];
            for (int i = 0; i < cols; i++ )
            {
                cov[i] = new double[cols];
            }
            for (int i = 0; i < cols; i++)
            {
                for (int j = i; j < cols; j++)
                {
                    double s = 0.0;
                    for (int k = 0; k < rows; k++)
                        s += (cluster[k][j] - means[j]) * (cluster[k][i] - means[i]);
                    s /= (rows-1);
                    cov[i][j] = s;
                    cov[j][i] = s;
                }
            }
            return cov;
        }

        public static GaussianMixtureModel[] InitializeComponents (double[][] source, int order)
        {
            GaussianMixtureModel[] components = new GaussianMixtureModel[order];
            for (int i = 0; i < order; i++)
                components[i] = new GaussianMixtureModel();
            int numClusters = components.Length;
            int vectors = source.Length;
            int numCoefs = source[0].Length;
            int [] clustering = new int[vectors];
            double[][] codebook = VectorQuantinization.CreateCodebook(source, numClusters, numCoefs, 100, out clustering);
            List<List<double[]>> clusters = VectorQuantinization.ReturnClusters(source, clustering, numClusters, numCoefs);
            for (int i=0; i<numClusters; i++)
            {
                components[i].means = codebook[i];
                double[][] cluster = clusters[i].ToArray();
                components[i].covariance = InitializeCovarianceMatrix(cluster,components[i].means);
                components[i].pi = (double)cluster.Length/vectors;
            }
            return components;
        }

        public static double NormalDistribution (double[] vector, double[] means, double[][] covariance)
        {
            double Nd = 0;
            double[][] degreeM;
            double degree = 0;
            if (vector.Length == means.Length)
            {
                double[][] tempT = new double[vector.Length][];
                double[][] temp = new double[1][];
                for (int i = 0; i < temp.Length; i++)
                {
                    temp[i] = new double[vector.Length];
                    for (int j = 0; j < vector.Length; j++)
                        temp[i][j] = vector[j] - means[j];
                }
                double[][] covInv = MatrixInverse(covariance);
                double[][] intermediate = MatrixProduct(temp, covInv);
                for (int i = 0; i < vector.Length; i++)
                {
                    tempT[i] = new double[1];
                    tempT[i][0] = vector[i] - means[i];
                }
                degreeM = MatrixProduct(intermediate, tempT);
                degree = degreeM[0][0];
                double covDet = MatrixDeterminant(covariance);
                Nd = (1/(Math.Pow(2*Math.PI,vector.Length/2) * Math.Pow(Math.Abs(covDet),0.5))) *
                    Math.Pow(Math.E,-0.5 * degree);
            }
            else
                throw new ArgumentException("вектор наблюдения и средних должны иметь одинаковую длину");
            return Nd;
        }
        public static double[][] EStep (double[][] vectors, GaussianMixtureModel[] components)
        {
            double[][] assignmentScores = new double[vectors.Length][];
            for (int i=0; i<assignmentScores.Length;i++)
                assignmentScores[i] = new double [components.Length];
            for (int i = 0; i < assignmentScores.Length; i++)
            {
                for (int j = 0; j < assignmentScores[i].Length; j++)
                {
                    double numerator = components[j].pi *
                        NormalDistribution(vectors[i], components[j].means, components[j].covariance);
                    double denominator = 0;
                    for (int k = 0; k < components.Length; k++ )
                        denominator += components[k].pi *
                        NormalDistribution(vectors[i], components[k].means, components[k].covariance);
                    assignmentScores[i][j] = numerator / denominator;
                }
            }
            return assignmentScores;
        }

        public static GaussianMixtureModel[] UpdateMeans(double[][] vectors, double[][] assignmentScores, GaussianMixtureModel[] components)
        {
            double[] N = new double[components.Length];
            for (int i = 0; i < N.Length; i++)
            {
                N[i] = 0;
                for (int j = 0; j < vectors.Length; j++)
                {
                    N[i] += assignmentScores[j][i];
                }
            }
            for (int i=0; i<components.Length;i++)
            {
                double[] tempMeans = new double[components[i].means.Length];
                for (int j=0; j<vectors.Length;j++)
                {
                    for (int k=0; k<vectors[j].Length; k++)
                    {
                        tempMeans[k] += assignmentScores[j][i] * vectors[j][k];
                    }
                }
                for (int k=0; k<tempMeans.Length;k++)
                {
                    components[i].means[k] = tempMeans[k] / N[i];
                }
            }
            return components;
        }

        public static GaussianMixtureModel[] UpdateCovariance(double[][] vectors, double[][] assignmentScores, GaussianMixtureModel[] components)
        {
            int rows = vectors.GetLength(0);
            int cols = vectors[0].Length;
            for (int k = 0; k < components.Length; k++)
                for (int i = 0; i < components[k].covariance.Length; i++)
                    for (int j = 0; j < components[k].covariance.Length; j++ )
                    {
                        components[k].covariance[i][j] = 0;
                    }
            double[] N = new double[components.Length];
            for (int i = 0; i < N.Length; i++)
            {
                N[i] = 0;
                for (int j = 0; j < vectors.Length; j++)
                {
                    N[i] += assignmentScores[j][i];
                }
            }
            for (int k = 0; k < components.Length; k++)
            {
                for (int i = 0; i < cols; i++)
                {
                    for (int j = i; j < cols; j++)
                    {
                        double s = 0.0;
                        for (int l = 0; l < rows; l++)
                            s += assignmentScores[l][k] * ((vectors[l][j] - components[k].means[j]) * (vectors[l][i] - components[k].means[i]));
                        s /= N[k];
                        components[k].covariance[i][j] = s;
                        components[k].covariance[j][i] = s;
                    }
                }
            }
            return components;
        }

        public static GaussianMixtureModel[] UpdatePi (double[][] vectors, double[][] assignmentScores, GaussianMixtureModel[] components)
        {
            int NTotal = vectors.Length;
            double[] N = new double[components.Length];
            for (int i = 0; i < N.Length; i++)
            {
                N[i] = 0;
                for (int j = 0; j < vectors.Length; j++)
                {
                    N[i] += assignmentScores[j][i];
                }
            }
            for (int k = 0; k < components.Length; k++)
                components[k].pi = N[k] / NTotal;
            return components;
        }

        public static double LogLikelihood (double[][] vectors, GaussianMixtureModel[] components)
        {
            int N = vectors.Length;
            double likelihood=0;
            for (int i=0; i<N; i++)
            {
                double temp=0;
                for (int k=0; k< components.Length;k++)
                {
                    temp += components[k].pi * NormalDistribution(vectors[i], components[k].means, components[k].covariance);
                }
                likelihood += Math.Log(temp);
            }
            return likelihood;
        }

        public static GaussianMixtureModel[] Training (double[][] vectors, int order, double constant, int maxIter)
        {
            GaussianMixtureModel[] components = InitializeComponents(vectors, order);
            int iter = 0;
            double likelihoodPrev = LogLikelihood(vectors,components);
            double likelihoodCur = Double.MaxValue;
            while (Math.Abs(likelihoodCur - likelihoodPrev) > constant)
            {
                if (iter > maxIter)
                    break;
                else
                {
                    double[][] assignmentScores = EStep(vectors, components);
                    components = UpdateMeans(vectors, assignmentScores, components);
                    components = UpdateCovariance(vectors, assignmentScores, components);
                    components = UpdatePi(vectors, assignmentScores, components);
                    if (iter == 0)
                        likelihoodCur = LogLikelihood(vectors, components);
                    else
                    {
                        likelihoodPrev = likelihoodCur;
                        likelihoodCur = LogLikelihood(vectors, components);
                    }
                    iter++;
                }
            }
            return components;
        }

        public static int Classification(double[][] vectors, GaussianMixtureModel[][] models)
        {
            int classes = models.Length;
            double[] pdfs = new double[classes];
            for (int i = 0; i < classes; i++)
            {
                for (int j=0; j<vectors.Length; j++)
                {
                    for (int k=0; k<models[i].Length;k++)
                    {
                        pdfs[i] += models[i][k].pi * NormalDistribution(vectors[j], models[i][k].means, models[i][k].covariance);
                    }
                }
            }
            int index = 0;
            double max = Double.MinValue;
            for (int i=0; i<classes; i++)
            {
                if (pdfs[i]>max)
                {
                    index = i;
                    max = pdfs[i];
                }
            }
            return index;
        }

        static double[][] MatrixCreate(int rows, int cols)
        {
            double[][] result = new double[rows][];
            for (int i = 0; i < rows; ++i)
                result[i] = new double[cols]; 
            return result;
        }

        static double[][] MatrixProduct(double[][] matrixA, double[][] matrixB)
        {
            int aRows = matrixA.Length; int aCols = matrixA[0].Length;
            int bRows = matrixB.Length; int bCols = matrixB[0].Length;
            if (aCols != bRows)
                throw new Exception("Конфликт размерности матриц");
            double[][] result = MatrixCreate(aRows, bCols);
            Parallel.For(0, aRows, i =>
            {
                for (int j = 0; j < bCols; ++j)
                    for (int k = 0; k < aCols; ++k)
                        result[i][j] += matrixA[i][k] * matrixB[k][j];
            }
            );
            return result;
        }

        static double[][] MatrixMakeCopy(double[][] matrix)
        {
            double[][] result = MatrixCreate(matrix.Length, matrix[0].Length);
            for (int i = 0; i < matrix.Length; ++i)
                for (int j = 0; j < matrix[i].Length; ++j)
                    result[i][j] = matrix[i][j];
            return result;
        }

        static double[][] MatrixDecompose(double[][] matrix, out int[] perm, out int toggle)
        {
            int n = matrix.Length; 
            double[][] result = MatrixMakeCopy(matrix);
            perm = new int[n];
            for (int i = 0; i < n; ++i) { perm[i] = i; }
            toggle = 1;
            for (int j = 0; j < n - 1; ++j) 
            {
                double colMax = Math.Abs(result[j][j]); 
                int pRow = j;
                for (int i = j + 1; i < n; ++i)
                {
                    if (result[i][j] > colMax)
                    {
                        colMax = result[i][j];
                        pRow = i;
                    }
                }
                if (pRow != j) 
                {
                    double[] rowPtr = result[pRow];
                    result[pRow] = result[j];
                    result[j] = rowPtr;
                    int tmp = perm[pRow];
                    perm[pRow] = perm[j];
                    perm[j] = tmp;
                    toggle = -toggle; 
                }
                if (Math.Abs(result[j][j]) < 1.0E-20)
                    return null; 
                for (int i = j + 1; i < n; ++i)
                {
                    result[i][j] /= result[j][j];
                    for (int k = j + 1; k < n; ++k)
                        result[i][k] -= result[i][j] * result[j][k];
                }
            } 
            return result;
        }

        static double[] Solve(double[][] luMatrix, double[] b)
        {
            int n = luMatrix.Length;
            double[] x = new double[n];
            b.CopyTo(x, 0);
            for (int i = 1; i < n; ++i)
            {
                double sum = x[i];
                for (int j = 0; j < i; ++j)
                    sum -= luMatrix[i][j] * x[j];
                x[i] = sum;
            }
            x[n - 1] /= luMatrix[n - 1][n - 1];
            for (int i = n - 2; i >= 0; --i)
            {
                double sum = x[i];
                for (int j = i + 1; j < n; ++j)
                    sum -= luMatrix[i][j] * x[j];
                x[i] = sum / luMatrix[i][i];
            }
            return x;
        }

        static double[][] MatrixInverse(double[][] matrix)
        {
            int n = matrix.Length;
            double[][] result = MatrixMakeCopy(matrix);
            int[] perm;
            int toggle;
            double[][] lum = MatrixDecompose(matrix, out perm, out toggle);
            if (lum == null)
                throw new Exception("Unable to compute inverse");
            double[] b = new double[n];
            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < n; ++j)
                {
                    if (i == perm[j])
                        b[j] = 1.0;
                    else
                        b[j] = 0.0;
                }
                double[] x = Solve(lum, b);
                for (int j = 0; j < n; ++j)
                    result[j][i] = x[j];
            }
            return result;
        }

        static double MatrixDeterminant(double[][] matrix)
        {
            int[] perm;
            int toggle;
            double[][] lum = MatrixDecompose(matrix, out perm, out toggle);
            if (lum == null)
                throw new Exception("Unable to compute MatrixDeterminant");
            double result = toggle;
            for (int i = 0; i < lum.Length; ++i)
                result *= lum[i][i];
            return result;
        }
    }

    public class SupportVectorMachine
    {
        // версия со списками
        public static double[] Training(List<List<double>> vectors, List<int> labels, double epsilon, out double threshold)
        {
            double gamma = 0.01;
            double[] diagonal = new double[vectors.Count];
            for (int i = 0; i < vectors.Count; i++)
            {
                diagonal[i] = Kernel(i, i, vectors) + gamma;
            }

            List<int> ones = new List<int>();
            for (int i = 0; i < labels.Count; i++)
            {
                ones.Add(1);
            }

            double[] eta = ConjugateGradientMethod(labels, vectors, labels, diagonal, epsilon);
            double[] nu = ConjugateGradientMethod(ones, vectors, labels, diagonal, epsilon);

            double s = 0;
            for (int i = 0; i < labels.Count; i++)
            {
                s += labels[i] * eta[i];
            }

            threshold = 0;
            for (int i = 0; i < eta.Length; i++)
            {
                threshold += eta[i];
            }
            threshold = threshold / s;    //порог

            double[] weights = new double[nu.Length];
            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = (nu[i] - eta[i] * threshold) * labels[i];
            }
            return weights;
        }

        public static int ComputeOutput(double[] vector, List<List<double>> trainingVectors, double[] weights, double threshold)
        {
            double output = threshold;
            for (int i = 0; i < trainingVectors.Count; i++)
            {
                double sum = 0;
                for (int j = 0; j < vector.Length; j++)
                    sum += trainingVectors[i][j] * vector[j];
                output += weights[i] * sum;
            }
            if (output >= 0)
                return 1;
            else
                return -1;
        }

        public static int Classification(double[][] testVectors, List<List<double>> trainingVectors, double[] weights, double threshold)
        {
            int result = 0;
            for (int i = 0; i < testVectors.Length; i++)
            {
                result += ComputeOutput(testVectors[i], trainingVectors, weights, threshold);
            }
            if (result >= 0)
                return 1;
            else
                return -1;
        }

        public static double[] ConjugateGradientMethod(List<int> vector, List<List<double>> vectors, List<int> labels, double[] diagonal, double epsilon)
        {
            double[] x = new double[vector.Count];
            double[] r = new double[vector.Count];
            double[] p = new double[vector.Count];
            double[] H = new double[vector.Count];
            double[] cols = new double[vectors.Count];

            for (int i = 0; i < vector.Count; i++)
            {
                p[i] = r[i] = vector[i];
            }

            double norma = 0;
            for (int i = 0; i < r.Length; i++)
            {
                norma += r[i] * r[i];
            }

            double beta = 1;
            int iter = 0;

            while (norma > epsilon)
            {
                iter++;
                if (iter > vector.Count)
                    break;

                for (int i = 0; i < p.Length; i++)
                {
                    p[i] = r[i] + beta * p[i];
                }

                for (int i = 0; i < p.Length; i++)
                {
                    for (int j = 0; j < cols.Length; j++)
                    {
                        cols[j] = Kernel(i, j, vectors) * labels[j] * p[j];
                    }
                    double s = 0;
                    for (int j = 0; j < i; j++)
                        s += cols[j];
                    for (int j = i + 1; j < cols.Length; j++) //пропускаем i-й
                        s += cols[j];
                    if (labels[i] > 0)
                        H[i] = s;
                    else
                        H[i] = -s;
                }

                for (int i = 0; i < p.Length; i++)
                {
                    H[i] += diagonal[i] * p[i];
                }

                double sum = 0;
                for (int i = 0; i < p.Length; i++)
                {
                    sum += p[i] * H[i];
                }

                double alpha = norma / sum;
                for (int i = 0; i < p.Length; i++)
                {
                    x[i] += alpha * p[i];
                }
                for (int i = 0; i < r.Length; i++)
                {
                    r[i] -= alpha * H[i];
                }

                double newNorma = 0;
                for (int i = 0; i < r.Length; i++)
                    newNorma += r[i] * r[i]; // эрки нового шага
                beta = newNorma / norma; // делим на эрки с прошлого шага
                norma = newNorma;        //обновляем для следующего шага
            }
            return x;
        }

        public static double Kernel(int i, int j, List<List<double>> vectors)
        {
            double result = 0;
            for (int k = 0; k < vectors[i].Count; k++)
            {
                result += vectors[i][k] * vectors[j][k];
            }
            return result;
        }

       // версия с массивами
        public static double[] Training (double[][] vectors, int[] labels, double epsilon, out double threshold)
        {
            double gamma = 0.01;
            double[] diagonal = new double[vectors.Length];
            for (int i=0; i<vectors.Length; i++)
            {
                diagonal[i] = Kernel(i,i,vectors) + gamma;
            }

            int[] ones = new int[labels.Length];
            for (int i = 0; i< labels.Length; i++)
            {
                ones[i] = 1;
            }

            double[] eta = ConjugateGradientMethod(labels, vectors, labels, diagonal, epsilon);
            double[] nu = ConjugateGradientMethod(ones, vectors, labels, diagonal, epsilon);

            double s = 0;
            for (int i=0; i<labels.Length;i++)
            {
                s += labels[i] * eta[i];
            }

            threshold = 0;
            for (int i = 0; i < eta.Length; i++)
            {
                threshold += eta[i];
            }
            threshold = threshold / s;    //порог

            double[] weights = new double[nu.Length];
            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = (nu[i] - eta[i] * threshold) * labels[i];
            }
            return weights;
        }

        public static int ComputeOutput(double[] vector, double[][] trainingVectors, double[] weights, double threshold)
        {
            double output = threshold;
            for (int i = 0; i < trainingVectors.Length; i++)
            {
                double sum = 0;
                for (int j = 0; j < vector.Length; j++)
                    sum += trainingVectors[i][j] * vector[j];
                output += weights[i] * sum;
            }
            if (output >= 0)
                return 1;
            else
                return -1;
        }

        public static int Classification(double[][] testVectors, double[][] trainingVectors, double[] weights, double threshold)
        {
            int result = 0;
            for (int i=0; i<testVectors.Length; i++)
            {
                result += ComputeOutput(testVectors[i], trainingVectors, weights, threshold);
            }
            if (result >= 0)
                return 1;
            else
                return -1;
        }

        public static double[][] MultiClassesTraining(double[][] trainingVectors, int[]labels, int classes, double epsilon, out double[] thresholds)
        {
            double[][] modelsWeights = new double[classes][];
            thresholds = new double[classes];
            for (int i = 0; i < classes; i++)
            {
                modelsWeights[i] = new double[trainingVectors.Length];
                int[] tempLabels = new int[trainingVectors.Length];
                for (int j=0; j<trainingVectors.Length; j++)
                {
                    if(i != labels[j])  //текущий класс метим 1 остальные -1 (стратегия один против всех)
                    {
                        tempLabels[j] = -1;
                    }
                    else
                        tempLabels[j] = 1;
                }
                modelsWeights[i] = Training(trainingVectors, tempLabels, epsilon, out thresholds[i]);
            }
            return modelsWeights;
        }

        public static int MultiClassesClassification(double[][] testVectors, double[][] trainingVectors, double[][] weights, double[] thresholds, int classes)
        {
            int[] results = new int[classes];
            for (int i = 0; i < classes; i++)
            {
                for (int j = 0; j < testVectors.Length; j++)
                {
                    results[i] += ComputeOutput(testVectors[j], trainingVectors, weights[i], thresholds[i]);
                }
            }
            int index = 0;
            double max = Double.MinValue;
            for (int i=0; i<results.Length; i++)
            {
                if (results[i]>=max)
                {
                    max = results[i];
                    index = i;
                }
            }
            return index;
        }

        public static double[] ConjugateGradientMethod(int[] vector, double[][] vectors, int[]labels, double[] diagonal, double epsilon)
        {
            double[] x = new double [vector.Length];
            double[] r = new double [vector.Length];
            double[] p = new double [vector.Length];
            double[] H = new double [vector.Length];
            double[] cols = new double[vectors.Length];

            for (int i=0; i<vector.Length;i++)
            {
                p[i] = r[i] = vector[i];
            }

            double norma = 0;
            for (int i=0; i<r.Length; i++)
            {
                norma += r[i] * r[i];
            }

            double beta = 1;
            int iter = 0;

            while (norma > epsilon)
            {
                iter++;
                if (iter > vector.Length)
                    break;

                for (int i=0; i<p.Length; i++)
                {
                    p[i] = r[i] + beta * p[i];
                }

                for (int i=0; i<p.Length; i++)
                {
                    for (int j=0; j<cols.Length; j++)
                    {
                        cols[j] = Kernel(i, j, vectors) * labels[j] * p[j];
                    }
                    double s = 0;
                    for (int j = 0; j < i; j++)
                        s += cols[j];
                    for (int j = i + 1; j < cols.Length; j++) //пропускаем i-й
                        s += cols[j];
                    if (labels[i] > 0)
                        H[i] = s;
                    else
                        H[i] = -s;
                }

                for (int i=0; i<p.Length;i++)
                {
                    H[i] += diagonal[i] * p[i];
                }

                double sum = 0;
                for (int i=0; i<p.Length;i++)
                {
                    sum += p[i] * H[i];
                }

                double alpha = norma / sum;
                for (int i=0; i<p.Length;i++)
                {
                    x[i] += alpha * p[i];
                }
                for (int i=0;i<r.Length;i++)
                {
                    r[i] -= alpha * H[i];
                }

                double newNorma = 0;
                for (int i = 0; i < r.Length; i++)
                    newNorma += r[i] * r[i]; // эрки нового шага
                beta = newNorma / norma; // делим на эрки с прошлого шага
                norma = newNorma;        //обновляем для следующего шага
            }
            return x;
        }

        public static double Kernel(int i, int j, double[][] vectors)
        {
            double result = 0;
            for (int k = 0; k < vectors[i].Length; k++)
            {
                result += vectors[i][k] * vectors[j][k];
            }
            return result;
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
                    normalizedData[j] = normalizedData[j] * (0.53836 - 0.46164 * Math.Cos((2 * Math.PI * (j)) / (normalizedData.Length - 1)));
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

    public class Filters
    {
        public static double[] LowPassFilter(double[] data, double Fc, int order, string window, out double[] weights)
        {
            double[] output = new double[data.Length];

            weights = new double[order];

            for (int i = 0; i < order; i++)
            {
                if (i - order / 2 == 0)
                    weights[i] = 2 * Math.PI * Fc;
                else
                    weights[i] = Math.Sin(2 * Math.PI * Fc * (i - order / 2)) / ((i - order / 2));
                if (window == "hm")
                    weights[i] *= (0.54 - 0.46 * Math.Cos(2 * Math.PI * i / (order - 1)));
                if (window == "hn")
                    weights[i] *= (0.5 * (1 - Math.Cos((2 * Math.PI * i) / (order - 1))));
                if (window == "bm")
                    weights[i] *= (0.42 - 0.5 * Math.Cos((2 * Math.PI * i) / (order - 1)) + 0.08 * Math.Cos((4 * Math.PI * i) / (order - 1)));
            }

            double sum = 0;
            for (int i = 0; i < order; i++)
            {
                sum += weights[i];
            }
            for (int i = 0; i < order; i++)
            {
                weights[i] /= sum;
            }

            for (int i = 0; i < order; i++)
            {
                output[i] = 0;
            }
            double[] temp = new double[Form1.newWav.LeftChData.Length];
            Array.Copy(WAV.GetNormalizedData(Form1.newWav), temp, temp.Length);
            for (int i = order; i < Form1.newWav.LeftChData.Length; i++)
            {
                output[i] = 0;
                for (int j = 0; j < order; j++)
                {
                    output[i] += temp[i - j] * weights[j];
                }
            }
            return output;
        }

        public static double[] HighPassFilter(double[] data, double Fc, int order, string window, out double[] weights)
        {
            double[] output = new double[data.Length];

            weights = new double[order];

            for (int i = 0; i < order; i++)
            {
                if (i - order / 2 == 0)
                    weights[i] = 2 * Math.PI * Fc;
                else
                    weights[i] = Math.Sin(2 * Math.PI * Fc * (i - order / 2)) / ((i - order / 2));
                if (window == "hm")
                    weights[i] *= (0.54 - 0.46 * Math.Cos(2 * Math.PI * i / (order - 1)));
                if (window == "hn")
                    weights[i] *= (0.5 * (1 - Math.Cos((2 * Math.PI * i) / (order - 1))));
                if (window == "bm")
                    weights[i] *= (0.42 - 0.5 * Math.Cos((2 * Math.PI * i) / (order - 1)) + 0.08 * Math.Cos((4 * Math.PI * i) / (order - 1)));
            }

            double sum = 0;
            for (int i = 0; i < order; i++)
            {
                sum += weights[i];
            }
            for (int i = 0; i < order; i++)
            {
                weights[i] /= -sum;
            }
            weights[order / 2] += 1;
            for (int i = 0; i < order; i++)
            {
                output[i] = 0;
            }
            double[] temp = new double[Form1.newWav.LeftChData.Length];
            Array.Copy(WAV.GetNormalizedData(Form1.newWav), temp, temp.Length);
            for (int i = order; i < Form1.newWav.LeftChData.Length; i++)
            {
                output[i] = 0;
                for (int j = 0; j < order; j++)
                {
                    output[i] += temp[i - j] * weights[j];
                }
            }
            return output;
        }

        public static double[] BandPassFilter(double[] data, double FcL, double FcH, int order, string window, out double[] weights)
        {
            if (FcL == FcH)
            {
                FcL -= 0.01;
                FcH += 0.01;
            }
            // low
            double[] weightsL = new double[order];

            for (int i = 0; i < order; i++)
            {
                if (i - order / 2 == 0)
                    weightsL[i] = 2 * Math.PI * FcL;
                else
                    weightsL[i] = Math.Sin(2 * Math.PI * FcL * (i - order / 2)) / ((i - order / 2));
                if (window == "hm")
                    weightsL[i] *= (0.54 - 0.46 * Math.Cos(2 * Math.PI * i / (order - 1)));
                if (window == "hn")
                    weightsL[i] *= (0.5 * (1 - Math.Cos((2 * Math.PI * i) / (order - 1))));
                if (window == "bm")
                    weightsL[i] *= (0.42 - 0.5 * Math.Cos((2 * Math.PI * i) / (order - 1)) + 0.08 * Math.Cos((4 * Math.PI * i) / (order - 1)));
            }

            double sum = 0;
            for (int i = 0; i < order; i++)
            {
                sum += weightsL[i];
            }
            for (int i = 0; i < order; i++)
            {
                weightsL[i] /= sum;
            }


            //high
            double[] weightsH = new double[order];

            for (int i = 0; i < order; i++)
            {
                if (i - order / 2 == 0)
                    weightsH[i] = 2 * Math.PI * FcH;
                else
                    weightsH[i] = Math.Sin(2 * Math.PI * FcH * (i - order / 2)) / ((i - order / 2));
                if (window == "hm")
                    weightsH[i] *= (0.54 - 0.46 * Math.Cos(2 * Math.PI * i / (order - 1)));
                if (window == "hn")
                    weightsH[i] *= (0.5 * (1 - Math.Cos((2 * Math.PI * i) / (order - 1))));
                if (window == "bm")
                    weightsH[i] *= (0.42 - 0.5 * Math.Cos((2 * Math.PI * i) / (order - 1)) + 0.08 * Math.Cos((4 * Math.PI * i) / (order - 1)));
            }

            sum = 0;
            for (int i = 0; i < order; i++)
            {
                sum += weightsH[i];
            }
            for (int i = 0; i < order; i++)
            {
                weightsH[i] /= -sum;
            }
            weightsH[order / 2] += 1;

            // band
            weights = new double[order];
            for (int i = 0; i < weights.Length; i++)
                weights[i] = weightsL[i] + weightsH[i];

            for (int i = 0; i < weights.Length; i++)
                weights[i] = -weights[i];
            weights[order / 2]++;

            // output
            double[] output = new double[data.Length];

            for (int i = 0; i < order; i++)
            {
                output[i] = 0;
            }
            double[] temp = new double[Form1.newWav.LeftChData.Length];
            Array.Copy(WAV.GetNormalizedData(Form1.newWav), temp, temp.Length);
            for (int i = order; i < Form1.newWav.LeftChData.Length; i++)
            {
                output[i] = 0;
                for (int j = 0; j < order; j++)
                {
                    output[i] += temp[i - j] * weights[j];
                }
            }
            return output;
        }

        public static double[] BandStopFilter(double[] data, double FcL, double FcH, int order, string window, out double[] weights)
        {
            if (FcL == FcH)
            {
                FcL -= 0.01;
                FcH += 0.01;
            }
            // low
            double[] weightsL = new double[order];

            for (int i = 0; i < order; i++)
            {
                if (i - order / 2 == 0)
                    weightsL[i] = 2 * Math.PI * FcL;
                else
                    weightsL[i] = Math.Sin(2 * Math.PI * FcL * (i - order / 2)) / ((i - order / 2));
                if (window == "hm")
                    weightsL[i] *= (0.54 - 0.46 * Math.Cos(2 * Math.PI * i / (order - 1)));
                if (window == "hn")
                    weightsL[i] *= (0.5 * (1 - Math.Cos((2 * Math.PI * i) / (order - 1))));
                if (window == "bm")
                    weightsL[i] *= (0.42 - 0.5 * Math.Cos((2 * Math.PI * i) / (order - 1)) + 0.08 * Math.Cos((4 * Math.PI * i) / (order - 1)));
            }

            double sum = 0;
            for (int i = 0; i < order; i++)
            {
                sum += weightsL[i];
            }
            for (int i = 0; i < order; i++)
            {
                weightsL[i] /= sum;
            }


            //high
            double[] weightsH = new double[order];

            for (int i = 0; i < order; i++)
            {
                if (i - order / 2 == 0)
                    weightsH[i] = 2 * Math.PI * FcH;
                else
                    weightsH[i] = Math.Sin(2 * Math.PI * FcH * (i - order / 2)) / ((i - order / 2));
                if (window == "hm")
                    weightsH[i] *= (0.54 - 0.46 * Math.Cos(2 * Math.PI * i / (order - 1)));
                if (window == "hn")
                    weightsH[i] *= (0.5 * (1 - Math.Cos((2 * Math.PI * i) / (order - 1))));
                if (window == "bm")
                    weightsH[i] *= (0.42 - 0.5 * Math.Cos((2 * Math.PI * i) / (order - 1)) + 0.08 * Math.Cos((4 * Math.PI * i) / (order - 1)));
            }

            sum = 0;
            for (int i = 0; i < order; i++)
            {
                sum += weightsH[i];
            }
            for (int i = 0; i < order; i++)
            {
                weightsH[i] /= -sum;
            }
            weightsH[order / 2] += 1;

            // band
            weights = new double[order];
            for (int i = 0; i < weights.Length; i++)
                weights[i] = weightsL[i] + weightsH[i];

            //output
            double[] output = new double[data.Length];

            for (int i = 0; i < order; i++)
            {
                output[i] = 0;
            }
            double[] temp = new double[Form1.newWav.LeftChData.Length];
            Array.Copy(WAV.GetNormalizedData(Form1.newWav), temp, temp.Length);
            for (int i = order; i < Form1.newWav.LeftChData.Length; i++)
            {
                output[i] = 0;
                for (int j = 0; j < order; j++)
                {
                    output[i] += temp[i - j] * weights[j];
                }
            }
            return output;
        }

        public static double[] MovingAverageFilter(double[] data, int order)
        {
            double[] output = new double[data.Length];

            double average = 0;
            for (int i = 0; i < order; i++ )
            {
                average += data[i];
            }
            output[order / 2] = average / order;

            for (int i = order / 2; i < data.Length - order/2; i++)
            {
                average = average + data[i + order / 2] - data[i - order / 2];
                output[i] = average / order;
            }

            return output;
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
            return LeftChData;
        }


        public static double[] GetNormalizedData(WAV curFile)
        {
            if (curFile.channels == 1)
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
            else
            {
                int samples = curFile.wavData.Length / 4; //16 bit  stereo
                double[] LeftChData = new double[samples];
                int i = 0;
                int j = 0;
                while (i < samples)
                {
                    LeftChData[i] = bytesToDouble(curFile.wavData[j], curFile.wavData[j + 1]);
                    j += 4;
                    i++;
                }
                return LeftChData;
            }
        }

        public static byte[] ComputeBytesOfWavData(double[] LeftChData)
        {
            int dataLength = LeftChData.Length * 2;
            byte[] WavData = new byte[dataLength];
            int i = 0;
            int j = 0;
            while (i < LeftChData.Length)
            {
                byte[] bytes = bytesFromDouble(LeftChData[i]);
                WavData[j] = bytes[1];
                WavData[j + 1] = bytes[0];
                i++;
                j += 2;
            }
            return WavData;
        }

        public static byte[] ComputeBytesOfWavData(double[] LeftChData, bool boo)
        {
            int dataLength = LeftChData.Length * 2;
            byte[] WavData = new byte[dataLength];
            int i = 0;
            int j = 0;
            while (i < LeftChData.Length)
            {
                byte[] bytes = bytesFromDouble2(LeftChData[i]);
                WavData[j] = bytes[1];
                WavData[j + 1] = bytes[0];
                i++;
                j += 2;
            }
            return WavData;
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

        public static byte[] bytesFromDouble(double value)
        {
            if (value >= 1)
                value = 0.999;
            if (value <= -1)
                value = -0.999;

            short s = Convert.ToInt16(value * 32768);
            return new byte[] { (byte)(s>>8), (byte)(s&0xff) };
        }

        public static byte[] bytesFromDouble2(double value)
        {
            if (value >= 32768)
                value = 32767;
            if (value <= -32768)
                value = -32767;

            short s = Convert.ToInt16(value);
            return new byte[] { (byte)(s >> 8), (byte)(s & 0xff) };
        }

        public static string GetDuration (double dur, int param)
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
            if (param == 0)
            {
                string[] temp = secondsStr.Split('.');
                secondsStr = String.Join("", temp);
            }
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
