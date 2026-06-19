using System;
using Projekat_tema7.Simulation;
using Projekat_tema7.Detection;

class Program
{
    static void Main()
    {
        byte[] myData = { 10, 20, 30, 40, 50, 60, 70, 80 }; // Test podaci
        BurstChannel channel = new BurstChannel { BurstProbability = 0.5 };
        CRC32 crc = new CRC32();
        PerformanceAnalyzer analyzer = new PerformanceAnalyzer();

        // Pokreni analizu 1000 paketa
        analyzer.RunStressTest(500, myData, channel, crc);
    }
}