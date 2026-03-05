using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// One-click scene builder.
/// Menu: Tools → Setup Fight Scene
/// </summary>
public static class SceneSetup
{
    [MenuItem("Tools/Setup Fight Scene")]
    public static void SetupFightScene()
    {
        // Load ScriptableObject assets
        var ragdollCfg    = AssetDatabase.LoadAssetAtPath<RagdollConfig>   ("Assets/ScriptableObjects/Data/RagdollConfig.asset");
        var jointStateCfg = AssetDatabase.LoadAssetAtPath<JointStateConfig>("Assets/ScriptableObjects/Data/JointStateConfig.asset");
        var matchRules    = AssetDatabase.LoadAssetAtPath<MatchRules>      ("Assets/ScriptableObjects/Data/MatchRules.asset");

        if (ragdollCfg == null || jointStateCfg == null || matchRules == null)
        {
            bool create = EditorUtility.DisplayDialog(
                "Missing ScriptableObjects",
                "RagdollConfig, JointStateConfig, or MatchRules assets not found in Assets/ScriptableObjects/Data/.\n\nCreate them now?",
                "Yes – Create", "Cancel");

            if (!create) return;
            CreateScriptableObjects(out ragdollCfg, out jointStateCfg, out matchRules);
        }

        // Ensure Ground tag exists
        EnsureTag("Ground");

        // ── Ground ──────────────────────────────────────────────
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.tag  = "Ground";
        ground.transform.localScale = new Vector3(5f, 1f, 5f);

        // ── Fighters ────────────────────────────────────────────
        var f0go = BuildFighter("Fighter0", new Vector3(-1.5f, 1f, 0f), 0, ragdollCfg, jointStateCfg);
        var f1go = BuildFighter("Fighter1", new Vector3( 1.5f, 1f, 0f), 1, ragdollCfg, jointStateCfg);

        var f0ctrl = f0go.GetComponent<RagdollController>();
        var f1ctrl = f1go.GetComponent<RagdollController>();

        // ── Managers ────────────────────────────────────────────
        var managers = new GameObject("Managers");

        var evalSys = managers.AddComponent<EvaluationSystem>();
        evalSys.rules = matchRules;

        var scoreMgr = managers.AddComponent<ScoreManager>();
        scoreMgr.rules = matchRules;

        var simRunner = managers.AddComponent<SimulationRunner>();
        simRunner.rules = matchRules;

        var turnMgr = managers.AddComponent<TurnManager>();
        turnMgr.fighter0         = f0ctrl;
        turnMgr.fighter1         = f1ctrl;
        turnMgr.simulationRunner = simRunner;
        turnMgr.evaluationSystem = evalSys;
        turnMgr.scoreManager     = scoreMgr;
        turnMgr.rules            = matchRules;

        var bootstrap = managers.AddComponent<GameBootstrap>();
        bootstrap.fighter0Builder = f0go.GetComponent<RagdollBuilder>();
        bootstrap.fighter1Builder = f1go.GetComponent<RagdollBuilder>();
        bootstrap.turnManager     = turnMgr;

        // ── Camera ──────────────────────────────────────────────
        var rigGo = new GameObject("CameraRig");
        rigGo.transform.position = new Vector3(0f, 1.5f, 0f);
        var cameraRig = rigGo.AddComponent<CameraRig>();
        cameraRig.fighter0 = f0ctrl;
        cameraRig.fighter1 = f1ctrl;

        var camGo = new GameObject("Main Camera");
        camGo.transform.SetParent(rigGo.transform);
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.clearFlags       = CameraClearFlags.Skybox;
        cam.fieldOfView      = 60f;
        cam.transform.position = new Vector3(0f, 2f, -8f);
        camGo.AddComponent<AudioListener>();

        var orbitCam = camGo.AddComponent<OrbitCamera>();
        orbitCam.pivot = rigGo.transform;
        cameraRig.orbitCamera = orbitCam;

        // ── Delete default Main Camera if it exists ─────────────
        var defaultCam = GameObject.FindWithTag("MainCamera");
        if (defaultCam != null && defaultCam != camGo)
            Object.DestroyImmediate(defaultCam);

        // ── Basic HUD Canvas ─────────────────────────────────────
        BuildHUD(turnMgr, scoreMgr);

        // ── Directional Light ───────────────────────────────────
        if (Object.FindFirstObjectByType<Light>() == null)
        {
            var lightGo = new GameObject("Directional Light");
            var light   = lightGo.AddComponent<Light>();
            light.type  = LightType.Directional;
            light.intensity = 1f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        Debug.Log("[SceneSetup] Fight scene built successfully. Hit Play to test.");
        EditorUtility.DisplayDialog("Done!", "Fight scene created.\n\nHit Play — check the Console for:\n[RagdollBuilder] Built fighter P0 with 17 body parts.", "OK");
    }

    // ──────────────────────────────────────────────────────────
    //  Helpers
    // ──────────────────────────────────────────────────────────

    static GameObject BuildFighter(string name, Vector3 pos, int playerIndex, RagdollConfig rc, JointStateConfig jsc)
    {
        var go = new GameObject(name);
        go.transform.position = pos;

        var builder = go.AddComponent<RagdollBuilder>();
        builder.config          = rc;
        builder.jointStateConfig = jsc;
        builder.playerIndex     = playerIndex;

        go.AddComponent<RagdollController>();
        return go;
    }

    static void BuildHUD(TurnManager turnMgr, ScoreManager scoreMgr)
    {
        // Canvas
        var canvasGo = new GameObject("HUD Canvas");
        var canvas   = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();

        // Phase label
        var phaseLabel = CreateLabel(canvasGo.transform, "PhaseLabel", "Select Moves",
            new Vector2(0.5f, 1f), new Vector2(0, -30), 24);

        // Timer label
        var timerLabel = CreateLabel(canvasGo.transform, "TimerLabel", "30.0s",
            new Vector2(0.5f, 1f), new Vector2(0, -60), 20);

        // Turn label
        var turnLabel = CreateLabel(canvasGo.transform, "TurnLabel", "Turn 0",
            new Vector2(0.5f, 1f), new Vector2(0, -85), 16);

        // Score labels
        var p0Score = CreateLabel(canvasGo.transform, "P0Score", "P1  0",
            new Vector2(0f, 1f), new Vector2(80, -40), 20);
        var p1Score = CreateLabel(canvasGo.transform, "P1Score", "P2  0",
            new Vector2(1f, 1f), new Vector2(-80, -40), 20);

        // HUD controller
        var hud = canvasGo.AddComponent<HUDController>();
        hud.lblPhase   = phaseLabel;
        hud.lblTimer   = timerLabel;
        hud.lblTurn    = turnLabel;
        hud.lblP0Score = p0Score;
        hud.lblP1Score = p1Score;
        hud.turnManager  = turnMgr;
        hud.scoreManager = scoreMgr;
    }

    static TextMeshProUGUI CreateLabel(Transform parent, string name, string text,
        Vector2 anchor, Vector2 anchoredPos, int fontSize)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt       = go.AddComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot     = anchor;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta        = new Vector2(300, 40);

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;
        return tmp;
    }

    static void CreateScriptableObjects(out RagdollConfig rc, out JointStateConfig jsc, out MatchRules mr)
    {
        System.IO.Directory.CreateDirectory("Assets/ScriptableObjects/Data");
        AssetDatabase.Refresh();

        rc = ScriptableObject.CreateInstance<RagdollConfig>();
        AssetDatabase.CreateAsset(rc, "Assets/ScriptableObjects/Data/RagdollConfig.asset");

        jsc = ScriptableObject.CreateInstance<JointStateConfig>();
        AssetDatabase.CreateAsset(jsc, "Assets/ScriptableObjects/Data/JointStateConfig.asset");

        mr = ScriptableObject.CreateInstance<MatchRules>();
        AssetDatabase.CreateAsset(mr, "Assets/ScriptableObjects/Data/MatchRules.asset");

        AssetDatabase.SaveAssets();
    }

    static void EnsureTag(string tag)
    {
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var tags       = tagManager.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;

        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }
}
