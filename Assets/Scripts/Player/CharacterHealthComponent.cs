using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterHealthComponent : NetworkBehaviour
{

    [Networked(OnChanged = nameof(OnHealthChanged))] public float NetworkedHealth { get; set; }
    [Networked(OnChanged = nameof(OnIsAliveChanged))] public bool NetworkedIsAlive { get; set; }

    [SerializeField] private float m_baseHealth = 100f;
    private Character m_character;
    private bool isInitialized;
    private App m_app;

    public void Initialize(Character character)
    {
        m_app = App.FindInstance();
        m_character = character;
        NetworkedHealth = m_baseHealth;
        NetworkedIsAlive = true;
        isInitialized = true;
    }

    public void OnTakeDamage(float damage)
    {
        if (!isInitialized) return;
        if (!NetworkedIsAlive) return;

//        Debug.Log($"{m_character.Player.Name} took {damage} damage");
        NetworkedHealth -= damage;

        if (NetworkedHealth <= 0)
        {
            Debug.Log($"{m_character.Player.Name} couldn't make it");
            NetworkedIsAlive = false;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (m_character.Player && m_character.Player.InputEnabled && GetInput(out InputData data))
        {
            if (data.GetButton(ButtonFlag.RESPAWN))
            {
                Respawn();
            }
        }
    }

    static void OnHealthChanged(Changed<CharacterHealthComponent> changed)
    {
        if (changed.Behaviour.m_character.Object.HasInputAuthority)
        {
            GameUIViewController.Instance.UpdateHealthText(Mathf.RoundToInt(changed.Behaviour.NetworkedHealth).ToString());
        }

    }

    static void OnIsAliveChanged(Changed<CharacterHealthComponent> changed)
    {
        if (!changed.Behaviour.NetworkedIsAlive)
            changed.Behaviour.Respawn();
    }

    private void Respawn()
    {
        if(RespawnCoroutine != null)
            return;

        RespawnCoroutine = RespawnCO();
        StartCoroutine(RespawnCoroutine);
    }

    private IEnumerator RespawnCoroutine;
    private IEnumerator RespawnCO()
    {
        Debug.Log("Waiting to respawn");
        yield return new WaitForSeconds(1);
        GetComponent<NetworkRigidbody>().TeleportToPosition(m_app.Session.Map.GetSpawnPoint(Object.InputAuthority).transform.position);
        NetworkedHealth = m_baseHealth;
        Debug.Log("Finish respawn");
        StopRespawnCoroutine();
    }

    private void StopRespawnCoroutine()
    {
        NetworkedIsAlive = true;
        StopCoroutine(RespawnCoroutine);
        RespawnCoroutine = null;
    }
}
