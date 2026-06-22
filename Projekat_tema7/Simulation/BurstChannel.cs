using System;

namespace Projekat_tema7.Simulation
{
    public class BurstChannel
    {
        private Random _rng = new Random();

        public double BurstProbability { get; set; } = 0.1;
        public int MaxBurstLength { get; set; } = 5;

        public byte[] ApplyNoise(byte[] data)
        {
            byte[] noisy = (byte[])data.Clone();

            if (_rng.NextDouble() < BurstProbability)
            {
                int start = _rng.Next(data.Length);
                int length = _rng.Next(1, MaxBurstLength + 1);

                for (int i = 0; i < length; i++)
                {
                    int pos = start + i;

                    if (pos >= noisy.Length)
                        break;

                    noisy[pos] ^= (byte)_rng.Next(1, 256);
                }
            }

            return noisy;
        }

        public byte[] InjectSpecificError(byte[] data, int position, byte errorValue)
        {
            byte[] corrupted = (byte[])data.Clone();

            if (position >= 0 && position < corrupted.Length)
                corrupted[position] ^= errorValue;

            return corrupted;
        }
    }
}