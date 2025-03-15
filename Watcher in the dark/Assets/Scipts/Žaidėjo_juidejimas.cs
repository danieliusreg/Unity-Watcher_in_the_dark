using UnityEngine;
using UnityEngine.UI;

public class FirstPersonMovement : MonoBehaviour
{
    public CharacterController controller;
    public float speed = 5f;
    public float sprintSpeed = 8f; // Reguliuojamas sprinto greitis
    public float crouchSpeed = 2f; // Reguliuojamas atsitūpimo greitis
    public float gravity = 9.81f;
    
    public float sprintDuration = 3f; // Kiek ilgai galima sprintinti
    public float sprintRechargeRate = 1f; // Per kiek laiko atsistato sprintas
    public float sprintRechargeDelay = 2f; // Užlaikymas prieš pradėdamas krautis sprintas
    
    private float currentSprintTime;
    private bool isSprinting;
    private float sprintCooldownTimer = 0f;
    
    private Vector3 velocity;

    // UI
    public Slider sprintBar;

    // Crouching
    public float crouchHeight = 1f;
    public float standingHeight = 2f;
    public Transform playerCamera;
    public float crouchTransitionSpeed = 5f;
    private bool isCrouching = false;
    
    void Start()
    {
        currentSprintTime = sprintDuration;
        if (sprintBar != null)
        {
            sprintBar.maxValue = sprintDuration;
            sprintBar.value = sprintDuration;
        }
    }
    
    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        bool sprintKey = Input.GetKey(KeyCode.LeftShift);
        bool crouchKey = Input.GetKey(KeyCode.LeftControl);
        float currentSpeed = speed;
        
        if (crouchKey)
        {
            currentSpeed = crouchSpeed;
            isCrouching = true;
        }
        else
        {
            isCrouching = false;
        }
        
        if (sprintKey && currentSprintTime > 0 && !isCrouching)
        {
            currentSpeed = sprintSpeed;
            currentSprintTime -= Time.deltaTime;
            sprintCooldownTimer = 0f; // Nustatome užlaikymą į 0, kol sprintinam
        }
        else if (!sprintKey && currentSprintTime < sprintDuration)
        {
            if (sprintCooldownTimer >= sprintRechargeDelay)
            {
                currentSprintTime += Time.deltaTime * sprintRechargeRate;
            }
            else
            {
                sprintCooldownTimer += Time.deltaTime;
            }
        }

        currentSprintTime = Mathf.Clamp(currentSprintTime, 0, sprintDuration);

        // Atnaujinti sprinto juostą
        if (sprintBar != null)
        {
            sprintBar.value = currentSprintTime;
        }

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Gravitacija
        velocity.y -= gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Crouch transition
        float targetHeight = isCrouching ? crouchHeight : standingHeight;
        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
        if (playerCamera != null)
        {
            playerCamera.localPosition = new Vector3(playerCamera.localPosition.x, controller.height - 0.1f, playerCamera.localPosition.z);
        }
    }
}
