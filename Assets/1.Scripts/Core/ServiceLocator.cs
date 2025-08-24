// 파일명: ServiceLocator.cs (Unregister 메소드 추가)
using System;
using System.Collections.Generic;
using UnityEngine;

public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

    /// <summary>
    /// 특정 타입의 서비스가 이미 등록되어 있는지 확인합니다.
    /// </summary>
    public static bool IsRegistered<T>()
    {
        return services.ContainsKey(typeof(T));
    }

    public static void Register<T>(T service)
    {
        Type type = typeof(T);
        if (services.ContainsKey(type))
        {
            Debug.LogWarning($"[ServiceLocator] '{type.Name}' 서비스가 이미 등록되어 있습니다. 덮어씁니다.");
            services[type] = service;
        }
        else
        {
            services.Add(type, service);
            Debug.Log($"[ServiceLocator] '{type.Name}' 서비스가 새로 등록되었습니다.");
        }
    }

    public static T Get<T>()
    {
        Type type = typeof(T);
        if (!services.TryGetValue(type, out object service))
        {
            Debug.LogError($"[ServiceLocator] '{type.Name}' 타입의 서비스를 찾을 수 없습니다!");
            return default;
        }
        return (T)service;
    }

    /// <summary>
    /// [추가된 메소드] 특정 타입의 서비스를 등록 해제합니다.
    /// PlayerController처럼 씬이 변경될 때 파괴되는 객체를 위해 필요합니다.
    /// </summary>
    public static void Unregister<T>(T service)
    {
        Type type = typeof(T);
        if (services.ContainsKey(type) && services[type].Equals(service))
        {
            services.Remove(type);
            Debug.Log($"[ServiceLocator] '{type.Name}' 서비스가 등록 해제되었습니다.");
        }
    }

    public static void Clear()
    {
        services.Clear();
    }
}