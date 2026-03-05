using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// RMB drag + scroll wheel on macOS; one-finger drag + pinch on iOS.
/// </summary>
public class OrbitCamera : MonoBehaviour
{
    [Header("Orbit")]
    public float orbitSpeed  = 200f;
    public float minPitch    = -20f;
    public float maxPitch    =  80f;

    [Header("Zoom")]
    public float zoomSpeed   = 5f;
    public float minDistance = 2f;
    public float maxDistance = 20f;

    [Header("Target")]
    public Transform pivot;  // CameraRig sets this to midpoint

    private float _yaw;
    private float _pitch = 25f;
    private float _distance = 8f;

    // Input state
    private Vector2 _prevPointer;
    private float   _prevPinchDist;
    private bool    _isDragging;
    private bool    _isPinching;

    void LateUpdate()
    {
        HandleDesktopInput();
        HandleTouchInput();
        ApplyTransform();
    }

    void HandleDesktopInput()
    {
#if !UNITY_IOS
        // RMB drag
        if (Mouse.current != null)
        {
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                _isDragging  = true;
                _prevPointer = Mouse.current.position.ReadValue();
            }
            if (Mouse.current.rightButton.wasReleasedThisFrame)
                _isDragging = false;

            if (_isDragging)
            {
                Vector2 delta = (Vector2)Mouse.current.position.ReadValue() - _prevPointer;
                _yaw   += delta.x * orbitSpeed * Time.deltaTime;
                _pitch -= delta.y * orbitSpeed * Time.deltaTime;
                _pitch  = Mathf.Clamp(_pitch, minPitch, maxPitch);
                _prevPointer = Mouse.current.position.ReadValue();
            }

            // Scroll zoom
            float scroll = Mouse.current.scroll.ReadValue().y;
            _distance -= scroll * zoomSpeed * Time.deltaTime;
            _distance  = Mathf.Clamp(_distance, minDistance, maxDistance);
        }
#endif
    }

    void HandleTouchInput()
    {
#if UNITY_IOS
        var touches = Touchscreen.current;
        if (touches == null) return;

        int touchCount = 0;
        foreach (var t in touches.touches)
            if (t.isInProgress) touchCount++;

        if (touchCount == 1)
        {
            var t0 = touches.touches[0];
            if (t0.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
            {
                _isDragging  = true;
                _prevPointer = t0.position.ReadValue();
            }
            if (t0.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended)
                _isDragging = false;

            if (_isDragging)
            {
                Vector2 delta = t0.position.ReadValue() - _prevPointer;
                _yaw   += delta.x * (orbitSpeed * 0.5f) * Time.deltaTime;
                _pitch -= delta.y * (orbitSpeed * 0.5f) * Time.deltaTime;
                _pitch  = Mathf.Clamp(_pitch, minPitch, maxPitch);
                _prevPointer = t0.position.ReadValue();
            }
        }
        else if (touchCount == 2)
        {
            var t0 = touches.touches[0];
            var t1 = touches.touches[1];
            float pinchDist = Vector2.Distance(t0.position.ReadValue(), t1.position.ReadValue());

            if (!_isPinching) { _prevPinchDist = pinchDist; _isPinching = true; }

            float delta = _prevPinchDist - pinchDist;
            _distance   += delta * zoomSpeed * 0.01f;
            _distance    = Mathf.Clamp(_distance, minDistance, maxDistance);
            _prevPinchDist = pinchDist;
        }
        else
        {
            _isPinching = false;
        }
#endif
    }

    void ApplyTransform()
    {
        if (pivot == null) return;

        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        transform.position  = pivot.position + rotation * (Vector3.back * _distance);
        transform.LookAt(pivot.position);
    }

    public void SetDistance(float d) => _distance = Mathf.Clamp(d, minDistance, maxDistance);
    public float Distance => _distance;
}
