using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private float _speed;
    public bool isInUse = false;
    void Start()
    {
        isInUse = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isInUse) {
            return;
        }
        transform.Translate(Vector3.back * _speed * Time.deltaTime);
        if (transform.position.z < -50) {
            isInUse = false;
        }
    }
}
