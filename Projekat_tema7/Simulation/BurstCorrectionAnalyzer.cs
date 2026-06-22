using System;
using System.Linq;
using Projekat_tema7.Correction;

namespace Projekat_tema7.Simulation
{
    public class BurstCorrectionAnalyzer
    {
        public void Analyze(ReedSolomon rs, BurstChannel channel, byte[] data, int iterations)
        {
            int successfulCorrections = 0;
            int failedCorrections = 0;

            for (int i = 0; i < iterations; i++)
            {
                byte[] encoded = rs.Encode(data);

                byte[] corrupted = channel.ApplyNoise(encoded);

                byte[] corrected = rs.Decode(corrupted);

                bool success =
                    corrected
                    .Skip(4)
                    .SequenceEqual(data);

                if (success)
                    successfulCorrections++;
                else
                    failedCorrections++;
            }

            Console.WriteLine();
            Console.WriteLine("===== ANALIZA RS KOREKCIJE =====");
            Console.WriteLine($"Ukupno testova      : {iterations}");
            Console.WriteLine($"Uspešno ispravljeno : {successfulCorrections}");
            Console.WriteLine($"Neuspešno           : {failedCorrections}");
            Console.WriteLine($"Stopa uspeha        : {(double)successfulCorrections / iterations * 100:F2}%");
            Console.WriteLine();
        }
    }
}