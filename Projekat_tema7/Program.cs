using Projekat_tema7.Correction;
using Projekat_tema7.Detection;
using Projekat_tema7.Simulation;
using System;
using System.Linq;

class Program
{
    static void Main()
    {
        ReedSolomon rs = new ReedSolomon(4, 2);
        byte[] originalData = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };

        Console.Title = "Reed-Solomon Test Suite";

        RunBasicCorrectionTest(rs, originalData);
        RunEdgeCaseTests(rs, originalData);
        RunStressTestSequence(rs);

        Console.WriteLine("\nSvi testovi završeni. Pritisni bilo koji taster za izlaz.");
        Console.ReadKey();
    }
    static void RunBasicCorrectionTest(ReedSolomon rs, byte[] originalData)
    {
        Console.WriteLine("=== 1. OSNOVNI TEST DEKODERA ===");
        byte[] encoded = rs.Encode(originalData);
        byte[] corrupted = (byte[])encoded.Clone();

        corrupted[0] ^= 5;
        corrupted[5] ^= 10;

        byte[] corrected = rs.Decode(corrupted);

        bool success = originalData.SequenceEqual(corrected.Skip(4)); 

        Console.WriteLine($"Originalni podaci : {string.Join(", ", originalData)}");
        Console.WriteLine($"Podaci iz dekodera: {string.Join(", ", corrected.Skip(4))}");
        Console.WriteLine($"Status ispravke   : {(success ? "USPEH" : "NEUSPEH")}");
        Console.WriteLine("--------------------------------------\n");
    }

    static void RunEdgeCaseTests(ReedSolomon rs, byte[] data)
    {
        Console.WriteLine("=== 2. GRANIČNI SLUČAJEVI (Edge Cases) ===");
        TestRS(rs, data, new int[] { }, new int[] { });             
        TestRS(rs, data, new int[] { 4 }, new int[] { 7 });         
        TestRS(rs, data, new int[] { 2, 8 }, new int[] { 3, 12 });  
        TestRS(rs, data, new int[] { 0, 1 }, new int[] { 5, 2 });  
        Console.WriteLine("--------------------------------------\n");
    }

    static void RunStressTestSequence(ReedSolomon rs)
    {
        Console.WriteLine("=== 3. STRES TEST KANALA ===");
        byte[] myData = { 10, 20, 30, 40, 50, 60, 70, 80 };
        var analyzer = new PerformanceAnalyzer();
        analyzer.RunStressTest(500, myData, new BurstChannel { BurstProbability = 0.5 }, new CRC32());
    }

    static void TestRS(ReedSolomon rs, byte[] data, int[] errorIndices, int[] errorValues)
    {
        byte[] encoded = rs.Encode(data);
        byte[] corrupted = (byte[])encoded.Clone();

        for (int i = 0; i < errorIndices.Length; i++)
            corrupted[errorIndices[i]] ^= (byte)errorValues[i];

        byte[] corrected = rs.Decode(corrupted);

        bool success = true;
        for (int i = 0; i < data.Length; i++)
            if (corrected[i + 4] != data[i]) success = false;

        Console.WriteLine($"Test ({errorIndices.Length} grešaka) na [{string.Join(",", errorIndices)}] : {(success ? "USPEH" : "NEUSPEH")}");
    }
}