using Prism.Events;

namespace ThorCyte.Infrastructure.Events
{
    /// <summary>
    /// Publish on macro start Region/Tile Process
    /// </summary>
    public class MacroStart : PubSubEvent<MacroStartEventArgs>
    {
    }

    public class MacroStartEventArgs
    {
        public int RegionId;
        public int TileId;

        public MacroStartEventArgs()
        {
            RegionId = 0;
            TileId = 0;
        }
    }
}
