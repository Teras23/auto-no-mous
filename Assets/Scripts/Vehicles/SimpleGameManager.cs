using System;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using Random = UnityEngine.Random;

public class SimpleGameManager : MonoBehaviour
{
    [SerializeField]
    public GameObject carPrefab;

    [SerializeField]
    public int nrOfCars = 10;

    [SerializeField]
    public bool includeManual;

    public static float maxRayLength = 10;

    private GameObject[] _cars;

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
    }

    public void ClearCars()
    { 
        for(var i = 0; i < _cars.Length; i++)
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
            car.GetComponent<CombinedCarController>().AI = true;
            _cars[i] = car;
        }
    }
}
