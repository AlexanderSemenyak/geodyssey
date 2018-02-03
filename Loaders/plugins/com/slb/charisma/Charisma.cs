using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using Utility;
using Builders;

namespace Loaders.plugins.com.slb.charisma
{
    class Charisma : Loader, IFactoryProduct
    {
        #region Fields
        private static readonly Regex[] uriPatterns = { new Regex(@"\.dat$", RegexOptions.IgnoreCase),
                                               new Regex(@"charisma", RegexOptions.IgnoreCase),
                                               new Regex(@"\.chr$", RegexOptions.IgnoreCase),
                                               new Regex(@"\.cma$", RegexOptions.IgnoreCase) };
        private static readonly string[] fileExtensions = { "dat", "*" };
        private static readonly string description = "Charisma";

        /// <summary>
        /// Pattern to skip Charsima comment, Charsima history lines, and blank lines
        /// </summary>
        internal static readonly Regex commentBlanksPattern = new Regex(@"^(\s*|\s*#.*)");
        // Surfaces have a header: #foo1-1 foo-1 ...
        internal static readonly Regex surfaceRegex = new Regex(@"^\s*#\s*(\w*-\d+\s*)+");
        private static string entityType = null;
        #endregion

        public Charisma()
        {
        }

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
            string type = DetermineType();
            if (Factory<ICharismaEntityLoader>.Instance.ContainsKey(type))
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
                    throw new OpenException("Unable to determine Charisma entity type");
                }
            }
            if (Factory<ICharismaEntityLoader>.Instance.ContainsKey(entityType))
            {
                ICharismaEntityLoader entityLoader = Factory<ICharismaEntityLoader>.Instance.Create(entityType);
            }
            else
            {
                StringBuilder message = new StringBuilder();
                message.AppendFormat("No loader for Charisma entity {0}", entityType);
                throw new OpenException(message.ToString());
            }
        }
        #endregion
        #endregion

        /// <summary>
        /// Determines the type of the contents of the file.
        /// Either "Fault" or "Surface"
        /// </summary>
        /// <param name="lineReader"></param>
        /// <returns>The type of the entity within the Charisma file</returns>
        private string DetermineType()
        {
            int searchLimit = 50; // Number of lines to test
            // TODO: Identify Charisma fault files
            using (LineReader lineReader = new LineReader(Location.LocalPath))
            {
                do
                {
                    string line = lineReader.ReadLine().Trim();
                    if (surfaceRegex.IsMatch(line))
                    {
                        return "Surface";
                    }

                }
                while (lineReader.PhysicalLineNumber < searchLimit);
            }
            return "Unknown";
        }

        #region IFactoryProduct Members

        public object GetFactoryKey()
        {
            return "Charisma";
        }

        #endregion
    }
}
