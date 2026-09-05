using System;
using UnityEngine;

public class BowString : MonoBehaviour
{
    [SerializeField] private BowController bowController;
    [SerializeField] private LineRenderer lineRenderer;

    [SerializeField] private Transform stringTop;
    [SerializeField] private Transform stringMiddle;
    [SerializeField] private Transform stringBottom;

    [SerializeField] private Transform followTarget;
    [SerializeField] private float maxPull = 0.35f;
    [SerializeField] private float smoothSpeed = 20f;

    private Vector3 restPosition;

    private void Awake()
    {
        if (stringMiddle) restPosition = stringMiddle.localPosition;
        if (lineRenderer)
        {
            lineRenderer.positionCount = 3;
            lineRenderer.useWorldSpace = true;
        }
    }

    private void LateUpdate()
    {
        if (!lineRenderer || !stringTop || !stringMiddle || !stringBottom || !bowController) return;
        
        UpdateMiddlePoint();
        UpdateString();
    }

    private void UpdateMiddlePoint()
    {
        Vector3 targetPosition;

        var isDrawing = bowController.State is BowState.Drawing or BowState.Ready;

        if (isDrawing)
        {
            if (followTarget)
            {
                targetPosition = followTarget.position;
            }
            else
            {
                var pull = bowController.Tension * maxPull;
                targetPosition = transform.TransformPoint(restPosition + Vector3.back * pull);
            }
        }
        else
        {
            targetPosition = transform.TransformPoint(restPosition);
        }
        
        var t = 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime);
        stringMiddle.position = Vector3.Lerp(stringMiddle.position, targetPosition, t);
    }

    private void UpdateString()
    {
        lineRenderer.SetPosition(0, stringTop.position);
        lineRenderer.SetPosition(1, stringMiddle.position);
        lineRenderer.SetPosition(2, stringBottom.position);
    }
}
