using System;
using System.Collections.Generic;
using System.Text;

namespace Loaders.plugins.com.slb.charisma
{
    interface ICharismaEntityLoader
    {
        void Open(Builders.IGeoModel model);

        Uri Location
        {
            get;
            set;
        }
    }
}
