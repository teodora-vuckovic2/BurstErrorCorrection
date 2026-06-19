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
        private readonly int _t;    // Broj grešaka koje se koriguju
        private readonly int _n;    // Dužina bloka (n = 2^m - 1)
        private readonly int _k;    // Dužina poruke (k = n - 2*t)
        private readonly int[] _genPoly; // Generatorski polinom

        public ReedSolomon(int m, int t)
        {
            _gf = new GaloisField(m);
            _t = t;
            _n = (1 << m) - 1;
            _k = _n - (2 * t);
            _genPoly = GenerateGeneratorPolynomial();
        }

        // 1. Generisanje generatorskog polinoma (g(x) = (x-a^1)(x-a^2)...)
        private int[] GenerateGeneratorPolynomial()
        {
            int[] poly = { 1 }; // Početni polinom 1
            for (int i = 1; i <= 2 * _t; i++)
            {
                int[] factor = { _gf.GetAlphaTo(i), 1 }; // (x + alpha^i)
                poly = MultiplyPolynomials(poly, factor);
            }
            return poly;
        }

        // 2. Encode: Sistematsko kodiranje
        public byte[] Encode(byte[] data)
        {
            // Kodna reč je: [Poruka] + [Paritet]
            byte[] codeword = new byte[_n];
            int[] parity = new int[2 * _t];

            // Feedback shift register logika za deljenje polinoma
            for (int i = 0; i < _k; i++)
            {
                int feedback = _gf.Add(data[i], parity[_t * 2 - 1]);
                for (int j = 2 * _t - 1; j > 0; j--)
                {
                    parity[j] = _gf.Add(parity[j - 1], _gf.Multiply(feedback, _genPoly[j]));
                }
                parity[0] = _gf.Multiply(feedback, _genPoly[0]);
            }

            // Spajanje poruke i pariteta
            Array.Copy(data, 0, codeword, 0, _k);
            for (int i = 0; i < 2 * _t; i++)
                codeword[_k + i] = (byte)parity[i];

            return codeword;
        }

        public byte[] Decode(byte[] data)
        {
            throw new NotImplementedException("Dekoder ćemo implementirati nakon što potvrdimo da Enkoder radi!");
        }

        // Pomoćna metoda za množenje polinoma
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
