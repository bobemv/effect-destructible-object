using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomMesh : MonoBehaviour
{
    [SerializeField] private int sizeCircle = 1;
    [SerializeField] private float step = 0.2f;
    // Start is called before the first frame update
    void Start()
    {
        Mesh custom = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        vertices.Add(new Vector3(0, 0, 0));
        int numVerticesLayer = 5;

        for (int i = 2; i <= sizeCircle ; i++) {
            
        }
        vertices.Add();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
