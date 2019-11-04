using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Regeneration : MonoBehaviour
{

    private Mesh mesh;

    private List<VerticeMesh> verticesMesh = new List<VerticeMesh>();
    private List<TriangleMesh> trianglesMesh = new List<TriangleMesh>();
    private List<VerticeMesh> verticeDuplicates = new List<VerticeMesh>();
    private List<TriangleMesh> triangleDuplicates = new List<TriangleMesh>();
    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        ConvertMeshToVerticesAndTriangles(mesh);
    }
     
     public void ConvertMeshToVerticesAndTriangles(Mesh mesh)  {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        mesh.GetVertices(vertices);
        mesh.GetTriangles(triangles, 0);
        verticesMesh = new List<VerticeMesh>();
        trianglesMesh = new List<TriangleMesh>();
        
        //Debug.Log("Mesh sphere: ");
        for (int i = 0; i < vertices.Count; i++) {
            //Debug.Log(i + " " + vertices[i].ToString("#.00000000"));

            verticesMesh.Add(new VerticeMesh(vertices[i], i));
        }
        for (int i = 0; i < triangles.Count; i+=3) {
            TriangleMesh newTriangleMesh = new TriangleMesh(verticesMesh[triangles[i]], verticesMesh[triangles[i + 1]], verticesMesh[triangles[i + 2]], i);
            trianglesMesh.Add(newTriangleMesh);
            verticesMesh[triangles[i]].triangles.Add(newTriangleMesh);
            verticesMesh[triangles[i + 1]].triangles.Add(newTriangleMesh);
            verticesMesh[triangles[i + 2]].triangles.Add(newTriangleMesh);
        }

        /*verticeDuplicates = new List<VerticeMesh>();
        triangleDuplicates = new List<TriangleMesh>();
        int numSameVertices = 0;
        for (int i = 0; i < verticesMesh.Count ; i++) {
            for (int j = i + 1; j < verticesMesh.Count; j++) {
                if (verticesMesh[i].position.x == verticesMesh[j].position.x && verticesMesh[i].position.y == verticesMesh[j].position.y && verticesMesh[i].position.z == verticesMesh[j].position.z) {
                    //Debug.Log("Same vertices: " + verticesMesh[i].indexMesh + " y " + verticesMesh[j].indexMesh);
                    verticesMesh[i].duplicates.Add(verticesMesh[j]);
                    verticeDuplicates.Add(verticesMesh[j]);
                    numSameVertices++;
                }
            }
        }
        for (int i = 0; i < trianglesMesh.Count; i++) {
            bool hasVerticeDuplicated = false;
            hasVerticeDuplicated = verticeDuplicates.Exists(verticeDuplicate => verticeDuplicate.indexMesh == trianglesMesh[i].first.indexMesh);
            hasVerticeDuplicated |= verticeDuplicates.Exists(verticeDuplicate => verticeDuplicate.indexMesh == trianglesMesh[i].second.indexMesh);
            hasVerticeDuplicated |= verticeDuplicates.Exists(verticeDuplicate => verticeDuplicate.indexMesh == trianglesMesh[i].third.indexMesh);

            if (hasVerticeDuplicated) {
                triangleDuplicates.Add(trianglesMesh[i]);
            }
        }*/

        verticesMesh.Sort(delegate(VerticeMesh vertice1, VerticeMesh vertice2) {
            if (vertice1.position.y < vertice2.position.y) {
                return -1;
            }
            if (vertice1.position.y > vertice2.position.y) {
                return 1;
            }
            return 0;
        });

        StartCoroutine(Reconstructing());
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    List<VerticeMesh> verticesDraw = new List<VerticeMesh>();
    List<TriangleMesh> trianglesDraw = new List<TriangleMesh>();
    int verticeIndex = 0;
    int countByLayer = 5;
    public float speedReconstruction = 0.1f;
    private IEnumerator Reconstructing() {

        while (verticeIndex < verticesMesh.Count) {
            List<VerticeMesh> newLayer = new List<VerticeMesh>();
            newLayer = verticesMesh.GetRange(verticeIndex, countByLayer);
            verticeIndex += countByLayer;
            verticesDraw = verticesDraw.Concat(newLayer).ToList();


            trianglesDraw = new List<TriangleMesh>();

            for (int i = 0; i < trianglesMesh.Count; i++) {
                bool isAllVerticesPresent = true;
                isAllVerticesPresent &= verticesDraw.Exists(verticeDraw => verticeDraw.indexMesh == trianglesMesh[i].first.indexMesh);
                isAllVerticesPresent &= verticesDraw.Exists(verticeDraw => verticeDraw.indexMesh == trianglesMesh[i].second.indexMesh);
                isAllVerticesPresent &= verticesDraw.Exists(verticeDraw => verticeDraw.indexMesh == trianglesMesh[i].third.indexMesh);

                if (isAllVerticesPresent) {
                    trianglesDraw.Add(trianglesMesh[i]);
                }
            }

            List<VerticeMesh> edgeHole = new List<VerticeMesh>();

            for (int i = 0; i < verticesDraw.Count ; i++) {
                bool isAllTrianlesPresent = true;
                for (int j = 0; j < verticesDraw[i].triangles.Count; j++) {
                    if (!trianglesDraw.Exists(triangleDraw => triangleDraw == verticesDraw[i].triangles[j])) {
                        isAllTrianlesPresent = false;
                        break;
                    }
                }
                if (!isAllTrianlesPresent) {
                    edgeHole.Add(verticesDraw[i]);
                }
            }


            Vector3[] meshVertices;
            int[] meshTriangles;
            Plane plane = new Plane(Vector3.forward, Vector3.right);
            
            List<Vertice> verticesForTriangulation = new List<Vertice>();

            for (int i = 0; i < edgeHole.Count; i++) {
                verticesForTriangulation.Add(new Vertice(PointProjectedInPlane(edgeHole[i].position, plane.normal), edgeHole[i].indexMesh));
            }

            List<Triangle> triangulation = BowyerWatson(verticesForTriangulation, plane, GetSuperTriangle(plane));

            for (int i = 0; i < triangulation.Count; i++) {
                VerticeMesh first = verticesMesh.Find(verticeMesh => verticeMesh.indexMesh == triangulation[i].first.origin.index);
                VerticeMesh second = verticesMesh.Find(verticeMesh => verticeMesh.indexMesh == triangulation[i].second.origin.index);
                VerticeMesh third = verticesMesh.Find(verticeMesh => verticeMesh.indexMesh == triangulation[i].third.origin.index);
                TriangleMesh newTriangleMesh = new TriangleMesh(first, second, third, i);
                trianglesDraw.Add(newTriangleMesh);
            }

            ConvertTrianglesToMeshVerticesAndTriangles(verticesMesh, trianglesDraw, out meshVertices, out meshTriangles);

            mesh.Clear();
            mesh.vertices = meshVertices;
            mesh.triangles = meshTriangles;


            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            yield return new WaitForSeconds(speedReconstruction);
        }

    }

    private void ConvertTrianglesToMeshVerticesAndTriangles(List<VerticeMesh> verticesGenerated, List<TriangleMesh> trianglesMesh, out Vector3[] meshVertices, out int[] meshTriangles) {
        int sizeMeshVertices = verticesGenerated.Count;
        int sizeMeshTriangles = 3 * trianglesMesh.Count;

        meshVertices = new Vector3[sizeMeshVertices];
        meshTriangles = new int[sizeMeshTriangles];

        for (int i = 0; i < verticesGenerated.Count; i++) {
            meshVertices[verticesGenerated[i].indexMesh] = verticesGenerated[i].position;
        }



        for (int i = 0; i < trianglesMesh.Count; i++) {
            meshTriangles[i * 3] = trianglesMesh[i].first.indexMesh;

            meshTriangles[(i * 3) + 1] = trianglesMesh[i].second.indexMesh;

            meshTriangles[(i * 3) + 2] = trianglesMesh[i].third.indexMesh;
        }


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

    private List<Triangle> BowyerWatson(List<Vertice> points, Plane plane, Triangle superTriangle) {

        List<Triangle> currentTriangulation = new List<Triangle>();
        currentTriangulation.Add(superTriangle);
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
            //Instantiate(_verticePrefab, points[pointIndex].position * transform.localScale.x + transform.position, Quaternion.identity);

            List<Triangle> badTriangles = new List<Triangle>();
            currentTriangulation.ForEach(triangle => {
                if (isPointInsideCircumcircleTriangle(points[pointIndex].position, triangle)) {
                    badTriangles.Add(triangle);
                }
            });


            for (int badTriangleIndex = 0; badTriangleIndex < badTriangles.Count; badTriangleIndex++) {
                currentTriangulation.Remove(badTriangles[badTriangleIndex]);
    
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
                    //Debug.Log("Added to Polygon: o(" + possibleEdge.origin.position + ") e(" + possibleEdge.end.position);
                    polygon.Add(possibleEdge);
                }
                else {
                    //Debug.Log("NOT Added to Polygon: o(" + possibleEdge.origin.position + ") e(" + possibleEdge.end.position);
                }
            });

            polygon.ForEach(edge => {
                //Debug.DrawLine(edge.origin.position* transform.localScale.x + transform.position, edge.end.position* transform.localScale.x + transform.position, Color.yellow, 2f);
            });


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
    
            }
            

        }


        List<Triangle> finalcurrentTriangulation = new List<Triangle>();
        currentTriangulation.ForEach(triangle => {
            if (!isSharedVerticeSuperTriangle(triangle)) {
                finalcurrentTriangulation.Add(triangle);
            }
        });


        currentTriangulation = finalcurrentTriangulation;
        /* 
        currentVertices.Remove(superTriangle.first.origin);
        currentVertices.Remove(superTriangle.second.origin);
        currentVertices.Remove(superTriangle.third.origin);
        */
        return currentTriangulation;

    }

    private bool isSharedVerticeSuperTriangle(Triangle triangle) {
        return triangle.first.origin.index == -1 || triangle.second.origin.index == -1 || triangle.third.origin.index == -1 ||
               triangle.first.origin.index == -2 || triangle.second.origin.index == -2 || triangle.third.origin.index == -2 ||
               triangle.first.origin.index == -3 || triangle.second.origin.index == -3 || triangle.third.origin.index == -3;
    }


    
    private bool isPointInsideCircumcircleTriangle_works(Vector3 point, Triangle triangle) {
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
            //Debug.Log("[X] Circumcenter: " + circumcenter + " Radius: " + circumradius + " Distance: " + distancePointToCircumcenter);

        }
        else {
            //Debug.Log("[O] Circumcenter: " + circumcenter + " Radius: " + circumradius + " Distance: " + distancePointToCircumcenter);

        }
        return isPointInside;
        
        //circumcenterX /= (Mathf.Sin(2 * angleFirst) + Mathf.Sin(2 * angleSecond) + Mathf.Sin(2 * angleThird));
        //circumcenterX /= (Mathf.Sin(2 * angleFirst) + Mathf.Sin(2 * angleSecond) + Mathf.Sin(2 * angleThird));
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
        
        //circumcenterX /= (Mathf.Sin(2 * angleFirst) + Mathf.Sin(2 * angleSecond) + Mathf.Sin(2 * angleThird));
        //circumcenterX /= (Mathf.Sin(2 * angleFirst) + Mathf.Sin(2 * angleSecond) + Mathf.Sin(2 * angleThird));
    }

    private float len2(Vector3 v) {
        return v.x*v.x + v.y*v.y + v.z*v.z;
    }
}
