using UnityEngine;
using Fusion;
using System.Collections;

public class BulletTrailBehavior : MonoBehaviour
{

    Coroutine CoroutineMoveFromTo;
    Transform tr;
    Vector3 lastPosition;
    public LayerMask m_damagableLayerMask = ~(1 << 9);  //Do not hit itemLayer
    Vector3 direction;
    Ray ray;
    RaycastHit[] bulletHits;
    Transform hitTransform;

    public float rayCastDistance = 5f;
    public float bulletSpeed = 10f;

    [SerializeField] private Vector3 targetPosition;

    [SerializeField] private PlayerRef m_ownerRef;

    private IEnumerator m_destroyBulletCoroutine;
    private App m_app;

    // Use this for initialization
    private void OnEnable()
    {
        tr = GetComponent<Transform>();
        lastPosition = tr.position;
        bulletHits = new RaycastHit[255];
        m_app = App.FindInstance();
        secondsElapsed = 0;
        t = 0;
    }

    float t;
    int secondsElapsed;
    public void OnBulletFixedUpdate()
    {
        if (m_app == null) return;
        if (!m_ownerRef.IsValid) return;

        //Debug.LogWarning($"Bullet {name}, OwnerID: {m_ownerRef.PlayerId}, deltaTime= {m_app.Session.Runner.DeltaTime}");
        ray = new Ray(lastPosition, direction);

        m_app.Session.Runner.LagCompensation.Raycast(origin: lastPosition, direction: direction, 100, player: m_app.Session.Object.InputAuthority, hit: out var hitInfo, layerMask: m_damagableLayerMask, HitOptions.IncludePhysX);

        float hitDistance = 100;
        if (hitInfo.Distance > 0)
            hitDistance = hitInfo.Distance;

        if (hitInfo.Hitbox != null)
        {
            Debug.Log($"We hit a HitBox Object: {hitInfo.Collider.transform.name}");
            DestroyBulletTrail();
        }
        else if (hitInfo.Collider != null)
        {
            Debug.Log($"We hit a Physx Object: {hitInfo.Collider.transform.name}");
            DestroyBulletTrail();
        }
        else
        {
            tr.position += direction.normalized * bulletSpeed * m_app.Session.Runner.DeltaTime;
        }
        lastPosition = tr.position;

        //if (Physics.RaycastNonAlloc(ray, bulletHits, rayCastDistance, m_damagableLayerMask) > 0)
        //{
        //    foreach (RaycastHit hit in bulletHits)
        //    {
        //        if (hit.collider == null) continue;
        //
        //        hitTransform = hit.transform;
        //        if (hitTransform.CompareTag("Player"))
        //        {
        //            if (hitTransform.GetComponent<PlayerRef>() == null) continue;
        //
        //            if (hitTransform.GetComponent<PlayerRef>().PlayerId != m_ownerRef.PlayerId)
        //            {
        //                ObjectPoolManager.Instance.UnsubscribeFromProjectileUpdate(OnBulletFixedUpdate);
        //                ObjectPoolManager.Instance.DestroyFXPrefab(gameObject, ObjectPoolManager.Instance.bulletList);
        //                return;
        //            }
        //            else
        //            {
        //                tr.position += direction.normalized * bulletSpeed * m_app.Session.Runner.DeltaTime;
        //            }
        //        }
        //        else
        //        {
        //            ObjectPoolManager.Instance.UnsubscribeFromProjectileUpdate(OnBulletFixedUpdate);
        //            ObjectPoolManager.Instance.DestroyFXPrefab(gameObject, ObjectPoolManager.Instance.bulletList);
        //            return;
        //        }
        //    }
        //}
        //else
        //{
        //    tr.position += direction.normalized * bulletSpeed * m_app.Session.Runner.DeltaTime;
        //}
        //lastPosition = tr.position;
    }

    private void DestroyBulletTrail()
    {
        if (m_destroyBulletCoroutine != null)
        {
            StopCoroutine(m_destroyBulletCoroutine);
            m_destroyBulletCoroutine = null;
        }
        m_destroyBulletCoroutine = DestroyBulletTrailCO(.95f);
        StartCoroutine(m_destroyBulletCoroutine);
    }

    private IEnumerator DestroyBulletTrailCO(float time)
    {
        yield return new WaitForSeconds(time);
        ObjectPoolManager.Instance.UnsubscribeFromProjectileUpdate(OnBulletFixedUpdate);
        ObjectPoolManager.Instance.DestroyFXPrefab(gameObject, ObjectPoolManager.Instance.bulletList);
    }

    public void SetBulletDirection(Vector3 dir)
    {
        direction = dir;
    }

    public void SetOwner(PlayerRef ownerRef)
    {
        m_ownerRef = ownerRef;
        ObjectPoolManager.Instance.SubscribeToProjectileUpdate(OnBulletFixedUpdate);
    }
}
