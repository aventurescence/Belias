using System.Numerics;
using Belias.Windows.NodesSystem.Types;

namespace Belias.Windows.NodesSystem;

/// <summary>
/// Represents an input connection point on a node.
/// </summary>
public class NodeInput
{
    /// <summary>
    /// The node that owns this input.
    /// </summary>
    public VisualNode Node { get; }

    /// <summary>
    /// Display name of the input.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The type of value this input accepts.
    /// </summary>
    public NodePinType Type { get; }

    /// <summary>
    /// The output that is connected to this input, if any.
    /// </summary>
    public NodeOutput? ConnectedOutput { get; set; }

    public void ConnectTo(NodeOutput output)
    {
        if (Type != output.Type) return; // Type check
        var oldOutput = ConnectedOutput;
        ConnectedOutput = output;
    }

    public NodeInput(VisualNode node, string name, NodePinType type)
    {
        Node = node;
        Name = name;
        Type = type;
    }
}

/// <summary>
/// Represents an output connection point on a node.
/// </summary>
public class NodeOutput
{
    /// <summary>
    /// The node that owns this output.
    /// </summary>
    public VisualNode Node { get; }

    /// <summary>
    /// Display name of the output.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The type of value this output provides.
    /// </summary>
    public NodePinType Type { get; }

    public NodeOutput(VisualNode node, string name, NodePinType type)
    {
        Node = node;
        Name = name;
        Type = type;
    }
}
