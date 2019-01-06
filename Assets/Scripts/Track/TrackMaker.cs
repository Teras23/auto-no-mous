using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackMaker : MonoBehaviour {
	public TrackSegment segment;
	public Checkpoint checkpoint;
	public float trackWidth, wallWidth, checkpointWidth;
	public int segmentCount;
	Stack<TrackSegment> pieceHistory = new Stack<TrackSegment>();
	float x, y, ax = 10, bx, cx = -10, ay, by, cy;
	Camera cam;
	int checkpointCounter = 1;
	//This is a hack solution, but it's so much better to do this stuff in the editor
	[System.Serializable]
	struct Track {
		public Vector2[] points;
	}

	[SerializeField]
	#pragma warning disable IDE0044 //Add readonly modifier
	Track[] tracks;
	#pragma warning restore IDE0044 //Add readonly modifier

	void Awake() {
		cam = Camera.main;
	}

	public void EnterBuildMode() {
		MousePlace();
		StartCoroutine(BuildMode());
	}

	public void LeaveBuildMode() {
		StopAllCoroutines();
		Remove();
	}

	IEnumerator BuildMode() {
		while (true) {
			if (!Input.GetButtonDown("Fire1")) {
				Remove();
			} else { //TODO: Temporary
				Ray forward = cam.ScreenPointToRay(Input.mousePosition);
				new Plane(Vector3.back, Vector3.zero).Raycast(forward, out float distance);
				Vector3 mouseLoc = forward.origin + forward.direction * distance;
				Debug.Log(mouseLoc); //TODO: Create pre-built tracks from these locations
			}
			if (!Input.GetButtonDown("Cancel")) {
				MousePlace();
			}
			yield return null;
		}
	}

	public void BuildTrack(int trackNumber) {
		if (trackNumber >= 0 && trackNumber < tracks.Length) {
			RemoveAll();
			foreach (Vector2 point in tracks[trackNumber].points) {
				PlaceNew(point.x, point.y);
			}
		}
	}

	void MousePlace() {
		//Find coordinate on plane
		Ray forward = cam.ScreenPointToRay(Input.mousePosition);
		new Plane(Vector3.back, Vector3.zero).Raycast(forward, out float distance);
		Vector3 mouseLoc = forward.origin + forward.direction * distance;
		PlaceNew(mouseLoc.x, mouseLoc.y);
	}

	void PlaceNew(float x1, float y1) {
		//Create a new piece
		float xSlope = FindSlope(ax, bx, 1);
		float ySlope = FindSlope(ay, by, 1);
		SolveQuadratic(xSlope, x, x1, out ax, out bx, out cx);
		SolveQuadratic(ySlope, y, y1, out ay, out by, out cy);
		TrackSegment newPiece = Instantiate(segment, transform);
		newPiece.SetData(x1, y1, ax, bx, cx, ay, by, cy);
		pieceHistory.Push(newPiece);

		//Find segment locations along the path and construct a mesh and colliders
		Mesh mesh = new Mesh();
		newPiece.GetComponent<MeshFilter>().mesh = mesh;
		Vector3[] vertices = new Vector3[segmentCount * 8 + 8];
		int[] triangles = new int[segmentCount * 48];
		EdgeCollider2D[] segmentColliders = newPiece.GetComponents<EdgeCollider2D>();
		segmentColliders[0].edgeRadius = wallWidth;
		segmentColliders[1].edgeRadius = wallWidth;
		Vector2[] leftColliderPoints = new Vector2[segmentCount + 1];
		Vector2[] rightColliderPoints = new Vector2[segmentCount + 1];
		int i = 0;

		//First iteration
		//Left side
		FindLeftEdges(x, y, xSlope, ySlope, out Vector2 collider, out Vector3 outerWall, out Vector3 innerWall);
		leftColliderPoints[0] = collider;
		vertices[0] = outerWall - new Vector3(0, 0, 2 * wallWidth);
		vertices[1] = innerWall - new Vector3(0, 0, 2 * wallWidth);
		vertices[2] = innerWall;
		vertices[3] = outerWall;
		//Right side
		FindRightEdges(x, y, xSlope, ySlope, out collider, out outerWall, out innerWall);
		rightColliderPoints[0] = collider;
		vertices[4] = outerWall - new Vector3(0, 0, 2 * wallWidth);
		vertices[5] = innerWall - new Vector3(0, 0, 2 * wallWidth);
		vertices[6] = innerWall;
		vertices[7] = outerWall;
		//Subsequent iterations
		for (float t = 0, wallInterval = 1f / segmentCount; i < segmentCount; i++) {
			int i8 = i * 8;
			int i48 = i8 * 6;
			t += wallInterval;
			//Debug.DrawLine(new Vector3(x, y), new Vector3((ax * t + bx) * t + cx, (ay * t + by) * t + cy), Color.red);
			x = (ax * t + bx) * t + cx;
			y = (ay * t + by) * t + cy;

			//Find wall and collider points
			xSlope = FindSlope(ax, bx, t);
			ySlope = FindSlope(ay, by, t);
			//Left side
			FindLeftEdges(x, y, xSlope, ySlope, out collider, out outerWall, out innerWall);
			leftColliderPoints[i + 1] = collider;
			vertices[i8 + 8] = outerWall - new Vector3(0, 0, 2 * wallWidth);
			vertices[i8 + 9] = innerWall - new Vector3(0, 0, 2 * wallWidth);
			vertices[i8 + 10] = innerWall;
			vertices[i8 + 11] = outerWall;
			int[] indices = new int[] { 0, 8, 9, 0, 9, 1, 1, 9, 10, 1, 10, 2, 3, 10, 11, 3, 2, 10, 0, 11, 8, 0, 3, 11, 13, 12, 4, 5, 13, 4, 14, 13, 5, 6, 14, 5, 15, 14, 7, 14, 6, 7, 12, 15, 4, 15, 7, 4 };
			for (int index = 0; index < 24; index++) {
				triangles[i48 + index] = i8 + indices[index];
			}
			//Right side
			FindRightEdges(x, y, xSlope, ySlope, out collider, out outerWall, out innerWall);
			rightColliderPoints[i + 1] = collider;
			vertices[i8 + 12] = outerWall - new Vector3(0, 0, 2 * wallWidth);
			vertices[i8 + 13] = innerWall - new Vector3(0, 0, 2 * wallWidth);
			vertices[i8 + 14] = innerWall;
			vertices[i8 + 15] = outerWall;
			for (int index = 24; index < 48; index++) {
				triangles[i48 + index] = i8 + indices[index];
			}

			//Add checkpoints
			Checkpoint cp = Instantiate(checkpoint, newPiece.transform);
			EdgeCollider2D cpCollider = cp.GetComponent<EdgeCollider2D>();
			cpCollider.edgeRadius = checkpointWidth;
			cpCollider.points = new Vector2[] { leftColliderPoints[i + 1], rightColliderPoints[i + 1] };
			cp.order = checkpointCounter++;
		}

		//Update last values
		segmentColliders[0].points = leftColliderPoints;
		segmentColliders[1].points = rightColliderPoints;
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		x = x1;
		y = y1;
	}

	void Remove() {
		//Destroy the last piece on the stack and revert to the values before that
		if (pieceHistory.Count > 0) {
			TrackSegment lastPiece = pieceHistory.Pop();
			Destroy(lastPiece.gameObject);
			checkpointCounter -= segmentCount;
			if (pieceHistory.Count > 0) {
				lastPiece = pieceHistory.Peek();
				lastPiece.GetData(out x, out y, out ax, out bx, out cx, out ay, out by, out cy);
				return;
			}
		}

		//Stack is empty, use default values
		x = 0;
		y = 0;
		ax = 10;
		bx = 0;
		cx = -10;
		ay = 0;
		by = 0;
		cy = 0;
	}

	void RemoveAll() {
		while (pieceHistory.Count > 0) {
			Remove();
		}
	}

	void FindLeftEdges(float x, float y, float xSlope, float ySlope, out Vector2 collider, out Vector3 outerWall, out Vector3 innerWall) {
		Vector2 direction = new Vector2(-ySlope, xSlope).normalized;
		collider = new Vector2(x, y) + direction * trackWidth;
		outerWall = collider + direction * wallWidth;
		innerWall = collider - direction * wallWidth;
	}

	void FindRightEdges(float x, float y, float xSlope, float ySlope, out Vector2 collider, out Vector3 outerWall, out Vector3 innerWall) {
		Vector2 direction = new Vector2(ySlope, -xSlope).normalized;
		collider = new Vector2(x, y) + direction * trackWidth;
		outerWall = collider + direction * wallWidth;
		innerWall = collider - direction * wallWidth;
	}

	static float FindSlope(float a, float b, float t) {
		return 2 * a * t + b;
	}

	static void SolveQuadratic(float slope, float x0, float x1, out float a, out float b, out float c) {
		c = x0;
		b = slope;
		a = x1 - x0 - slope;
	}
}
