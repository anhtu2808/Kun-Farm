using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mevement : MonoBehaviour
{
    public float speed ; // Speed of the movement
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(horizontal, vertical);
        transform.position += direction * speed * Time.deltaTime;
    }
}
