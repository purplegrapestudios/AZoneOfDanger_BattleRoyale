using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMuzzleComponent : NetworkBehaviour
{
    [Networked] public Vector3 NetworkedMuzzlePosition { get; set; }
    private CharacterShootComponent m_characterShoot;
    private ParticleSystem m_muzzleFlash;
    private bool isInitialized;
    public void Initialize(CharacterShootComponent characterShootComponent, ParticleSystem muzzleFlash)
    {
        m_characterShoot = characterShootComponent;
        m_muzzleFlash = muzzleFlash;
        isInitialized = true;
    }

    private void LateUpdate()
    {
        if (!isInitialized) return;
        MuzzleFlashUpdate();
    }

    private void MuzzleFlashUpdate()
    {
        if (m_characterShoot.NetworkedFire)
        {
            if (!m_muzzleFlash.isPlaying)
                m_muzzleFlash.Play();
        }
        else
        {
            if (!m_muzzleFlash.isStopped)
                m_muzzleFlash.Stop();
        }
    }
}
