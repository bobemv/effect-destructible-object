using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// This complete script can be attached to a camera to make it
// continuously point at another object.

public class LookCamera : MonoBehaviour
{
    public Player player;
    public float speed;
    public float radiusExplosion;
    public float _sensitivity;

    void Update()
    {
        //Vector3 point = GetComponent<Camera>().ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 400f));
        // Rotate the camera every frame so it keeps looking at the target
        //transform.LookAt(point * Time.deltaTime);

            Cursor.lockState = CursorLockMode.Locked;
            float _mouseY = -Input.GetAxis("Mouse Y");

            Vector3 newRotation = transform.localEulerAngles;
            newRotation.x += _mouseY * _sensitivity;
            transform.localEulerAngles = newRotation;

        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            Ray ray = GetComponent<Camera>().ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 100f));
            RaycastHit hit;
            //Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow, 1f);
            Physics.Raycast(ray, out hit, 100f);
            if (hit.collider != null && hit.collider.tag == "Destructible") {
                Debug.Log("Llegue!");
                hit.collider.gameObject.GetComponent<Destructible>().Destruction(hit.point, radiusExplosion);
                //Rigidbody body = hit.collider.gameObject.GetComponent<Rigidbody>();
                //body.AddExplosionForce(10f, hit.point, 5f);
            }

        }

        // Same as above, but setting the worldUp parameter to Vector3.left in this example turns the camera on its side
        //transform.LookAt(target, Vector3.left);
    }
}