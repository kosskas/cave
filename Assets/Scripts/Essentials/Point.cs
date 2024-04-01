using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


/// <summary>
/// Obiekt klasy Point rysuje punkt o współrzędnych określnonych względem położenia obiektu rodzica. Dynamicznie reaguje na transformację położenia obiektu rodzica.
/// </summary>
public class Point : MonoBehaviour {

	/// <summary>
	/// Współrzędne punktu, liczone względem położenia obiektu rodzica
	/// </summary>
	private Vector3 point = Vector3.zero;

	/// <summary>
	/// Kolor punktu
	/// </summary>
	private Color pointColor = Color.black;

	/// <summary>
	/// Średnica punktu
	/// </summary>
	private float pointSize = 1.0f;

	/// <summary>
	/// Referencja na obiekt klasy LineRenderer używany do rysowania punktu
	/// </summary>
	LineRenderer lineRenderer = null;

	/// <summary>
	/// Referencja na GameObject zawierający komponent z objektem klasy Label 
	/// </summary>
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
		TransformPointCoordinates();
	}


	/// <summary>
	/// Metoda transformuje współrzędne punktu w zależności od aktualnego położenia obiektu rodzica
	/// </summary>
	private void TransformPointCoordinates()
	{
		point = this.transform.position;

		lineRenderer.SetPosition(0, point);
		lineRenderer.SetPosition(1, point);
	}


	/// <summary>
	/// Metoda ustawia, nadpisując dotychczasowe, współrzędne punktu
	/// </summary>
	/// <param name="point">Obiekt klasy Vector3, współrzędne punktu, liczone względem położenia obiektu rodzica</param>
	public void SetCoordinates(Vector3 point)
	{
		this.transform.position = point;
		this.point = point;
	}

	/// <summary>
	/// Metoda zwraca aktualne współrzędne punktu
	/// </summary>
	/// <returns>Obiekt klasy Vector3, współrzędne punktu, liczone względem położenia obiektu rodzica</returns>
	public Vector3 GetCoordinates()
	{
		return point;
	}

	/// <summary>
	/// Metoda ustawia właściowści rysowanego punktu
	/// </summary>
	/// <param name="pointColor">Kolor punktu</param>
	/// <param name="pointSize">Średnica punktu</param>
	public void SetStyle(Color pointColor, float pointSize)
	{
		this.pointColor = pointColor;
		this.pointSize = pointSize;
	}

	/// <summary>
	/// Metoda:
	/// jeśli komponent etykiety (obiekt klasy Label) został już dołączony do punktu, aktualizuje właściowści wyświetlanego tekstu tej etykiety
	/// jeśli nie, dołącza komponent etykiety (obiekt klasy Label) i ustawia właściowści wyświetlanego tekstu tej etykiety
	/// </summary>
	/// <param name="text">Tekst która ma zostać wyświetlony na etykiecie</param>
	/// <param name="fontSize">Rozmiar fontu wyświetlanego tekstu</param>
	/// <param name="textColor">Kolor wyświetlanego tekstu</param>
	public void SetLabel(string text, float fontSize, Color textColor)
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
		label.SetLabel(text, fontSize, textColor);
	}

}
