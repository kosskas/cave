using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Obiekt klasy Label wyświetla tekstową etykietę opisującą inny obiekt, do którego zostało dołączony jako GameObject. Etykieta dynamicznie obraca się przodem do gracza 'FPSPLayer'.
/// </summary>
public class Label : MonoBehaviour {

	/// <summary>
	/// Wyświetlany tekst
	/// </summary>
	private string text = "<?>";

	/// <summary>
	/// Rozmiar fontu wyświetlanego tekstu
	/// </summary>
	private float fontSize = 1.0f;

	/// <summary>
	/// Kolor wyświetlanego tekstu
	/// </summary>
	private Color color = Color.black;

	/// <summary>
	/// Referencja na objekt gracza 'FPSPlayer'
	/// </summary>
	GameObject player = null;

	/// <summary>
	/// Referencja na TextMesh
	/// </summary>
	TextMesh textMesh = null;


	// Use this for initialization
	void Start ()
	{
		player = GameObject.Find("FPSPlayer");

		textMesh = gameObject.AddComponent<TextMesh>();
		textMesh.text = text;
		textMesh.characterSize = fontSize;
		textMesh.color = color;
	}
	
	// Update is called once per frame
	void Update ()
	{
		RotateLabelToPlayer();
	}


	/// <summary>
	/// Metoda obracająca przód/awers etykiety w kierunku do gracza
	/// </summary>
	private void RotateLabelToPlayer()
	{
		Vector3 playerPosition = player.transform.position;
		Vector3 directionToPlayer = (playerPosition + 2*Vector3.up - gameObject.transform.position).normalized;

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
		this.text = text;
		this.fontSize = fontSize;
		this.color = textColor;
	}

}
