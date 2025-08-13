using System;
using System.Collections.Generic;

public static class EventBus
{
    // 타입 기반 이벤트 저장
    private static Dictionary<Type, Delegate> typedEventTable = new();

    // 문자열 기반 이벤트 저장
    private static Dictionary<string, Action<object>> stringEventTable = new();

    // enum 기반 이벤트 저장
    private static Dictionary<Enum, Action<object>> enumEventTable = new();

    // 타입 기반 구독
    public static void Subscribe<T>(Action<T> callback)
    {
        if (typedEventTable.TryGetValue(typeof(T), out var del))
            typedEventTable[typeof(T)] = Delegate.Combine(del, callback);
        else
            typedEventTable[typeof(T)] = callback;
    }

    // 타입 기반 구독 해제
    public static void Unsubscribe<T>(Action<T> callback)
    {
        if (typedEventTable.TryGetValue(typeof(T), out var del))
        {
            var currentDel = Delegate.Remove(del, callback);
            if (currentDel == null)
                typedEventTable.Remove(typeof(T));
            else
                typedEventTable[typeof(T)] = currentDel;
        }
    }

    // 타입 기반 발행
    public static void Publish<T>(T evt)
    {
        if (typedEventTable.TryGetValue(typeof(T), out var del))
        {
            var callback = del as Action<T>;
            callback?.Invoke(evt);
        }
    }

    // 문자열 기반 구독
    public static void Subscribe(string eventName, Action<object> callback)
    {
        if (stringEventTable.TryGetValue(eventName, out var existing))
            stringEventTable[eventName] = existing + callback;
        else
            stringEventTable[eventName] = callback;
    }

    // 문자열 기반 구독 해제
    public static void Unsubscribe(string eventName, Action<object> callback)
    {
        if (stringEventTable.TryGetValue(eventName, out var existing))
        {
            existing -= callback;
            if (existing == null)
                stringEventTable.Remove(eventName);
            else
                stringEventTable[eventName] = existing;
        }
    }

    // 문자열 기반 발행
    public static void Publish(string eventName, object param = null)
    {
        if (stringEventTable.TryGetValue(eventName, out var callback))
        {
            callback?.Invoke(param);
        }
    }

    // enum 기반 구독
    public static void Subscribe(Enum eventKey, Action<object> callback)
    {
        if (enumEventTable.TryGetValue(eventKey, out var existing))
            enumEventTable[eventKey] = existing + callback;
        else
            enumEventTable[eventKey] = callback;
    }

    // enum 기반 구독 해제
    public static void Unsubscribe(Enum eventKey, Action<object> callback)
    {
        if (enumEventTable.TryGetValue(eventKey, out var existing))
        {
            existing -= callback;
            if (existing == null)
                enumEventTable.Remove(eventKey);
            else
                enumEventTable[eventKey] = existing;
        }
    }

    // enum 기반 발행
    public static void Publish(Enum eventKey, object param = null)
    {
        if (enumEventTable.TryGetValue(eventKey, out var callback))
        {
            callback?.Invoke(param);
        }
    }

    //모든 구독 내역 초기화
    public static void Clear()
    {
        typedEventTable = new();
        stringEventTable = new();
        enumEventTable = new();
    }
}