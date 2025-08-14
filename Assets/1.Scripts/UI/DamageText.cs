using UnityEngine;
using TMPro;
using System.Collections;

public class DamageText : MonoBehaviour
{
    private TextMeshProUGUI textMesh;

    void Awake()
    {
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void ShowDamage(int damage, Vector3 position)
    {
        transform.position = position;
        textMesh.text = damage.ToString();
        StartCoroutine(AnimateAndRelease());
    }

    private IEnumerator AnimateAndRelease()
    {
        float duration = 0.7f;
        float timer = 0;
        Vector3 startPos = transform.position;
        Color startColor = textMesh.color;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            // ���� �������� ȿ��
            transform.position = startPos + new Vector3(0, progress * 1.5f, 0);

            // ���� ���������� ȿ��
            textMesh.color = new Color(startColor.r, startColor.g, startColor.b, 1 - progress);

            yield return null;
        }

        // �ִϸ��̼��� ������ PoolManager�� ��ȯ
        PoolManager.Instance.Release(gameObject);
    }
}