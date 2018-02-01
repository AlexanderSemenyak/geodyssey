using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Geodyssey
{
    public class ReportLogger
    {
        private static ReportLogger instance = new ReportLogger();

        private XmlWriter writer = null;

        #region Construction

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static ReportLogger()
        {
        }

        public ReportLogger()
        {
        }

        public static ReportLogger Instance
        {
            get {
                Debug.Assert(instance != null);
                return instance;
            }
        }

        public void Create(string filename)
        {
            writer = new XmlTextWriter(filename, Encoding.UTF8);
        }

        public XmlWriter Writer
        {
            get {
                Debug.Assert(writer != null);
                return writer;
            }
        }

        public void Close()
        {
            Debug.Assert(writer != null);
            writer.Close();
        }

        #endregion
    }
}
