using System; 
using System.Linq; 

namespace Projekat_tema7.Detection
{
    public class CRC32
    {
        private uint[] table;
        private const uint poly = 0xEDB88320;

        public CRC32()
        {
            createTable();
        }
          
        private void createTable()
        {
            table = new uint[256]; 

            for (uint i = 0; i < 256; i++)
            {
                uint crc = i;
                for (int j = 8; j > 0; j--)
                {
                    if ((crc & 1) == 1)
                        crc = (crc >> 1) ^ poly;
                    else
                        crc >>= 1;
                }
                table[i] = crc;
            }
        }
         
        public uint Compute(byte[] data)
        {
            uint crc = 0xFFFFFFFF;
            foreach (byte b in data) 
                crc = (crc >> 8) ^ table[(crc ^ b) & 0xFF]; 

            return crc ^ 0xFFFFFFFF;
        } 
    } 
}