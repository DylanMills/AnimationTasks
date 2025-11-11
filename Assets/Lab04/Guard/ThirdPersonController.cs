using DitzelGames.FastIK;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
{
    [Header("IK Settings")]
    public FastIKFabric rightHandIK;

    public FastIKFabric leftHandIK;
    public bool handsAttached = false;
    public int IKBonesCount = 2;
    public Transform rightHandTarget;
    public Transform leftHandTarget;

    [Header("Camera Orbit Settings")]
    public float mouseSensitivity = 2f;

    public float cameraDistance = 4f;
    public float cameraHeight = 1.5f;
    public float minPitch = 40f;
    public float maxPitch = 60f;

    private float yaw = 0f;
    private float pitch = 40f;

    public bool restricted = false;

    public float walkSpeed = 4f;
    public float crouchSpeed = 2f;
    public float rotationSpeed = 10f;
    public Transform cameraTransform;
    public Animator animator;

    [Header("Attack Settings")]
    public bool isBlocking = false;

    public bool isAttacking = false;
    public float attackCooldown = 0.2f;

    public Collider weaponCollider;


    [Header("Attack Slowdown Settings")]
    [Tooltip("Curve controlling how quickly the player slows down when attack interrupts movement.")]
    public AnimationCurve attackSlowdownCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    [Tooltip("Duration of the slowdown effect (seconds).")]
    public float slowdownDuration = 0.5f;

    [Tooltip("Current movement speed multiplier (1 = normal).")]
    [Range(0f, 1f)] public float movementMultiplier = 1f;

    private bool isSlowingDown;
    private float slowdownTimer;
    [Header("Jump Settings")]
    public float jumpHeight = 1.5f;

    public float groundCheckDistance = 0.2f;
    public LayerMask groundMask = ~0; // All layers by default

    private bool isGrounded = false;

    [Header("Sneak Layer Settings")]
    public int sneakLayerIndex = 1; // Index of your sneaking override layer

    public float sneakBlendSpeed = 4f; // How quickly to blend in/out
    private bool isSneaking = false;

    [Header("Crouch Layer Settings")]
    public int crouchLayerIndex = 2; // Set this to your crouch layer index in the Animator

    public float crouchBlendSpeed = 4f; // How quickly to blend in/out

    [Header("WeaponLayer Settings")]
    public int swordLayerIndex = 3;

    public int axeLayerIndex = 4;
    public int bowLayerIndex = 5;
    public int activeWeaponLayerIndex = 0;
    public int inactiveWeaponLayerIndex = 0;

    private CharacterController controller;
    private float gravity = -9.81f;
    private Vector3 velocity;
    private bool isCrouching = false;
    private float smoothTurnVelocity;


    float horizontalMaster;
    float verticalMaster;


    PlayerInventory inventory;

    private void Start()
    {
        inventory = GetComponent<PlayerInventory>();
        controller = GetComponent<CharacterController>();
        if (!cameraTransform)
            cameraTransform = Camera.main.transform;

        // Initialize yaw and pitch based on current camera
        Vector3 angles = cameraTransform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        // Initialize IK

        rightHandIK.ChainLength = IKBonesCount;
        rightHandIK.Target = rightHandTarget;
        leftHandIK.ChainLength = IKBonesCount;
        leftHandIK.Target = leftHandTarget;

        AttachHands(false);
        handsAttached = false;
    }

    private void Update()
    {
        HandleCameraOrbit();
        HandleGroundCheck();

        if (!restricted && !isAttacking)

        {
            HandleInput();

            HandleSneakLayer();
            HandleCrouchLayer();
            HandleWeaponLayer(activeWeaponLayerIndex);
            if (activeWeaponLayerIndex != inactiveWeaponLayerIndex)
            { UnequipWeaponLayer(inactiveWeaponLayerIndex); }
        }
    


        if (isSlowingDown)
        {
            slowdownTimer += Time.deltaTime;
            float t = Mathf.Clamp01(slowdownTimer / slowdownDuration);
            movementMultiplier = attackSlowdownCurve.Evaluate(t);

            if (slowdownTimer >= slowdownDuration)
                isSlowingDown = false;
        }
        
        HandleMovement();
    }

    public void AttachHands(bool attach)
    {
        rightHandIK.enabled = attach;

        leftHandIK.enabled = attach;
    }

    private void HandleGroundCheck()
    {
        // Use a simple sphere check at the feet to determine if grounded
        Vector3 spherePosition = transform.position + Vector3.up * 0.1f;
        isGrounded = Physics.CheckSphere(spherePosition, groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small downward force to keep grounded
        }

        if (animator)
        {
            animator.SetBool("IsGrounded", isGrounded);
        }
    }

    private void HandleCameraOrbit()
    {
        // Mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Calculate camera position
        Vector3 targetPosition = transform.position + Vector3.up * cameraHeight;
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 offset = rotation * new Vector3(0, 0, -cameraDistance);

        cameraTransform.position = targetPosition + offset;
        cameraTransform.rotation = rotation;
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            TryAttack();
        }
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            ToggleBlock();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            handsAttached = !handsAttached;
            AttachHands(handsAttached);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            isCrouching = !isCrouching;
            animator.SetBool("IsCrouching", isCrouching);
            isSneaking = isCrouching; // Sync sneaking with crouching
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (!isCrouching) // Only toggle sneak if not crouching
                isSneaking = !isSneaking;
        }
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
            if (animator)
                animator.SetTrigger("Jump");
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
            WeaponSwitchInput(1);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            WeaponSwitchInput(2);
      //  else if (Input.GetKeyDown(KeyCode.Alpha3))
       //     WeaponSwitchInput(3);
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            WeaponSwitchInput(4);
    }

    private void TryAttack()
    {
        if (animator && !isAttacking)
        {
            animator.SetTrigger("Attack");
            isAttacking = true;
            horizontalMaster = Input.GetAxis("Horizontal");
            verticalMaster = Input.GetAxis("Vertical");
            StartAttackSlowdown();
            Invoke(nameof(AttackFinished), attackCooldown);
            Invoke(nameof(EnableWeaponCollider), 0.05f);

        }
    }
    private void EnableWeaponCollider()
    {
        if (weaponCollider)
            weaponCollider.enabled = true;
        Invoke(nameof(DisableWeaponCollider), 0.1f);
    }
    private void DisableWeaponCollider()
    {
        if (weaponCollider)
            weaponCollider.enabled = false;
    }
    private void AttackFinished()
    {
        isAttacking = false;
    }
    private void ToggleBlock()
    {
        isBlocking = !isBlocking;
        if (animator)
        {
            animator.SetBool("Block", isBlocking);
        }
        if (isBlocking)
        {
            WeaponSwitchInput(axeLayerIndex);
        }
        else
        {
            WeaponSwitchInput(0);
        }
    }

    public void WeaponSwitchInput(int input)
    {
        if (inactiveWeaponLayerIndex <= 0)
        {
            animator.SetLayerWeight(swordLayerIndex, 0);
            animator.SetLayerWeight(axeLayerIndex, 0);
            animator.SetLayerWeight(bowLayerIndex, 0);
        }
        //animator.SetLayerWeight(activeWeaponLayerIndex, 0);
        inactiveWeaponLayerIndex = activeWeaponLayerIndex;
        //if (activeWeaponLayerIndex < 0)
        //   {
        switch (input)
        {
            case 1:
                activeWeaponLayerIndex = swordLayerIndex;
                break;

            case 2:
                activeWeaponLayerIndex = axeLayerIndex;
                break;

            case 3:
                activeWeaponLayerIndex = bowLayerIndex;
                break;

            default:
                activeWeaponLayerIndex = 0;
                //if(inactiveWeaponLayerIndex<0)
                //animator.SetLayerWeight(swordLayerIndex, 0);
                //animator.SetLayerWeight(axeLayerIndex, 0);
                //animator.SetLayerWeight(bowLayerIndex, 0);

                break;
        }

        //if(activeWeaponLayerIndex >= 0)
        //animator.SetLayerWeight(activeWeaponLayerIndex, 1);
        //}
    }

    private void UnequipWeaponLayer(int weaponIndex)
    {
        if (!animator) return;

        float targetWeight = 0f;
        float currentWeight = animator.GetLayerWeight(weaponIndex);
        float newWeight = Mathf.MoveTowards(currentWeight, targetWeight, Time.deltaTime * 6);
        animator.SetLayerWeight(weaponIndex, newWeight);
    }
    
    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        if (isAttacking)
        {
            horizontal = horizontalMaster;
            vertical = verticalMaster;
        }
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref smoothTurnVelocity, 0.1f);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
           
            float speed = isCrouching||isBlocking ? crouchSpeed : walkSpeed;
            if (isAttacking)
            {
                
                controller.Move(movementMultiplier * moveDir * speed * Time.deltaTime);
            }else
                controller.Move(moveDir * speed * Time.deltaTime);

            animator.SetFloat("Moving", 1.0f);
            animator.SetBool("IsMoving", true);
            animator.SetBool("IsCrouching", isCrouching);
        }
        else
        {
            animator.SetFloat("Moving", 0.0f);
            animator.SetBool("IsMoving", false);
        }

        // Gravity
    
      
            velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleWeaponLayer(int weaponIndex)
    {
        if (!animator) return;
        if (weaponIndex <= 0) return;

        float targetWeight = activeWeaponLayerIndex > 0 ? 1f : 0f;
        float currentWeight = animator.GetLayerWeight(weaponIndex);
        float newWeight = Mathf.MoveTowards(currentWeight, targetWeight, Time.deltaTime * 3);
        if (weaponIndex >= 0)
            animator.SetLayerWeight(weaponIndex, newWeight);
    }

    private void HandleSneakLayer()
    {
        if (!animator) return;

        float targetWeight = isSneaking ? 1f : 0f;
        float currentWeight = animator.GetLayerWeight(sneakLayerIndex);
        float newWeight = Mathf.MoveTowards(currentWeight, targetWeight, Time.deltaTime * sneakBlendSpeed);
        animator.SetLayerWeight(sneakLayerIndex, newWeight);
    }

    private void HandleCrouchLayer()
    {
        if (!animator) return;

        float targetWeight = isCrouching ? 1f : 0f;
        float currentWeight = animator.GetLayerWeight(crouchLayerIndex);
        float newWeight = Mathf.MoveTowards(currentWeight, targetWeight, Time.deltaTime * crouchBlendSpeed);
        animator.SetLayerWeight(crouchLayerIndex, newWeight);
    }







    public void StartAttackSlowdown()
    {
        isSlowingDown = true;
        slowdownTimer = 0f;
    }









}