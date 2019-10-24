using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private Material materialDebris;
    private Mesh mesh;
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Destruction(Vector3 impact, float radiusExplosion) {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Color> colors = new List<Color>();
        List<Vector2> uvs = new List<Vector2>();
        mesh.GetVertices(vertices);
        Debug.Log("Vertices: " + vertices.Count);
                
        
        mesh.GetTriangles(triangles, 0);

        Debug.DrawLine(Camera.main.transform.position, impact, Color.white, 1.0f);
        Debug.DrawLine(transform.transform.position, impact, Color.white, 1.0f);
        for (int verticesIndex = 0; verticesIndex < vertices.Count; verticesIndex++) {
            //Debug.Log("Distance("+vertices[verticesIndex]+" y "+(impact - transform.transform.position)+"): " + Vector3.Distance(vertices[verticesIndex], impact - transform.transform.position));
            if (Vector3.Distance(vertices[verticesIndex] * transform.localScale.x, impact - transform.transform.position) < radiusExplosion) {
                //Debug.Log("Quitando vertice: index " + verticesIndex + " value: " + vertices[verticesIndex]);
                vertices.RemoveAt(verticesIndex);
                //Debug.Log("Triangles left: " + (triangles.Count / 3));
                for (int trianglesIndex = 0; trianglesIndex < triangles.Count; trianglesIndex++) {
                    //Debug.Log("Index triangle: " + trianglesIndex);
                    if(triangles[trianglesIndex] == verticesIndex) {
                //Debug.Log("Quitando triangulo: ");

                        int startTriangleIndex = trianglesIndex - (trianglesIndex%3);
                        for (int triangleIndex = 2; triangleIndex >= 0; triangleIndex--) {
                            Debug.Log((startTriangleIndex + triangleIndex) + " - " + triangles.ElementAtOrDefault(startTriangleIndex + triangleIndex));

                            triangles.RemoveAt(startTriangleIndex + triangleIndex);

                        }
                        //Debug.Log("Triangles left: " + (triangles.Count / 3));
                                //triangles.ForEach(triangle => {
            //Debug.Log("Triangle: " + triangle);
       // });
                        trianglesIndex -= ((trianglesIndex%3) + 1); //we stay in the sema position because there has been deletions
                    }
                    else if (triangles[trianglesIndex] > verticesIndex) {
                       triangles[trianglesIndex]--;
                    }
                }
                verticesIndex--; //we stay in the sema position because there has been deletions
            }
        }

        //Debug.Log("Vertices filtered: " + vertices.Count);


        //Debug.Log("Triangles: " + triangles.Count);
        //Debug.Log("Triangles info: ");
        //triangles.ForEach(triangle => {
            //Debug.Log("Triangle: " + triangle);
        //});

        mesh.triangles = triangles.ToArray();

        mesh.vertices = vertices.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        

        return;
    }

}
