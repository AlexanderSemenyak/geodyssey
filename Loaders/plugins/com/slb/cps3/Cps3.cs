using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using Utility;
using Builders;

namespace Loaders.plugins.com.slb.cps3
{
    class Cps3 : Loader, IFactoryProduct
    {
        private static Regex[] uriPatterns = { new Regex(@"\.dat$", RegexOptions.IgnoreCase),
                                               new Regex(@"\.cps$", RegexOptions.IgnoreCase),
                                               new Regex(@"\.cps3$", RegexOptions.IgnoreCase) };
        private static string[] fileExtensions = { "dat", "cps", "cps3", "grd" };
        private static string description = "CPS3";

        /// <summary>
        /// Pattern to skip Cps3 comments, and blank lines
        /// </summary>
        public readonly static Regex commentBlanksPattern = new Regex(@"^(\s*|\s*!.*)$");

        private string entityType = null;

        #region Constructors

        public Cps3()
        {
        }

        #endregion

        #region ILoader members
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
            // Pre-condition - the URI is valid and the resource exists
            entityType = DetermineType();
            if (Factory<ICps3EntityLoader>.Instance.ContainsKey(entityType))
            {
                return Recognition.Yes;
            }
            return Recognition.No;
        }

        public override void Open(IGeoModel model)
        {
            if (entityType == null)
            {
                // TODO: Translate exceptions into OpenExceptions
                entityType = DetermineType();
                if (entityType == null)
                {
                    StringBuilder message = new StringBuilder();
                    throw new OpenException("Unable to determine CPS3 entity type");
                }
            }
            if (Factory<ICps3EntityLoader>.Instance.ContainsKey(entityType))
            {
                ICps3EntityLoader entityLoader = Factory<ICps3EntityLoader>.Instance.Create(entityType);
                entityLoader.Location = Location;
                entityLoader.Open(model);
            }
            else
            {
                StringBuilder message = new StringBuilder();
                message.AppendFormat("No loader for CPS3 entity {0}", entityType);
                throw new OpenException(message.ToString());
            }
        }
        #endregion
        #endregion

        private string DetermineType()
        {
            int searchLimit = 50; // Number of lines to test
            // TODO: Assemble this list of patterns by querying the Cps3 Entity Loaders
            Dictionary<string, Regex> recognitionPatterns = new Dictionary<string, Regex>();
            recognitionPatterns.Add("Surface", new Regex(@"^(FSASCI|!\s*NCOL)"));
            recognitionPatterns.Add("Fault", new Regex(@"^(FFASCI|->)"));
            using (SkippingLineReader lineReader = new SkippingLineReader(Location.LocalPath, commentBlanksPattern))
            {
                do
                {
                    string line = lineReader.ReadLine().Trim();
                    foreach(string type in recognitionPatterns.Keys)
                    {
                        if (recognitionPatterns[type].IsMatch(line))
                        {
                            return type;
                        }
                    }
                }
                while (lineReader.PhysicalLineNumber < searchLimit);
            }
            return "Unknown";
        }

        #region IFactoryProduct Members

        public object GetFactoryKey()
        {
            return "Cps3";
        }

        #endregion
    }
}
