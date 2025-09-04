using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public class VfxAutoReturn : MonoBehaviour
{
    [Tooltip("이 시각 효과가 풀로 반환되기까지의 시간(초)입니다. 파티클의 LifeTime보다 길게 설정하세요.")]
    public float lifeTime = 2f;

    private void OnEnable()
    {
        ReturnToPoolAfterDelay().Forget();
    }

    private async UniTaskVoid ReturnToPoolAfterDelay()
    {
        // lifeTime이 0보다 클 때만 지연 로직 실행
        if (lifeTime > 0)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(lifeTime));
        }

        if (this != null && gameObject.activeInHierarchy)
        {
            ServiceLocator.Get<PoolManager>()?.Release(gameObject);
        }
    }
}