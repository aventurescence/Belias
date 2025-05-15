using System;
using System.Collections.Generic;
using System.Numerics;
using Belias.Actions;

namespace Belias.Windows.NodesSystem.Types;

/// <summary>
/// Represents the base class for all nodes in the visual editor.
/// </summary>
public abstract class VisualNode
{
    /// <summary>
    /// Unique identifier for this node.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Title displayed on the node.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Position of the node in the editor.
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// Node inputs.
    /// </summary>
    public List<NodeInput> Inputs { get; } = new();

    /// <summary>
    /// Node outputs.
    /// </summary>
    public List<NodeOutput> Outputs { get; } = new();

    protected VisualNode(int id, string title)
    {
        Id = id;
        Title = title;
    }

    /// <summary>
    /// Called when the node is executed in the rotation sequence.
    /// </summary>
    /// <returns>True if the node action was successful, false otherwise.</returns>
    public abstract bool Execute();
}

/// <summary>
/// Base class for action nodes that map to Dalamud actions.
/// </summary>
public abstract class ActionNode : VisualNode
{    /// <summary>
    /// The underlying action from RotationSolver.
    /// </summary>
    public IRSAction RSAction { get; }

    /// <summary>
    /// Additional conditions that must be met for this action to be used.
    /// </summary>
    public List<ConditionNode> Conditions { get; } = new();    protected ActionNode(int id, string title, IRSAction action) : base(id, title)
    {
        RSAction = action;

        // Common inputs/outputs for action nodes
        Inputs.Add(new NodeInput(this, "Execute", NodePinType.Flow));
        Outputs.Add(new NodeOutput(this, "Next", NodePinType.Flow));
        Outputs.Add(new NodeOutput(this, "Complete", NodePinType.Flow));
    }public override bool Execute()
    {
        // Check conditions
        foreach (var condition in Conditions)
        {
            if (!condition.Evaluate()) return false;
        }        // Try to use the action
        return RSAction.ActionCheck(false);
    }
}

/// <summary>
/// Base class for nodes that evaluate conditions.
/// </summary>
public abstract class ConditionNode : VisualNode
{
    protected ConditionNode(int id, string title) : base(id, title)
    {
        Outputs.Add(new NodeOutput(this, "True", NodePinType.Flow));
        Outputs.Add(new NodeOutput(this, "False", NodePinType.Flow));
    }

    /// <summary>
    /// Evaluates the condition.
    /// </summary>
    /// <returns>True if the condition is met, false otherwise.</returns>
    public abstract bool Evaluate();
}

/// <summary>
/// Types of node pin connections.
/// </summary>
public enum NodePinType
{
    Flow, // Control flow between nodes
    Boolean, // Boolean values
    Number, // Numeric values
    Status, // Status effects
    Target, // Target information
    Action // Action references
}
