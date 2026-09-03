using System;
using UnityEngine;

public class XRReferences : MonoBehaviour
{
    public static XRReferences Instance { get; private set; }

    [Header("XR References")]
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    [SerializeField] private GameObject locomotionGameObject;

    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void ResetTransform()
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.Euler(Vector3.zero);
    }

    public void EnableMovement()
    {
        locomotionGameObject?.SetActive(true);
    }
    
    public void DisableMovement()
    {
        locomotionGameObject?.SetActive(false);
    }
}