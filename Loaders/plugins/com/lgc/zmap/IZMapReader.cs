using System;
using System.Collections.Generic;
using System.Text;

using Builders;

namespace Loaders.plugins.com.lgc.zmap
{
    public interface IZMapReader
    {
        void Read(ILineReader lineReader, IGeoModel model);
    }
}
