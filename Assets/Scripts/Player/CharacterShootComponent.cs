using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterShootComponent : NetworkBehaviour
{
    public LayerMask m_damagableLayerMask = ~(1 << 9);  //Do not hit itemLayer
    private Character m_character;
    private CharacterHealthComponent m_characterHealth;
    private CharacterCamera m_characterCamera;
    private CharacterMuzzleComponent m_characterMuzzle;
    private CharacterWeapons m_characterWeapons;
    private ParticleSystem m_muzzleFlash;

    private Weapon m_currentWeapon => m_characterWeapons.Weapons[NetworkedWeaponID];
    [SerializeField] private int m_fireRate = 10;
    [SerializeField] private bool m_isInitialized;
    
    private System.Action<float, CharacterHealthComponent> m_takeDamageCallback;
    private System.Action<EAudioClip> m_audioCallback;
    private System.Action<int, int, int> m_ammoCounterCallback;
    private System.Action<CharacterShootComponent> m_crosshairCallback;
    private System.Action<float> m_aimCallback;
    private TickTimer m_reloadTimer;

    [Networked] public NetworkBool NetworkedHasAmmo { get; set; }
    [Networked] public NetworkBool NetworkedFire { get; set; }
    [Networked] public NetworkBool NetworkedReload { get; set; }
    [Networked] public NetworkBool NetworkedSwitchWeapon { get; set; }
    [Networked] public int NetworkedWeaponID { get; set; }
    [Networked] public int NetworkedCurrWeaponID { get; set; }

    private InputData m_inputData;
    private App m_app;

    public void Initialize(Character character, CharacterHealthComponent characterHealth, CharacterCamera characterCamera, CharacterWeapons characterWeapons, CharacterMuzzleComponent characterMuzzle, ParticleSystem muzzleFlash, System.Action<float, CharacterHealthComponent> damageCallback, System.Action<EAudioClip> audioCallback, System.Action<int, int, int> ammoCounterCallback, System.Action<CharacterShootComponent> crosshairCallback, System.Action<float> aimCallback)
    {
        m_app = App.FindInstance();
        m_character = character;
        m_characterHealth = characterHealth;
        m_characterWeapons = characterWeapons;
        m_muzzleFlash = muzzleFlash;
        m_characterCamera = characterCamera;
        m_characterMuzzle = characterMuzzle;
        m_characterMuzzle.Initialize(m_character, m_muzzleFlash);

        NetworkedHasAmmo = false;
        NetworkedFire = false;
        NetworkedSwitchWeapon = false;

        NetworkedWeaponID = 0;
        NetworkedCurrWeaponID = 0;

        m_takeDamageCallback = damageCallback;
        m_audioCallback = audioCallback;
        m_ammoCounterCallback = ammoCounterCallback;
        m_crosshairCallback = crosshairCallback;
        m_aimCallback = aimCallback;

        m_crosshairCallback(this);

        m_isInitialized = true;
    }

    public override void FixedUpdateNetwork()
    {
        if (!m_app.AllowInput) return;
        if (!m_isInitialized) return;
        if (!m_characterHealth.NetworkedIsAlive) return;
        if (GameLogicManager.Instance.NetworkedGameIsFinished) return;

        if (m_character.PlayerInputEnabled() && GetInput(out InputData data))
        {
            m_inputData = data;
            if (data.GetButton(ButtonFlag.FIRE))
            {
                if (FireCoroutine != null)
                    return;
            
                NetworkedFire = true;
            }
            else
            {
                NetworkedFire = false;
            }

            if (data.GetButton(ButtonFlag.WEAPON_00))
            {
                SwitchWeapon(0);
            }
            else if (data.GetButton(ButtonFlag.WEAPON_01))
            {
                SwitchWeapon(1);
            }
            else if (data.GetButton(ButtonFlag.WEAPON_02))
            {
                SwitchWeapon(2);
            }

            if (data.GetButton(ButtonFlag.AIM))
            {
                m_aimCallback(50f);
            }
            else
            {
                m_aimCallback(70f);
            }

            if (data.GetButton(ButtonFlag.RELOAD))
            {
                NetworkedReload = true;
            }
        }
        //GameUIViewController.Instance.GetCrosshair().ShowDamageCrosshairUpdate(false);
        FireInput();
        SwitchWeaponInput();
        ReloadInput();
    }

    private void FireInput()
    {
        if (NetworkedFire)
        {
            if (FireCoroutine != null) return;
            BeginFireCoroutine();
        }
    }

    private void BeginFireCoroutine()
    {
        if (FireCoroutine != null)
        {
            Debug.Log("Still Firing");
            return; 
        }
        FireCoroutine = FireCO();
        StartCoroutine(FireCoroutine);
    }

    IEnumerator FireCoroutine;
    IEnumerator FireCO()
    {
        if (NetworkedFire)
        {
            if (NetworkedWeaponID == 0)
            {
                FireHitScanWeapon();
                yield return new WaitForSeconds(1f / m_fireRate);
            }
            else if (NetworkedWeaponID == 1)
            {
                FireShotgunSpread();
                yield return new WaitForSeconds(1f);
            }else if (NetworkedWeaponID == 2)
            {
                SpawnProjectile();
                yield return new WaitForSeconds(1f);
            }
        }
        StopFireCoroutine();
    }


    private void StopFireCoroutine()
    {
        //NetworkedFire = false;
        StopCoroutine(FireCoroutine);
        FireCoroutine = null;
    }

    private void SwitchWeapon(int weaponIndex)
    {
        var switchToWeapon = m_characterWeapons.Weapons[weaponIndex];
        NetworkedWeaponID = switchToWeapon.id;

        if (NetworkedCurrWeaponID != NetworkedWeaponID)
        {
            NetworkedSwitchWeapon = true;
        }
    }

    private void SwitchWeaponInput()
    {
        if (!NetworkedSwitchWeapon) return;
        if (SwitchWeaponCoroutine != null) return;
        SwitchWeaponCoroutine = SwitchWeaponCO();
        StartCoroutine(SwitchWeaponCoroutine);
    }

    IEnumerator SwitchWeaponCoroutine;
    IEnumerator SwitchWeaponCO()
    {
        if (NetworkedSwitchWeapon)
        {
            m_characterWeapons.SwitchWeapons(NetworkedWeaponID);
            NetworkedCurrWeaponID = NetworkedWeaponID;
            m_ammoCounterCallback(m_currentWeapon.ammoInClipCount, m_currentWeapon.ammoCount, m_currentWeapon.clipSize);
            NetworkedHasAmmo = m_currentWeapon.ammoCount > 0;
            m_crosshairCallback(this);

            yield return new WaitForSeconds(.5f);
            NetworkedSwitchWeapon = false;
        }
        StopCoroutine(SwitchWeaponCoroutine);
        SwitchWeaponCoroutine = null;
    }

    private void ReloadInput()
    {
        if (!NetworkedReload) return;
        if (ReloadCoroutine != null) return;
        ReloadCoroutine = ReloadCO();
        StartCoroutine(ReloadCoroutine);
    }

    IEnumerator ReloadCoroutine;
    IEnumerator ReloadCO()
    {
        if (NetworkedReload)
        {
            var timeOffset = 0f;
            if (NetworkedWeaponID == 0) m_audioCallback(NetworkedHasAmmo ? m_currentWeapon.reloadAudio : m_currentWeapon.reloadEmptyAudio);
            if (NetworkedWeaponID == 1) {
                m_audioCallback(m_currentWeapon.shotgunOpenAudio);
                timeOffset = 0.2f;
                yield return new WaitForSeconds(timeOffset);
                m_audioCallback(m_currentWeapon.shotgunCloseAudio);
            }

            yield return new WaitForSeconds(m_currentWeapon.reloadTime - timeOffset);
            NetworkedHasAmmo = m_currentWeapon.ReloadAmmo(Object.HasInputAuthority);
            NetworkedReload = false;
        }
        StopCoroutine(ReloadCoroutine);
        ReloadCoroutine = null;
    }

    private void SpawnProjectile()
    {
        var forwardDir = m_character.GetAimDirection();
        var firePosCameraOffset = GetFirePosCameraOffset(1.5f);
        var orig = m_characterCamera.NetworkedPosition + forwardDir * firePosCameraOffset;
        
        ObjectPoolManager.Instance.SpawnProjectile(startPos: orig,
            endPos: forwardDir.normalized,
            hitTarget: HitTargets.Player,
            ownerRef: Object.InputAuthority,
            owner: m_character,
            //muzzlePos: m_character.CharacterMuzzleDolly.NetworkedRenderedShotPosition,
            muzzlePos: m_character.CharacterMuzzle.NetworkedMuzzlePosition,
            damageCallback: m_takeDamageCallback
            );
        
        //ObjectPoolManager.Instance.SpawnProjectile(m_characterCamera.NetworkedPosition + camToMuzzle, m_characterCamera.NetworkedPosition + m_characterCamera.NetworkedForwward * 100, HitTargets.Player, ownerRef: Object.InputAuthority, owner: m_character, muzzlePos: m_character.CharacterMuzzle.NetworkedMuzzlePosition, damageCallback: m_takeDamageCallback);
        m_audioCallback(EAudioClip.FireGL);
    }

    private void FireHitScanWeapon()
    {
        if (!NetworkedFire) return;
        if (m_currentWeapon.ammoInClipCount <= 0) return; // Do Reload Stuff
        m_currentWeapon.ConsumeAmmo(Object.HasInputAuthority, 1);

        var forwardDir = m_character.GetAimDirection();
        var firePosCameraOffset = GetFirePosCameraOffset(1.5f);

        var orig = m_characterCamera.NetworkedPosition + forwardDir * firePosCameraOffset;
        Runner.LagCompensation.Raycast(origin: orig, direction: forwardDir, 100, player: Object.InputAuthority, hit: out var hitInfo, layerMask: m_damagableLayerMask, HitOptions.IgnoreInputAuthority | HitOptions.IncludePhysX);
        //Debug.DrawRay(orig, forwardDir * 100, Color.red, 0.1f);

        float hitDistance = 100;
        if (hitInfo.Distance > 0)
            hitDistance = hitInfo.Distance;
        
        if (hitInfo.Hitbox != null)
        {
            if (hitInfo.Hitbox.Root.GetComponent<Character>().Object.Id != m_character.Object.Id)
            {
                //Debug.Log($"We hit a HitBox Object: {hitInfo.Hitbox.transform.root.name}, Pos: {hitInfo.Point}");
                ObjectPoolManager.Instance.SpawnImpact(hitInfo.Point, hitInfo.Normal, HitTargets.Player);

                if (HasStateAuthority)
                {
                    hitInfo.Hitbox.Root.GetComponent<CharacterShootComponent>().m_takeDamageCallback(hitInfo.Hitbox.HitboxIndex == 0 ? 25 : 50, m_character.CharacterHealth);
                }
            }
        }
        else if (hitInfo.Collider != null)
        {
            //Debug.Log($"We hit a Physx Object: {hitInfo.Collider.transform.name}, Pos: {hitInfo.Point}");
            ObjectPoolManager.Instance.SpawnImpact(hitInfo.Point, hitInfo.Normal, HitTargets.Environment);
        }

        m_audioCallback(EAudioClip.FireAR);
    }


    private void FireShotgunSpread()
    {
        if (!NetworkedFire) return;
        if (m_currentWeapon.ammoInClipCount <= 0) return; // Do Reload Stuff
        m_currentWeapon.ConsumeAmmo(Object.HasInputAuthority, 1);

        var forwardDir = m_character.GetAimDirection();
        var firePosCameraOffset = GetFirePosCameraOffset(1.5f);

        var orig = m_characterCamera.NetworkedPosition + forwardDir * firePosCameraOffset;

        for (int i = 0; i < 10; i++)
        {
            float angleX = Random.Range(-180, 180) * Mathf.Deg2Rad;
            float angleZ = Random.Range(-180, 180) * Mathf.Deg2Rad;
            Vector3 spreadDir = Quaternion.Euler(0f, angleX, angleZ) * forwardDir;
            Runner.LagCompensation.Raycast(origin: orig, direction: spreadDir, 100, player: Object.InputAuthority, hit: out var hitInfo, layerMask: m_damagableLayerMask, HitOptions.IgnoreInputAuthority | HitOptions.IncludePhysX);
            Debug.DrawRay(orig, spreadDir * 100, Color.red, 0.1f);

            float hitDistance = 100;
            if (hitInfo.Distance > 0)
                hitDistance = hitInfo.Distance;

            if (hitInfo.Hitbox != null)
            {
                if (hitInfo.Hitbox.Root.GetComponent<Character>().Object.Id != m_character.Object.Id)
                {

                    //Debug.Log($"We hit a HitBox Object: {hitInfo.Hitbox.transform.root.name}, Pos: {hitInfo.Point}");
                    ObjectPoolManager.Instance.SpawnImpact(hitInfo.Point, hitInfo.Normal, HitTargets.Player);

                    if (HasStateAuthority)
                    {
                        Debug.Log($"{m_character.Player.Name} took {5} damage");
                        if (hitInfo.Hitbox.HitboxIndex == 0)
                            hitInfo.Hitbox.Root.GetComponent<CharacterShootComponent>().m_takeDamageCallback(5, GetComponent<CharacterHealthComponent>());
                        else
                            hitInfo.Hitbox.Root.GetComponent<CharacterShootComponent>().m_takeDamageCallback(15, GetComponent<CharacterHealthComponent>());
                    }
                }
            }
            else if (hitInfo.Collider != null)
            {
                //Debug.Log($"We hit a Physx Object: {hitInfo.Collider.transform.name}, Pos: {hitInfo.Point}");
                ObjectPoolManager.Instance.SpawnImpact(hitInfo.Point, hitInfo.Normal, HitTargets.Environment);
            }

        }
        m_audioCallback(EAudioClip.FireShotgun);
    }

    private float GetFirePosCameraOffset(float minOffsetFactor = 1.5f)
    {
        //This is because the Third Person camera is behind the player, so we need to calculate it's firing posiiton with an offset.
        var distFromCamToMuzzle = Vector3.Distance(m_characterMuzzle.NetworkedMuzzlePosition, m_characterCamera.NetworkedPosition);
        return Mathf.Min(distFromCamToMuzzle, m_character.transform.localScale.x * minOffsetFactor);

    }

}
