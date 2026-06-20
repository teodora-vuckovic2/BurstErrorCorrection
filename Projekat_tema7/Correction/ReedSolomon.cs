using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekat_tema7.Correction
{
    internal class ReedSolomon : IErrorCorrection
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
                        gg[j] =
                            gg[j - 1] ^
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
                throw new ArgumentException(
                    $"RS({_n},{_k}) zahteva {_k} simbola.");

            int paritySymbols = _n - _k;

            int[] bb = new int[paritySymbols];

            for (int i = 0; i < paritySymbols; i++)
                bb[i] = 0;

            for (int i = _k - 1; i >= 0; i--)
            {
                int feedback =
                    _gf.GetIndexOf(
                        data[i] ^ bb[paritySymbols - 1]);

                if (feedback != -1)
                {
                    for (int j = paritySymbols - 1; j > 0; j--)
                    {
                        if (_genPoly[j] != -1)
                        {
                            bb[j] =
                                bb[j - 1] ^
                                _gf.GetAlphaTo(
                                    (_genPoly[j] + feedback) % _n);
                        }
                        else
                        {
                            bb[j] = bb[j - 1];
                        }
                    }

                    bb[0] =
                        _gf.GetAlphaTo(
                            (_genPoly[0] + feedback) % _n);
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

        public int[] ComputeSyndromes(byte[] codeword)
        {
            int[] syndromes = new int[2 * _t];

            for (int i = 1; i <= 2 * _t; i++)
            {
                int syndrome = 0;

                for (int j = 0; j < _n; j++)
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

        public byte[] Decode(byte[] data)
        {
            throw new NotImplementedException("Dekoder ćemo implementirati nakon što potvrdimo da Enkoder radi!");
        }

        private int[] MultiplyPolynomials(int[] p1, int[] p2)
        {
            int[] res = new int[p1.Length + p2.Length - 1];
            for (int i = 0; i < p1.Length; i++)
                for (int j = 0; j < p2.Length; j++)
                    res[i + j] = _gf.Add(res[i + j], _gf.Multiply(p1[i], p2[j]));
            return res;
        }
    }
}
