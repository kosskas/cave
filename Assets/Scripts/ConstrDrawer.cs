using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstrDrawer : MonoBehaviour
{
    private bool showCircles = true;

    private bool showLines = true;

	GameObject constrDrawingsDir = null;


    // Use this for initialization
    void Start()
    {
        Init();
    }
    public void Init()
    {
        constrDrawingsDir = new GameObject("constrDrawingsDir");
        constrDrawingsDir.transform.SetParent(gameObject.transform);
    }

    void Update()
    {
        if (Input.GetKeyDown("h"))
        {
            WallController wc = (WallController)FindObjectOfType(typeof(WallController));
            CreateHelpingCircle(new Vector3(1.6f, 1.0f, 0.2f), new Vector3(1.6f, 1.0f, -0.4f), wc.GetWallByName("Wall4"));
            CreateHelpingLine(new Vector3(1.6f, 1.0f, 0.2f), new Vector3(1.6f, 1.0f, -0.4f), wc.GetWallByName("Wall4"));
        }
    }

    public void Clear()
    {
        Destroy(constrDrawingsDir);
    }

    public void CreateHelpingLine(Vector3 pos1, Vector3 pos2, WallInfo wall)
    {
        const float antiztrackhit = 0.0001f;
        const float lineLen = 10f;
        Vector3 fixedpos1 = pos1 + antiztrackhit * wall.GetNormal();
        Vector3 fixedpos2 = pos2 + antiztrackhit * wall.GetNormal();

        GameObject hline = new GameObject($"Line {fixedpos1}{fixedpos2}");
        LineRenderer lineRenderer = hline.AddComponent<LineRenderer>();

        hline.transform.SetParent(constrDrawingsDir.transform);
        lineRenderer.positionCount = 2;

        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;

        Vector3 direction = (fixedpos2 - fixedpos1).normalized;

        Vector3 start = fixedpos1 - direction * lineLen;
        Vector3 end = fixedpos2 + direction * lineLen;

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }
    public void CreateHelpingCircle(Vector3 pos1, Vector3 pos2, WallInfo wall)
    {
        const float antiztrackhit = 0.0001f;
        Vector3 fixedpos1 = pos1 + antiztrackhit * wall.GetNormal();
        Vector3 fixedpos2 = pos2 + antiztrackhit * wall.GetNormal();
        const int steps = 100;
        float radius = Vector3.Distance(fixedpos1, fixedpos2);
        Vector3 center = (fixedpos1 + fixedpos2) / 2f; //?


        Vector3 normal = wall.GetNormal().normalized;
        Vector3 right = Vector3.Cross(Vector3.up, normal).normalized;
        Vector3 up = Vector3.Cross(normal, right).normalized;

        GameObject circle = new GameObject($"Circle {fixedpos1}{fixedpos2}");
        LineRenderer circleRenderer = circle.AddComponent<LineRenderer>();

        circle.transform.SetParent(constrDrawingsDir.transform);
        circleRenderer.positionCount = steps + 1;
        circleRenderer.loop = true;

        circleRenderer.material = new Material(Shader.Find("Unlit/Color"));
        circleRenderer.startColor = Color.white;
        circleRenderer.endColor = Color.white;
        circleRenderer.startWidth = 0.02f; 
        circleRenderer.endWidth = 0.02f;

        for (int step = 0; step <= steps; step++)
        {
            float progress = (float)step / steps;
            float radian = 2 * Mathf.PI * progress;

            float x = Mathf.Cos(radian) * radius;
            float y = Mathf.Sin(radian) * radius;

            Vector3 currPos = center + right * x + up * y;
            circleRenderer.SetPosition(step, currPos);
        }
    }


}
