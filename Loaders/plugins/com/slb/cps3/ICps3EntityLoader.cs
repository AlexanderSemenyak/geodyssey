using System;
namespace Loaders.plugins.com.slb.cps3
{
    // TODO: Promote this to an IEntityLoader, factor it out
    //       of ILoader, share it between the different plugins
    //       and create an Abstract Base Class to contain the URI
    interface ICps3EntityLoader
    {
        void Open(Builders.IGeoModel model);

        Uri Location
        {
            get;
            set;
        }
    }
}
