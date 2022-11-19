using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCameraDolly : MonoBehaviour
{
    [SerializeField] private CharacterCamera m_characterCam;
    private Transform m_cameraTr;

    [SerializeField] private float minDistance = 1.0f;
    [SerializeField] private float maxDistance = 4.0f;
    [SerializeField] private float smooth = 10.0f;
    private Vector3 dollyDir;
    private Vector3 dollyDirAdjusted;
    private float distance;

    private Vector3 desiredCameraPos;
    private RaycastHit hit;
    private Transform tr;

    private Ray rayForward;
    private Ray rayLeft;
    private RaycastHit rayHit;

    [SerializeField] private float modZPos;
    [SerializeField] private float modYPos;
    [SerializeField] private float modXPos;

    [SerializeField] private bool hitLevel;
    private bool m_initailized;

    public void Initialize(Character character)
    {
        tr = GetComponent<Transform>();
        m_cameraTr = m_characterCam.transform;
        dollyDir = tr.localPosition.normalized;
        distance = tr.localPosition.magnitude;
        //playerAnimation.playerCameraView = PlayerCameraView.FirstPerson;

        RestDollyPosition();
        m_cameraTr.localPosition = new Vector3(modXPos, 0.75f - tr.localPosition.y - modYPos, modZPos);
        m_cameraTr.localPosition = new Vector3(-.5f, .375f / 4, -tr.localPosition.z);
        
        m_initailized = true;

        m_characterCam.Initialize(character);
    }

    private void LateUpdate()
    {
        if (!m_initailized) return;

        if (hitLevel)
        {
            modZPos = (maxDistance - .5f);
            modXPos = modZPos / 2;
        }
        else
        {
            modZPos = Mathf.Abs(m_characterCam.GetCameraRotationY()) / 90f * (maxDistance - .5f);
            modXPos = modZPos / 2;
        }

        if (m_characterCam.GetCameraRotationY() > 0)
        {
            if (hitLevel)
            {
                modYPos = Mathf.Abs(m_characterCam.GetCameraRotationY()) / 90f * (maxDistance - .5f);
            }
            else
            {
                modYPos = modZPos;
            }
        }
        else
        {
            modYPos = -maxDistance * modZPos / 2;
        }

        m_cameraTr.localPosition = Vector3.Lerp(m_cameraTr.localPosition, new Vector3(modXPos, 0.75f - tr.localPosition.y - modYPos, modZPos), Time.deltaTime * smooth);

        desiredCameraPos = tr.parent.TransformPoint(dollyDir * maxDistance * 2);
        Debug.DrawLine(tr.parent.position, desiredCameraPos, Color.magenta);
        if (Physics.Linecast(tr.parent.position, desiredCameraPos, out hit))
        {
            hitLevel = true;
            distance = Mathf.Clamp(hit.distance, minDistance, maxDistance);
            distance -= .5f;
        }
        else
        {
            hitLevel = false;
            distance = maxDistance;
        }


        tr.localPosition = Vector3.Lerp(tr.localPosition, dollyDir * distance, Time.deltaTime * smooth);

    }

    public void RestDollyPosition()
    {
        distance = maxDistance;
        tr.localPosition = dollyDir * distance;
    }

    public void UpdatePlayerPosition(Vector3 playerPos)
    {
        dollyDir = playerPos;
    }
}
