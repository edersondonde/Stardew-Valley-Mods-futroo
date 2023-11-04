using System.Collections.Generic;
using SObject = StardewValley.Object;

namespace ShippingBinSummary
{
    internal record DataModel
    {
        public HashSet<int> ForceSellable { get; }

    public DataModel(HashSet<int>? forceSellable)
    {
        this.ForceSellable = forceSellable ?? new();
    }
}
}
