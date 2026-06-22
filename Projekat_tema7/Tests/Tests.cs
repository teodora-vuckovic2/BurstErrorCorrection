using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Projekat_tema7.Correction;
using Projekat_tema7.Simulation; 

namespace Projekat_tema7.Tests
{
    public class Tests
    {
        public static void RunBasicCorrectionTest(ReedSolomon rs, byte[] data)
        {
            Console.WriteLine("=== 1. OSNOVNI TEST ===");

            byte[] encoded = rs.Encode(data);
            byte[] corrupted = (byte[])encoded.Clone();

            corrupted[0] ^= 5;
            corrupted[5] ^= 10;

            byte[] corrected = rs.Decode(corrupted);

            bool success = data.SequenceEqual(corrected.Skip(4));

            Console.WriteLine($"Original: {string.Join(",", data)}");
            Console.WriteLine($"Decoded : {string.Join(",", corrected.Skip(4))}");
            Console.WriteLine($"Status  : {(success ? "USPEH" : "NEUSPEH")}");
            Console.WriteLine("--------------------------------------\n");
        }

        public static void RunEdgeCaseTests(ReedSolomon rs, byte[] data)
        {
            Console.WriteLine("=== 2. EDGE CASES ===");

            TestRS(rs, data, new int[] { }, new int[] { });
            TestRS(rs, data, new int[] { 4 }, new int[] { 7 });
            TestRS(rs, data, new int[] { 2, 8 }, new int[] { 3, 12 });
            TestRS(rs, data, new int[] { 0, 1 }, new int[] { 5, 2 });

            Console.WriteLine("--------------------------------------\n");
        }

        public static void RunStressTestSequence(ReedSolomon rs)
        {
            Console.WriteLine("=== 3. STRESS TEST (RS + BURST) ===");

            BurstChannel channel = new BurstChannel { BurstProbability = 0.5 };

            byte[] data = { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110 };

            int success = 0;

            for (int i = 0; i < 500; i++)
            {
                byte[] encoded = rs.Encode(data);
                byte[] noisy = channel.ApplyNoise(encoded);
                byte[] decoded = rs.Decode(noisy);

                if (data.SequenceEqual(decoded.Skip(4)))
                    success++;
            }

            Console.WriteLine($"Success rate: {(double)success / 500 * 100:F2}%");
            Console.WriteLine("--------------------------------------\n");
        }

        public static void RunBurstLengthAnalysis(ReedSolomon rs)
        {
            Console.WriteLine("=== 4. BURST LENGTH ANALIZA ===");

            byte[] data = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };

            for (int burst = 1; burst <= 5; burst++)
            {
                int success = 0;
                BurstChannel channel = new BurstChannel { BurstProbability = 1.0, MaxBurstLength = burst };

                for (int t = 0; t < 300; t++)
                {
                    byte[] encoded = rs.Encode(data);
                    byte[] noisy = channel.ApplyNoise(encoded);
                    byte[] decoded = rs.Decode(noisy);

                    if (data.SequenceEqual(decoded.Skip(4)))
                        success++;
                }

                Console.WriteLine($"Burst {burst} -> {(double)success / 300 * 100:F2}%");
            }

            Console.WriteLine();
        }

        public static void RunCapabilityTest(ReedSolomon rs)
        {
            Console.WriteLine("=== 5. LIMIT TEST ===");

            byte[] data = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };

            for (int errors = 0; errors <= 4; errors++)
            {
                int success = 0;

                for (int t = 0; t < 200; t++)
                {
                    byte[] encoded = rs.Encode(data);
                    byte[] corrupted = (byte[])encoded.Clone();

                    for (int i = 0; i < errors; i++)
                        corrupted[i] ^= 9;

                    byte[] decoded = rs.Decode(corrupted);

                    if (data.SequenceEqual(decoded.Skip(4)))
                        success++;
                }

                Console.WriteLine($"{errors} errors -> {(double)success / 200 * 100:F2}%");
            }

            Console.WriteLine();
        }

        public static void TestRS(ReedSolomon rs, byte[] data, int[] idx, int[] val)
        {
            byte[] encoded = rs.Encode(data);
            byte[] corrupted = (byte[])encoded.Clone();

            for (int i = 0; i < idx.Length; i++)
                corrupted[idx[i]] ^= (byte)val[i];

            byte[] decoded = rs.Decode(corrupted);

            bool ok = data.SequenceEqual(decoded.Skip(4));

            Console.WriteLine($"Test {idx.Length} errors [{string.Join(",", idx)}] -> {(ok ? "USPEH" : "NEUSPEH")}");
        }
    }
}
