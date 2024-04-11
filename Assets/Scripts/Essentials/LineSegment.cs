using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Obiekt klasy LineSegment rysuje odcinek pomiędzy parą współrzędnych (startPoint i endPoint) określnonych względem położenia obiektu rodzica.
/// </summary>
public class LineSegment : MonoBehaviour {

	/// <summary>
	/// Współrzędne początku odcinka, liczone względem położenia obiektu rodzica
	/// </summary>
	private Vector3 startPoint = Vector3.zero;

	/// <summary>
	/// Współrzędne końca odcinka, liczone względem położenia obiektu rodzica
	/// </summary>
	private Vector3 endPoint= Vector3.zero;

	/// <summary>
	/// Kolor linii odcinka
	/// </summary>
	private Color lineColor = Color.black;

	/// <summary>
	/// Szerokość linii odcinka
	/// </summary>
	private float lineWidth = 1.0f;

	/// <summary>
	/// Referencja na obiekt klasy LineRenderer używany do rysowania odcinka
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

		lineRenderer.startColor = lineColor;
		lineRenderer.endColor = lineColor;

		lineRenderer.startWidth = lineWidth;
		lineRenderer.endWidth = lineWidth;
	}
	
	// Update is called once per frame
	void Update ()
	{
		UpdateLineCoordinates();
	}


	/// <summary>
	/// Aktualizacja współrzędnych początku i końca odcinka, na podstawie których jest on rysowany
	/// </summary>
	private void UpdateLineCoordinates()
	{
		lineRenderer.SetPosition(0, startPoint);
		lineRenderer.SetPosition(1, endPoint);
	}


	/// <summary>
	/// Metoda ustawia, nadpisując dotychczasowe, współrzędne początku i końca odcinka
	/// </summary>
	/// <param name="startPoint">Obiekt klasy Vector3, współrzędne początku odcinka, liczone względem położenia obiektu rodzica</param>
	/// <param name="endPoint">Obiekt klasy Vector3, współrzędne końca odcinka, liczone względem położenia obiektu rodzica</param>
	public void SetCoordinates(Vector3 startPoint, Vector3 endPoint)
	{
		this.transform.position = startPoint + 0.5f * (endPoint - startPoint);

		this.startPoint = startPoint;
		this.endPoint = endPoint;
	}

	/// <summary>
	/// Metoda zwraca aktualne współrzędne początku i końca odcinka
	/// </summary>
	/// <returns>Krotka dwóch obiektów klasy Vector3. Item1 określa współrzędne początku odcinka, a Item2 współrzędne końca odcinka. Oba liczone względem położenia obiektu rodzica.</returns>
	public Tuple<Vector3, Vector3> GetCoordinates()
	{
		return new Tuple<Vector3, Vector3>(startPoint, endPoint);
	}

	/// <summary>
	/// Metoda ustawia właściowści rysowanego odcinka
	/// </summary>
	/// <param name="lineColor">Kolor linii odcinka</param>
	/// <param name="lineWidth">Szerokość linii odcinka</param>
	public void SetStyle(Color lineColor, float lineWidth)
	{
		this.lineColor = lineColor;
		this.lineWidth = lineWidth;
	}

	/// <summary>
	/// Metoda:
	/// jeśli komponent etykiety (obiekt klasy Label) został już dołączony do odcinka, aktualizuje właściowści wyświetlanego tekstu tej etykiety
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

	/// <summary>
	/// Metoda umożliwia włączenie lub wyłączenie widoczności (renderowania) odcinka
	/// </summary>
	/// <param name="mode">Flaga ustawiająca widoczność. Jeśli "true" odcinek zacznie być rysowany, jeśli "false" odcinek przestanie być rysowany </param>
	public void SetEnable(bool mode)
	{
		if (lineRenderer == null)
		{
			return;
		}

		lineRenderer.enabled = mode;
	}

}
