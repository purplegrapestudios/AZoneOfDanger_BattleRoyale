using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EAudioClip
{
    testShot,
}

[Serializable]
public struct AudioClipKeyPair
{
    public EAudioClip key;
    public AudioClip clip;
}

public class CharacterAudioComponent : NetworkBehaviour
{
    [SerializeField] private List<AudioClipKeyPair> m_audioList;
    private Dictionary<EAudioClip, AudioClip> m_audioDictionary = new Dictionary<EAudioClip, AudioClip>();
    [SerializeField] private AudioSource m_audioSource;
    private bool m_isInitialized;

    private void Awake()
    {
        foreach (var kvp in m_audioList)
        {
            m_audioDictionary[kvp.key] = kvp.clip;
        }
    }

    public void Initialize(AudioSource audioSource)
    {
        m_audioSource = audioSource;
        m_isInitialized = true;
    }

    public void OnPlayClip(EAudioClip clipKey)
    {
        if (!m_isInitialized) return;
        m_audioSource.volume = 0;
        m_audioSource.Stop();

        m_audioSource.volume = 1;
        m_audioSource.PlayOneShot(m_audioDictionary[clipKey]);
    }
}
