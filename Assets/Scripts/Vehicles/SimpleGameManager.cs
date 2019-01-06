using System;
using System.Linq;
using UnityEngine;

public class SimpleGameManager : MonoBehaviour
{
    [SerializeField] public GameObject carPrefab;

    [SerializeField] public int nrOfCars = 10;

    [SerializeField] public bool includeManual;

    public static float maxRayLength = 10;

    private GameObject[] _cars;

    private bool started = false;
    private int generation = 1;

    // Start is called before the first frame update
    void Start()
    {
        _cars = new GameObject[nrOfCars];
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Play"))
        {
            ClearCars();
            SpawnCars();
        }

        var allFinished = true;

        foreach (var car in _cars)
        {
            if (car != null && car.activeSelf)
            {
                allFinished = false;
            }
        }

        if (started && allFinished)
        {
            SpawnNewCars();
            Debug.Log("All finished");
        }
    }

    public void ClearCars()
    {
        for (var i = 0; i < _cars.Length; i++)
        {
            if (_cars[i] != null)
            {
                Destroy(_cars[i]);
                _cars[i] = null;
            }
        }
    }

    private void SpawnCars()
    {
        for (var i = 0; i < nrOfCars; i++)
        {
            var car = Instantiate(carPrefab);
            car.GetComponent<CarController>().AI = true;
            _cars[i] = car;
        }

        started = true;
    }

    private const float LastInclude = 0.2f;
    private const float Merge = 0.6f;
    
    private void SpawnNewCars()
    {
        var lastCars = _cars.OrderByDescending(go => go.GetComponent<CarController>().points).ToList();
        ClearCars();
        SpawnCars();

        // Include the best 2 in the next run
        for (var i = 0; i < (int)(nrOfCars * LastInclude); i++)
        {
            _cars[i].GetComponent<NeuralNetwork>().SetNetwork(lastCars[i].GetComponent<NeuralNetwork>().GetNetwork());
        }

        Debug.Log((int)(nrOfCars * LastInclude));
        Debug.Log((int)(nrOfCars * Merge));
        
        // Merge 6 of the cars
        for (var i = (int)(nrOfCars * LastInclude); i < (int)(nrOfCars * Merge); i++)
        {
            _cars[i].GetComponent<NeuralNetwork>().SetNetwork(NeuralNetwork.Merge(lastCars[0], lastCars[1]));
        }
        
        // Let 2 cars be totally random again

        generation++;
    }
}