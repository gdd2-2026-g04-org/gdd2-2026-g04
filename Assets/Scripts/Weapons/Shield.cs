using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Shield : MonoBehaviour
{
    [Header("Raised Settings")]
    [SerializeField, Min(0f)] private float raiseThreshold = .2f;

    public bool isHeld;
    public bool isRaised;
    
    private Transform playerHead;

    private void OnEnable()
    {
        isHeld = true;
        CheckHead();
    }

    private void OnDisable()
    {
        isHeld = false;
        isRaised = false;
    }

    private void Update()
    {
        if (!playerHead)
        {
            CheckHead();

            if (!playerHead)
            {
                isRaised = false;
                return;
            }
        }

        isRaised = transform.position.y >= playerHead.position.y - raiseThreshold;
    }

    private void CheckHead()
    {
        playerHead = XRReferences.Instance != null ? XRReferences.Instance.head : null;
    }
}