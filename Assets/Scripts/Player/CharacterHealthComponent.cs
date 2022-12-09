using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterHealthComponent : NetworkBehaviour
{

    [Networked(OnChanged = nameof(OnHealthChanged))] public float NetworkedHealth { get; set; }
    [Networked(OnChanged = nameof(OnKillsChanged))] public int NetworkedKills { get; set; }
    [Networked(OnChanged = nameof(OnDeathsChanged))] public int NetworkedDeaths { get; set; }
    [Networked(OnChanged = nameof(OnIsAliveChanged))] public NetworkBool NetworkedIsAlive { get; set; }
    public CharacterHealthComponent Instigator => m_instigator;

    [SerializeField] private float m_baseHealth = 100f;
    private Character m_character;
    private CharacterHealthComponent m_instigator;
    private bool isInitialized;
    private App m_app;

    public void Initialize(Character character)
    {
        m_app = App.FindInstance();
        m_character = character;
        NetworkedHealth = m_baseHealth;
        NetworkedDeaths = 0;
        NetworkedKills = 0;
        
        if (m_character.Object.HasInputAuthority)
        {
            GameUIViewController.Instance.UpdateHealthText($"{Mathf.RoundToInt(NetworkedHealth)} HP");
            GameUIViewController.Instance.UpdateKillsText($"{NetworkedKills} <sprite=9>");
            GameUIViewController.Instance.UpdateDeathsText($"{NetworkedDeaths} <sprite=10>");
        }

        NetworkedIsAlive = true;
        isInitialized = true;
    }

    public void OnTakeDamage(float damage, CharacterHealthComponent instigator)
    {
        if (!isInitialized) return;
        if (!NetworkedIsAlive) return;

        //        Debug.Log($"{m_character.Player.Name} took {damage} damage");
        m_instigator = instigator;
        NetworkedHealth -= damage;

        if (NetworkedHealth <= 0)
        {
            Debug.Log($"{m_character.Player.Name} couldn't make it");
            NetworkedIsAlive = false;
            NetworkedDeaths += 1;

            if (instigator != null)
            {
                Debug.Log($"Instigator updated kill to: {instigator.NetworkedKills}");
                instigator.UpdateKillCount();
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (m_character.Player && m_character.Player.InputEnabled && GetInput(out InputData data))
        {
            if (data.GetButton(ButtonFlag.RESPAWN))
            {
                NetworkedDeaths += 1;
                Respawn(this);
            }
        }
    }

    static void OnHealthChanged(Changed<CharacterHealthComponent> changed)
    {
        if (changed.Behaviour.m_character.Object.HasInputAuthority)
        {
            GameUIViewController.Instance.UpdateHealthText($"{Mathf.RoundToInt(changed.Behaviour.NetworkedHealth)} HP");
        }

    }

    static void OnIsAliveChanged(Changed<CharacterHealthComponent> changed)
    {
        //if (changed.Behaviour.m_character.Object.HasInputAuthority)
        //{
        //    //if (!changed.Behaviour.NetworkedIsAlive)
        //    //    changed.Behaviour.NetworkedDeaths += 1;
        //}

        if (!changed.Behaviour.NetworkedIsAlive)
            changed.Behaviour.Respawn(changed.Behaviour);
    }

    static void OnKillsChanged(Changed<CharacterHealthComponent> changed)
    {
        if (changed.Behaviour.m_character.Object.HasInputAuthority)
        {
            GameUIViewController.Instance.UpdateKillsText($"{changed.Behaviour.NetworkedKills} <sprite=9>");
        }
    }

    static void OnDeathsChanged(Changed<CharacterHealthComponent> changed)
    {
        if (changed.Behaviour.m_character.Object.HasInputAuthority)
        {
            GameUIViewController.Instance.UpdateDeathsText($"{changed.Behaviour.NetworkedDeaths} <sprite=10>");
        }
    }


    private void UpdateKillCount()
    {
        NetworkedKills += 1;
    }

    private void Respawn(CharacterHealthComponent changedBehaviour)
    {
        if(RespawnCoroutine != null)
            return;

        RespawnCoroutine = RespawnCO(changedBehaviour);
        StartCoroutine(RespawnCoroutine);
    }

    private IEnumerator RespawnCoroutine;
    private IEnumerator RespawnCO(CharacterHealthComponent changedBehaviour)
    {
        Debug.Log("Waiting to respawn");
        yield return new WaitForSeconds(1);
        GetComponent<NetworkRigidbody>().TeleportToPosition(m_app.Session.Map.GetSpawnPoint(Object.InputAuthority).transform.position);
        Debug.Log("Finish respawn");
        StopRespawnCoroutine(changedBehaviour);
    }

    private void StopRespawnCoroutine(CharacterHealthComponent changedBehaviour)
    {
        changedBehaviour.NetworkedHealth = m_baseHealth;
        changedBehaviour.NetworkedIsAlive = true;
        StopCoroutine(RespawnCoroutine);
        RespawnCoroutine = null;
    }
}
