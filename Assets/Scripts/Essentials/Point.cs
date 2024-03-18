using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Point : MonoBehaviour {

	private Vector3 referencePoint;
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
		lineRenderer.SetPosition(0, referencePoint + (transform.rotation * point));
		lineRenderer.SetPosition(1, referencePoint + (transform.rotation * point));
	}

	public void SetBaseCoordinates(Vector3 referencePoint, Vector3 point)
	{
		this.referencePoint = referencePoint;
		this.transform.position = referencePoint;

		this.point = point;
	}

	public void SetStyle(Color pointColor, float pointSize)
	{
		this.pointColor = pointColor;
		this.pointSize = pointSize;
	}

	public void SetLabel(string text, float textSize, Color textColor)
	{
		if (labelObject == null)
		{
			labelObject = new GameObject("Label");
			labelObject.transform.SetParent(gameObject.transform);
			labelObject.transform.position = referencePoint + point;
			labelObject.AddComponent<Label>();
		}

		Label label = labelObject.GetComponent<Label>();
		label.SetLabel(text, textSize, textColor);
	}
}
