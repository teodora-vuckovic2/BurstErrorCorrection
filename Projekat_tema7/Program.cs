using Projekat_tema7.Correction;
using Projekat_tema7.Simulation;
using Projekat_tema7.Tests;
using System;
using System.Linq;

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

        Tests.RunBasicCorrectionTest(rs, originalData);
        Tests.RunEdgeCaseTests(rs, originalData);
        Tests.RunStressTestSequence(rs);
        Tests.RunBurstLengthAnalysis(rs);
        Tests.RunCapabilityTest(rs);

        Console.WriteLine("\nSvi testovi završeni.");
        Console.ReadKey();
    }
}