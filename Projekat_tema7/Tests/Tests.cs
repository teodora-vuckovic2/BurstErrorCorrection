using System;
using System.Linq;
using System.Collections.Generic;
using Projekat_tema7.Correction;
using Projekat_tema7.Detection;
using Projekat_tema7.Simulation;

namespace Projekat_tema7.Tests
{
    public class Tests
    {
        public static void BasicCorrection(ReedSolomon rs, byte[] data)
        { 
            Console.WriteLine(" ---------- 1. OSNOVNI TEST -----------"); 
            CRC32 crc = new CRC32(); 
            uint originalCRC = crc.Compute(data);

            byte[] encoded = rs.Encode(data);
            byte[] corrupted = (byte[])encoded.Clone();

            corrupted[0] ^= 5;
            corrupted[5] ^= 10;

            byte[] corrected = rs.Decode(corrupted); 
            byte[] decodedData = corrected.Skip(4).ToArray(); 
            uint decodedCRC = crc.Compute(decodedData);

            bool success = data.SequenceEqual(decodedData);

            Console.WriteLine($"\n   Original: {string.Join(",", data)}");
            Console.WriteLine($"   Decoded : {string.Join(",", decodedData)}");
            Console.WriteLine($"\n   Original CRC: {originalCRC}");
            Console.WriteLine($"   Decoded CRC : {decodedCRC}");
            Console.WriteLine($"   CRC Match   : {(originalCRC == decodedCRC ? "DA" : "NE")}");
            Console.WriteLine($"\n   Status      : {(success ? "USPEH" : "NEUSPEH")}");
            Console.WriteLine("\n --------------------------------------\n");
        }

        public static void EdgeCases(ReedSolomon rs, byte[] data)
        { 
            Console.WriteLine(" -------- 2. IVIČNI SLUČAJEVI ---------\n");

            TestRS(rs, data, new int[] { }, new int[] { });
            TestRS(rs, data, new int[] { 4 }, new int[] { 7 });
            TestRS(rs, data, new int[] { 2, 8 }, new int[] { 3, 12 });
            TestRS(rs, data, new int[] { 0, 1 }, new int[] { 5, 2 });

            Console.WriteLine("\n --------------------------------------\n");
        }

        public static void StressTest(ReedSolomon rs)
        {  
            Console.WriteLine(" ----------- 3. STRES TEST ------------");

            BurstChannel channel = new BurstChannel { Probability = 0.5 }; 
            byte[] data = { 10,20,30,40,50,60,70,80,90,100,110 }; 
            int success = 0;

            for (int i = 0; i < 500; i++)
            {
                byte[] encoded = rs.Encode(data);
                byte[] noisy = channel.ApplyNoise(encoded);
                byte[] decoded = rs.Decode(noisy);

                if (data.SequenceEqual(decoded.Skip(4)))
                    success++;
            }

            Console.WriteLine($"\n   Procenat uspeha: {(double)success / 500 * 100:F2}%\n"); 
            Console.WriteLine(" --------------------------------------\n");
        }

        public static void BurstLengthAnalysis(ReedSolomon rs)
        {  
            Console.WriteLine(" ------ 4. BURST LENGTH ANALIZA -------\n");

            byte[] data = { 1,2,3,4,5,6,7,8,9,10,11 };

            for (int burst = 1; burst <= 5; burst++)
            {
                BurstChannel channel = new BurstChannel { Probability = 1.0, MaxLength = burst};
                int success = 0; 

                for (int t = 0; t < 300; t++)
                {
                    byte[] encoded = rs.Encode(data);
                    byte[] noisy = channel.ApplyNoise(encoded);
                    byte[] decoded = rs.Decode(noisy);

                    if (data.SequenceEqual(decoded.Skip(4)))
                        success++;
                } 
                Console.WriteLine($"   Burst {burst} -> {(double)success / 300 * 100:F2}%");
            }
            Console.WriteLine("\n --------------------------------------\n"); 
        }

        public static void CapabilityTest(ReedSolomon rs)
        {
            Console.WriteLine(" ---------- 5. TEST GRANICA -----------\n");

            byte[] data = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
            Random rand = new Random();

            for (int errors = 0; errors <= 4; errors++)
            {
                int success = 0;

                for (int t = 0; t < 500; t++)
                {
                    byte[] encoded = rs.Encode(data);
                    byte[] corrupted = (byte[])encoded.Clone();

                    HashSet<int> used = new HashSet<int>();

                    for (int i = 0; i < errors; i++)
                    {
                        int pos;

                        do
                        {
                            pos = rand.Next(encoded.Length);
                        }
                        while (!used.Add(pos));

                        corrupted[pos] ^= 9;
                    }

                    byte[] decoded = rs.Decode(corrupted);

                    if (data.SequenceEqual(decoded.Skip(4)))
                        success++;
                }

                Console.WriteLine($"   {errors} errors -> {(double)success / 500 * 100:F2}%");
            }


            Console.WriteLine("\n --------------------------------------\n");
        }

        public static void BurstVsRandomComparison(ReedSolomon rs)
        {
            Console.WriteLine(" ---- 6. BURST VS RANDOM POREĐENJE ----\n");

            byte[] data = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };

            int totalTests = 1000;

            int burstSuccess = 0;
            int randomSuccess = 0;

            Random rand = new Random();

            int paritySize = rs.Encode(data).Length - data.Length;
             
            BurstChannel burstChannel = new BurstChannel
            {
                Probability = 0.7,       
                MaxLength = 3
            };

            for (int i = 0; i < totalTests; i++)
            {
                byte[] encoded = rs.Encode(data);
                byte[] corrupted = burstChannel.ApplyNoise(encoded); 
                byte[] decoded = rs.Decode(corrupted);

                if (data.SequenceEqual(decoded.Skip(paritySize)))
                    burstSuccess++;
            }
             
            for (int i = 0; i < totalTests; i++)
            {
                byte[] encoded = rs.Encode(data);
                byte[] corrupted = (byte[])encoded.Clone(); 
                int errorCount = rand.Next(1, 4); 

                for (int e = 0; e < errorCount; e++)
                {
                    int pos = rand.Next(corrupted.Length); 
                    corrupted[pos] ^= (byte)(1 << rand.Next(0, 7));
                }

                byte[] decoded = rs.Decode(corrupted);

                if (data.SequenceEqual(decoded.Skip(paritySize)))
                    randomSuccess++;
            }

            Console.WriteLine($"    Burst greške : {(double)burstSuccess / totalTests * 100:F2}%");
            Console.WriteLine($"    Random greške: {(double)randomSuccess / totalTests * 100:F2}%\n");
        }

        public static void TestRS(ReedSolomon rs, byte[] data, int[] idx, int[] val)
        {
            byte[] encoded = rs.Encode(data);
            byte[] corrupted = (byte[])encoded.Clone();

            for (int i = 0; i < idx.Length; i++)
                corrupted[idx[i]] ^= (byte)val[i];

            byte[] decoded = rs.Decode(corrupted); 
            bool ok = data.SequenceEqual(decoded.Skip(4));

            Console.WriteLine($"   Test {idx.Length} errors [{string.Join(",", idx)}] -> {(ok ? "USPEH" : "NEUSPEH")}");
        }
    }
}