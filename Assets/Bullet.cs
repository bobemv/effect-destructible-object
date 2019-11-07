using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private float _radiusExplosion;
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
            other.gameObject.GetComponent<MarchingCubes>().Destruction(transform.position, _radiusExplosion);
            //Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}
