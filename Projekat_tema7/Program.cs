using Projekat_tema7.Correction;
using Projekat_tema7.Detection;
using Projekat_tema7.Simulation;
using System;
using System.Linq;

class Program
{
    static void Main()
    {
        // 1. Inicijalizacija RS-a (m=4, t=2 -> n=15, k=11)
        ReedSolomon rs = new ReedSolomon(4, 2);

        // PAŽNJA: Data mora imati tačno _k = 11 elemenata za m=4, t=2
        byte[] originalData = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };

        // Test 1: Enkodiranje
        byte[] encoded = rs.Encode(originalData);

        // Test 2: Provera sindroma (Mora biti sve nula)
        int[] syndromes = rs.ComputeSyndromes(encoded);
        bool isSyndromeZero = syndromes.All(s => s == 0);

        Console.WriteLine("--- Reed-Solomon Test ---");
        Console.WriteLine($"Originalni podaci: {string.Join(", ", originalData)}");
        Console.WriteLine($"Kodirana reč: {string.Join(", ", encoded)}");
        Console.WriteLine($"Sindromi (treba biti sve 0): {string.Join(", ", syndromes)}");
        Console.WriteLine($"Da li je enkoder validan? {isSyndromeZero} \n \n");

        if (!isSyndromeZero)
        {
            Console.WriteLine("UPOZORENJE: Enkoder ne generiše validne kodne reči!");
            return; 
        }
         
        byte[] myData = { 10, 20, 30, 40, 50, 60, 70, 80 };
        BurstChannel channel = new BurstChannel { BurstProbability = 0.5 };
        CRC32 crc = new CRC32();
        PerformanceAnalyzer analyzer = new PerformanceAnalyzer();

        analyzer.RunStressTest(500, myData, channel, crc);


    }
}