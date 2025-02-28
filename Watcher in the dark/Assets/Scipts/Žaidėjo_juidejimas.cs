using UnityEngine;

public class FirstPersonMovement : MonoBehaviour
{
    public CharacterController controller;
    public float speed = 5f;
    public float gravity = 9.81f;

    private Vector3 velocity;
    
    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * speed * Time.deltaTime);

        // Gravitacija
        velocity.y -= gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}


