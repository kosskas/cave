using UnityEngine;

public class Kamerki : MonoBehaviour {

	public Transform[] miejsca;
	public Transform kamjejki;

	void Update () {
        if (Input.GetKeyDown(KeyCode.Keypad1))
            kamjejki.SetPositionAndRotation(miejsca[0].position, miejsca[0].rotation);
        else if (Input.GetKeyDown(KeyCode.Keypad2))
            kamjejki.SetPositionAndRotation(miejsca[1].position, miejsca[1].rotation);
        else if (Input.GetKeyDown(KeyCode.Keypad3))
            kamjejki.SetPositionAndRotation(miejsca[2].position, miejsca[2].rotation);
        else if (Input.GetKeyDown(KeyCode.Keypad4))
            kamjejki.SetPositionAndRotation(miejsca[3].position, miejsca[3].rotation);
        else if (Input.GetKeyDown(KeyCode.Keypad5))
            kamjejki.SetPositionAndRotation(miejsca[4].position, miejsca[4].rotation);
    }
}
