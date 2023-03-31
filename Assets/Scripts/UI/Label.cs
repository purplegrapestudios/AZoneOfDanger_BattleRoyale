using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text)), RequireComponent(typeof(RectTransform))]
public class Label : MonoBehaviour
{
    private TMP_Text m_textComponent;
    private RectTransform m_rectTr;

    private void Awake()
    {
        m_textComponent = GetComponent<TMP_Text>();
    }

    public void UpdateText(string s)
    {
        m_textComponent.text = s;
        m_rectTr.sizeDelta = new Vector2(m_textComponent.preferredWidth, m_rectTr.sizeDelta.y);
    }
}
