using System;
using System.Collections.Generic;
using System.Text;
using Wintellect.PowerCollections;

namespace Loaders
{
    public abstract class ChoiceList : IChoiceList
    {
        #region Fields
        private List<Pair<object, string>> items;
        #endregion

        #region Construction
        protected ChoiceList(List<Pair<object, string>> items)
        {
            this.items = items;
        }
        #endregion

        #region Properties
        public List<Pair<object, string>> Items
        {
            get { return items; }
        }
        #endregion

        #region Methods
        public abstract object Show();
        #endregion
    }
}
