using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using Builders;

namespace Loaders
{
    public abstract class Loader : ILoader
    {
        private Uri location;

        protected Loader()
        {
        }

        #region ILoader Members

        public Uri Location
        {
            get { return location; }
            set { location = value; }
        }

        #region Properties

        public abstract Regex[] UriPatterns
        {
            get;
        }

        public abstract string[] FileExtensions
        {
            get;
        }

        public abstract string Description
        {
            get;
        }

        #endregion

        #region Methods

        // TODO: Make Recognition and Open public non-virtual
        //       and make their virtual counterparts protected
        public abstract Recognition Recognize();
        public abstract void Open(IGeoModel model);

        #endregion

        #endregion
    }
}
