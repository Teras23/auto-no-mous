using UnityEngine;

public class CameraController : MonoBehaviour {
	enum CameraMode {
		Disabled,
		Flat,
		Free,
		Follow
	}

	CameraMode cameraMode = CameraMode.Flat;
	public float speed, rotSpeed;
	public GameObject canvas;

	void Update() {
		switch (cameraMode) {
			case CameraMode.Flat:
				transform.Translate(new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), Input.GetAxisRaw("UpDown")) * (Time.unscaledDeltaTime * speed));
				break;
			case CameraMode.Free:
				transform.Translate(new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("UpDown"), Input.GetAxisRaw("Vertical")) * (Time.unscaledDeltaTime * speed));
				transform.Rotate(Vector3.right * rotSpeed * -Input.GetAxisRaw("Mouse Y"));
				transform.Rotate(Vector3.up * rotSpeed * Input.GetAxisRaw("Mouse X"), Space.World);
				break;
		}

		if (Input.GetButtonDown("ChangeCamera")) {
			switch (cameraMode) {
				case CameraMode.Disabled:
					cameraMode = CameraMode.Flat;
					transform.localRotation = Quaternion.identity;
					break;
				case CameraMode.Flat:
					cameraMode = CameraMode.Free;
					Cursor.lockState = CursorLockMode.Locked;
					Cursor.visible = false;
					canvas.SetActive(false);
					break;
				case CameraMode.Free:
					cameraMode = CameraMode.Disabled;
					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = true;
					canvas.SetActive(true);
					break;
			}
		}
	}
}