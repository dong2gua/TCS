using Prism.Events;

namespace ThorCyte.Infrastructure.Events
{
    /// <summary>
    /// Publish on macro start Region/Tile Process
    /// </summary>
    public class MacroStart : PubSubEvent<MacroStartEventArgs>
    {
    }
}
