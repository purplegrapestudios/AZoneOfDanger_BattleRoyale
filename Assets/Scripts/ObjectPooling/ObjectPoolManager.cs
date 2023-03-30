using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public enum HitTargets
{
    Nothing,
    Player,
    Environment,
    Explosive_1,
}

public class ObjectPoolManager : NetworkBehaviour
{
    public static ObjectPoolManager Instance;

    public GameObject ProjectilePrefab, BulletImpactPrefab, RPGImpactPrefab, MuzzleFlashPrefab, BloodSplatterPrefab;
    public Transform ProjectileContainer, BulletImpactContainer, RPGImpactContainer, MuzzleFlashContainer, BloodSplatterContainer;
    [SerializeField] public List<GameObject> projectileList, bulletImpactList, rpgImpactList, muzzleFlashList, bloodSplatterList;
    public List<GameObject> liveBulletList;

    [SerializeField] private List<GameObject> tempList;

    public System.Action FixedUpdateProjectileCallback;
    public System.Action RenderProjectileCallback;

    public void SubscribeToProjectileUpdate(System.Action callback)
    {
        FixedUpdateProjectileCallback += callback;
    }
    public void UnsubscribeFromProjectileUpdate(System.Action callbackToUnsub)
    {
        FixedUpdateProjectileCallback -= callbackToUnsub;
    }

    private App m_app;

    private void Awake()
    {
        Instance = this;
        m_app = App.FindInstance();
        liveBulletList = new List<GameObject>();
        PoolPrefab(250, ProjectilePrefab, parentContainer: ProjectileContainer, "projectile", out projectileList);
        PoolPrefab(250, BulletImpactPrefab, parentContainer: BulletImpactContainer, "impact", out bulletImpactList);
        PoolPrefab(250, RPGImpactPrefab, parentContainer: RPGImpactContainer, "rpg_impact", out rpgImpactList);
        PoolPrefab(250, MuzzleFlashPrefab, parentContainer: MuzzleFlashContainer, "muzzle", out muzzleFlashList);
        PoolPrefab(250, BloodSplatterPrefab, parentContainer: BloodSplatterContainer, "blood", out bloodSplatterList);
    }

    public void PoolPrefab(int size, GameObject prefab, Transform parentContainer, string baseName, out List<GameObject> list)
    {
        list = new List<GameObject>();
        for (int i = 0; i < size; i++)
        {
            GameObject obj = (GameObject)Instantiate(prefab, Vector3.zero, Quaternion.identity, parent: parentContainer);
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

    public void SpawnProjectile(Vector3 startPos, Vector3 endPos, HitTargets hitTarget, PlayerRef ownerRef, Character owner, Vector3 muzzlePos, System.Action<float, CharacterHealthComponent> damageCallback)
    {
        if (ProjectilePrefab != null)
        {
            GameObject currentBullet;
            GetFXPrefab(projectileList, out currentBullet);
            if (currentBullet == null)
                return;
            currentBullet.transform.rotation = Quaternion.LookRotation(endPos - startPos);
            currentBullet.transform.position = startPos;
            //currentBullet.GetComponent<ParticleSystem>().Play();

            var projectile = currentBullet.GetComponent<ProjectileBehavior>();
            //Debug.DrawRay(startPos, endPos, Color.yellow, 0.1f);
            projectile.SetProjectileData(runner: Runner, instigator: ownerRef, owner, startingTick: Runner.Tick, firePosition: startPos, endPosition: endPos, muzzlePosition: muzzlePos, speed: projectile.StartSpeed);
            if (m_app.IsServerMode() && HasStateAuthority)
            {
                if (projectile.GetComponent<AudioSource>())
                    projectile.GetComponent<AudioSource>().enabled = false;
            }
        }
        //if (BulletImpactPrefab != null && hitTarget == HitTargets.Environment)
        //{
        //    GameObject currentBulletImpact;
        //    GetFXPrefab(bulletImpactList, out currentBulletImpact);
        //    if (currentBulletImpact == null)
        //        return;
        //    currentBulletImpact.transform.rotation = Quaternion.LookRotation(endPos - startPos);
        //    currentBulletImpact.transform.position = endPos;
        //}
        //if (BloodSplatterPrefab != null && hitTarget == HitTargets.Player)
        //{
        //    GameObject currentBloodSplatter;
        //    GetFXPrefab(bloodSplatterList, out currentBloodSplatter);
        //    if (currentBloodSplatter == null)
        //        return;
        //    currentBloodSplatter.transform.rotation = Quaternion.LookRotation(endPos - startPos);
        //    currentBloodSplatter.transform.position = endPos;
        //}
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

            if(m_app.IsServerMode() && HasStateAuthority)
            {
                if (currentBloodSplatter.GetComponent<AudioSource>())
                    currentBloodSplatter.GetComponent<AudioSource>().enabled = false;
            }
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
            if (m_app.IsServerMode() && HasStateAuthority)
            {
                if (currentBulletImpact.GetComponent<AudioSource>())
                    currentBulletImpact.GetComponent<AudioSource>().enabled = false;
            }
        }
        else if (hitTarget.Equals(HitTargets.Explosive_1))
        {
            if (RPGImpactPrefab == null) return;
            GameObject currentRPGImpact;
            GetFXPrefab(rpgImpactList, out currentRPGImpact);
            if (currentRPGImpact == null)
                return;
            currentRPGImpact.transform.rotation = Quaternion.LookRotation(lookDirection);
            currentRPGImpact.transform.position = impactPos;
            if (m_app.IsServerMode() && HasStateAuthority)
            {
                if (currentRPGImpact.GetComponent<AudioSource>())
                    currentRPGImpact.GetComponent<AudioSource>().enabled = false;
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Instance == null) return;

        FixedUpdateProjectileCallback?.Invoke();
    }

    public override void Render()
    {
        if (Instance == null) return;

        RenderProjectileCallback?.Invoke();
    }
}