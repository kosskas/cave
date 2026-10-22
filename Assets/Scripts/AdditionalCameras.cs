using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

// Unity 2018 / C# 6.0 / Built-in Render Pipeline.
// Camera browsing is available on PC without scene setup or LZWP dependencies.
[DefaultExecutionOrder(-100)]
public class AdditionalCameras : MonoBehaviour
{
    private const int GroupsCount = 3;
    private const int CamerasPerGroup = 8;
    private const int TotalCameras = GroupsCount * CamerasPerGroup;

    public static AdditionalCameras Instance { get; private set; }

    // Read this from other scripts to disable normal player controls.
    public bool IsCameraBrowsingMode { get; private set; }

    public event Action<bool> CameraBrowsingModeChanged;

    [Header("Camera adjustment")]
    public float adjustmentAmountDeg = 1f;
    public float adjustmentAmountFovDeg = 1f;

    [Header("PC preview")]
    public Color cameraBackgroundColor = new Color(0.3f, 0.3f, 0.3f);

    private AdditionalCamerasConfig cfg;
    private Camera[] cameras;
    private int currentCameraIdx = -1;
    private GUIStyle cameraNumberStyle;

    // Create the manager automatically after the scene loads.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (FindObjectOfType<AdditionalCameras>() != null)
            return;

        var root = new GameObject("PC_Additional_Cameras");
        root.AddComponent<AdditionalCameras>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        cfg = AdditionalCamerasConfig.Load();

        bool invalidLayout = cfg.cameras == null || cfg.cameras.Length != TotalCameras;
        if (!invalidLayout)
        {
            foreach (var cameraConfig in cfg.cameras)
            {
                if (cameraConfig == null)
                {
                    invalidLayout = true;
                    break;
                }
            }
        }

        if (invalidLayout)
        {
            Debug.LogWarning("[AdditionalCameras] Invalid camera layout; using default positions.", this);
            cfg.cameras = AdditionalCamerasConfig.CreateDefaultCameras(cfg.sceneCenter);
        }

        cameras = new Camera[TotalCameras];
        CreateCameras();

        SetGroupAndCamera(
            Mathf.Clamp(cfg.initialGroupIdx, 0, GroupsCount - 1),
            Mathf.Clamp(cfg.initialCameraInGroupIdx, 0, CamerasPerGroup - 1));

        // The normal PC view is displayed until C is pressed.
        IsCameraBrowsingMode = false;
        Debug.Log("[AdditionalCameras] 24 cameras ready. Config: " + AdditionalCamerasConfig.GetPath(), this);
    }

    private void CreateCameras()
    {
        var cameraRoot = new GameObject("Cameras").transform;
        cameraRoot.SetParent(transform, false);

        for (int i = 0; i < TotalCameras; i++)
        {
            var cameraConfig = cfg.cameras[i];
            var label = string.IsNullOrEmpty(cameraConfig.label)
                ? (i + 1).ToString()
                : cameraConfig.label;

            var cameraObject = new GameObject("Camera_" + label);
            cameraObject.transform.SetParent(cameraRoot, false);
            cameraObject.transform.position = cameraConfig.position;
            cameraObject.transform.rotation = GetCameraRotation(cameraConfig);

            var cameraComponent = cameraObject.AddComponent<Camera>();
            cameraComponent.enabled = false;
            cameraComponent.rect = new Rect(0f, 0f, 1f, 1f);
            cameraComponent.fieldOfView = Mathf.Clamp(cameraConfig.fov, 1f, 179f);
            cameraComponent.clearFlags = CameraClearFlags.SolidColor;
            cameraComponent.backgroundColor = cameraBackgroundColor;
            cameraComponent.nearClipPlane = 0.03f;
            cameraComponent.farClipPlane = 100f;
            cameraComponent.depth = 100f;
            cameraComponent.stereoTargetEye = StereoTargetEyeMask.None;
            cameraComponent.useOcclusionCulling = false;

            cameras[i] = cameraComponent;
        }
    }

    private Quaternion GetCameraRotation(AdditionalCamerasConfig.CameraConfig cameraConfig)
    {
        var direction = cfg.sceneCenter - cameraConfig.position;
        if (direction.sqrMagnitude < 1e-10f)
            return Quaternion.identity;

        return Quaternion.LookRotation(direction.normalized, Vector3.up)
               * Quaternion.Euler(cameraConfig.rotationCorrection);
    }

    private void Update()
    {
        if (cameras == null)
            return;

        bool ctrlPressed = Input.GetKey(KeyCode.LeftControl) ||
                           Input.GetKey(KeyCode.RightControl);

        // C is the only browsing shortcut available outside browsing mode.
        if (!ctrlPressed && Input.GetKeyDown(KeyCode.C))
        {
            SetCameraBrowsingMode(!IsCameraBrowsingMode);
            return;
        }

        // Do not intercept the application's normal controls outside browsing mode.
        if (!IsCameraBrowsingMode)
            return;

        // Save changes only in browsing mode.
        if (ctrlPressed)
        {
            if (Input.GetKeyDown(KeyCode.S))
                cfg.Save();
            return;
        }

        // Browse the three groups and eight cameras in each group.
        if (Input.GetKeyDown(KeyCode.DownArrow)) ChangeGroup(-1);
        if (Input.GetKeyDown(KeyCode.UpArrow)) ChangeGroup(1);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) ChangeCamera(-1);
        if (Input.GetKeyDown(KeyCode.RightArrow)) ChangeCamera(1);

        if (!cfg.adjustingEnabled)
            return;

        // 1-2: FOV, 3-4: horizontal, 5-6: vertical, 7-8: roll.
        if (Input.GetKeyDown(KeyCode.Alpha1)) AdjustFov(-adjustmentAmountFovDeg);
        if (Input.GetKeyDown(KeyCode.Alpha2)) AdjustFov(adjustmentAmountFovDeg);
        if (Input.GetKeyDown(KeyCode.Alpha3)) AdjustAngle(-adjustmentAmountDeg, 0f, 0f);
        if (Input.GetKeyDown(KeyCode.Alpha4)) AdjustAngle(adjustmentAmountDeg, 0f, 0f);
        if (Input.GetKeyDown(KeyCode.Alpha5)) AdjustAngle(0f, -adjustmentAmountDeg, 0f);
        if (Input.GetKeyDown(KeyCode.Alpha6)) AdjustAngle(0f, adjustmentAmountDeg, 0f);
        if (Input.GetKeyDown(KeyCode.Alpha7)) AdjustAngle(0f, 0f, -adjustmentAmountDeg);
        if (Input.GetKeyDown(KeyCode.Alpha8)) AdjustAngle(0f, 0f, adjustmentAmountDeg);
    }

    private void SetCameraBrowsingMode(bool enabled)
    {
        if (cameras == null || currentCameraIdx < 0)
            return;

        IsCameraBrowsingMode = enabled;
        cameras[currentCameraIdx].enabled = enabled;

        CameraBrowsingModeChanged?.Invoke(enabled);
    }

    private void SetGroupAndCamera(int groupIndex, int cameraInGroupIndex)
    {
        if (groupIndex < 0 || groupIndex >= GroupsCount ||
            cameraInGroupIndex < 0 || cameraInGroupIndex >= CamerasPerGroup)
            return;

        int nextIdx = groupIndex * CamerasPerGroup + cameraInGroupIndex;
        if (nextIdx == currentCameraIdx)
            return;

        if (currentCameraIdx >= 0)
            cameras[currentCameraIdx].enabled = false;

        currentCameraIdx = nextIdx;
        cameras[currentCameraIdx].enabled = IsCameraBrowsingMode;

        Debug.Log("[AdditionalCameras] Group " + groupIndex + ", camera " +
                  cameraInGroupIndex + " (" + cfg.cameras[nextIdx].label + ")", this);
    }

    private void ChangeGroup(int delta)
    {
        int group = currentCameraIdx / CamerasPerGroup + delta;
        if (group < 0 || group >= GroupsCount)
        {
            if (cfg.stopOnLastGroup)
                return;
            group = (group + GroupsCount) % GroupsCount;
        }
        SetGroupAndCamera(group, currentCameraIdx % CamerasPerGroup);
    }

    private void ChangeCamera(int delta)
    {
        int group = currentCameraIdx / CamerasPerGroup;
        int index = (currentCameraIdx % CamerasPerGroup + delta + CamerasPerGroup) % CamerasPerGroup;
        SetGroupAndCamera(group, index);
    }

    private void AdjustAngle(float horizontal, float vertical, float roll)
    {
        if (currentCameraIdx < 0)
            return;

        var cameraConfig = cfg.cameras[currentCameraIdx];
        var correction = cameraConfig.rotationCorrection;
        correction.x += vertical;
        correction.y += horizontal;
        correction.z -= roll;
        cameraConfig.rotationCorrection = correction;
        cameras[currentCameraIdx].transform.rotation = GetCameraRotation(cameraConfig);
    }

    private void AdjustFov(float delta)
    {
        if (currentCameraIdx < 0)
            return;

        var cameraConfig = cfg.cameras[currentCameraIdx];
        cameraConfig.fov = Mathf.Clamp(cameraConfig.fov + delta, 1f, 179f);
        cameras[currentCameraIdx].fieldOfView = cameraConfig.fov;
    }

    private void OnGUI()
    {
        if (!IsCameraBrowsingMode || currentCameraIdx < 0 || cfg == null)
            return;

        if (cameraNumberStyle == null)
        {
            cameraNumberStyle = new GUIStyle(GUI.skin.box);
            cameraNumberStyle.alignment = TextAnchor.MiddleCenter;
            cameraNumberStyle.fontStyle = FontStyle.Bold;
            cameraNumberStyle.normal.textColor = Color.white;
        }

        cameraNumberStyle.fontSize = Mathf.Max(20, Mathf.RoundToInt(Screen.height * 0.035f));

        const float margin = 16f;
        float width = Mathf.Clamp(Screen.width * 0.12f, 80f, 160f);
        float height = Mathf.Max(46f, cameraNumberStyle.fontSize + 18f);
        var label = cfg.cameras[currentCameraIdx].label;

        // Display the camera label in the lower-right corner.
        GUI.Box(new Rect(Screen.width - width - margin,
            Screen.height - height - margin, width, height), label, cameraNumberStyle);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}

[Serializable]
public class AdditionalCamerasConfig
{
    [Serializable]
    public class CameraConfig
    {
        public int group;
        public int id;
        public string label = "";
        public float fov = 60f;

        [JsonConverter(typeof(Vector3ArrayConverter))]
        public Vector3 position;

        [JsonConverter(typeof(Vector3ArrayConverter))]
        public Vector3 rotationCorrection;
    }

    [JsonConverter(typeof(Vector3ArrayConverter))]
    public Vector3 sceneCenter = new Vector3(0f, -0.3f, 0f);

    public int initialGroupIdx = 0;
    public int initialCameraInGroupIdx = 0;
    public bool stopOnLastGroup = true;
    public bool adjustingEnabled = true;
    public bool viewportStealingEnabled = true;
    public CameraConfig[] cameras;

    public static string GetPath()
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, "../AdditionalCameras.json"));
    }

    public static AdditionalCamerasConfig Load()
    {
        string path = GetPath();
        if (File.Exists(path))
        {
            try
            {
                var loaded = JsonConvert.DeserializeObject<AdditionalCamerasConfig>(File.ReadAllText(path));
                if (loaded != null)
                {
                    if (loaded.cameras == null || loaded.cameras.Length != 24)
                        loaded.cameras = CreateDefaultCameras(loaded.sceneCenter);
                    return loaded;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[AdditionalCameras] Invalid configuration: " + e.Message);
            }
        }
        else
        {
            Debug.LogWarning("[AdditionalCameras] JSON not found; using the default 24-camera layout: " + path);
        }

        var defaults = new AdditionalCamerasConfig();
        defaults.cameras = CreateDefaultCameras(defaults.sceneCenter);
        return defaults;
    }

    public static CameraConfig[] CreateDefaultCameras(Vector3 center)
    {
        var result = new CameraConfig[24];
        float[] x = { -1.7f, -1.7f, -1.7f, 0f, 1.7f, 1.7f, 1.7f, 0f };
        float[] z = { -1.7f, 0f, 1.7f, 1.7f, 1.7f, 0f, -1.7f, -1.7f };

        for (int group = 0; group < 3; group++)
        {
            for (int i = 0; i < 8; i++)
            {
                int idx = group * 8 + i;
                result[idx] = new CameraConfig
                {
                    group = group,
                    id = i,
                    label = (group * 10 + i + 1).ToString(),
                    fov = 60f,
                    position = center + new Vector3(x[i], (group - 1) * 1.7f, z[i]),
                    rotationCorrection = Vector3.zero
                };
            }
        }
        return result;
    }

    public void Save()
    {
        try
        {
            File.WriteAllText(GetPath(), JsonConvert.SerializeObject(this, Formatting.Indented));
            Debug.Log("[AdditionalCameras] Config saved: " + GetPath());
        }
        catch (Exception e)
        {
            Debug.LogError("[AdditionalCameras] Could not save configuration: " + e.Message);
        }
    }
}

// Serializes Unity Vector3 values as JSON arrays [x, y, z].
public class Vector3ArrayConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Vector3);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var vector = (Vector3)value;
        writer.WriteStartArray();
        writer.WriteValue(vector.x);
        writer.WriteValue(vector.y);
        writer.WriteValue(vector.z);
        writer.WriteEndArray();
    }

    public override object ReadJson(
        JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var values = JArray.Load(reader);
        if (values.Count != 3)
            throw new JsonSerializationException("Vector3 requires exactly 3 elements.");

        return new Vector3(
            values[0].Value<float>(),
            values[1].Value<float>(),
            values[2].Value<float>());
    }
}
