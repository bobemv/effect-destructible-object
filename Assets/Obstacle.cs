using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObstacleType{ BigLeft, BigRight, BigDown, BigUp, SmallRandom, BigAll };
public class Obstacle : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private float _speed;
    public bool isInUse = false;

    private float rightMax = 9f;
    private float leftMax = -9f;
    private float upMax = 12f;
    private float downMax = 4f;

    private float sizeCube = 1f;

    private float posZ = 60;
    private Vector3 sizesMaxCube;
    public void CreateObstacle(ObstacleType type) {
        isInUse = true;
        MarchingCubes marchingCubes;
        if(!transform.TryGetComponent<MarchingCubes>(out marchingCubes)) {
            Debug.Log("Not added Marching Cubes component to Obstacle");
            return;
        }
        SetSizeByObstacleType(type);
        marchingCubes.CreateModel(sizesMaxCube, sizeCube);
    }

    // Update is called once per frame
    void Update()
    {
        
        transform.Translate(Vector3.back * _speed * Time.deltaTime);
        if (transform.position.z < -1000) {
            Destroy(gameObject);
        }
    }

    void SetSizeByObstacleType(ObstacleType type) {
        switch(type) {
            case ObstacleType.BigLeft:
                sizesMaxCube = new Vector3(Mathf.Abs(leftMax - rightMax) / 2 + 2, Mathf.Abs(upMax - downMax) + 2, 2);
                transform.position = new Vector3(leftMax - 1, downMax - 1, posZ);
            break;
            case ObstacleType.BigRight:
                sizesMaxCube = new Vector3(Mathf.Abs(leftMax - rightMax) / 2 + 2, Mathf.Abs(upMax - downMax) + 2, 2);
                transform.position = new Vector3(Mathf.Abs(leftMax - rightMax) / 2 + leftMax, downMax - 1, posZ);
            break;
            case ObstacleType.BigUp:
                sizesMaxCube = new Vector3(Mathf.Abs(leftMax - rightMax) + 2, Mathf.Abs(upMax - downMax) / 2 + 2, 2);
                transform.position = new Vector3(leftMax - 1, (upMax - downMax) / 2 + downMax, posZ);
            break;
            case ObstacleType.BigDown:
                sizesMaxCube = new Vector3(Mathf.Abs(leftMax - rightMax) + 2, Mathf.Abs(upMax - downMax) / 2 + 2, 2);
                transform.position = new Vector3(leftMax - 1, downMax - 1, posZ);
            break;
            case ObstacleType.SmallRandom:
                sizesMaxCube = new Vector3(4, 4, 4);
                transform.position = new Vector3(Random.Range(leftMax, rightMax), Random.Range(downMax, upMax), posZ);
            break;
            case ObstacleType.BigAll:
                sizesMaxCube = new Vector3((Mathf.Abs(leftMax - rightMax) + 2) / 2, (Mathf.Abs(upMax - downMax) + 2) / 2, 100);
                sizeCube = 2;
                transform.position = new Vector3(leftMax - 1, downMax - 1, posZ);
            break;
        }
    }
}
