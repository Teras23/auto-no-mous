using System;
using System.Collections.Generic;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;

public class NeuralNetwork : MonoBehaviour
{
    public List<int> hiddenLayers;
    public int inputSize;
    public int outputSize;
    
    private static double softplusRelu(double x)
    {
        return Math.Log(1 + Math.Pow(Math.E, x));
    }

    private static double sigmoid(double x)
    {
        return 1 / (1 + Math.Pow(Math.E, -x));
    }

    private List<Matrix<double>> weights;
    private List<Vector<double>> biases;

    private bool _initiated = false; // Was doing calculations before a car was initiated
    
    void Start()
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

    public double[] calculate(params double[] input)
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
            layer = (layer * weights[i] + biases[i]).Map(x => sigmoid(x));
        }        
        
        // Last layer to be calculated is the output
        return layer.AsArray();
    }

    public static Tuple<List<Matrix<double>>, List<Vector<double>>> merge(NeuralNetwork network1,
        NeuralNetwork network2)
    {
        List<Matrix<double>> newWeights = new List<Matrix<double>>(network1.weights.Count);
        List<Vector<double>> newBiases = new List<Vector<double>>(network1.biases.Count);
        
        for (var i = 0; i < newWeights.Count; i++)
        {
            network1.weights[i].CopyTo(newWeights[i]);   
        }
        
        for (var i = 0; i < newBiases.Count; i++)
        {
            network1.biases[i].CopyTo(newBiases[i]);   
        }
        
        return new Tuple<List<Matrix<double>>, List<Vector<double>>>(newWeights, newBiases);
    }
}