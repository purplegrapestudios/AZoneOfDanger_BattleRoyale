using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
    [SerializeField] private GameObject m_crosshairNormal;
    [SerializeField] private GameObject m_crosshairDamage;

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
}
