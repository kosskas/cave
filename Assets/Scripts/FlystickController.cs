using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class FlystickController : MonoBehaviour
{
    [SerializeField] GameObject flystick;
    LineSegment rayline;

    public Vector3 RayLineOrigin = Vector3.zero;
    public Vector3 RayLineDirection = Vector3.zero;

    public enum Btn
    {
        _1,
        _2,
        _3,
        _4,
        JOYSTICK,
        FIRE
    }

    public enum ActOn
    {
        PRESS,
        RELEASE,
        TILT_LEFT,
        TILT_RIGHT,
        TILT_UP,
        TILT_DOWN
    }

    private PlayerController _PCref;

    private static Action _OnJoystickTiltedLeft = () => { };
    private static Action _OnJoystickTiltedRight = () => { };
    private static Action _OnJoystickTiltedUp = () => { };
    private static Action _OnJoystickTiltedDown = () => { };

    private const float _RAY_WEIGHT = 0.005f;
    private const float _RAY_RANGE = 100f;
    private const float _LABEL_SIZE = 0.01f;
    private Color _RAY_COLOR = Color.red;
    private Color _LABEL_COLOR = Color.white;
    private bool _isTiltedX = false;
    private bool _isTiltedY = false;

    public void Init(PlayerController PCref)
    {
        _PCref = PCref;
        flystick = GameObject.Find("TrackedObject");
        rayline = flystick.AddComponent<LineSegment>();
        rayline.SetStyle(_RAY_COLOR, _RAY_WEIGHT);
        rayline.SetLabel("", _LABEL_SIZE, _LABEL_COLOR);
        _TransformRayline();
    }

    void Update()
    {
        _TransformRayline();
        _HandleJoystickTiltActions();
    }

    private void _TransformRayline()
    {
        RayLineOrigin = flystick.transform.position;
        RayLineDirection = flystick.transform.forward * _RAY_RANGE;
        if (_PCref.LockedRaycastObject != null)
        {
            // Kiedy mamy locka – linia zawsze idzie do zapamiętanego punktu
            rayline.SetCoordinates(RayLineOrigin, _PCref.LockedRayPoint);
        }
        else
        {
            // Normalny tryb – idziemy do aktualnego miejsca raycastu
            rayline.SetCoordinates(RayLineOrigin, RayLineDirection);
        }
    }

    private void _HandleJoystickTiltActions()
    {
        if (Lzwp.sync.isMaster)
        {
            int flystickIdx = 0;

            if (Lzwp.input.flysticks.Count > flystickIdx)
            {
                float x = Lzwp.input.flysticks[flystickIdx].joysticks[0];
                if (x <= -0.8f)
                {
                    if (_isTiltedX == false)
                    {
                        _OnJoystickTiltedLeft();
                        _isTiltedX = true;
                    }
                }
                else if (x >= 0.8f)
                {
                    if (_isTiltedX == false)
                    {
                        _OnJoystickTiltedRight();
                        _isTiltedX = true;
                    }
                }
                else
                {
                    _isTiltedX = false;
                }

                float y = Lzwp.input.flysticks[flystickIdx].joysticks[1];
                if (y <= -0.8f)
                {
                    if (_isTiltedY == false)
                    {
                        _OnJoystickTiltedDown();
                        _isTiltedY = true;
                    }
                }
                else if (y >= 0.8f)
                {
                    if (_isTiltedY == false)
                    {
                        _OnJoystickTiltedUp();
                        _isTiltedY = true;
                    }
                }
                else
                {
                    _isTiltedY = false;
                }
            }
        }
    }


    public static void ClearActions()
    {
        Lzwp.input.flysticks[0].GetButton(LzwpInput.Flystick.ButtonID.Button1).OnPress = delegate { };
        Lzwp.input.flysticks[0].GetButton(LzwpInput.Flystick.ButtonID.Button1).OnRelease = delegate { };

        Lzwp.input.flysticks[0].GetButton(LzwpInput.Flystick.ButtonID.Button2).OnPress = delegate { };
        Lzwp.input.flysticks[0].GetButton(LzwpInput.Flystick.ButtonID.Button2).OnRelease = delegate { };

        Lzwp.input.flysticks[0].GetButton(LzwpInput.Flystick.ButtonID.Button3).OnPress = delegate { };
        Lzwp.input.flysticks[0].GetButton(LzwpInput.Flystick.ButtonID.Button3).OnRelease = delegate { };

        Lzwp.input.flysticks[0].GetButton(LzwpInput.Flystick.ButtonID.Button4).OnPress = delegate { };
        Lzwp.input.flysticks[0].GetButton(LzwpInput.Flystick.ButtonID.Button4).OnRelease = delegate { };

        Lzwp.input.flysticks[0].GetButton(LzwpInput.Flystick.ButtonID.Joystick).OnPress = delegate { };
        Lzwp.input.flysticks[0].GetButton(LzwpInput.Flystick.ButtonID.Joystick).OnRelease = delegate { };
        _OnJoystickTiltedLeft = () => { };
        _OnJoystickTiltedRight = () => { };
        _OnJoystickTiltedUp = () => { };
        _OnJoystickTiltedDown = () => { };

        Lzwp.input.flysticks[0].GetButton(LzwpInput.Flystick.ButtonID.Fire).OnPress = delegate { };
        Lzwp.input.flysticks[0].GetButton(LzwpInput.Flystick.ButtonID.Fire).OnRelease = delegate { };
    }

    public static void SetAction(Btn btn, ActOn actOn, Action action)
    {
        if (Lzwp.sync.isMaster)
        {
            switch (btn)
            {
                case Btn._1:
                    switch (actOn)
                    {
                        case ActOn.PRESS:
                            Lzwp.input.flysticks[0].GetButton(LzwpInput.Flystick.ButtonID.Button1).OnPress += action;
                            break;
                        case ActOn.RELEASE:
                            Lzwp.input.flysticks[0].GetButton(LzwpInput.Flystick.ButtonID.Button1).OnRelease += action;
                            break;
                        default:
                            break;
                    }
                    break;

                case Btn._2:
                    switch (actOn)
                    {
                        case ActOn.PRESS:
                            Lzwp.input.flysticks[0].GetButton(LzwpInput.Flystick.ButtonID.Button2).OnPress += action;
                            break;
                        case ActOn.RELEASE:
                            Lzwp.input.flysticks[0].GetButton(LzwpInput.Flystick.ButtonID.Button2).OnRelease += action;
                            break;
                        default:
                            break;
                    }
                    break;

                case Btn._3:
                    switch (actOn)
                    {
                        case ActOn.PRESS:

                            Lzwp.input.flysticks[0].GetButton(LzwpInput.Flystick.ButtonID.Button3).OnPress += action;
                            break;
                        case ActOn.RELEASE:
                            Lzwp.input.flysticks[0].GetButton(LzwpInput.Flystick.ButtonID.Button3).OnRelease += action;
                            break;
                        default:
                            break;
                    }
                    break;

                case Btn._4:
                    switch (actOn)
                    {
                        case ActOn.PRESS:
                            Lzwp.input.flysticks[0].GetButton(LzwpInput.Flystick.ButtonID.Button4).OnPress += action;
                            break;
                        case ActOn.RELEASE:
                            Lzwp.input.flysticks[0].GetButton(LzwpInput.Flystick.ButtonID.Button4).OnRelease += action;
                            break;
                        default:
                            break;
                    }
                    break;

                case Btn.JOYSTICK:
                    switch (actOn)
                    {
                        case ActOn.PRESS:
                            Lzwp.input.flysticks[0].GetButton(LzwpInput.Flystick.ButtonID.Joystick).OnPress += action;
                            break;
                        case ActOn.RELEASE:
                            Lzwp.input.flysticks[0].GetButton(LzwpInput.Flystick.ButtonID.Joystick).OnRelease += action;
                            break;
                        case ActOn.TILT_LEFT:
                            _OnJoystickTiltedLeft += action;
                            break;
                        case ActOn.TILT_RIGHT:
                            _OnJoystickTiltedRight += action;
                            break;
                        case ActOn.TILT_UP:
                            _OnJoystickTiltedUp += action;
                            break;
                        case ActOn.TILT_DOWN:
                            _OnJoystickTiltedDown += action;
                            break;
                        default:
                            break;
                    }
                    break;

                case Btn.FIRE:
                    switch (actOn)
                    {
                        case ActOn.PRESS:
                            Lzwp.input.flysticks[0].GetButton(LzwpInput.Flystick.ButtonID.Fire).OnPress += action;
                            break;
                        case ActOn.RELEASE:
                            Lzwp.input.flysticks[0].GetButton(LzwpInput.Flystick.ButtonID.Fire).OnRelease += action;
                            break;
                        default:
                            break;
                    }
                    break;

                default:
                    break;
            }
        }

    }
}