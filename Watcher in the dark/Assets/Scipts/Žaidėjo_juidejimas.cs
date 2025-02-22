using UnityEngine;

public class FirstPersonMovement : MonoBehaviour
{
    public CharacterController controller;
    public float speed = 5f;

    void Update()
    {
        float moveX = Input.GetAxis("Horizontal"); // A/D arba ←/→
        float moveZ = Input.GetAxis("Vertical");   // W/S arba ↑/↓

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * speed * Time.deltaTime);
    }
}

