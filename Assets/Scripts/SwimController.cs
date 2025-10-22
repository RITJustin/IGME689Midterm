using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SwimController : MonoBehaviour
{
    private CharacterController controller;

    [Header("Swimming Settings")]
    public float swimSpeed = 5f;            // Horizontal movement speed
    public float swimUpSpeed = 3f;          // Vertical up speed (Space)
    public float swimDownSpeed = 3f;        // Vertical down speed (Ctrl)
    public float floatSmoothSpeed = 3f;     // Smooth floating at surface
    public float floatOffset = 0.2f;        // Small offset above water surface

    private bool isSwimming = false;
    private Collider currentWater = null;

    void Start()
    {
        // Find the CharacterController automatically
        controller = GetComponent<CharacterController>();
        if (controller == null)
            controller = GetComponentInChildren<CharacterController>();

        if (controller == null)
            Debug.LogError("No CharacterController found on player or children!");
    }

    void Update()
    {
        if (isSwimming && currentWater != null)
        {
            SwimMovement();
        }
        else
        {
            WalkMovement();
        }
    }

    // Swim logic
    void SwimMovement()
    {
        // Horizontal input
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 horizontalMove = transform.right * h + transform.forward * v;
        horizontalMove *= swimSpeed;

        // Vertical input
        float verticalMove = 0f;
        if (Input.GetKey(KeyCode.Space))
            verticalMove = swimUpSpeed;
        else if (Input.GetKey(KeyCode.LeftControl))
            verticalMove = -swimDownSpeed;

        // Float at water surface
        float targetY = currentWater.bounds.max.y + floatOffset;
        float floatAdjustment = 0f;
        if (transform.position.y < targetY)
        {
            floatAdjustment = Mathf.Lerp(0, targetY - transform.position.y, Time.deltaTime * floatSmoothSpeed);
        }

        // Combine final movement
        Vector3 finalMove = horizontalMove + Vector3.up * (verticalMove + floatAdjustment);
        controller.Move(finalMove * Time.deltaTime);
    }

    // Walk/fall logic
    void WalkMovement()
    {
        Vector3 move = Vector3.zero;
        move.y += Physics.gravity.y * Time.deltaTime;
        controller.Move(move * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            isSwimming = true;
            currentWater = other;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Water") && other == currentWater)
        {
            isSwimming = false;
            currentWater = null;
        }
    }
}
