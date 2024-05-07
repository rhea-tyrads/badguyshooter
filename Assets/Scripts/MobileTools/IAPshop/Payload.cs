using System;
using System.Collections.Generic;

namespace MobileTools.IAPshop
{
    [Serializable]
    public class Payload
    {
        public string json;
        public string signature;
        public List<SkuDetails> skuDetails;
        public PayloadData payloadData;
    }
}