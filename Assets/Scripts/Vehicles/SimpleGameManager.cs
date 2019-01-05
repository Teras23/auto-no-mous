using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class SimpleGameManager : MonoBehaviour
{
    [SerializeField]
    public CarMovement carPrefab;

    [SerializeField]
    public int nrOfCars = 10;

    [SerializeField]
    public bool includeManual;

    public static float maxRayLength = 10;

    private CarMovement[] _cars;

    // Start is called before the first frame update
    void Start()
    {
        Physics2D.IgnoreLayerCollision(Physics.IgnoreRaycastLayer, Physics.IgnoreRaycastLayer);

        _cars = new CarMovement[nrOfCars];
        for (var i = 0; i < nrOfCars; i++)
        {
            var car = Instantiate(carPrefab);
            car.Id = i;
            _cars[i] = car;
        }

        if (includeManual)
        {
            _cars[0].isManual = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
