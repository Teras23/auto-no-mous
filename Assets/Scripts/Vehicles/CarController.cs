using UnityEngine;

public class CarController : MonoBehaviour {
	public float maxAcc, maxTurn, wheelFriction;
	Rigidbody2D rb;
	public bool AI;
	//Score variables
	public int points = 0;
	float startTime, lastTime;
	public float TotalTime => lastTime - startTime;
	//AI Driving
	public float[] sensorAngles;
	Vector2[] sensorDirections;
	NeuralNetwork neuralNetwork;

	void Start() {
		rb = GetComponent<Rigidbody2D>();
		neuralNetwork = GetComponent<NeuralNetwork>();
		maxTurn *= Mathf.Deg2Rad;
		wheelFriction *= 10;
		startTime = Time.time;
		lastTime = Time.time;
		//Initialize sensors
		sensorDirections = new Vector2[sensorAngles.Length];
		for (int i = 0; i < sensorAngles.Length; i++) {
			sensorDirections[i] = new Vector2(Mathf.Cos(sensorAngles[i] * Mathf.Deg2Rad), -Mathf.Sin(sensorAngles[i] * Mathf.Deg2Rad));
		}
	}

	void OnTriggerEnter2D(Collider2D collision) {
		int n = collision.GetComponent<Checkpoint>().order;
		if (n == points + 1) {
			points = n;
			lastTime = Time.time;
		}
	}

	private void OnCollisionEnter2D(Collision2D other)
	{
		gameObject.SetActive(false);
	}

	void FixedUpdate() {
		if (AI) {
			double[] results = neuralNetwork.Calculate(System.Array.ConvertAll<Vector2, double>(sensorDirections, direction => {
				RaycastHit2D hit = Physics2D.Raycast(rb.position, rb.GetRelativeVector(direction), float.PositiveInfinity, LayerMask.GetMask("Wall"));
				return hit ? hit.distance : float.MaxValue;
			}));
			ControlVehicle((float) results[0], (float) results[1]);

			//Stop car if it has not gone through any checkpoint for 3 seconds
			if (Time.time > lastTime + 3) {
				gameObject.SetActive(false);
			}
		} else {
			ControlVehicle(Input.GetAxisRaw("Vertical"), Input.GetAxisRaw("Horizontal"));
		}
	}

	void ControlVehicle(float acc, float turn) {
		Vector2 accelerationVector;
		Vector2 direction = rb.GetRelativeVector(Vector2.right);
		Vector2 sideDirection = rb.GetRelativeVector(Vector2.down);
		float forwardSpeed = Vector2.Dot(rb.velocity, direction);
		float sideSpeed = Vector2.Dot(rb.velocity, sideDirection);

		if (acc > 0) {
			accelerationVector = direction * acc * maxAcc; //Forward acceleration for a mass of 1 unit
		} else {
			if (forwardSpeed > -0.3f * acc) { //Regular braking
				accelerationVector = direction * wheelFriction * acc;
			} else { //Come to a halt if speed is low enough
				accelerationVector = Vector2.zero;
				rb.velocity = sideSpeed * sideDirection;
				forwardSpeed = 0;
			}
		}

		//Realistic turn formula for a wheel separation of 3m (0.34 ~= 1 / 3)
		//Tan would be for back wheel speed, Sin for front wheel speed. So it's the average (0.34 * 0.5 = 0.17)
		float realTurn = turn * maxTurn;
		rb.rotation -= Vector2.Dot(rb.velocity, direction) * (Mathf.Tan(realTurn) + Mathf.Sin(realTurn)) * 0.17f * Mathf.Rad2Deg * Time.fixedDeltaTime;

		//Air resistance for an air density of 1.2kg/m^3, average cross-section of 2.5m^2, mass of 2 tons, and a drag coefficient of 0.3
		//(0.3 * 1.2 * v^2 * 2.5) / (2 * 2000) = 0.000225v^2
		accelerationVector -= 0.000225f * rb.velocity.magnitude * rb.velocity;

		//Anti-drift friction
		float antiDriftAcceleration;
		if (sideSpeed > 0.3f) { //Sliding to the right
			antiDriftAcceleration = wheelFriction;
		} else if (sideSpeed < -0.3f) { //Sliding to the left
			antiDriftAcceleration = -wheelFriction;
		} else { //Come to a halt if speed is low enough
			antiDriftAcceleration = 0;
			rb.velocity = forwardSpeed * direction;
			sideSpeed = 0;
		}
		accelerationVector -= antiDriftAcceleration * sideDirection;

		rb.AddForce(accelerationVector);
	}
}
