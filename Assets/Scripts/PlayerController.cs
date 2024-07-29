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
    private int labelIdx = 0;
    SolidImporter si;
    /// <summary>
    /// Zmienna czy gałka jest przechylona
    /// </summary>
    private bool is_tilted = false;
    private Ray ray;
    private RaycastHit hit;
    [SerializeField] GameObject flystick;
    LineSegment rayline;
    private const float POINT_SIZE = 0.05f;
    private const float RAY_WEIGHT = 0.005f;
    private const float RAY_RANGE = 100f;

    private char[] alpha;
    void Start()
    {
        ray =  Camera.main.ScreenPointToRay(Input.mousePosition); rayline = flystick.AddComponent<LineSegment>();
        rayline.SetStyle(Color.red, RAY_WEIGHT);
        rayline.SetCoordinates(flystick.transform.position, flystick.transform.forward * RAY_RANGE);
        rayline.SetLabel("", 0.01f, Color.white);


        alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        characterController = GetComponentInChildren<CharacterController>();

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (Lzwp.sync.isMaster)
        {
            Lzwp.input.flysticks[0].GetButton(LzwpInput.Flystick.ButtonID.Button1).OnPress += () => {
                ObjectProjecter op = (ObjectProjecter)GameObject.FindObjectOfType(typeof(ObjectProjecter));
                if (op)
                {
                    op.SetShowingProjectionLines();
                }
            };

            Lzwp.input.flysticks[0].GetButton(LzwpInput.Flystick.ButtonID.Button2).OnPress += () => {
                ObjectProjecter op = (ObjectProjecter)GameObject.FindObjectOfType(typeof(ObjectProjecter));
                if (op)
                {
                    op.SetShowingReferenceLines();
                }
            };
            Lzwp.input.flysticks[0].GetButton(LzwpInput.Flystick.ButtonID.Button3).OnPress += () =>
            {
                wcrt.SwitchWallVisibility(hit);

            };
            Lzwp.input.flysticks[0].GetButton(LzwpInput.Flystick.ButtonID.Button4).OnPress += () =>
            {
                wcrt.CreatePoint(hit, POINT_SIZE);
            };

            Lzwp.input.flysticks[0].GetButton(LzwpInput.Flystick.ButtonID.Joystick).OnPress += () => {
                wc.PopBackWall();
            };
        }
    }

    void Update()
    {
        ///Raycasting
        ray = new Ray(flystick.transform.position, flystick.transform.forward * RAY_RANGE);
        rayline.SetCoordinates(flystick.transform.position, flystick.transform.forward * RAY_RANGE);
        Physics.Raycast(ray, out hit, 100);

        if (Lzwp.sync.isMaster)
        {
            int flystickIdx = 0;
            if (Lzwp.input.flysticks.Count > flystickIdx)
            {
                float x = Lzwp.input.flysticks[flystickIdx].joysticks[0];
                if( x <= -0.8f)
                {
                    if (is_tilted == false)
                    {
                        wc.SetBasicWalls();
                        wc.SetDefaultShowRules();
                        si.SetDownDirection();
                        si.ImportSolid();
                        is_tilted = true;
                    }
                }
                else if( x >= 0.8f)
                {
                    if (is_tilted == false)
                    {
                        wc.SetBasicWalls();
                        wc.SetDefaultShowRules();
                        si.SetUpDirection();
                        si.ImportSolid();
                        is_tilted = true;
                    }
                }
                else
                {
                    is_tilted = false;
                }
            }
        }

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

        if (Input.GetKeyDown("o"))
        {
            pp.CreateLabel(hit, $"{alpha[labelIdx]}");
        }
        if (Input.GetKeyDown("p"))
        {
            pp.RemoveLabel(hit, $"{alpha[labelIdx]}");
        }
        if (Input.GetKeyDown("4"))
        {
            labelIdx = (labelIdx - 1 < 0 ? alpha.Length - 1 : labelIdx - 1);
            Debug.Log($"Current label {alpha[labelIdx]}");
        }
        if (Input.GetKeyDown("5"))
        {
            labelIdx = (labelIdx+1)% alpha.Length;
            Debug.Log($"Current label {alpha[labelIdx]}");
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
            wcrt.SwitchWallVisibility(hit);
        }

        //Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);
        if (Input.GetKeyDown("c"))
        {
            wcrt.CreatePoint(hit, POINT_SIZE);
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