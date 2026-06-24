using Projekat_tema7.Correction;
using Projekat_tema7.Simulation;
using Projekat_tema7.Detection;
using Projekat_tema7.Tests;
using System;

class Program
{
    static void Main()
    {
        ReedSolomon rs = new ReedSolomon(4, 2);

        byte[] originalData = { 1,2,3,4,5,6,7,8,9,10,11};

        Console.Title = "Tehnike za ispravljanje burst grešaka";

        Console.WriteLine(" ======================================");
        Console.WriteLine("         SKUP OSNOVNIH TESTOVA");
        Console.WriteLine(" ======================================\n");

        Tests.BasicCorrection(rs, originalData);
        Tests.EdgeCases(rs, originalData);
        Tests.StressTest(rs);
        Tests.BurstLengthAnalysis(rs); 
        Tests.CapabilityTest(rs);
        Tests.BurstVsRandomComparison(rs);

        Console.WriteLine("\n ======================================");
        Console.WriteLine("           ANALIZA PERFORMANSI");
        Console.WriteLine(" ======================================\n");

        PerformanceAnalyzer analyzer = new PerformanceAnalyzer();

        BurstChannel channel = new BurstChannel
        {
            Probability = 0.3,
            MaxLength = 5
        };

        CRC32 crc = new CRC32();
         
        Console.WriteLine(" ------ SIMULACIJA CRC DETEKCIJE -------\n");
        analyzer.RunSimulation(totalPackets: 500, data: originalData, channel: channel, crc: crc);


        Console.WriteLine("\n --------------------------------------");

        analyzer.RunStressTest(packetsPerLevel: 200, data: originalData, channel: channel, rs: rs);

        Console.WriteLine("\n =====================================");
        Console.WriteLine("                  KRAJ");
        Console.WriteLine(" =====================================");
         
        Console.ReadKey();
    }
}