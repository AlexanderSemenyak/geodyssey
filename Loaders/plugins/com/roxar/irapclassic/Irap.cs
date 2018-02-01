using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using Utility;
using Builders;

namespace Loaders.plugins.com.roxar.irapclassic
{
    class Irap : Loader
    {
        private static ProductFactory<string, ILoader, Irap> registerThis = new ProductFactory<string, ILoader, Irap>("Irap");

        private static Regex[] uriPatterns = { new Regex(@"irap", RegexOptions.IgnoreCase) };
        private static string[] fileExtensions = { "*" };
        private static string description = "Roxar Irap Classic Grid";

        #region Constructors

        public Irap(Uri location) :
            base(location)
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
        }

        public override void Open(IGeoModel model)
        {
        }

        #endregion
        #endregion
    }
}
