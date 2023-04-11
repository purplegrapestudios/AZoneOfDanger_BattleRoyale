using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MPUIKIT;

public class Crosshair : MonoBehaviour
{
    [SerializeField] private GameObject m_crosshairNormal;
    [SerializeField] private GameObject m_crosshairDamage;

    [SerializeField] private Sprite m_crosshairAR;
    [SerializeField] private Sprite m_crosshairShotgun;
    private MPImage m_crosshairImage;

    private void Awake()
    {
        m_crosshairImage = GetComponent<MPImage>();
        m_crosshairImage.color = new Color(1, 1, 1, 0);
    }

    public void SetWeaponCrosshair(CharacterShootComponent characterShoot)
    {
        if (!characterShoot.Object.HasInputAuthority) return;

        m_crosshairImage.color = Color.white;
        if (characterShoot.NetworkedCurrWeaponID == 0)
        {
            m_crosshairImage.sprite = m_crosshairAR;
        }
        else if (characterShoot.NetworkedCurrWeaponID == 1)
        {
            m_crosshairImage.sprite = m_crosshairShotgun;
        }
    }
}
