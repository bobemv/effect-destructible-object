using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Vertice {
    public Vertice(Vector3 _position, int _index) {
        position = _position;
        index = _index;
    }
    public Vector3 position;
    public int index;
}

public class Edge {
    public Edge (Vertice _origin, Vertice _end) {
        origin = _origin;
        end = _end;
    }
    public Vertice origin;
    public Vertice end;

    public float MagnitudeEdge() {
        return Vector3.Distance(origin.position, end.position);
    }
}

public class Triangle {
    public Edge first;
    public Edge second;
    public Edge third;
}

public class CustomMesh : MonoBehaviour
{
    [SerializeField] private int sizeCircle = 1;
    [SerializeField] private float step = 0.2f;
    [SerializeField] private List<Vector2> points;
    // Start is called before the first frame update
    private Triangle superTriangle = new Triangle();
    private List<Vertice> vertices = new List<Vertice>();

    private int indexVertice = 0;
    void Start()
    {


        points.ForEach(point => {
            vertices.Add(new Vertice(new Vector3(point.x, point.y, 0), indexVertice++));
        });


        Mesh customMesh = new Mesh();


        List<Vector3> meshVertices = new List<Vector3>();
        List<int> meshTriangles = new List<int>();
        ConvertTrianglesToMeshVerticesAndTriangles(BowyerWatson(), meshVertices, meshTriangles);

        customMesh.vertices = meshVertices.ToArray();;
        customMesh.triangles = meshTriangles.ToArray();

        customMesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = customMesh;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ConvertTrianglesToMeshVerticesAndTriangles(List<Triangle> trianglesGenerated, List<Vector3> meshVertices, List<int> meshTriangles) {
        int indexVertice = 0;
        List<Vertice> vertices = new List<Vertice>();

        trianglesGenerated.ForEach(triangleGenerated => {
            if (!vertices.Exists(vertice => vertice == triangleGenerated.first.origin)) {
                triangleGenerated.first.origin.index = indexVertice++;
                vertices.Add(triangleGenerated.first.origin);
                meshVertices.Add(new Vector3(triangleGenerated.first.origin.position.x, triangleGenerated.first.origin.position.y, 0));
                meshTriangles.Add(triangleGenerated.first.origin.index);
            }
            else {
                meshTriangles.Add(triangleGenerated.first.origin.index);                
            }
            if (!vertices.Exists(vertice => vertice == triangleGenerated.second.origin)) {
                triangleGenerated.second.origin.index = indexVertice++;
                vertices.Add(triangleGenerated.second.origin);
                meshVertices.Add(new Vector3(triangleGenerated.second.origin.position.x, triangleGenerated.second.origin.position.y, 0));
                meshTriangles.Add(triangleGenerated.second.origin.index);

            }
            else {
                meshTriangles.Add(triangleGenerated.second.origin.index);
            }
            if (!vertices.Exists(vertice => vertice == triangleGenerated.third.origin)) {
                triangleGenerated.third.origin.index = indexVertice++;
                vertices.Add(triangleGenerated.third.origin);
                meshVertices.Add(new Vector3(triangleGenerated.third.origin.position.x, triangleGenerated.third.origin.position.y, 0));
                meshTriangles.Add(triangleGenerated.third.origin.index);

            }
            else {
                meshTriangles.Add(triangleGenerated.third.origin.index);
            }
        });
    }

    //Algorithm https://es.wikipedia.org/wiki/Algoritmo_de_Bowyer-Watson
    private List<Triangle> BowyerWatson () {

        List<Triangle> triangulation = new List<Triangle>();
        Triangle superTriangle = GetSuperTriangle(points);
        triangulation.Add(superTriangle);

        points.ForEach(point => {
            Vertice verticePoint = new Vertice(new Vector3(point.x, point.y, 0), indexVertice++);
            List<Triangle> badTriangles = new List<Triangle>();
            triangulation.ForEach(triangle => {
                if (isPointInsideCircumcircleTriangle(point, triangle)) {
                    badTriangles.Add(triangle);
                }
            });

            badTriangles.ForEach(badTriangle => {
                triangulation.Remove(badTriangle);
            });

            List<Edge> polygon = new List<Edge>();
            List<Edge> possibleNotSharedEdges = new List<Edge>();
            badTriangles.ForEach(triangle => {
                possibleNotSharedEdges.Add(triangle.first);
                possibleNotSharedEdges.Add(triangle.second);
                possibleNotSharedEdges.Add(triangle.third);
            });

            possibleNotSharedEdges.ForEach(possibleEdge => {
                if (possibleNotSharedEdges.FindAll(edge => possibleEdge == edge).Count() == 1) {
                    polygon.Add(possibleEdge);
                }
            });

            polygon.ForEach(edge => {
                Triangle newTri = new Triangle();
                newTri.first = edge;
                newTri.second = new Edge(verticePoint, edge.origin);
                newTri.third = new Edge(edge.end, verticePoint);

                if (newTri.second.origin.position.x < newTri.first.origin.position.x) {
                    Edge aux = newTri.first;
                    newTri.first = newTri.second;
                    newTri.second = aux;
                }
                if (newTri.third.origin.position.x < newTri.second.origin.position.x) {
                    Edge aux = newTri.second;
                    newTri.second = newTri.third;
                    newTri.third = aux;
                }
                if (newTri.second.origin.position.x < newTri.first.origin.position.x) {
                    Edge aux = newTri.first;
                    newTri.first = newTri.second;
                    newTri.second = aux;
                }
                triangulation.Add(newTri);
            });

        });

        List<Triangle> finalTriangulation = new List<Triangle>();
        triangulation.ForEach(triangle => {
            if (!isSharedVerticeTriangles(triangle, superTriangle)) {
                finalTriangulation.Add(triangle);
            }
        });

        return finalTriangulation;
    }

    private bool isSharedVerticeTriangles(Triangle triangle1, Triangle triangle2) {
        return triangle1.first.origin == triangle2.first.origin ||
               triangle1.first.origin == triangle2.second.origin ||
               triangle1.first.origin == triangle2.third.origin ||
               triangle1.second.origin == triangle2.first.origin ||
               triangle1.second.origin == triangle2.second.origin ||
               triangle1.second.origin == triangle2.third.origin ||
               triangle1.third.origin == triangle2.first.origin ||
               triangle1.third.origin == triangle2.second.origin ||
               triangle1.third.origin == triangle2.third.origin;
    }

    private Triangle GetSuperTriangle(List<Vector2> points) {
        superTriangle = new Triangle();
        Vertice firstVertice = new Vertice(new Vector3(0, 0.4f, 0), indexVertice++);
        Vertice secondtVertice = new Vertice(new Vector3(-0.7f, -0.4f, 0), indexVertice++);
        Vertice thirdVertice = new Vertice(new Vector3(0.7f, -0.4f, 0), indexVertice++);

        superTriangle.first = new Edge(firstVertice, secondtVertice);
        superTriangle.second = new Edge(secondtVertice, thirdVertice);
        superTriangle.third = new Edge(thirdVertice, firstVertice);
        return superTriangle;
    }

    //Circumcenter https://byjus.com/maths/circumcenter-of-a-triangle/
    //Radius https://www.mathopenref.com/trianglecircumcircle.html
    private bool isPointInsideCircumcircleTriangle(Vector2 point, Triangle triangle) {
        float angleFirst = Vector3.Angle(triangle.second.origin.position - triangle.first.origin.position, triangle.third.origin.position - triangle.first.origin.position);
        float angleSecond = Vector3.Angle(triangle.first.origin.position - triangle.second.origin.position, triangle.third.origin.position - triangle.second.origin.position);
        float angleThird = Vector3.Angle(triangle.first.origin.position - triangle.third.origin.position, triangle.second.origin.position - triangle.third.origin.position);
        angleFirst *= Mathf.Deg2Rad;
        angleSecond *= Mathf.Deg2Rad;
        angleThird *= Mathf.Deg2Rad;

        float circumcenterX = triangle.first.origin.position.x * Mathf.Sin(2 * angleFirst) + triangle.second.origin.position.x * Mathf.Sin(2 * angleSecond) + triangle.third.origin.position.x * Mathf.Sin(2 * angleThird);
        float circumcenterY = triangle.first.origin.position.y * Mathf.Sin(2 * angleFirst) + triangle.second.origin.position.y * Mathf.Sin(2 * angleSecond) + triangle.third.origin.position.y * Mathf.Sin(2 * angleThird);

        Vector3 circumcenter = new Vector3(circumcenterX, circumcenterY, 0) / (Mathf.Sin(2 * angleFirst) + Mathf.Sin(2 * angleSecond) + Mathf.Sin(2 * angleThird));

        float radiusFirst = triangle.first.MagnitudeEdge();
        float radiusSecond = triangle.second.MagnitudeEdge();
        float radiusThird = triangle.third.MagnitudeEdge();
        
        float circumradius = radiusFirst * radiusSecond * radiusThird / Mathf.Sqrt((radiusFirst + radiusSecond + radiusThird) * (radiusSecond + radiusThird - radiusFirst) * (radiusThird + radiusFirst - radiusSecond) * (radiusFirst + radiusSecond - radiusThird));
        
        Debug.Log("Circumcenter: " + circumcenter + " Radius: " + circumradius);
        return Vector3.Distance(new Vector3(point.x, point.y, 0), circumcenter) < circumradius;
        
        //circumcenterX /= (Mathf.Sin(2 * angleFirst) + Mathf.Sin(2 * angleSecond) + Mathf.Sin(2 * angleThird));
        //circumcenterX /= (Mathf.Sin(2 * angleFirst) + Mathf.Sin(2 * angleSecond) + Mathf.Sin(2 * angleThird));
    }


}
