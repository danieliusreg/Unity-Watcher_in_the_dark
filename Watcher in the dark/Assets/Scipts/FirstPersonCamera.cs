using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    public Transform playerBody; // Žaidėjo kūnas, kuris suksis aplink vertikalę
    public float mouseSensitivity = 200f;

    private float cameraVerticalRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Gauti pelės judėjimą
        float inputX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float inputY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Kamera aukštyn/žemyn
        cameraVerticalRotation -= inputY;
        cameraVerticalRotation = Mathf.Clamp(cameraVerticalRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(cameraVerticalRotation, 0f, 0f);

        // Žaidėjo kūno sukimas
        playerBody.Rotate(Vector3.up * inputX);
    }
}
