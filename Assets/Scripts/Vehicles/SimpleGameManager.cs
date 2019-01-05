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
        Physics2D.IgnoreLayerCollision(Physics.IgnoreRaycastLayer, Physics.IgnoreRaycastLayer);

        _cars = new GameObject[nrOfCars];
        for (var i = 0; i < nrOfCars; i++)
        {
            var car = Instantiate(carPrefab);
            car.GetComponent<CombinedCarController>().AI = true;
            _cars[i] = car;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
