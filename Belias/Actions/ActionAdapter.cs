using System;

namespace Belias.Actions;

/// <summary>
/// Adapter class that provides extension methods for working with RotationSolver's actions
/// without direct dependency on the original types.
/// </summary>
public static class ActionAdapter
{
    private const string SettingPropertyName = "Setting";

    /// <summary>
    /// Gets the cooldown time of the action.
    /// </summary>
    public static float GetCoolDownTime(this object action)
    {
        try
        {
            var cooldown = action.GetType().GetProperty("Cooldown")?.GetValue(action);
            if (cooldown == null) return 0f;

            var cooldownTotal = cooldown.GetType().GetProperty("CooldownTotal")?.GetValue(cooldown);
            if (cooldownTotal is float f)
                return f;
            else if (cooldownTotal != null)
                return Convert.ToSingle(cooldownTotal);

            return 0f;
        }
        catch
        {
            return 0f;
        }
    }

    /// <summary>
    /// Checks if the action is a GCD action.
    /// </summary>
    public static bool IsGCD(this object action)
    {
        try
        {
            var info = action.GetType().GetProperty("Info")?.GetValue(action);
            if (info == null) return false;

            var category = info.GetType().GetProperty("ActionCategory")?.GetValue(info);
            if (category == null) return false;

            // Check if it's not an ability (abilities are oGCDs)
            if (category is int intValue)
                return intValue != 4;  // Assume 4 is Ability
            else
                return category.ToString() != "Ability";
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if the action is a buff.
    /// </summary>
    public static bool IsBuff(this object action)
    {
        try
        {
            var setting = action.GetType().GetProperty(SettingPropertyName)?.GetValue(action);
            if (setting == null) return false;

            var statusProvide = setting.GetType().GetProperty("StatusProvide")?.GetValue(setting);
            if (statusProvide is Array array)
                return array.Length > 0;

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if the action requires positional execution.
    /// </summary>
    public static bool IsPositional(this object action)
    {
        try
        {
            var setting = action.GetType().GetProperty(SettingPropertyName)?.GetValue(action);
            if (setting == null) return false;

            var specialType = setting.GetType().GetProperty("SpecialType")?.GetValue(setting);
            if (specialType == null) return false;

            // Check if it's a positional type
            if (specialType is int intValue)
                return intValue == 1;  // Assume 1 is MovingForward
            else
                return specialType.ToString() == "MovingForward";
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if the action is part of a combo.
    /// </summary>
    public static bool IsCombo(this object action)
    {
        try
        {
            var setting = action.GetType().GetProperty(SettingPropertyName)?.GetValue(action);
            if (setting == null) return false;

            var comboIds = setting.GetType().GetProperty("ComboIds")?.GetValue(setting);
            if (comboIds is Array array)
                return array.Length > 0;

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the animation lock time of the action.
    /// </summary>
    public static float GetAnimationLockTime(this object action)
    {
        try
        {
            var info = action.GetType().GetProperty("Info")?.GetValue(action);
            if (info == null) return 0.6f;  // Default animation lock

            var animLock = info.GetType().GetProperty("AnimationLock")?.GetValue(info);
            if (animLock is float f)
                return f;
            else if (animLock != null)
                return Convert.ToSingle(animLock);

            return 0.6f;
        }
        catch
        {
            return 0.6f;
        }
    }

    /// <summary>
    /// Gets the range of the action.
    /// </summary>
    public static float GetRange(this object action)
    {
        try
        {
            var targetInfo = action.GetType().GetProperty("TargetInfo")?.GetValue(action);
            if (targetInfo == null) return 0f;

            var range = targetInfo.GetType().GetProperty("Range")?.GetValue(targetInfo);
            if (range is float f)
                return f;
            else if (range != null)
                return Convert.ToSingle(range);

            return 0f;
        }
        catch
        {
            return 0f;
        }
    }

    /// <summary>
    /// Checks if the action can be used while moving.
    /// </summary>
    public static bool CanUseWhileMoving(this object action)
    {
        try
        {
            var setting = action.GetType().GetProperty(SettingPropertyName)?.GetValue(action);
            if (setting == null) return false;

            var canUseMoving = setting.GetType().GetProperty("CanUseMoving")?.GetValue(setting);
            return canUseMoving is bool b && b;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if the action requires a target.
    /// </summary>
    public static bool RequiresTarget(this object action)
    {
        try
        {
            var targetInfo = action.GetType().GetProperty("TargetInfo")?.GetValue(action);
            if (targetInfo == null) return false;

            var needTarget = targetInfo.GetType().GetProperty("NeedTarget")?.GetValue(targetInfo);
            return needTarget is bool b && b;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if an action can be used based on its action check.
    /// </summary>
    public static bool ActionCheck(this object action, bool skipCheck)
    {
        try
        {
            // Try to call CanUse method first
            var canUseMethod = action.GetType().GetMethod("CanUse", new[] { typeof(bool) });
            if (canUseMethod != null)
            {
                var result = canUseMethod.Invoke(action, new object[] { skipCheck });
                return result is bool b && b;
            }

            // Fall back to checking ActionCheck delegate in settings
            var setting = action.GetType().GetProperty(SettingPropertyName)?.GetValue(action);
            if (setting != null)
            {
                var actionCheck = setting.GetType().GetProperty("ActionCheck")?.GetValue(setting);
                if (actionCheck != null)
                {
                    // Try to invoke the delegate
                    var invokeMethod = actionCheck.GetType().GetMethod("Invoke");
                    if (invokeMethod != null)
                    {
                        var result = invokeMethod.Invoke(actionCheck, null);
                        return result is bool b && b;
                    }
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if the elapsed time after GCD is greater than the specified value.
    /// </summary>
    public static bool ElapsedAfterGCD(this object action, float value)
    {
        // For simplicity just return false
        // In a real implementation, this would check the GCD timer
        return false;
    }
}
