using System;
using System.Collections.Generic;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;

public class NeuralNetwork : MonoBehaviour
{
    public List<int> hiddenLayers;

    private static double softplusRelu(double x)
    {
        return Math.Log(1 + Math.Pow(Math.E, x));
    }

    private static double sigmoid(double x)
    {
        return 1 / (1 + Math.Pow(Math.E, -x));
    }

    private const int InputSize = 2;
    private const int OutputSize = 2;

    private List<Matrix<double>> weights;
    private List<Vector<double>> biases;

    void Start()
    {
        weights = new List<Matrix<double>>(hiddenLayers.Count);
        biases = new List<Vector<double>>(hiddenLayers.Count);

        // Add all the hidden layers based on user input
        for (var i = 0; i < hiddenLayers.Count; i++)
        {
            var layerSize = hiddenLayers[i];
            var lastLayerSize = i - 1 < 0 ? InputSize : hiddenLayers[i - 1];

            var hiddenLayer = Matrix<double>.Build.Random(lastLayerSize, layerSize);

            weights.Add(hiddenLayer);
            
            var bias = Vector<double>.Build.Random(layerSize);

            biases.Add(bias);
        }

        // Matrix and bias for last hidden layer connection to output
        var outputLayer = Matrix<double>.Build.Random(hiddenLayers[hiddenLayers.Count - 1], OutputSize);
        weights.Add(outputLayer);
        
        var outputBias = Vector<double>.Build.Random(OutputSize);
        biases.Add(outputBias);

        var results = calculate(0.5, 0.3);

        Debug.Log(results[0] + " " + results[1]);
    }

    public double[] calculate(params double[] input)
    {
        if (input.Length != InputSize)
        {
            Debug.LogError("Arguments amount " + input.Length + " does not match input layer size " + InputSize);
            throw new InvalidParameterException();
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
}