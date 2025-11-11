using UnityEngine;
using System.Collections.Generic;
public class PathController : MonoBehaviour
{
    [SerializeField]
    public PathManager pathManager;

    List<Waypoint> thePath;

    Waypoint target;

    public float MoveSpeed;
    public float RotateSpeed;

    public Animator animator;
    bool isWalking;

    bool canTrigger = true;

    int i = -1;

    void Start()
    {
        isWalking = true;
        animator.SetBool("isWalking", isWalking);
     
        thePath = pathManager.GetPath();
        if (thePath != null && thePath.Count > 0)
        {
            target = thePath[0];
        }
    }



    void Update()
    {
      //  if(Input.anyKeyDown)
      //  {
       //     isWalking = !isWalking;
      //      animator.SetBool("isWalking", isWalking);
      //  }
        if (isWalking)
        {
            rotateTowardsTarget();
            MoveForward();
        } else if (i > 0)
        {
            isWalking = false;
            animator.SetBool("isWalking", isWalking);         
            //i = -1;
        }
    }

    void rotateTowardsTarget()
    {
        float stepSize = RotateSpeed * Time.deltaTime;

        Vector3 targetDir = target.pos - transform.position;
        Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, stepSize, 0.0f);


        transform.rotation = Quaternion.LookRotation(newDir);
    }
    void DisableCollision()
    {
        bool canTrigger = false;
        Invoke("EnableCollision", 2.0f);

    }
    void EnableCollision()
    {
        bool canTrigger = true;
    }
    void MoveForward()
    {
        float stepSize = MoveSpeed * Time.deltaTime;
        float distanceToTarget = Vector3.Distance(transform.position, target.pos);

        Vector3 moveDir = Vector3.forward;
        transform.Translate(moveDir * stepSize);
    }


    private void OnTriggerEnter(Collider other)
    {
        DisableCollision();
        target = pathManager.GetNextTarget();
        
    }
  
    private void OnCollisionEnter(Collision collision)
    {
        i++;
        if (i > 0){
            animator.SetTrigger("Hit");
            isWalking = false;
            animator.SetBool("isWalking", isWalking);
            // target = pathManager.GetNextTarget(); 

        }
    }




}
