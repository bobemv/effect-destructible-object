using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{

    private float rightMax = 9f;
    private float leftMax = -9f;
    private float upMax = 12f;
    private float downMax = 4f;

    private float desiredRotationZ = -90;
    private float desiredRotationX = 0;
    private float rotationRightMax = -120f;
    private float rotationLeftMax = -60f;
    private float rotationUpMax = -30f;
    private float rotationDownMax = 30f;

    [SerializeField] private Vector3 _startingPosition = new Vector3(0, 8, -30);
    [SerializeField] private Vector3 _startingRotation = new Vector3(180, 0, 270);
    [SerializeField] private float _speed = 5.0f;
    [SerializeField] private float _rotationSpeed = 1.0f;
    [SerializeField] private GameObject _bulletPrefab;
    // Start is called before the first frame update
    void Start()
    {
        transform.position = _startingPosition;
        transform.rotation = Quaternion.Euler(x, y, z);
        //transform.eulerAngles = _startingRotation;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 move = Vector3.zero;
        Vector3 rotate = Vector3.zero;
        bool isMovingLeft = false;
        bool isMovingRight = false;
        bool isMovingUp= false;
        bool isMovingDown = false;
        bool isMovingHorizontal= false;
        bool isMovingVertical = false;

        if (Input.GetKey(KeyCode.A)) {
            move += Vector3.left;
            isMovingLeft = true;
            isMovingHorizontal = true;
            //desiredRotationZ = rotationLeftMax;
        }
        else if (Input.GetKey(KeyCode.D)) {
            move += Vector3.right;
            isMovingRight = true;
            isMovingHorizontal = true;
            //desiredRotationZ = rotationRightMax;
        }
        if (Input.GetKey(KeyCode.W)) {
            move += Vector3.down;
            isMovingDown = true;
            isMovingVertical = true;
        }
        else if (Input.GetKey(KeyCode.S)) {
            move += Vector3.up;
            isMovingUp = true;
            isMovingVertical = true;
        }

        Vector3 previousPosition = transform.position;
        transform.Translate(move * _speed * Time.deltaTime, Space.World);

        if (transform.position.x > rightMax) {
            transform.position = new Vector3(rightMax, transform.position.y, transform.position.z);
            isMovingRight = false;
        }
        if (transform.position.x < leftMax) {
            transform.position = new Vector3(leftMax, transform.position.y, transform.position.z);
            isMovingLeft = false;
        }
        if (transform.position.y > upMax) {
            transform.position = new Vector3(transform.position.x, upMax, transform.position.z);
            isMovingUp = false;
        }
        if (transform.position.y < downMax) {
            transform.position = new Vector3(transform.position.x, downMax, transform.position.z);
            isMovingDown = false;
        }

        if (isMovingLeft) {
            rotate += Vector3.back;
        }
        else if (isMovingRight) {
            rotate += Vector3.forward;
        }
        else {
            rotate += z > 270 ? Vector3.back : Vector3.forward;
        }
        if (isMovingDown) {
            rotate += Vector3.right;
        }
        else if (isMovingUp) {
            rotate += Vector3.left;
        }
        else {
            rotate += x > 180 ? Vector3.left : Vector3.right;
        }

        if (!(isMovingHorizontal && isMovingVertical)) {
            rotate += y > 0 ? Vector3.down : Vector3.up;
        }

        //Vector3 previousRotation = transform.eulerAngles;

        //transform.Rotate(rotate * _rotationSpeed * Time.deltaTime);
        x += rotate.x * _rotationSpeed * Time.deltaTime;
        y += rotate.y * _rotationSpeed * Time.deltaTime;
        z += rotate.z * _rotationSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Euler(x, y, z);
        //Debug.Log("transform.eulerAngles: " + transform.eulerAngles);
        if (isMovingLeft && z < 240) {
            //Debug.Log("Dntroo");
            z = 240;
            transform.rotation = Quaternion.Euler(new Vector3(x, y, 240));
        }
        if (isMovingRight && z > 300) {
            //Debug.Log("Dntroo");
            z = 300;
            transform.rotation = Quaternion.Euler(new Vector3(x, y, 300));
        }
        if (isMovingDown && x > 210) {
            //Debug.Log("Dntroo");
            x = 210;
            transform.rotation = Quaternion.Euler(new Vector3(210, y, z));
        }
        if (isMovingUp && x < 150) {
            //Debug.Log("Dntroo");
            x = 150;

            transform.rotation = Quaternion.Euler(new Vector3(150, y, z));
        }
        if (!(isMovingHorizontal && isMovingVertical) && Mathf.Abs(y) < 2) {
            y = 0;
            transform.rotation = Quaternion.Euler(new Vector3(x, 0, z));
        }
        if (!isMovingHorizontal && Mathf.Abs(z - 270) < 2) {
            z = 270;
            transform.rotation = Quaternion.Euler(new Vector3(x, y, 270));
        }
        if (!isMovingVertical && Mathf.Abs(x - 180) < 2) {
            x = 180;
            transform.rotation = Quaternion.Euler(new Vector3(180, y, z));
        }
        

    

        //transform.Translate(transform.right * _speed * Time.deltaTime, Space.World);
        //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.FromToRotation(Vector3.right, transform.up), _rotationSpeed * Time.deltaTime);
      
        Shoot();
    }
    float x = 180;
    float y = 0;
    float z = 270;

    void Shoot() {
        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            Instantiate(_bulletPrefab, transform.position + Vector3.forward, Quaternion.Euler(270, 0, 0));
        }
    }
    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Obstacle") {
            Destroy(gameObject);

        }
    }
}
