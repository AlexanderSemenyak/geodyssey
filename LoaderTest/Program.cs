using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Loaders;
using Builders;
using Utility;

namespace LoaderTest
{
    class Program
    {
        static int Main(string[] args)
        {
            Uri uri;
            try
            {
                uri = new Uri(args[0]);
            }
            catch (UriFormatException)
            {
                string absPath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + args[0];
                try
                {
                    uri = new Uri(absPath);
                }
                catch (UriFormatException)
                {
                    return 1;
                }
            }
            IGeoModel model = Factory<IGeoModel>.Instance.Create("Test");
            LoaderController.Instance.Open(uri, model);
            Console.ReadKey();
            return 0;
        }
    }
}
