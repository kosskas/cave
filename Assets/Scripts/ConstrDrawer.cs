using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstrDrawer : MonoBehaviour
{
    private bool showCircles = true;

    private bool showLines = true;

	GameObject constrDrawingsDir = null;

    public void Init()
    {
        constrDrawingsDir = new GameObject("constrDrawingsDir");
        constrDrawingsDir.transform.SetParent(gameObject.transform);
    }
    public void Clear()
    {
        Destroy(constrDrawingsDir);
    }

    public void CreateHelpingLine(Vector3 pos1, Vector3 pos2, WallInfo wall)
    {
        const float antiztrackhit = 0.002f;
        const float lineLen = 10f;
        Vector3 fixedpos1 = pos1 + antiztrackhit * wall.GetNormal();
        Vector3 fixedpos2 = pos2 + antiztrackhit * wall.GetNormal();

        GameObject hline = new GameObject($"Line {fixedpos1}{fixedpos2}");
        LineRenderer lineRenderer = hline.AddComponent<LineRenderer>();

        hline.transform.SetParent(constrDrawingsDir.transform);
        lineRenderer.positionCount = 2;

        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        lineRenderer.material.color = ReconstructionInfo.LINE_2D_COLOR;
        lineRenderer.startColor = ReconstructionInfo.LINE_2D_COLOR;
        lineRenderer.endColor = ReconstructionInfo.LINE_2D_COLOR;
        lineRenderer.startWidth = ReconstructionInfo.LINE_2D_WIDTH;
        lineRenderer.endWidth = ReconstructionInfo.LINE_2D_WIDTH;

        Vector3 direction = (fixedpos2 - fixedpos1).normalized;

        Vector3 start = fixedpos1 - direction * lineLen;
        Vector3 end = fixedpos2 + direction * lineLen;

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }
    public void CreateHelpingCircle(Vector3 pos1, Vector3 pos2, WallInfo wall)
    {
        const float antiztrackhit = 0.002f;
        Vector3 fixedpos1 = pos1 + antiztrackhit * wall.GetNormal();
        Vector3 fixedpos2 = pos2 + antiztrackhit * wall.GetNormal();
        const int steps = 100;
        float radius = Vector3.Distance(fixedpos1, fixedpos2);
        Vector3 center = fixedpos1; //?


        Vector3 normal = wall.GetNormal().normalized;
        Vector3 right = Vector3.Cross(wall.gameObject.transform.up, normal).normalized;
        Vector3 up = Vector3.Cross(normal, right).normalized;

        GameObject circle = new GameObject($"Circle {fixedpos1}{fixedpos2}");
        LineRenderer circleRenderer = circle.AddComponent<LineRenderer>();

        circle.transform.SetParent(constrDrawingsDir.transform);
        circleRenderer.positionCount = steps + 1;
        circleRenderer.loop = true;

        circleRenderer.material = new Material(Shader.Find("Unlit/Color"));
        circleRenderer.material.color = ReconstructionInfo.CIRCLE_2D_COLOR;
        circleRenderer.startColor = ReconstructionInfo.CIRCLE_2D_COLOR;
        circleRenderer.endColor = ReconstructionInfo.CIRCLE_2D_COLOR;
        circleRenderer.startWidth = ReconstructionInfo.CIRCLE_2D_WIDTH; 
        circleRenderer.endWidth = ReconstructionInfo.CIRCLE_2D_WIDTH;

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
