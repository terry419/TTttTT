// 
using UnityEngine;

public class PooledObjectInfo : MonoBehaviour
{
    // [수정] GameObject 참조를 string 키로 변경합니다.
    public string AssetKey { get; private set; }

    public void Initialize(string key)
    {
        AssetKey = key;
    }
}