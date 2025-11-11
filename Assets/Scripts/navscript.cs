using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine.InputSystem.XR;
public class navscript : MonoBehaviour
{
    public Guard guard;
    public GameObject theTarget;
    private NavMeshAgent agent;
    private Animator animator;
    bool isMoving=false;
    public float AgentSpeed= 3.5f;
    private float animationSpeed=1.0f/3.5f;
    public bool isAwake;
    public float distanceFromTarget;

    public CharacterController controller;
    public float knockbackStrength = 30f;
    public float knockbackDecay = 3f; // higher = faster slowdown
    public AnimationCurve knockbackCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private Vector3 knockbackVelocity;
    private float knockbackTimer;
    private bool isKnockedback;

    // Start is called before the first frame update
    void Start() {
        agent = GetComponent<NavMeshAgent>();
        animator= GetComponent<Animator>();
        agent.speed = AgentSpeed;
        animator.SetFloat("Speed", AgentSpeed*animationSpeed);
        animator.SetFloat("AttackSpeed", (1.7f+(AgentSpeed/1.4f ))*animationSpeed);
        animator.SetBool("isAwake", isAwake);
        if (isAwake)
        {
            Invoke("StartMoving", 1.0f);
        }
    }
    // Update is called once per frame
    void Update()
    {if(guard.health <= 0)
        {
            agent.isStopped = true;
            isMoving = false;
            animator.SetBool("isMoving", false);
            guard.isRestricted = true;
            return;
        }
        if (theTarget != null)
        {
            if (isMoving)
                agent.destination = theTarget.transform.position;
            else
                agent.destination = transform.position;
        

        distanceFromTarget = Vector3.Distance(theTarget.transform.position, transform.position);
        if(distanceFromTarget < 1.0f)
        {
            isMoving = false;
            animator.SetTrigger("Attack");
        }
        else if (distanceFromTarget < 3.0f)
        {
            isMoving = true;
            guard.WeaponSwitchInput(2);
          
        }else if (distanceFromTarget <8.0f){
            animator.SetBool("isAwake", true);
            Invoke("StartMoving", 3.0f);
        }
        if (distanceFromTarget>4.0f && isAwake)
        {
            guard.WeaponSwitchInput(0);
        }



        if (Input.GetKeyDown(KeyCode.K)) // Example trigger
        {
            Debug.Log("Knockback Applied");
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
                Invoke("StartMoving", 0.6f);
                guard.isRestricted = false;
            }
            else
            {
                guard.isRestricted = true;
            }
            }
        }
    }

    void StartMoving()
    {
        isMoving = true;
        animator.SetBool("isMoving", true);
        isKnockedback = false;
        
    }

    //private void OnTriggerEnter(Collider other)
    //{

    //    agent.isStopped = true;
    //    theTarget.SetActive(false);
    //    theTarget.GetComponent<Animator>().SetBool("isWalking", false);

    //}


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("hitbox"))
        {
           Debug.Log("Attack Landed");
            Debug.Log("Knockback Applied");
            Vector3 source = theTarget.transform.position; // example attacker position
            ApplyKnockback(source, knockbackStrength);
            guard.TakeDamage();
            //animator.SetTrigger("Attack");
            //agent.isStopped = true;
            //theTarget.SetActive(false);
            //theTarget.GetComponent<Animator>().SetBool("isWalking", false);
        }
    }


    public void ApplyKnockback(Vector3 sourcePosition, float strength)
    {
        Vector3 direction = (transform.position - sourcePosition).normalized;
        direction.y = 0; // optional — keep it horizontal
        knockbackVelocity = direction * strength;
        knockbackTimer = 0f;
        isKnockedback = true;
    }
    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.name == "Player")
    //    {
    //        isWalking = true;
    //        animator.SetTrigger("WALK");
    //    }
    //} 
}