using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
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
    // Start is called before the first frame update
    void Start() {
        agent = GetComponent<NavMeshAgent>();
        animator= GetComponent<Animator>();
        agent.speed = AgentSpeed;
        animator.SetFloat("Speed", AgentSpeed*animationSpeed);
        animator.SetFloat("AttackSpeed", (1.7f+(AgentSpeed/1.4f ))*animationSpeed);
    }
    // Update is called once per frame
    void Update()
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
        else if (distanceFromTarget < 4.0f)
        {
            isMoving = true;
            guard.WeaponSwitchInput(2);
          
        }else if (distanceFromTarget <7.0f){
            animator.SetBool("isAwake", true);
            Invoke("StartMoving", 2.0f);
        }
    }

    void StartMoving()
    {
        isMoving = true;
        animator.SetBool("isMoving", true);
        
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
            //animator.SetTrigger("Attack");
            //agent.isStopped = true;
            //theTarget.SetActive(false);
            //theTarget.GetComponent<Animator>().SetBool("isWalking", false);
        }
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