using UnityEngine;
using UnityEngine.UI;

public class ToggleAnimator : MonoBehaviour
{
    [SerializeField] Image background;
    [SerializeField] Sprite spriteOff;
    [SerializeField] Sprite spriteOn;

    Toggle toggle;

    void Awake()
{
    background = GetComponentInChildren<Image>();
    UpdateVisual(false);
}

void Start()
{
    toggle = GetComponentInChildren<Toggle>();
    toggle.onValueChanged.AddListener(OnToggleChanged);
}

    void OnToggleChanged(bool val)
    {
        UpdateVisual(val);
    }

    void UpdateVisual(bool val)
    {
        if (background == null) return;
        background.sprite = val ? spriteOn : spriteOff;
        background.color  = Color.white;
        background.SetNativeSize();
    }
}