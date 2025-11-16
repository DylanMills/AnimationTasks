using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine.InputSystem.XR;
using NUnit.Framework.Constraints;
public class navscript : MonoBehaviour
{
    public Guard guard;
    public GameObject theTarget;
    private NavMeshAgent agent;
    private Animator animator;
    public bool isMoving=false;
    public float AgentSpeed= 3.5f;
    private float animationSpeed=1.0f/3.5f;
    public bool isAwake;
    public float distanceFromTarget;

    public CharacterController controller;
    public float knockbackStrength = 30f;
    public float knockbackDecay = 3f; // higher = faster slowdown
    public AnimationCurve knockbackCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private Vector3 knockbackVelocity;
    public float knockbackTimer;
    public bool isKnockedback=false;

    // Start is called before the first frame update
    void Start() {
        agent = GetComponent<NavMeshAgent>();
        animator= GetComponent<Animator>();
        agent.speed = AgentSpeed;
        animator.SetFloat("Speed", AgentSpeed*animationSpeed);
        animator.SetFloat("AttackSpeed", (1.7f+(AgentSpeed/1.4f ))*animationSpeed);
    }
    // Update is called once per frame

    void DetermineMovement()
    {

        distanceFromTarget = Vector3.Distance(theTarget.transform.position, transform.position);

        if (distanceFromTarget < 1.0f)
        {
            isMoving = false;
            animator.SetTrigger("Attack");
        }
        else if (distanceFromTarget < 3.0f)
        {
            isMoving = true;
            guard.WeaponSwitchInput(2);

        }
         if (distanceFromTarget < 8.0f&&!isKnockedback&&!isMoving)
        {
            animator.SetBool("isAwake", true);
            Invoke("StartMoving", 2.0f);
        }
        if (distanceFromTarget > 4.0f && isAwake)
        {
            guard.WeaponSwitchInput(0);
        }
        if (isMoving)
            agent.destination = theTarget.transform.position;
        else
            agent.destination = transform.position;

        if (isKnockedback)
        {
            agent.destination = transform.position;
        }

    }
    void Update()
    {if(guard.health <= 0&&isMoving)
        {
        Invoke("Death", 0.05f);
        }


        if (Input.GetKeyDown(KeyCode.K)) // Example trigger
        {
            //Debug.Log("Knockback Applied");
            Vector3 source = theTarget.transform.position; // example attacker position
            ApplyKnockback(source, knockbackStrength);
        }

        if (isKnockedback)
        {

            knockbackTimer += Time.deltaTime * knockbackDecay;
            float t = Mathf.Clamp01(knockbackTimer);
            float strength = knockbackCurve.Evaluate(t);

            controller.Move(knockbackVelocity * strength * Time.deltaTime);

            if (t >= 1f)
            {
                Debug.Log("Knockback ended: t="+t);
                StartMoving();
                isKnockedback = false;
            }

        }
        else
        {
            DetermineMovement();
        }
    }
    void Death()
    {
        Debug.Log("Death called");

        if (!isMoving)
        {
            return;
        }
        agent.isStopped = true;
        isMoving = false;
        animator.SetBool("isMoving", false);
        guard.isRestricted = true;
        return;
    }
    void StartMoving()
    {
        Debug.Log("StartMoving called");
        isMoving = true;
        animator.SetBool("isMoving", true);
        isKnockedback = false;
        guard.isRestricted = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("hitbox"))
        {
           Debug.Log("Attack Landed");
            Debug.Log("Knockback Applied");
            Vector3 source = theTarget.transform.position; // example attacker position
            ApplyKnockback(source, knockbackStrength);
            guard.TakeDamage();
        }
    }


    public void ApplyKnockback(Vector3 sourcePosition, float strength)
    {
        Vector3 direction = (transform.position - sourcePosition).normalized;
        direction.y = 0.01f; // optional — keep it horizontal
        knockbackVelocity = direction * strength;
        knockbackTimer = 0f;
        isKnockedback = true;
        guard.isRestricted = true;
    }

}