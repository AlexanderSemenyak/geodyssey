using System;
using System.Collections.Generic;
using System.Diagnostics;
using Wintellect.PowerCollections;

namespace FaultMapper
{
    /// <summary>
    /// Contains the geometric representation of the fault network
    /// </summary>
    public interface INode
    {
        int Degree { get; }
    }
}
