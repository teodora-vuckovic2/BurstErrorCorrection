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
            byte[] noisyData = (byte[])data.Clone();
             
            if (_rng.NextDouble() < BurstProbability)
            {
                int burstStart = _rng.Next(0, noisyData.Length);
                int burstLength = _rng.Next(1, MaxBurstLength + 1);

                for (int i = 0; i < burstLength; i++)
                {
                    int pos = (burstStart + i) % noisyData.Length; 
                    noisyData[pos] ^= (byte)_rng.Next(1, 256);
                }
            }
            return noisyData;
        }
    }
}