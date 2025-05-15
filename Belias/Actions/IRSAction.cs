using System;

namespace Belias.Actions;

/// <summary>
/// A simplified interface that represents an action from RotationSolver
/// without directly depending on the RotationSolver implementation.
/// </summary>
public interface IRSAction
{
    /// <summary>
    /// The ID of the action.
    /// </summary>
    uint ID { get; }
    
    /// <summary>
    /// The name of the action.
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Whether the action is enabled.
    /// </summary>
    bool IsEnabled { get; }
    
    /// <summary>
    /// The cooldown time of the action.
    /// </summary>
    float CoolDownTime { get; }
    
    /// <summary>
    /// Whether the action is a GCD.
    /// </summary>
    bool IsGCD { get; }
    
    /// <summary>
    /// Whether the action is a buff.
    /// </summary>
    bool IsBuff { get; }
    
    /// <summary>
    /// Whether the action requires positional execution.
    /// </summary>
    bool IsPositional { get; }
    
    /// <summary>
    /// Whether the action is part of a combo.
    /// </summary>
    bool IsCombo { get; }
    
    /// <summary>
    /// Gets whether this action can be used while moving.
    /// </summary>
    bool CanUseWhileMoving { get; }
    
    /// <summary>
    /// Gets whether this action requires a target.
    /// </summary>
    bool RequiresTarget { get; }
    
    /// <summary>
    /// The animation lock time of the action.
    /// </summary>
    float AnimationLockTime { get; }
    
    /// <summary>
    /// The range of the action.
    /// </summary>
    float Range { get; }
    
    /// <summary>
    /// Checks if the action can be used.
    /// </summary>
    /// <param name="skipCheck">Whether to skip the action check.</param>
    /// <returns>True if the action can be used, otherwise false.</returns>
    bool ActionCheck(bool skipCheck);
    
    /// <summary>
    /// Checks if the elapsed time after GCD is greater than the specified value.
    /// </summary>
    /// <param name="value">The value to check against.</param>
    /// <returns>True if the elapsed time is greater, otherwise false.</returns>
    bool ElapsedAfterGCD(float value);
}
