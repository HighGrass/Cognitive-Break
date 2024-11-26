using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 8f; //Velocidade
    public float mouseSensitivity = 100f; //Sensibilidade do rato

    private float verticalRotation = 0f;
    public Transform playerCamera; //Câmera

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; //Bloqueio do cursor
    }

    private void Update()
    {
        float horizontal = Input.GetAxis("Horizontal"); //Movimentação teclas A e D
        float vertical = Input.GetAxis("Vertical"); //Movimentação teclas W e S
        Vector3 movement = transform.right * horizontal + transform.forward * vertical;
        transform.Translate(movement * speed * Time.deltaTime, Space.World);

        //Rotação do rato
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        //Rotação horizontal
        transform.Rotate(Vector3.up * mouseX);

        //Rotação vertical
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f); //Limitação de angulo
        playerCamera.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }
}
