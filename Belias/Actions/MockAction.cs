using System;

namespace Belias.Actions;

/// <summary>
/// A mock implementation of IRSAction for the node editor
/// </summary>
public class MockAction : IRSAction
{
    public uint ID { get; }
    public string Name { get; }
    public bool IsEnabled { get; }
    public float CoolDownTime { get; }
    public bool IsGCD { get; }
    public bool IsBuff { get; }
    public bool IsPositional { get; }
    public bool IsCombo { get; }
    public bool CanUseWhileMoving { get; }
    public bool RequiresTarget { get; }
    public float AnimationLockTime { get; }
    public float Range { get; }
    
    /// <summary>
    /// Creates a mock action for use in the node editor.
    /// </summary>
    /// <param name="id">The action ID</param>
    /// <param name="name">The action name</param>
    /// <param name="isGCD">Whether this is a GCD action</param>
    /// <param name="isBuff">Whether this is a buff action</param>
    /// <param name="isPositional">Whether this requires positional execution</param>
    /// <param name="isCombo">Whether this is part of a combo chain</param>
    public MockAction(uint id, string name, bool isGCD = true, bool isBuff = false, bool isPositional = false, bool isCombo = false)
    {
        ID = id;
        Name = name;
        IsEnabled = true;
        CoolDownTime = 0f;
        IsGCD = isGCD;
        IsBuff = isBuff;
        IsPositional = isPositional;
        IsCombo = isCombo;
        
        // Default values for other properties
        CanUseWhileMoving = !isGCD; // Assume oGCDs can be used while moving
        RequiresTarget = true;      // Most actions require targets
        AnimationLockTime = isGCD ? 0.7f : 0.5f; // Typical animation lock times
        Range = isPositional ? 3f : 25f; // Close range for positionals, longer for others
    }

    /// <summary>
    /// Checks if the action can be used.
    /// </summary>
    /// <param name="skipCheck">Whether to skip the action check.</param>
    /// <returns>True if the action can be used, otherwise false.</returns>
    public bool ActionCheck(bool skipCheck = false)
    {
        // Mock implementation that always returns true
        return true;
    }
    
    /// <summary>
    /// Checks if the elapsed time after GCD is greater than the specified value.
    /// </summary>
    /// <param name="value">The value to check against.</param>
    /// <returns>True if the elapsed time is greater, otherwise false.</returns>
    public bool ElapsedAfterGCD(float value)
    {
        // This is a mock implementation, so we'll just return true
        return true;
    }
}
