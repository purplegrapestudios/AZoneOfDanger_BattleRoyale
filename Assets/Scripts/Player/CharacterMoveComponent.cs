using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SlideOnObstacleData
{
    public Vector3 directionVector;
    public bool isHit;
    public float finalSpeed;

    public SlideOnObstacleData(Vector3 directionVector, float finalSpeed, bool isHit) : this()
    {
        this.directionVector = directionVector;
        this.finalSpeed = finalSpeed;
        this.isHit = isHit;
    }

    public SlideOnObstacleData Data
    {
        get
        {
            return new SlideOnObstacleData(directionVector, finalSpeed, isHit);
        }
        set
        {
            directionVector = value.directionVector;
            finalSpeed = value.finalSpeed;
            isHit = value.isHit;
        }
    }
}

public class CharacterMoveComponent : MonoBehaviour
{
    [SerializeField] private float kDamper = .95f;
    [SerializeField] private Vector3 m_directionVector;

    private Vector3 CrossResult;
    private float DotResult;
    private Vector3 CorrectedDirection;
    private Transform HitTransform;
    private Ray directionRay;
    private RaycastHit[] hits;

    public SlideOnObstacleData SlideOnObstacle(Ray ray, float rayDistance, Vector3 directionVector, float currentSpeed, Vector3 upVector)
    {
        m_directionVector = directionVector;
        directionRay = ray;
        hits = new RaycastHit[10];
        if (Physics.RaycastNonAlloc(directionRay, hits, rayDistance) > 0)//, playerCollisionLayers) > 0)
        {
            HitTransform = hits[0].transform;
            if (!HitTransform.CompareTag("Level"))
            {
                return new SlideOnObstacleData(m_directionVector, currentSpeed, false);
            }
            if (HitTransform.CompareTag("Level"))
            {
                CrossResult = Vector3.Cross(hits[0].normal, upVector);
                DotResult = Vector3.Dot(CrossResult, m_directionVector);
                if (DotResult < 0)
                {
                    CorrectedDirection = CrossResult * -1;
                }
                else
                {
                    CorrectedDirection = CrossResult;
                }

                //Do this if absolutely need to stop if corrected direction has an obstacle in its path
                //if (Physics.Raycast(transform.position, CorrectedDirection, rayDistance, playerCollisionLayers))
                //{
                //    speed = 0;
                //}

                //Debug.Log($"Hit Target: {HitTransform.tag}, {HitTransform.transform.name}");


                //We are not Modifying the Rotation here, rather the m_directionVector is used to Calculate Movement Direction. So player would move sideways along wall, if he ran into it.
                //Using state.DirectionVector here, will change and synchronize the values across the network. But we do NOT want it to change in this situation.
                m_directionVector = CorrectedDirection.normalized;
                currentSpeed *= kDamper;
                return new SlideOnObstacleData(m_directionVector, currentSpeed, true);
            }

            return new SlideOnObstacleData(m_directionVector, currentSpeed, false);
        }
        else
        {
            return new SlideOnObstacleData(m_directionVector, currentSpeed, false);
        }
    }

    private Transform groundTransform;
    private RaycastHit[] groundHits;
    public bool GroundCheck(Ray ray)
    {
        groundHits = new RaycastHit[1];
        if (Physics.RaycastNonAlloc(ray, groundHits, transform.localScale.y / 2) > 0)
        {
            if (groundHits[0].transform == this.transform)
            {
                return false;
            }
            return true;
        }
        return false;
    }

}
