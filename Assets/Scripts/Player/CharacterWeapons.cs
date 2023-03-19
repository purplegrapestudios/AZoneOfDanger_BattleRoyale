using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Weapon: NetworkBehaviour
{
    public int id;
    public GameObject weaponObj;
    public int clipSize;
    public int maxAmmo;
    public int ammoCount;
    public int ammoInClipCount;

    public Weapon(int id, GameObject weaponObj, int clipSize, int maxAmmo, int ammoCount, int ammoInClipCount)
    {
        this.id = id;
        this.weaponObj = weaponObj;
        this.clipSize = clipSize;
        this.maxAmmo = maxAmmo;
        this.ammoCount = ammoCount;
        this.ammoInClipCount = ammoInClipCount;
    }


    public void ConsumeAmmo(int ammoConsumed)
    {
        ammoInClipCount = (ammoInClipCount - ammoConsumed >= 0) ? ammoInClipCount - ammoConsumed : 0;
        GameUIViewController.Instance.SetAmmoInfo(ammoInClipCount, ammoCount, clipSize);
    }
}

public class CharacterWeapons : MonoBehaviour
{
    [SerializeField] private GameObject m_AR_01;
    [SerializeField] private GameObject m_Shotgun_01;
    public GameObject Weapon_0 => m_AR_01;
    public GameObject Weapon_1 => m_Shotgun_01;

    private List<Weapon> m_weapons;
    public List<Weapon> Weapons => m_weapons;


    private void Awake()
    {
        m_weapons = new List<Weapon> {
            new Weapon(0, Weapon_0, clipSize: 30, maxAmmo: 999, ammoCount: 200, ammoInClipCount: 30),
            new Weapon(1, Weapon_1, clipSize: 4, maxAmmo: 500, ammoCount: 50, ammoInClipCount: 4)
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
