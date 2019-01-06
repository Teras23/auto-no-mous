using UnityEngine;

public class PremadeTrackMaker : TrackMaker {
	//This is a hack solution, but it's so much better to do this stuff in the editor
	[System.Serializable]
	struct Track {
		public Vector2[] points;
	}

	[SerializeField]
	#pragma warning disable IDE0044 // Add readonly modifier
	Track[] tracks;
	#pragma warning restore IDE0044 // Add readonly modifier

	void BuildTrack(int trackNumber) {
		RemoveAll();
		foreach (Vector2 point in tracks[trackNumber].points) {
			PlaceNew(point.x, point.y);
		}
	}
}
