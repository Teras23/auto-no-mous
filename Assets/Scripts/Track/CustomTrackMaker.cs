using System.Collections;
using UnityEngine;

public class CustomTrackMaker : TrackMaker {
	Camera cam;
    private bool inBuildMode;

    public bool InBuildMode => inBuildMode;

	void Awake() {
		cam = Camera.main;
	}

    public void EnterBuildMode()
    {
        inBuildMode = !inBuildMode;
        MousePlace();
        StartCoroutine(BuildMode());
    }

    public void CommitAndLeaveBuildMode()
    {
        inBuildMode = !inBuildMode;
        StopAllCoroutines();
        Remove();
    }

	//void Update() {
	//	if (Input.GetButtonDown("EnterBuildMode")) {
	//		if (inBuildMode) {
	//			StopAllCoroutines();
	//			Remove();
	//		} else {
	//			MousePlace();
	//			StartCoroutine(BuildMode());
	//		}
	//		inBuildMode = !inBuildMode;
	//	}
	//}

	IEnumerator BuildMode() {
		while (true) {
			if (!Input.GetButtonDown("Fire1")) {
				Remove();
			}
			if (!Input.GetButtonDown("Cancel")) {
				MousePlace();
			}
			yield return null;
		}
	}

	void MousePlace() {
		//Find coordinate on plane
		Ray forward = cam.ScreenPointToRay(Input.mousePosition);
		new Plane(Vector3.back, Vector3.zero).Raycast(forward, out float distance);
		Vector3 mouseLoc = forward.origin + forward.direction * distance;
		Debug.Log(mouseLoc); //TODO: Create pre-built tracks from these locations
		PlaceNew(mouseLoc.x, mouseLoc.y);
	}
}
