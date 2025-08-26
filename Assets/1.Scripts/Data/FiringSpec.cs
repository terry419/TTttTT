// ��õ ���: Assets/1.Scripts/Data/FiringSpec.cs
using UnityEngine.AddressableAssets;
using UnityEngine;

/// <summary>
/// NewCardDataSO���� EffectExecutor��, ���������� BulletController�� ���޵�
/// ����ü�� �ٽ� �߻� ����� �����ϴ� ������ ����ü�Դϴ�.
/// </summary>
public struct FiringSpec
{
    public float baseDamage;                    // �÷������� ���� �⺻ ���ط�
    public AssetReferenceGameObject projectilePrefabRef; // ����� ����ü �������� Addressable ����
    // �� �ܿ��� ����ü �ӵ�, ũ�� �� �ʿ��� ��� ����� ���⿡ �߰��� �� �ֽ��ϴ�.
}