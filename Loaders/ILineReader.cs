using System;
namespace Loaders
{
    public interface ILineReader : IDisposable
    {
        int LogicalLineNumber { get; }
        int PhysicalLineNumber { get; }
        string PeekLine();
        string ReadLine();
        string UnreadLine();
    }
}
