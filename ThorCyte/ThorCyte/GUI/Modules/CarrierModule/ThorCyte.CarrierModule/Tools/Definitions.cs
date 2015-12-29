
namespace ThorCyte.CarrierModule.Tools
{
    /// <summary>
    /// Defines drawing tool
    /// </summary>
    public enum ToolType
    {
        None,
        Pointer,
        Select,
        Drag,
        Max
    };

    /// <summary>
    /// Context menu command types
    /// </summary>
    internal enum ContextMenuCommand
    {
        SelectAll,
        UnselectAll,
        Delete,
        DeleteAll,
        MoveToFront,
        MoveToBack,
        Undo,
        Redo,
        SerProperties
    };
}
