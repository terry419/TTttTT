// ϸ: UICursorManager.cs (丵 Ϸ)
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UICursorManager : MonoBehaviour
{
    [SerializeField]
    private Image globalCursorImage;

    private Sprite defaultCursorSprite;

    void Awake()
    {
        ServiceLocator.Register<UICursorManager>(this);
        if (globalCursorImage != null)
        {
            defaultCursorSprite = globalCursorImage.sprite;
        }
    }

    void Update()
    {
        if (globalCursorImage == null || EventSystem.current == null) return;

        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;

        if (selectedObject != null)
        {
            globalCursorImage.gameObject.SetActive(true);
            globalCursorImage.rectTransform.position = selectedObject.transform.position;
        }
        else
        {
            globalCursorImage.gameObject.SetActive(false);
        }
    }

    public void ChangeCursorSprite(Sprite newSprite)
    {
        if (globalCursorImage == null) return;
        globalCursorImage.sprite = newSprite;
    }

    public void ResetCursorToDefault()
    {
        if (globalCursorImage == null || defaultCursorSprite == null) return;
        globalCursorImage.sprite = defaultCursorSprite;
    }
}