using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Switches Input System action maps between PC and Touch on startup.
/// Also handles safe area adaptation for iOS notch/dynamic island.
/// </summary>
public class InputRouter : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionAsset inputActions;

    [Header("Action Map Names")]
    public string pcActionMapName    = "GameplayPC";
    public string touchActionMapName = "GameplayTouch";

    [Header("Canvas Safe Area")]
    public RectTransform safeAreaRoot; // root RectTransform to constrain to safe area

    void Awake()
    {
#if UNITY_IOS
        Application.targetFrameRate = 60;
        ActivateMap(touchActionMapName);
        ApplySafeArea();
#else
        ActivateMap(pcActionMapName);
#endif
    }

    void ActivateMap(string mapName)
    {
        if (inputActions == null) return;

        foreach (var map in inputActions.actionMaps)
            map.Disable();

        var target = inputActions.FindActionMap(mapName);
        if (target != null)
        {
            target.Enable();
            Debug.Log($"[InputRouter] Activated action map: {mapName}");
        }
        else
        {
            Debug.LogWarning($"[InputRouter] Action map '{mapName}' not found.");
        }
    }

    void ApplySafeArea()
    {
        if (safeAreaRoot == null) return;

        Rect safeArea  = Screen.safeArea;
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        safeAreaRoot.anchorMin = anchorMin;
        safeAreaRoot.anchorMax = anchorMax;
    }
}
