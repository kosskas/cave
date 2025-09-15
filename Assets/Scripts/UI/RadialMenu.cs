using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RadialMenu : MonoBehaviour
{
    private GameObject itemPrefab;
    private int itemCount;
    private float radius;
    private List<string> labels;

    public KeyCode prevKey = KeyCode.RightArrow;
    public KeyCode nextKey = KeyCode.LeftArrow;
    public KeyCode confirmKey = KeyCode.Return;
    public float selectedScale = 1.3f;
    public float fontSizeScale = 0.1f;
    public Vector3 initialMenuItemScale;
    public Vector3 initialMenuScale = new Vector3(0.5f, 0.5f, 0.5f);

    public bool animateRotation = true;
    public float rotationDuration = 0.25f; 

    public float targetAngleForSelected = 90f; 

    private List<GameObject> spawnedItems = new List<GameObject>();
    private int selectedIndex = 0;
    private ModeExperimental modeExperimental;

    private float angleStep;
    private float baseStartAngle = 0f;
    private float rotationOffset = 0f; 
    private Coroutine rotateCoroutine;

    public bool isMenuActive = false;

 
    public static RadialMenu Create(Transform parent, int count, float rad, List<string> lbls, GameObject prefab, ModeExperimental modeExperimental)
    {
        GameObject menuObject = new GameObject("RadialMenu");
        menuObject.transform.SetParent(parent, false);
        menuObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        RadialMenu menu = menuObject.AddComponent<RadialMenu>();
        menu.itemCount = count;
        menu.radius = rad;
        menu.labels = lbls;
        menu.itemPrefab = prefab;
        menu.modeExperimental = modeExperimental;
        menu.initialMenuItemScale = prefab.GetComponent<RectTransform>().localScale;
        FlystickController.SetAction(
            FlystickController.Btn.JOYSTICK,
            FlystickController.ActOn.TILT_LEFT,
            () =>
            {
                menu.Select((menu.selectedIndex - 1 + menu.itemCount) % menu.itemCount);
                modeExperimental._ChangeDrawContextPrev();
            }
        );
        FlystickController.SetAction(
            FlystickController.Btn.JOYSTICK,
            FlystickController.ActOn.TILT_RIGHT,
            () =>
            {
                menu.Select((menu.selectedIndex + 1 + menu.itemCount) % menu.itemCount);
                modeExperimental._ChangeDrawContextNext();
            }
        );
        menu.Generate();
        return menu;
    }

    void Update()
    {
        if (!isMenuActive) return;

        if (spawnedItems.Count == 0) return;

        if (Input.GetKeyDown(prevKey))
        {
            Select((selectedIndex - 1 + itemCount) % itemCount);
            modeExperimental?._ChangeDrawContextPrev();
        }

        if (Input.GetKeyDown(nextKey))
        {
            Select((selectedIndex + 1) % itemCount);
            modeExperimental?._ChangeDrawContextNext();
        }

        if (Input.GetKeyDown(confirmKey))
            ConfirmSelection();
    }

    private void ConfirmSelection()
    {
        switch (selectedIndex)
        {
            case 0:
                Debug.Log("Wybrano opcję 0");
                break;
            case 1:
                Debug.Log("Wybrano opcję 1");
                break;
            case 2:
                Debug.Log("Wybrano opcję 2");
                break;
            default:
                Debug.LogWarning($"Brak obsługi dla indeksu {selectedIndex}");
                break;
        }
    }

    public void Generate()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
#if UNITY_EDITOR
            DestroyImmediate(transform.GetChild(i).gameObject);
#else
            Destroy(transform.GetChild(i).gameObject);
#endif
        spawnedItems.Clear();

        while (labels.Count < itemCount)
            labels.Add($"Item {labels.Count + 1}");
        if (labels.Count > itemCount)
            labels.RemoveRange(itemCount, labels.Count - itemCount);

        angleStep = 360f / itemCount;
        float angle = baseStartAngle;
        for (int i = 0; i < itemCount; i++)
        {
            var go = Instantiate(itemPrefab, transform);
            go.name = $"MenuItem {i}";
            spawnedItems.Add(go);

            var rt = go.GetComponent<RectTransform>();
            float rad = Mathf.Deg2Rad * angle;
            rt.anchoredPosition = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;

            var txt = go.GetComponentInChildren<Text>();
            if (txt)
            {
                txt.text = labels[i];
                float itemWidth = rt.sizeDelta.x;
                txt.fontSize = Mathf.RoundToInt(itemWidth * fontSizeScale);
            }

            angle += angleStep;
        }

        selectedIndex = 0;
        rotationOffset = NormalizeAngle(targetAngleForSelected - baseStartAngle - selectedIndex * angleStep);
        UpdatePositions();
        if (spawnedItems.Count > 0 && spawnedItems[selectedIndex] != null)
            spawnedItems[selectedIndex].GetComponent<RectTransform>().localScale = initialMenuItemScale * selectedScale;
        isMenuActive = true;
    }

    private void Select(int newIndex, bool instant = false)
    {
        if (isMenuActive)
        {
            if (spawnedItems.Count == 0) return;

            if (spawnedItems[selectedIndex] != null)
                spawnedItems[selectedIndex].GetComponent<RectTransform>().localScale = initialMenuItemScale;

            selectedIndex = newIndex;

            if (spawnedItems[selectedIndex] != null)
                spawnedItems[selectedIndex].GetComponent<RectTransform>().localScale = initialMenuItemScale * selectedScale;

            float rawTarget = targetAngleForSelected - baseStartAngle - selectedIndex * angleStep;
            float targetRotationOffset = NormalizeAngle(rawTarget);

            if (instant || !animateRotation || rotationDuration <= 0f)
            {
                if (rotateCoroutine != null) StopCoroutine(rotateCoroutine);
                rotationOffset = targetRotationOffset;
                UpdatePositions();
            }
            else
            {
                if (rotateCoroutine != null) StopCoroutine(rotateCoroutine);
                rotateCoroutine = StartCoroutine(AnimateRotationShortest(rotationOffset, targetRotationOffset, rotationDuration));
            }
        }
        
    }

    private IEnumerator AnimateRotationShortest(float fromAngle, float toAngle, float duration)
    {
        float from = NormalizeAngle(fromAngle);
        float to = NormalizeAngle(toAngle);

        float delta = Mathf.DeltaAngle(from, to);

        float start = from;
        float end = from + delta;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / duration);
            float s = Mathf.SmoothStep(0f, 1f, u);
            rotationOffset = Mathf.Lerp(start, end, s);
            rotationOffset = NormalizeAngle(rotationOffset);
            UpdatePositions();
            yield return null;
        }

        rotationOffset = to;
        UpdatePositions();
        rotateCoroutine = null;
    }

    private float NormalizeAngle(float a)
    {
        a = Mathf.Repeat(a + 180f, 360f) - 180f;
        return a;
    }

    private void UpdatePositions()
    {
        for (int i = 0; i < spawnedItems.Count; i++)
        {
            if (spawnedItems[i] == null) continue;
            var rt = spawnedItems[i].GetComponent<RectTransform>();
            float angle = baseStartAngle + i * angleStep + rotationOffset;
            float rad = Mathf.Deg2Rad * angle;
            rt.anchoredPosition = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
        }
    }

    public void SelectIndex(int index)
    {
        if (itemCount <= 0) return;
        Select((index % itemCount + itemCount) % itemCount);
    }

    public void SetRadialMenuActive(bool flag)
    {
        isMenuActive = flag;
        foreach (GameObject item in spawnedItems)
        {
            item.SetActive(isMenuActive);
        }
    }
}
