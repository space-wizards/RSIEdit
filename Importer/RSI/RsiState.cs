using System.Collections.Generic;
using Importer.DMI;

namespace Importer.RSI
{
    public class RsiState
    {
        public RsiState(
            string name,
            DirectionTypes directions = DirectionTypes.None,
            List<List<float>>? delays = null,
            Dictionary<object, object>? flags = null)
        {
            Name = name;
            Directions = directions;
            Delays = delays;
            Flags = flags;
        }

        public string Name { get; set; }

        public DirectionTypes Directions { get; set; }

        public List<List<float>>? Delays { get; set; }

        public Dictionary<object, object>? Flags { get; set; }
    }
}
