using System.Numerics;
using Belias.Actions;
using Belias.Windows.NodesSystem.Types;

namespace Belias.Windows.NodesSystem;

public static class NodeFactoryExtensions
{
    /// <summary>
    /// Creates a node for a GCD action.
    /// </summary>
    public static GCDActionNode CreateGCDNode(IRSAction action, Vector2 position)
    {
        return new GCDActionNode(GetNextNodeId(), action)
        {
            Position = position
        };
    }

    /// <summary>
    /// Creates a node for an oGCD action.
    /// </summary>
    public static OGCDActionNode CreateOGCDNode(IRSAction action, Vector2 position)
    {
        return new OGCDActionNode(GetNextNodeId(), action)
        {
            Position = position
        };
    }

    /// <summary>
    /// Creates a node for a buff action.
    /// </summary>
    public static BuffActionNode CreateBuffNode(IRSAction action, Vector2 position)
    {
        return new BuffActionNode(GetNextNodeId(), action)
        {
            Position = position
        };
    }

    /// <summary>
    /// Creates a node for a combo action.
    /// </summary>
    public static ComboActionNode CreateComboNode(IRSAction action, Vector2 position)
    {
        return new ComboActionNode(GetNextNodeId(), action)
        {
            Position = position
        };
    }    /// <summary>
    /// Creates a node for a positional action.
    /// </summary>
    public static PositionalActionNode CreatePositionalNode(IRSAction action, Vector2 position)
    {
        return new PositionalActionNode(GetNextNodeId(), action)
        {
            Position = position
        };
    }    // Keep track of node IDs
    private static int NextNodeId = 1;
    private static int GetNextNodeId() => NextNodeId++;
}
