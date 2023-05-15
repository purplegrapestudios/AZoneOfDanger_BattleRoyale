using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PlayerSelectionViewController : MonoBehaviour
{
    [SerializeField] private Button m_confirmSelectionButton;
    [SerializeField] private ScrollRect m_playerSelectionScrollRect;
    [SerializeField] private PlayerSelectionCard m_playerSelectionCard;
    [SerializeField] private TopBar m_topbar;
    [SerializeField] private List<GameObject> m_playerCharacterObjects;
    [SerializeField] private List<Sprite> m_playerCharacterSprites;
    [SerializeField] private List<string> m_playerCharacterNameInfos;
    [SerializeField] private float m_curY = -10;
    [SerializeField] private float m_cardHeight = 100;
    [SerializeField] private float m_padding = 10;
    private Vector3 m_playerCharacterPosition = new Vector3(0f, 3f, 1.9f);
    private Vector3 m_playerCharacterEulerRotation = new Vector3(0f, 175f,0 );
    private UnityAction m_exitCallback;
    [SerializeField] private int m_currentSelectionIndex;
    private int m_savedPlayerSelectionIndex = -1;
    public List<GameObject> m_characterList;
    public List<PlayerSelectionCard> m_playerCards;
    private float m_spacing => m_cardHeight + m_padding;
    
    private Vector3 m_screenPressPoint;
    private Quaternion m_playerModelOrigRot;

    private App m_app;

    private void Awake()
    {
        m_app = App.FindInstance();

    }
    public void Initialize(UnityAction exitCallback)
    {
        m_exitCallback = exitCallback;

        for (int i = 0; i < m_playerCharacterObjects.Count; i++)
        {
            GameObject characterObj = Instantiate(m_playerCharacterObjects[i]);
            m_characterList.Add(characterObj);
            characterObj.transform.localScale = Vector3.one;
            characterObj.transform.position = m_playerCharacterPosition;
            characterObj.transform.rotation = Quaternion.Euler(m_playerCharacterEulerRotation);
            characterObj.SetActive(false);
            PlayerSelectionCard playerCard = Instantiate(m_playerSelectionCard, m_playerSelectionScrollRect.content);

            playerCard.Initialize(i, m_playerCharacterSprites[i] ?? null, m_playerCharacterNameInfos[i] ?? null,
                (int index) => {
                    m_characterList[m_currentSelectionIndex].SetActive(false);
                    m_currentSelectionIndex = index;
                    m_characterList[m_currentSelectionIndex].SetActive(true);
                    m_characterList[m_currentSelectionIndex].transform.rotation = Quaternion.Euler(m_playerCharacterEulerRotation);
                });
            m_playerCards.Add(playerCard);

            var rt = playerCard.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, m_curY);
            m_curY -= m_spacing;
        }

        if (m_savedPlayerSelectionIndex >= 0)
        {
            m_playerCards[m_savedPlayerSelectionIndex].Tapped();
        }
        else
        {
            m_playerCards[0].Tapped();
        }

        m_confirmSelectionButton.onClick.AddListener(() => { TappedConfirmButton(); });

        m_topbar.SetEscapeButtonCallback(() => {
            ExitPlayerSelectionCallback();
        });
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            m_screenPressPoint = Input.mousePosition;
            m_playerModelOrigRot = m_characterList[m_currentSelectionIndex].transform.rotation;
        }
        else if (Input.GetMouseButton(0))
        {
            float curDistSinceInitialPress = (Input.mousePosition - m_screenPressPoint).x;
            m_characterList[m_currentSelectionIndex].transform.rotation = m_playerModelOrigRot * Quaternion.Euler(Vector3.up * (curDistSinceInitialPress / Screen.width) * 360);
        }
    }

    private void ExitPlayerSelectionCallback()
    {
        ResetSelection();
        m_exitCallback();
    }

    private void TappedConfirmButton()
    {
        m_savedPlayerSelectionIndex = m_currentSelectionIndex;
        m_app.CharacterSelectionIndex = m_savedPlayerSelectionIndex;
        Debug.Log($"Tapped Confirmed Player Button, app.CharacterSelectionIndex: {m_app.CharacterSelectionIndex}");
        ExitPlayerSelectionCallback();
    }

    public void ResetSelection()
    {
        for (int i = 0; i < m_playerCharacterObjects.Count; i++)
        {
            if (m_savedPlayerSelectionIndex >= 0)
            {
                m_characterList[i].SetActive(i == m_savedPlayerSelectionIndex);
            }
            else
            {
                m_characterList[i].SetActive(i == 0);
            }
        }
        m_currentSelectionIndex = m_savedPlayerSelectionIndex >= 0 ? m_savedPlayerSelectionIndex : 0;
    }
}