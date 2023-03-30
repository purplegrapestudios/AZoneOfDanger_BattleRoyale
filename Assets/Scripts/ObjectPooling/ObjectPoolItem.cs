using UnityEngine;
using System.Collections;

public enum DestroyFXType
{
    Projectile,
    BulletImpact,
    RPGImpact,
    BloodSplatter,
    MuzzleFlash,
}

public class ObjectPoolItem : MonoBehaviour
{
    WaitForSeconds waitforSelfDestructTime;
    public float selfDestructTime = 1.0f;
    public DestroyFXType destroyFXType;

    void Awake()
    {
        waitforSelfDestructTime = new WaitForSeconds(selfDestructTime);
    }

    private void OnEnable()
    {
        if (ObjectPoolManager.Instance)
        {
            if (DestroyFxCO == null)
            {
                DestroyFxCO = DestroyFX_CO();
                StartCoroutine(DestroyFxCO);
            }
            else
            {
                StopCoroutine(DestroyFxCO);
                DestroyFxCO = DestroyFX_CO();
                StartCoroutine(DestroyFxCO);
            }
        }
    }

    IEnumerator DestroyFxCO;
    IEnumerator DestroyFX_CO()
    {
        yield return new WaitForSeconds(selfDestructTime);
        if (destroyFXType == DestroyFXType.Projectile)
        {
            if (GetComponent<ProjectileBehavior>())
            {
                ObjectPoolManager.Instance.UnsubscribeFromProjectileUpdate(GetComponent<ProjectileBehavior>().OnFixedUpdateProjectile);
                ObjectPoolManager.Instance.projectileList = ObjectPoolManager.Instance.DestroyFXPrefab(gameObject, ObjectPoolManager.Instance.projectileList);
            }
        }
        else if (destroyFXType == DestroyFXType.RPGImpact)
            ObjectPoolManager.Instance.rpgImpactList = ObjectPoolManager.Instance.DestroyFXPrefab(gameObject, ObjectPoolManager.Instance.rpgImpactList);
        else if (destroyFXType == DestroyFXType.BulletImpact)
            ObjectPoolManager.Instance.bulletImpactList = ObjectPoolManager.Instance.DestroyFXPrefab(gameObject, ObjectPoolManager.Instance.bulletImpactList);
        else if (destroyFXType == DestroyFXType.MuzzleFlash)
            ObjectPoolManager.Instance.muzzleFlashList = ObjectPoolManager.Instance.DestroyFXPrefab(gameObject, ObjectPoolManager.Instance.muzzleFlashList);
        else if (destroyFXType == DestroyFXType.BloodSplatter)
            ObjectPoolManager.Instance.bloodSplatterList = ObjectPoolManager.Instance.DestroyFXPrefab(gameObject, ObjectPoolManager.Instance.bloodSplatterList);

        DestroyFxCO = null;
    }
}