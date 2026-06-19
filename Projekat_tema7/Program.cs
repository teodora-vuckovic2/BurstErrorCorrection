using System;
using Projekat_tema7.Detection;

class Program
{
    static void Main()
    {
        CRC32 crcModule = new CRC32();
        byte[] testData = System.Text.Encoding.UTF8.GetBytes("Ovo je test poruka");

        uint checksum = crcModule.Compute(testData);

        Console.WriteLine($"CRC-32 checksum: {checksum:X8}"); 
    }
}