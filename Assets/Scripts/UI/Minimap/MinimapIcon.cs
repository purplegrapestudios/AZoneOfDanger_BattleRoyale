using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MinimapIcon : MonoBehaviour
{
    public PhotonView pv;
    public Image IconImage;
    public TextMeshProUGUI IconText;
    public RectTransform PrefabRectTransform;
    public RectTransform IconRectTransform;
    public bool isLocalMinimapPlayer;


    public void SetIcon(Sprite icon) => IconImage.sprite = icon;

    public void SetColor(Color color) => IconImage.color = color;

    public void SetText(string txt)
    {
        if (!string.IsNullOrEmpty(txt))
        {
            IconText.enabled = true;
            IconText.text = txt;
        }
    }

    public void SetTextSize(int size) => IconText.fontSize = size;

    public void SetIsLocalMinimapPlayer(bool val) => isLocalMinimapPlayer = val;
}
