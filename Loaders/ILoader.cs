using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using Builders;

namespace Loaders
{
    public interface ILoader
    {
        Uri Location
        {
            get;
            set;
        }

        Regex[] UriPatterns
        {
            get;
        }

        string[] FileExtensions
        {
            get;
        }

        string Description
        {
            get;
        }

        Recognition Recognize();

        void Open(IGeoModel model);
    }
}
