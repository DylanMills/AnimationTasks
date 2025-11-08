using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class VaultAndClimb : MonoBehaviour
{
    [Header("References")]
    public LayerMask obstacleMask;
    public Transform cameraTransform;
    public Animator animator;
    public ThirdPersonController thirdPersonController;

    public float armsWidth = 1.5f;

    [Header("Vault Settings")]
    public AnimationCurve vaultCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float vaultCheckDistance = 1.2f;
    public float vaultHeight = 0.2f;
    public float vaultOverHeight = 1.0f;

    [Header("Climb Settings")]
    public float climbCheckDistance = 1.0f;
    public float climbHeightMin = 1.3f;
    public float climbHeightMax = 2.2f;

    [Header("Debug Settings")]
    public bool showDebug = true;
    public Color vaultRayColor = Color.green;
    public Color climbRayColor = Color.cyan;
    public Color hitColor = Color.yellow;
    public Color targetColor = Color.magenta;

    private CharacterController controller;
    private bool isVaulting;
    private bool isClimbing;
    private Vector3 lastHitPoint;
    private Vector3 targetPosition;
    private string debugMessage = "";

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (!cameraTransform)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        if (isVaulting || isClimbing)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryVaultOrClimb();
        }
    }

    void TryVaultOrClimb()
    {
        Vector3 origin = transform.position + Vector3.up * (controller.height * 0.6f);

        Vector3 dir = thirdPersonController.transform.forward;
        dir.y = 0;
        dir.Normalize();

        bool hitSomething = Physics.Raycast(origin, dir, out RaycastHit hit, vaultCheckDistance, obstacleMask);

        if (showDebug)
            Debug.DrawRay(origin, dir * vaultCheckDistance, vaultRayColor, 1f);

        if (!hitSomething)
        {
            debugMessage = "No obstacle detected.";
            return;
        }

        lastHitPoint = hit.point;

        // Cast a ray downward from slightly above the obstacle’s hit point
        Vector3 topCheckOrigin = hit.point + Vector3.up * climbHeightMax;
        bool foundTop = Physics.Raycast(topCheckOrigin, Vector3.down, out RaycastHit topHit, climbHeightMax * 1.5f, obstacleMask);

        float surfaceHeight = foundTop ? topHit.point.y : hit.point.y;
        float obstacleHeight = surfaceHeight - transform.position.y;

        if (showDebug)
        {
            Debug.DrawRay(topCheckOrigin, Vector3.down * (climbHeightMax * 1.5f), Color.cyan, 1f);
            Debug.DrawLine(hit.point, topHit.point, Color.magenta);
            Debug.Log($"[VaultCheck] Obstacle top at {surfaceHeight:F2}, height diff {obstacleHeight:F2}");
        }

        if (!foundTop)
        {
            debugMessage = "No surface found on obstacle.";
            return;
        }

        if (obstacleHeight < vaultHeight)
        {
            Vector3 vaultEnd = topHit.point + dir * 0.8f;
            vaultEnd.y = surfaceHeight;
            targetPosition = vaultEnd;
            debugMessage = $"Vaultable obstacle ({obstacleHeight:F2}m).";
            StartCoroutine(Vault(vaultEnd));
        }
        else if (obstacleHeight >= climbHeightMin && obstacleHeight <= climbHeightMax)
        {
            //Vector3 climbEnd = topHit.point + dir * 0.5f;
            //climbEnd.y = surfaceHeight + 1.0f;
            //targetPosition = climbEnd;
            //debugMessage = $"Climbable obstacle ({obstacleHeight:F2}m).";
            //StartCoroutine(Climb(climbEnd));
        }
        else
        {
            debugMessage = $"Obstacle too tall ({obstacleHeight:F2}m).";
        }
    }


    IEnumerator Vault(Vector3 target)
    {
        SetHands(target);
        thirdPersonController.restricted = true;

        isVaulting = true;
        animator.SetTrigger("Vault");
        yield return new WaitForSeconds(0.15f);

        Vector3 start = transform.position;
        float totalTime = 1f; // duration scale (adjust or tie to animation length)
        float elapsed = 0f;

        //if (debugVault)
            //Debug.Log($"[Vault] Starting vault from {start} to {target}");

        while (elapsed < totalTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / totalTime);

            // Base linear interpolation for horizontal movement
            Vector3 flatPos = Vector3.Lerp(start, target, t);

            // Add height offset using the curve
            float heightOffset = vaultCurve.Evaluate(t) * vaultHeight*0.1f;
            transform.position = flatPos + Vector3.up * heightOffset;

            //if (debugVault)
                //Debug.DrawLine(start, target, Color.yellow);

            yield return null;
        }

        transform.position = target;
        DetatchHands();
        isVaulting = false;
        thirdPersonController.restricted = false;
        //if (debugVault)
            //Debug.Log("[Vault] Vault complete");
    }

    void SetHands(Vector3 target)
    {
        //target.y += 1.5f; // Adjust hand height slightly
        thirdPersonController.rightHandTarget.position = target + (thirdPersonController.transform.right * armsWidth);

        thirdPersonController.leftHandTarget.position = target + (thirdPersonController.transform.right * -armsWidth);
        thirdPersonController.AttachHands(true);

    }
    void DetatchHands()
    {
        thirdPersonController.AttachHands(false);
    }


    IEnumerator Climb(Vector3 target)
    {
        SetHands(target);
        isClimbing = true;
        animator.SetTrigger("Climb");
        yield return new WaitForSeconds(0.25f);

        Vector3 start = transform.position;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }
        DetatchHands();
        isClimbing = false;
    }

    void OnDrawGizmos()
    {
        if (!showDebug || !Application.isPlaying)
            return;

        Gizmos.color = vaultRayColor;
        Vector3 origin = transform.position + Vector3.up * 1f;
        Vector3 dir = cameraTransform ? cameraTransform.forward : transform.forward;
        dir.y = 0;
        dir.Normalize();

        // Draw ray
        Gizmos.DrawLine(origin, origin + dir * vaultCheckDistance);

        // Draw hit point
        Gizmos.color = hitColor;
        Gizmos.DrawSphere(lastHitPoint, 0.05f);

        // Draw target position
        Gizmos.color = targetColor;
        Gizmos.DrawSphere(targetPosition, 0.07f);

#if UNITY_EDITOR
        // Display message in Scene view
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, debugMessage);
#endif
    }
}
