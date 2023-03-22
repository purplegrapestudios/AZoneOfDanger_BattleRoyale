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
    
    [SerializeField] private int m_fireRate = 10;
    [SerializeField] private bool m_isInitialized;
    
    private System.Action<float, CharacterHealthComponent> m_takeDamageCallback;
    private System.Action<EAudioClip> m_audioCallback;
    private System.Action<int, int, int> m_ammoCounterCallback;
    private System.Action<CharacterShootComponent> m_crosshairCallback;

    [Networked] public NetworkBool NetworkedHasAmmo { get; set; }
    [Networked] public NetworkBool NetworkedFire { get; set; }
    [Networked] public NetworkBool NetworkedReload { get; set; }
    [Networked] public NetworkBool NetworkedSwitchWeapon { get; set; }
    [Networked] public int NetworkedWeaponID { get; set; }
    [Networked] public int NetworkedCurrWeaponID { get; set; }

    private InputData m_inputData;
    private App m_app;

    public void Initialize(Character character, CharacterHealthComponent characterHealth, CharacterCamera characterCamera, CharacterWeapons characterWeapons, CharacterMuzzleComponent characterMuzzle, ParticleSystem muzzleFlash, System.Action<float, CharacterHealthComponent> damageCallback, System.Action<EAudioClip> audioCallback, System.Action<int, int, int> ammoCounterCallback, System.Action<CharacterShootComponent> crosshairCallback)
    {
        m_app = App.FindInstance();
        m_character = character;
        m_characterHealth = characterHealth;
        m_characterWeapons = characterWeapons;
        m_muzzleFlash = muzzleFlash;
        m_characterCamera = characterCamera;
        m_characterMuzzle = characterMuzzle;
        m_characterMuzzle.Initialize(this, m_muzzleFlash);

        NetworkedHasAmmo = false;
        NetworkedFire = false;
        NetworkedSwitchWeapon = false;

        NetworkedWeaponID = 0;
        NetworkedCurrWeaponID = 0;

        m_takeDamageCallback = damageCallback;
        m_audioCallback = audioCallback;
        m_ammoCounterCallback = ammoCounterCallback;
        m_crosshairCallback = crosshairCallback;

        m_crosshairCallback(this);

        m_isInitialized = true;
    }

    public override void FixedUpdateNetwork()
    {
        if (!m_isInitialized) return;
        if (!m_characterHealth.NetworkedIsAlive) return;

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
            else
            {
                NetworkedSwitchWeapon = false;
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
        if (NetworkedSwitchWeapon)
        {
            if (SwitchWeaponCoroutine != null) return;
            BeginSwitchWeaponCoroutine();
        }
    }

    private void BeginSwitchWeaponCoroutine()
    {
        if (SwitchWeaponCoroutine != null)
        {
            Debug.Log("Still Switching Weapon");
            return;
        }
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
            m_ammoCounterCallback(m_characterWeapons.Weapons[NetworkedWeaponID].ammoInClipCount, m_characterWeapons.Weapons[NetworkedWeaponID].ammoCount, m_characterWeapons.Weapons[NetworkedWeaponID].clipSize);
            NetworkedHasAmmo = m_characterWeapons.Weapons[NetworkedWeaponID].ammoCount > 0;
            m_crosshairCallback(this);

            yield return new WaitForSeconds(.5f);
        }
        StopCoroutine(SwitchWeaponCoroutine);
        SwitchWeaponCoroutine = null;
    }

    private void ReloadInput()
    {
        if (ReloadCoroutine != null)
        {
            Debug.Log("Still Reloading");
            return;
        }
        ReloadCoroutine = ReloadCO();
        StartCoroutine(ReloadCoroutine);
    }

    IEnumerator ReloadCoroutine;
    IEnumerator ReloadCO()
    {
        if (NetworkedReload)
        {
            if(NetworkedWeaponID == 0) m_audioCallback(NetworkedHasAmmo ? m_characterWeapons.Weapons[NetworkedWeaponID].reloadAudio : m_characterWeapons.Weapons[NetworkedWeaponID].reloadEmptyAudio);
            if (NetworkedWeaponID == 1) {
                m_audioCallback(m_characterWeapons.Weapons[NetworkedWeaponID].shotgunOpenAudio);
                yield return new WaitForSeconds(0.2f);
                m_audioCallback(m_characterWeapons.Weapons[NetworkedWeaponID].shotgunCloseAudio);
            }

            yield return new WaitForSeconds(0.5f);
            NetworkedHasAmmo = m_characterWeapons.Weapons[NetworkedWeaponID].ReloadAmmo(Object.HasInputAuthority);
            NetworkedReload = false;
        }
        StopCoroutine(ReloadCoroutine);
        ReloadCoroutine = null;
    }

    private void SpawnProjectile()
    {
        ObjectPoolManager.Instance.SpawnProjectile(m_muzzleFlash.transform.position + m_characterCamera.transform.forward, transform.position + m_characterCamera.transform.forward * 100, HitTargets.Player, Runner.LocalPlayer, m_muzzleFlash.transform);
    }

    private void FireHitScanWeapon()
    {
        if (!NetworkedFire) return;
        if (m_characterWeapons.Weapons[NetworkedWeaponID].ammoInClipCount <= 0) return; // Do Reload Stuff
        m_characterWeapons.Weapons[NetworkedWeaponID].ConsumeAmmo(Object.HasInputAuthority, 1);

        var rot = m_character.GetComponent<NetworkRigidbody>().ReadRotation() * Quaternion.AngleAxis(m_characterCamera.NetworkedRotationY, Vector3.left);
        var dir = rot * Vector3.forward;
        var distFromCamToMuzzle = Vector3.Distance(m_characterMuzzle.transform.position, m_characterCamera.NetworkedPosition);
        distFromCamToMuzzle = Mathf.Min(distFromCamToMuzzle, m_character.transform.localScale.x * 1.5f);
        var orig = m_characterCamera.NetworkedPosition + dir * distFromCamToMuzzle;
        Runner.LagCompensation.Raycast(origin: orig, direction: dir, 100, player: Object.InputAuthority, hit: out var hitInfo, layerMask: m_damagableLayerMask, HitOptions.IncludePhysX);
        Debug.DrawRay(orig, dir * 100, Color.red, 0.1f);

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
                        hitInfo.Hitbox.Root.GetComponent<CharacterShootComponent>().m_takeDamageCallback(25, GetComponent<CharacterHealthComponent>());
                    else
                        hitInfo.Hitbox.Root.GetComponent<CharacterShootComponent>().m_takeDamageCallback(50, GetComponent<CharacterHealthComponent>());
                }

                //Change localPlayer's crosshair only
                if (m_character.Object.HasInputAuthority)
                    GameUIViewController.Instance.GetCrosshair().ShowDamageCrosshair();

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
        if (m_characterWeapons.Weapons[NetworkedWeaponID].ammoInClipCount <= 0) return; // Do Reload Stuff
        m_characterWeapons.Weapons[NetworkedWeaponID].ConsumeAmmo(Object.HasInputAuthority, 1);

        var rot = m_character.GetComponent<NetworkRigidbody>().ReadRotation() * Quaternion.AngleAxis(m_characterCamera.NetworkedRotationY, Vector3.left);
        var dir = rot * Vector3.forward;
        var distFromCamToMuzzle = Vector3.Distance(m_characterMuzzle.transform.position, m_characterCamera.NetworkedPosition);
        distFromCamToMuzzle = Mathf.Min(distFromCamToMuzzle, m_character.transform.localScale.x * 1.5f);
        var orig = m_characterCamera.NetworkedPosition + dir * distFromCamToMuzzle;

        for (int i=0;i< 10; i++)
        {
            float angleX = Random.Range(-180, 180) * Mathf.Deg2Rad;
            float angleZ = Random.Range(-180, 180) * Mathf.Deg2Rad;
            Vector3 spreadDir = Quaternion.Euler(0f, angleX, angleZ) * dir;
            Runner.LagCompensation.Raycast(origin: orig, direction: spreadDir, 100, player: Object.InputAuthority, hit: out var hitInfo, layerMask: m_damagableLayerMask, HitOptions.IncludePhysX);
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

                    //Change localPlayer's crosshair only
                    if (m_character.Object.HasInputAuthority)
                        GameUIViewController.Instance.GetCrosshair().ShowDamageCrosshair();

                }
            }
            else if (hitInfo.Collider != null)
            {
                //Debug.Log($"We hit a Physx Object: {hitInfo.Collider.transform.name}, Pos: {hitInfo.Point}");
                ObjectPoolManager.Instance.SpawnImpact(hitInfo.Point, hitInfo.Normal, HitTargets.Environment);
            }

        }
            m_audioCallback(EAudioClip.FireShotgun);
        //m_fireWeaponAudioCallback(EAudioClip.FireShotgun);


    }
}