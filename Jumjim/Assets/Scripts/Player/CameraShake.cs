using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public float shakeMagnitude = 0.3f;  // Stronger punch
    public float shakeDuration = 0.15f;  // Short and quick

    private Vector3 originalPos;
    private float shakeTimer = 0f;

    void Start()
    {
        originalPos = transform.localPosition;
    }

    void Update()
    {
        if (shakeTimer > 0)
        {
            // Quick jitter using Perlin noise for randomness and punchiness
            float shakeAmountX = (Mathf.PerlinNoise(Time.time * 40f, 0f) - 0.5f) * 2f * shakeMagnitude;
            float shakeAmountY = (Mathf.PerlinNoise(0f, Time.time * 40f) - 0.5f) * 2f * shakeMagnitude;

            transform.localPosition = originalPos + new Vector3(shakeAmountX, shakeAmountY, 0f);

            shakeTimer -= Time.deltaTime;
        }
        else
        {
            shakeTimer = 0f;
            transform.localPosition = originalPos;
        }
    }

    public void Shake()
    {
        shakeTimer = shakeDuration;
    }

    public void Shake(float magnitude, float duration)
    {
        shakeMagnitude = magnitude;
        shakeDuration = duration;
        shakeTimer = shakeDuration;
    }
}
