using System;
using System.Linq;
using System.Collections.Generic;

namespace Projekat_tema7.Correction
{
    public class ReedSolomon : IErrorCorrection
    {
        private readonly GaloisField gf;
        private readonly int t;
        private readonly int n;
        private readonly int k;
        private readonly int[] generator;

        public ReedSolomon(int m, int t)
        {
            this.gf = new GaloisField(m);
            this.t = t;
            this.n = (1 << m) - 1;
            this.k = n - (2 * t);

            this.generator = GenerateGeneratorPolynomial();
        }

        private int[] GenerateGeneratorPolynomial()
        {
            int paritySymbols = n - k; 
            int[] gg = new int[paritySymbols + 1];

            gg[0] = 2;
            gg[1] = 1;

            for (int i = 2; i <= paritySymbols; i++)
            {
                gg[i] = 1;

                for (int j = i - 1; j > 0; j--)
                {
                    if (gg[j] != 0) 
                        gg[j] = gg[j - 1] ^ gf.GetAlphaTo((gf.GetIndexOf(gg[j]) + i) % n); 
                    else 
                        gg[j] = gg[j - 1]; 
                }

                gg[0] = gf.GetAlphaTo((gf.GetIndexOf(gg[0]) + i) % n);
            }

            for (int i = 0; i <= paritySymbols; i++)
                gg[i] = gf.GetIndexOf(gg[i]);

            return gg;
        }

        public byte[] Encode(byte[] data)
        {
            if (data.Length != k)
                throw new ArgumentException($"RS({n},{k}) zahteva {k} simbola.");

            int paritySymbols = n - k;
            int[] bb = new int[paritySymbols];

            for (int i = 0; i < paritySymbols; i++)
                bb[i] = 0;

            for (int i = k - 1; i >= 0; i--)
            {
                int feedback = gf.GetIndexOf(data[i] ^ bb[paritySymbols - 1]);

                if (feedback != -1)
                {
                    for (int j = paritySymbols - 1; j > 0; j--)
                    {
                        if (generator[j] != -1) 
                            bb[j] = bb[j - 1] ^ gf.GetAlphaTo((generator[j] + feedback) % n); 
                        else 
                            bb[j] = bb[j - 1]; 
                    }

                    bb[0] = gf.GetAlphaTo((generator[0] + feedback) % n);
                }
                else
                {
                    for (int j = paritySymbols - 1; j > 0; j--)
                        bb[j] = bb[j - 1];

                    bb[0] = 0;
                }
            }

            byte[] codeword = new byte[n];

            for (int i = 0; i < paritySymbols; i++)
                codeword[i] = (byte)bb[i];

            for (int i = 0; i < k; i++)
                codeword[i + paritySymbols] = data[i];

            return codeword;
        }

        public byte[] Decode(byte[] data)
        {
            int[] syndromes = ComputeSyndromes(data);

            bool noErr = true;
            for (int i = 0; i < syndromes.Length; i++)
            {
                if (syndromes[i] != 0)
                {
                    noErr = false;
                    break;
                }
            }
            if (noErr) return data;

            int[] sigma = BerlekampMassey(syndromes);
            int[] errorLocations = ChienSearch(sigma);

            return Forney(data, syndromes, errorLocations);
        }

        public int[] ComputeSyndromes(byte[] codeword)
        {
            int[] syndromes = new int[2 * t];

            for (int i = 1; i <= 2 * t; i++)
            {
                int syndrome = 0; 
                for (int j = 0; j < codeword.Length; j++) 
                    if (codeword[j] != 0) 
                        syndrome ^= gf.GetAlphaTo((gf.GetIndexOf(codeword[j]) + i * j) % n);  

                syndromes[i - 1] = syndrome;
            }

            return syndromes;
        }

        private int[] MultiplyPolynomials(int[] p1, int[] p2)
        {
            int[] res = new int[p1.Length + p2.Length - 1];

            for (int i = 0; i < p1.Length; i++)
                for (int j = 0; j < p2.Length; j++)
                    res[i + j] = gf.Add(res[i + j], gf.Multiply(p1[i], p2[j]));

            return res;
        }

        private int[] BerlekampMassey(int[] syndromes)
        {
            int n = syndromes.Length;
            int[] C = new int[n + 1]; C[0] = 1;
            int[] B = new int[n + 1]; B[0] = 1;

            int L = 0, m = 1, b = 1;

            for (int n_idx = 0; n_idx < n; n_idx++)
            {
                int d = syndromes[n_idx];

                for (int i = 1; i <= L; i++)
                    d = gf.Add(d, gf.Multiply(C[i], syndromes[n_idx - i]));

                if (d != 0)
                {
                    int[] T = (int[])C.Clone();
                    int factor = gf.Multiply(d, gf.Inverse(b));

                    for (int i = 0; i + m < C.Length; i++)
                        C[i + m] = gf.Add(C[i + m], gf.Multiply(factor, B[i]));

                    if (2 * L <= n_idx)
                    {
                        L = n_idx + 1 - L;
                        B = T;
                        b = d;
                        m = 1;
                    }
                    else m++;
                }
                else m++;
            }

            return C.Take(L + 1).ToArray();
        }

        private int[] ChienSearch(int[] sigma)
        {
            List<int> locations = new();

            for (int i = 0; i < n; i++)
            {
                int x = gf.GetAlphaTo((n - i) % n); 

                int sum = sigma[0];

                for (int j = 1; j < sigma.Length; j++)
                    sum = gf.Add(sum, gf.Multiply(sigma[j], Power(x, j)));

                if (sum == 0)
                    locations.Add(i);
            }

            return locations.Distinct().ToArray();
        }

        private byte[] Forney(byte[] data, int[] syndromes, int[] errorLocations)
        {
            byte[] result = (byte[])data.Clone();
            if (errorLocations.Length == 0) return result;

            int[] sigma = BerlekampMassey(syndromes);
            int[] omega = MultiplyPolynomials(syndromes, sigma);
             
            if (omega.Length > 2 * t)
                omega = omega.Take(2 * t).ToArray();
            else if (omega.Length < 2 * t)
                omega = omega.Concat(new int[2 * t - omega.Length]).ToArray();

            foreach (int pos in errorLocations)
            {
                int X = gf.GetAlphaTo(pos);
                int invX = gf.Inverse(X);

                int num = 0;
                for (int i = 0; i < omega.Length; i++)
                    num = gf.Add(num, gf.Multiply(omega[i], Power(invX, i)));

                int den = 0;
                for (int i = 1; i < sigma.Length; i += 2)
                    den = gf.Add(den, gf.Multiply(sigma[i], Power(invX, i - 1)));

                if (den == 0) continue;

                int errorValue = gf.Divide(num, den);
                result[pos] ^= (byte)errorValue;
            }

            return result;
        }

        private int Power(int a, int p)
        {
            int r = 1;
            for (int i = 0; i < p; i++)
                r = gf.Multiply(r, a);
            
            return r;
        }
    }
}