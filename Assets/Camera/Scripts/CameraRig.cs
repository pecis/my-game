using UnityEngine;

/// <summary>
/// Smoothly follows the midpoint between both fighters.
/// Auto-zooms the OrbitCamera so both fighters stay in frame.
/// </summary>
public class CameraRig : MonoBehaviour
{
    [Header("References")]
    public OrbitCamera     orbitCamera;
    public RagdollController fighter0;
    public RagdollController fighter1;

    [Header("Follow Settings")]
    public float followSpeed     = 5f;
    public float zoomPadding     = 2f;   // extra distance so fighters aren't at screen edge
    public float minAutoDistance = 5f;
    public float maxAutoDistance = 18f;

    void LateUpdate()
    {
        if (fighter0 == null || fighter1 == null) return;

        Vector3 midpoint = (fighter0.GetCenterOfMass() + fighter1.GetCenterOfMass()) * 0.5f;
        transform.position = Vector3.Lerp(transform.position, midpoint, followSpeed * Time.deltaTime);

        // Auto-zoom: base distance on separation between fighters
        float separation = Vector3.Distance(fighter0.GetCenterOfMass(), fighter1.GetCenterOfMass());
        float targetDist  = Mathf.Clamp(separation + zoomPadding, minAutoDistance, maxAutoDistance);

        if (orbitCamera != null)
            orbitCamera.SetDistance(Mathf.Lerp(orbitCamera.Distance, targetDist, followSpeed * Time.deltaTime));
    }
}
