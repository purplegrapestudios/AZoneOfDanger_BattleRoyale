using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public enum HitTargets
{
    Nothing,
    Player,
    Environment
}

public class ObjectPoolManager : NetworkBehaviour
{
    public static ObjectPoolManager Instance;

    public GameObject ProjectilePrefab, BulletImpactPrefab, MuzzleFlashPrefab, BloodSplatterPrefab;
    [SerializeField] public List<GameObject> projectileList, bulletImpactList, muzzleFlashList, bloodSplatterList;
    public List<GameObject> liveBulletList;

    [SerializeField] private List<GameObject> tempList;

    public System.Action Callback_ProjectileUpdate;

    public void SubscribeToProjectileUpdate(System.Action callback)
    {
        Callback_ProjectileUpdate += callback;
    }
    public void UnsubscribeFromProjectileUpdate(System.Action callbackToUnsub)
    {
        Callback_ProjectileUpdate -= callbackToUnsub;
    }

    private void Awake()
    {
        Instance = this;
        liveBulletList = new List<GameObject>();
        PoolPrefab(250, ProjectilePrefab, "projectile", out projectileList);
        PoolPrefab(250, BulletImpactPrefab, "impact", out bulletImpactList);
        PoolPrefab(250, MuzzleFlashPrefab, "muzzle", out muzzleFlashList);
        PoolPrefab(250, BloodSplatterPrefab, "blood", out bloodSplatterList);
    }

    public void PoolPrefab(int size, GameObject prefab, string baseName, out List<GameObject> list)
    {
        list = new List<GameObject>();
        for (int i = 0; i < size; i++)
        {
            GameObject obj = (GameObject)Instantiate(prefab, Vector3.zero, Quaternion.identity);
            obj.name = baseName + "_" + i;
            obj.SetActive(false);
            list.Add(obj);
        }
    }

    public List<GameObject> GetFXPrefab(List<GameObject> list, out GameObject obj)
    {
        tempList = list;
        obj = null;
        if (tempList.Count > 0)
        {
            obj = tempList[0];
            liveBulletList.Add(tempList[0]);
            tempList.RemoveAt(0);
            obj.SetActive(true);
            return tempList;
        }
        return tempList;
    }

    public List<GameObject> DestroyFXPrefab(GameObject obj, List<GameObject> list)
    {
        list.Add(obj);
        liveBulletList.Remove(obj);
        if (obj.GetComponent<ParticleSystem>()) obj.GetComponent<ParticleSystem>().Stop();
        obj.SetActive(false);
        return list;
    }

    public void SpawnProjectile(Vector3 startPos, Vector3 endPos, HitTargets hitTarget, PlayerRef ownerRef, Character ownerCharacter, Transform weaponHand, System.Action<float, CharacterHealthComponent> damageCallback)
    {
        if (ProjectilePrefab != null)
        {
            GameObject currentBullet;
            GetFXPrefab(projectileList, out currentBullet);
            if (currentBullet == null)
                return;
            currentBullet.transform.rotation = Quaternion.LookRotation(endPos - startPos);
            currentBullet.transform.position = startPos;
            //currentBullet.transform.parent = weaponHand;
            //currentBullet.GetComponent<ParticleSystem>().Play();
            currentBullet.GetComponent<ProjectileBehavior>().SetRunner(Runner);
            currentBullet.GetComponent<ProjectileBehavior>().SetBulletDirection(endPos - startPos);
            currentBullet.GetComponent<ProjectileBehavior>().SetOwner(ownerRef, ownerCharacter);
            currentBullet.GetComponent<ProjectileBehavior>().SetDamageCallback(damageCallback);
        }
        if (BulletImpactPrefab != null && hitTarget == HitTargets.Environment)
        {
            GameObject currentBulletImpact;
            GetFXPrefab(bulletImpactList, out currentBulletImpact);
            if (currentBulletImpact == null)
                return;
            currentBulletImpact.transform.rotation = Quaternion.LookRotation(endPos - startPos);
            currentBulletImpact.transform.position = endPos;
        }
        if (BloodSplatterPrefab != null && hitTarget == HitTargets.Player)
        {
            GameObject currentBloodSplatter;
            GetFXPrefab(bloodSplatterList, out currentBloodSplatter);
            if (currentBloodSplatter == null)
                return;
            currentBloodSplatter.transform.rotation = Quaternion.LookRotation(endPos - startPos);
            currentBloodSplatter.transform.position = endPos;
        }
    }

    public void SpawnImpact(Vector3 impactPos, Vector3 lookDirection, HitTargets hitTarget)
    {
        if (hitTarget.Equals(HitTargets.Player))
        {
            GameObject currentBloodSplatter;
            GetFXPrefab(bloodSplatterList, out currentBloodSplatter);
            if (currentBloodSplatter == null)
                return;
            currentBloodSplatter.transform.rotation = Quaternion.LookRotation(lookDirection);
            currentBloodSplatter.transform.position = impactPos;
        }
        else if(hitTarget.Equals(HitTargets.Environment))
        {
            if (BulletImpactPrefab == null) return;
            GameObject currentBulletImpact;
            GetFXPrefab(bulletImpactList, out currentBulletImpact);
            if (currentBulletImpact == null)
                return;
            currentBulletImpact.transform.rotation = Quaternion.LookRotation(lookDirection);
            currentBulletImpact.transform.position = impactPos;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Instance == null) return;
        if (!HasStateAuthority) return;

        if (Callback_ProjectileUpdate != null)
            Callback_ProjectileUpdate();
    }
}