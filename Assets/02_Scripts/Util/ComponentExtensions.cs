using UnityEngine;

public static class ComponentExtensions
{
    public static bool TryGetComponentInChildren<T>(this Component parent, out T result, bool includeInactive = false) where T : Component
    {
        result = parent.GetComponentInChildren<T>(includeInactive);
        return result != null;
    }

    public static bool TryGetComponentInParent<T>(this Component child, out T result, bool includeInactive = false) where T : Component
    {
        result = child.GetComponentInParent<T>(includeInactive);
        return result != null;
    }
}