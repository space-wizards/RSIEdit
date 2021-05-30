using System.Collections.Generic;

namespace Editor.Models.RSI
{
    public static class RsiExtensions
    {
        public static void OrderStates(this RsiItem item)
        {
            item.States.OrderStates();
        }

        public static void OrderStates(this List<RsiState> states)
        {
            states.Sort((a, b) => a.Name.CompareTo(b));
        }
    }
}
