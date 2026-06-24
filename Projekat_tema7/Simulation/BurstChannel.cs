using System;

namespace Projekat_tema7.Simulation
{
    public class BurstChannel
    {
        private Random rand = new Random();

        public double Probability { get; set; } = 0.1;
        public int MaxLength { get; set; } = 5;

        public byte[] ApplyNoise(byte[] data)
        {
            byte[] res = (byte[])data.Clone();

            if (rand.NextDouble() < Probability)
            {
                int start = rand.Next(data.Length);
                int length = rand.Next(1, MaxLength + 1);

                for (int i = 0; i < length; i++)
                {
                    int pos = start + i;

                    if (pos >= res.Length)
                        break;

                    byte temp = (byte)rand.Next(1, 256);
                    res[pos] ^= temp; 
                }
            }  
            return res;
        } 
    }
}