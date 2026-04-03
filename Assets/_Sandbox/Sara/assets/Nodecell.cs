using UnityEngine;
using UnityEngine.UI;
using System;

public class NodeCell : MonoBehaviour
{
    Image bg;
    Button btn;

    public void Setup(bool isBurnt, Action onClick)
    {
        bg  = GetComponent<Image>();
        btn = GetComponent<Button>();

        if (isBurnt)
        {
            btn.interactable = false;
            bg.color = new Color(0.4f, 0.02f, 0.02f);
        }
        else
        {
            btn.onClick.AddListener(() => onClick());
        }
    }

    public void SetColor(Color c)
    {
        if (bg != null) bg.color = c;
    }
}