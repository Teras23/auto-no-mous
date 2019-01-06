﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class SimpleGameManager : MonoBehaviour {
	private GameObject[] _cars;
	private GameObject _playerCar;
	private bool started = false;
	public int generation = 1;

	public GameObject _aiCarPrefab;
	public GameObject _playerCarPrefab;
	public int nrOfCars = 10;

	public static float maxRayLength = 10;

	[SerializeField]
	private TrackMaker _trackMaker;

	/// <summary>
	/// Used by UI controller
	/// </summary>
	public bool InGame { get; private set; }

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
		InGame = true;

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
		InGame = false;
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

	private const float Mutate = 0.4f;
	private const float Crossover = 0.3f;

	private GameObject SelectBiasBest(List<GameObject> cars, int pointsTotal)
	{
		var random = new Random();
		var randomPoints = random.Next(pointsTotal);

		var totalIterator = 0;
		
		foreach (var car in cars)
		{
			totalIterator += car.GetComponent<CarController>().points;			
			
			if (totalIterator >= randomPoints)
			{
				return car;
			}
		}
		
		return cars.Last();
	}
	
	private void SpawnNewCars() {
		for (var i = 0; i < _cars.Length; i++)
		{
			if (_cars[i].GetComponent<CarController>().points == _trackMaker.checkpointCounter - 1)
			{
				_cars[i].GetComponent<CarController>().points *= 2;
			}
		}
		
		
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
		for (var i = 0; i < (int) (nrOfCars * Mutate); i++)
		{
			var selectedCar = SelectBiasBest(lastCars, pointsTotal);
			
			_cars[i].GetComponent<NeuralNetwork>().SetNetwork(
				NeuralNetwork.Mutate(selectedCar));
			_cars[i].name = "AICar (Mutated) " + selectedCar.GetComponent<CarController>().points;
			_cars[i].GetComponentInChildren<MeshRenderer>().material.color = Color.green;
		}

		// Crossover best cars
		for (var i = (int) (nrOfCars * Mutate); i < (int) (nrOfCars * (Mutate + Crossover)); i++) {
			var selectedCar1 = SelectBiasBest(lastCars, pointsTotal);
			var selectedCar2 = SelectBiasBest(lastCars, pointsTotal);
			
			_cars[i].GetComponent<NeuralNetwork>().SetNetwork(
				NeuralNetwork.Crossover(selectedCar1, selectedCar2));
			_cars[i].name = "AICar (Crossed) " + selectedCar1.GetComponent<CarController>().points + " " + selectedCar2.GetComponent<CarController>().points;
			_cars[i].GetComponentInChildren<MeshRenderer>().material.color = Color.blue;
		}

		// Crossover and mutate the rest of the cars
		for (var i = (int) (nrOfCars * (Mutate + Crossover)); i < _cars.Length - 1; i++) {
			var selectedCar1 = SelectBiasBest(lastCars, pointsTotal);
			var selectedCar2 = SelectBiasBest(lastCars, pointsTotal);
			
			_cars[i].GetComponent<NeuralNetwork>().SetNetwork(
				NeuralNetwork.Crossover(selectedCar1, selectedCar2));
			_cars[i].GetComponent<NeuralNetwork>().SetNetwork(NeuralNetwork.Mutate(_cars[i]));
			_cars[i].name = "AICar (Mutated and crossed) " + selectedCar1.GetComponent<CarController>().points + " " + selectedCar2.GetComponent<CarController>().points;;
			_cars[i].GetComponentInChildren<MeshRenderer>().material.color = Color.red;
		}
		
		// Last car is the best car
		_cars[_cars.Length - 1].GetComponent<NeuralNetwork>().SetNetwork(lastCars[0].GetComponent<NeuralNetwork>().GetNetwork());
		_cars[_cars.Length - 1].name = "AICar (Best car) " + lastCars[0].GetComponent<CarController>().points;
		_cars[_cars.Length - 1].GetComponentInChildren<MeshRenderer>().material.color = Color.yellow;
		
		generation++;
	}
}
