using System;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;

public class NeuralNetwork : MonoBehaviour {
	public int[] hiddenLayers;
	public int inputSize, outputSize;

	static System.Random rng = new System.Random();

	Matrix<double>[] weights;
	Vector<double>[] biases;
	bool initiated = false; // Was doing calculations before a car was initiated

	void Start() {
		if (!initiated) {
			SetRandom();
			initiated = true;
		}
	}

	public double[] Calculate(params double[] input) {
		if (input.Length != inputSize) {
			Debug.LogError("Arguments amount " + input.Length + " does not match input layer size " + inputSize);
			throw new InvalidParameterException();
		}

		if (!initiated) {
			return new double[outputSize];
		}

		// First layer is the input layer
		var layer = Vector<double>.Build.DenseOfArray(input);

		// Based on every layer weight and bias, calculate the layer values
		for (var i = 0; i < weights.Length; i++) {
			layer = (layer * weights[i] + biases[i]).Map(x => Math.Tanh(x / 2));
		}

		// Last layer to be calculated is the output
		return layer.AsArray();
	}

	public void SetNetwork(Matrix<double>[] weights, Vector<double>[] biases) {
		this.weights = weights;
		this.biases = biases;

		initiated = true;
	}

	public void GetNetwork(out Matrix<double>[] weights, out Vector<double>[] biases) {
		weights = this.weights;
		biases = this.biases;
	}

	public static void Mutate(GameObject go, out Matrix<double>[] weights, out Vector<double>[] biases) {
		Mutate(go.GetComponent<NeuralNetwork>(), out weights, out biases);
	}

	public static void FullCrossover(GameObject go1, GameObject go2, out Matrix<double>[] weights, out Vector<double>[] biases) {
		FullCrossover(go1.GetComponent<NeuralNetwork>(), go2.GetComponent<NeuralNetwork>(), out weights, out biases);
	}

	public static void LinearCrossover(GameObject go1, GameObject go2, out Matrix<double>[] weights, out Vector<double>[] biases) {
		LinearCrossover(go1.GetComponent<NeuralNetwork>(), go2.GetComponent<NeuralNetwork>(), out weights, out biases);
	}

	public void SetRandom() {
		weights = new Matrix<double>[hiddenLayers.Length + 1];
		biases = new Vector<double>[hiddenLayers.Length + 1];

		// Add all the hidden layers based on user input
		for (var i = 0; i < hiddenLayers.Length; i++) {
			var layerSize = hiddenLayers[i];
			var lastLayerSize = i <= 0 ? inputSize : hiddenLayers[i - 1];

			weights[i] = Matrix<double>.Build.Random(lastLayerSize, layerSize);
			biases[i] = Vector<double>.Build.Random(layerSize);
		}

		// Matrix and bias for last hidden layer connection to output
		weights[hiddenLayers.Length] = Matrix<double>.Build.Random(hiddenLayers[hiddenLayers.Length - 1], outputSize);
		biases[hiddenLayers.Length] = Vector<double>.Build.Random(outputSize);
	}

	public static void Mutate(NeuralNetwork network1, out Matrix<double>[] weights, out Vector<double>[] biases) {
		weights = new Matrix<double>[network1.weights.Length];
		biases = new Vector<double>[network1.biases.Length];

		for (var i = 0; i < network1.weights.Length; i++) {
			var newWeight = Matrix<double>.Build.Random(network1.weights[i].RowCount, network1.weights[i].ColumnCount);

			for (var r = 0; r < newWeight.RowCount; r++) {
				for (var c = 0; c < newWeight.ColumnCount; c++) {
					double choice = rng.NextDouble();
					if (choice < 0.005) { //Sign change
						newWeight[r, c] = -network1.weights[i][r, c];
					} else if (choice < 0.02) { //Additive noise
						newWeight[r, c] = network1.weights[i][r, c] + rng.NextDouble() * 1.5 - 0.5;
					} else if (choice < 0.035) { //Multiplicative noise
						newWeight[r, c] = network1.weights[i][r, c] * (rng.NextDouble() * 1.5 + 0.5);
					} else if (choice < 0.06) { //Keep the new random

					} else { //Keep the old one
						newWeight[r, c] = network1.weights[i][r, c];
					}
				}
			}
			weights[i] = newWeight;
		}

		for (var i = 0; i < network1.biases.Length; i++) {
			var newBias = Vector<double>.Build.Random(network1.biases[i].Count);

			for (var c = 0; c < newBias.Count; c++) {
				double choice = rng.NextDouble();
				if (choice < 0.005) { //Sign change
					newBias[c] = -network1.biases[i][c];
				} else if (choice < 0.02) { //Additive noise
					newBias[c] = network1.biases[i][c] + rng.NextDouble() * 1.5 - 0.5;
				} else if (choice < 0.035) { //Multiplicative noise
					newBias[c] = network1.biases[i][c] * (rng.NextDouble() * 1.5 + 0.5);
				} else if (choice < 0.06) { //Keep the new random

				} else { //Keep the old one
					newBias[c] = network1.biases[i][c];
				}
			}
			biases[i] = newBias;
		}
	}

	public static void FullCrossover(NeuralNetwork network1, NeuralNetwork network2, out Matrix<double>[] weights, out Vector<double>[] biases) {
		weights = new Matrix<double>[network1.weights.Length];
		biases = new Vector<double>[network1.biases.Length];

		for (var i = 0; i < network1.weights.Length; i++) {
			var newWeight = Matrix<double>.Build.Random(network1.weights[i].RowCount, network1.weights[i].ColumnCount);

			for (var r = 0; r < newWeight.RowCount; r++) {
				for (var c = 0; c < newWeight.ColumnCount; c++) {
					if (rng.NextDouble() < 0.5) {
						newWeight[r, c] = network1.weights[i][r, c];
					} else {
						newWeight[r, c] = network2.weights[i][r, c];
					}
				}
			}
			weights[i] = newWeight;
		}

		for (var i = 0; i < network1.biases.Length; i++) {
			var newBias = Vector<double>.Build.Random(network1.biases[i].Count);

			for (var c = 0; c < newBias.Count; c++) {
				if (rng.NextDouble() < 0.5) {
					newBias[c] = network1.biases[i][c];
				} else {
					newBias[c] = network2.biases[i][c];
				}
			}
			biases[i] = newBias;
		}
	}

	public static void LinearCrossover(NeuralNetwork network1, NeuralNetwork network2, out Matrix<double>[] weights, out Vector<double>[] biases) {
		weights = new Matrix<double>[network1.weights.Length];
		biases = new Vector<double>[network1.biases.Length];

		for (var i = 0; i < network1.weights.Length; i++) {
			var newWeight = Matrix<double>.Build.Random(network1.weights[i].RowCount, network1.weights[i].ColumnCount);

			for (var r = 0; r < newWeight.RowCount; r++) {
				for (var c = 0; c < newWeight.ColumnCount; c++) {
					double weight = rng.NextDouble();
					newWeight[r, c] = network1.weights[i][r, c] * weight + network2.weights[i][r, c] * (1 - weight);
				}
			}
			weights[i] = newWeight;
		}

		for (var i = 0; i < network1.biases.Length; i++) {
			var newBias = Vector<double>.Build.Random(network1.biases[i].Count);

			for (var c = 0; c < newBias.Count; c++) {
				double weight = rng.NextDouble();
				newBias[c] = network1.biases[i][c] * weight + network2.biases[i][c] * (1 - weight);
			}
			biases[i] = newBias;
		}
	}
}