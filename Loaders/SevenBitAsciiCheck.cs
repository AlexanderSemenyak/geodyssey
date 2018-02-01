using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Loaders
{
    class SevenBitAsciiCheck
    {
        const byte zero = 0x00;
        const byte tab  = 0x09;
        const byte lineFeed = 0x0a;
        const byte carriageReturn = 0x0d;
        const byte firstControl = 0x01;
        const byte lastControl = 0x1f;
        const byte delete = 0x7f;

        public float TextProbability(Uri uri, int bytes)
        {
            int binaryCount = 0;
            using (FileStream stream = new FileStream(uri.LocalPath, FileMode.Open))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    
                    for (int i = 0 ; i < bytes ; ++i)
                    {
                        byte b = reader.ReadByte();
                        if (b == zero)
                        {
                            ++binaryCount;
                        }
                        else if (b >= firstControl && b <= lastControl)
                        {
                            if (b != tab && b != lineFeed && b != carriageReturn)
                            {
                                 ++binaryCount;
                            }
                        }
                        else if (b == delete)
                        {
                            ++binaryCount;
                        }
                        else if (b == 0x81 || b == 0x8d || b == 0x8f || b == 0x90 || b == 0x9d)
                        {
                            // Windows-1252 control characters
                            ++binaryCount;
                        }
                    }
                }
            }
            return binaryCount / bytes;
        }
    }
}
