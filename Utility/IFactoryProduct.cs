using System;
using System.Collections.Generic;
using System.Text;

namespace Utility
{
    // IFactoryProduct Interface, provides a means
    // for the class factory to retrieve a classes
    // mapping key
    public interface IFactoryProduct
    {
        object GetFactoryKey();
    }
}

