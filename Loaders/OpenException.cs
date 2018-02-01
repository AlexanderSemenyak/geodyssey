using System;
using System.Collections.Generic;
using System.Text;

namespace Loaders
{
    public class OpenException : Exception
    {
        private Uri uri;

        public OpenException(string message)
            :
            base(message)
        {
        }

        public OpenException(string message, Uri uri) :
            this(message, uri, null)
        {
        }

        public OpenException(string message, Uri uri, Exception innerException)
            :
            this(message, innerException)
        {
            this.uri = uri;
        }

        public OpenException(string message, Exception innerException)
            :
            base(message, innerException)
        {
        }
    }
}
