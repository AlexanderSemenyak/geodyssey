using System;
using System.Collections.Generic;
using System.Text;

using Utility;

namespace Builders
{
    class ConsoleGridBuild : IGrid2DBuilder, IFactoryProduct
    {
        #region IGrid2DBuilder Members

        double IGrid2DBuilder.Orientation
        {
            set
            {
                Console.WriteLine("Orientation = {0}", value);
            }
        }

        double IGrid2DBuilder.XOrigin
        {
            set
            {
                Console.WriteLine("XOrigin = {0}", value);
            }
        }

        double IGrid2DBuilder.YOrigin
        {
            set
            {
                Console.WriteLine("YOrigin = {0}", value);
            }
        }

        double IGrid2DBuilder.IInc
        {
            set
            {
                Console.WriteLine("IInc = {0}", value); ;
            }
        }

        double IGrid2DBuilder.JInc
        {
            set
            {
                Console.WriteLine("JInc = {0}", value); ;
            }
        }

        int IGrid2DBuilder.INum
        {
            set
            {
                Console.WriteLine("INum = {0}", value); ;
            }
        }

        int IGrid2DBuilder.JNum
        {
            set
            {
                Console.WriteLine("JNum = {0}", value); ;
            }
        }

        double IGrid2DBuilder.this[int i, int j]
        {
            set
            {
                Console.WriteLine("Z[{0}, {1}] = {2}", i, j, value); ;
            }
        }

        #endregion

        #region IBuilder Members

        public void Build()
        {
            Console.WriteLine("Build");
        }

        #endregion

        #region IFactoryProduct Members

        public object GetFactoryKey()
        {
            return "Console";
        }

        #endregion
    }
}
