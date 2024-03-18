using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class LineSegment : MonoBehaviour {

	private Vector3 referencePoint;
	private Vector3 startPoint;
	private Vector3 endPoint;
	private Color lineColor = Color.black;
	private float lineWidth = 1.0f;

	LineRenderer lineRenderer = null;
	GameObject labelObject = null;

	// Use this for initialization
	void Start ()
	{
		lineRenderer = gameObject.AddComponent<LineRenderer>();
		lineRenderer.positionCount = 2;
		lineRenderer.material = new Material(Shader.Find("Standard"));
		lineRenderer.numCapVertices = 10;
		lineRenderer.shadowCastingMode = ShadowCastingMode.Off;

		lineRenderer.startColor = lineColor;
		lineRenderer.endColor = lineColor;

		lineRenderer.startWidth = lineWidth;
		lineRenderer.endWidth = lineWidth;
	}
	
	// Update is called once per frame
	void Update ()
	{
		lineRenderer.SetPosition(0, referencePoint + (transform.rotation * startPoint));
		lineRenderer.SetPosition(1, referencePoint + (transform.rotation * endPoint));
	}

	public void SetBaseCoordinates(Vector3 referencePoint, Vector3 startPoint, Vector3 endPoint)
	{
		this.referencePoint = referencePoint;
		this.transform.position = referencePoint;

		this.startPoint = startPoint;
		this.endPoint = endPoint;
	}

	public void SetStyle(Color lineColor, float lineWidth)
	{
		this.lineColor = lineColor;
		this.lineWidth = lineWidth;
	}

	public void SetLabel(string text, float textSize, Color textColor)
	{
		if (labelObject == null)
		{
			labelObject = new GameObject("Label");
			labelObject.transform.SetParent(gameObject.transform);
			labelObject.transform.position = referencePoint + startPoint + 0.5f * (endPoint - startPoint);
			labelObject.AddComponent<Label>();
		}

		Label label = labelObject.GetComponent<Label>();
		label.SetLabel(text, textSize, textColor);
	}
}
