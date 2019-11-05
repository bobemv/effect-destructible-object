using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangulation : MonoBehaviour
{
[SerializeField] private int sizeCircle = 1;
    [SerializeField] private float step = 0.2f;
    [SerializeField] public List<Vector3> pointsToTriangulate;
    [SerializeField] private GameObject _verticePrefab;
    // Start is called before the first frame update
    private Triangle superTriangle = new Triangle();
    private List<Vertice> vertices = new List<Vertice>();
    List<Triangle> currentTriangulation = new List<Triangle>();
    List<Vertice> currentVertices = new List<Vertice>();
    List<Vertice> triangulationVerticev2 = new List<Vertice>();
    public Plane plane;
    public Vector3 centerSphere;
    public float radiusSphere;

    private int indexVertice = 0;
    public bool isActive = false;
    public float _speedConstructionMesh = 1.0f;
    public bool isTriangulationFinished = false;


    void Update() {
        Mesh customMesh = new Mesh();
    
        Vector3[] meshVertices;
        int[] meshTriangles;

        ConvertTrianglesToMeshVerticesAndTriangles(currentTriangulation, currentVertices, out meshVertices, out meshTriangles);

        customMesh.vertices = meshVertices;
        customMesh.triangles = meshTriangles;

        customMesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = customMesh;
        
    }


    public void Start() {
        currentTriangulation = new List<Triangle>();
        currentVertices = new List<Vertice>();
        plane = new Plane(new Vector3(0, 0, 1), new Vector3(1, 0, 0));
        // SUPER TRIANGLE
        Triangle superTriangle = GetSuperTriangle(plane);

        currentVertices.Add(superTriangle.first.origin);
        currentVertices.Add(superTriangle.second.origin);
        currentVertices.Add(superTriangle.third.origin);

        //CREATE VERTICES, PROJECT THEM INTO A PLANE AND ADD TO ARRAYS
        pointsToTriangulate = new List<Vector3>();
        
        for (int i = 0; i < 10; i++) {
            pointsToTriangulate.Add(new Vector3(Mathf.Cos(i * Mathf.PI * 2 / 10) + Random.Range(0, 0.01f), 0, Mathf.Sin(i * Mathf.PI * 2 / 10) + Random.Range(0, 0.01f)));
        }
        for (int i = 0; i < 5; i++) {
            pointsToTriangulate.Add(new Vector3(Mathf.Cos(i * Mathf.PI * 2 / 5) * 0.5f + Random.Range(0, 0.01f), 0, Mathf.Sin(i * Mathf.PI * 2 / 5) * 0.5f + Random.Range(0, 0.01f) ));
        }
        for (int i = 0; i < 1; i++) {
            pointsToTriangulate.Add(new Vector3(Random.Range(0, 0.01f), 0, Random.Range(0, 0.01f) ));
        }

        int indexVertices = 0;
        List<Vertice> verticesProjectedInPlane = new List<Vertice>();

        pointsToTriangulate.ForEach(pointToTriangulate => {
            //Vertice newVerticeProjected = new Vertice(PointProjectedStereographic(pointToTriangulate, centerSphere, radiusSphere), indexVertices);
            Vertice newVerticeMesh = new Vertice(pointToTriangulate, indexVertices++);
            //Instantiate(_verticePrefab, point * transform.localScale.x + transform.position, Quaternion.identity);
            //Debug.Log(pointToTriangulate.ToString("#.00000"));
            //verticesProjectedInPlane.Add(newVerticeProjected);
            currentVertices.Add(newVerticeMesh);
        });

        isTriangulationFinished = false;
        StartCoroutine(BowyerWatson(currentVertices, plane, superTriangle));

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
                if (possibleNotSharedEdges.FindAll(edge => Edge.IsSameVertices(possibleEdge, edge)).Count == 1) {
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
        float m00 = triangle.first.origin.position.x - point.x;
        float m01 = triangle.first.origin.position.z - point.z;
        float m02 = Mathf.Pow(triangle.first.origin.position.x - point.x, 2) + Mathf.Pow(triangle.first.origin.position.z - point.z, 2);
        float m03 = 0;
        float m10 = triangle.second.origin.position.x - point.x;
        float m11 = triangle.second.origin.position.z - point.z;
        float m12 = Mathf.Pow(triangle.second.origin.position.x - point.x, 2) + Mathf.Pow(triangle.second.origin.position.z - point.z, 2);
        float m13 = 0;
        float m20 = triangle.third.origin.position.x - point.x;
        float m21 = triangle.third.origin.position.z - point.z;
        float m22 = Mathf.Pow(triangle.third.origin.position.x - point.x, 2) + Mathf.Pow(triangle.third.origin.position.z - point.z, 2);
        float m23 = 0;
        float m30 = 0;
        float m31 = 0;
        float m32 = 0;
        float m33 = 1f;

        Vector4 column0 = new Vector4(m00, m10, m20, m30);
        Vector4 column1 = new Vector4(m01, m11, m21, m31);
        Vector4 column2 = new Vector4(m02, m12, m22, m32);
        Vector4 column3 = new Vector4(m03, m13, m23, m33);

        Matrix4x4 robustMatrix = new Matrix4x4(column0, column1, column2, column3);

        return robustMatrix.determinant > 0;
    
    }

}
