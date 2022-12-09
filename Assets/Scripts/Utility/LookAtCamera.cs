using UnityEngine;
using TMPro;

public class LookAtCamera : MonoBehaviour
{
    [SerializeField] private TMP_Text m_nameLabel;

    private void Awake()
    {
        m_nameLabel.gameObject.layer = LayerMask.NameToLayer("TransparentFX");
    }
}
