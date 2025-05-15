using System.Numerics;
using System.Collections.Generic;

namespace Belias.Windows.NodesSystem.Types;

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

    /// <summary>
    /// Connect this input to an output.
    /// </summary>
    /// <param name="output">The output to connect to.</param>
    public void ConnectTo(NodeOutput output)
    {
        if (Type != output.Type) return; // Type check
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

    /// <summary>
    /// The inputs that are connected to this output.
    /// </summary>
    public List<NodeInput> ConnectedInputs { get; } = new();

    /// <summary>
    /// Connect this output to an input.
    /// </summary>
    /// <param name="input">The input to connect to.</param>
    public void ConnectTo(NodeInput input)
    {
        if (Type != input.Type) return; // Type check
        if (!ConnectedInputs.Contains(input))
        {
            ConnectedInputs.Add(input);
        }
        input.ConnectedOutput = this;
    }

    public NodeOutput(VisualNode node, string name, NodePinType type)
    {
        Node = node;
        Name = name;
        Type = type;
    }
}

// NodePinType is already defined in BaseNodes.cs
