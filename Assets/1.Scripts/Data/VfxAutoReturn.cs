using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public class VfxAutoReturn : MonoBehaviour
{
    [Tooltip("�� �ð� ȿ���� Ǯ�� ��ȯ�Ǳ������ �ð�(��)�Դϴ�. ��ƼŬ�� LifeTime���� ��� �����ϼ���.")]
    public float lifeTime = 2f;

    private void OnEnable()
    {
        ReturnToPoolAfterDelay().Forget();
    }

    private async UniTaskVoid ReturnToPoolAfterDelay()
    {
        // lifeTime�� 0���� Ŭ ���� ���� ���� ����
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