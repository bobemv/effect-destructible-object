using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private Material materialDebris;
    [SerializeField] private GameObject _verticePrefab;
    private Mesh mesh;
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Destruction(Vector3 impact, float radiusExplosion, Vector3 impactDir) {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> verticesRemoved = new List<Vector3>();
        List<int> trianglesRemoved = new List<int>();
        List<Color> colors = new List<Color>();
        List<Vector2> uvs = new List<Vector2>();
        mesh.GetVertices(vertices);
        Debug.Log("Vertices: " + vertices.Count);
                
        
        mesh.GetTriangles(triangles, 0);

        Debug.DrawLine(Camera.main.transform.position, impact, Color.white, 1.0f);
        Debug.DrawLine(transform.transform.position, impact, Color.white, 1.0f);

        List<Vector3> verticesCoverHole = new List<Vector3>();
        for (int verticesIndex = 0; verticesIndex < vertices.Count; verticesIndex++) {
            Debug.Log("Distance("+vertices[verticesIndex]+" y "+(impact - transform.transform.position)+"): " + Vector3.Distance(vertices[verticesIndex], impact - transform.transform.position));
            if (Vector3.Distance(vertices[verticesIndex] * transform.localScale.x, impact - transform.transform.position) < radiusExplosion) {
                
                Debug.Log("Quitando vertice: index " + verticesIndex + " value: " + vertices[verticesIndex]);
                
                //Debug.Log("Triangles left: " + (triangles.Count / 3));
                bool isVerticeImpactAdded = false;
                for (int trianglesIndex = 0; trianglesIndex < triangles.Count; trianglesIndex++) {
                    
                    //Debug.Log("Index triangle: " + trianglesIndex);
                    if(triangles[trianglesIndex] == verticesIndex) {
                //Debug.Log("Quitando triangulo: ");

                        int startTriangleIndex = trianglesIndex - (trianglesIndex%3);
                        List<Vector3> verticesDeletedFromTriangle = new List<Vector3>();
                        List<Vector3> verticesNotDeletedFromTriangle = new List<Vector3>();
                        for (int triangleIndex = 0; triangleIndex <= 2; triangleIndex++) {

                            if(Vector3.Distance(vertices[triangles[startTriangleIndex + triangleIndex]] * transform.localScale.x, impact - transform.transform.position) < radiusExplosion) {
                                verticesDeletedFromTriangle.Add(vertices[triangles[startTriangleIndex + triangleIndex]]);
                            }
                            else {
                                verticesNotDeletedFromTriangle.Add(vertices[triangles[startTriangleIndex + triangleIndex]]);
                            }
                        }
                        for (int triangleIndex = 2; triangleIndex >= 0; triangleIndex--) {
                            //Debug.Log((startTriangleIndex + triangleIndex) + " - " + triangles.ElementAtOrDefault(startTriangleIndex + triangleIndex));

                            trianglesRemoved.Add(triangles[startTriangleIndex + triangleIndex]);
                            triangles.RemoveAt(startTriangleIndex + triangleIndex);

                        }

                        if (verticesDeletedFromTriangle.Count == 1 && !isVerticeImpactAdded) {
                            Vector3 verticeBorderImpact = verticesDeletedFromTriangle[0];
                            isVerticeImpactAdded = true;
                            //Vector3 directionToImpact = impact - verticeBorderImpact;
                            //directionToImpact = new Vector3(directionToImpact.x,directionToImpact.y,0); //we dont want "going back" to the impact
                            //directionToImpact.Normalize();
                            //verticeBorderImpact += impactDir + directionToImpact;

                            verticesCoverHole.Add(verticeBorderImpact);
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
                verticesRemoved.Add(vertices[verticesIndex]);
                vertices.RemoveAt(verticesIndex);
                verticesIndex--; //we stay in the sema position because there has been deletions
            }
        }

        //Debug.Log("Vertices filtered: " + vertices.Count);


        //Debug.Log("Triangles: " + triangles.Count);
        //Debug.Log("Triangles info: ");
        //triangles.ForEach(triangle => {
            //Debug.Log("Triangle: " + triangle);
        //});

        //Debug.Log("Vertices in hole: " + verticesCoverHole.Count);
        //for (int i = 0; i < verticesCoverHole.Count; i++) {
            //Debug.DrawLine(verticesCoverHole[i], verticesCoverHole[(i+1)%verticesCoverHole.Count], Color.white, 3);
        //}
        //GameObject.Find("CustomMesh").GetComponent<CustomMesh>().points = verticesCoverHole;
        //GameObject.Find("CustomMesh").GetComponent<CustomMesh>().Startv2();
        //List<Triangle> trianglesHole = BowyerWatson(verticesCoverHole, impact, impactDir);
        Vector3 firstVectorPlane = Vector3.Cross(impactDir, Vector3.up);
        Vector3 secondVectorPlane = Vector3.Cross(impactDir, firstVectorPlane);

        List<Vertice> verticeForTriangulation = new List<Vertice>();
        int indexVertices = vertices.Count;
        verticesCoverHole.ForEach(verticeCoverHole => {
            verticeForTriangulation.Add(new Vertice(verticeCoverHole, indexVertices++));
        });
        //verticeForTriangulation.Add(new Vertice((impact - transform.position) * 0.2f, indexVertices++));
        List<Triangle> trianglesHole = BowyerWatsonv3(verticeForTriangulation, new Plane(Vector3.Cross(impactDir, Vector3.up), Vector3.Cross(impactDir, firstVectorPlane)));
        Debug.Log("Triangles created: " +  trianglesHole.Count);

        //ConvertTrianglesToMeshVerticesAndTrianglesv1(trianglesHole, vertices, triangles);
        List<Vector3> verticesMesh = new List<Vector3>();
        List<int> trianglesMesh = new List<int>();
        AddGeneratedVerticesAndTrianglesToMesh(mesh, verticesMesh, trianglesMesh, verticeForTriangulation, trianglesHole);

        mesh.Clear();
        mesh.vertices = vertices.ToList().Concat(verticesMesh).ToArray();
        mesh.triangles = triangles.ToList().Concat(trianglesMesh).ToArray();


        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        Debug.Log("Vertices removed: ");
        //verticesRemoved.ForEach(vertice => {
            //Debug.Log("Vertice: " + vertice + " Distance: " + Vector3.Distance(vertice * transform.localScale.x, impact - transform.transform.position));
        //}); 

        
        

        return;
    }

    private List<Triangle> BowyerWatson(List<Vector3> points, Vector3 impact, Vector3 impactDir) {

        //List<Triangle> triangulationv2 = new List<Triangle>();
        Debug.Log("Start Bowyer-watson");
        Triangle superTriangle = GetSuperTrianglev1(points, impact, impactDir);
        superTriangle.DrawTriangle();
        List<Triangle> triangulationv2 = new List<Triangle>();
        triangulationv2.Add(superTriangle);
        //points.ForEach(point => {
        for (int pointIndex = 0; pointIndex < points.Count; pointIndex++) {
            Vector3 verticePosition = new Vector3(points[pointIndex].x, points[pointIndex].y, points[pointIndex].z);
            Vertice verticePoint = new Vertice(verticePosition, 0);
            Instantiate(_verticePrefab, verticePosition * transform.localScale.x + transform.position, Quaternion.identity);

            List<Triangle> badTriangles = new List<Triangle>();
            triangulationv2.ForEach(triangle => {
                if (isPointInsideCircumcircleTrianglev1(points[pointIndex], triangle)) {
                    badTriangles.Add(triangle);
                }
            });


            for (int badTriangleIndex = 0; badTriangleIndex < badTriangles.Count; badTriangleIndex++) {
                triangulationv2.Remove(badTriangles[badTriangleIndex]);
    
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

        Debug.Log("Polygon count " + polygon.Count);

            
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
    
            }
            

        }


        List<Triangle> finalTriangulationv2 = new List<Triangle>();
        Debug.Log("finalTriangulationv2 number before: " + triangulationv2.Count);
        triangulationv2.ForEach(triangle => {
            if (!isSharedVerticeTrianglesv1(triangle, superTriangle)) {
                finalTriangulationv2.Add(triangle);
            }
        });
        Debug.Log("finalTriangulationv2 number after: " + finalTriangulationv2.Count);

        triangulationv2 = finalTriangulationv2;

        return triangulationv2;
    }

    //TODO coger plano perpendicular a la direccion de impacto y coger un triangulo bien grandote y sabrosote
    private Triangle GetSuperTrianglev1(List<Vector3> points, Vector3 impact, Vector3 impactDir) {
        Triangle superTriangle = new Triangle();

        Vector3 firstVectorPlane = Vector3.Cross(impactDir, Vector3.up);
        Vector3 secondVectorPlane = Vector3.Cross(impactDir, firstVectorPlane);

        

        Vertice firstVertice = new Vertice(impact + firstVectorPlane * 100, 0);
        Vertice secondtVertice = new Vertice(impact - firstVectorPlane * 20 + secondVectorPlane * 80, 0);
        Vertice thirdVertice = new Vertice(impact - firstVectorPlane * 20 - secondVectorPlane * 80, 0);

        superTriangle.first = new Edge(secondtVertice, firstVertice);
        superTriangle.second = new Edge(firstVertice, thirdVertice);
        superTriangle.third = new Edge(thirdVertice, secondtVertice);
        return superTriangle;
    }

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
            Debug.Log("Point is inside!!");
            //DrawCircle(circumcenter, circumradius, 100);
        }
        return isPointInside;
        
        //circumcenterX /= (Mathf.Sin(2 * angleFirst) + Mathf.Sin(2 * angleSecond) + Mathf.Sin(2 * angleThird));
        //circumcenterX /= (Mathf.Sin(2 * angleFirst) + Mathf.Sin(2 * angleSecond) + Mathf.Sin(2 * angleThird));
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

    private void ConvertTrianglesToMeshVerticesAndTrianglesv1(List<Triangle> trianglesGenerated, List<Vector3> meshVertices, List<int> meshTriangles) {
        int indexVertice = meshVertices.Count;
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
            Instantiate(_verticePrefab, vertices[verticesIndex].position * transform.localScale.x + transform.position, Quaternion.identity);

            List<Triangle> badTriangles = new List<Triangle>();
            triangulation.ForEach(triangle => {
                if (isPointInsideCircumcircleTrianglev4(verticeProjected.position, triangle)) {
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
                    //Debug.Log("Added to Polygon: o(" + possibleEdge.origin.position + ") e(" + possibleEdge.end.position);
                    polygon.Add(possibleEdge);
                }
                else {
                    //Debug.Log("NOT Added to Polygon: o(" + possibleEdge.origin.position + ") e(" + possibleEdge.end.position);
                }
            });


            for (int polygonIndex = 0; polygonIndex < polygon.Count; polygonIndex++) {
                Triangle newTri = new Triangle();
                newTri.first = polygon[polygonIndex];
                newTri.second = new Edge(verticeProjected, polygon[polygonIndex].origin);
                newTri.third = new Edge(polygon[polygonIndex].end, verticeProjected);

                Vector3 normal = Vector3.Cross(newTri.first.GetEdgeVector(), newTri.second.GetEdgeVector());

                if (Vector3.Dot(normal, plane.normal) < 0) {
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
                Vector3 ac = triangle.third.origin.position - triangle.first.origin.position;
        Vector3 ab = triangle.second.origin.position - triangle.first.origin.position;
        Vector3 abXac = Vector3.Cross(ab, ac);

        Vector3 toCircumsphereCenter = (Vector3.Cross(abXac, ab) * len2(ac) + Vector3.Cross(abXac, ac) * len2(ab)) / (2f * len2(abXac));
        float circumradius2 = toCircumsphereCenter.magnitude;
        Vector3 circumcenter2 = triangle.first.origin.position + toCircumsphereCenter;

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
        Debug.Log("v3 Circumcenter: " + circumcenter + " Radius: " + circumradius + " Distance: " + distancePointToCircumcenter);
        Debug.Log("v4 Circumcenter: " + circumcenter2 + " Radius: " + circumradius2 );
        bool isPointInside = distancePointToCircumcenter < circumradius;

        if (isPointInside) {
            //DrawCircle(circumcenter, circumradius, 100);
        }
        return isPointInside;
        
        //circumcenterX /= (Mathf.Sin(2 * angleFirst) + Mathf.Sin(2 * angleSecond) + Mathf.Sin(2 * angleThird));
        //circumcenterX /= (Mathf.Sin(2 * angleFirst) + Mathf.Sin(2 * angleSecond) + Mathf.Sin(2 * angleThird));
    }

    private bool isPointInsideCircumcircleTrianglev4(Vector3 point, Triangle triangle) {
        Vector3 ac = triangle.third.origin.position - triangle.first.origin.position;
        Vector3 ab = triangle.second.origin.position - triangle.first.origin.position;
        Vector3 abXac = Vector3.Cross(ab, ac);

        Vector3 toCircumsphereCenter = (Vector3.Cross(abXac, ab) * len2(ac) + Vector3.Cross(ac, abXac) * len2(ab)) / (2f * len2(abXac));
        float circumradius = toCircumsphereCenter.magnitude;
        Vector3 circumcenter = triangle.first.origin.position + toCircumsphereCenter;

        float distancePointToCircumcenter = Vector3.Distance(point, circumcenter);
        //Debug.Log("Circumcenter: " + circumcenter + " Radius: " + circumradius + " Distance: " + distancePointToCircumcenter);
        bool isPointInside = distancePointToCircumcenter < circumradius;

        if (isPointInside) {
            //DrawCircle(circumcenter, circumradius, 100);
        }
        return isPointInside;
        
        //circumcenterX /= (Mathf.Sin(2 * angleFirst) + Mathf.Sin(2 * angleSecond) + Mathf.Sin(2 * angleThird));
        //circumcenterX /= (Mathf.Sin(2 * angleFirst) + Mathf.Sin(2 * angleSecond) + Mathf.Sin(2 * angleThird));
    }

    private float len2(Vector3 v) {
        return v.x*v.x + v.y*v.y + v.z*v.z;
    }

}
