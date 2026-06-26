using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ParallaxTest : MonoBehaviour
{
    public Camera camera;
    private Vector3 startPosition;
    private float startSize;
    public Vector3 endPosition;
    public float endSize = 6;

    public float speed = 1;

    protected bool zoomOut;
    private Vector3 clamp = Vector3.one * 0.001f;
    public bool activatePerlin;
    // Start is called before the first frame update
    void Start()
    {
        startPosition = camera.transform.position;
        startSize = camera.orthographicSize;

        originalPosition = transform.localPosition;
        seedX = Random.Range(0f, 100f);
        seedY = Random.Range(100f, 200f);
    }

    // Update is called once per frame
    void Update()
    {
        if(Keyboard.current.enterKey.wasPressedThisFrame)
            zoomOut = !zoomOut;

        if(activatePerlin)
        {
            if(zoomOut)
            {
                originalPosition = Vector3.LerpUnclamped(originalPosition, endPosition, Time.deltaTime * speed);
                camera.orthographicSize = Mathf.Clamp(Mathf.Lerp(camera.orthographicSize, endSize + 0.001f, Time.deltaTime * speed), 0, endSize);
            }
            else
            {
                originalPosition = Vector3.LerpUnclamped(originalPosition, startPosition, Time.deltaTime * speed);
                camera.orthographicSize = Mathf.Clamp(Mathf.Lerp(camera.orthographicSize, startSize - 0.001f, Time.deltaTime * speed), startSize, Mathf.Infinity);
            }

            // 1. Calculate standard smooth idle/handheld movement
            float idleX = (Mathf.PerlinNoise(Time.time * idleSpeed + seedX, 0f) - 0.5f) * 2f * idleMagnitude;
            float idleY = (Mathf.PerlinNoise(0f, Time.time * idleSpeed + seedY) - 0.5f) * 2f * idleMagnitude;
            Vector3 finalOffset = new Vector3(idleX, idleY, 0f);

            // 2. Handle temporary impact shake (e.g., from explosions)
            if (currentShakeTime > 0)
            {
                currentShakeTime -= Time.deltaTime;

                // Sample high frequency noise
                float shakeX = (Mathf.PerlinNoise(Time.time * shakeSpeed, seedX) - 0.5f) * 2f * shakeMagnitude;
                float shakeY = (Mathf.PerlinNoise(seedY, Time.time * shakeSpeed) - 0.5f) * 2f * shakeMagnitude;

                // Fade out the shake intensity over its remaining duration
                float fadeProgress = currentShakeTime / shakeDuration;
                finalOffset += new Vector3(shakeX, shakeY, 0f) * fadeProgress;
            }

            // 3. Apply changes relative to the starting position
            transform.localPosition = originalPosition + finalOffset;
        }
        else
        {
            if(zoomOut)
            {
                camera.transform.position = Vector3.LerpUnclamped(camera.transform.position, endPosition, EaseFunctions.InOutSine(Time.deltaTime * speed));
                // camera.transform.position = MathUtils.LerpClamp(Vector3.LerpUnclamped(camera.transform.position, endPosition, EaseFunctions.InOutSine(Time.deltaTime * speed)), startPosition, endPosition);
                camera.orthographicSize = Mathf.Clamp(Mathf.Lerp(camera.orthographicSize, endSize + 0.001f, EaseFunctions.InOutSine(Time.deltaTime * speed)), endSize > startSize ? 0 : endSize, endSize > startSize ? endSize : Mathf.Infinity);
            }
            else
            {
                camera.transform.position = Vector3.LerpUnclamped(camera.transform.position, startPosition, EaseFunctions.InOutSine(Time.deltaTime * speed));
                // camera.transform.position = MathUtils.LerpClamp(Vector3.LerpUnclamped(camera.transform.position, startPosition, EaseFunctions.InOutSine(Time.deltaTime * speed)), endPosition, startPosition);
                camera.orthographicSize = Mathf.Clamp(Mathf.Lerp(camera.orthographicSize, startSize - 0.001f, EaseFunctions.InOutSine(Time.deltaTime * speed)), endSize > startSize ? startSize : 0, endSize > startSize ? Mathf.Infinity : startSize);
            }
        }
        
    }

    Vector3 NormalizeClamp(Vector3 baseVector, Vector3 multiplier)
    {
        Vector3 output = Vector3.zero;
        output.x = baseVector.x == 0 ? 0 : baseVector.x > 0 ? 1 : -1;
        output.y = baseVector.y == 0 ? 0 : baseVector.y > 0 ? 1 : -1;
        output.z = baseVector.z == 0 ? 0 : baseVector.z > 0 ? 1 : -1;

        return Vector3.Scale(output, multiplier);
    }
    
    [Header("Idle Movement (Continuous)")]
    public float idleSpeed = 1f;
    public float idleMagnitude = 0.1f;

    [Header("Impact Shake settings")]
    public float shakeDuration = 0f;
    public float shakeSpeed = 15f;
    public float shakeMagnitude = 0.5f;

    private Vector3 originalPosition;
    private float currentShakeTime;
    
    // Seed offsets to ensure X and Y sample different parts of the infinite noise map
    private float seedX;
    private float seedY;

    // Call this public function from other scripts to trigger an explosion/hit shake
    public void TriggerShake(float duration, float magnitude)
    {
        shakeDuration = duration;
        currentShakeTime = duration;
        shakeMagnitude = magnitude;
    }
}
