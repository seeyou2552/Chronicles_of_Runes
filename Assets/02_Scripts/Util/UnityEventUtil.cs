using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

public class UnityEventUtil
{
    public static Action GetEventAction(UnityEvent unityEvent, Component runtimeContext)
    {
        if (unityEvent == null || unityEvent.GetPersistentEventCount() == 0)
        {
            return null;
        }

        var prefabTarget = unityEvent.GetPersistentTarget(0);
        var methodName = unityEvent.GetPersistentMethodName(0);
        var targetType = prefabTarget.GetType();
        if (prefabTarget == null || string.IsNullOrEmpty(methodName))
        {
            return null;
        }

        var runtimeTarget = runtimeContext.GetComponent(targetType);
        if (runtimeTarget == null)
        {
            return null;
        }

        MethodInfo methodInfo = runtimeTarget.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (methodInfo == null)
        {
            return null;
        }

        if (methodInfo.GetParameters().Length != 0)
        {
            return null;
        }

        return (Action)Delegate.CreateDelegate(typeof(Action), runtimeTarget, methodInfo);
    }
}
