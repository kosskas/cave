using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Experimental.Utils;
using Assets.Scripts.Experimental;
using System;
using System.Linq;
using UnityEditor;

public class RadialMenu : MonoBehaviour
{
    private GameObject itemPrefab;
    private int itemCount;
    private float radius;
    private List<string> labels;

    public KeyCode prevKey = KeyCode.Comma;
    public KeyCode nextKey = KeyCode.Period;
    public KeyCode confirmKey = KeyCode.Return;
    public float selectedScale = 1.44f;
    public float fontSizeScale = 0.34f;
    public Vector3 initialMenuItemScale;
    public Vector3 initialMenuScale = new Vector3(0.5f, 0.5f, 0.5f);

    public bool animateRotation = true;
    public float rotationDuration = 0.25f; 

    public float targetAngleForSelected = 90f; 

    private List<GameObject> spawnedItems = new List<GameObject>();
    private int selectedIndex = 0;

    private float angleStep;
    private float baseStartAngle = 0f;
    private float rotationOffset = 0f; 
    private Coroutine rotateCoroutine;
    private CircularIterator<KeyValuePair<ExContext, Action>> _currentCtx;

    public bool isMenuActive = false;

 
    public static RadialMenu Create(Transform parent, GameObject prefab)
    {
        GameObject menuObject = new GameObject("RadialMenu");
        menuObject.transform.SetParent(parent, false);
        menuObject.transform.localScale = new Vector3(0.55f, 0.34f, 0.13f);
        RadialMenu menu = menuObject.AddComponent<RadialMenu>();
        menu.itemPrefab = prefab;
        menu.initialMenuItemScale = prefab.GetComponent<RectTransform>().localScale;
        return menu;
    }

    void Update()
    {
        if (!isMenuActive) return;

        if (spawnedItems.Count == 0) return;

        if (Input.GetKeyDown(prevKey))
        {
            Select((selectedIndex - 1 + itemCount) % itemCount);
            _currentCtx.Previous();
        }

        if (Input.GetKeyDown(nextKey))
        {
            Select((selectedIndex + 1) % itemCount);
            _currentCtx.Next();
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

    public void Generate(CircularIterator<KeyValuePair<ExContext, Action>> ctx, float menuRadius)
    {
        radius = menuRadius;
        _currentCtx = ctx;
        _currentCtx.Begin();
        selectedIndex = 0;
        labels = new List<string>();
        foreach (var kv in _currentCtx.All())
        {
            labels.Add(kv.Key.GetDescription());
        }
        itemCount = labels.Count;
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
        {
            spawnedItems[selectedIndex].GetComponent<RectTransform>().localScale = initialMenuItemScale * selectedScale + new Vector3(0f, 0f, 1f);
            spawnedItems[selectedIndex].GetComponentInChildren<Text>().fontStyle = FontStyle.Bold;
        }
        isMenuActive = true;
    }

    private void Select(int newIndex, bool instant = false)
    {
        if (isMenuActive)
        {
            if (spawnedItems.Count == 0) return;

            if (spawnedItems[selectedIndex] != null)
            {
                spawnedItems[selectedIndex].GetComponent<RectTransform>().localScale = initialMenuItemScale;
                spawnedItems[selectedIndex].GetComponentInChildren<Text>().fontStyle = FontStyle.Normal;
            }

            selectedIndex = newIndex;

            if (spawnedItems[selectedIndex] != null)
            {
                spawnedItems[selectedIndex].GetComponent<RectTransform>().localScale = initialMenuItemScale * selectedScale + new Vector3(0f, 0f, 1f);
                spawnedItems[selectedIndex].GetComponentInChildren<Text>().fontStyle = FontStyle.Bold;
            }

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

    public void ToggleRadialMenuActive()
    {
        isMenuActive = !isMenuActive;
        foreach (GameObject item in spawnedItems)
        {
            item.SetActive(isMenuActive);
        }
    }
}
