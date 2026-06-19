using Projekat_tema7.Correction;
using Projekat_tema7.Detection;
using Projekat_tema7.Simulation;
using System;

class Program
{
    static void Main()
    {
        // 1. Inicijalizacija
        int m = 4; // GF(2^4)
        int t = 2; // Korekcija 2 greške
        ReedSolomon rs = new ReedSolomon(m, t);
        BurstChannel channel = new BurstChannel { BurstProbability = 1.0 }; // 100% šanse da probamo
        CRC32 crc = new CRC32();

        // 2. Podaci
        byte[] originalData = { 1, 2, 3, 4, 5, 6, 7 }; // tvoja poruka
        byte[] encoded = rs.Encode(originalData);

        // 3. Simulacija šuma
        byte[] noisyData = channel.ApplyNoise(encoded);

        // 4. Provera (ovde će CRC verovatno prijaviti promenu)
        uint originalCrc = crc.Compute(encoded);
        uint noisyCrc = crc.Compute(noisyData);

        Console.WriteLine($"Originalni CRC: {originalCrc:X8}");
        Console.WriteLine($"CRC nakon šuma: {noisyCrc:X8}");
        Console.WriteLine($"Da li je došlo do promene? {originalCrc != noisyCrc}");
    }
}