using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
{
    [Header("Player")]
    public float MoveSpeed = 4.0f;
    public float SprintSpeed = 6.0f;
    public float RotationSmoothTime = 0.12f;

    [Header("Jump & Gravity")]
    public float JumpHeight = 1.2f;
    public float Gravity = -9.81f;
    public float TerminalVelocity = -53f;

    [Header("Grounded")]
    public Transform GroundCheck;
    public float GroundedOffset = -0.14f;
    public float GroundedRadius = 0.28f;
    public LayerMask GroundLayers;

    [Header("Swimming")]
    public LayerMask WaterLayer;
    public float SwimSpeed = 2f;

    private float _rotationVelocity;
    private float _verticalVelocity;
    private bool _grounded;
    private CharacterController _controller;
    private Vector3 _moveDirection;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        GroundedCheck();
        HandleGravity();
        HandleMovement();
    }

    private void GroundedCheck()
    {
        _grounded = Physics.CheckSphere(GroundCheck.position + Vector3.up * GroundedOffset, GroundedRadius, GroundLayers);
        if (_grounded && _verticalVelocity < 0f)
        {
            _verticalVelocity = -2f; // keep grounded
            OnLand(); // stub for AnimationEvent
        }
    }

    private void HandleGravity()
    {
        if (!_grounded)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
            if (_verticalVelocity < TerminalVelocity)
                _verticalVelocity = TerminalVelocity;
        }

        _controller.Move(new Vector3(0f, _verticalVelocity, 0f) * Time.deltaTime);
    }

    private void HandleMovement()
    {
        float inputX = Input.GetAxis("Horizontal");
        float inputZ = Input.GetAxis("Vertical");

        Vector3 inputDir = new Vector3(inputX, 0f, inputZ).normalized;
        if (inputDir.magnitude >= 0.1f)
        {
            // Rotate smoothly
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _rotationVelocity, RotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Move
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            _moveDirection = moveDir * MoveSpeed * Time.deltaTime;

            // Swimming
            if (Physics.CheckSphere(transform.position, 0.5f, WaterLayer))
            {
                _moveDirection.y = 0f;
                _moveDirection *= SwimSpeed / MoveSpeed;
            }

            _controller.Move(_moveDirection);
        }
    }

    public void Jump()
    {
        if (_grounded)
        {
            _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
        }
    }

    // Stub to avoid AnimationEvent error
    public void OnLand() { }
}
