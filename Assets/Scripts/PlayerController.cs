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

    public RaycastHit Hit;

    public enum Mode
    {
        ModeMenu,
        Mode3Dto2D,
        //Mode2Dto3D,
        ModeExperimental
    }


    private Vector3 _moveDirection = Vector3.zero;
    private float _rotationX = 0;
    private CharacterController _characterController;
    private Ray _ray;
    private IMode _modeController;
    private Mode _mode = Mode.ModeMenu;
    private bool _isModeChanged = false;
    private LineSegment rayline;
    private Vector3 _lockedRayPoint = Vector3.zero;

    void Start()
    {
        _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        _characterController = GetComponentInChildren<CharacterController>();
        _SetModeController();

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        GameObject.Find("KreaButton").SetActive(false);
        rayline = gameObject.AddComponent<LineSegment>();
        rayline.SetStyle(Color.red, 0.005f);
        rayline.SetLabel("", 0, Color.black);
        Debug.Log($"<color=blue> [MODE MENU]  1 -> grp  ,  2 -> inz  [MODE MENU] </color>");
    }

    void Update()
    {
        _UpdateRaycasting();        
        _UpdateMovement();
        _UpdateMode();
        _TransformRayline();
    }
    private void _TransformRayline()
    {
        Vector3 RayLineOrigin = gameObject.transform.position;

        if (LockedRaycastObject != null)
        {
            // Kiedy mamy locka – linia zawsze idzie do zapamiętanego punktu
            rayline.SetCoordinates(RayLineOrigin, _lockedRayPoint);
        }
        else
        {
            // Normalny tryb – idziemy do aktualnego miejsca raycastu
            rayline.SetCoordinates(RayLineOrigin, _ray.origin + _ray.direction * 100f);
        }
    }

    public GameObject LockedRaycastObject { get; set; } = null;
    private void _UpdateRaycasting()
    {
        _ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (LockedRaycastObject != null)
        {
            Collider lockedCollider = LockedRaycastObject.GetComponent<Collider>();
            if (lockedCollider != null)
            {
                if (lockedCollider.Raycast(_ray, out Hit, 100))
                {
                    // Trafiamy w zafokusowany obiekt – aktualizuj punkt
                    _lockedRayPoint = Hit.point;
                }
                // Jeśli nie trafiamy – nie zmieniamy Hit ani _lockedRayPoint
            }
            return; // w locku nie sprawdzamy innych obiektów
        }

        // Tryb bez locka – normalny raycast
        if (Physics.Raycast(_ray, out Hit, 100))
            _lockedRayPoint = Hit.point;
    }

    private void _UpdateMovement()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float curSpeedX = canMove ? walkingSpeed * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? walkingSpeed * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = _moveDirection.y;

        _moveDirection = (forward * curSpeedX) + (right * curSpeedY);
        _moveDirection.y = movementDirectionY;
        _characterController.Move(_moveDirection * Time.deltaTime);

        // Player and Camera rotation
        if (canMove)
        {
            _rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            _rotationX = Mathf.Clamp(_rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(_rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }

    private void _UpdateMode()
    {
        _modeController.HandleInput();

        if (_isModeChanged) {
             _SetModeController();
            _isModeChanged = false;
        }
    }

    private void _SetModeController()
    {
        switch (_mode)
        {
            case Mode.ModeMenu:
                _modeController = new ModeMenu(this);
                break;

            case Mode.Mode3Dto2D:
                _modeController = new Mode3Dto2D(this);
                break;

            case Mode.ModeExperimental:
                _modeController = new ModeExperimental(this);
                break;

            default:
                _modeController = new ModeMenu(this);
                break;
        }
    }


    public void ChangeMode(Mode mode)
    {
        _mode = mode;
        _isModeChanged = true;
    }
}







        // if (Input.GetKeyDown("o"))
        // {
        //     pp.CreateLabel(hit, $"{alpha[labelIdx]}");
        // }
        // if (Input.GetKeyDown("p"))
        // {
        //     pp.RemoveLabel(hit, $"{alpha[labelIdx]}");
        // }
        // if (Input.GetKeyDown("4"))
        // {
        //     labelIdx = (labelIdx - 1 < 0 ? alpha.Length - 1 : labelIdx - 1);
        //     Debug.Log($"Current label {alpha[labelIdx]}");
        // }
        // if (Input.GetKeyDown("5"))
        // {
        //     labelIdx = (labelIdx+1)% alpha.Length;
        //     Debug.Log($"Current label {alpha[labelIdx]}");
        // }