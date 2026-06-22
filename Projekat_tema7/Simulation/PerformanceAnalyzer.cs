using System;
using System.Linq;
using Projekat_tema7.Correction;
using Projekat_tema7.Detection;

namespace Projekat_tema7.Simulation
{
    public class PerformanceAnalyzer
    {
        public void RunSimulation(
            int totalPackets,
            byte[] data,
            BurstChannel channel,
            CRC32 crc)
        {
            int detectedErrors = 0;
            int undetectedErrors = 0;

            for (int i = 0; i < totalPackets; i++)
            {
                uint originalCrc = crc.Compute(data);

                byte[] corrupted =
                    channel.ApplyNoise(data);

                uint newCrc =
                    crc.Compute(corrupted);

                if (originalCrc != newCrc)
                    detectedErrors++;
                else if (!data.SequenceEqual(corrupted))
                    undetectedErrors++;
            }

            Console.WriteLine(
                $"Detektovane greške: {detectedErrors}");

            Console.WriteLine(
                $"Nedetektovane greške: {undetectedErrors}");
        }

        public void RunStressTest(
            int packetsPerLevel,
            byte[] data,
            BurstChannel channel,
            ReedSolomon rs)
        {
            Console.WriteLine();
            Console.WriteLine(
                "--- RS STRES TEST ---");

            Console.WriteLine(
                "| Burst Prob. | Success Rate |");

            Console.WriteLine(
                "--------------------------------");

            for (double prob = 0.1;
                 prob <= 1.0;
                 prob += 0.1)
            {
                channel.BurstProbability = prob;

                int success = 0;

                for (int i = 0;
                     i < packetsPerLevel;
                     i++)
                {
                    byte[] encoded =
                        rs.Encode(data);

                    byte[] corrupted =
                        channel.ApplyNoise(encoded);

                    byte[] corrected =
                        rs.Decode(corrupted);

                    if (corrected
                        .Skip(4)
                        .SequenceEqual(data))
                    {
                        success++;
                    }
                }

                Console.WriteLine(
                    $"| {prob,10:P0} | " +
                    $"{(double)success / packetsPerLevel * 100,10:F2}% |");
            }

            Console.WriteLine();
        }
    }
}