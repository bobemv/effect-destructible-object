﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Cube {
    public int combination;
    public int index;
    public int[] adyacentCubes;
}
public class MarchingCubes : MonoBehaviour
{
    private Mesh mesh;
    private List<Vector3> vertices;
    private List<int> triangles;



    private List<int[]> combinations;
    private int[] edgeTable;
    private int[,] relationEdgeToVertices;
    private Vector3[] verticesCubeRelativePositions;
    private int sizeMaxCube = 10;
    private Cube[, ,] cubes;
    private bool[, , ,] impacted;
    public GameObject _verticePrefab;
    // Start is called before the first frame update

    float sizeCube = 0.5f;
    float radiusSphere = 2.5f;
    Vector3 centerSphere = new Vector3(2.5f, 2.5f, 2.5f);
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
        vertices = new List<Vector3>();
        triangles = new List<int>();

        cubes = new Cube[sizeMaxCube, sizeMaxCube, sizeMaxCube];
        impacted = new bool[sizeMaxCube, sizeMaxCube, sizeMaxCube, 8];


        for (int k = 0; k < sizeMaxCube; k++) {
            for (int j = 0; j < sizeMaxCube; j++) {
                for (int i = 0; i < sizeMaxCube; i++) {
                    Cube cube = cubes[i, j, k];
                    cube.index = i + sizeMaxCube * j + sizeMaxCube * sizeMaxCube * k;
                    cube.combination = 0; 
                    Vector3 vertice1 = new Vector3(i * sizeCube + sizeCube, j * sizeCube, k * sizeCube);
                    Vector3 vertice2 = new Vector3(i * sizeCube, j * sizeCube, k * sizeCube);
                    Vector3 vertice3 = new Vector3(i * sizeCube, j * sizeCube, k * sizeCube + sizeCube);
                    Vector3 vertice4 = new Vector3(i * sizeCube + sizeCube, j * sizeCube, k * sizeCube + sizeCube);
                    Vector3 vertice5 = new Vector3(i * sizeCube + sizeCube, j * sizeCube + sizeCube, k * sizeCube);
                    Vector3 vertice6 = new Vector3(i * sizeCube, j * sizeCube + sizeCube, k * sizeCube);
                    Vector3 vertice7 = new Vector3(i * sizeCube, j * sizeCube + sizeCube, k * sizeCube + sizeCube);
                    Vector3 vertice8 = new Vector3(i * sizeCube + sizeCube, j * sizeCube + sizeCube, k * sizeCube + sizeCube);

                    if (Vector3.Distance(vertice1, centerSphere) < radiusSphere && !impacted[i, j, k, 0]) {
                        cube.combination |= (1 << 0);
                        //Instantiate(_verticePrefab, vertice1, Quaternion.identity);
                    }
                    if (Vector3.Distance(vertice2, centerSphere) < radiusSphere && !impacted[i, j, k, 1]) {
                        cube.combination |= (1 << 1);
                        //Instantiate(_verticePrefab, vertice2, Quaternion.identity);

                    }
                    if (Vector3.Distance(vertice3, centerSphere) < radiusSphere && !impacted[i, j, k, 2]) {
                        cube.combination |= (1 << 2);
                        //Instantiate(_verticePrefab, vertice3, Quaternion.identity);

                    }
                    if (Vector3.Distance(vertice4, centerSphere) < radiusSphere && !impacted[i, j, k, 3]) {
                        cube.combination |= (1 << 3);
                        //Instantiate(_verticePrefab, vertice4, Quaternion.identity);

                    }
                    if (Vector3.Distance(vertice5, centerSphere) < radiusSphere && !impacted[i, j, k, 4]) {
                        cube.combination |= (1 << 4);
                        //Instantiate(_verticePrefab, vertice5, Quaternion.identity);

                    }
                    if (Vector3.Distance(vertice6, centerSphere) < radiusSphere && !impacted[i, j, k, 5]) {
                        cube.combination |= (1 << 5);
                        //Instantiate(_verticePrefab, vertice6, Quaternion.identity);

                    }
                    if (Vector3.Distance(vertice7, centerSphere) < radiusSphere && !impacted[i, j, k, 6]) {
                        cube.combination |= (1 << 6);
                        //Instantiate(_verticePrefab, vertice7, Quaternion.identity);

                    }
                    if (Vector3.Distance(vertice8, centerSphere) < radiusSphere && !impacted[i, j, k, 7]) {
                        cube.combination |= (1 << 7);
                        //Instantiate(_verticePrefab, vertice8, Quaternion.identity);

                    }

                    int edgeMask = edgeTable[cube.combination];

                    if (edgeMask == 0) {
                        continue;
                    }

                    int[] edges = new int[12];

					for (int edgeIndex = 0; edgeIndex < 12; edgeIndex++) {
						if ((edgeMask & (1 << edgeIndex)) == 0) {
							continue;
						}

						int verticeStartIndex = relationEdgeToVertices[edgeIndex, 0];
						int verticeEndIndex = relationEdgeToVertices[edgeIndex, 1];
						Vector3 p0 = verticesCubeRelativePositions[verticeStartIndex];
						Vector3 p1 = verticesCubeRelativePositions[verticeEndIndex];

                        Vector3 midPoint = ((p1 - p0) * 0.5f) + p0;

                        midPoint *= sizeCube;
                        midPoint += new Vector3(i * sizeCube, j * sizeCube, k * sizeCube);

						edges[edgeIndex] = vertices.Count;
						vertices.Add(midPoint);

                    }

                    int[] combination = combinations[cube.combination];

                    /*for (int n = 0; n < combination.Length; n++) {
                        triangles.Add(edges[combination[n]]);
                    }*/
                    for (int n = 0; n < combination.Length; n+=3) {
                        triangles.Add(edges[combination[n + 2]]);
                        triangles.Add(edges[combination[n + 1]]);
                        triangles.Add(edges[combination[n]]);
                    }
                }
            }
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();


        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        GetComponent<MeshCollider>().sharedMesh = null;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }


    public void Destruction(Vector3 impact, float radiusImpact) {
        for (int k = 0; k < sizeMaxCube; k++) {
            for (int j = 0; j < sizeMaxCube; j++) {
                for (int i = 0; i < sizeMaxCube; i++) {
                    Vector3 vertice1 = new Vector3(i * sizeCube + sizeCube, j * sizeCube, k * sizeCube);
                    Vector3 vertice2 = new Vector3(i * sizeCube, j * sizeCube, k * sizeCube);
                    Vector3 vertice3 = new Vector3(i * sizeCube, j * sizeCube, k * sizeCube + sizeCube);
                    Vector3 vertice4 = new Vector3(i * sizeCube + sizeCube, j * sizeCube, k * sizeCube + sizeCube);
                    Vector3 vertice5 = new Vector3(i * sizeCube + sizeCube, j * sizeCube + sizeCube, k * sizeCube);
                    Vector3 vertice6 = new Vector3(i * sizeCube, j * sizeCube + sizeCube, k * sizeCube);
                    Vector3 vertice7 = new Vector3(i * sizeCube, j * sizeCube + sizeCube, k * sizeCube + sizeCube);
                    Vector3 vertice8 = new Vector3(i * sizeCube + sizeCube, j * sizeCube + sizeCube, k * sizeCube + sizeCube);
                    if (Vector3.Distance(vertice1, impact) < radiusImpact) {
                        impacted[i, j, k, 0] = true;
                    }
                    if (Vector3.Distance(vertice2, impact) < radiusImpact) {
                        impacted[i, j, k, 1] = true;
                    }
                    if (Vector3.Distance(vertice3, impact) < radiusImpact) {
                        impacted[i, j, k, 2] = true;
                    }
                    if (Vector3.Distance(vertice4, impact) < radiusImpact) {
                        impacted[i, j, k, 3] = true;
                    }
                    if (Vector3.Distance(vertice5, impact) < radiusImpact) {
                        impacted[i, j, k, 4] = true;
                    }
                    if (Vector3.Distance(vertice6, impact) < radiusImpact) {
                        impacted[i, j, k, 5] = true;
                    }
                    if (Vector3.Distance(vertice7, impact) < radiusImpact) {
                        impacted[i, j, k, 6] = true;
                    }
                    if (Vector3.Distance(vertice8, impact) < radiusImpact) {
                        impacted[i, j, k, 7] = true;
                    }
                }
            }
        }


        mesh = GetComponent<MeshFilter>().mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
        vertices = new List<Vector3>();
        triangles = new List<int>();

        cubes = new Cube[sizeMaxCube, sizeMaxCube, sizeMaxCube];
        //impacted = new bool[sizeMaxCube, sizeMaxCube, sizeMaxCube, 8];


        for (int k = 0; k < sizeMaxCube; k++) {
            for (int j = 0; j < sizeMaxCube; j++) {
                for (int i = 0; i < sizeMaxCube; i++) {
                    Cube cube = cubes[i, j, k];
                    cube.index = i + sizeMaxCube * j + sizeMaxCube * sizeMaxCube * k;
                    cube.combination = 0; 
                    Vector3 vertice1 = new Vector3(i * sizeCube + sizeCube, j * sizeCube, k * sizeCube);
                    Vector3 vertice2 = new Vector3(i * sizeCube, j * sizeCube, k * sizeCube);
                    Vector3 vertice3 = new Vector3(i * sizeCube, j * sizeCube, k * sizeCube + sizeCube);
                    Vector3 vertice4 = new Vector3(i * sizeCube + sizeCube, j * sizeCube, k * sizeCube + sizeCube);
                    Vector3 vertice5 = new Vector3(i * sizeCube + sizeCube, j * sizeCube + sizeCube, k * sizeCube);
                    Vector3 vertice6 = new Vector3(i * sizeCube, j * sizeCube + sizeCube, k * sizeCube);
                    Vector3 vertice7 = new Vector3(i * sizeCube, j * sizeCube + sizeCube, k * sizeCube + sizeCube);
                    Vector3 vertice8 = new Vector3(i * sizeCube + sizeCube, j * sizeCube + sizeCube, k * sizeCube + sizeCube);

                    if (Vector3.Distance(vertice1, centerSphere) < radiusSphere && !impacted[i, j, k, 0]) {
                        cube.combination |= (1 << 0);
                    }
                    if (Vector3.Distance(vertice2, centerSphere) < radiusSphere && !impacted[i, j, k, 1]) {
                        cube.combination |= (1 << 1);

                    }
                    if (Vector3.Distance(vertice3, centerSphere) < radiusSphere && !impacted[i, j, k, 2]) {
                        cube.combination |= (1 << 2);

                    }
                    if (Vector3.Distance(vertice4, centerSphere) < radiusSphere && !impacted[i, j, k, 3]) {
                        cube.combination |= (1 << 3);

                    }
                    if (Vector3.Distance(vertice5, centerSphere) < radiusSphere && !impacted[i, j, k, 4]) {
                        cube.combination |= (1 << 4);

                    }
                    if (Vector3.Distance(vertice6, centerSphere) < radiusSphere && !impacted[i, j, k, 5]) {
                        cube.combination |= (1 << 5);

                    }
                    if (Vector3.Distance(vertice7, centerSphere) < radiusSphere && !impacted[i, j, k, 6]) {
                        cube.combination |= (1 << 6);

                    }
                    if (Vector3.Distance(vertice8, centerSphere) < radiusSphere && !impacted[i, j, k, 7]) {
                        cube.combination |= (1 << 7);

                    }

                    int edgeMask = edgeTable[cube.combination];

                    if (edgeMask == 0) {
                        continue;
                    }

                    int[] edges = new int[12];

					for (int edgeIndex = 0; edgeIndex < 12; edgeIndex++) {
						if ((edgeMask & (1 << edgeIndex)) == 0) {
							continue;
						}

						int verticeStartIndex = relationEdgeToVertices[edgeIndex, 0];
						int verticeEndIndex = relationEdgeToVertices[edgeIndex, 1];
						Vector3 p0 = verticesCubeRelativePositions[verticeStartIndex];
						Vector3 p1 = verticesCubeRelativePositions[verticeEndIndex];

                        Vector3 midPoint = ((p1 - p0) * 0.5f) + p0;

                        midPoint *= sizeCube;
                        midPoint += new Vector3(i * sizeCube, j * sizeCube, k * sizeCube);

						edges[edgeIndex] = vertices.Count;
						vertices.Add(midPoint);

                    }

                    int[] combination = combinations[cube.combination];

                    /*for (int n = 0; n < combination.Length; n++) {
                        triangles.Add(edges[combination[n]]);
                    }*/
                    for (int n = 0; n < combination.Length; n+=3) {
                        triangles.Add(edges[combination[n + 2]]);
                        triangles.Add(edges[combination[n + 1]]);
                        triangles.Add(edges[combination[n]]);
                    }
                }
            }
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();


        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        GetComponent<MeshCollider>().sharedMesh = null;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    void Awake () {
		combinations = new List<int[]>();
		combinations.Add(new int[]{});
		combinations.Add(new int[]{0, 8, 3});
		combinations.Add(new int[]{0, 1, 9});
		combinations.Add(new int[]{1, 8, 3, 9, 8, 1});
		combinations.Add(new int[]{1, 2, 10});
		combinations.Add(new int[]{0, 8, 3, 1, 2, 10});
		combinations.Add(new int[]{9, 2, 10, 0, 2, 9});
		combinations.Add(new int[]{2, 8, 3, 2, 10, 8, 10, 9, 8});
		combinations.Add(new int[]{3, 11, 2});
		combinations.Add(new int[]{0, 11, 2, 8, 11, 0});
		combinations.Add(new int[]{1, 9, 0, 2, 3, 11});
		combinations.Add(new int[]{1, 11, 2, 1, 9, 11, 9, 8, 11});
		combinations.Add(new int[]{3, 10, 1, 11, 10, 3});
		combinations.Add(new int[]{0, 10, 1, 0, 8, 10, 8, 11, 10});
		combinations.Add(new int[]{3, 9, 0, 3, 11, 9, 11, 10, 9});
		combinations.Add(new int[]{9, 8, 10, 10, 8, 11});
		combinations.Add(new int[]{4, 7, 8});
		combinations.Add(new int[]{4, 3, 0, 7, 3, 4});
		combinations.Add(new int[]{0, 1, 9, 8, 4, 7});
		combinations.Add(new int[]{4, 1, 9, 4, 7, 1, 7, 3, 1});
		combinations.Add(new int[]{1, 2, 10, 8, 4, 7});
		combinations.Add(new int[]{3, 4, 7, 3, 0, 4, 1, 2, 10});
		combinations.Add(new int[]{9, 2, 10, 9, 0, 2, 8, 4, 7});
		combinations.Add(new int[]{2, 10, 9, 2, 9, 7, 2, 7, 3, 7, 9, 4});
		combinations.Add(new int[]{8, 4, 7, 3, 11, 2});
		combinations.Add(new int[]{11, 4, 7, 11, 2, 4, 2, 0, 4});
		combinations.Add(new int[]{9, 0, 1, 8, 4, 7, 2, 3, 11});
		combinations.Add(new int[]{4, 7, 11, 9, 4, 11, 9, 11, 2, 9, 2, 1});
		combinations.Add(new int[]{3, 10, 1, 3, 11, 10, 7, 8, 4});
		combinations.Add(new int[]{1, 11, 10, 1, 4, 11, 1, 0, 4, 7, 11, 4});
		combinations.Add(new int[]{4, 7, 8, 9, 0, 11, 9, 11, 10, 11, 0, 3});
		combinations.Add(new int[]{4, 7, 11, 4, 11, 9, 9, 11, 10});
		combinations.Add(new int[]{9, 5, 4});
		combinations.Add(new int[]{9, 5, 4, 0, 8, 3});
		combinations.Add(new int[]{0, 5, 4, 1, 5, 0});
		combinations.Add(new int[]{8, 5, 4, 8, 3, 5, 3, 1, 5});
		combinations.Add(new int[]{1, 2, 10, 9, 5, 4});
		combinations.Add(new int[]{3, 0, 8, 1, 2, 10, 4, 9, 5});
		combinations.Add(new int[]{5, 2, 10, 5, 4, 2, 4, 0, 2});
		combinations.Add(new int[]{2, 10, 5, 3, 2, 5, 3, 5, 4, 3, 4, 8});
		combinations.Add(new int[]{9, 5, 4, 2, 3, 11});
		combinations.Add(new int[]{0, 11, 2, 0, 8, 11, 4, 9, 5});
		combinations.Add(new int[]{0, 5, 4, 0, 1, 5, 2, 3, 11});
		combinations.Add(new int[]{2, 1, 5, 2, 5, 8, 2, 8, 11, 4, 8, 5});
		combinations.Add(new int[]{10, 3, 11, 10, 1, 3, 9, 5, 4});
		combinations.Add(new int[]{4, 9, 5, 0, 8, 1, 8, 10, 1, 8, 11, 10});
		combinations.Add(new int[]{5, 4, 0, 5, 0, 11, 5, 11, 10, 11, 0, 3});
		combinations.Add(new int[]{5, 4, 8, 5, 8, 10, 10, 8, 11});
		combinations.Add(new int[]{9, 7, 8, 5, 7, 9});
		combinations.Add(new int[]{9, 3, 0, 9, 5, 3, 5, 7, 3});
		combinations.Add(new int[]{0, 7, 8, 0, 1, 7, 1, 5, 7});
		combinations.Add(new int[]{1, 5, 3, 3, 5, 7});
		combinations.Add(new int[]{9, 7, 8, 9, 5, 7, 10, 1, 2});
		combinations.Add(new int[]{10, 1, 2, 9, 5, 0, 5, 3, 0, 5, 7, 3});
		combinations.Add(new int[]{8, 0, 2, 8, 2, 5, 8, 5, 7, 10, 5, 2});
		combinations.Add(new int[]{2, 10, 5, 2, 5, 3, 3, 5, 7});
		combinations.Add(new int[]{7, 9, 5, 7, 8, 9, 3, 11, 2});
		combinations.Add(new int[]{9, 5, 7, 9, 7, 2, 9, 2, 0, 2, 7, 11});
		combinations.Add(new int[]{2, 3, 11, 0, 1, 8, 1, 7, 8, 1, 5, 7});
		combinations.Add(new int[]{11, 2, 1, 11, 1, 7, 7, 1, 5});
		combinations.Add(new int[]{9, 5, 8, 8, 5, 7, 10, 1, 3, 10, 3, 11});
		combinations.Add(new int[]{5, 7, 0, 5, 0, 9, 7, 11, 0, 1, 0, 10, 11, 10, 0});
		combinations.Add(new int[]{11, 10, 0, 11, 0, 3, 10, 5, 0, 8, 0, 7, 5, 7, 0});
		combinations.Add(new int[]{11, 10, 5, 7, 11, 5});
		combinations.Add(new int[]{10, 6, 5});
		combinations.Add(new int[]{0, 8, 3, 5, 10, 6});
		combinations.Add(new int[]{9, 0, 1, 5, 10, 6});
		combinations.Add(new int[]{1, 8, 3, 1, 9, 8, 5, 10, 6});
		combinations.Add(new int[]{1, 6, 5, 2, 6, 1});
		combinations.Add(new int[]{1, 6, 5, 1, 2, 6, 3, 0, 8});
		combinations.Add(new int[]{9, 6, 5, 9, 0, 6, 0, 2, 6});
		combinations.Add(new int[]{5, 9, 8, 5, 8, 2, 5, 2, 6, 3, 2, 8});
		combinations.Add(new int[]{2, 3, 11, 10, 6, 5});
		combinations.Add(new int[]{11, 0, 8, 11, 2, 0, 10, 6, 5});
		combinations.Add(new int[]{0, 1, 9, 2, 3, 11, 5, 10, 6});
		combinations.Add(new int[]{5, 10, 6, 1, 9, 2, 9, 11, 2, 9, 8, 11});
		combinations.Add(new int[]{6, 3, 11, 6, 5, 3, 5, 1, 3});
		combinations.Add(new int[]{0, 8, 11, 0, 11, 5, 0, 5, 1, 5, 11, 6});
		combinations.Add(new int[]{3, 11, 6, 0, 3, 6, 0, 6, 5, 0, 5, 9});
		combinations.Add(new int[]{6, 5, 9, 6, 9, 11, 11, 9, 8});
		combinations.Add(new int[]{5, 10, 6, 4, 7, 8});
		combinations.Add(new int[]{4, 3, 0, 4, 7, 3, 6, 5, 10});
		combinations.Add(new int[]{1, 9, 0, 5, 10, 6, 8, 4, 7});
		combinations.Add(new int[]{10, 6, 5, 1, 9, 7, 1, 7, 3, 7, 9, 4});
		combinations.Add(new int[]{6, 1, 2, 6, 5, 1, 4, 7, 8});
		combinations.Add(new int[]{1, 2, 5, 5, 2, 6, 3, 0, 4, 3, 4, 7});
		combinations.Add(new int[]{8, 4, 7, 9, 0, 5, 0, 6, 5, 0, 2, 6});
		combinations.Add(new int[]{7, 3, 9, 7, 9, 4, 3, 2, 9, 5, 9, 6, 2, 6, 9});
		combinations.Add(new int[]{3, 11, 2, 7, 8, 4, 10, 6, 5});
		combinations.Add(new int[]{5, 10, 6, 4, 7, 2, 4, 2, 0, 2, 7, 11});
		combinations.Add(new int[]{0, 1, 9, 4, 7, 8, 2, 3, 11, 5, 10, 6});
		combinations.Add(new int[]{9, 2, 1, 9, 11, 2, 9, 4, 11, 7, 11, 4, 5, 10, 6});
		combinations.Add(new int[]{8, 4, 7, 3, 11, 5, 3, 5, 1, 5, 11, 6});
		combinations.Add(new int[]{5, 1, 11, 5, 11, 6, 1, 0, 11, 7, 11, 4, 0, 4, 11});
		combinations.Add(new int[]{0, 5, 9, 0, 6, 5, 0, 3, 6, 11, 6, 3, 8, 4, 7});
		combinations.Add(new int[]{6, 5, 9, 6, 9, 11, 4, 7, 9, 7, 11, 9});
		combinations.Add(new int[]{10, 4, 9, 6, 4, 10});
		combinations.Add(new int[]{4, 10, 6, 4, 9, 10, 0, 8, 3});
		combinations.Add(new int[]{10, 0, 1, 10, 6, 0, 6, 4, 0});
		combinations.Add(new int[]{8, 3, 1, 8, 1, 6, 8, 6, 4, 6, 1, 10});
		combinations.Add(new int[]{1, 4, 9, 1, 2, 4, 2, 6, 4});
		combinations.Add(new int[]{3, 0, 8, 1, 2, 9, 2, 4, 9, 2, 6, 4});
		combinations.Add(new int[]{0, 2, 4, 4, 2, 6});
		combinations.Add(new int[]{8, 3, 2, 8, 2, 4, 4, 2, 6});
		combinations.Add(new int[]{10, 4, 9, 10, 6, 4, 11, 2, 3});
		combinations.Add(new int[]{0, 8, 2, 2, 8, 11, 4, 9, 10, 4, 10, 6});
		combinations.Add(new int[]{3, 11, 2, 0, 1, 6, 0, 6, 4, 6, 1, 10});
		combinations.Add(new int[]{6, 4, 1, 6, 1, 10, 4, 8, 1, 2, 1, 11, 8, 11, 1});
		combinations.Add(new int[]{9, 6, 4, 9, 3, 6, 9, 1, 3, 11, 6, 3});
		combinations.Add(new int[]{8, 11, 1, 8, 1, 0, 11, 6, 1, 9, 1, 4, 6, 4, 1});
		combinations.Add(new int[]{3, 11, 6, 3, 6, 0, 0, 6, 4});
		combinations.Add(new int[]{6, 4, 8, 11, 6, 8});
		combinations.Add(new int[]{7, 10, 6, 7, 8, 10, 8, 9, 10});
		combinations.Add(new int[]{0, 7, 3, 0, 10, 7, 0, 9, 10, 6, 7, 10});
		combinations.Add(new int[]{10, 6, 7, 1, 10, 7, 1, 7, 8, 1, 8, 0});
		combinations.Add(new int[]{10, 6, 7, 10, 7, 1, 1, 7, 3});
		combinations.Add(new int[]{1, 2, 6, 1, 6, 8, 1, 8, 9, 8, 6, 7});
		combinations.Add(new int[]{2, 6, 9, 2, 9, 1, 6, 7, 9, 0, 9, 3, 7, 3, 9});
		combinations.Add(new int[]{7, 8, 0, 7, 0, 6, 6, 0, 2});
		combinations.Add(new int[]{7, 3, 2, 6, 7, 2});
		combinations.Add(new int[]{2, 3, 11, 10, 6, 8, 10, 8, 9, 8, 6, 7});
		combinations.Add(new int[]{2, 0, 7, 2, 7, 11, 0, 9, 7, 6, 7, 10, 9, 10, 7});
		combinations.Add(new int[]{1, 8, 0, 1, 7, 8, 1, 10, 7, 6, 7, 10, 2, 3, 11});
		combinations.Add(new int[]{11, 2, 1, 11, 1, 7, 10, 6, 1, 6, 7, 1});
		combinations.Add(new int[]{8, 9, 6, 8, 6, 7, 9, 1, 6, 11, 6, 3, 1, 3, 6});
		combinations.Add(new int[]{0, 9, 1, 11, 6, 7});
		combinations.Add(new int[]{7, 8, 0, 7, 0, 6, 3, 11, 0, 11, 6, 0});
		combinations.Add(new int[]{7, 11, 6});
		combinations.Add(new int[]{7, 6, 11});
		combinations.Add(new int[]{3, 0, 8, 11, 7, 6});
		combinations.Add(new int[]{0, 1, 9, 11, 7, 6});
		combinations.Add(new int[]{8, 1, 9, 8, 3, 1, 11, 7, 6});
		combinations.Add(new int[]{10, 1, 2, 6, 11, 7});
		combinations.Add(new int[]{1, 2, 10, 3, 0, 8, 6, 11, 7});
		combinations.Add(new int[]{2, 9, 0, 2, 10, 9, 6, 11, 7});
		combinations.Add(new int[]{6, 11, 7, 2, 10, 3, 10, 8, 3, 10, 9, 8});
		combinations.Add(new int[]{7, 2, 3, 6, 2, 7});
		combinations.Add(new int[]{7, 0, 8, 7, 6, 0, 6, 2, 0});
		combinations.Add(new int[]{2, 7, 6, 2, 3, 7, 0, 1, 9});
		combinations.Add(new int[]{1, 6, 2, 1, 8, 6, 1, 9, 8, 8, 7, 6});
		combinations.Add(new int[]{10, 7, 6, 10, 1, 7, 1, 3, 7});
		combinations.Add(new int[]{10, 7, 6, 1, 7, 10, 1, 8, 7, 1, 0, 8});
		combinations.Add(new int[]{0, 3, 7, 0, 7, 10, 0, 10, 9, 6, 10, 7});
		combinations.Add(new int[]{7, 6, 10, 7, 10, 8, 8, 10, 9});
		combinations.Add(new int[]{6, 8, 4, 11, 8, 6});
		combinations.Add(new int[]{3, 6, 11, 3, 0, 6, 0, 4, 6});
		combinations.Add(new int[]{8, 6, 11, 8, 4, 6, 9, 0, 1});
		combinations.Add(new int[]{9, 4, 6, 9, 6, 3, 9, 3, 1, 11, 3, 6});
		combinations.Add(new int[]{6, 8, 4, 6, 11, 8, 2, 10, 1});
		combinations.Add(new int[]{1, 2, 10, 3, 0, 11, 0, 6, 11, 0, 4, 6});
		combinations.Add(new int[]{4, 11, 8, 4, 6, 11, 0, 2, 9, 2, 10, 9});
		combinations.Add(new int[]{10, 9, 3, 10, 3, 2, 9, 4, 3, 11, 3, 6, 4, 6, 3});
		combinations.Add(new int[]{8, 2, 3, 8, 4, 2, 4, 6, 2});
		combinations.Add(new int[]{0, 4, 2, 4, 6, 2});
		combinations.Add(new int[]{1, 9, 0, 2, 3, 4, 2, 4, 6, 4, 3, 8});
		combinations.Add(new int[]{1, 9, 4, 1, 4, 2, 2, 4, 6});
		combinations.Add(new int[]{8, 1, 3, 8, 6, 1, 8, 4, 6, 6, 10, 1});
		combinations.Add(new int[]{10, 1, 0, 10, 0, 6, 6, 0, 4});
		combinations.Add(new int[]{4, 6, 3, 4, 3, 8, 6, 10, 3, 0, 3, 9, 10, 9, 3});
		combinations.Add(new int[]{10, 9, 4, 6, 10, 4});
		combinations.Add(new int[]{4, 9, 5, 7, 6, 11});
		combinations.Add(new int[]{0, 8, 3, 4, 9, 5, 11, 7, 6});
		combinations.Add(new int[]{5, 0, 1, 5, 4, 0, 7, 6, 11});
		combinations.Add(new int[]{11, 7, 6, 8, 3, 4, 3, 5, 4, 3, 1, 5});
		combinations.Add(new int[]{9, 5, 4, 10, 1, 2, 7, 6, 11});
		combinations.Add(new int[]{6, 11, 7, 1, 2, 10, 0, 8, 3, 4, 9, 5});
		combinations.Add(new int[]{7, 6, 11, 5, 4, 10, 4, 2, 10, 4, 0, 2});
		combinations.Add(new int[]{3, 4, 8, 3, 5, 4, 3, 2, 5, 10, 5, 2, 11, 7, 6});
		combinations.Add(new int[]{7, 2, 3, 7, 6, 2, 5, 4, 9});
		combinations.Add(new int[]{9, 5, 4, 0, 8, 6, 0, 6, 2, 6, 8, 7});
		combinations.Add(new int[]{3, 6, 2, 3, 7, 6, 1, 5, 0, 5, 4, 0});
		combinations.Add(new int[]{6, 2, 8, 6, 8, 7, 2, 1, 8, 4, 8, 5, 1, 5, 8});
		combinations.Add(new int[]{9, 5, 4, 10, 1, 6, 1, 7, 6, 1, 3, 7});
		combinations.Add(new int[]{1, 6, 10, 1, 7, 6, 1, 0, 7, 8, 7, 0, 9, 5, 4});
		combinations.Add(new int[]{4, 0, 10, 4, 10, 5, 0, 3, 10, 6, 10, 7, 3, 7, 10});
		combinations.Add(new int[]{7, 6, 10, 7, 10, 8, 5, 4, 10, 4, 8, 10});
		combinations.Add(new int[]{6, 9, 5, 6, 11, 9, 11, 8, 9});
		combinations.Add(new int[]{3, 6, 11, 0, 6, 3, 0, 5, 6, 0, 9, 5});
		combinations.Add(new int[]{0, 11, 8, 0, 5, 11, 0, 1, 5, 5, 6, 11});
		combinations.Add(new int[]{6, 11, 3, 6, 3, 5, 5, 3, 1});
		combinations.Add(new int[]{1, 2, 10, 9, 5, 11, 9, 11, 8, 11, 5, 6});
		combinations.Add(new int[]{0, 11, 3, 0, 6, 11, 0, 9, 6, 5, 6, 9, 1, 2, 10});
		combinations.Add(new int[]{11, 8, 5, 11, 5, 6, 8, 0, 5, 10, 5, 2, 0, 2, 5});
		combinations.Add(new int[]{6, 11, 3, 6, 3, 5, 2, 10, 3, 10, 5, 3});
		combinations.Add(new int[]{5, 8, 9, 5, 2, 8, 5, 6, 2, 3, 8, 2});
		combinations.Add(new int[]{9, 5, 6, 9, 6, 0, 0, 6, 2});
		combinations.Add(new int[]{1, 5, 8, 1, 8, 0, 5, 6, 8, 3, 8, 2, 6, 2, 8});
		combinations.Add(new int[]{1, 5, 6, 2, 1, 6});
		combinations.Add(new int[]{1, 3, 6, 1, 6, 10, 3, 8, 6, 5, 6, 9, 8, 9, 6});
		combinations.Add(new int[]{10, 1, 0, 10, 0, 6, 9, 5, 0, 5, 6, 0});
		combinations.Add(new int[]{0, 3, 8, 5, 6, 10});
		combinations.Add(new int[]{10, 5, 6});
		combinations.Add(new int[]{11, 5, 10, 7, 5, 11});
		combinations.Add(new int[]{11, 5, 10, 11, 7, 5, 8, 3, 0});
		combinations.Add(new int[]{5, 11, 7, 5, 10, 11, 1, 9, 0});
		combinations.Add(new int[]{10, 7, 5, 10, 11, 7, 9, 8, 1, 8, 3, 1});
		combinations.Add(new int[]{11, 1, 2, 11, 7, 1, 7, 5, 1});
		combinations.Add(new int[]{0, 8, 3, 1, 2, 7, 1, 7, 5, 7, 2, 11});
		combinations.Add(new int[]{9, 7, 5, 9, 2, 7, 9, 0, 2, 2, 11, 7});
		combinations.Add(new int[]{7, 5, 2, 7, 2, 11, 5, 9, 2, 3, 2, 8, 9, 8, 2});
		combinations.Add(new int[]{2, 5, 10, 2, 3, 5, 3, 7, 5});
		combinations.Add(new int[]{8, 2, 0, 8, 5, 2, 8, 7, 5, 10, 2, 5});
		combinations.Add(new int[]{9, 0, 1, 5, 10, 3, 5, 3, 7, 3, 10, 2});
		combinations.Add(new int[]{9, 8, 2, 9, 2, 1, 8, 7, 2, 10, 2, 5, 7, 5, 2});
		combinations.Add(new int[]{1, 3, 5, 3, 7, 5});
		combinations.Add(new int[]{0, 8, 7, 0, 7, 1, 1, 7, 5});
		combinations.Add(new int[]{9, 0, 3, 9, 3, 5, 5, 3, 7});
		combinations.Add(new int[]{9, 8, 7, 5, 9, 7});
		combinations.Add(new int[]{5, 8, 4, 5, 10, 8, 10, 11, 8});
		combinations.Add(new int[]{5, 0, 4, 5, 11, 0, 5, 10, 11, 11, 3, 0});
		combinations.Add(new int[]{0, 1, 9, 8, 4, 10, 8, 10, 11, 10, 4, 5});
		combinations.Add(new int[]{10, 11, 4, 10, 4, 5, 11, 3, 4, 9, 4, 1, 3, 1, 4});
		combinations.Add(new int[]{2, 5, 1, 2, 8, 5, 2, 11, 8, 4, 5, 8});
		combinations.Add(new int[]{0, 4, 11, 0, 11, 3, 4, 5, 11, 2, 11, 1, 5, 1, 11});
		combinations.Add(new int[]{0, 2, 5, 0, 5, 9, 2, 11, 5, 4, 5, 8, 11, 8, 5});
		combinations.Add(new int[]{9, 4, 5, 2, 11, 3});
		combinations.Add(new int[]{2, 5, 10, 3, 5, 2, 3, 4, 5, 3, 8, 4});
		combinations.Add(new int[]{5, 10, 2, 5, 2, 4, 4, 2, 0});
		combinations.Add(new int[]{3, 10, 2, 3, 5, 10, 3, 8, 5, 4, 5, 8, 0, 1, 9});
		combinations.Add(new int[]{5, 10, 2, 5, 2, 4, 1, 9, 2, 9, 4, 2});
		combinations.Add(new int[]{8, 4, 5, 8, 5, 3, 3, 5, 1});
		combinations.Add(new int[]{0, 4, 5, 1, 0, 5});
		combinations.Add(new int[]{8, 4, 5, 8, 5, 3, 9, 0, 5, 0, 3, 5});
		combinations.Add(new int[]{9, 4, 5});
		combinations.Add(new int[]{4, 11, 7, 4, 9, 11, 9, 10, 11});
		combinations.Add(new int[]{0, 8, 3, 4, 9, 7, 9, 11, 7, 9, 10, 11});
		combinations.Add(new int[]{1, 10, 11, 1, 11, 4, 1, 4, 0, 7, 4, 11});
		combinations.Add(new int[]{3, 1, 4, 3, 4, 8, 1, 10, 4, 7, 4, 11, 10, 11, 4});
		combinations.Add(new int[]{4, 11, 7, 9, 11, 4, 9, 2, 11, 9, 1, 2});
		combinations.Add(new int[]{9, 7, 4, 9, 11, 7, 9, 1, 11, 2, 11, 1, 0, 8, 3});
		combinations.Add(new int[]{11, 7, 4, 11, 4, 2, 2, 4, 0});
		combinations.Add(new int[]{11, 7, 4, 11, 4, 2, 8, 3, 4, 3, 2, 4});
		combinations.Add(new int[]{2, 9, 10, 2, 7, 9, 2, 3, 7, 7, 4, 9});
		combinations.Add(new int[]{9, 10, 7, 9, 7, 4, 10, 2, 7, 8, 7, 0, 2, 0, 7});
		combinations.Add(new int[]{3, 7, 10, 3, 10, 2, 7, 4, 10, 1, 10, 0, 4, 0, 10});
		combinations.Add(new int[]{1, 10, 2, 8, 7, 4});
		combinations.Add(new int[]{4, 9, 1, 4, 1, 7, 7, 1, 3});
		combinations.Add(new int[]{4, 9, 1, 4, 1, 7, 0, 8, 1, 8, 7, 1});
		combinations.Add(new int[]{4, 0, 3, 7, 4, 3});
		combinations.Add(new int[]{4, 8, 7});
		combinations.Add(new int[]{9, 10, 8, 10, 11, 8});
		combinations.Add(new int[]{3, 0, 9, 3, 9, 11, 11, 9, 10});
		combinations.Add(new int[]{0, 1, 10, 0, 10, 8, 8, 10, 11});
		combinations.Add(new int[]{3, 1, 10, 11, 3, 10});
		combinations.Add(new int[]{1, 2, 11, 1, 11, 9, 9, 11, 8});
		combinations.Add(new int[]{3, 0, 9, 3, 9, 11, 1, 2, 9, 2, 11, 9});
		combinations.Add(new int[]{0, 2, 11, 8, 0, 11});
		combinations.Add(new int[]{3, 2, 11});
		combinations.Add(new int[]{2, 3, 8, 2, 8, 10, 10, 8, 9});
		combinations.Add(new int[]{9, 10, 2, 0, 9, 2});
		combinations.Add(new int[]{2, 3, 8, 2, 8, 10, 0, 1, 8, 1, 10, 8});
		combinations.Add(new int[]{1, 10, 2});
		combinations.Add(new int[]{1, 3, 8, 9, 1, 8});
		combinations.Add(new int[]{0, 9, 1});
		combinations.Add(new int[]{0, 3, 8});
		combinations.Add(new int[]{});

        edgeTable = new int[]{
            0x0  , 0x109, 0x203, 0x30a, 0x406, 0x50f, 0x605, 0x70c,
            0x80c, 0x905, 0xa0f, 0xb06, 0xc0a, 0xd03, 0xe09, 0xf00,
            0x190, 0x99 , 0x393, 0x29a, 0x596, 0x49f, 0x795, 0x69c,
            0x99c, 0x895, 0xb9f, 0xa96, 0xd9a, 0xc93, 0xf99, 0xe90,
            0x230, 0x339, 0x33 , 0x13a, 0x636, 0x73f, 0x435, 0x53c,
            0xa3c, 0xb35, 0x83f, 0x936, 0xe3a, 0xf33, 0xc39, 0xd30,
            0x3a0, 0x2a9, 0x1a3, 0xaa , 0x7a6, 0x6af, 0x5a5, 0x4ac,
            0xbac, 0xaa5, 0x9af, 0x8a6, 0xfaa, 0xea3, 0xda9, 0xca0,
            0x460, 0x569, 0x663, 0x76a, 0x66 , 0x16f, 0x265, 0x36c,
            0xc6c, 0xd65, 0xe6f, 0xf66, 0x86a, 0x963, 0xa69, 0xb60,
            0x5f0, 0x4f9, 0x7f3, 0x6fa, 0x1f6, 0xff , 0x3f5, 0x2fc,
            0xdfc, 0xcf5, 0xfff, 0xef6, 0x9fa, 0x8f3, 0xbf9, 0xaf0,
            0x650, 0x759, 0x453, 0x55a, 0x256, 0x35f, 0x55 , 0x15c,
            0xe5c, 0xf55, 0xc5f, 0xd56, 0xa5a, 0xb53, 0x859, 0x950,
            0x7c0, 0x6c9, 0x5c3, 0x4ca, 0x3c6, 0x2cf, 0x1c5, 0xcc ,
            0xfcc, 0xec5, 0xdcf, 0xcc6, 0xbca, 0xac3, 0x9c9, 0x8c0,
            0x8c0, 0x9c9, 0xac3, 0xbca, 0xcc6, 0xdcf, 0xec5, 0xfcc,
            0xcc , 0x1c5, 0x2cf, 0x3c6, 0x4ca, 0x5c3, 0x6c9, 0x7c0,
            0x950, 0x859, 0xb53, 0xa5a, 0xd56, 0xc5f, 0xf55, 0xe5c,
            0x15c, 0x55 , 0x35f, 0x256, 0x55a, 0x453, 0x759, 0x650,
            0xaf0, 0xbf9, 0x8f3, 0x9fa, 0xef6, 0xfff, 0xcf5, 0xdfc,
            0x2fc, 0x3f5, 0xff , 0x1f6, 0x6fa, 0x7f3, 0x4f9, 0x5f0,
            0xb60, 0xa69, 0x963, 0x86a, 0xf66, 0xe6f, 0xd65, 0xc6c,
            0x36c, 0x265, 0x16f, 0x66 , 0x76a, 0x663, 0x569, 0x460,
            0xca0, 0xda9, 0xea3, 0xfaa, 0x8a6, 0x9af, 0xaa5, 0xbac,
            0x4ac, 0x5a5, 0x6af, 0x7a6, 0xaa , 0x1a3, 0x2a9, 0x3a0,
            0xd30, 0xc39, 0xf33, 0xe3a, 0x936, 0x83f, 0xb35, 0xa3c,
            0x53c, 0x435, 0x73f, 0x636, 0x13a, 0x33 , 0x339, 0x230,
            0xe90, 0xf99, 0xc93, 0xd9a, 0xa96, 0xb9f, 0x895, 0x99c,
            0x69c, 0x795, 0x49f, 0x596, 0x29a, 0x393, 0x99 , 0x190,
            0xf00, 0xe09, 0xd03, 0xc0a, 0xb06, 0xa0f, 0x905, 0x80c,
            0x70c, 0x605, 0x50f, 0x406, 0x30a, 0x203, 0x109, 0x0
	    };
        relationEdgeToVertices = new int[,]{
            {0, 1},
            {1, 2},
            {2, 3},
            {3, 0},
            {4, 5},
            {5, 6},
            {6, 7},
            {7, 4},
            {0, 4},
            {1, 5},
            {2, 6},
            {3, 7},
        };
        verticesCubeRelativePositions = new Vector3[]{
            new Vector3(1,0,0),
            new Vector3(0,0,0),
            new Vector3(0,0,1),
            new Vector3(1,0,1),
            new Vector3(1,1,0),
            new Vector3(0,1,0),
            new Vector3(0,1,1),
            new Vector3(1,1,1)
        };
    }
}