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
        Instance = this;
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