using UnityEngine;

public class CarController : MonoBehaviour {
	public float maxAcc, maxTurn;
	Rigidbody2D rb;
	public bool AI;

	void Start() {
		rb = GetComponent<Rigidbody2D>();
		maxTurn *= Mathf.Deg2Rad;
	}

	void FixedUpdate() {
		if (AI) {
			ControlVehicle(0, 0); //TODO: Hook up to NN
		} else {
			ControlVehicle(Input.GetAxisRaw("Vertical"), Input.GetAxisRaw("Horizontal"));
		}
	}

	void ControlVehicle(float acc, float turn) {
		Vector2 accelerationVector;
		if (acc > 0) {
			accelerationVector = rb.GetRelativeVector(Vector2.right) * acc * maxAcc; //Forward acceleration for a mass of 1 unit
		} else {
			accelerationVector = Vector2.zero; //TODO: Braking
		}
		//Realistic turn formula for a wheel separation of 2.5m (0.4 = 1 / 2.5)
		//Tan would be for back wheel speed, Sin for front wheel speed. So it's the average (0.4 * 0.5 = 0.2)
		float realTurn = turn * maxTurn;
		rb.rotation -= rb.velocity.magnitude * (Mathf.Tan(realTurn) + Mathf.Sin(realTurn)) * 0.2f * Mathf.Rad2Deg * Time.fixedDeltaTime; //TODO: Forward velocity component only

		//TODO: Anti-drift friction (Currently old code, no clue if at all correct)
		float driftForce = Vector2.Dot(rb.velocity, rb.GetRelativeVector(Vector2.left)) * 2.0f;
		Vector2 relativeForce = Vector2.right * driftForce;
		Debug.DrawLine(rb.position, rb.GetRelativePoint(relativeForce), Color.green);
		rb.AddForce(accelerationVector);
	}

}
