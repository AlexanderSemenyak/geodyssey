using System;
namespace Loaders
{
    interface IChoiceList
    {
        System.Collections.Generic.List<Wintellect.PowerCollections.Pair<object, string>> Items { get; }
        object Show();
    }
}
