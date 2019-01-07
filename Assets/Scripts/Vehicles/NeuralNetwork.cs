using System;
using System.Collections.Generic;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;

public class NeuralNetwork : MonoBehaviour {
	public int[] hiddenLayers;
	public int inputSize, outputSize;

	static System.Random rng;

	Matrix<double>[] weights;
	Vector<double>[] biases;
	bool initiated = false; // Was doing calculations before a car was initiated

	void Start() {
		rng = new System.Random();
		if (!initiated) {
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

	private const double MutationChance = 0.05;
	private const double MutationChanceBias = 0.05;

	public static void Mutate(GameObject go, out Matrix<double>[] weights, out Vector<double>[] biases) {
		Mutate(go.GetComponent<NeuralNetwork>(), out weights, out biases);
	}

	public static void Crossover(GameObject go1, GameObject go2, out Matrix<double>[] weights, out Vector<double>[] biases) {
		Crossover(go1.GetComponent<NeuralNetwork>(), go2.GetComponent<NeuralNetwork>(), out weights, out biases);
	}

	public static void Mutate(NeuralNetwork network1, out Matrix<double>[] weights, out Vector<double>[] biases) {
		weights = new Matrix<double>[network1.weights.Length];
		biases = new Vector<double>[network1.biases.Length];

		for (var i = 0; i < network1.weights.Length; i++) {
			var newWeight = Matrix<double>.Build.Random(network1.weights[i].RowCount, network1.weights[i].ColumnCount);

			for (var r = 0; r < newWeight.RowCount; r++) {
				for (var c = 0; c < newWeight.ColumnCount; c++) {
					var choice = rng.NextDouble();

					if (choice > MutationChance) {
						newWeight[r, c] = network1.weights[i][r, c];
					} else {
						newWeight[r, c] *= (rng.NextDouble() * 2) * (rng.Next(2) * 2 - 1);
					}
				}
			}
			weights[i] = newWeight;
		}

		for (var i = 0; i < network1.biases.Length; i++) {
			var newBias = Vector<double>.Build.Random(network1.biases[i].Count);

			for (var c = 0; c < newBias.Count; c++) {
				var choice = rng.NextDouble();

				if (choice > MutationChanceBias) {
					newBias[c] = network1.biases[i][c];
				} else {
					newBias[c] *= (rng.NextDouble() * 2) * (rng.Next(2) * 2 - 1);
				}
			}
			biases[i] = newBias;
		}
	}

	public static void Crossover(NeuralNetwork network1, NeuralNetwork network2, out Matrix<double>[] weights, out Vector<double>[] biases) {
		weights = new Matrix<double>[network1.weights.Length];
		biases = new Vector<double>[network1.biases.Length];

		for (var i = 0; i < network1.weights.Length; i++) {
			var newWeight = Matrix<double>.Build.Random(network1.weights[i].RowCount, network1.weights[i].ColumnCount);

			for (var r = 0; r < newWeight.RowCount; r++) {
				for (var c = 0; c < newWeight.ColumnCount; c++) {
					var choice = rng.NextDouble();

					if (choice < 0.5) {
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
				var choice = rng.NextDouble();

				if (choice < 0.5) {
					newBias[c] = network1.biases[i][c];
				} else {
					newBias[c] = network2.biases[i][c];
				}
			}
			biases[i] = newBias;
		}
	}
}