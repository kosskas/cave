using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class LineSegment : MonoBehaviour {

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
		lineRenderer.SetPosition(0, this.startPoint);
		lineRenderer.SetPosition(1, this.endPoint);
	}

	public void SetCoordinates(Vector3 startPoint, Vector3 endPoint)
	{
		this.transform.position = startPoint + 0.5f * (endPoint - startPoint);

		this.startPoint = startPoint;
		this.endPoint = endPoint;
	}

	public Tuple<Vector3, Vector3> GetCoordinates()
	{
		return new Tuple<Vector3, Vector3>(this.startPoint, this.endPoint);
	}

	public void SetStyle(Color lineColor, float lineWidth)
	{
		this.lineColor = lineColor;
		this.lineWidth = lineWidth;
	}

	public void SetLabel(string text, float textSize, Color textColor)
	{
		if (text.Length == 0)
		{
			return;
		}

		if (labelObject == null)
		{
			labelObject = new GameObject("Label");
			labelObject.transform.SetParent(gameObject.transform);
			labelObject.transform.position = this.transform.position;
			labelObject.AddComponent<Label>();
		}

		Label label = labelObject.GetComponent<Label>();
		label.SetLabel(text, textSize, textColor);
	}

	public void SetEnable(bool mode)
	{
		if (lineRenderer == null) return;

		lineRenderer.enabled = mode;
	}
}
