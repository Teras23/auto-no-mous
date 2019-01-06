using UnityEngine;

public class CameraController : MonoBehaviour {
	enum CameraMode {
		Disabled,
		Flat,
		Follow
	}

    public float speed, rotSpeed;
    public Vector3 startPosition = new Vector3(60, -45, -100);

    CameraMode cameraMode = CameraMode.Flat;
	CameraMode oldCameraMode = CameraMode.Flat;
	GameObject player;

	void Start() {
		player = GameObject.FindGameObjectWithTag("Player");
        transform.localPosition = startPosition;
    }

	void Update() {
		switch (cameraMode) {
			case CameraMode.Flat:
				transform.Translate(new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), Input.GetAxisRaw("UpDown")) * (Time.unscaledDeltaTime * speed));
				break;
			case CameraMode.Follow:
				//TODO: Follow best car
				/*
				if (player == null && transform.parent != currentLead) {
					transform.SetParent(currentLead, false);
				}
				*/
				transform.Translate(new Vector3(0, 0, Input.GetAxisRaw("Vertical")) * (Time.unscaledDeltaTime * speed * 0.1f));
				float rotAmount = Time.unscaledDeltaTime * rotSpeed * 60;
				transform.RotateAround(transform.parent.position, transform.TransformDirection(Vector3.right), Input.GetAxisRaw("UpDown") * rotAmount);
				transform.RotateAround(transform.parent.position, Vector3.back, Input.GetAxisRaw("Horizontal") * rotAmount);
				break;
		}

		if (Input.GetButtonDown("ChangeCamera")) {
			switch (cameraMode) {
				case CameraMode.Flat:
					cameraMode = CameraMode.Follow;
					break;
				case CameraMode.Follow:
					cameraMode = CameraMode.Flat;
					break;
			}
			SwitchMode();
		}

		if (Input.GetButtonDown("LockCamera")) {
			if (cameraMode != CameraMode.Disabled) {
				oldCameraMode = cameraMode;
				cameraMode = CameraMode.Disabled;
			} else {
				cameraMode = oldCameraMode;
			}
			SwitchMode();
		}
	}

	void SwitchMode() {
		switch (cameraMode) {
			case CameraMode.Flat:
				transform.parent = null;
				transform.localPosition = startPosition;
				transform.localRotation = Quaternion.identity;
				break;
			case CameraMode.Follow:
				if (player == null) {
					return;
				}
				transform.parent = player.transform;
				transform.localPosition = new Vector3(-Mathf.Sqrt(75), 0, -5);
				transform.localRotation = Quaternion.Euler(0, 60, -90);
				break;
		}
	}
}