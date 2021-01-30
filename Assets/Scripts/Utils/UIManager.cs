using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour {
    [Header("UI Elements")]
    public GameObject startMenu;
    public GameObject chatbox;

    [Header("Input fields")]
    public TMP_InputField usernameField;
    public TMP_InputField chatTextField;

    [Space]
    public TextMeshProUGUI chatHistoryText;
    public int chatHistoryLimit = 15;

    public static UIManager get;
    public bool chatBoxActive { get; private set; } = false;

    private bool _chatHistoryActive = false;
    private float _chatHistoryTimer = 0.0f;
    private List<string> _chatHistory = new List<string>();

    private void Awake() {
        if(get != null) {
            Logger.Log("UI", "Instance already exists, destroying new object!");
            Destroy(this);
            return;
        }

        get = this;
    }

    private void Update() {
        if(!Client.get.isConnected) return;

        if(chatBoxActive) {
            Cursor.lockState = CursorLockMode.None;
            chatTextField.interactable = true;
            chatTextField.ActivateInputField();
            chatTextField.Select();

            if(Input.GetKeyDown(KeyCode.Return)) {
                chatbox.SetActive(chatBoxActive = false);
                ClientSend.PlayerChatMessage(chatTextField.text);
                _chatHistoryTimer = 5.0f;
            }

            if(Input.GetKeyDown(KeyCode.Escape)) {
                chatbox.SetActive(chatBoxActive = false);
                chatHistoryText.gameObject.SetActive(_chatHistoryActive = false);
            }
        }
        else {
            Cursor.lockState = CursorLockMode.Locked;
            chatTextField.interactable = false;

            if(_chatHistoryTimer <= 0.0f) {
                chatHistoryText.gameObject.SetActive(_chatHistoryActive = false);
            }

            if(Input.GetKeyDown(KeyCode.T)) {
                chatbox.SetActive(chatBoxActive = true);
                chatHistoryText.gameObject.SetActive(_chatHistoryActive = true);
                chatTextField.text = "";
            }
        }

        if(_chatHistoryActive) {
            chatHistoryText.text = "";
            var messageCap = Math.Min(chatHistoryLimit, _chatHistory.Count);
            for(var i = messageCap - 1; i >= 0; --i) {
                chatHistoryText.text += _chatHistory[i] + "\n";
            }

            _chatHistoryTimer -= Time.deltaTime;
        }

    }

    public void ConnectToServer() {
        startMenu.SetActive(false);
        usernameField.interactable = false;
        Client.get.ConnectToServer();
    }

    public void AddToChatHistory(string message) {
        _chatHistory.Insert(0, message);
        chatHistoryText.gameObject.SetActive(_chatHistoryActive = true);
        _chatHistoryTimer = 5.0f;
    }
}
