using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Klasa PlayerController jest klasą strerującą graczem w aplikacji. Obsługuje ruch gracza po mapie oraz wszelkie dodatkowe czynności inicjowane przyciskiem z klawiatury.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// Określa prędkość poruszania się gracza podczas chodzenia.
    /// </summary>
    public float walkingSpeed = 7.5f;
    /// <summary>
    /// Referencja do obiektu kamery, który jest używany do sterowania widokiem gracza.
    /// </summary>
    public Camera playerCamera;
    /// <summary>
    /// Określa prędkość obrotu kamery wokół osi Y i X
    /// </summary>
    public float lookSpeed = 2.0f;
    /// <summary>
    /// Określa maksymalny kąt obrotu kamery wokół osi X, co pozwala na kontrolę ograniczenia skrętu w górę i w dół. 
    /// </summary>
    public float lookXLimit = 45.0f;
    /// <summary>
    /// Zmienna warunkowa, określająca czy gracz może się poruszać.
    /// </summary>
    public bool canMove = true;
    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;
    //[HideInInspector]

    SolidImporter si;
    CameraScript cs;
    void Start()
    {
        characterController = GetComponentInChildren<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        si = (SolidImporter)GameObject.FindObjectOfType(typeof(SolidImporter));
        cs = (CameraScript)GameObject.FindObjectOfType(typeof(CameraScript));
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        ////
        /// NOTE: jedyny Input nie będący tutaj jest w pliku ObjectRotator!
        ///
        //from CameraScript
        if(Input.GetKeyDown("1"))
        {
			cs.SetCam1();			
        }
        if (Input.GetKeyDown("2"))
		{
			cs.SetCam2();	
		}
        //from solidimporter
        if(Input.GetKeyDown("p"))
        {
            si.ImportSolid();
        }

        if(Input.GetKeyDown("o")){
            ObjectProjecter op = (ObjectProjecter)GameObject.FindObjectOfType(typeof(ObjectProjecter));
            op.SetShowingProjectionLines();
        }
        if(Input.GetKeyDown("i")){
            ObjectProjecter op = (ObjectProjecter)GameObject.FindObjectOfType(typeof(ObjectProjecter));
            op.SetShowingReferenceLines();
        }
        if(Input.GetKeyDown("l")){
            ObjectProjecter op = (ObjectProjecter)GameObject.FindObjectOfType(typeof(ObjectProjecter));
            op.watchPerpendicularity();
        }

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float curSpeedX = canMove ? walkingSpeed * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? walkingSpeed * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;

        moveDirection = (forward * curSpeedX) + (right * curSpeedY);
        moveDirection.y = movementDirectionY;
        characterController.Move(moveDirection * Time.deltaTime);

        // Player and Camera rotation
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }

}