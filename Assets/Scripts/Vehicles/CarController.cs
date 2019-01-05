﻿using UnityEngine;

public class CarController : MonoBehaviour {
	public float maxAcc, maxTurn, wheelFriction;
	Rigidbody2D rb;
	public bool AI;

	void Start() {
		rb = GetComponent<Rigidbody2D>();
		maxTurn *= Mathf.Deg2Rad;
		wheelFriction *= 10;
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

		//Air resistance for an air density of 1.2kg/m^3, frontal area of 2m^2, mass of 2 tons, and a drag coefficient of 0.3
		//(0.3 * 1.2 * v^2 * 2) / (2 * 2000) = 0.00018v^2
		accelerationVector -= 0.00018f * rb.velocity.magnitude * rb.velocity;

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
