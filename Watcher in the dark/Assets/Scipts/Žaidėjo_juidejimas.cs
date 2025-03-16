using UnityEngine;
using UnityEngine.UI;

public class FirstPersonMovement : MonoBehaviour
{
    public CharacterController controller;
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float crouchSpeed = 2f;
    public float gravity = 9.81f;

    public float sprintDuration = 3f;
    public float sprintRechargeRate = 1f;
    public float sprintRechargeDelay = 2f;

    private float currentSprintTime;
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

    // Išsaugoti originalius greičius
    private float originalWalkSpeed;
    private float originalSprintSpeed;
    private float originalCrouchSpeed;

    void Start()
    {
        // Išsaugoti pradinius greičius
        originalWalkSpeed = walkSpeed;
        originalSprintSpeed = sprintSpeed;
        originalCrouchSpeed = crouchSpeed;

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
        float currentSpeed = walkSpeed;

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
            sprintCooldownTimer = 0f; 
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

        if (sprintBar != null)
        {
            sprintBar.value = currentSprintTime;
        }

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * currentSpeed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y -= gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        float targetHeight = isCrouching ? crouchHeight : standingHeight;
        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
        controller.center = new Vector3(0, controller.height / 2f, 0);

        if (playerCamera != null)
        {
            Vector3 camPos = playerCamera.localPosition;
            camPos.y = Mathf.Lerp(camPos.y, targetHeight - 0.1f, Time.deltaTime * crouchTransitionSpeed);
            playerCamera.localPosition = camPos;
        }
    }

    // 📌 Sumažina greitį patekus į purvo zoną
    public void ApplySlow(float multiplier)
    {
        walkSpeed *= multiplier;
        sprintSpeed *= multiplier;
        crouchSpeed *= multiplier;
    }

    // 📌 Atkuria originalų greitį išėjus iš purvo zonos
    public void RemoveSlow()
    {
        walkSpeed = originalWalkSpeed;
        sprintSpeed = originalSprintSpeed;
        crouchSpeed = originalCrouchSpeed;
    }
}
