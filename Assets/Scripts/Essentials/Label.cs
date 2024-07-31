using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Obiekt klasy Label wyświetla tekstową etykietę opisującą inny obiekt, do którego zostało dołączony jako GameObject. Etykieta dynamicznie obraca się przodem do gracza 'FPSPLayer'.
/// </summary>
public class Label : MonoBehaviour {

	/// <summary>
	/// Wektor przesunięcia etykiety względem obiektu rodzica, do którego jest dołączona
	/// </summary>
	private Vector3 _offset = Vector3.zero;
	
	/// <summary>
	/// Wyświetlany tekst
	/// </summary>
	private string _text = "<?>";

	/// <summary>
	/// Rozmiar fontu wyświetlanego tekstu
	/// </summary>
	private float _fontSize = 1.0f;

	/// <summary>
	/// Kolor wyświetlanego tekstu
	/// </summary>
	private Color _color = Color.black;

	/// <summary>
	/// Flaga określająca widoczność etykiety
	/// </summary>
	private bool _isEnabled = true;

	/// <summary>
	/// Referencja na objekt gracza 'FPSPlayer'
	/// </summary>
	private GameObject _player = null;

	/// <summary>
	/// Referencja na TextMesh
	/// </summary>
	private TextMesh _textMesh = null;

	/// <summary>
	/// Referencja na obiekt klasy MeshRenderer używany do rysowania etykiety
	/// </summary>
	private MeshRenderer _labelRenderer = null;
	GameObject leftCamera = null;


	// Use this for initialization
	void Start ()
	{
        //this._player = GameObject.Find("FPSPlayer");
        leftCamera = GameObject.FindGameObjectWithTag("MainCamera");
        this._textMesh = gameObject.AddComponent<TextMesh>();
		this._textMesh.text = this._text;
		this._textMesh.characterSize = this._fontSize;
		this._textMesh.color = this._color;

		this._labelRenderer = gameObject.GetComponent<MeshRenderer>();
		this._labelRenderer.enabled = this._isEnabled;
	}
	
	// Update is called once per frame
	void Update ()
	{
		UpdateLabel();
		RotateLabelToPlayer();
	}

	/// <summary>
	/// Metoda uaktualnia wartość, rozmiar i kolor tekstu 
	/// </summary>
	private void UpdateLabel()
	{
		this._textMesh.text = this._text;
		this._textMesh.characterSize = this._fontSize;
		this._textMesh.color = this._color;
		this._labelRenderer.enabled = this._isEnabled;
		gameObject.transform.position = gameObject.transform.parent.transform.position + this._offset;
	}

	/// <summary>
	/// Metoda obracająca przód/awers etykiety w kierunku do gracza
	/// </summary>
	private void RotateLabelToPlayer()
	{
		//Vector3 playerPosition = player.transform.position;
		Vector3 leftcameraPos = leftCamera.transform.position;
		Vector3 directionToPlayer = (leftcameraPos + 2*Vector3.up - gameObject.transform.position).normalized;

		gameObject.transform.rotation = Quaternion.LookRotation(-directionToPlayer);
	}


	/// <summary>
	/// Metoda ustawiająca właściowści wyświetlanego tekstu etykiety
	/// </summary>
	/// <param name="text">Tekst która ma zostać wyświetlony</param>
	/// <param name="fontSize">Rozmiar fontu wyświetlanego tekstu</param>
	/// <param name="textColor">Kolor wyświetlanego tekstu</param>
	public void SetLabel(string text, float fontSize, Color textColor)
	{
		this._text = text;
		this._fontSize = fontSize;
		this._color = textColor;
	}
    /// <summary>
    /// Metoda ustawiająca właściowści wyświetlanego tekstu etykiety
    /// </summary>
    /// <param name="textColor">Kolor wyświetlanego tekstu</param>
    public void SetLabel(Color textColor)
    {
        this._color = textColor;
    }

	public void SetLabel(string text)
	{
		this._text = text;
	}

    /// <summary>
    /// Metoda włączająca/wyłączająca widoczność etykiety
    /// </summary>
    /// <param name="isEnabled">Jeśli "true" etykieta zacznie być rysowana, jeśli "false" przestanie być rysowana</param>
    public void SetEnable(bool isEnabled)
	{
		this._isEnabled = isEnabled;
	}

	public void SetOffset(Vector3 offset)
	{
		this._offset = offset;
	}
}
