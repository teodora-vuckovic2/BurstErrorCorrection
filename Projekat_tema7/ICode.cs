using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekat_tema7
{
    public interface IErrorCorrection
    {
        byte[] Encode(byte[] data);
        byte[] Decode(byte[] data); 
    }
}
