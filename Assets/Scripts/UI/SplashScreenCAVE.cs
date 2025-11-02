using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SplashScreenCave : MonoBehaviour
{
    public float splashDuration = 2f;   //czas trwania
    public float fadeDuration = 1f;     //czas zanikania
    public string splashText = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. " +
        "Donec sagittis nulla eget massa tincidunt accumsan. Etiam sodales luctus leo. Quisque metus justo, vehicula id congue nec, scelerisque vel sem.";

    private Canvas splashCanvas;
    private Image background;
    private Text text;
    private Image fadeOverlay;

    void Start()
    {
        StartCoroutine(ShowSplashThenHide());
    }

    IEnumerator ShowSplashThenHide()
    {
        //canvas
        GameObject canvasObj = new GameObject("SplashCanvas");
        splashCanvas = canvasObj.AddComponent<Canvas>();
        splashCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();

        //background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(splashCanvas.transform);
        background = bgObj.AddComponent<Image>();
        background.color = new Color(0, 0, 0, 1);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        //text
        GameObject textObj = new GameObject("SplashText");
        textObj.transform.SetParent(splashCanvas.transform);
        text = textObj.AddComponent<Text>();
        text.text = splashText;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.fontSize = 48;
        RectTransform txtRect = textObj.GetComponent<RectTransform>();
        txtRect.anchorMin = new Vector2(0.5f, 0.5f);
        txtRect.anchorMax = new Vector2(0.5f, 0.5f);
        txtRect.anchoredPosition = Vector2.zero;
        txtRect.sizeDelta = new Vector2(600, 200);

        //fade
        GameObject fadeObj = new GameObject("FadeOverlay");
        fadeObj.transform.SetParent(splashCanvas.transform);
        fadeOverlay = fadeObj.AddComponent<Image>();
        fadeOverlay.color = new Color(0, 0, 0, 1f);
        RectTransform fadeRect = fadeObj.GetComponent<RectTransform>();
        fadeRect.anchorMin = Vector2.zero;
        fadeRect.anchorMax = Vector2.one;
        fadeRect.offsetMin = Vector2.zero;
        fadeRect.offsetMax = Vector2.zero;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            fadeOverlay.color = new Color(0, 0, 0, 1 - (t / fadeDuration));
            yield return null;
        }
        fadeOverlay.color = new Color(0, 0, 0, 0);

        yield return new WaitForSeconds(splashDuration);

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            fadeOverlay.color = new Color(0, 0, 0, t / fadeDuration);
            yield return null;
        }

        // kursor tworzony tutaj, zeby nie bylo go na splash screenie
        GameObject crosshair = new GameObject("Crosshair");
        CrosshairGUI crosshairScript = crosshair.AddComponent<CrosshairGUI>();

        Destroy(splashCanvas.gameObject);
    }
}
