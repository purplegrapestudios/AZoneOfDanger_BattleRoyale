using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterWeapons : MonoBehaviour
{
    [SerializeField] private GameObject m_AR_01;
    [SerializeField] private GameObject m_Shotgun_01;
    private GameObject m_currWeapon;
    public GameObject Weapon_0 => m_AR_01;
    public GameObject Weapon_1 => m_Shotgun_01;

    private List<GameObject> m_weapons;
    public List<GameObject> Weapons => m_weapons;

    public GameObject CurrWeapon => m_currWeapon;

    private void Awake()
    {
        m_currWeapon = Weapon_0;
        m_weapons = new List<GameObject> { Weapon_0, Weapon_1 };
    }

    public void SwitchWeapons(GameObject current, GameObject switchTo)
    {
        current.SetActive(false);
        switchTo.SetActive(true);
        m_currWeapon = switchTo;
    }
}
