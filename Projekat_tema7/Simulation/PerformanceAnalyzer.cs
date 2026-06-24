using System;
using System.Linq;
using Projekat_tema7.Correction;
using Projekat_tema7.Detection;

namespace Projekat_tema7.Simulation
{
    public class PerformanceAnalyzer
    {
        public void RunSimulation(int totalPackets, byte[] data, BurstChannel channel, CRC32 crc)
        {
            int detectedErr = 0, undetectedErr = 0;

            for (int i = 0; i < totalPackets; i++)
            {
                uint orgCrc = crc.Compute(data); 
                byte[] corrupted = channel.ApplyNoise(data); 
                uint newCrc = crc.Compute(corrupted);

                if (orgCrc != newCrc)
                    detectedErr++;
                else if (!data.SequenceEqual(corrupted))
                    undetectedErr++;
            }

            Console.WriteLine($"   - Detektovane greške: {detectedErr}"); 
            Console.WriteLine($"   - Nedetektovane greške: {undetectedErr}");
        }

        public void RunStressTest(int packetsPerLevel, byte[] data, BurstChannel channel, ReedSolomon rs)
        {
            Console.WriteLine();
            Console.WriteLine(" ------- REED-SOLOMON STRES TEST ------");
            Console.WriteLine(" --------------------------------------");
            Console.WriteLine("  | Burst Verovatnoća | % Uspešnosti |"); 
            Console.WriteLine("  ------------------------------------");

            for (double prob = 0.1; prob <= 1.0; prob += 0.1)
            {
                channel.Probability = prob; 
                int success = 0;

                for (int i = 0; i < packetsPerLevel; i++)
                {
                    byte[] encoded = rs.Encode(data); 
                    byte[] corrupted = channel.ApplyNoise(encoded); 
                    byte[] corrected = rs.Decode(corrupted);

                    if (corrected.Skip(4).SequenceEqual(data)) 
                        success++; 
                } 
                Console.WriteLine($"  | {prob,17:P0} | " + $"{(double)success / packetsPerLevel * 100,11:F2}% |");
            } 
            Console.WriteLine();
        }
    }
}