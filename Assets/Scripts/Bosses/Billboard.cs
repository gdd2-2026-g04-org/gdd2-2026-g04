
using UnityEngine;

public class Billboard : MonoBehaviour
{
    void Update()
    {
        // This makes the canvas look at the camera
        transform.LookAt(Camera.main.transform.position);
    }
}