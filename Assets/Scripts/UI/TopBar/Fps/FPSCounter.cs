using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] private TMP_Text m_fpsText;
    private float deltaTime;

    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        m_fpsText.text = "FPS: " + Mathf.RoundToInt(fps);
    }
}