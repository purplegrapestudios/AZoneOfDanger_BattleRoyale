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
    [Networked(OnChanged = nameof(OnIsRespawnChanged))] public NetworkBool NetworkedRespawn { get; set; }
    public CharacterHealthComponent Instigator => m_instigator;

    [SerializeField] private float m_baseHealth = 100f;
    private Character m_character;
    private GameObject m_characterModel;
    private CharacterHealthComponent m_instigator;
    private bool isInitialized;
    private App m_app;

    public void Initialize(Character character, GameObject characterModel)
    {
        m_app = App.FindInstance();
        m_character = character;
        m_characterModel = characterModel;
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
            NetworkedRespawn = true;
            NetworkedDeaths += 1;

            if (instigator != null)
            {
                instigator.UpdateKillCount();
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!isInitialized) return;
        if (!m_app.AllowInput) return;
        if (!NetworkedIsAlive) return;

        if (m_character.Player && m_character.Player.InputEnabled && GetInput(out InputData data))
        {
            if (data.GetButton(ButtonFlag.RESPAWN))
            {
                NetworkedIsAlive = false;
                NetworkedRespawn = true;
                NetworkedDeaths += 1;
            }
            if (NetworkedRespawn)
            {
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

    static void OnIsRespawnChanged(Changed<CharacterHealthComponent> changed)
    {
        if (changed.Behaviour.NetworkedRespawn)
        {
            changed.Behaviour.Respawn(changed.Behaviour);
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
        {
            changed.Behaviour.Respawn(changed.Behaviour);
        }
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
        Debug.Log($"Instigator updated kill to: {NetworkedKills}");
    }

    private void Respawn(CharacterHealthComponent changedBehaviour)
    {
        EnableSpectateMode(true);
        if (RespawnCoroutine != null) return;
        if (!GameLogicManager.Instance.NetworkedRespawnAllowed)
        {
            if (!NetworkedIsAlive)
                GameLogicManager.Instance.NetworkedPlayerDictionary.Remove(m_character.Player.Object.InputAuthority);
            return;
        }

        RespawnCoroutine = RespawnCO(changedBehaviour);
        StartCoroutine(RespawnCoroutine);
    }

    private IEnumerator RespawnCoroutine;
    private IEnumerator RespawnCO(CharacterHealthComponent changedBehaviour)
    {
        Debug.Log("Waiting to respawn");
        yield return new WaitForSeconds(.5f);
        changedBehaviour.NetworkedRespawn = false;
        yield return new WaitForSeconds(5);

        m_characterModel.SetActive(false);

        if (GameLogicManager.Instance.NetworkedRespawnAllowed)
        {
            GetComponent<NetworkRigidbody>().TeleportToPosition(m_app.Session.Map.GetSpawnPoint(Object.InputAuthority).transform.position);

            yield return new WaitForSeconds(3);

            m_characterModel.SetActive(true);
            NetworkedIsAlive = true;
            Debug.Log("Finish respawn");
            EnableSpectateMode(false);
        }
        StopRespawnCoroutine(changedBehaviour);
        EnableSpectateMode(false);
    }

    private void StopRespawnCoroutine(CharacterHealthComponent changedBehaviour)
    {
        changedBehaviour.NetworkedHealth = m_baseHealth;
        changedBehaviour.NetworkedIsAlive = true;
        StopCoroutine(RespawnCoroutine);
        RespawnCoroutine = null;
    }

    private void EnableSpectateMode(bool isSpectateMode)
    {
        if (Object.InputAuthority != Runner.LocalPlayer) return;

        if (isSpectateMode)
        {
            if (!(GameLogicManager.Instance.NetworkedPlayerDictionary.Count > 0)) return;
            if (SceneCamera.Instance.SpectateCamTr != null) return;

            m_character.SwitchCursorMode(shouldUnlock: true);
            GameUIViewController.Instance.ShowSpectatePlayerOptions(true);
            m_character.CharacterCamera.EnableCameraAndAudioListener(false);

            var playerToSpectate = GameLogicManager.Instance.GetSpectatePlayer(Mathf.RoundToInt(Random.Range(0, GameLogicManager.Instance.NetworkedPlayerDictionary.Count)), m_character.Player);
            SceneCamera.Instance.SetSpectateCamTransform(playerToSpectate.NetworkedCharacter.CharacterCamera.transform, $"{playerToSpectate.Id}");
        }
        else
        {
            m_character.SwitchCursorMode(shouldUnlock: false);
            GameUIViewController.Instance.ShowSpectatePlayerOptions(false);
            m_character.CharacterCamera.EnableCameraAndAudioListener(true);
            SceneCamera.Instance.SetSpectateCamTransform(null, string.Empty);
        }
    }
}