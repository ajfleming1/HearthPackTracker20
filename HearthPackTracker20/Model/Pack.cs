using System;
using System.Collections.Generic;
using System.Text;
using Amazon.DynamoDBv2.DataModel;

namespace HearthPackTracker20.Model
{
    [DynamoDBTable("HearthPacks")]
    public class Pack
    {
        [DynamoDBHashKey]
        public string UserId { get; set; }
        public PacksMap PacksMap { get; set; }
    }

    public class PacksMap
    {
        public int ClassicCount { get; set; }
        public int KoboldsCount { get; set; }
        public int GadgetzanCount { get; set; }
        public int GVGCount { get; set; }
        public int OldGodsCount { get; set; }
        public int TGTCount { get; set; }
        public int UnGoroCount { get; set; }
    }
}
