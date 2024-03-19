using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Point : MonoBehaviour {

	private Vector3 point;
	private Color pointColor = Color.black;
	private float pointSize = 1.0f;

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

		lineRenderer.startColor = pointColor;
		lineRenderer.endColor = pointColor;

		lineRenderer.startWidth = pointSize;
		lineRenderer.endWidth = pointSize;
	}
	
	// Update is called once per frame
	void Update () 
	{
		// lineRenderer.SetPosition(0, this.point);
		// lineRenderer.SetPosition(1, this.point);
		lineRenderer.SetPosition(0, this.transform.position);
		lineRenderer.SetPosition(1, this.transform.position);
	}

	public void SetCoordinates(Vector3 point)
	{
		this.transform.position = point;

		this.point = point;
	}

	public Vector3 GetCoordinates()
	{
		return this.transform.position;
	}

	public void SetStyle(Color pointColor, float pointSize)
	{
		this.pointColor = pointColor;
		this.pointSize = pointSize;
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
}
