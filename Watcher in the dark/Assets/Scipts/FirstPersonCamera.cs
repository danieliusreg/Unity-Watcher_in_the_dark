using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    public Transform player;
    public float mouseSensitivity = 200f;
    private float cameraVerticalRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float inputX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float inputY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Kamera aukštyn/žemyn
        cameraVerticalRotation -= inputY;
        cameraVerticalRotation = Mathf.Clamp(cameraVerticalRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(cameraVerticalRotation, 0f, 0f);

        // Žaidėjo kūno sukimas
        player.Rotate(Vector3.up * inputX);
    }
}
