using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {
	private readonly List<Selectable> _toDisableInBuildMode = new List<Selectable>();

	public Slider timeSpeed;
	public Button playButton;
	public Toggle participation;
	public InputField levelField;
	public Button editButton;

	public TrackMaker trackMaker;
	bool inBuildMode = false;
	public SimpleGameManager gameManager;

	public RectTransform menu;
	public RectTransform buildShortcuts;
	public RectTransform playButtonGroup;

	void Start() {
		_toDisableInBuildMode.AddRange(new Selectable[]
		{
			playButton, editButton, participation, timeSpeed
		});
	}

	void Update() {
		if (Input.GetButtonDown("Play")) {
			TogglePlayMode();
		}
	}

	public void TogglePlayMode() {
		if (!inBuildMode) {
			if (gameManager.InGame) {
				LeavePlayMode();
			} else {
				EnterPlayMode();
			}
		}
	}

	private void EnterPlayMode() {
		// ShowPlayModeInputs()
		playButton.GetComponentInChildren<Text>().text = "Stop";
		gameManager.EnterPlayMode(participation.isOn);
	}

	private void LeavePlayMode() {
		playButton.GetComponentInChildren<Text>().text = "Race";
		gameManager.LeavePlayMode();
	}

	public void LoadLevel() {
		trackMaker.BuildTrack(int.Parse(levelField.text));
	}

	public void ToggleBuildMode() {
		inBuildMode = !inBuildMode;
		if (inBuildMode) {
            editButton.GetComponentInChildren<Text>().text = "Save changes";
            trackMaker.EnterBuildMode();
		} else {
            editButton.GetComponentInChildren<Text>().text = "Custom track";
            trackMaker.LeaveBuildMode();
		}
	}

	private void DisplayUI() {
		foreach (var input in _toDisableInBuildMode) {
			input.enabled = false;
		}

		menu.gameObject.SetActive(false);
	}

	private void HideUI() {
		foreach (var input in _toDisableInBuildMode) {
			input.enabled = true;
		}

		menu.gameObject.SetActive(true);
	}
}
