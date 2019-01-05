using System.Linq;
using UnityEngine;

public class CombinedCarController : MonoBehaviour {
	public float maxAcc, maxTurn, sideFriction;
	Rigidbody2D rb;
	public bool AI;

    private CarSensor[] _sensors;

    public float[] debugSensorValues;
    
    public CarSensor.SensorData[] SensorData =>
        _sensors == null ? new CarSensor.SensorData[0] : _sensors.Select(x => x.Data).ToArray();

    public int Id { get; set; }

    void Start() {
		rb = GetComponent<Rigidbody2D>();
		maxTurn *= Mathf.Deg2Rad;

        _sensors = GetComponentsInChildren<CarSensor>();
    }

	void FixedUpdate() {
        debugSensorValues = SensorData.Select(x => x.Distance).ToArray();

        if (AI) {
			ControlVehicle(0, 0); //TODO: Hook up to NN
		} else {
			ControlVehicle(Input.GetAxisRaw("Vertical"), Input.GetAxisRaw("Horizontal"));
		}
	}

	void ControlVehicle(float acc, float turn) {
		Vector2 accelerationVector;
		Vector2 direction = rb.GetRelativeVector(Vector2.right);
		Vector2 sideDirection = rb.GetRelativeVector(Vector2.down);
		float currentSpeed = rb.velocity.magnitude;

		if (acc > 0) {
			accelerationVector = direction * acc * maxAcc; //Forward acceleration for a mass of 1 unit
		} else {
			accelerationVector = Vector2.zero; //TODO: Braking
		}

		//Realistic turn formula for a wheel separation of 3m (0.34 ~= 1 / 3)
		//Tan would be for back wheel speed, Sin for front wheel speed. So it's the average (0.34 * 0.5 = 0.17)
		float realTurn = turn * maxTurn;
		rb.rotation -= Vector2.Dot(rb.velocity, direction) * (Mathf.Tan(realTurn) + Mathf.Sin(realTurn)) * 0.17f * Mathf.Rad2Deg * Time.fixedDeltaTime;

		//Air resistance for an air density of 1.2kg/m^3, frontal area of 2m^2, mass of 2 tons, and a drag coefficient of 0.25
		//(0.25 * 1.2 * v^2 * 2) / (2 * 2000) = 0.00015v^2
		accelerationVector -= 0.00015f * currentSpeed * currentSpeed * rb.velocity;

		//Anti-drift friction
		float antiDriftAcceleration = Vector2.Dot(rb.velocity, sideDirection) * sideFriction;
		accelerationVector -= antiDriftAcceleration * sideDirection;
		//Add a fraction of that to forward speed
		accelerationVector += 0.5f * Mathf.Abs(antiDriftAcceleration) * direction;

		rb.AddForce(accelerationVector);
	}

}
