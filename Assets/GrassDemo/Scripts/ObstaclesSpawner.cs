using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObstaclesSpawner : MonoBehaviour 
{
	[SerializeField]
	List<GameObject> _obstaclesPrefab;

	[SerializeField]
	float _spawnDistanceToCamera = 0f;

	[SerializeField]
	float _spawnObsacleForceMin = 5f;

	[SerializeField]
	float _spawnObsacleForceMax = 15f;

	GrassEngine _grassEngine;

	List<Obstacle> _obstacles = new List<Obstacle>();

	int kMaxObstaclesData = 128;

	GrassEngine.ObstacleData[] _obstaclesData;
	int _numObstacles = 0;

	void Awake()
	{
		_grassEngine = GameObject.FindObjectOfType<GrassEngine>();
	}

	void Start()
	{
		_obstaclesData = new GrassEngine.ObstacleData[kMaxObstaclesData];
		_numObstacles = 0;
	}

	void Update()
	{
		if(Input.GetMouseButtonUp(0))
		{
		 	Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			Vector3 rayDirXZ = new Vector3(ray.direction.x, 0, ray.direction.z).normalized;
			Vector3 pos = Camera.main.transform.position + rayDirXZ * _spawnDistanceToCamera;
			SpawnObstacle(pos, rayDirXZ);
		}

		SendObstaclesDataToGPU();
	}

	void SpawnObstacle(Vector3 position, Vector3 direction)
	{
		GameObject instance = GameObject.Instantiate(GetRandomPrefab(), position, Quaternion.identity) as GameObject;
		Obstacle obstacle = instance.GetComponentInChildren<Obstacle>();
		obstacle.Init();

		_obstacles.Add(obstacle);
		_obstaclesData[_numObstacles++] = new GrassEngine.ObstacleData(){ Position = position, Radius = obstacle.GetRadius(), ExpansiveForce = 0f };

		obstacle.Play(direction * Random.Range(_spawnObsacleForceMin, _spawnObsacleForceMax));
	}

	GameObject GetRandomPrefab()
	{
		int idx = Random.Range(0, _obstaclesPrefab.Count);
		return _obstaclesPrefab[idx];
	}

	void SendObstaclesDataToGPU()
	{
		for (int i = 0; i < _obstacles.Count; ++i) 
		{
			_obstaclesData[i].Position = _obstacles[i].transform.position;
			//_obstaclesData[i].Radius = _obstacles[i].GetRadius();
			_obstaclesData[i].ExpansiveForce = _obstacles[i].ExpansiveForce;
		}

		_grassEngine.SetObstaclesData(_obstaclesData);
	}
}
