using UnityEngine;

public class SmoothBillboard : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField, Min(0f)] private float rotationSpeed = 12f;

    [SerializeField] private float pitchOffset = 0f;
    private void LateUpdate()
    {
        if (!target)
        {
            if (XRReferences.Instance)
                target = XRReferences.Instance.head;

            if (!target && Camera.main)
                target = Camera.main.transform;

            if (!target)
                return;
        }

        var dir = target.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude <= 0.0001f)
            return;

        var targetRotation =
            Quaternion.LookRotation(dir, Vector3.up);

        targetRotation *= Quaternion.Euler(pitchOffset, 0f, 0f);

        var t = 1f - Mathf.Exp(-rotationSpeed * Time.deltaTime);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            t
        );
    }
}