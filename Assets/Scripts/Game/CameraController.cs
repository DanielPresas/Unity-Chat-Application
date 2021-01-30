using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    public PlayerManager player;
    public float sensitivity = 100.0f;
    public float clampAngle = 85.0f;

    private float verticalRotation;
    private float horizontalRotation;

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        verticalRotation   = transform.localEulerAngles.x;
        horizontalRotation = player.transform.eulerAngles.y;
    }

    void Update() {
        Debug.DrawRay(transform.position, transform.right * 2.5f, Color.red);
        Debug.DrawRay(transform.position, transform.up * 2.5f, Color.green);
        Debug.DrawRay(transform.position, transform.forward * 2.5f, Color.blue);

        if(UIManager.get.chatBoxActive) return;
        
        float mouseH = Input.GetAxisRaw("Mouse X");
        float mouseV = -Input.GetAxisRaw("Mouse Y");

        horizontalRotation += mouseH * sensitivity * Time.deltaTime;
        verticalRotation   += mouseV * sensitivity * Time.deltaTime;
        verticalRotation    = Mathf.Clamp(verticalRotation, -clampAngle, clampAngle);

        transform.localRotation   = Quaternion.Euler(verticalRotation, 0.0f, 0.0f);
        player.transform.rotation = Quaternion.Euler(0.0f, horizontalRotation, 0.0f);
    }
}
