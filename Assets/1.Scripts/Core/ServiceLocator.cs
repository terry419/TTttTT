using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///   ٽ (Ŵ) ϰ ϴ ߾ Դϴ.
/// ̱ νϽ    üϿ յ ϴ.
/// </summary>
public static class ServiceLocator
{
    // 񽺵 Ÿ(Type) ã  ųʸ
    private static readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

    /// <summary>
    /// (Ŵ) ȳ ũ մϴ.
    /// </summary>
    /// <typeparam name="T">  Ÿ</typeparam>
    /// <param name="service">  νϽ</param>
    public static void Register<T>(T service)
    {
        Type type = typeof(T);
        if (services.ContainsKey(type))
        {
            Debug.LogWarning($"[ServiceLocator] '{type.Name}' (ID: {service.GetHashCode()}) 서비스가 이미 등록되어 있습니다. 이전 ID: {services[type].GetHashCode()}. 덮어씁니다.");
            services[type] = service;
        }
        else
        {
            services.Add(type, service);
            Debug.Log($"[ServiceLocator] '{type.Name}' (ID: {service.GetHashCode()}) 서비스가 새로 등록되었습니다.");
        }
    }

    /// <summary>
    /// ϵ 񽺸 ȳ ũ ãƿɴϴ.
    /// </summary>
    /// <typeparam name="T">ãƿ  Ÿ</typeparam>
    /// <returns>ϵ  νϽ</returns>
    public static T Get<T>()
    {
        Type type = typeof(T);
        if (!services.TryGetValue(type, out object service))
        {
            Debug.LogError($"[ServiceLocator] '{type.Name}' 타입의 서비스를 찾을 수 없습니다! 요청 ID: {typeof(T).GetHashCode()}");
            return default; // null 반환
        }
        Debug.Log($"[ServiceLocator] '{type.Name}' 타입 서비스 요청. 반환 ID: {service.GetHashCode()}");
        return (T)service;
    }

    /// <summary>
    /// ϵ  񽺸 ʱȭմϴ. ( ȯ    )
    /// </summary>
    public static void Clear()
    {
        services.Clear();
        Debug.Log("[ServiceLocator]  񽺰 Ǿϴ.");
    }
}
