
using UnityEngine;

public class ObjectRotator : MonoBehaviour {

	// Use this for initialization
	[SerializeField] float rotationSpeed = 10f;
	public Camera cam;
	void OnMouseDrag()
    {
		float rotationX = Input.GetAxis("Mouse X") * rotationSpeed;
		float rotationY = Input.GetAxis("Mouse Y") * rotationSpeed;

		Vector3 right = Vector3.Cross(cam.transform.up, transform.position - cam.transform.position);
		Vector3 up = Vector3.Cross(transform.position - cam.transform.position, right);
		transform.rotation = Quaternion.AngleAxis(-rotationX, up) * transform.rotation;
		transform.rotation = Quaternion.AngleAxis(rotationY, right) * transform.rotation;

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
