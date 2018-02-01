using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using Utility;
using Builders;

namespace Loaders.plugins.com.lgc.zmap
{
    class ZMapPlus : Loader, IFactoryProduct
    {
        #region Fields
        private static readonly Regex[] uriPatterns = { new Regex(@"\.dat$", RegexOptions.IgnoreCase),
                                               new Regex(@"zmap", RegexOptions.IgnoreCase),
                                               new Regex(@"zyc", RegexOptions.IgnoreCase),
                                               new Regex(@"zycor", RegexOptions.IgnoreCase)};
        private static readonly string[] fileExtensions = { "*" };
        private const string description = "ZMap Plus";
        private static readonly Regex commentHistoryBlanksPattern = new Regex(@"^\s*[!\+]");
        private static readonly Dictionary<string, string> typeLoaders = new Dictionary<string, string>();

        #endregion

        #region Constructors

        static ZMapPlus()
        {
            // Keys are found in the ZMap file. Values are the keys for IZMapReaders
            // returned from the Factory<IZMapReader>
            typeLoaders.Add("DATA", "Data");
            typeLoaders.Add("GRID", "Surface");
            typeLoaders.Add("FALT", "Fault");
            typeLoaders.Add("FAULT", "Fault");
            typeLoaders.Add("VERT", "Fault");
        }

        public ZMapPlus()
        {
        }

        #endregion

        #region ILoader Members
        #region Properties

        public override Regex[] UriPatterns
        {
            get { return uriPatterns; }
        }

        public override string[] FileExtensions
        {
            get { return fileExtensions; }
        }

        public override string Description
        {
            get { return description; }
        }

        #endregion

        #region Methods

        public override Recognition Recognize()
        {
            using (SkippingLineReader lineReader = new SkippingLineReader(Location.LocalPath, commentHistoryBlanksPattern))
            {
                string type = DetermineType(lineReader);
                if (typeLoaders.ContainsKey(type))
                {
                    return Recognition.Yes;
                }
            }
            return Recognition.No;
        }

        public override void Open(IGeoModel model)
        {
            using (SkippingLineReader lineReader = new SkippingLineReader(Location.LocalPath, commentHistoryBlanksPattern))
            {
                string type = DetermineType(lineReader);
                if (!typeLoaders.ContainsKey(type))
                {
                    StringBuilder message = new StringBuilder();
                    message.AppendFormat("Unrecognized ZMap object type code '{0}' at line {1}", type, lineReader.PhysicalLineNumber);
                    throw new OpenException(message.ToString(), Location);
                }
                string key = typeLoaders[type];
                if (!Factory<IZMapReader>.Instance.ContainsKey(key))
                {
                    StringBuilder message = new StringBuilder();
                    message.AppendFormat("ZMap object type code '{0}' recognized at line {1}, but no loader has been implemented.", type, lineReader.PhysicalLineNumber);
                    throw new OpenException(message.ToString(), Location);
                }
                lineReader.UnreadLine();
                IZMapReader zMapReader = Factory<IZMapReader>.Instance.Create(key);
                zMapReader.Read(lineReader, model);
            }
        }

        #endregion
        #endregion

        #region Methods

        private static string DetermineType(ILineReader lineReader)
        {

            string line = lineReader.ReadLine();
            if (line.StartsWith("@"))
            {
                string[] fields = line.Split(',');
                string type = fields[1].Trim();
                return type;
            }
            return "Unknown";
        }

        #endregion

        #region IFactoryProduct Members

        public object GetFactoryKey()
        {
            return "ZMapPlus";
        }

        #endregion
    }
}
