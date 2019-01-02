using UnityEngine;

public class PreTrack : MonoBehaviour {
	public float trackWidth, wallWidth;

	void Start() {
		Mesh mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;
		Vector3[] vertices = new Vector3[16] { new Vector3(-5, trackWidth + wallWidth, -2 * wallWidth), new Vector3(-5, trackWidth - wallWidth, -2 * wallWidth), new Vector3(-5, trackWidth - wallWidth), new Vector3(-5, trackWidth + wallWidth), new Vector3(-5, -trackWidth - wallWidth, -2 * wallWidth), new Vector3(-5, -trackWidth + wallWidth, -2 * wallWidth), new Vector3(-5, -trackWidth + wallWidth), new Vector3(-5, -trackWidth - wallWidth), new Vector3(0, trackWidth + wallWidth, -2 * wallWidth), new Vector3(0, trackWidth - wallWidth, -2 * wallWidth), new Vector3(0, trackWidth - wallWidth), new Vector3(0, trackWidth + wallWidth), new Vector3(0, -trackWidth - wallWidth, -2 * wallWidth), new Vector3(0, -trackWidth + wallWidth, -2 * wallWidth), new Vector3(0, -trackWidth + wallWidth), new Vector3(0, -trackWidth - wallWidth) };
		int[] triangles = new int[72] { 0, 8, 9, 0, 9, 1, 1, 9, 10, 1, 10, 2, 3, 10, 11, 3, 2, 10, 0, 11, 8, 0, 3, 11, 13, 12, 4, 5, 13, 4, 14, 13, 5, 6, 14, 5, 15, 14, 7, 14, 6, 7, 12, 15, 4, 15, 7, 4, 0, 1, 2, 0, 2, 3, 10, 9, 8, 10, 8, 11, 6, 5, 4, 7, 6, 4, 12, 13, 14, 15, 12, 14 };
		EdgeCollider2D[] colliders = GetComponents<EdgeCollider2D>();
		colliders[0].edgeRadius = wallWidth;
		colliders[1].edgeRadius = wallWidth;
		colliders[0].points = new Vector2[2] { new Vector2(-5, trackWidth), new Vector2(0, trackWidth) };
		colliders[1].points = new Vector2[2] { new Vector2(-5, -trackWidth), new Vector2(0, -trackWidth) };
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
	}
}
