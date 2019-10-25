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
    [SerializeField] private GameObject _verticePrefab;
    // Start is called before the first frame update
    private Triangle superTriangle = new Triangle();
    private List<Vertice> vertices = new List<Vertice>();
    List<Triangle> triangulationv2 = new List<Triangle>();

    private int indexVertice = 0;

    void Start() {
        Startv2();
    }

    void Update() {
        Updatev2();
    }
    void Startv2() {

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

    public float _speedConstructionMesh = 1.0f;
    private IEnumerator BowyerWatsonv2() {

        //List<Triangle> triangulationv2 = new List<Triangle>();
        Triangle superTriangle = GetSuperTrianglev1(points);
        triangulationv2.Add(superTriangle);
        yield return new WaitForSeconds(_speedConstructionMesh);
        //points.ForEach(point => {
        for (int pointIndex = 0; pointIndex < points.Count; pointIndex++) {
            Vector3 verticePosition = new Vector3(points[pointIndex].x, points[pointIndex].y, 0);
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

    private Triangle GetSuperTrianglev1(List<Vector2> points) {
        superTriangle = new Triangle();
        Vertice firstVertice = new Vertice(new Vector3(0, 0.4f, 0), indexVertice++);
        Vertice secondtVertice = new Vertice(new Vector3(-0.7f, -0.4f, 0), indexVertice++);
        Vertice thirdVertice = new Vertice(new Vector3(0.7f, -0.4f, 0), indexVertice++);

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
