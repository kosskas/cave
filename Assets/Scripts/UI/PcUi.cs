using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PcUi : MonoBehaviour
{
    private ModeExperimental experimental;

    private List<Button> buttons;

    public Text contextText;

    public bool areButtonsActive;

    private PlayerController _playerController;

    // Use this for initialization
    void Start () {
        GameObject canvas = GameObject.Find("PcUICanvas");
        GameObject contextTextObject = GameObject.Find("ContextText");
        contextText = contextTextObject.GetComponentInChildren<Text>();
        contextText.gameObject.SetActive(false);
        buttons = new List<Button>(canvas.GetComponentsInChildren<Button>());
        foreach (var button in buttons)
        {
            button.onClick.AddListener(() => SetContext(button.GetComponentInChildren<Text>().text));
            button.gameObject.SetActive(false);
        }

        areButtonsActive = false;
    }
	
	// Update is called once per frame
	void Update () {
        if (experimental == null)
        {
            GameObject player = GameObject.Find("FPSPlayer");
            _playerController = player.GetComponent<PlayerController>();
            experimental = (ModeExperimental) _playerController.GetModeController();
            Debug.Log("ustawiono exp");
        }
    }

    public void SetContext(string context)
    {
		Debug.Log(context);
        experimental.SetContext(context);
        contextText.text = context;
        SetButtonsInactive();
    }

    public void SetButtonsActive()
    {
        Cursor.lockState = CursorLockMode.None; //moze zmienic na Confined, ale wtedy sie buguje w edytorze
        Cursor.visible = true;
        foreach (var button in buttons)
        {
            button.gameObject.SetActive(true);
        }

        _playerController.lookSpeed = 0.0f;
        areButtonsActive = true;
        contextText.gameObject.SetActive(false);
    }

    public void SetButtonsInactive()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        foreach (var button in buttons)
        {
            button.gameObject.SetActive(false);
        }

        _playerController.lookSpeed = 2.0f;
        areButtonsActive = false;
        contextText.gameObject.SetActive(true);
    }

    public void SwitchButtonsActive()
    {
        if (areButtonsActive)
        {
            SetButtonsInactive();
        }
        else
        {
            SetButtonsActive();
        }
    }
}
