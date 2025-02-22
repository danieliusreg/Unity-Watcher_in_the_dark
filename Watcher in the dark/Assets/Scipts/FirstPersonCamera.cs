using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    public Transform player;
    public float mouseSensitivity = 2f;
    private float cameraVerticalRotation = 0f;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        float inputX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float inputY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Kamera aukštyn/žemyn (X ašis)
        cameraVerticalRotation -= inputY;
        cameraVerticalRotation = Mathf.Clamp(cameraVerticalRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(cameraVerticalRotation, 0f, 0f);

        // Žaidėjo kūno sukimas į kairę/dešinę (Y ašis)
        player.Rotate(Vector3.up * inputX);
    }
}
