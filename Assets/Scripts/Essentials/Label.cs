using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Klasa Label opisuje właściwości wsyświetlania etykiety opisującej obiekt
/// </summary>
public class Label : MonoBehaviour {

	private string text = "<?>";
	private float size = 1.0f;
	private Color color = Color.black;

	GameObject player = null;
	TextMesh textMesh = null;

	// Use this for initialization
	void Start ()
	{
		player = GameObject.Find("FPSPlayer");

		textMesh = gameObject.AddComponent<TextMesh>();
		textMesh.text = text;
		textMesh.characterSize = size;
		textMesh.color = color;
	}
	
	// Update is called once per frame
	void Update ()
	{
		Vector3 playerPosition = player.transform.position;
		Vector3 directionToPlayer = (playerPosition + 2*Vector3.up - gameObject.transform.position).normalized;
		gameObject.transform.rotation = Quaternion.LookRotation(-directionToPlayer);
	}

	public void SetLabel(string text, float textSize, Color textColor)
	{
		this.text = text;
		this.size = textSize;
		this.color = textColor;
	}
}
