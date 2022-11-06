using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MPUIKIT;

public class TopBar : MonoBehaviour
{
    public static TopBar Instance;

    [SerializeField] private Button m_escapeBtn;
    [SerializeField] private MPImage m_vivoxConnectionImage;
    [SerializeField] private TMP_Text m_vivoxConnectionLabel;

    private void Awake()
    {
        if (Instance == null) {
            Instance = this; 
        }
        DontDestroyOnLoad(Instance.gameObject);
    }

    public void SetEscapeButtonCallback(System.Action escapeBtnCallback)
    {
        m_escapeBtn.onClick.AddListener(() => {
            escapeBtnCallback();
        });
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
}
