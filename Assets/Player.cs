using UnityEngine;
using System.Collections;

// This script moves the character controller forward
// and sideways based on the arrow keys.
// It also jumps when pressing space.
// Make sure to attach a character controller to the same game object.
// It is recommended that you make only one call to Move or SimpleMove per frame.

public class Player : MonoBehaviour
{
    public float speed = 6.0f;
    public float _sensitivity = 1.0f;

    private Vector3 moveDirection = Vector3.zero;

    void Start()
    {
        
    }   

    void Update()
    {
        //if (characterController.isGrounded)
        //{
            // We are grounded, so recalculate
            // move direction directly from axes

            //Debug.Log("forward: " + transform.forward);
            moveDirection = new Vector3( Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
            transform.Translate(moveDirection * Time.deltaTime * speed);

            float _mouseX = Input.GetAxis("Mouse X");

            Vector3 newRotation = transform.localEulerAngles;
            newRotation.y += _mouseX * _sensitivity;
            transform.localEulerAngles = newRotation;


        //}

        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)
        //moveDirection.y -= gravity * Time.deltaTime;

         
        //transform.LookAt(posToLook * Time.deltaTime);
    }
}