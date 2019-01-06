using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;
using Random = System.Random;

public class NeuralNetwork : MonoBehaviour
{
    public List<int> hiddenLayers;
    public int inputSize;
    public int outputSize;
    
    private static double SoftplusRelu(double x)
    {
        return Math.Log(1 + Math.Pow(Math.E, x));
    }

    private static double Sigmoid(double x)
    {
        return 1 / (1 + Math.Pow(Math.E, -x));
    }

    private List<Matrix<double>> weights;
    private List<Vector<double>> biases;

    private bool _initiated = false; // Was doing calculations before a car was initiated
    
    void Start()
    {
        if (!_initiated)
        {
            weights = new List<Matrix<double>>(hiddenLayers.Count);
            biases = new List<Vector<double>>(hiddenLayers.Count);

            // Add all the hidden layers based on user input
            for (var i = 0; i < hiddenLayers.Count; i++)
            {
                var layerSize = hiddenLayers[i];
                var lastLayerSize = i - 1 < 0 ? inputSize : hiddenLayers[i - 1];

                var hiddenLayer = Matrix<double>.Build.Random(lastLayerSize, layerSize);

                weights.Add(hiddenLayer);

                var bias = Vector<double>.Build.Random(layerSize);

                biases.Add(bias);
            }

            // Matrix and bias for last hidden layer connection to output
            var outputLayer = Matrix<double>.Build.Random(hiddenLayers[hiddenLayers.Count - 1], outputSize);
            weights.Add(outputLayer);

            var outputBias = Vector<double>.Build.Random(outputSize);
            biases.Add(outputBias);
            _initiated = true;
        }
    }

    public double[] Calculate(params double[] input)
    {
        if (input.Length != inputSize)
        {
            Debug.LogError("Arguments amount " + input.Length + " does not match input layer size " + inputSize);
            throw new InvalidParameterException();
        }

        if (!_initiated)
        {
            var output = new double[outputSize];

            for (var i = 0; i < output.Length; i++)
            {
                output[i] = 0.0;
            }
            
            return output;
        }

        // First layer is the input layer
        var layer = Vector<double>.Build.DenseOfArray(input);

        // Based on every layer weight and bias, calculate the layer values
        for (var i = 0; i < weights.Count; i++)
        {
            layer = (layer * weights[i] + biases[i]).Map(x => Math.Tanh(x / 2));
        }        
        
        // Last layer to be calculated is the output
        return layer.AsArray();
    }

    public void SetNetwork(Tuple<List<Matrix<double>>, List<Vector<double>>> newNetwork)
    {
        weights = newNetwork.Item1.ToList();
        biases = newNetwork.Item2.ToList();
                
        _initiated = true;
    }

    public Tuple<List<Matrix<double>>, List<Vector<double>>> GetNetwork()
    {
        return new Tuple<List<Matrix<double>>, List<Vector<double>>>(weights, biases); 
    }
    
    private const double MutationChance = 0.2;
    private const double MutationChanceBias = 0.2;

    public static Tuple<List<Matrix<double>>, List<Vector<double>>> Merge(GameObject go1, GameObject go2)
    {
        return Merge(go1.GetComponent<NeuralNetwork>(), go2.GetComponent<NeuralNetwork>());
    }
    
    public static Tuple<List<Matrix<double>>, List<Vector<double>>> Merge(NeuralNetwork network1,
        NeuralNetwork network2)
    {
        List<Matrix<double>> newWeights = new List<Matrix<double>>();
        List<Vector<double>> newBiases = new List<Vector<double>>();
        
        Random random = new Random();

        for (var i = 0; i < network1.weights.Count; i++)
        {
            var newWeight = Matrix<double>.Build.Random(network1.weights[i].RowCount, network1.weights[i].ColumnCount);

            for (var r = 0; r < newWeight.RowCount; r++)
            {
                for (var c = 0; c < newWeight.ColumnCount; c++)
                {
                    var choice = random.NextDouble();
                    
                    if (choice < (1 - MutationChance) / 2)
                    {
                        newWeight[r, c] = network1.weights[i][r, c];
                    }
                    else if (choice < 1 - MutationChance)
                    {
                        newWeight[r, c] = network2.weights[i][r, c];
                    }                    
                }
            }
            newWeights.Add(newWeight);
        }
        
        for (var i = 0; i < network1.biases.Count; i++)
        {
            var newBias = Vector<double>.Build.Random(network1.biases[i].Count);

            for (var c = 0; c < newBias.Count; c++)
            {
                var choice = random.NextDouble();
                    
                if (choice < (1 - MutationChanceBias) / 2)
                {
                    newBias[c] = network1.biases[i][c];
                }
                else if (choice < 1 - MutationChanceBias)
                {
                    newBias[c] = network2.biases[i][c];
                }  
            }
            newBiases.Add(newBias);
        }
        
        return new Tuple<List<Matrix<double>>, List<Vector<double>>>(newWeights, newBiases);
    }
}