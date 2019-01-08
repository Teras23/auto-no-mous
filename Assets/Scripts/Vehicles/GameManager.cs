using MathNet.Numerics.LinearAlgebra;
using UnityEngine;

public class GameManager : MonoBehaviour {
	GameObject[] cars;
	private GameObject customCar;
	public bool started = false;
	int generation = 0;

    public GameObject aiCarPrefab;
	public int nrOfCars;
	public TrackMaker trackMaker;
	public UIController UIController;

	System.Random rng = new System.Random();

	private Matrix<double>[] bestWeights;
	private Vector<double>[] bestBiases;
	private Matrix<double>[] customWeights;
	private Vector<double>[] customBiases;
	
	void Start() {
		cars = new GameObject[nrOfCars];
	}

	void Update() {
		if (started) {
			foreach (var car in cars) {
				if (car != null && car.activeSelf) {
					return;
				}
			}

			SpawnNewCars();
		}
	}

	public void EnterPlayMode() {
		generation = 0;
		SpawnCars();
		UIController.UpdateInfoPanel(generation);
	}

	public void LeavePlayMode() {
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

		if (customCar != null) {
			Destroy(customCar);
		}
	}

	void SpawnCars() {
		cars = new GameObject[nrOfCars];
		for (var i = 0; i < nrOfCars; i++) {
			var car = Instantiate(aiCarPrefab);
			cars[i] = car;
		}

		if (customWeights != null && customBiases != null) {
			customCar = Instantiate(aiCarPrefab);
			customCar.GetComponent<NeuralNetwork>().SetNetwork(customWeights, customBiases);
			customCar.name = "Custom car";
			customCar.GetComponentInChildren<MeshRenderer>().material.color = Color.black;
		}
		
		started = true;
	}

	public void Restart() {
		GameObject[] lastCars = cars;
		
		ClearCars();
		SpawnCars();
		
		for (int i = 0; i < cars.Length; i++) {
			lastCars[i].GetComponent<NeuralNetwork>() .GetNetwork(out Matrix<double>[] weights, out Vector<double>[] biases);
			cars[i].GetComponent<NeuralNetwork>().SetNetwork(weights, biases);
			cars[i].name = lastCars[i].name;
			cars[i].GetComponentInChildren<MeshRenderer>().material.color = lastCars[i].GetComponentInChildren<MeshRenderer>().material.color;
		}
		
		UIController.UpdateInfoPanel(generation);
	}
	
	void SelectPair(CarController[] cars, out CarController car1, out CarController car2) {
		int choice1 = rng.Next(cars.Length);
		int choice2 = 0;
		do {
			choice2 = rng.Next(cars.Length);
		} while (choice2 == choice1);

		car1 = cars[choice1];
		car2 = cars[choice2];
	}

	CarController SelectOne(CarController[] cars) {
		return cars[rng.Next(cars.Length)];
	}

	const float Elite = 0.1f;
	const float Newbie = 0.2f;
	const float FullCross = 0.4f;
	const float LinearCross = 0.6f;

	void SpawnNewCars() {
		CarController[] lastCars = System.Array.ConvertAll(cars, car => car.GetComponent<CarController>());
		System.Array.Sort(lastCars, (car1, car2) => {
			int diff = car2.points - car1.points;
			if (diff != 0) {
				return diff;
			} else {
				if (car1.TotalTime == car2.TotalTime) {
					return 0;
				} else {
					return car1.TotalTime > car2.TotalTime ? 1 : -1;
				}
			}
		});
		
		System.Array.Resize(ref lastCars, Mathf.RoundToInt(Elite * 4 * nrOfCars));

		UIController.UpdateInfoPanel(++generation, lastCars[0]);

		ClearCars();
		SpawnCars();

		lastCars[0].GetComponent<NeuralNetwork>().GetNetwork(out bestWeights, out bestBiases);
		
		//Keep best cars
		for (int i = 0; i < Mathf.RoundToInt(Elite * nrOfCars); i++) {
			lastCars[i].GetComponent<NeuralNetwork>().GetNetwork(out Matrix<double>[] bestWeights, out Vector<double>[] bestBiases);
			cars[i].GetComponent<NeuralNetwork>().SetNetwork(bestWeights, bestBiases);
			cars[i].name = "Best car " + lastCars[i].points + "p " + lastCars[i].TotalTime + "t";
			cars[i].GetComponentInChildren<MeshRenderer>().material.color = Color.HSVToRGB(0.2f, 1, 1);

		}

		//Generate new cars
		for (int i = Mathf.RoundToInt(Elite * nrOfCars); i < Mathf.RoundToInt(Newbie * nrOfCars); i++) {
			cars[i].GetComponent<NeuralNetwork>().SetRandom();
			cars[i].name = "Random restart";
			cars[i].GetComponentInChildren<MeshRenderer>().material.color = Color.red;
		}

		//Full cross best cars
		for (int i = Mathf.RoundToInt(Newbie * nrOfCars); i < Mathf.RoundToInt(FullCross * nrOfCars); i++) {
			SelectPair(lastCars, out var selectedCar1, out var selectedCar2);

			NeuralNetwork.FullCrossover(selectedCar1.gameObject, selectedCar2.gameObject, out Matrix<double>[] weights, out Vector<double>[] biases);
			cars[i].GetComponent<NeuralNetwork>().SetNetwork(weights, biases);
			cars[i].name = "Full cross (" + selectedCar1.points + "p " + selectedCar1.TotalTime + "t) & (" + selectedCar2.points + "p " + selectedCar2.TotalTime + "t)";
			cars[i].GetComponentInChildren<MeshRenderer>().material.color = Color.HSVToRGB(0.4f, 1, 1);
		}

		//Linear cross best cars
		for (int i = Mathf.RoundToInt(FullCross * nrOfCars); i < Mathf.RoundToInt(LinearCross * nrOfCars); i++) {
			SelectPair(lastCars, out var selectedCar1, out var selectedCar2);

			NeuralNetwork.LinearCrossover(selectedCar1.gameObject, selectedCar2.gameObject, out Matrix<double>[] weights, out Vector<double>[] biases);
			cars[i].GetComponent<NeuralNetwork>().SetNetwork(weights, biases);
			cars[i].name = "Linear cross (" + selectedCar1.points + "p " + selectedCar1.TotalTime + "t) & (" + selectedCar2.points + "p " + selectedCar2.TotalTime + "t)";
			cars[i].GetComponentInChildren<MeshRenderer>().material.color = Color.HSVToRGB(0.6f, 1, 1);
		}

		//Mutate best cars
		for (int i = Mathf.RoundToInt(LinearCross * nrOfCars); i < cars.Length; i++) {
			var selectedCar = SelectOne(lastCars);

			NeuralNetwork.Mutate(selectedCar.gameObject, out Matrix<double>[] weights, out Vector<double>[] biases);
			cars[i].GetComponent<NeuralNetwork>().SetNetwork(weights, biases);
			cars[i].name = "Mutant " + selectedCar.points + "p " + selectedCar.TotalTime + "t";
			cars[i].GetComponentInChildren<MeshRenderer>().material.color = Color.HSVToRGB(0.8f, 1, 1);
		}
	}

	public string ExportBest() {
		if (bestWeights != null && bestBiases != null) {
			return NeuralNetwork.Export(bestWeights, bestBiases);			
		}

		return null;
	}

	public void Import(string nn) {
		NeuralNetwork.Import(nn, out customWeights, out customBiases);
	}

	public void RemoveCustomCar() {
		customWeights = null;
		customBiases = null;
	}
}
