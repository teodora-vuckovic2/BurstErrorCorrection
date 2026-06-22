using System;
using System.Linq;
using System.Collections.Generic;

namespace Projekat_tema7.Correction
{
    public class ReedSolomon : IErrorCorrection
    {
        private readonly GaloisField _gf;
        private readonly int _t;
        private readonly int _n;
        private readonly int _k;
        private readonly int[] _genPoly;

        public ReedSolomon(int m, int t)
        {
            _gf = new GaloisField(m);
            _t = t;
            _n = (1 << m) - 1;
            _k = _n - (2 * t);

            _genPoly = GenerateGeneratorPolynomial();
        }

        private int[] GenerateGeneratorPolynomial()
        {
            int paritySymbols = _n - _k;

            int[] gg = new int[paritySymbols + 1];

            gg[0] = 2;
            gg[1] = 1;

            for (int i = 2; i <= paritySymbols; i++)
            {
                gg[i] = 1;

                for (int j = i - 1; j > 0; j--)
                {
                    if (gg[j] != 0)
                    {
                        gg[j] = gg[j - 1] ^
                            _gf.GetAlphaTo(
                                (_gf.GetIndexOf(gg[j]) + i) % _n);
                    }
                    else
                    {
                        gg[j] = gg[j - 1];
                    }
                }

                gg[0] =
                    _gf.GetAlphaTo(
                        (_gf.GetIndexOf(gg[0]) + i) % _n);
            }

            for (int i = 0; i <= paritySymbols; i++)
                gg[i] = _gf.GetIndexOf(gg[i]);

            return gg;
        }

        public byte[] Encode(byte[] data)
        {
            if (data.Length != _k)
                throw new ArgumentException($"RS({_n},{_k}) zahteva {_k} simbola.");

            int paritySymbols = _n - _k;
            int[] bb = new int[paritySymbols];

            for (int i = 0; i < paritySymbols; i++)
                bb[i] = 0;

            for (int i = _k - 1; i >= 0; i--)
            {
                int feedback = _gf.GetIndexOf(data[i] ^ bb[paritySymbols - 1]);

                if (feedback != -1)
                {
                    for (int j = paritySymbols - 1; j > 0; j--)
                    {
                        if (_genPoly[j] != -1)
                        {
                            bb[j] = bb[j - 1] ^
                                _gf.GetAlphaTo((_genPoly[j] + feedback) % _n);
                        }
                        else
                        {
                            bb[j] = bb[j - 1];
                        }
                    }

                    bb[0] = _gf.GetAlphaTo((_genPoly[0] + feedback) % _n);
                }
                else
                {
                    for (int j = paritySymbols - 1; j > 0; j--)
                        bb[j] = bb[j - 1];

                    bb[0] = 0;
                }
            }

            byte[] codeword = new byte[_n];

            for (int i = 0; i < paritySymbols; i++)
                codeword[i] = (byte)bb[i];

            for (int i = 0; i < _k; i++)
                codeword[i + paritySymbols] = data[i];

            return codeword;
        }

        public byte[] Decode(byte[] data)
        {
            int[] syndromes = ComputeSyndromes(data);

            if (syndromes.All(s => s == 0))
                return data;

            int[] sigma = BerlekampMassey(syndromes);
            int[] errorLocations = ChienSearch(sigma);

            return Forney(data, syndromes, errorLocations);
        }

        public int[] ComputeSyndromes(byte[] codeword)
        {
            int[] syndromes = new int[2 * _t];

            for (int i = 1; i <= 2 * _t; i++)
            {
                int syndrome = 0;

                for (int j = 0; j < codeword.Length; j++)
                {
                    if (codeword[j] != 0)
                    {
                        syndrome ^= _gf.GetAlphaTo(
                            (_gf.GetIndexOf(codeword[j]) + i * j) % _n);
                    }
                }

                syndromes[i - 1] = syndrome;
            }

            return syndromes;
        }

        private int[] MultiplyPolynomials(int[] p1, int[] p2)
        {
            int[] res = new int[p1.Length + p2.Length - 1];

            for (int i = 0; i < p1.Length; i++)
                for (int j = 0; j < p2.Length; j++)
                    res[i + j] = _gf.Add(res[i + j], _gf.Multiply(p1[i], p2[j]));

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
                    d = _gf.Add(d, _gf.Multiply(C[i], syndromes[n_idx - i]));

                if (d != 0)
                {
                    int[] T = (int[])C.Clone();
                    int factor = _gf.Multiply(d, _gf.Inverse(b));

                    for (int i = 0; i + m < C.Length; i++)
                        C[i + m] = _gf.Add(C[i + m], _gf.Multiply(factor, B[i]));

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

            for (int i = 0; i < _n; i++)
            {
                int x = _gf.GetAlphaTo(i);

                int sum = sigma[sigma.Length - 1];

                for (int j = sigma.Length - 2; j >= 0; j--)
                    sum = _gf.Add(_gf.Multiply(sum, x), sigma[j]);

                if (sum == 0)
                    locations.Add((_n - i) % _n);
            }

            return locations.Distinct().ToArray();
        }

        private byte[] Forney(byte[] data, int[] syndromes, int[] errorLocations)
        {
            byte[] result = (byte[])data.Clone();
            if (errorLocations.Length == 0) return result;

            int[] sigma = BerlekampMassey(syndromes);
            int[] omega = MultiplyPolynomials(syndromes, sigma);
            omega = omega.Take(2 * _t).ToArray();

            foreach (int pos in errorLocations)
            {
                int X = _gf.GetAlphaTo(pos);
                int invX = _gf.Inverse(X);

                int num = 0;
                for (int i = 0; i < omega.Length; i++)
                    num = _gf.Add(num, _gf.Multiply(omega[i], Power(invX, i)));

                int den = 0;
                for (int i = 1; i < sigma.Length; i += 2)
                    den = _gf.Add(den, _gf.Multiply(sigma[i], Power(invX, i - 1)));

                if (den == 0) continue;

                int errorValue = _gf.Divide(num, den);
                result[pos] ^= (byte)errorValue;
            }

            return result;
        }

        private int Power(int a, int p)
        {
            int r = 1;
            for (int i = 0; i < p; i++)
                r = _gf.Multiply(r, a);
            return r;
        }
    }
}