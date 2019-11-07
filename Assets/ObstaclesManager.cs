using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObstaclesManager : MonoBehaviour
{
    [SerializeField] private GameObject _obstaclePrefab;
    [SerializeField] private float _spawnRate;

    private float rightMax = 9f;
    private float leftMax = -9f;
    private float upMax = 12f;
    private float downMax = 4f;
    private List<Obstacle> obstacles;
    // Start is called before the first frame update
    void Start()
    {
        obstacles = new List<Obstacle>();



        /*for (int i = 0; i < 100; i++) {
            Obstacle obstacle = Instantiate(_obstaclePrefab, Vector3.zero +  new Vector3(0, 50, 0), Quaternion.identity).GetComponent<Obstacle>();
            obstacles.Add(obstacle);
        }*/

        StartCoroutine(SpawnObstacles());
    }

    // Update is called once per frame
    void Update()
    {
        

    }

    IEnumerator SpawnObstacles() {
        while (true) {
            //Obstacle obstacle = obstacles.First(o => o.isInUse == false);
            //obstacle.transform.position = new Vector3(Random.Range(-9f, 9f), Random.Range(4f, 12f), 60);
            //obstacle.isInUse = true;
            Obstacle obstacle = Instantiate(_obstaclePrefab, Vector3.zero, Quaternion.identity).GetComponent<Obstacle>();
            obstacle.CreateObstacle((ObstacleType) Random.Range(0, 5));
            yield return new WaitForSeconds(_spawnRate);
        }
    }
}
