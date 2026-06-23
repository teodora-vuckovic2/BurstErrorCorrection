using System; 
using System.Linq; 

namespace Projekat_tema7.Detection
{
    public class CRC32
    {
        private uint[] crcTable;
        private const uint Polynomial = 0xEDB88320;

        public CRC32()
        {
            GenerateTable();
        }
         
        private void GenerateTable()
        {
            crcTable = new uint[256];
            for (uint i = 0; i < 256; i++)
            {
                uint crc = i;
                for (int j = 8; j > 0; j--)
                {
                    if ((crc & 1) == 1)
                        crc = (crc >> 1) ^ Polynomial;
                    else
                        crc >>= 1;
                }
                crcTable[i] = crc;
            }
        }
         
        public uint Compute(byte[] data)
        {
            uint crc = 0xFFFFFFFF;
            foreach (byte b in data) 
                crc = (crc >> 8) ^ crcTable[(crc ^ b) & 0xFF]; 

            return crc ^ 0xFFFFFFFF;
        }

        private static bool VerifyWithCRC(byte[] originalData, byte[] decodedCodeword)
        {
            CRC32 crc = new CRC32();

            uint originalCRC = crc.Compute(originalData);

            byte[] decodedData =
                decodedCodeword.Skip(4).ToArray();

            uint decodedCRC =
                crc.Compute(decodedData);

            return originalCRC == decodedCRC;
        }
    }

}