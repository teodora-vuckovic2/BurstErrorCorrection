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
            int[] poly = { 1 }; 
            for (int i = 1; i <= 2 * _t; i++)
            {
                int[] factor = { _gf.GetAlphaTo(i), 1 }; 
                poly = MultiplyPolynomials(poly, factor);
            }
            return poly;
        }

        public byte[] Encode(byte[] data)
        {
            byte[] codeword = new byte[_n];
            int[] parity = new int[2 * _t];

            // U profesorovom kodu: data[0] je najznačajniji koeficijent
            for (int i = 0; i < data.Length; i++)
            {
                int feedback = _gf.Add(data[i], parity[2 * _t - 1]);
                for (int j = 2 * _t - 1; j > 0; j--)
                    parity[j] = _gf.Add(parity[j - 1], _gf.Multiply(feedback, _genPoly[j]));
                parity[0] = _gf.Multiply(feedback, _genPoly[0]);
            }

            // Kodna reč: [Data] [Parity]
            Array.Copy(data, 0, codeword, 0, data.Length);
            for (int i = 0; i < 2 * _t; i++)
                codeword[data.Length + i] = (byte)parity[2 * _t - 1 - i];

            return codeword;
        }

        public int[] ComputeSyndromes(byte[] codeword)
        {
            int[] syndromes = new int[2 * _t];
            for (int i = 1; i <= 2 * _t; i++)
            {
                int syndrome = 0;
                int alpha_i = _gf.GetAlphaTo(i);
                int alpha_pow = 1; // Ovo će biti (alpha^i)^j

                for (int j = 0; j < _n; j++)
                {
                    // Profesorov metod: s = sum(c[j] * (alpha^i)^j)
                    if (codeword[j] != 0)
                        syndrome = _gf.Add(syndrome, _gf.Multiply(codeword[j], alpha_pow));

                    alpha_pow = _gf.Multiply(alpha_pow, alpha_i);
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
