using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCameraDolly : NetworkBehaviour
{
    [SerializeField] private CharacterCamera m_characterCam;
    private Character m_character;
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
    [Networked] public Vector3 NetworkedDollyOffset { get; set; }
    [Networked] public Vector3 NetworkedCameraOffset { get; set; }


    [SerializeField] private bool hitLevel;
    [Header("Layers for Camera LineCast")] public LayerMask RayCastLineCastLayers;
    private bool m_initailized;

    public void Initialize(Character character)
    {
        tr = GetComponent<Transform>();
        m_cameraTr = m_characterCam.transform;
        tr.localPosition = new Vector3(1, 1, -2);
        dollyDir = new Vector3(1, 1, -2).normalized;// tr.localPosition.normalized;
        distance = tr.localPosition.magnitude;
        //playerAnimation.playerCameraView = PlayerCameraView.FirstPerson;

        RestDollyPosition();
        m_cameraTr.localPosition = new Vector3(modXPos, modYPos, modZPos);
        //m_cameraTr.localPosition = new Vector3(-.5f, .375f / 4, -tr.localPosition.z);
        
        m_character = character;
        m_initailized = true;
        m_characterCam.Initialize(character);

    }

    public override void FixedUpdateNetwork()
    {
        if (!m_initailized) return;
        if (m_character.Player && m_character.Player.InputEnabled && GetInput(out InputData data))
        {
            NetworkedCameraOffset = new Vector3(modXPos, modYPos, modZPos);
            NetworkedDollyOffset = dollyDir * distance;
        }
    }

    public override void Render()
    {
        maxDistance = 1 + (100 - m_characterCam.FieldOfView) / 100f;

        if (!m_initailized) return;
        if (!m_character.PlayerInputEnabled()) return;

        if (hitLevel)
        {
            modZPos = 0.3f;
            modXPos = 0;
            modYPos = 0.75f;
        }
        else
        {
            if (m_characterCam.GetCameraRotationY() < 0)
            {
                modYPos = (-m_characterCam.GetCameraRotationY() / m_characterCam.maximumY) / 4 + 0.5f; //Inverse Y offset of 0.5 looking straight to 0.75 looking down
                modZPos = -m_characterCam.GetCameraRotationY() * 1.1f / m_characterCam.maximumY;  //Z offset of +1.1 (Looing up)
            }
            else 
            {
                modYPos = (-m_characterCam.GetCameraRotationY() / m_characterCam.maximumY) + 0.5f; //Inverse Y offset of 0.5 looking straight to -0.5 looking up
                modZPos = m_characterCam.GetCameraRotationY() * 0.3f / m_characterCam.maximumY;  //Z Offset of +0.3 (Looing Down)
            }
            modXPos = -0.2f;
        }

        m_cameraTr.localPosition = Vector3.Lerp(m_cameraTr.localPosition, new Vector3(modXPos, modYPos, modZPos), Time.deltaTime * smooth);

        desiredCameraPos = tr.parent.TransformPoint(dollyDir * maxDistance * 2);
        //Debug.DrawLine(tr.parent.position, desiredCameraPos, Color.magenta);
        if (Physics.Linecast(tr.parent.position, desiredCameraPos, out hit, RayCastLineCastLayers))
        {
            hitLevel = true;
            distance = Mathf.Clamp(hit.distance, minDistance, maxDistance);
            distance -= .75f;
        }
        else
        {
            hitLevel = false;
            distance = maxDistance;
        }


        tr.localPosition = Vector3.Lerp(tr.localPosition, dollyDir * distance, Time.deltaTime * smooth);

        //if (Input.GetKeyDown(KeyCode.Return))
        //    UnityEditor.EditorApplication.isPaused = true;

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
