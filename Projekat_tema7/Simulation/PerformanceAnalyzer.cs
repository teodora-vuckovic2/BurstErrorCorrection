using System;
using System.Linq;
using Projekat_tema7.Detection;

namespace Projekat_tema7.Simulation
{
    public class PerformanceAnalyzer
    {
        public void RunSimulation(int totalPackets, byte[] data, BurstChannel channel, CRC32 crc)
        {
            int detectedErrors = 0;
            int undetectedErrors = 0; 

            for (int i = 0; i < totalPackets; i++)
            {
                uint originalCrc = crc.Compute(data);
                byte[] corrupted = channel.ApplyNoise(data);
                uint newCrc = crc.Compute(corrupted);
                 
                if (originalCrc != newCrc)
                    detectedErrors++;
                 
                else if (!data.SequenceEqual(corrupted))
                    undetectedErrors++;
            }

            Console.WriteLine($"--- REZULTATI ANALIZE ---");
            Console.WriteLine($"Ukupno paketa: {totalPackets}");
            Console.WriteLine($"Detektovane greške: {detectedErrors}");
            Console.WriteLine($"Nedetektovane greške: {undetectedErrors}");
            Console.WriteLine($"FER (Frame Error Rate): {(double)detectedErrors / totalPackets * 100}%");
        }

        public void RunStressTest(int packetsPerLevel, byte[] data, BurstChannel channel, CRC32 crc)
        {
            Console.WriteLine("--- STRES TEST KANALA ---");
            Console.WriteLine("| Prob. Šuma | FER (%) |");
            Console.WriteLine("--------------------------");
             
            for (double prob = 0.1; prob <= 1.05; prob += 0.1)
            {
                channel.BurstProbability = prob;
                int detectedErrors = 0;

                for (int i = 0; i < packetsPerLevel; i++)
                {
                    uint originalCrc = crc.Compute(data);
                    byte[] corrupted = channel.ApplyNoise(data);

                    if (originalCrc != crc.Compute(corrupted))
                        detectedErrors++;
                }

                double fer = (double)detectedErrors / packetsPerLevel * 100;
                Console.WriteLine($"| {prob,10:P0} | {fer,7:F1}% |");
            }
        }
    }
}