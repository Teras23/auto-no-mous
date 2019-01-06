using System.Linq;
using UnityEngine;

public class SimpleGameManager : MonoBehaviour
{
    private GameObject[] _cars;
    private GameObject _playerCar;
    private bool _inGame;
    private bool started = false;
    private int generation = 1;

    [SerializeField]
    private GameObject _aiCarPrefab;

    [SerializeField]
    private GameObject _playerCarPrefab;

    [SerializeField]
    public int nrOfCars = 10;

    public static float maxRayLength = 10;

    /// <summary>
    /// Used by UI controller
    /// </summary>
    public bool InGame => _inGame;

    // Start is called before the first frame update
    void Start()
    {
        _cars = new GameObject[nrOfCars];
    }

    // Update is called once per frame
    void Update()
    {
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


    /// <summary>
    /// Used by UI controller
    /// </summary>
    public void EnterPlayMode(bool includePlayer = false)
    {
        _inGame = true;

        if (includePlayer)
        {
            _playerCar = GameObject.Instantiate(_playerCarPrefab);
        }

        ClearCars();
        SpawnCars();
    }


    /// <summary>
    /// Used by UI controller
    /// </summary>
    public void LeavePlayMode()
    {
        _inGame = false;
        started = false;
        ClearCars();
    }

    public void ClearCars()
    {
        if (_playerCar != null)
        {
            Destroy(_playerCar);
            _playerCar = null;
        }

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
            var car = Instantiate(_aiCarPrefab);
            _cars[i] = car;
        }

        started = true;
    }

    private const float LastInclude = 0.2f;
    private const float Merge = 0.6f;

    private void SpawnNewCars()
    {
        var lastCars = _cars.OrderByDescending(go => go.GetComponent<CarController>().points)
            .ThenBy(go => go.GetComponent<CarController>().TotalTime).ToList();
        ClearCars();
        SpawnCars();

        // Include the best 2 in the next run
        for (var i = 0; i < (int) (nrOfCars * LastInclude); i++)
        {
            _cars[i].GetComponent<NeuralNetwork>().SetNetwork(lastCars[i].GetComponent<NeuralNetwork>().GetNetwork());
        }

        Debug.Log((int) (nrOfCars * LastInclude));
        Debug.Log((int) (nrOfCars * Merge));

        // Merge 6 of the cars
        for (var i = (int) (nrOfCars * LastInclude); i < (int) (nrOfCars * Merge); i++)
        {
            _cars[i].GetComponent<NeuralNetwork>().SetNetwork(NeuralNetwork.Merge(lastCars[0], lastCars[1]));
        }

        // Let 2 cars be totally random again

        generation++;
    }
}
