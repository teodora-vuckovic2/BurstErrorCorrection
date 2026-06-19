using Projekat_tema7;
using Projekat_tema7.Detection;
using System;

class Program
{
    static void Main()
    {
        CRC32 crcModule = new CRC32();
        byte[] testData = System.Text.Encoding.UTF8.GetBytes("Ovo je test poruka");

        uint checksum = crcModule.Compute(testData);

        Console.WriteLine($"CRC-32 checksum: {checksum:X8}");

        // 2. Tvoj novi test za Galois Field
        Console.WriteLine("\n--- Testiranje GaloisField-a ---");
        GaloisField gf = new GaloisField(4, 0x13);

        int a = 2;
        int b = 2;
        int rez = gf.Multiply(a, b);
        Console.WriteLine($"Množenje: {a} * {b} = {rez} (Očekivano: 4)");

        int inv = gf.Inverse(2);
        Console.WriteLine($"Inverz: Inverz od 2 je {inv} (Očekivano: 8)");

        Console.WriteLine($"Sabiranje: 5 ^ 3 = {gf.Add(5, 3)} (Očekivano: 6)");
    }
}