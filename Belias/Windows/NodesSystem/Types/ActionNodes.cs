using System.Numerics;
using Belias.Actions;

namespace Belias.Windows.NodesSystem.Types;

/// <summary>
/// Node representing a GCD (Global Cooldown) action.
/// </summary>
public class GCDActionNode : ActionNode
{
    public GCDActionNode(int id, IRSAction action) : base(id, action.Name, action) 
    {
        // Additional outputs specific to GCD actions
        Outputs.Add(new NodeOutput(this, "OnCooldown", NodePinType.Flow));
        Outputs.Add(new NodeOutput(this, "CooldownTime", NodePinType.Number));
    }
}

/// <summary>
/// Node representing an oGCD (off Global Cooldown) action.
/// </summary>
public class OGCDActionNode : ActionNode
{
    public OGCDActionNode(int id, IRSAction action) : base(id, action.Name, action)
    {
        // Additional inputs/outputs for oGCD clipping prevention
        Inputs.Add(new NodeInput(this, "GCDAvailable", NodePinType.Boolean));
        Outputs.Add(new NodeOutput(this, "AnimationLock", NodePinType.Number));
    }
}

/// <summary>
/// Node representing a buff or utility action.
/// </summary>
public class BuffActionNode : ActionNode
{
    public BuffActionNode(int id, IRSAction action) : base(id, action.Name, action)
    {
        // Status effect tracking
        Inputs.Add(new NodeInput(this, "TargetStatus", NodePinType.Status));
        Outputs.Add(new NodeOutput(this, "StatusApplied", NodePinType.Status));
        Outputs.Add(new NodeOutput(this, "Duration", NodePinType.Number));
    }
}

/// <summary>
/// Node for actions that require specific positioning or range checks.
/// </summary>
public class PositionalActionNode : ActionNode
{
    public PositionalActionNode(int id, IRSAction action) : base(id, action.Name, action)
    {
        // Position and range info
        Inputs.Add(new NodeInput(this, "TargetPosition", NodePinType.Target));
        Inputs.Add(new NodeInput(this, "InRange", NodePinType.Boolean));
        Outputs.Add(new NodeOutput(this, "Range", NodePinType.Number));
    }
}

/// <summary>
/// Node for combo actions that need to follow specific sequences.
/// </summary>
public class ComboActionNode : ActionNode
{
    public ComboActionNode(int id, IRSAction action) : base(id, action.Name, action)
    {
        // Combo management
        Inputs.Add(new NodeInput(this, "PreviousComboAction", NodePinType.Action));
        Outputs.Add(new NodeOutput(this, "ComboBreak", NodePinType.Flow));
        Outputs.Add(new NodeOutput(this, "NextInCombo", NodePinType.Action));
    }
}
