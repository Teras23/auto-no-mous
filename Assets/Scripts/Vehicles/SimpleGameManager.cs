using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class SimpleGameManager : MonoBehaviour {
	private GameObject[] _cars;
	private GameObject _playerCar;
	private bool _inGame;
	private bool started = false;
	private int generation = 1;

	[SerializeField]
	private GameObject _aiCarPrefab;

	[SerializeField]
	private GameObject _playerCarPrefab;

	[SerializeField]
	public int nrOfCars = 10;

	public static float maxRayLength = 10;

	/// <summary>
	/// Used by UI controller
	/// </summary>
	public bool InGame => _inGame;

	// Start is called before the first frame update
	void Start() {
		_cars = new GameObject[nrOfCars];
	}

	// Update is called once per frame
	void Update() {
		var allFinished = true;

		foreach (var car in _cars) {
			if (car != null && car.activeSelf) {
				allFinished = false;
			}
		}

		if (started && allFinished) {
			SpawnNewCars();
		}
	}


	/// <summary>
	/// Used by UI controller
	/// </summary>
	public void EnterPlayMode(bool includePlayer = false) {
		_inGame = true;

		if (includePlayer) {
			_playerCar = Instantiate(_playerCarPrefab);
		}

		ClearCars();
		SpawnCars();
	}


	/// <summary>
	/// Used by UI controller
	/// </summary>
	public void LeavePlayMode() {
		_inGame = false;
		started = false;
		ClearCars();
	}

	public void ClearCars() {
		if (_playerCar != null) {
			Destroy(_playerCar);
			_playerCar = null;
		}

		for (var i = 0; i < _cars.Length; i++) {
			if (_cars[i] != null) {
				Destroy(_cars[i]);
				_cars[i] = null;
			}
		}
	}

	private void SpawnCars() {
		for (var i = 0; i < nrOfCars; i++) {
			var car = Instantiate(_aiCarPrefab);
			_cars[i] = car;
		}

		started = true;
	}

	private const float Mutate = 0.3f;
	private const float Crossover = 0.4f;

	private GameObject SelectBiasBest(List<GameObject> cars, int pointsTotal)
	{
		var random = new Random();
		var randomPoints = random.Next(pointsTotal);

		var totalIterator = 0;
		
		foreach (var car in cars)
		{
			totalIterator += car.GetComponent<CarController>().points;			
			
			if (totalIterator < randomPoints)
			{
				return car;
			}
		}
		
		return cars.Last();
	}
	
	private void SpawnNewCars() {
		var lastCars = _cars.OrderByDescending(go => go.GetComponent<CarController>().points)
			.ThenBy(go => go.GetComponent<CarController>().TotalTime).ToList();
		ClearCars();
		SpawnCars();

		var pointsTotal = 0;

		foreach (var car in lastCars)
		{
			pointsTotal += car.GetComponent<CarController>().points;
		}
		
		// Mutate best cars
		for (var i = 0; i < (int) (nrOfCars * Mutate); i++) {
			_cars[i].GetComponent<NeuralNetwork>().SetNetwork(
				NeuralNetwork.Mutate(SelectBiasBest(lastCars, pointsTotal)));
			_cars[i].name = "AICar (Mutated)";
			_cars[i].GetComponentInChildren<MeshRenderer>().material.color = Color.green;
		}

		// Crossover best cars
		for (var i = (int) (nrOfCars * Mutate); i < (int) (nrOfCars * (Mutate + Crossover)); i++) {
			_cars[i].GetComponent<NeuralNetwork>().SetNetwork(
				NeuralNetwork.Crossover(SelectBiasBest(lastCars, pointsTotal), SelectBiasBest(lastCars, pointsTotal)));
			_cars[i].name = "AICar (Crossed)";
			_cars[i].GetComponentInChildren<MeshRenderer>().material.color = Color.blue;
		}

		// Crossover and mutate the rest of the cars
		for (var i = (int) (nrOfCars * (Mutate + Crossover)); i < _cars.Length; i++) {
			_cars[i].GetComponent<NeuralNetwork>().SetNetwork(
				NeuralNetwork.Crossover(SelectBiasBest(lastCars, pointsTotal), SelectBiasBest(lastCars, pointsTotal)));
			_cars[i].GetComponent<NeuralNetwork>().SetNetwork(NeuralNetwork.Mutate(_cars[i]));
			_cars[i].name = "AICar (Mutated and crossed)";
			_cars[i].GetComponentInChildren<MeshRenderer>().material.color = Color.red;
		}

		generation++;
	}
}
