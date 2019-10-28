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

    public Vector3 GetEdgeVector() {
        return end.position - origin.position;
    }

    static public bool IsSameVertices(Edge e1, Edge e2) {
        return (e1.origin.position == e2.origin.position || e1.origin.position == e2.end.position) && (e1.end.position == e2.origin.position || e1.end.position == e2.end.position);
    }
}
/*
-2.67 0.73 -1.76
-1.2 0.95 -1.5
-0.85 0.84 -1.65
-0.4 0.76 -1.73
-1.20 4.04 -1.5
-0.85 4.15 -1.66
-0.4 4.23 -1.74
-1.54 1.299 -1.54
-1.65 1.64 -1.65
-1.55 3.7 -1.54
-2.06 2.49 -1.405
1.405 2.49 -2.06
-9.05 3.9 -2.06
-2.02 2.04 -1.37
-2.03 2.94 -1.38
0.87 1.18 -1.93
0.919 1.58 -2.12
0.45 3.87 -2.034
0.92 3.41 -2.12
0.96 2.96 -2.25
-0.5 2.43 -2.43


 */
public class Triangle {
    public Edge first;
    public Edge second;
    public Edge third;

    public void DrawTriangle() {
        Debug.DrawLine(first.origin.position, first.end.position, Color.white, 3f);
        Debug.DrawLine(second.origin.position, second.end.position, Color.white, 3f);
        Debug.DrawLine(third.origin.position, third.end.position, Color.white, 3f);
    }
}

public class VerticeOnSphere {
    public Vertice vertice;
    public Vector3 localPosition;

    public VerticeOnSphere(Vertice _vertice, Vector3 _centerSphere) {
        vertice = _vertice;
        localPosition = _vertice.position - _centerSphere;
    }
}

public class Plane {
    public Vector3 firstVector;
    public Vector3 secondVector;

    public Vector3 normal;

    public Plane(Vector3 _firstVector, Vector3 _secondVector) {
        firstVector = _firstVector;
        secondVector = _secondVector;
        normal = Vector3.Cross(_firstVector, _secondVector);
    }
}
public class CustomMesh : MonoBehaviour
{
    [SerializeField] private int sizeCircle = 1;
    [SerializeField] private float step = 0.2f;
    [SerializeField] public List<Vector3> points;
    [SerializeField] private GameObject _verticePrefab;
    // Start is called before the first frame update
    private Triangle superTriangle = new Triangle();
    private List<Vertice> vertices = new List<Vertice>();
    List<Triangle> triangulationv2 = new List<Triangle>();

    private int indexVertice = 0;
    public bool isActive = false;
    void Start() {
        //Startv3();
        /* 
        points = new List<Vector3>();

        for (int i = 0; i < 10; i++) {
            points.Add(new Vector3(Mathf.Cos(i * Mathf.PI * 2 / 10) + Random.Range(0, 0.1f), Mathf.Sin(i * Mathf.PI * 2 / 10) + Random.Range(0, 0.1f), 0 ));
        }
        for (int i = 0; i < 5; i++) {
            points.Add(new Vector3(Mathf.Cos(i * Mathf.PI * 2 / 5) * 0.5f + Random.Range(0, 0.1f), Mathf.Sin(i * Mathf.PI * 2 / 5) * 0.5f + Random.Range(0, 0.1f), 0.2f ));
        }
        for (int i = 0; i < 1; i++) {
            points.Add(new Vector3(Random.Range(0, 0.1f), 0, 0.4f ));
        }*/
        Startv3();
    }

    void Update() {
        if (isActive) {
            Updatev2();
        }
    }

   

    public void Startv3() {
        List<VerticeOnSphere> points = new List<VerticeOnSphere>();
        Vector3 center = new Vector3(0.5f,0.5f,0.5f);
        Vertice v1 = new Vertice(new Vector3(0,0,0), 0);
        Vertice v2 = new Vertice(new Vector3(0,0,0.5f), 1);
        Vertice v3 = new Vertice(new Vector3(0,0,1), 2);
        Vertice v4 = new Vertice(new Vector3(0.1f,0.5f,0), 3);
        Vertice v5 = new Vertice(new Vector3(0.1f,1,0), 4);
        Vertice v6 = new Vertice(new Vector3(0.1f,0.5f,0.5f), 5);
        Vertice v7 = new Vertice(new Vector3(0.2f,1,0.5f), 6);
        Vertice v8 = new Vertice(new Vector3(0.2f,1,1), 7);

        points.Add(new VerticeOnSphere(v1, center));
        points.Add(new VerticeOnSphere(v2, center));
        points.Add(new VerticeOnSphere(v3, center));
        points.Add(new VerticeOnSphere(v4, center));
        points.Add(new VerticeOnSphere(v5, center));
        points.Add(new VerticeOnSphere(v6, center));
        points.Add(new VerticeOnSphere(v7, center));
        points.Add(new VerticeOnSphere(v8, center));

        //List<VerticeOnSphere> pointsToTriangulate = new List<VerticeOnSphere>();
        List<Vertice> verticesToTriangulate = new List<Vertice>();
        List<Triangle> trianglesBowyerWatson = new List<Triangle>();
        List<Triangle> triangles = new List<Triangle>();
        Plane plane;

        //Half sphere where X < 0
        points.FindAll(point => point.localPosition.x < 0).ForEach(point => {
            verticesToTriangulate.Add(point.vertice);
        });
        Debug.Log("Vertices to triangulate: " + verticesToTriangulate.Count);
        plane = new Plane(Vector3.forward, Vector3.up);
        trianglesBowyerWatson = trianglesBowyerWatson.Concat(BowyerWatsonv3(verticesToTriangulate, plane)).ToList();
        verticesToTriangulate = new List<Vertice>();

        //Half sphere where X >= 0
        points.FindAll(point => point.localPosition.x >= 0).ForEach(point => {
            verticesToTriangulate.Add(point.vertice);
        });
        Debug.Log("Vertices to triangulate: " + verticesToTriangulate.Count);
        plane = new Plane(Vector3.back, Vector3.up);
        trianglesBowyerWatson = trianglesBowyerWatson.Concat(BowyerWatsonv3(verticesToTriangulate, plane)).ToList();
        verticesToTriangulate = new List<Vertice>();

        //Half sphere where Y < 0
        points.FindAll(point => point.localPosition.y < 0).ForEach(point => {
            verticesToTriangulate.Add(point.vertice);
        });
        Debug.Log("Vertices to triangulate: " + verticesToTriangulate.Count);
        plane = new Plane(Vector3.right, Vector3.forward);
        trianglesBowyerWatson = trianglesBowyerWatson.Concat(BowyerWatsonv3(verticesToTriangulate, plane)).ToList();
        verticesToTriangulate = new List<Vertice>();

        //Half sphere where Y >= 0
        points.FindAll(point => point.localPosition.y >= 0).ForEach(point => {
            verticesToTriangulate.Add(point.vertice);
        });
        Debug.Log("Vertices to triangulate: " + verticesToTriangulate.Count);
        plane = new Plane(Vector3.right, Vector3.back);
        trianglesBowyerWatson = trianglesBowyerWatson.Concat(BowyerWatsonv3(verticesToTriangulate, plane)).ToList();
        verticesToTriangulate = new List<Vertice>();
        
        //Half sphere where Z < 0
        points.FindAll(point => point.localPosition.z < 0).ForEach(point => {
            verticesToTriangulate.Add(point.vertice);
        });
        Debug.Log("Vertices to triangulate: " + verticesToTriangulate.Count);
        plane = new Plane(Vector3.left, Vector3.up);
        trianglesBowyerWatson = trianglesBowyerWatson.Concat(BowyerWatsonv3(verticesToTriangulate, plane)).ToList();
        verticesToTriangulate = new List<Vertice>();

        //Half sphere where Y >= 0
        points.FindAll(point => point.localPosition.z >= 0).ForEach(point => {
            verticesToTriangulate.Add(point.vertice);
        });
        Debug.Log("Vertices to triangulate: " + verticesToTriangulate.Count);
        plane = new Plane(Vector3.right, Vector3.up);
        trianglesBowyerWatson = trianglesBowyerWatson.Concat(BowyerWatsonv3(verticesToTriangulate, plane)).ToList();
        verticesToTriangulate = new List<Vertice>();

        points.ForEach(point => {
            verticesToTriangulate.Add(point.vertice);
        });

        Mesh customMesh = new Mesh();

        List<Vector3> meshVertices = new List<Vector3>();
        List<int> meshTriangles = new List<int>();

        meshVertices = customMesh.vertices.ToList();
        meshTriangles = customMesh.triangles.ToList();
        AddGeneratedVerticesAndTrianglesToMesh(customMesh, meshVertices, meshTriangles, verticesToTriangulate, trianglesBowyerWatson);
        
        
        customMesh.vertices = meshVertices.ToArray();
        customMesh.triangles = meshTriangles.ToArray();

        customMesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = customMesh;

  
    }

    private void AddGeneratedVerticesAndTrianglesToMesh(Mesh mesh, List<Vector3> meshVertices, List<int> meshTriangles, List<Vertice> verticesGenerated, List<Triangle> trianglesGenerated) {
   

        
        verticesGenerated.ForEach(verticeGenerated => {
            meshVertices.Add(verticeGenerated.position);
        });



        trianglesGenerated.ForEach(triangleGenerated => {
            meshTriangles.Add(triangleGenerated.first.origin.index);
            meshTriangles.Add(triangleGenerated.second.origin.index);
            meshTriangles.Add(triangleGenerated.third.origin.index);
        });




    }

    private Vertice PointProjectedInPlane(Vertice point, Vector3 normalPlane) {
        Vector3 origToPoint = point.position - new Vector3(0,0,0);
        float d = Vector3.Dot(origToPoint, normalPlane);
        return new Vertice(point.position - d * normalPlane, point.index);
    }
    private List<Triangle> BowyerWatsonv3(List<Vertice> vertices, Plane plane) {

        List<Triangle> triangulation = new List<Triangle>();
        Triangle superTriangle = GetSuperTrianglev3(plane);
        triangulation.Add(superTriangle);
        //points.ForEach(point => {
        for (int verticesIndex = 0; verticesIndex < vertices.Count; verticesIndex++) {
            Vertice verticeProjected= PointProjectedInPlane(vertices[verticesIndex], plane.normal);
            //Vertice verticePoint = new Vertice(verticePosition, indexVertice++);
            //Instantiate(_verticePrefab, verticePosition * transform.localScale.x + transform.position, Quaternion.identity);

            List<Triangle> badTriangles = new List<Triangle>();
            triangulation.ForEach(triangle => {
                if (isPointInsideCircumcircleTrianglev3(verticeProjected.position, triangle)) {
                    badTriangles.Add(triangle);
                }
            });


            for (int badTriangleIndex = 0; badTriangleIndex < badTriangles.Count; badTriangleIndex++) {
                triangulation.Remove(badTriangles[badTriangleIndex]);
    
            }
            

            List<Edge> polygon = new List<Edge>();
            List<Edge> possibleNotSharedEdges = new List<Edge>();
            badTriangles.ForEach(triangle => {
                possibleNotSharedEdges.Add(triangle.first);
                possibleNotSharedEdges.Add(triangle.second);
                possibleNotSharedEdges.Add(triangle.third);
            });

            possibleNotSharedEdges.ForEach(possibleEdge => {
                if (possibleNotSharedEdges.FindAll(edge => Edge.IsSameVertices(possibleEdge, edge)).Count() == 1) {
                    Debug.Log("Added to Polygon: o(" + possibleEdge.origin.position + ") e(" + possibleEdge.end.position);
                    polygon.Add(possibleEdge);
                }
                else {
                    Debug.Log("NOT Added to Polygon: o(" + possibleEdge.origin.position + ") e(" + possibleEdge.end.position);
                }
            });


            for (int polygonIndex = 0; polygonIndex < polygon.Count; polygonIndex++) {
                Triangle newTri = new Triangle();
                newTri.first = polygon[polygonIndex];
                newTri.second = new Edge(verticeProjected, polygon[polygonIndex].origin);
                newTri.third = new Edge(polygon[polygonIndex].end, verticeProjected);

                Vector3 normal = Vector3.Cross(newTri.first.GetEdgeVector(), newTri.second.GetEdgeVector());

                if (Vector3.Dot(normal, plane.normal) > 0) {
                    Edge aux = newTri.first;
                    newTri.first = newTri.third;
                    newTri.third = aux;
                }
                triangulation.Add(newTri);
    
            }
            

        }


        List<Triangle> finaltriangulation = new List<Triangle>();
        triangulation.ForEach(triangle => {
            if (!isSharedVerticeTrianglesv1(triangle, superTriangle)) {
                finaltriangulation.Add(triangle);
            }
        });


        triangulation = finaltriangulation;

        Debug.Log("Triangulation: number triangles - " + triangulation.Count);

        return triangulation;
    }

    private Triangle GetSuperTrianglev3(Plane plane) {
        Triangle superTriangle = new Triangle();
        Vertice firstVertice = new Vertice(plane.firstVector * 666, -1);
        Vertice secondtVertice = new Vertice(plane.secondVector * 500 - plane.firstVector * 200, -1);
        Vertice thirdVertice = new Vertice(-plane.secondVector * 500 - plane.firstVector * 200, -1);

        superTriangle.first = new Edge(secondtVertice, firstVertice);
        superTriangle.second = new Edge(firstVertice, thirdVertice);
        superTriangle.third = new Edge(thirdVertice, secondtVertice);
        return superTriangle;
    }

    private bool isPointInsideCircumcircleTrianglev3(Vector3 point, Triangle triangle) {
        float angleFirst = Vector3.Angle(triangle.second.origin.position - triangle.first.origin.position, triangle.third.origin.position - triangle.first.origin.position);
        float angleSecond = Vector3.Angle(triangle.first.origin.position - triangle.second.origin.position, triangle.third.origin.position - triangle.second.origin.position);
        float angleThird = Vector3.Angle(triangle.first.origin.position - triangle.third.origin.position, triangle.second.origin.position - triangle.third.origin.position);
        angleFirst *= Mathf.Deg2Rad;
        angleSecond *= Mathf.Deg2Rad;
        angleThird *= Mathf.Deg2Rad;

        float circumcenterX = triangle.first.origin.position.x * Mathf.Sin(2 * angleFirst) + triangle.second.origin.position.x * Mathf.Sin(2 * angleSecond) + triangle.third.origin.position.x * Mathf.Sin(2 * angleThird);
        float circumcenterY = triangle.first.origin.position.y * Mathf.Sin(2 * angleFirst) + triangle.second.origin.position.y * Mathf.Sin(2 * angleSecond) + triangle.third.origin.position.y * Mathf.Sin(2 * angleThird);
        float circumcenterZ = triangle.first.origin.position.z * Mathf.Sin(2 * angleFirst) + triangle.second.origin.position.z * Mathf.Sin(2 * angleSecond) + triangle.third.origin.position.z * Mathf.Sin(2 * angleThird);

        Vector3 circumcenter = new Vector3(circumcenterX, circumcenterY, circumcenterZ) / (Mathf.Sin(2 * angleFirst) + Mathf.Sin(2 * angleSecond) + Mathf.Sin(2 * angleThird));

        float radiusFirst = triangle.first.MagnitudeEdge();
        float radiusSecond = triangle.second.MagnitudeEdge();
        float radiusThird = triangle.third.MagnitudeEdge();
        
        float circumradius = radiusFirst * radiusSecond * radiusThird / Mathf.Sqrt((radiusFirst + radiusSecond + radiusThird) * (radiusSecond + radiusThird - radiusFirst) * (radiusThird + radiusFirst - radiusSecond) * (radiusFirst + radiusSecond - radiusThird));
        
        float distancePointToCircumcenter = Vector3.Distance(point, circumcenter);
        Debug.Log("Circumcenter: " + circumcenter + " Radius: " + circumradius + " Distance: " + distancePointToCircumcenter);
        bool isPointInside = distancePointToCircumcenter < circumradius;

        if (isPointInside) {
            DrawCircle(circumcenter, circumradius, 100);
        }
        return isPointInside;
        
        //circumcenterX /= (Mathf.Sin(2 * angleFirst) + Mathf.Sin(2 * angleSecond) + Mathf.Sin(2 * angleThird));
        //circumcenterX /= (Mathf.Sin(2 * angleFirst) + Mathf.Sin(2 * angleSecond) + Mathf.Sin(2 * angleThird));
    }
    public void Startv2() {
        isActive = true;
        StartCoroutine(BowyerWatsonv2());

    }

    void Updatev2() {
                
        Mesh customMesh = new Mesh();
        
        List<Vector3> meshVertices = new List<Vector3>();
        List<int> meshTriangles = new List<int>();
        ConvertTrianglesToMeshVerticesAndTrianglesv1(triangulationv2, meshVertices, meshTriangles);

        customMesh.vertices = meshVertices.ToArray();;
        customMesh.triangles = meshTriangles.ToArray();

        customMesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = customMesh;
    }
    void Startv1()
    {


        points.ForEach(point => {
            vertices.Add(new Vertice(new Vector3(point.x, point.y, 0), indexVertice++));
        });

        
        Mesh customMesh = new Mesh();


        
        List<Vector3> meshVertices = new List<Vector3>();
        List<int> meshTriangles = new List<int>();
        ConvertTrianglesToMeshVerticesAndTrianglesv1(BowyerWatsonv1(), meshVertices, meshTriangles);

        customMesh.vertices = meshVertices.ToArray();;
        customMesh.triangles = meshTriangles.ToArray();

        customMesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = customMesh;
    }

    // Update is called once per frame

    private void ConvertTrianglesToMeshVerticesAndTrianglesv1(List<Triangle> trianglesGenerated, List<Vector3> meshVertices, List<int> meshTriangles) {
        int indexVertice = 0;
        List<Vertice> vertices = new List<Vertice>();

        trianglesGenerated.ForEach(triangleGenerated => {
            if (!vertices.Exists(vertice => vertice == triangleGenerated.first.origin)) {
                triangleGenerated.first.origin.index = indexVertice++;
                vertices.Add(triangleGenerated.first.origin);
                meshVertices.Add(new Vector3(triangleGenerated.first.origin.position.x, triangleGenerated.first.origin.position.y, triangleGenerated.first.origin.position.z));
                meshTriangles.Add(triangleGenerated.first.origin.index);
            }
            else {
                meshTriangles.Add(triangleGenerated.first.origin.index);                
            }
            if (!vertices.Exists(vertice => vertice == triangleGenerated.second.origin)) {
                triangleGenerated.second.origin.index = indexVertice++;
                vertices.Add(triangleGenerated.second.origin);
                meshVertices.Add(new Vector3(triangleGenerated.second.origin.position.x, triangleGenerated.second.origin.position.y, triangleGenerated.second.origin.position.z));
                meshTriangles.Add(triangleGenerated.second.origin.index);

            }
            else {
                meshTriangles.Add(triangleGenerated.second.origin.index);
            }
            if (!vertices.Exists(vertice => vertice == triangleGenerated.third.origin)) {
                triangleGenerated.third.origin.index = indexVertice++;
                vertices.Add(triangleGenerated.third.origin);
                meshVertices.Add(new Vector3(triangleGenerated.third.origin.position.x, triangleGenerated.third.origin.position.y, triangleGenerated.third.origin.position.z));
                meshTriangles.Add(triangleGenerated.third.origin.index);

            }
            else {
                meshTriangles.Add(triangleGenerated.third.origin.index);
            }
        });
    }

    public float _speedConstructionMesh = 1.0f;
    private IEnumerator BowyerWatsonv2() {

        //List<Triangle> triangulationv2 = new List<Triangle>();
        Triangle superTriangle = GetSuperTrianglev1(points);
        triangulationv2.Add(superTriangle);
        yield return new WaitForSeconds(_speedConstructionMesh);
        //points.ForEach(point => {
        for (int pointIndex = 0; pointIndex < points.Count; pointIndex++) {
            Vector3 verticePosition = new Vector3(points[pointIndex].x, points[pointIndex].y, points[pointIndex].z);
            Vertice verticePoint = new Vertice(verticePosition, indexVertice++);
            Instantiate(_verticePrefab, verticePosition * transform.localScale.x + transform.position, Quaternion.identity);
            yield return new WaitForSeconds(_speedConstructionMesh);
            List<Triangle> badTriangles = new List<Triangle>();
            triangulationv2.ForEach(triangle => {
                if (isPointInsideCircumcircleTrianglev1(points[pointIndex], triangle)) {
                    badTriangles.Add(triangle);
                }
            });

            yield return new WaitForSeconds(_speedConstructionMesh);
            for (int badTriangleIndex = 0; badTriangleIndex < badTriangles.Count; badTriangleIndex++) {
                triangulationv2.Remove(badTriangles[badTriangleIndex]);
                yield return new WaitForSeconds(_speedConstructionMesh);
            }
            

            List<Edge> polygon = new List<Edge>();
            List<Edge> possibleNotSharedEdges = new List<Edge>();
            badTriangles.ForEach(triangle => {
                possibleNotSharedEdges.Add(triangle.first);
                possibleNotSharedEdges.Add(triangle.second);
                possibleNotSharedEdges.Add(triangle.third);
            });

            possibleNotSharedEdges.ForEach(possibleEdge => {
                if (possibleNotSharedEdges.FindAll(edge => Edge.IsSameVertices(possibleEdge, edge)).Count() == 1) {
                    Debug.Log("Added to Polygon: o(" + possibleEdge.origin.position + ") e(" + possibleEdge.end.position);
                    polygon.Add(possibleEdge);
                }
                else {
                    Debug.Log("NOT Added to Polygon: o(" + possibleEdge.origin.position + ") e(" + possibleEdge.end.position);
                }
            });

            yield return new WaitForSeconds(_speedConstructionMesh);
            for (int polygonIndex = 0; polygonIndex < polygon.Count; polygonIndex++) {
                Triangle newTri = new Triangle();
                newTri.first = polygon[polygonIndex];
                newTri.second = new Edge(verticePoint, polygon[polygonIndex].origin);
                newTri.third = new Edge(polygon[polygonIndex].end, verticePoint);

                Vector3 normal = Vector3.Cross(newTri.first.GetEdgeVector(), newTri.second.GetEdgeVector());

                if (Vector3.Dot(normal, -Vector3.forward) < 0) {
                    Edge aux = newTri.first;
                    newTri.first = newTri.third;
                    newTri.third = aux;
                }
                triangulationv2.Add(newTri);
                yield return new WaitForSeconds(_speedConstructionMesh);
            }
            

        }

        yield return new WaitForSeconds(_speedConstructionMesh);

        List<Triangle> finalTriangulationv2 = new List<Triangle>();
        triangulationv2.ForEach(triangle => {
            if (!isSharedVerticeTrianglesv1(triangle, superTriangle)) {
                finalTriangulationv2.Add(triangle);
            }
        });


        triangulationv2 = finalTriangulationv2;
        yield return new WaitForSeconds(_speedConstructionMesh);
    }

    //Algorithm https://es.wikipedia.org/wiki/Algoritmo_de_Bowyer-Watson
    private List<Triangle> BowyerWatsonv1 () {

        List<Triangle> triangulation = new List<Triangle>();
        Triangle superTriangle = GetSuperTrianglev1(points);
        triangulation.Add(superTriangle);

        points.ForEach(point => {
            Vertice verticePoint = new Vertice(new Vector3(point.x, point.y, 0), indexVertice++);
            List<Triangle> badTriangles = new List<Triangle>();
            triangulation.ForEach(triangle => {
                if (isPointInsideCircumcircleTrianglev1(point, triangle)) {
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
            if (!isSharedVerticeTrianglesv1(triangle, superTriangle)) {
                finalTriangulation.Add(triangle);
            }
        });

        return finalTriangulation;
    }

    private bool isSharedVerticeTrianglesv1(Triangle triangle1, Triangle triangle2) {
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

    private Triangle GetSuperTrianglev1(List<Vector3> points) {
        superTriangle = new Triangle();
        Vertice firstVertice = new Vertice(new Vector3(0, 30.4f, 0), indexVertice++);
        Vertice secondtVertice = new Vertice(new Vector3(-30.7f, -30.4f, 0), indexVertice++);
        Vertice thirdVertice = new Vertice(new Vector3(30.7f, -30.4f, 0), indexVertice++);

        superTriangle.first = new Edge(secondtVertice, firstVertice);
        superTriangle.second = new Edge(firstVertice, thirdVertice);
        superTriangle.third = new Edge(thirdVertice, secondtVertice);
        return superTriangle;
    }

    //Circumcenter https://byjus.com/maths/circumcenter-of-a-triangle/
    //Radius https://www.mathopenref.com/trianglecircumcircle.html
    private bool isPointInsideCircumcircleTrianglev1(Vector2 point, Triangle triangle) {
        float angleFirst = Vector3.Angle(triangle.second.origin.position - triangle.first.origin.position, triangle.third.origin.position - triangle.first.origin.position);
        float angleSecond = Vector3.Angle(triangle.first.origin.position - triangle.second.origin.position, triangle.third.origin.position - triangle.second.origin.position);
        float angleThird = Vector3.Angle(triangle.first.origin.position - triangle.third.origin.position, triangle.second.origin.position - triangle.third.origin.position);
        angleFirst *= Mathf.Deg2Rad;
        angleSecond *= Mathf.Deg2Rad;
        angleThird *= Mathf.Deg2Rad;

        float circumcenterX = triangle.first.origin.position.x * Mathf.Sin(2 * angleFirst) + triangle.second.origin.position.x * Mathf.Sin(2 * angleSecond) + triangle.third.origin.position.x * Mathf.Sin(2 * angleThird);
        float circumcenterY = triangle.first.origin.position.y * Mathf.Sin(2 * angleFirst) + triangle.second.origin.position.y * Mathf.Sin(2 * angleSecond) + triangle.third.origin.position.y * Mathf.Sin(2 * angleThird);

        Vector2 circumcenter = new Vector2(circumcenterX, circumcenterY) / (Mathf.Sin(2 * angleFirst) + Mathf.Sin(2 * angleSecond) + Mathf.Sin(2 * angleThird));

        float radiusFirst = triangle.first.MagnitudeEdge();
        float radiusSecond = triangle.second.MagnitudeEdge();
        float radiusThird = triangle.third.MagnitudeEdge();
        
        float circumradius = radiusFirst * radiusSecond * radiusThird / Mathf.Sqrt((radiusFirst + radiusSecond + radiusThird) * (radiusSecond + radiusThird - radiusFirst) * (radiusThird + radiusFirst - radiusSecond) * (radiusFirst + radiusSecond - radiusThird));
        
        Debug.Log("Circumcenter: " + circumcenter + " Radius: " + circumradius);
        bool isPointInside = Vector2.Distance(new Vector2(point.x, point.y), circumcenter) < circumradius;

        if (isPointInside) {
            DrawCircle(circumcenter, circumradius, 100);
        }
        return isPointInside;
        
        //circumcenterX /= (Mathf.Sin(2 * angleFirst) + Mathf.Sin(2 * angleSecond) + Mathf.Sin(2 * angleThird));
        //circumcenterX /= (Mathf.Sin(2 * angleFirst) + Mathf.Sin(2 * angleSecond) + Mathf.Sin(2 * angleThird));
    }

    private void DrawCircle(Vector2 point, float radius, int numPoints) {
        Debug.Log("Drawing Circle");
        List<Vector3> points = new List<Vector3>();
        Vector2 pointScaled = point * transform.localScale.x + new Vector2(transform.position.x, transform.position.y);
        float radiusScaled = radius * transform.localScale.x;
        for (int i = 0; i < numPoints; i++) {
            Vector3 newPoint = new Vector3(pointScaled.x + Mathf.Cos(i * Mathf.PI * 2 / numPoints) * radiusScaled, pointScaled.y + Mathf.Sin(i * Mathf.PI * 2 / numPoints) * radiusScaled, 0);
            points.Add(newPoint);
        }

        for (int i = 0; i < points.Count(); i++) {

            Debug.DrawLine(points[i] , points[(i + 1)%points.Count()], Color.yellow, _speedConstructionMesh * 4);
        }
    }


}
