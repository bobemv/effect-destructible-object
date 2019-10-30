using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VerticeCoverHole{

    public VerticeCoverHole(Vector3 _position) {
        position = _position;
        previouslyConnected = new List<Vector3>();
    }
    public Vector3 position;

    public List<Vector3> previouslyConnected;
    public int index;
}

public class VerticeMesh {

    public VerticeMesh(Vector3 _position, int _indexMesh) {
        position = _position;
        indexMesh = _indexMesh;
        triangles = new List<TriangleMesh>();
        toBeDeleted = false;
    }
    public int indexMesh;

    public Vector3 position;

    public List<TriangleMesh> triangles;

    public bool toBeDeleted;

    override public string ToString() {
        return "Index: " + indexMesh + " - Position: " + position + " - Num triangles: " + triangles.Count;
    }
}

public class TriangleMesh {
    public TriangleMesh(VerticeMesh _first, VerticeMesh _second, VerticeMesh _third, int _indexMesh) {
        first = _first;
        second = _second;
        third = _third;
        indexMesh = _indexMesh;
    }

    public int indexMesh;
    public VerticeMesh first;
    public VerticeMesh second;
    public VerticeMesh third;
}


public class MeshModificationInfo {

}
public class Destructible : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private Material materialDebris;
    [SerializeField] private GameObject _verticePrefab;
    private Mesh mesh;

    private List<VerticeMesh> verticesMesh = new List<VerticeMesh>();
    private List<TriangleMesh> trianglesMesh = new List<TriangleMesh>();

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        ConvertMeshToVerticesAndTriangles(mesh);
    }

    void Update() {
        //Convert();
    }

    public void ConvertMeshToVerticesAndTriangles(Mesh mesh)  {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        mesh.GetVertices(vertices);
        mesh.GetTriangles(triangles, 0);
        verticesMesh = new List<VerticeMesh>();
        trianglesMesh = new List<TriangleMesh>();


        for (int i = 0; i < vertices.Count; i++) {
            verticesMesh.Add(new VerticeMesh(vertices[i], i));
        }

        for (int i = 0; i < triangles.Count; i+=3) {
            TriangleMesh newTriangleMesh = new TriangleMesh(verticesMesh[triangles[i]], verticesMesh[triangles[i + 1]], verticesMesh[triangles[i + 2]], i);
            trianglesMesh.Add(newTriangleMesh);
            verticesMesh[triangles[i]].triangles.Add(newTriangleMesh);
            verticesMesh[triangles[i + 1]].triangles.Add(newTriangleMesh);
            verticesMesh[triangles[i + 2]].triangles.Add(newTriangleMesh);
        }
    }

    public void Destruction(Vector3 impact, float radiusExplosion, Vector3 impactDir) {

        //ConvertMeshToVerticesAndTriangles(mesh);
        
        // -- START VERTICE DELETION --
        for (int i = 0; i < verticesMesh.Count; i++) {
            verticesMesh[i].toBeDeleted = Vector3.Distance(verticesMesh[i].position * transform.localScale.x, impact - transform.position) < radiusExplosion;
        }

        List<VerticeMesh> verticesToBeDeleted = verticesMesh.FindAll(verticeMesh => verticeMesh.toBeDeleted == true);
        int numVerticesToBeDeleted = 0;
        int numTrianglesToBeDeleted = 0;
        List<VerticeMesh> verticesEdgeHole = new List<VerticeMesh>();

        for (int i = 0; i < verticesToBeDeleted.Count; i++) {
            List<TriangleMesh> trianglesToCheck = verticesToBeDeleted[i].triangles;

            while (trianglesToCheck.Count > 0) {
                TriangleMesh triangle = trianglesToCheck[0];

                if (triangle.first.toBeDeleted) {
                    triangle.first.triangles.Remove(triangle);
                }
                else {
                    if (!verticesEdgeHole.Exists(verticeEdgeHole => verticeEdgeHole == triangle.first)) {
                        verticesEdgeHole.Add(triangle.first);
                    }
                }
                if (triangle.second.toBeDeleted) {
                    triangle.second.triangles.Remove(triangle);
                }
                else {
                    if (!verticesEdgeHole.Exists(verticeEdgeHole => verticeEdgeHole == triangle.second)) {
                        verticesEdgeHole.Add(triangle.second);
                    }
                }
                if (triangle.third.toBeDeleted) {
                    triangle.third.triangles.Remove(triangle);
                }
                else {
                    if (!verticesEdgeHole.Exists(verticeEdgeHole => verticeEdgeHole == triangle.third)) {
                        verticesEdgeHole.Add(triangle.third);
                    }
                }
                trianglesMesh.Remove(triangle);
                numTrianglesToBeDeleted++;
            }
        }

        numVerticesToBeDeleted = verticesMesh.RemoveAll(verticeMesh => verticeMesh.toBeDeleted == true);

        int indexVerticeMesh = 0;
        verticesMesh.ForEach(verticeMesh => verticeMesh.indexMesh = indexVerticeMesh++);




        // ---- END VERTICE DELETION ----
        //verticesEdgeHole
        //verticesMesh
        //trianglesMesh

        // START CREATING HOLE VERTICES
        Vector3 dirHole = new Vector3(0,0,0);
        Vector3 impactScaled = impact * 0.2f;
        verticesEdgeHole.ForEach(verticeEdgeHole => {
            dirHole += (verticeEdgeHole.position - impactScaled);
        });
        dirHole /= verticesEdgeHole.Count;
        dirHole.Normalize();

        Vector3 pointToConverge = dirHole * (radiusExplosion / transform.localScale.x) * 0.3f;

        List<VerticeMesh> layerHole = new List<VerticeMesh>();
        List<VerticeMesh> newLayerHole = new List<VerticeMesh>();
        List<VerticeMesh> verticesHole = new List<VerticeMesh>();

        layerHole = verticesEdgeHole;
        verticesEdgeHole.ForEach(verticeEdgeHole => {
            verticeEdgeHole.position = Vector3.Lerp(verticeEdgeHole.position, pointToConverge, (Random.Range(0, 0.3f)));
        });
        for (float i = 0; i < verticesEdgeHole.Count; i+=1f) {
            //verticesEdgeHole.RemoveAt(Mathf.FloorToInt(i));
        }
        for (int i = 3; i < 3; i++) {
            for (int j = 0; j < layerHole.Count; j++) {
                if (j % i == 0) {
                    VerticeMesh verticeMesh = new VerticeMesh(Vector3.Lerp(layerHole[j].position, pointToConverge, i * (0.20f + Random.Range(-0.02f, 0.02f))) ,indexVerticeMesh++);
                    newLayerHole.Add(verticeMesh);
                }
                
            }
            verticesHole = verticesHole.Concat(newLayerHole).ToList();
            layerHole = newLayerHole;
            newLayerHole = new List<VerticeMesh>();
        }

        verticesMesh = verticesMesh.Concat(verticesHole).ToList();
        verticesHole = verticesHole.Concat(verticesEdgeHole).ToList();

        // ---- END CREATING HOLE VERTICES ----

        // START BOWYER WATSON WITH EDGE VERTICES AND VERTICES GENERATED



        Vector3 firstVectorPlane = Vector3.Cross(impactDir, Vector3.up);
        Vector3 secondVectorPlane = Vector3.Cross(impactDir, firstVectorPlane);
        Plane plane = new Plane(firstVectorPlane, secondVectorPlane);
        plane.DrawPlane(impact, 5f, 10f);
        //plane.normal = dirHole;
        List<Vertice> verticeForTriangulation = new List<Vertice>();

        //List<Vector3> pointsToCustomMesh = new List<Vector3>();

        verticesHole.ForEach(verticeHole => {
            verticeForTriangulation.Add(new Vertice(PointProjectedInPlane(verticeHole.position, plane.normal), verticeHole.indexMesh));
            //pointsToCustomMesh.Add(verticeHole.position);
        });


        //currentTriangulation = new List<Triangle>();
        //currentVertices = new List<Vertice>();

        // SUPER TRIANGLE
        


        //CREATE VERTICES, PROJECT THEM INTO A PLANE AND ADD TO ARRAYS
        //int indexVertices = 0;
        //List<Vertice> verticesProjectedInPlane = new List<Vertice>();

        /*points.ForEach(point => {
            Vertice newVerticeProjected = new Vertice(PointProjectedInPlane(point, plane.normal), indexVertices);
            Vertice newVerticeMesh = new Vertice(point, indexVertices);
            Instantiate(_verticePrefab, point * transform.localScale.x + transform.position, Quaternion.identity);

            verticesProjectedInPlane.Add(newVerticeProjected);
            currentVertices.Add(newVerticeMesh);
            indexVertices++;
        });*/

        Triangle superTriangle = GetSuperTriangle(plane);
        
        List<Triangle> triangulation = BowyerWatson(verticeForTriangulation, plane, superTriangle);



        // ---- END BOWYER WATSON WITH EDGE VERTICES AND VERTICES GENERATED ----

       // START CONVERTING VERTICE AND TRIANGLE DESTRUCTION + RECONSTRUCTION WITH BOWYER-WATSON TO MESH

        Vector3[] meshVertices;
        int[] meshTriangles;

        //trianglesMesh = new List<TriangleMesh>();
        for (int i = 0; i < triangulation.Count; i++) {
            VerticeMesh first = verticesMesh.Find(verticeMesh => verticeMesh.indexMesh == triangulation[i].first.origin.index);
            VerticeMesh second = verticesMesh.Find(verticeMesh => verticeMesh.indexMesh == triangulation[i].second.origin.index);
            VerticeMesh third = verticesMesh.Find(verticeMesh => verticeMesh.indexMesh == triangulation[i].third.origin.index);
            TriangleMesh newTriangleMesh = new TriangleMesh(first, second, third, i);
            trianglesMesh.Add(newTriangleMesh);
            first.triangles.Add(newTriangleMesh);
            second.triangles.Add(newTriangleMesh);
            third.triangles.Add(newTriangleMesh);
        }
        ConvertTrianglesToMeshVerticesAndTriangles(verticesMesh, trianglesMesh, out meshVertices, out meshTriangles);

        mesh.Clear();
        mesh.vertices = meshVertices;
        mesh.triangles = meshTriangles;


        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

       // ---- END CONVERTING VERTICE AND TRIANGLE DESTRUCTION + RECONSTRUCTION WITH BOWYER-WATSON TO MESH ----


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
            Instantiate(_verticePrefab, points[pointIndex].position * transform.localScale.x + transform.position, Quaternion.identity);

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
