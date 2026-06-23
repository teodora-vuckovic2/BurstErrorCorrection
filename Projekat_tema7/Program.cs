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

        byte[] originalData =
        {
            1,2,3,4,5,6,7,8,9,10,11
        };

        Console.Title = "Burst Error Correction Analysis";

        Console.WriteLine("======================================");
        Console.WriteLine("         BASIC TEST SUITE");
        Console.WriteLine("======================================\n");

        Tests.RunBasicCorrectionTest(rs, originalData);
        Tests.RunEdgeCaseTests(rs, originalData);
        Tests.RunStressTestSequence(rs);
        Tests.RunBurstLengthAnalysis(rs);
        Tests.RunCapabilityTest(rs);

        Console.WriteLine("\n======================================");
        Console.WriteLine("        ADVANCED ANALYSIS");
        Console.WriteLine("======================================\n");

        PerformanceAnalyzer analyzer = new PerformanceAnalyzer();

        BurstChannel channel = new BurstChannel
        {
            BurstProbability = 0.3,
            MaxBurstLength = 5
        };

        CRC32 crc = new CRC32();

        Console.WriteLine(">> CRC DETECTION SIMULATION");
        analyzer.RunSimulation(
            totalPackets: 500,
            data: originalData,
            channel: channel,
            crc: crc
        );

        Console.WriteLine("\n>> REED-SOLOMON STRESS ANALYSIS");
        analyzer.RunStressTest(
            packetsPerLevel: 200,
            data: originalData,
            channel: channel,
            rs: rs
        );

        Console.WriteLine("\n======================================");
        Console.WriteLine("              FINISH");
        Console.WriteLine("======================================");

        Console.WriteLine("\nSvi testovi i analize su završeni.");
        Console.ReadKey();
    }
}