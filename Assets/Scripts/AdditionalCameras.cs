using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AdditionalCameras : MonoBehaviour
{
    static readonly int GROUPS_COUNT = 3;
    static readonly int CAMERAS_PER_GROUP = 8;

    static readonly int ColorProperty = Shader.PropertyToID("_Color");
    Material indicatorMaterial;
    MaterialPropertyBlock propertyBlock;

    AdditionalCamerasConfig cfg;

    GameObject camWrapper;
    Camera camL;
    Camera camR;

    public Transform selectionTool;
    public int selectionFlystickIdx = 0;
    public LzwpInput.Flystick.ButtonID selectionFlystickButton = LzwpInput.Flystick.ButtonID.Joystick;
    bool selecting = false;
    int lastHoveredIdx = -1;
    Collider lastRaycastHit = null;

    List<GameObject> indicators = new List<GameObject>();
    List<Renderer> indicatorRenderers = new List<Renderer>();
    bool indicatorsVisible = false;
    public Color disabledIndicatorColor = Color.red;
    public Color hoveredIndicatorColor = Color.yellow;
    public Color enabledIndicatorColor = Color.green;
    [LayerField] public int indicatorLayer;

    int currentGroupIdx = -1;
    int currentCameraInGroupIdx = -1;

    [Header("Key bindings")]
    public KeyCode keyToggleIndicators = KeyCode.C;
    public KeyCode keyToggleIndicatorsAlt = KeyCode.Keypad5;

    public KeyCode keyGroupPrev = KeyCode.DownArrow;
    public KeyCode keyGroupNext = KeyCode.UpArrow;
    public KeyCode keyCamPrev = KeyCode.LeftArrow;
    public KeyCode keyCamNext = KeyCode.RightArrow;
    //public bool stopOnLastGroup = true;

    [Header("Cameras in group")]
    public KeyCode keyCam0 = KeyCode.Alpha1;
    public KeyCode keyCam1 = KeyCode.Alpha2;
    public KeyCode keyCam2 = KeyCode.Alpha3;
    public KeyCode keyCam3 = KeyCode.Alpha4;
    public KeyCode keyCam4 = KeyCode.Alpha5;
    public KeyCode keyCam5 = KeyCode.Alpha6;
    public KeyCode keyCam6 = KeyCode.Alpha7;
    public KeyCode keyCam7 = KeyCode.Alpha8;

    [Space]
    public KeyCode keyCam0Alt = KeyCode.Keypad1;
    public KeyCode keyCam1Alt = KeyCode.Keypad4;
    public KeyCode keyCam2Alt = KeyCode.Keypad7;
    public KeyCode keyCam3Alt = KeyCode.Keypad8;
    public KeyCode keyCam4Alt = KeyCode.Keypad9;
    public KeyCode keyCam5Alt = KeyCode.Keypad6;
    public KeyCode keyCam6Alt = KeyCode.Keypad3;
    public KeyCode keyCam7Alt = KeyCode.Keypad2;

    [Header("Adjustment")]
    public KeyCode keyAdjustHorizontalMinus = KeyCode.F5;
    public KeyCode keyAdjustHorizontalPlus = KeyCode.F6;
    public KeyCode keyAdjustVerticalMinus = KeyCode.F7;
    public KeyCode keyAdjustVerticalPlus = KeyCode.F8;
    public KeyCode keyAdjustRollLeft = KeyCode.F9;
    public KeyCode keyAdjustRollRight = KeyCode.F10;
    public KeyCode keyAdjustFovMinus = KeyCode.F3;
    public KeyCode keyAdjustFovPlus = KeyCode.F4;
    public KeyCode keyAdjustmentSave = KeyCode.F2;
    public KeyCode keyAdjustmentStealViewport = KeyCode.F1;
    public float adjustmentAmountDeg = 1f;
    public float adjustmentAmountFovDeg = 1f;
    bool viewportStolen = false;
    static readonly float STOLEN_VIEWPORT_MARGIN = 0.01f;
    public Rect audViewport = new Rect(0.75f, 0.33333333f, 0.25f, 0.33333333f);
    //public Rect caveViewport = new Rect(0.0f, 0.66666666f, 0.25f, 0.33333333f);
    public Rect caveViewport = new Rect(STOLEN_VIEWPORT_MARGIN, 0.66666666f + STOLEN_VIEWPORT_MARGIN, 0.25f - 2f * STOLEN_VIEWPORT_MARGIN, 0.33333333f - 2f * STOLEN_VIEWPORT_MARGIN);

    void Reset()
    {
        var tool = FindObjectsOfType<TrackedObject>()
                .Where(x => x.trackedType == TrackedObject.TrackedObjectType.Flystick)
                .OrderBy(x => x.idx)
                .FirstOrDefault();
        if (tool)
            selectionTool = tool.transform;
    }

    void Start()
    {
        propertyBlock = new MaterialPropertyBlock();

        indicatorMaterial = new Material(Shader.Find("Standard"));

        indicatorMaterial.SetFloat("_Mode", 2); // 2 - Fade, 3 - Transparent
        indicatorMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        indicatorMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        indicatorMaterial.SetInt("_ZWrite", 0);

        indicatorMaterial.DisableKeyword("_ALPHATEST_ON");
        indicatorMaterial.EnableKeyword("_ALPHABLEND_ON");
        indicatorMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");

        indicatorMaterial.renderQueue = 3000;

        Debug.Log("[AdditionalCameras] Config: " + AdditionalCamerasConfig.GetPath());
        cfg = AdditionalCamerasConfig.Load();

        camWrapper = new GameObject("AdditionalCameras");
        camWrapper.transform.SetParent(LzwpOrigin.Instance.transform);

        var camLObj = new GameObject("EyeLeft");
        camLObj.transform.SetParent(camWrapper.transform);

        var camRObj = new GameObject("EyeRight");
        camRObj.transform.SetParent(camWrapper.transform);

        camL = camLObj.AddComponent<Camera>();
        camR = camRObj.AddComponent<Camera>();

        camL.clearFlags = CameraClearFlags.SolidColor;
        camL.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
        camL.nearClipPlane = 0.1f;
        camL.farClipPlane = 10f;
        camL.rect = new Rect(0.75f, 0.33333333f, 0.25f, 0.33333333f);
        camL.useOcclusionCulling = false;
        camL.allowHDR = false;
        camL.allowMSAA = false;
        camL.allowDynamicResolution = false;
        camL.stereoTargetEye = StereoTargetEyeMask.Left;
        camL.stereoSeparation = cfg.useCustomStereoHandling ? 0 : cfg.stereoSeparation;
        camL.stereoConvergence = cfg.stereoConvergence;

        camR.clearFlags = CameraClearFlags.SolidColor;
        camR.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
        camR.nearClipPlane = 0.1f;
        camR.farClipPlane = 10f;
        camR.rect = new Rect(0.75f, 0.33333333f, 0.25f, 0.33333333f);
        camR.useOcclusionCulling = false;
        camR.allowHDR = false;
        camR.allowMSAA = false;
        camR.allowDynamicResolution = false;
        camR.stereoTargetEye = StereoTargetEyeMask.Right;
        camR.stereoSeparation = cfg.useCustomStereoHandling ? 0 : cfg.stereoSeparation;
        camR.stereoConvergence = cfg.stereoConvergence;



        if (cfg.useCustomStereoHandling)
        {
            camLObj.transform.localPosition = Vector3.right * cfg.stereoSeparation / (cfg.swapEyes ? 2f : -2f);
            camRObj.transform.localPosition = Vector3.right * cfg.stereoSeparation / (cfg.swapEyes ? -2f : 2f);
            UpdateCameras();
        }

        SpawnIndicators();
        SetGroupAndCamera(cfg.initialGroupIdx, cfg.initialCameraInGroupIdx);
    }

    void Update()
    {
        if (Input.GetKeyDown(keyToggleIndicators) || Input.GetKeyDown(keyToggleIndicatorsAlt)) ToggleIndicatorsVisibility();

        if (Input.GetKeyDown(keyGroupPrev)) GoToPrevGroup();
        if (Input.GetKeyDown(keyGroupNext)) GoToNextGroup();
        if (Input.GetKeyDown(keyCamPrev)) GoToPrevCam();
        if (Input.GetKeyDown(keyCamNext)) GoToNextCam();

        if (Input.GetKeyDown(keyCam0) || Input.GetKeyDown(keyCam0Alt)) SetCameraInGroup(0);
        if (Input.GetKeyDown(keyCam1) || Input.GetKeyDown(keyCam1Alt)) SetCameraInGroup(1);
        if (Input.GetKeyDown(keyCam2) || Input.GetKeyDown(keyCam2Alt)) SetCameraInGroup(2);
        if (Input.GetKeyDown(keyCam3) || Input.GetKeyDown(keyCam3Alt)) SetCameraInGroup(3);
        if (Input.GetKeyDown(keyCam4) || Input.GetKeyDown(keyCam4Alt)) SetCameraInGroup(4);
        if (Input.GetKeyDown(keyCam5) || Input.GetKeyDown(keyCam5Alt)) SetCameraInGroup(5);
        if (Input.GetKeyDown(keyCam6) || Input.GetKeyDown(keyCam6Alt)) SetCameraInGroup(6);
        if (Input.GetKeyDown(keyCam7) || Input.GetKeyDown(keyCam7Alt)) SetCameraInGroup(7);

        if (Input.GetKeyDown(keyAdjustHorizontalMinus)) AdjustAngle(-adjustmentAmountDeg, 0, 0);
        if (Input.GetKeyDown(keyAdjustHorizontalPlus)) AdjustAngle(adjustmentAmountDeg, 0, 0);
        if (Input.GetKeyDown(keyAdjustVerticalMinus)) AdjustAngle(0, -adjustmentAmountDeg, 0);
        if (Input.GetKeyDown(keyAdjustVerticalPlus)) AdjustAngle(0, adjustmentAmountDeg, 0);
        if (Input.GetKeyDown(keyAdjustRollLeft)) AdjustAngle(0, 0, -adjustmentAmountDeg);
        if (Input.GetKeyDown(keyAdjustRollRight)) AdjustAngle(0, 0, adjustmentAmountDeg);

        if (Input.GetKeyDown(keyAdjustFovMinus)) AdjustFov(-adjustmentAmountFovDeg);
        if (Input.GetKeyDown(keyAdjustFovPlus)) AdjustFov(adjustmentAmountFovDeg);

        if (Input.GetKeyDown(keyAdjustmentSave)) cfg.Save();
        if (Input.GetKeyDown(keyAdjustmentStealViewport)) ToggleViewportStealing();

        if (Lzwp.input.flysticks.Count > selectionFlystickIdx)
            Lzwp.input.flysticks[selectionFlystickIdx].GetButton(selectionFlystickButton).OnPress += SelectionToolButtonPressed;

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.X)) SelectionToolButtonPressed();
#endif

        UpdateRaycast();
    }

    void UpdateCameras()
    {
        if (!cfg.useCustomStereoHandling)
            return;

        Vector3 scrCentre = (camWrapper.transform.rotation * Vector3.forward) * cfg.stereoConvergence + camWrapper.transform.position;
        Vector3 scrNormal = camWrapper.transform.rotation * Vector3.back;
        Vector3 scrUp = camWrapper.transform.rotation * Vector3.up;

        Plane plane = new Plane(scrNormal, scrCentre);

        // LEFT EYE

        Vector3 eyePos = camL.transform.position;

        float viewDistance = Mathf.Abs(plane.GetDistanceToPoint(eyePos));
        if (viewDistance == 0)
            viewDistance = 0.001f;

        camL.transform.LookAt(eyePos - scrNormal, scrUp);

        eyePos = Quaternion.Inverse(camL.transform.rotation) * (eyePos - scrCentre);

        float ratio = camL.nearClipPlane / viewDistance;

        camL.SetStereoProjectionMatrix(cfg.swapEyes ? Camera.StereoscopicEye.Right : Camera.StereoscopicEye.Left, Matrix4x4.Frustum(
            (-eyePos.x - 0.5f * cfg.screenWidth) * ratio,
            (-eyePos.x + 0.5f * cfg.screenWidth) * ratio,
            (-eyePos.y - 0.5f * cfg.screenHeight) * ratio,
            (-eyePos.y + 0.5f * cfg.screenHeight) * ratio,
            camL.nearClipPlane,
            camL.farClipPlane
        ));

        // RIGHT EYE

        eyePos = camR.transform.position;

        viewDistance = Mathf.Abs(plane.GetDistanceToPoint(eyePos));
        if (viewDistance == 0)
            viewDistance = 0.001f;

        camR.transform.LookAt(eyePos - scrNormal, scrUp);

        eyePos = Quaternion.Inverse(camR.transform.rotation) * (eyePos - scrCentre);

        ratio = camR.nearClipPlane / viewDistance;

        camR.SetStereoProjectionMatrix(cfg.swapEyes ? Camera.StereoscopicEye.Left : Camera.StereoscopicEye.Right, Matrix4x4.Frustum(
            (-eyePos.x - 0.5f * cfg.screenWidth) * ratio,
            (-eyePos.x + 0.5f * cfg.screenWidth) * ratio,
            (-eyePos.y - 0.5f * cfg.screenHeight) * ratio,
            (-eyePos.y + 0.5f * cfg.screenHeight) * ratio,
            camR.nearClipPlane,
            camR.farClipPlane
        ));
    }

    void SpawnIndicators()
    {
        propertyBlock.SetColor(ColorProperty, disabledIndicatorColor);

        for (int i = 0; i < cfg.cameras.Length; i++)
        {
            var camCfg = cfg.cameras[i];

            var indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            indicator.name = "Indicator_" + i + "_" + camCfg.label;
            indicator.transform.SetParent(LzwpOrigin.Instance.transform);
            indicator.transform.localPosition = camCfg.position;
            indicator.transform.localScale = Vector3.one * 0.4f;

            indicator.layer = indicatorLayer;
            indicator.SetActive(false);
            indicators.Add(indicator);

            indicator.GetComponent<SphereCollider>().radius = 0.75f;

            var renderer = indicator.GetComponent<Renderer>();
            renderer.sharedMaterial = indicatorMaterial;
            renderer.SetPropertyBlock(propertyBlock);
            indicatorRenderers.Add(renderer);

            var label = new GameObject("Label (" + camCfg.label + ")");
            label.transform.SetParent(indicator.transform);
            label.transform.localPosition = Vector3.zero;
            label.transform.localRotation = Quaternion.Euler(0, 180f, 0);

            var textMesh = label.AddComponent<TextMesh>();
            textMesh.text = camCfg.label;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;

            textMesh.characterSize = 0.02f;
            textMesh.fontSize = 100;
            textMesh.color = Color.black;
            textMesh.offsetZ = -0.01f;

            indicator.transform.LookAt(LzwpOrigin.GetPosition() + Vector3.up * 1.7f);
            indicator.transform.Rotate(camCfg.rotationCorrection, Space.Self);
        }
    }

    void ToggleViewportStealing()
    {
        if (!cfg.viewportStealingEnabled)
            return;

        viewportStolen = !viewportStolen;

        camL.rect = viewportStolen ? caveViewport : audViewport;
        camR.rect = camL.rect;
    }

    int GroupCamToIdx(int g, int c)
    {
        return g * CAMERAS_PER_GROUP + c;
    }

    int GetCurrentIdx()
    {
        return GroupCamToIdx(currentGroupIdx, currentCameraInGroupIdx);
    }

    void SetIndicatorColor(int idx, Color color)
    {
        propertyBlock.Clear();
        propertyBlock.SetColor(ColorProperty, color);
        indicatorRenderers[idx].SetPropertyBlock(propertyBlock);
    }

    void UpdateIndicatorColor(int idx)
    {
        if (idx < 0)
            return;

        if (idx == lastHoveredIdx)
            SetIndicatorColor(idx, hoveredIndicatorColor);

        SetIndicatorColor(idx, idx == GetCurrentIdx() ? enabledIndicatorColor : disabledIndicatorColor);
    }

    void SetIndicatorsVisibility(bool visible)
    {
        if (indicatorsVisible == visible)
            return;

        foreach (var ind in indicators)
            ind.SetActive(visible);

        indicatorsVisible = visible;
    }

    void ToggleIndicatorsVisibility()
    {
        SetIndicatorsVisibility(!indicatorsVisible);

        if (!indicatorsVisible && selecting)
        {
            selecting = false;

            if (lastHoveredIdx > -1)
            {
                int idx = lastHoveredIdx;
                lastHoveredIdx = -1;
                lastRaycastHit = null;
                UpdateIndicatorColor(idx);
            }
        }
    }

    void SetHoveredIndicator(int hIdx = -1)
    {
        if (lastHoveredIdx == hIdx)
            return;

        if (lastHoveredIdx > -1)
            SetIndicatorColor(lastHoveredIdx, GetCurrentIdx() == lastHoveredIdx ? enabledIndicatorColor : disabledIndicatorColor);

        if (hIdx > -1)
            SetIndicatorColor(hIdx, hoveredIndicatorColor);

        lastHoveredIdx = hIdx;
    }

    void GoToPrevGroup()
    {
        if (currentGroupIdx > 0)
            SetGroup(currentGroupIdx - 1);
        else if (!cfg.stopOnLastGroup)
            SetGroup(GROUPS_COUNT - 1);
    }

    void GoToNextGroup()
    {
        if (currentGroupIdx < GROUPS_COUNT - 1)
            SetGroup(currentGroupIdx + 1);
        else if (!cfg.stopOnLastGroup)
            SetGroup(0);
    }

    void GoToPrevCam()
    {
        SetCameraInGroup(currentCameraInGroupIdx > 0 ? currentCameraInGroupIdx - 1 : CAMERAS_PER_GROUP - 1);
    }

    void GoToNextCam()
    {
        SetCameraInGroup(currentCameraInGroupIdx >= CAMERAS_PER_GROUP - 1 ? 0 : currentCameraInGroupIdx + 1);
    }

    void SetGroup(int gIdx)
    {
        SetGroupAndCamera(gIdx, currentCameraInGroupIdx);
    }

    void SetCameraInGroup(int cIdx)
    {
        SetGroupAndCamera(currentGroupIdx, cIdx);
    }

    void SetGroupAndCamera(int gIdx, int cIdx)
    {
        if (currentGroupIdx == gIdx && currentCameraInGroupIdx == cIdx)
            return;

        int oldIdx = GroupCamToIdx(currentGroupIdx, currentCameraInGroupIdx);
        int idx = GroupCamToIdx(gIdx, cIdx);
        currentGroupIdx = gIdx;
        currentCameraInGroupIdx = cIdx;

        Debug.Log("[Additional cameras] Set GROUP <b>" + gIdx + "</b> and CAMERA <b>" + cIdx + "</b> --> " + cfg.cameras[idx].label);

        if (oldIdx >= 0)
            SetIndicatorColor(oldIdx, oldIdx != lastHoveredIdx ? disabledIndicatorColor : hoveredIndicatorColor);

        SetIndicatorColor(idx, idx != lastHoveredIdx ? enabledIndicatorColor : hoveredIndicatorColor);


        camWrapper.transform.SetPositionAndRotation(indicators[idx].transform.position, indicators[idx].transform.rotation);
        camL.fieldOfView = cfg.cameras[idx].fov;
        camR.fieldOfView = cfg.cameras[idx].fov;

        UpdateCameras();
    }

    void SelectionToolButtonPressed()
    {
        if (selecting)
        {
            SetIndicatorsVisibility(false);
            selecting = false;


            if (lastHoveredIdx > -1)
            {
                int g = lastHoveredIdx / CAMERAS_PER_GROUP;
                int c = lastHoveredIdx % CAMERAS_PER_GROUP;
                lastHoveredIdx = -1;
                lastRaycastHit = null;
                SetGroupAndCamera(g, c);
            }
        }
        else
        {
            SetIndicatorsVisibility(true);
            selecting = true;
        }
    }

    void UpdateRaycast()
    {
        if (!selecting || !selectionTool)
            return;

        RaycastHit hit;

        if (Physics.Raycast(selectionTool.position, selectionTool.forward, out hit, 7f, 1 << indicatorLayer))
        {
            if (hit.collider == lastRaycastHit)
                return;

            lastRaycastHit = hit.collider;

            var hitObj = lastRaycastHit.gameObject;

            for (int i = 0; i < indicators.Count; i++)
                if (indicators[i] == hitObj)
                {
                    SetHoveredIndicator(i);
                    return;
                }

            SetHoveredIndicator();
        }
        else if (lastRaycastHit)
        {
            lastRaycastHit = null;
            SetHoveredIndicator();
        }
    }

    void AdjustAngle(float hDelta, float vDelta, float rollDelta)
    {
        if (!cfg.adjustingEnabled)
            return;

        var corr = cfg.cameras[GetCurrentIdx()].rotationCorrection;
        corr.x += vDelta;
        corr.y += hDelta;
        corr.z -= rollDelta;

        var indicatorTr = indicators[GetCurrentIdx()].transform;
        indicatorTr.LookAt(LzwpOrigin.GetPosition() + Vector3.up * 1.7f);
        indicatorTr.Rotate(corr, Space.Self);

        camWrapper.transform.rotation = indicatorTr.rotation;

        cfg.cameras[GetCurrentIdx()].rotationCorrection = corr;

        UpdateCameras();
    }

    void AdjustFov(float delta)
    {
        if (!cfg.adjustingEnabled)
            return;

        float fov = Mathf.Clamp(camL.fieldOfView + delta, 1f, 179f);
        camL.fieldOfView = fov;
        camR.fieldOfView = fov;
        cfg.cameras[GetCurrentIdx()].fov = fov;
    }
}

[Serializable]
public class AdditionalCamerasConfig
{
    [Serializable]
    public class CameraConfig
    {
        public int group = 0;
        public int id = 0;
        public string label = "";
        public float fov = 60f;

        [JsonConverter(typeof(LzwpUtils.Converters.Vector3Converter))]
        public Vector3 position = Vector3.zero;

        [JsonConverter(typeof(LzwpUtils.Converters.Vector3Converter))]
        public Vector3 rotationCorrection = Vector3.zero;

    }

    public int initialGroupIdx = 0;
    public int initialCameraInGroupIdx = 0;
    public bool stopOnLastGroup = true;
    public bool adjustingEnabled = false;
    public bool viewportStealingEnabled = false;

    public float stereoSeparation = 0.05f;
    public float stereoConvergence = 1.5f;

    public bool useCustomStereoHandling = false;
    public bool swapEyes = false;
    public float screenWidth = 1.6f;
    public float screenHeight = 0.9f;

    public CameraConfig[] cameras = new[] {
        new CameraConfig(),
        new CameraConfig() { id = 1 },
        new CameraConfig() { group = 1 },
        new CameraConfig() { group = 1, id = 1 }
     };

    public static string GetPath()
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, "../AdditionalCameras.json"));
    }

    public static AdditionalCamerasConfig Load()
    {
        string path = GetPath();

        return File.Exists(path)
            ? JsonConvert.DeserializeObject<AdditionalCamerasConfig>(File.ReadAllText(path))
            : new AdditionalCamerasConfig();
    }

    public void Save()
    {
        File.WriteAllText(GetPath(), JsonConvert.SerializeObject(this, Formatting.Indented));
        Debug.Log("[Additional cameras] Config SAVED");
    }
}

public class LayerFieldAttribute : PropertyAttribute { }

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(LayerFieldAttribute))]
public class LayerFieldDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.Integer)
        {
            EditorGUI.LabelField(position, label.text, "LayerField can only be used with int.");
            return;
        }

        property.intValue = EditorGUI.LayerField(position, label, property.intValue);
    }
}

#endif
