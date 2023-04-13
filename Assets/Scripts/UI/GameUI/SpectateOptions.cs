using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpectateOptions : MonoBehaviour
{
    [SerializeField] private GameObject m_specContainer;
    [SerializeField] private TMP_Text m_spectateLabel;
    [SerializeField] private Button m_specPlayerButtonPrev;
    [SerializeField] private Button m_specPlayerButtonNext;

    public void Init(App app, GameLogicManager gameLogicInstance, GameUIViewController gameUIInstance, SceneCamera sceneCameraInstance)
    {
        m_specPlayerButtonNext.onClick.AddListener(() => { gameLogicInstance.GetSpectatePlayerNext(app.GetPlayer()); });
        m_specPlayerButtonPrev.onClick.AddListener(() => { gameLogicInstance.GetSpectatePlayerPrev(app.GetPlayer()); });
        m_specContainer.SetActive(false);
        gameUIInstance.SetCallback((bool val) => { m_specContainer.SetActive(val); });
        sceneCameraInstance.SetSpectatePlayerLabelCallback(SetSpectatePlayerLabel);
    }

    private void SetSpectatePlayerLabel(string s)
    {
        m_spectateLabel.text = s;
    }
}
