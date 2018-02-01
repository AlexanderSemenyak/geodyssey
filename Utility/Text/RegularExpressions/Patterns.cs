using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Utility.Text.RegularExpressions
{
    public static class Patterns
    {
        #region Fields
        private static readonly Regex cFloatPattern = new Regex(@"([+-]?)(?=\d|\.\d)\d*(\.\d*)?([Ee]([+-]?\d+))?");
        private static readonly Regex whitespacePattern = new Regex(@"\s+");
        #endregion

        #region Properties
        public static Regex CFloat
        {
            get { return cFloatPattern; }
        }

        public static Regex Whitespace
        {
            get { return whitespacePattern; }
        }
        #endregion

    }
}
