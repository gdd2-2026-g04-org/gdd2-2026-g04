using System;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    private Vector3 direction;
    private float speed;

    public void Initialize(Vector3 flightDir, float flightSpeed, float lifetime)
    {
        direction = flightDir.normalized;
        speed = flightSpeed;
        
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }
}
