using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameUIViewController : MonoBehaviour
{
    public static GameUIViewController Instance;
    [SerializeField] private TMP_Text m_healthTxt;

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateHealthText(string value)
    {
        m_healthTxt.text = value;
    }
}
