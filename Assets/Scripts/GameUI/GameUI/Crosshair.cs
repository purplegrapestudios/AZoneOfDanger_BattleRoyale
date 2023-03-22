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
    }

    public void ShowNormalCrosshair()
    {
        m_crosshairNormal.SetActive(true);
        m_crosshairDamage.SetActive(false);
    }

    public void ShowDamageCrosshair()
    {
        if (DamageCrosshairCoroutine != null)
        {
            StopDamageCrosshairCoroutine();
        }
        DamageCrosshairCoroutine = DamageCrosshairCO();
        StartCoroutine(DamageCrosshairCoroutine);
    }

    private IEnumerator DamageCrosshairCoroutine;
    private IEnumerator DamageCrosshairCO()
    {
        m_crosshairNormal.SetActive(false);
        m_crosshairDamage.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        ShowNormalCrosshair();
    }

    private void StopDamageCrosshairCoroutine()
    {
        StopCoroutine(DamageCrosshairCoroutine);
        DamageCrosshairCoroutine = null;
    }

    public void SetWeaponCrosshair(CharacterShootComponent characterShoot)
    {
        if (!characterShoot.Object.HasInputAuthority) return;

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
