using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mevement : MonoBehaviour
{
    public float speed; // Speed of the movement
    public Animator animator;
    private Vector3 direction; // Direction of movement
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

         direction = new Vector3(horizontal, vertical);

        AnimateMovement(direction);
        transform.position += direction * speed * Time.deltaTime;
    }
    private void FixedUpdate()
    {
        this.transform.position += direction.normalized * speed * Time.deltaTime;
    }
    void AnimateMovement(Vector3 direction)
    {
    if(animator != null)
       {
            if (animator != null)
            {
                if (direction.magnitude > 0)
                {
                    animator.SetBool("isMoving", true);
                    animator.SetFloat("horizontal", direction.x);
                    animator.SetFloat("vertical", direction.y);
                }
                else
                {
                    animator.SetBool("isMoving", false);
                }
                
           }
       }
    }
}
