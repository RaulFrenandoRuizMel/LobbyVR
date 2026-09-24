using UnityEngine;

public class FlickerLight : MonoBehaviour
{
    public Light targetLight;
    public float minIntensity = 0.05f;
    public float maxIntensity = 0.45f;
    public float flickerSpeed = 0.08f;

    private float timer;

    void Start()
    {
        if (targetLight == null)
            targetLight = GetComponent<Light>();

        ResetTimer();
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            if (targetLight != null)
                targetLight.intensity = Random.Range(minIntensity, maxIntensity);

            ResetTimer();
        }
    }

    void ResetTimer()
    {
        timer = Random.Range(flickerSpeed * 0.5f, flickerSpeed * 1.5f);
    }
}