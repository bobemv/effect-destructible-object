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
        return (e1.origin.index == e2.origin.index || e1.origin.index == e2.end.index) && (e1.end.index == e2.origin.index || e1.end.index == e2.end.index);
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

    public void DrawPlane(Vector3 position, float size, float time) {
        Vector3 firstScaled = firstVector * size;
        Vector3 secondScaled = secondVector * size;
        Debug.DrawLine(position + firstScaled, position + secondScaled, Color.yellow, time);
        Debug.DrawLine(position + firstScaled, position - secondScaled, Color.yellow, time);
        Debug.DrawLine(position - firstScaled, position - secondScaled, Color.yellow, time);
        Debug.DrawLine(position - firstScaled, position + secondScaled, Color.yellow, time);
        Debug.DrawLine(position - firstScaled, position + secondScaled, Color.yellow, time);
        Debug.DrawLine(position, position + normal, Color.yellow, time);
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
    List<Triangle> currentTriangulation = new List<Triangle>();
    List<Vertice> currentVertices = new List<Vertice>();
    List<Vertice> triangulationVerticev2 = new List<Vertice>();
    public Plane plane;

    private int indexVertice = 0;
    public bool isActive = false;
    public float _speedConstructionMesh = 1.0f;
    public bool isTriangulationFinished = false;

    void Update() {
        if (isActive) {
            Mesh customMesh = new Mesh();
        
            Vector3[] meshVertices;
            int[] meshTriangles;

            ConvertTrianglesToMeshVerticesAndTriangles(currentTriangulation, currentVertices, out meshVertices, out meshTriangles);

            customMesh.vertices = meshVertices;
            customMesh.triangles = meshTriangles;

            customMesh.RecalculateNormals();

            GetComponent<MeshFilter>().mesh = customMesh;
        }
    }


    public void Startv2() {
        isActive = true;
        currentTriangulation = new List<Triangle>();
        currentVertices = new List<Vertice>();

        // SUPER TRIANGLE
        Triangle superTriangle = GetSuperTriangle(plane);

        currentVertices.Add(superTriangle.first.origin);
        currentVertices.Add(superTriangle.second.origin);
        currentVertices.Add(superTriangle.third.origin);

        //CREATE VERTICES, PROJECT THEM INTO A PLANE AND ADD TO ARRAYS
        int indexVertices = 0;
        List<Vertice> verticesProjectedInPlane = new List<Vertice>();

        points.ForEach(point => {
            Vertice newVerticeProjected = new Vertice(PointProjectedInPlane(point, plane.normal), indexVertices);
            Vertice newVerticeMesh = new Vertice(point, indexVertices);
            Instantiate(_verticePrefab, point * transform.localScale.x + transform.position, Quaternion.identity);

            verticesProjectedInPlane.Add(newVerticeProjected);
            currentVertices.Add(newVerticeMesh);
            indexVertices++;
        });

        isTriangulationFinished = false;
        StartCoroutine(BowyerWatson(verticesProjectedInPlane, plane, superTriangle));

    }

    private Vector3 PointProjectedInPlane(Vector3 point, Vector3 normalPlane) {
        Vector3 origToPoint = point - new Vector3(0,0,0);
        float d = Vector3.Dot(origToPoint, normalPlane);
        return point - d * normalPlane;
    }

    private Triangle GetSuperTriangle(Plane plane) {
        Triangle superTriangle = new Triangle();
        Vertice firstVertice = new Vertice(plane.firstVector * 666, -1);
        Vertice secondtVertice = new Vertice(plane.secondVector * 500 - plane.firstVector * 200, -2);
        Vertice thirdVertice = new Vertice(-plane.secondVector * 500 - plane.firstVector * 200, -3);

        superTriangle.first = new Edge(secondtVertice, firstVertice);
        superTriangle.second = new Edge(firstVertice, thirdVertice);
        superTriangle.third = new Edge(thirdVertice, secondtVertice);
        return superTriangle;
    }

    private int GetIndexDependingOnSuperTrianglePresent(int index) {
        if (!isTriangulationFinished) {
            if (index == -1) {
                return 0;
            }
            if (index == -2) {
                return 1;
            }
            if (index == -3) {
                return 2;
            }
            return index + 3;
        }
        
        return index;
    }

    private void ConvertTrianglesToMeshVerticesAndTriangles(List<Triangle> trianglesGenerated, List<Vertice> verticesGenerated, out Vector3[] meshVertices, out int[] meshTriangles) {
        int sizeMeshVertices = verticesGenerated.Count;
        int sizeMeshTriangles = trianglesGenerated.Count * 3;

        meshVertices = new Vector3[sizeMeshVertices];
        meshTriangles = new int[sizeMeshTriangles];

        for (int i = 0; i < verticesGenerated.Count; i++) {
            //ADD SUPERTRIANGLE
            int indexParsed = GetIndexDependingOnSuperTrianglePresent(verticesGenerated[i].index);
            meshVertices[indexParsed] = verticesGenerated[i].position;
        }

        for (int i = 0; i < trianglesGenerated.Count; i++) {
            int indexParsed = GetIndexDependingOnSuperTrianglePresent(trianglesGenerated[i].first.origin.index);
            meshTriangles[i * 3] = indexParsed;

            indexParsed = GetIndexDependingOnSuperTrianglePresent(trianglesGenerated[i].second.origin.index);
            meshTriangles[(i * 3) + 1] = indexParsed;

            indexParsed = GetIndexDependingOnSuperTrianglePresent(trianglesGenerated[i].third.origin.index);
            meshTriangles[(i * 3) + 2] = indexParsed;
        }
    }

    private IEnumerator BowyerWatson(List<Vertice> points, Plane plane, Triangle superTriangle) {

        //List<Triangle> currentTriangulation = new List<Triangle>();
        currentTriangulation.Add(superTriangle);
        yield return new WaitForSeconds(_speedConstructionMesh);
        //points.ForEach(point => {

        // RANDOMIZE ORDER OF INSERTION (NECESSARY?)
        int[] orderPointsToBeInserted = new int[points.Count];
        for (int i = 0; i < points.Count; i++) {
            orderPointsToBeInserted[i] = i;
        }
        for (int i = points.Count - 1; i > 0; i--) {
            int j = Random.Range(0, i + 1);
            int temp = orderPointsToBeInserted[i];
            orderPointsToBeInserted[i] = orderPointsToBeInserted[j] ;
            orderPointsToBeInserted[j] = temp;
        }
        
        for (int i = 0; i < orderPointsToBeInserted.Length; i++) {
            int pointIndex = orderPointsToBeInserted[i];
            Instantiate(_verticePrefab, points[pointIndex].position * transform.localScale.x + transform.position, Quaternion.identity);
            yield return new WaitForSeconds(_speedConstructionMesh);
            List<Triangle> badTriangles = new List<Triangle>();
            currentTriangulation.ForEach(triangle => {
                if (isPointInsideCircumcircleTriangle(points[pointIndex].position, triangle)) {
                    badTriangles.Add(triangle);
                }
            });

            yield return new WaitForSeconds(_speedConstructionMesh);
            for (int badTriangleIndex = 0; badTriangleIndex < badTriangles.Count; badTriangleIndex++) {
                currentTriangulation.Remove(badTriangles[badTriangleIndex]);
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

            polygon.ForEach(edge => {
                Debug.DrawLine(edge.origin.position* transform.localScale.x + transform.position, edge.end.position* transform.localScale.x + transform.position, Color.yellow, 2f);
            });

            yield return new WaitForSeconds(_speedConstructionMesh);
            for (int polygonIndex = 0; polygonIndex < polygon.Count; polygonIndex++) {
                Triangle newTri = new Triangle();
                newTri.first = polygon[polygonIndex];
                newTri.second = new Edge(points[pointIndex], polygon[polygonIndex].origin);
                newTri.third = new Edge(polygon[polygonIndex].end, points[pointIndex]);

                Vector3 normal = Vector3.Cross(newTri.first.GetEdgeVector(), newTri.second.GetEdgeVector());

                if (Vector3.Dot(normal, plane.normal) < 0) {
                    Edge aux = newTri.first;
                    newTri.first = newTri.third;
                    newTri.third = aux;
                }
                currentTriangulation.Add(newTri);
                yield return new WaitForSeconds(_speedConstructionMesh);
            }
            

        }

        yield return new WaitForSeconds(_speedConstructionMesh);

        List<Triangle> finalcurrentTriangulation = new List<Triangle>();
        currentTriangulation.ForEach(triangle => {
            if (!isSharedVerticeSuperTriangle(triangle)) {
                finalcurrentTriangulation.Add(triangle);
            }
        });


        currentTriangulation = finalcurrentTriangulation;
        isTriangulationFinished = true;
        currentVertices.Remove(superTriangle.first.origin);
        currentVertices.Remove(superTriangle.second.origin);
        currentVertices.Remove(superTriangle.third.origin);

        yield return new WaitForSeconds(_speedConstructionMesh);

    }

    private bool isSharedVerticeSuperTriangle(Triangle triangle) {
        return triangle.first.origin.index == -1 || triangle.second.origin.index == -1 || triangle.third.origin.index == -1 ||
               triangle.first.origin.index == -2 || triangle.second.origin.index == -2 || triangle.third.origin.index == -2 ||
               triangle.first.origin.index == -3 || triangle.second.origin.index == -3 || triangle.third.origin.index == -3;
    }


    
    private bool isPointInsideCircumcircleTriangle(Vector3 point, Triangle triangle) {
        Vector3 ac = triangle.third.origin.position - triangle.first.origin.position;
        Vector3 ab = triangle.second.origin.position - triangle.first.origin.position;
        Vector3 abXac = Vector3.Cross(ab, ac);

        Vector3 toCircumsphereCenter = (Vector3.Cross(abXac, ab) * len2(ac) + Vector3.Cross(ac, abXac) * len2(ab)) / (2f * len2(abXac));
        float circumradius = toCircumsphereCenter.magnitude;
        Vector3 circumcenter = triangle.first.origin.position + toCircumsphereCenter;

        float distancePointToCircumcenter = Vector3.Distance(point, circumcenter);
        bool isPointInside = distancePointToCircumcenter < circumradius;

        if (isPointInside) {
            //DrawCircle(circumcenter, circumradius, 100);
            Debug.Log("[X] Circumcenter: " + circumcenter + " Radius: " + circumradius + " Distance: " + distancePointToCircumcenter);

        }
        else {
            Debug.Log("[O] Circumcenter: " + circumcenter + " Radius: " + circumradius + " Distance: " + distancePointToCircumcenter);

        }
        return isPointInside;
        
        //circumcenterX /= (Mathf.Sin(2 * angleFirst) + Mathf.Sin(2 * angleSecond) + Mathf.Sin(2 * angleThird));
        //circumcenterX /= (Mathf.Sin(2 * angleFirst) + Mathf.Sin(2 * angleSecond) + Mathf.Sin(2 * angleThird));
    }

    private bool isPointInsideCircumcircleTriangle_working_well(Vector3 point, Triangle triangle) {
        Vector3 ac = triangle.third.origin.position - triangle.first.origin.position;
        Vector3 ab = triangle.second.origin.position - triangle.first.origin.position;
        Vector3 abXac = Vector3.Cross(ab, ac);

        Vector3 toCircumsphereCenter = (Vector3.Cross(abXac, ab) * len2(ac) + Vector3.Cross(ac, abXac) * len2(ab)) / (2f * len2(abXac));
        float circumradius = toCircumsphereCenter.magnitude;
        Vector3 circumcenter = triangle.first.origin.position + toCircumsphereCenter;

        return (Mathf.Pow(point.x - circumcenter.x, 2) + Mathf.Pow(point.y - circumcenter.y, 2) + Mathf.Pow(point.z - circumcenter.z, 2)) <= Mathf.Pow(circumradius, 2);
    }

    private float len2(Vector3 v) {
        return v.x*v.x + v.y*v.y + v.z*v.z;
    }



}
