using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerControllerMode
{
    ModeMenu,
    Mode3Dto2D,
    Mode2Dto3D
}

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

    PlayerControllerMode mode = PlayerControllerMode.ModeMenu;
    CharacterController characterController;
    WallController wc;
    WallCreator wcrt;
    PointPlacer pp;
    GridCreator gc;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;
    //[HideInInspector]

    SolidImporter si;
    private Ray ray;
    private RaycastHit hit;
    void Start()
    {
        ray =  Camera.main.ScreenPointToRay(Input.mousePosition);

        characterController = GetComponentInChildren<CharacterController>();

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log($"<color=blue> [MODE MENU]  1 -> grp  ,  2 -> inz  [MODE MENU] </color>");
    }

    void Update()
    {
        ///Raycasting
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out hit, 100);

        ////
        /// NOTE: jedyny Input nie będący tutaj jest w pliku ObjectRotator!
        ///
        //from solidimporter

        switch (mode)
        {
            case PlayerControllerMode.ModeMenu:
                UpdateMenu();
                break;

            case PlayerControllerMode.Mode2Dto3D:
                Update2Dto3D();
                break;

            case PlayerControllerMode.Mode3Dto2D:
                Update3Dto2D();
                break;

            default:
                Debug.Log("[i] switch(PlayerControllerMode) default case");
                mode = PlayerControllerMode.ModeMenu;
                break;
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

    void UpdateMenu()
    {
        if (Input.GetKeyDown("1"))
        {
            mode = PlayerControllerMode.Mode3Dto2D;

            GameObject mainObject = GameObject.Find("MainObject");
            si = mainObject.GetComponent<SolidImporter>();
            
            GameObject wallsObject = GameObject.Find("Walls");
            wc = wallsObject.GetComponent<WallController>();
            wcrt = gameObject.GetComponent<WallCreator>();

            Debug.Log($"<color=blue> MODE grupowy ON </color>");
        }
        else if (Input.GetKeyDown("2"))
        {
            mode = PlayerControllerMode.Mode2Dto3D;

            gc = new GridCreator();
            GameObject wallsObject = GameObject.Find("Walls");
            wc = wallsObject.GetComponent<WallController>();

            pp = gameObject.GetComponent<PointPlacer>();
            pp.CreatePoint();
            pp.MovePointPrototype(hit);

            Debug.Log($"<color=blue> MODE inzynierka ON </color>");
        }
    }

    void Update2Dto3D()
    {
        pp.MovePointPrototype(hit);

        if (Input.GetKeyDown("p"))
        {
            pp.OnClick(hit);
        }

    }

    void Update3Dto2D()
    {
        if (Input.GetKeyDown("p"))
        {
            wc.SetBasicWalls();
            wc.SetDefaultShowRules();
            si.SetUpDirection();
            si.ImportSolid();
        }

        if (Input.GetKeyDown("u"))
        {
            wc.SetBasicWalls();
            wc.SetDefaultShowRules();
            si.SetDownDirection();
            si.ImportSolid();
        }

        if (Input.GetKeyDown("o"))
        {
            ObjectProjecter op = (ObjectProjecter)GameObject.FindObjectOfType(typeof(ObjectProjecter));
            op.SetShowingProjectionLines();
        }
        
        if(Input.GetKeyDown("i"))
        {
            ObjectProjecter op = (ObjectProjecter)GameObject.FindObjectOfType(typeof(ObjectProjecter));
            op.SetShowingReferenceLines();
        }

        if(Input.GetKeyDown("l"))
        {
            wc.PopBackWall();
        }

        if (Input.GetKeyDown("v"))
        {
            SetShowingProjection();
        }

        //Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);
        if (Input.GetKeyDown("c"))
        {
            wcrt.CreatePoint(hit);
        }
    }

    void SetShowingProjection()
    {
        if (hit.collider != null)
        {
            if (hit.collider.tag == "Wall")
            {
                //Debug.Log("hit: " + hit.collider.name);
                WallInfo info = wc.FindWallInfoByGameObject(hit.collider.gameObject);
                if (info != null)
                {
                    if (info.showLines)
                    {
                        wc.SetWallInfo(hit.collider.gameObject, false, false, false);
                    }
                    else
                    {
                        wc.SetWallInfo(hit.collider.gameObject, true, true, true);
                    }
                }

            }
        }
    }

}