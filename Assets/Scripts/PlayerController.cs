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
        ModeExperimental
    }


    private Vector3 _moveDirection = Vector3.zero;
    private float _rotationX = 0;
    private CharacterController _characterController;
    private FlystickController _flystickController;
    private Ray _ray;
    private IMode _modeController;
    private Mode _mode = Mode.ModeMenu;
    private bool _isModeChanged = false;
    public Vector3 LockedRayPoint { get; set; } = Vector3.zero;
    public GameObject LockedRaycastObject { get; set; } = null;
    void Start()
    {
        //_ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        _characterController = GetComponentInChildren<CharacterController>();
        _flystickController = gameObject.AddComponent<FlystickController>();
        _flystickController.Init(this);
        _ray = new Ray(_flystickController.RayLineOrigin, _flystickController.RayLineDirection);

        _SetModeController();

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log($"<color=blue> [MODE MENU]  1 -> grp  ,  2 -> inz  [MODE MENU] </color>");
    }

    void Update()
    {
        _UpdateRaycasting();        
        _UpdateMovement();
        _UpdateMode();
    }

    private void _UpdateRaycasting()
    {
        _ray.origin = _flystickController.RayLineOrigin;
        _ray.direction = _flystickController.RayLineDirection;
        //_ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (LockedRaycastObject != null)
        {
            Collider lockedCollider = LockedRaycastObject.GetComponent<Collider>();
            if (lockedCollider != null)
            {
                if (lockedCollider.Raycast(_ray, out Hit, 100))
                {
                    // Trafiamy w zafokusowany obiekt – aktualizuj punkt
                    LockedRayPoint = Hit.point;
                }
                // Jeśli nie trafiamy – nie zmieniamy Hit ani _lockedRayPoint
            }
            return; // w locku nie sprawdzamy innych obiektów
        }

        // Tryb bez locka – normalny raycast
        Physics.Raycast(_ray, out Hit, 100);
        LockedRayPoint = Hit.point;
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