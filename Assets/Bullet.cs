using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float _speed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(-transform.forward * _speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Obstacle") {
            other.gameObject.GetComponent<MarchingCubes>().Destruction(transform.position, 4);
            //Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}
