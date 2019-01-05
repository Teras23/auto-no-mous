﻿using UnityEngine;

public class CarSensor : MonoBehaviour
{
    /// <summary>
    /// Wrapper class for sensor's output and properties
    /// </summary>
    public class SensorData
    {
        /// <summary>
        /// Distance from sensor to closest collider in Default level
        /// </summary>
        public float Distance { get; set; }

        /// <summary>
        /// The direction this sensor is facing
        /// </summary>
        public Vector3 Direction { get; set; }

        /// <summary>
        /// The direction this sensor is facing relatively to the car
        /// </summary>
        public Vector3 LocalDirection { get; set; }

        /// <summary>
        /// The color of the sensor's ray visualization (debug)
        /// </summary>
        public Color RayColor { get; set; }
    }

    public SensorData Data => new SensorData
    {
        Distance = _sensorOutput,
        Direction = CurrentDirection,
        LocalDirection = _localDirection,
        RayColor = rayColor
    };

    private float _sensorOutput;
    private Rigidbody2D _carRb;
    private Vector3 _localDirection;

    [SerializeField]
    public Color rayColor = Color.red;

    private Vector3 CurrentDirection => transform == null || _carRb == null ? Vector3.zero : transform.position - (Vector3) _carRb.position;

    void Start()
    {
        _carRb = GetComponentInParent<Rigidbody2D>();
        _localDirection = CurrentDirection;
    }

    void FixedUpdate()
    {
        Debug.DrawRay(transform.position, CurrentDirection.normalized * SimpleGameManager.maxRayLength, rayColor, 0);

        var hit = Physics2D.Raycast(transform.position, CurrentDirection, SimpleGameManager.maxRayLength);
        _sensorOutput = hit ? hit.distance : float.MaxValue;
    }
}