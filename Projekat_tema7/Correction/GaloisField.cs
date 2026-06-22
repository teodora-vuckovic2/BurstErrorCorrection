using System;

namespace Projekat_tema7.Correction
{
    internal class GaloisField
    {
        private readonly int[] alphaTo;
        private readonly int[] indexOf;
        private readonly int fieldSize;
        private readonly int m;

        public GaloisField(int m = 4, int primitivePolynomial = 0x13)
        {
            this.m = m;
            fieldSize = (1 << m) - 1;

            alphaTo = new int[fieldSize + 1];
            indexOf = new int[fieldSize + 1];

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

            indexOf[alphaTo[m]] = m;

            mask >>= 1;

            for (int i = m + 1; i < fieldSize; i++)
            {
                if (alphaTo[i - 1] >= mask)
                    alphaTo[i] = alphaTo[m] ^ ((alphaTo[i - 1] ^ mask) << 1);
                else
                    alphaTo[i] = alphaTo[i - 1] << 1;

                indexOf[alphaTo[i]] = i;
            }

            indexOf[0] = -1;
            alphaTo[fieldSize] = 1;
        }

        public int Add(int a, int b) => a ^ b;

        public int Multiply(int a, int b)
        {
            if (a == 0 || b == 0) return 0;

            int ia = GetIndexOf(a);
            int ib = GetIndexOf(b);

            if (ia < 0 || ib < 0) return 0;

            return alphaTo[(ia + ib) % fieldSize];
        }

        public int Divide(int a, int b)
        {
            if (a == 0) return 0;
            if (b == 0) throw new DivideByZeroException();

            int ia = GetIndexOf(a);
            int ib = GetIndexOf(b);

            return alphaTo[(ia - ib + fieldSize) % fieldSize];
        }

        public int Inverse(int a)
        {
            int ia = GetIndexOf(a);
            return ia < 0 ? 0 : alphaTo[fieldSize - ia];
        }

        public int GetAlphaTo(int i)
        {
            if (fieldSize <= 0) return 0;
            if (i < 0) i = (i % fieldSize + fieldSize) % fieldSize;
            return alphaTo[i];
        }

        public int GetIndexOf(int a)
        {
            if (a <= 0) return -1;
            if (a >= indexOf.Length) return -1;
            return indexOf[a];
        }
    }
}