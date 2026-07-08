using System;
using UnityEngine;

public class SmoothHUDFollow : MonoBehaviour
{
    [SerializeField] private Transform vrCamera;

    [SerializeField] private float distance = 1.5f;
    [SerializeField] private Vector3 offset = new(-0.45f, -0.25f, 0f);

    [SerializeField] private float smoothTime = 0.15f;
    [SerializeField] private float smoothSpeed = 8f;

    private Vector3 velocity;

    private void Start()
    {
        vrCamera = Camera.main?.transform;
        if (TryGetComponent(out Canvas c))
        {
            c.worldCamera = Camera.main;
        }
    }

    private void LateUpdate()
    {
        if (vrCamera == null) return;
        
        var flattenedForward = vrCamera.forward;
        flattenedForward.y = 0;
        flattenedForward.Normalize();
        
        var flattenedRight = Vector3.Cross(Vector3.up, flattenedForward).normalized;
        
        var targetPos = vrCamera.position + flattenedForward * distance + flattenedRight * offset.x + vrCamera.up * offset.y;
        
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);

        var targetRot = Quaternion.LookRotation(transform.position - vrCamera.position, Vector3.up);
        
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, smoothSpeed * Time.deltaTime);
    }
}
