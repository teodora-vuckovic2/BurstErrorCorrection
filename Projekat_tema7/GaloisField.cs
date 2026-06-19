using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekat_tema7
{
    internal class GaloisField
    {
        private readonly int[] alphaTo;
        private readonly int[] indexOf;
        private readonly int fieldSize; // n = 2^m - 1
        private readonly int m;         // stepen polja

        public GaloisField(int m = 4, int primitivePolynomial = 0x13)
        {
            this.m = m;
            this.fieldSize = (1 << m) - 1;
            this.alphaTo = new int[fieldSize + 1];
            this.indexOf = new int[fieldSize + 1];

            GenerateTables(primitivePolynomial);
        }

        private void GenerateTables(int poly)
        {
            int mask = 1;
            alphaTo[m] = 0;
            for (int i = 0; i < m; i++)
            {
                alphaTo[i] = mask;
                indexOf[alphaTo[i]] = i;
                if ((poly & (1 << i)) != 0)
                    alphaTo[m] ^= mask;
                mask <<= 1;
            }

            mask >>= 1;
            for (int i = m + 1; i <= fieldSize; i++)
            {
                if (alphaTo[i - 1] >= mask)
                    alphaTo[i] = alphaTo[m] ^ ((alphaTo[i - 1] ^ mask) << 1);
                else
                    alphaTo[i] = alphaTo[i - 1] << 1;
                indexOf[alphaTo[i]] = i % fieldSize;
            }
            indexOf[0] = -1;
        }

        // Sabiranje u GF(2^m) je jednostavno XOR
        public int Add(int a, int b) => a ^ b;

        // Množenje pomoću tabela (index_of[a] + index_of[b])
        public int Multiply(int a, int b)
        {
            if (a == 0 || b == 0) return 0;
            return alphaTo[(indexOf[a] + indexOf[b]) % fieldSize];
        }

        // Deljenje (množenje inverznim elementom)
        public int Divide(int a, int b)
        {
            if (a == 0) return 0;
            if (b == 0) throw new DivideByZeroException();
            return alphaTo[(indexOf[a] - indexOf[b] + fieldSize) % fieldSize];
        }

        // Inverzija elementa
        public int Inverse(int a) => alphaTo[fieldSize - indexOf[a]];

        // Pristup tabelama za napredne operacije (potrebno za Berlekamp-Massey)
        public int GetAlphaTo(int i) => alphaTo[i % fieldSize];
        public int GetIndexOf(int a) => (a == 0) ? -1 : indexOf[a];
    }
}
