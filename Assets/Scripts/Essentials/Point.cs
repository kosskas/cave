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
	private Vector3 _point = Vector3.zero;

	/// <summary>
	/// Kolor punktu
	/// </summary>
	private Color _pointColor = Color.black;

	/// <summary>
	/// Średnica punktu
	/// </summary>
	private float _pointSize = 1.0f;

	/// <summary>
	/// Flaga określająca widoczność punktu
	/// </summary>
	private bool _isEnabled = true;

	/// <summary>
	/// Referencja na GameObject zawierający komponent PrimitiveType.Sphere
	/// </summary>
	private GameObject _pointObject = null;

	/// <summary>
	/// Referencja na obiekt klasy Renderer używany do rysowania punktu
	/// </summary>
	private Renderer _pointRenderer = null;

	/// <summary>
	/// Referencja na GameObject zawierający komponent z objektem klasy Label 
	/// </summary>
	private GameObject _labelObject = null;


	// Use this for initialization
	void Start ()
	{
		this._pointObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		this._pointObject.transform.SetParent(gameObject.transform);
		this._pointRenderer = this._pointObject.GetComponent<Renderer>();

		Destroy(this._pointObject.GetComponent<SphereCollider>());

		this._pointRenderer.material = new Material(Shader.Find("Unlit/Color"));
		this._pointRenderer.material.color = this._pointColor;
		this._pointRenderer.enabled = this._isEnabled;
		this._pointRenderer.shadowCastingMode = ShadowCastingMode.Off;

		this._pointObject.transform.localScale = new Vector3(this._pointSize, this._pointSize, this._pointSize);
		this._pointObject.transform.position = this._point;

		// lineRenderer = gameObject.AddComponent<LineRenderer>();

		// lineRenderer.positionCount = 2;
        // lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        // lineRenderer.material.color = pointColor;
        // lineRenderer.numCapVertices = 10;
		// lineRenderer.shadowCastingMode = ShadowCastingMode.Off;

		// lineRenderer.startColor = pointColor;
		// lineRenderer.endColor = pointColor;

		// lineRenderer.startWidth = pointSize;
		// lineRenderer.endWidth = pointSize;

		// lineRenderer.enabled = isEnabled;
	}
	
	// Update is called once per frame
	void Update () 
	{
		UpdatePointStyle();
		UpdatePointCoordinates();
	}

	/// <summary>
	/// Metoda transformuje współrzędne punktu w zależności od aktualnego położenia obiektu rodzica
	/// </summary>
	private void UpdatePointCoordinates()
	{
		this._point = this.transform.position;
		this._pointObject.transform.position = this._point;

		// lineRenderer.SetPosition(0, point);
		// lineRenderer.SetPosition(1, point);
	}

	/// <summary>
	/// Metoda uaktualnia wizualne właściowości punktu - kolor, rozmiar, widoczność
	/// </summary>
	private void UpdatePointStyle()
	{
		this._pointRenderer.material.color = this._pointColor;

		this._pointObject.transform.localScale = new Vector3(this._pointSize, this._pointSize, this._pointSize);

		this._pointRenderer.enabled = this._isEnabled;
		
		// lineRenderer.startColor = pointColor;
		// lineRenderer.endColor = pointColor;

		// lineRenderer.startWidth = pointSize;
		// lineRenderer.endWidth = pointSize;

		// lineRenderer.enabled = isEnabled;
	}


	/// <summary>
	/// Metoda ustawia, nadpisując dotychczasowe, współrzędne punktu
	/// </summary>
	/// <param name="point">Obiekt klasy Vector3, współrzędne punktu, liczone względem położenia obiektu rodzica</param>
	public void SetCoordinates(Vector3 point)
	{
		this.transform.position = point;
		this._point = point;
	}

	/// <summary>
	/// Metoda zwraca aktualne współrzędne punktu
	/// </summary>
	/// <returns>Obiekt klasy Vector3, współrzędne punktu, liczone względem położenia obiektu rodzica</returns>
	public Vector3 GetCoordinates()
	{
		return this._point;
	}

	/// <summary>
	/// Metoda ustawia właściowści rysowanego punktu
	/// </summary>
	/// <param name="pointColor">Kolor punktu</param>
	/// <param name="pointSize">Średnica punktu</param>
	public void SetStyle(Color pointColor, float pointSize)
	{
		this._pointColor = pointColor;
		this._pointSize = pointSize;
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

		if (this._labelObject == null)
		{
			this._labelObject = new GameObject("Label");
			this._labelObject.transform.SetParent(gameObject.transform);
			this._labelObject.transform.position = this.transform.position;
			this._labelObject.AddComponent<Label>();
		}

		Label label = this._labelObject.GetComponent<Label>();
		label.SetLabel(text, fontSize, textColor);
	}

    /// <summary>
    /// Metoda umożliwia włączenie lub wyłączenie widoczności (renderowania) punktu
    /// </summary>
    /// <param name="isEnabled">Flaga ustawiająca widoczność. Jeśli "true" punkt zacznie być rysowany, jeśli "false" punkt przestanie być rysowany </param>
    public void SetEnable(bool isEnabled)
    {
		this._isEnabled = isEnabled;
		this._labelObject?.GetComponent<Label>()?.SetEnable(isEnabled);
    }

	/// <summary>
	/// Metoda zwraca wartość określającą czy punkt jest widoczny
	/// </summary>
	/// <returns></returns>
	public bool IsEnabled()
	{
		return this._isEnabled;
	}
}
