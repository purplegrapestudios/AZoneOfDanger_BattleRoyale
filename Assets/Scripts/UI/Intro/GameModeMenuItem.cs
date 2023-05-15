using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MPUIKIT;
using TMPro;

public class GameModeMenuItem : MonoBehaviour
{
    [SerializeField] private Button m_gameModeButton;
    [SerializeField] private MPImage m_gameModeBG;
    [SerializeField] private TMP_Text m_gameModeLabel;
    [SerializeField] private GradientEffect m_gradientEffectServer;
    [SerializeField] private GradientEffect m_gradientEffectHost;
    private System.Action<int> m_onclickCallbackInt;
    private System.Action<bool> m_onclickCallbackBool;
    private System.Action m_onclickCallback;
    private bool m_init;
    
    public void Initialize(System.Action onclickCallback = null, System.Action<int> onclickCallbackInt = null, int intValue = 0, System.Action<bool> onclickCallbackBool = null, bool boolValue = false)
    {
        m_onclickCallback = onclickCallback;
        if(m_onclickCallback != null) m_gameModeButton.onClick.AddListener(() => { m_onclickCallback(); });

        m_onclickCallbackInt = onclickCallbackInt;
        if (m_onclickCallbackInt != null) m_gameModeButton.onClick.AddListener(() => { m_onclickCallbackInt(intValue); });

        m_onclickCallbackBool = onclickCallbackBool;
        if (m_onclickCallbackBool != null) m_gameModeButton.onClick.AddListener(() => { m_onclickCallbackBool(boolValue); });

        m_init = true;
    }

    public void UpdateLabel(string txt)
    {
        if (!m_init) return;
        m_gameModeLabel.text = txt;
    }

    public void UpdateGradientBG(bool showServerGradient)
    {
        if (!m_init) return;
        if (showServerGradient)
            m_gameModeBG.GradientEffect = m_gradientEffectServer;
        else
            m_gameModeBG.GradientEffect = m_gradientEffectHost;
    }
}
