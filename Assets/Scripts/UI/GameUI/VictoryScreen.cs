using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VictoryScreen : MonoBehaviour
{
    [SerializeField] private GameObject m_spectateOptionsContainer;
    [SerializeField] private GameObject m_playerGameInfoContainer;
    [SerializeField] private GameObject m_victoryScreenContainer;
    [SerializeField] private TMP_Text m_playerLabel;
    private bool m_isCallbackCalled;
    public void Init(GameUIViewController gameUIInstance)
    {
        m_victoryScreenContainer.SetActive(false);
        m_playerLabel.gameObject.SetActive(false);

        gameUIInstance.SetVictoryScreenCallback((bool show, bool isWinner, string playerID) =>
        {
            if (m_isCallbackCalled) return;
            m_victoryScreenContainer.SetActive(show && isWinner);
            m_playerLabel.gameObject.SetActive(show);
            m_playerGameInfoContainer.SetActive(!show);
            m_spectateOptionsContainer.SetActive(!show);
            m_playerLabel.text = isWinner? $"{playerID} Won the Game" : $"{playerID} Won the Game\nBut you lost...\nSorry mate, better luck next time :)";
            m_isCallbackCalled = true;
        });
    }
}