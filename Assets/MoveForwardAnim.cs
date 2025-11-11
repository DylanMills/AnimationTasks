using UnityEngine;

public class MoveForwardAnim : MonoBehaviour
{
    public float speed = 5f; // Speed of movement

    void Update()
    {
        // Move the object forward relative to its local Z-axis (blue arrow)
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}