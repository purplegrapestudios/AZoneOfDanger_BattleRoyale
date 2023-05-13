using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MPUIKIT;
using UnityEngine.Events;

public class TopBar : MonoBehaviour
{
    public static TopBar Instance;

    [SerializeField] private Button m_escapeBtn;
    [SerializeField] private MPImage m_vivoxConnectionImage;
    [SerializeField] private TMP_Text m_vivoxConnectionLabel;

    private void Awake()
    {
    }

    public void SetVivoxConnectionState(string state, Color stateColor, Color[] stateGradient = null)
    {
        m_vivoxConnectionLabel.text = state;

        if (stateGradient != null) { 
            GradientEffect gradientEffect = new GradientEffect();
            gradientEffect.CornerGradientColors = stateGradient;
            m_vivoxConnectionImage.GradientEffect = gradientEffect;
            return;
        }
        m_vivoxConnectionImage.color = stateColor;
    }

    public void SetEscapeButtonCallback(UnityAction action)
    {
        m_escapeBtn.onClick.RemoveAllListeners();
        m_escapeBtn.onClick.AddListener(action);
    }
}
