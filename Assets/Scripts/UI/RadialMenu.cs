using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class RadialMenu : MonoBehaviour
{
    [Header("Prefab i Ustawienia")]
    private GameObject itemPrefab;
    private int itemCount;
    private float radius;
    private List<string> labels;

    [Header("Nawigacja")]
    public KeyCode prevKey = KeyCode.LeftArrow;
    public KeyCode nextKey = KeyCode.RightArrow;
    public KeyCode confirmKey = KeyCode.Return;
    public float selectedScale = 1.3f;
    public float fontSizeScale = 0.1f;
    public Vector3 initialMenuItemScale;
    public Vector3 initialMenuScale = new Vector3(0.5f, 0.5f, 0.5f);

    private List<GameObject> spawnedItems = new List<GameObject>();
    private int selectedIndex = 0;
    private ModeExperimental modeExperimental;

    // Statyczna metoda do tworzenia i konfiguracji RadialMenu
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
        menu.Generate();
        return menu;
    }

    void Update()
    {
        if (spawnedItems.Count == 0) return;

        // Zmiana zaznaczenia
        if (Input.GetKeyDown(nextKey))
        {
            Select((selectedIndex - 1 + itemCount) % itemCount);
            modeExperimental._ChangeDrawContextNext();
        }


        if (Input.GetKeyDown(prevKey))
        {
            Select((selectedIndex + 1) % itemCount);
            modeExperimental._ChangeDrawContextPrev();
        }
            

        // Potwierdzenie
        if (Input.GetKeyDown(confirmKey))
            ConfirmSelection();
    }

    private void ConfirmSelection()
    {
        switch (selectedIndex)
        {
            case 0:
                Debug.Log("Wybrano opcję 0");
                // modeExperimental._ChangeDrawContext();
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
        // 1) Wyczyszczenie poprzednich elementów
        for (int i = transform.childCount - 1; i >= 0; i--)
#if UNITY_EDITOR
            DestroyImmediate(transform.GetChild(i).gameObject);
#else
            Destroy(transform.GetChild(i).gameObject);
#endif
        spawnedItems.Clear();

        // 2) Dopasuj listę labeli
        while (labels.Count < itemCount)
            labels.Add($"Item {labels.Count + 1}");
        if (labels.Count > itemCount)
            labels.RemoveRange(itemCount, labels.Count - itemCount);

        // 3) Stwórz nowe przyciski
        float angleStep = 360f / itemCount;
        float angle = 0f;
        for (int i = 0; i < itemCount; i++)
        {
            var go = Instantiate(itemPrefab, transform);
            go.name = $"MenuItem {i}";
            spawnedItems.Add(go);

            var rt = go.GetComponent<RectTransform>();
            float rad = Mathf.Deg2Rad * angle;
            rt.anchoredPosition = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
            //rt.localScale = initialScale;

            var txt = go.GetComponentInChildren<Text>();
            if (txt)
            {
                txt.text = labels[i];
                float itemWidth = rt.sizeDelta.x;
                txt.fontSize = Mathf.RoundToInt(itemWidth * fontSizeScale);
            }

            angle += angleStep;
        }

        // 4) Zaznacz pierwszy
        selectedIndex = 0;
        Select(0);
    }

    private void Select(int newIndex)
    {
        // Reset starego
        if (spawnedItems[selectedIndex] != null)
            spawnedItems[selectedIndex].GetComponent<RectTransform>().localScale = initialMenuItemScale;

        selectedIndex = newIndex;

        // Podświetl nowego
        if (spawnedItems[selectedIndex] != null)
            spawnedItems[selectedIndex].GetComponent<RectTransform>().localScale = initialMenuItemScale * selectedScale;
    }
}