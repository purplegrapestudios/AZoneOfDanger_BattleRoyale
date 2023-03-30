using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMuzzleComponent : NetworkBehaviour
{
    [Networked] public Vector3 NetworkedMuzzlePosition { get; set; }
    private Transform tr;
    private Character m_character;
    private ParticleSystem m_muzzleFlash;
    private bool m_isInitialized;
    public void Initialize(Character character, ParticleSystem muzzleFlash)
    {
        tr = GetComponent<Transform>();
        m_character = character;
        m_muzzleFlash = muzzleFlash;
        m_isInitialized = true;
    }

    public override void FixedUpdateNetwork()
    {
        if (!m_isInitialized) return;
        if (!m_character.CharacterHealth.NetworkedIsAlive) return;

        if (m_character.PlayerInputEnabled() && GetInput(out InputData data))
        {
            NetworkedMuzzlePosition = transform.position;
        }
    }

    private void LateUpdate()
    {
        if (!m_isInitialized) return;
        MuzzleFlashUpdate();
    }

    private void MuzzleFlashUpdate()
    {
        if (m_character.CharacterShoot.NetworkedFire)
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
