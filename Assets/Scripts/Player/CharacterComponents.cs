using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterComponents : MonoBehaviour
{
    public Transform CameraContainerTransform;
    public GameObject Dolly;
    public GameObject PlayerCamera;
    public GameObject DeathCamera;
    public GameObject ThirdPersonPlayer;
    public GameObject FirstPersonPlayer;
    public GameObject BumperCar;
    public GameObject PlayerLight;
    public GameObject DustPrefab;
    public GameObject MiniMapCammera;

    //public PlayerShooting playerShooting;
    public Animator animator1, animator3;

    private void Awake()
    {
        //Dolly = FindObjectOfType<CharacterCameraDolly>().gameObject;
        //PlayerCamera = FindObjectOfType<CharacterCamera>().gameObject;
        //CameraContainerTransform = Dolly.transform.parent;
    }

}
