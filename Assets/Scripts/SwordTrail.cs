using UnityEngine;

public class SwordTrail : MonoBehaviour
{
    [Header("Trail")]
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private float minSpeedForTrail = 3f;

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip swingSound;
    [SerializeField] private float minSpeedForSound = 4f;
    [SerializeField] private float soundCooldown = 0.4f;

    private Vector3 lastPosition;
    private float currentSpeed;
    private float lastSoundTime;

    private void Start()
    {
        lastPosition = transform.position;
    }

    private void Update()
    {
        if (trailRenderer == null) return;

        // Calculate speed
        currentSpeed = (transform.position - lastPosition).magnitude / Time.deltaTime;
        lastPosition = transform.position;

        // Trail
        trailRenderer.emitting = currentSpeed > minSpeedForTrail;

        // Sound
        if (currentSpeed > minSpeedForSound && Time.time > lastSoundTime + soundCooldown)
        {
            if (audioSource != null && swingSound != null)
            {
                audioSource.PlayOneShot(swingSound);
                lastSoundTime = Time.time;
            }
        }
    }
}