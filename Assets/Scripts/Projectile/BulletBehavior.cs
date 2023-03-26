using UnityEngine;
using Fusion;
using System.Collections;

public class BulletBehavior : MonoBehaviour
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
    }
    bool ret;
    public void OnBulletFixedUpdate()
    {
        if (ret) return;

        if (m_app ?? null == null)
        {
            Debug.Log("No App");
            ret = true;
        }
        if (m_app?.Session ?? null == null)
        {
            Debug.Log("No Session");
            ret = true;
        }
        if (m_app?.Session?.Runner ?? null == null)
        {
            Debug.Log("No Session Runner");
            ret = true;
        }
        if (m_app?.Session?.Object ?? null == null)
        {
            Debug.Log("No Session Object");
            ret = true;
        }

        if (!m_ownerRef.IsValid) return;

        //Debug.LogWarning($"Bullet {name}, OwnerID: {m_ownerRef.PlayerId}, deltaTime= {m_app.Session.Runner.DeltaTime}");
        ray = new Ray(lastPosition, direction);

        m_app.Session.Runner.LagCompensation.Raycast(origin: lastPosition, direction: direction, transform.localScale.z + bulletSpeed * Time.deltaTime, player: m_app.Session.Object.InputAuthority, hit: out var hitInfo, layerMask: m_damagableLayerMask, HitOptions.IncludePhysX);

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
        ObjectPoolManager.Instance.DestroyFXPrefab(gameObject, ObjectPoolManager.Instance.projectileList);
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
