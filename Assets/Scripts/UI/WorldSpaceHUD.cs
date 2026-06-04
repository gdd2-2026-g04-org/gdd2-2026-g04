using UnityEngine;

namespace GameAssets.UI
{
  public class WorldSpaceHUD : MonoBehaviour
  {
    [Tooltip("Distance in front of the camera")]
    [SerializeField] private float distance = 2f;

    [Tooltip("Vertical offset from camera centre")]
    [SerializeField] private float verticalOffset = -0.3f;

    private Transform cam;

    private void Start()
    {
      cam = Camera.main?.transform;
    }

    private void LateUpdate()
    {
      if (cam == null) return;

      transform.position = cam.position
          + cam.forward * distance
          + Vector3.up * verticalOffset;

      transform.rotation = Quaternion.LookRotation(transform.position - cam.position);
    }
  }
}
