
using UnityEngine;
/// <summary>
/// Klasa ObjectRotator służy do obracania bryłą przez gracza, jest dołączana do obiektu ładowanej bryły.
/// </summary>
public class ObjectRotator : MonoBehaviour {
	/// <summary>
	/// Szybkość obrotu
	/// </summary>
	[SerializeField] public float rotationSpeed = 10f;

	GameObject tmp;

	private bool isRotating = false;

	void Start(){
		//tmp = GameObject.Find("FlystickPlaceholder");

		FlystickController.SetAction(
            FlystickController.Btn.FIRE, 
            FlystickController.ActOn.PRESS, 
            () => {isRotating = true;}
        );
		FlystickController.SetAction(
            FlystickController.Btn.FIRE, 
            FlystickController.ActOn.RELEASE, 
            () => {isRotating = false;}
        );
	}

	void Update(){
        if (isRotating)
        {
            transform.rotation = Lzwp.input.flysticks[0].pose.rotation;
        }

	}
	/// <summary>
	/// Kamera potrzebna do wyliczania obrotu
	/// </summary>
	//public Camera cam;
	/*
	void OnMouseDrag()
    {
		float rotationX = Input.GetAxis("Mouse X") * rotationSpeed;
		float rotationY = Input.GetAxis("Mouse Y") * rotationSpeed;

		Vector3 right = Vector3.Cross(cam.transform.up, transform.position - cam.transform.position);
		Vector3 up = Vector3.Cross(transform.position - cam.transform.position, right);
		transform.rotation = Quaternion.AngleAxis(-rotationX, up) * transform.rotation;
		transform.rotation = Quaternion.AngleAxis(rotationY, right) * transform.rotation;

	}
	*/
}
