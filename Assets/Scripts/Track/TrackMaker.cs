using System.Collections.Generic;
using UnityEngine;

public class TrackMaker : MonoBehaviour {
	public TrackSegment segment;
	public float trackWidth, wallWidth;
	public int segmentCount;
	Stack<TrackSegment> pieceHistory = new Stack<TrackSegment>();
	float x, y, a, b, c;
	Camera cam;
	bool placeSucceeded = false;

	void Awake() {
		cam = Camera.main;
	}

	void Update() {
		if (!Input.GetButtonDown("Fire1") && placeSucceeded) {
			Remove();
		}
		if (!Input.GetButtonDown("Cancel")) {
			placeSucceeded = PlaceNew();
		}
	}

	public bool PlaceNew() {
		//Find coordinate on plane
		Ray forward = cam.ScreenPointToRay(Input.mousePosition);
		new Plane(Vector3.back, Vector3.zero).Raycast(forward, out float distance);
		Vector3 mouseLoc = forward.origin + forward.direction * distance;
		float x1 = mouseLoc.x;
		float y1 = mouseLoc.y;
		bool isRight = x1 - x > 0;

		//Create a new piece unless it's mathematically impossible
		if (Mathf.Abs(x - x1) < 1e-4) {
			return false;
		}
		float localSlope = FindSlope(a, b, x);
		SolveQuadratic(x, y, localSlope, x1, y1, out a, out b, out c);
		TrackSegment newPiece = Instantiate(segment, transform);
		newPiece.SetData(x, y, x1, y1, a, b, c);
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
		float antiA = float.NaN, antiB = float.NaN, wallDX = 0;
		//First iteration
		if (localSlope > 1e-4 || localSlope < -1e-4) {
			FindAntiSlope(x, y, localSlope, out antiA, out antiB);
			wallDX = (antiA > 0 ? 1 : -1) / Mathf.Sqrt(antiA * antiA + 1);
		}
		//Left side
		FindLeftEdges(isRight, x, y, localSlope, wallDX, antiA, antiB, out float colliderX, out float outerWallX, out float innerWallX, out float colliderY, out float outerWallY, out float innerWallY);
		leftColliderPoints[0] = new Vector2(colliderX, colliderY);
		vertices[0] = new Vector3(outerWallX, outerWallY, -2 * wallWidth);
		vertices[1] = new Vector3(innerWallX, innerWallY, -2 * wallWidth);
		vertices[2] = new Vector3(innerWallX, innerWallY, 0);
		vertices[3] = new Vector3(outerWallX, outerWallY, 0);
		//Right side
		FindRightEdges(isRight, x, y, localSlope, wallDX, antiA, antiB, out colliderX, out outerWallX, out innerWallX, out colliderY, out outerWallY, out innerWallY);
		rightColliderPoints[0] = new Vector2(colliderX, colliderY);
		vertices[4] = new Vector3(outerWallX, outerWallY, -2 * wallWidth);
		vertices[5] = new Vector3(innerWallX, innerWallY, -2 * wallWidth);
		vertices[6] = new Vector3(innerWallX, innerWallY, 0);
		vertices[7] = new Vector3(outerWallX, outerWallY, 0);
		//Subsequent iterations
		for (float wallInterval = (x1 - x) / segmentCount; i < segmentCount; i++) {
			int i8 = i * 8;
			int i48 = i8 * 6;
			x += wallInterval;
			y = a * x * x + b * x + c;

			//Find wall and collider points
			localSlope = FindSlope(a, b, x);
			if (localSlope > 1e-4 || localSlope < -1e-4) {
				FindAntiSlope(x, y, localSlope, out antiA, out antiB);
				wallDX = (antiA > 0 ? 1 : -1) / Mathf.Sqrt(antiA * antiA + 1);
			}
			//Left side
			FindLeftEdges(isRight, x, y, localSlope, wallDX, antiA, antiB, out colliderX, out outerWallX, out innerWallX, out colliderY, out outerWallY, out innerWallY);
			leftColliderPoints[i + 1] = new Vector2(colliderX, colliderY);
			vertices[i8 + 8] = new Vector3(outerWallX, outerWallY, -2 * wallWidth);
			vertices[i8 + 9] = new Vector3(innerWallX, innerWallY, -2 * wallWidth);
			vertices[i8 + 10] = new Vector3(innerWallX, innerWallY, 0);
			vertices[i8 + 11] = new Vector3(outerWallX, outerWallY, 0);
			int[] indices = new int[] { 0, 8, 9, 0, 9, 1, 1, 9, 10, 1, 10, 2, 3, 10, 11, 3, 2, 10, 0, 11, 8, 0, 3, 11, 13, 12, 4, 5, 13, 4, 14, 13, 5, 6, 14, 5, 15, 14, 7, 14, 6, 7, 12, 15, 4, 15, 7, 4 };
			for (int index = 0; index < 24; index++) {
				triangles[i48 + index] = i8 + indices[index];
			}
			//Right side
			FindRightEdges(isRight, x, y, localSlope, wallDX, antiA, antiB, out colliderX, out outerWallX, out innerWallX, out colliderY, out outerWallY, out innerWallY);
			rightColliderPoints[i + 1] = new Vector2(colliderX, colliderY);
			vertices[i8 + 12] = new Vector3(outerWallX, outerWallY, -2 * wallWidth);
			vertices[i8 + 13] = new Vector3(innerWallX, innerWallY, -2 * wallWidth);
			vertices[i8 + 14] = new Vector3(innerWallX, innerWallY, 0);
			vertices[i8 + 15] = new Vector3(outerWallX, outerWallY, 0);
			for (int index = 24; index < 48; index++) {
				triangles[i48 + index] = i8 + indices[index];
			}
		}

		//Update last values
		x = x1;
		y = y1;
		segmentColliders[0].points = leftColliderPoints;
		segmentColliders[1].points = rightColliderPoints;
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		return true;
	}

	public bool Remove() {
		//Destroy the last piece on the stack and revert to its values
		if (pieceHistory.Count > 0) {
			TrackSegment lastPiece = pieceHistory.Pop();
			lastPiece.GetData(out x, out y, out a, out b, out c);
			Destroy(lastPiece.gameObject);
			return true;
		}

		//Stack is empty, use default values
		x = 0;
		y = 0;
		a = 0;
		b = 0;
		c = 0;
		return false;
	}

	void FindLeftEdges(bool isRight, float x, float y, float localSlope, float wallDX, float antiA, float antiB, out float colliderX, out float outerWallX, out float innerWallX, out float colliderY, out float outerWallY, out float innerWallY) {
		if (localSlope > 1e-4 || localSlope < -1e-4) {
			colliderX = isRight ? x + wallDX * trackWidth : x - wallDX * trackWidth;
			outerWallX = isRight ? x + wallDX * (trackWidth + wallWidth) : x - wallDX * (trackWidth + wallWidth);
			innerWallX = isRight ? x + wallDX * (trackWidth - wallWidth) : x - wallDX * (trackWidth - wallWidth);
			colliderY = antiA * colliderX + antiB;
			outerWallY = antiA * outerWallX + antiB;
			innerWallY = antiA * innerWallX + antiB;
		} else {
			colliderX = x;
			outerWallX = x;
			innerWallX = x;
			colliderY = isRight ? y + trackWidth : y - trackWidth;
			outerWallY = isRight ? colliderY + wallWidth : colliderY - wallWidth;
			innerWallY = isRight ? colliderY - wallWidth : colliderY + wallWidth;
		}
	}

	void FindRightEdges(bool isRight, float x, float y, float localSlope, float wallDX, float antiA, float antiB, out float colliderX, out float outerWallX, out float innerWallX, out float colliderY, out float outerWallY, out float innerWallY) {
		if (localSlope > 1e-4 || localSlope < -1e-4) {
			colliderX = isRight ? x - wallDX * trackWidth : x + wallDX * trackWidth;
			outerWallX = isRight ? x - wallDX * (trackWidth + wallWidth) : x + wallDX * (trackWidth + wallWidth);
			innerWallX = isRight ? x - wallDX * (trackWidth - wallWidth) : x + wallDX * (trackWidth - wallWidth);
			colliderY = antiA * colliderX + antiB;
			outerWallY = antiA * outerWallX + antiB;
			innerWallY = antiA * innerWallX + antiB;
		} else {
			colliderX = x;
			outerWallX = x;
			innerWallX = x;
			colliderY = isRight ? y - trackWidth : y + trackWidth;
			outerWallY = isRight ? colliderY - wallWidth : colliderY + wallWidth;
			innerWallY = isRight ? colliderY + wallWidth : colliderY - wallWidth;
		}
	}

	static float FindSlope(float a, float b, float x) {
		return 2 * a * x + b;
	}

	static void FindAntiSlope(float x, float y, float slope, out float a, out float b) {
		a = -1 / slope;
		b = y + x / slope;
	}

    static void SolveQuadratic(float x0, float y0, float y, float x1, float y1, out float a, out float b, out float c) {
		float dx = x0 - x1;
		float dy = y0 - y1;
		float d = dx * dx;
		float x02 = x0 * x0;
		float x12 = x1 * x1;
		a = (y * dx - dy) / d;
		b = (2 * x0 * dy - y * (x02 - x12)) / d;
		c = (x02 * (x1 * y + y1) - x0 * x1 * (x1 * y + 2 * y0) + x12 * y0) / d;
	}
}
