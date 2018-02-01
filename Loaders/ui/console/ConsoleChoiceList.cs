using System;
using System.Collections.Generic;
using System.Text;
using Wintellect.PowerCollections;

using Utility;

namespace Loaders.ui.console
{
    class ConsoleChoiceList : ChoiceList, IFactoryProduct
    {
        // Needed for IFactoryProduct
        // TODO: Replace IFactoryProduct with an Attribute
        public ConsoleChoiceList() :
            base(null)
        {
        }

        public ConsoleChoiceList(List<Pair<object, string>> items) :
            base(items)
        {
        }

        public override object Show()
        {
            // Prints the menu to the console, and waits for a user response
            int count = 1;
            foreach (Pair<object, string> item in Items)
            {
                Console.WriteLine("{0}.\t{1}.", count, item.Second);
                ++count;
            }
            Console.WriteLine("{0}.\t{1}.", "C.", "- Cancel -");
            do
            {
                Console.Write("Choice? ");
                string option = Console.ReadLine().Trim();
                if (option == "C")
                {
                    return null;
                }
                int menu;
                bool ok = Int32.TryParse(option, out menu);
                if (ok)
                {
                    return Items[menu - 1].First;
                }
            }
            while (true);
        }

        #region IFactoryProduct Members
        public object GetFactoryKey()
        {
            return "Console";
        }
        #endregion
    }
}
