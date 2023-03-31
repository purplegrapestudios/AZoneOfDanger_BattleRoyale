using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapWorldObject : MonoBehaviour
{
    private App m_app;
    public Character m_character;
    public int PlayerID => m_character.Runner.LocalPlayer.PlayerId;
    public Sprite Icon;
    public Color IconColor = Color.white;
    public string Text;
    public int TextSize = 10;
    public bool isLocalMinimapPlayer;
    public Color OffScreenColor;

    public void Init(Character character)
    {
        m_character = character;

        if (m_character.PlayerInputEnabled() && m_character.Runner.LocalPlayer == m_character.Object.InputAuthority)
        {
            isLocalMinimapPlayer = true;
        }

        Minimap.Instance.RegisterMinimapWorldObject(this, OffScreenColor);
    }

}
