using System;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;

public class NeuralNetwork : MonoBehaviour
{
    public List<Neuron> inputs;

    public List<int> hiddenLayers;

    private static double softplusRelu(double x)
    {
        return Math.Log(1 + Math.Pow(Math.E, x));
    }

    public class Neuron
    {
    }

    private const int InputSize = 2;
    private const int OutputSize = 2;

    private List<Matrix<double>> weights;
    private Matrix<double> output;
    
    void Start()
    {
        weights = new List<Matrix<double>>(hiddenLayers.Count);

        for (var i = 0; i < hiddenLayers.Count; i++)
        {
            var layerSize = hiddenLayers[i];
            var lastLayerSize = i - 1 < 0 ? InputSize : hiddenLayers[i - 1];

            var hiddenLayer = Matrix<double>.Build.Random(lastLayerSize, layerSize);
            
            Debug.Log(hiddenLayer);
                
            weights.Add(hiddenLayer);
        }

        output = Matrix<double>.Build.Random(hiddenLayers[hiddenLayers.Count - 1], OutputSize);

        var results = calculate(0.5, 0.3);
        
        Debug.Log(results[0] + " " + results[1]);
    }

    public double[] calculate(params double[] input)
    {
        if (input.Length != InputSize)
        {
            Debug.LogError("Arguments amount " + input.Length + " does not match input layer size " + InputSize);
        }
        
        var inputMatrix = Vector<double>.Build.DenseOfArray(input);

        var hidden = inputMatrix * weights[0];
        Debug.Log(hidden);
        hidden.Map(x => softplusRelu(x));
        var output = hidden * weights[1];
        
        weights[0].Map(x => softplusRelu(x));
        
        return output.AsArray();
    }
}