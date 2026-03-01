using System.Collections.Generic;

namespace PotentialOverlay.Models
{
    public class PotentialItem
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "Ignore"; 
    }

    public class PotentialData : Dictionary<string, List<PotentialItem>> { }
}