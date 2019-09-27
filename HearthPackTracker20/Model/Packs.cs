using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;

namespace Models
{
    [DynamoDBTable("HearthPacks")]
    public class Packs
    {
        [DynamoDBHashKey]
        public string UserId { get; set; }
        public PackMap Pack { get; set; }
    }

    public class PackMap
    {
        public int ClassicCount { get; set; }
        public int WitchwoodCount { get; set; }
        public int KoboldsCount { get; set; }
        public int FrozenThroneCount { get; set; }
        public int GadgetzanCount { get; set; }
        public int GVGCount { get; set; }
        public int OldGodsCount { get; set; }
        public int TGTCount { get; set; }
        public int UnGoroCount { get; set; }
        public int BoomsdayCount { get; set; }
        public int RastakhansCount { get; set; }
        public int RiseOfShadowsCount { get; set; }
        public int SaviorsOfUldumCount { get; set; }
    }

    public static class PackTypeSynonyms
    {
        public static List<string> SaviorsOfUldum = new List<string>
    {
      "Saviors of Uldum",
      "Uldum"
    };

        public static List<string> RiseOfShadows = new List<string>
    {
      "Rise of Shadows",
      "Shadows",
      "Rise"
    };

        public static List<string> RastakhansRumble = new List<string>
    {
      "Rastakhan's Rumble",
      "Rastakhan",
      "Rumble"
    };

        public static List<string> Boomsday = new List<string>
      {
        "Boomsday",
        "The Boomsday Project",
        "Boomsday Project"
      };


        public static List<string> Kobolds = new List<string>
        {
            "kobolds",
            "k a c",
            "kac",
            "catacombs",
            "kobolds",
            "kobolds and catacombs"
        };

        public static List<string> Witchwood = new List<string>
        {
            "witchwood",
            "witch wood",
            "which wood",
            "witch",
            "wood"
        };

        public static List<string> UnGoro = new List<string>
        {
            "un'goro",
            "journey",
            "journey to ungoro",
            "ungoro",
            "journey to un'goro"
        };

        public static List<string> GVG = new List<string>
        {
            "gvg",
            "goblins versus gnomes",
            "goblins and gnomes"
        };

        public static List<string> Gadgetzan = new List<string>
        {
            "gadgetzan",
            "mean streets",
            "gadget",
            "mean streets of gadgetzan"
        };

        public static List<string> FrozenThrone = new List<string>
        {
            "frozen throne",
            "k o t f t",
            "knights",
            "k o f t",
            "knights of the frozen throne"
        };

        public static List<string> Classic = new List<string>
        {
            "classic",
            "expert"
        };

        public static List<string> OldGods = new List<string>
        {
            "old gods",
            "whispers of the old gods"
        };

        public static List<string> TGT = new List<string>
        {
            "tgt",
            "the grand tournament"
        };
    }
}
