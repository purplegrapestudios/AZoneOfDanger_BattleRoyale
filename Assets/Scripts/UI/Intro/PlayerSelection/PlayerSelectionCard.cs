using UnityEngine;
using UnityEngine.UI;
using MPUIKIT;
using TMPro;

public class PlayerSelectionCard : MonoBehaviour
{
    [SerializeField] private Button m_playerCardButton;
    [SerializeField] private MPImage m_playerCardImage;
    [SerializeField] private TMP_Text m_playerCardNameInfo;
    private int m_playerCardIndex;
    private System.Action<int> m_tappedPlayerCardCallback;

    public void Initialize(int index, Sprite sprite, string nameInfo, System.Action<int> tappedPlayerCardCallback)
    {
        m_playerCardIndex = index;
        m_playerCardImage.sprite = sprite;
        m_playerCardNameInfo.text = nameInfo;
        m_tappedPlayerCardCallback = tappedPlayerCardCallback;
        m_playerCardButton.onClick.AddListener(() => { Tapped(); });
    }

    public void Tapped()
    {
        m_tappedPlayerCardCallback(m_playerCardIndex);
    }
}