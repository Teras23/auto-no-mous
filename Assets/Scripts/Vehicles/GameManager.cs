using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;

public class GameManager : MonoBehaviour {
	GameObject[] cars;
	bool started = false;
	public bool inGame;
	int generation = 1;

    public GameObject aiCarPrefab;
	public int nrOfCars;
	public TrackMaker trackMaker;
	public UIController UIController;

	System.Random rng = new System.Random();

	void Start() {
		cars = new GameObject[nrOfCars];
	}

	void Update() {
		foreach (var car in cars) {
			if (car != null && car.activeSelf) {
				return;
			}
		}

		if (started) {
			SpawnNewCars();
		}
	}

	public void EnterPlayMode() {
		inGame = true;
		ClearCars();
		SpawnCars();
	}

	public void LeavePlayMode() {
		inGame = false;
		started = false;
		ClearCars();
	}

	public void ClearCars() {
		for (var i = 0; i < cars.Length; i++) {
			if (cars[i] != null) {
				Destroy(cars[i]);
				cars[i] = null;
			}
		}
	}

	void SpawnCars() {
		for (var i = 0; i < nrOfCars; i++) {
			var car = Instantiate(aiCarPrefab);
			cars[i] = car;
		}

		started = true;
	}

	const float Mutate = 0.4f;
	const float Crossover = 0.3f;

	GameObject SelectBiasBest(List<GameObject> cars, int pointsTotal) {
		var randomPoints = rng.Next(pointsTotal);

		var totalIterator = 0;

		foreach (var car in cars) {
			totalIterator += car.GetComponent<CarController>().points;

			if (totalIterator >= randomPoints) {
				return car;
			}
		}

		return cars.Last();
	}

	void SpawnNewCars() {
		for (var i = 0; i < cars.Length; i++) {
			if (cars[i].GetComponent<CarController>().points == trackMaker.checkpointCounter - 1) {
				cars[i].GetComponent<CarController>().points *= 2;
			}
		}

		var lastCars = cars.OrderByDescending(go => go.GetComponent<CarController>().points).ThenBy(go => go.GetComponent<CarController>().TotalTime).ToList();

		ClearCars();
		SpawnCars();

		var pointsTotal = 0;

		foreach (var car in lastCars) {
			pointsTotal += car.GetComponent<CarController>().points;
		}

		// Mutate best cars
		for (var i = 0; i < (int) (nrOfCars * Mutate); i++) {
			var selectedCar = SelectBiasBest(lastCars, pointsTotal);

			NeuralNetwork.Mutate(selectedCar, out Matrix<double>[] weights, out Vector<double>[] biases);
			cars[i].GetComponent<NeuralNetwork>().SetNetwork(weights, biases);
			cars[i].name = "AICar (Mutated) " + selectedCar.GetComponent<CarController>().points;
			cars[i].GetComponentInChildren<MeshRenderer>().material.color = Color.green;
		}

		// Crossover best cars
		for (var i = (int) (nrOfCars * Mutate); i < (int) (nrOfCars * (Mutate + Crossover)); i++) {
			var selectedCar1 = SelectBiasBest(lastCars, pointsTotal);
			var selectedCar2 = SelectBiasBest(lastCars, pointsTotal);

			NeuralNetwork.Crossover(selectedCar1, selectedCar2, out Matrix<double>[] weights, out Vector<double>[] biases);
			cars[i].GetComponent<NeuralNetwork>().SetNetwork(weights, biases);
			cars[i].name = "AICar (Crossed) " + selectedCar1.GetComponent<CarController>().points + " " + selectedCar2.GetComponent<CarController>().points;
			cars[i].GetComponentInChildren<MeshRenderer>().material.color = Color.blue;
		}

		// Crossover and mutate the rest of the cars
		for (var i = (int) (nrOfCars * (Mutate + Crossover)); i < cars.Length - 1; i++) {
			var selectedCar1 = SelectBiasBest(lastCars, pointsTotal);
			var selectedCar2 = SelectBiasBest(lastCars, pointsTotal);

			NeuralNetwork.Crossover(selectedCar1, selectedCar2, out Matrix<double>[] weights, out Vector<double>[] biases);
			cars[i].GetComponent<NeuralNetwork>().SetNetwork(weights, biases);
			NeuralNetwork.Mutate(cars[i], out weights, out biases);
			cars[i].GetComponent<NeuralNetwork>().SetNetwork(weights, biases);
			cars[i].name = "AICar (Mutated and crossed) " + selectedCar1.GetComponent<CarController>().points + " " + selectedCar2.GetComponent<CarController>().points;
			;
			cars[i].GetComponentInChildren<MeshRenderer>().material.color = Color.red;
		}

		// Last car is the best car
		lastCars[0].GetComponent<NeuralNetwork>().GetNetwork(out Matrix<double>[] bestWeights, out Vector<double>[] bestBiases);
		cars[cars.Length - 1].GetComponent<NeuralNetwork>().SetNetwork(bestWeights, bestBiases);
		cars[cars.Length - 1].name = "AICar (Best car) " + lastCars[0].GetComponent<CarController>().points;
		cars[cars.Length - 1].GetComponentInChildren<MeshRenderer>().material.color = Color.yellow;

		generation++;
        UIController.UpdateInfoPanel(generation, lastCars[0].GetComponent<CarController>());
	}
}
