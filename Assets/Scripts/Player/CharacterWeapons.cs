using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Weapon
{
    public int id;
    public GameObject weaponObj;
    public int clipSize;
    public int maxAmmo;
    public int ammoCount;
    public int ammoInClipCount;
    public EAudioClip shotAudio;
    public EAudioClip reloadAudio;
    public EAudioClip reloadEmptyAudio;
    public EAudioClip shotgunOpenAudio;
    public EAudioClip shotgunCloseAudio;


    public Weapon(int id, GameObject weaponObj, int clipSize, int maxAmmo, int ammoCount, int ammoInClipCount, EAudioClip shotAudio, EAudioClip reloadAudio, EAudioClip reloadEmptyAudio, EAudioClip shotgunOpenAudio, EAudioClip shotgunCloseAudio)
    {
        this.id = id;
        this.weaponObj = weaponObj;
        this.clipSize = clipSize;
        this.maxAmmo = maxAmmo;
        this.ammoCount = ammoCount;
        this.ammoInClipCount = ammoInClipCount;
        this.shotAudio = shotAudio;
        this.reloadAudio = reloadAudio;
        this.reloadEmptyAudio = reloadEmptyAudio;
        this.shotgunOpenAudio = shotgunOpenAudio;
        this.shotgunCloseAudio = shotgunCloseAudio;
    }

    public bool ReloadAmmo(bool hasInputAuthority)
    {
        if (!hasInputAuthority) return false;

        if (ammoCount <= 0) return false;

        var ammoInClipConsumed = clipSize - ammoInClipCount;  //For example, 30 clipsize - 0 ammoInClipCount means No ammo is used
        ammoInClipCount = (ammoCount - ammoInClipConsumed >= 0) ? clipSize : ammoCount;
        ammoCount = (ammoCount - ammoInClipConsumed >= 0) ? ammoCount - ammoInClipConsumed : 0;
        GameUIViewController.Instance.SetAmmoInfo(hasInputAuthority, ammoInClipCount, ammoCount, clipSize);
        return true;
    }

    public void ConsumeAmmo(bool hasInputAuthority, int ammoConsumed)
    {
        if (!hasInputAuthority) return;
        ammoInClipCount = (ammoInClipCount - ammoConsumed >= 0) ? ammoInClipCount - ammoConsumed : 0;
        GameUIViewController.Instance.SetAmmoInfo(hasInputAuthority, ammoInClipCount, ammoCount, clipSize);
    }
}

public class CharacterWeapons : MonoBehaviour
{
    [SerializeField] private GameObject m_AR_01;
    [SerializeField] private GameObject m_Shotgun_01;
    [SerializeField] private GameObject m_GL_01;
    public GameObject Weapon_0 => m_AR_01;
    public GameObject Weapon_1 => m_Shotgun_01;
    public GameObject Weapon_2 => m_GL_01;

    private List<Weapon> m_weapons;
    public List<Weapon> Weapons => m_weapons;


    private void Awake()
    {
        m_weapons = new List<Weapon> {
            new Weapon(0, Weapon_0, clipSize: 30, maxAmmo: 999, ammoCount: 200, ammoInClipCount: 30, shotAudio: EAudioClip.FireAR, reloadAudio: EAudioClip.ReloadAR, reloadEmptyAudio: EAudioClip.ReloadAREmpty, EAudioClip.None, EAudioClip.None),
            new Weapon(1, Weapon_1, clipSize: 4, maxAmmo: 500, ammoCount: 50, ammoInClipCount: 4, shotAudio: EAudioClip.FireShotgun, reloadAudio: EAudioClip.ReloadShotgun, reloadEmptyAudio: EAudioClip.None, shotgunOpenAudio: EAudioClip.ShotgunOpen, shotgunCloseAudio: EAudioClip.ShotgunClose),
            new Weapon(2, Weapon_2, clipSize: 10, maxAmmo: 100, ammoCount: 50, ammoInClipCount: 4, shotAudio: EAudioClip.FireShotgun, reloadAudio: EAudioClip.ReloadShotgun, reloadEmptyAudio: EAudioClip.None, shotgunOpenAudio: EAudioClip.ShotgunOpen, shotgunCloseAudio: EAudioClip.ShotgunClose)
        };
    }

    public void SwitchWeapons(int switchToWeaponID)
    {
        for (int i = 0; i < m_weapons.Count; i++)
        {
            m_weapons[i].weaponObj.SetActive(i == switchToWeaponID);
        }
    }
}
