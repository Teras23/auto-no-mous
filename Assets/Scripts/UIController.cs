using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {
	private readonly List<Selectable> _toDisableInBuildMode = new List<Selectable>();

	public Button playButton;
	public Button levelButton;
	public Button editButton;
	public Toggle participation;
	public Slider timeSpeed;

	public TrackMaker trackMaker;
	bool inBuildMode = false;
	public SimpleGameManager gameManager;

	public CanvasGroup menu;
	public CanvasGroup buildShortcuts;
	public CanvasGroup playButtonGroup;

	void Start() {
		_toDisableInBuildMode.AddRange(new Selectable[]
		{
			playButton, editButton, participation, timeSpeed
		});
	}

	void Update() {
		if (gameManager != null && Input.GetButtonDown("Play") && !inBuildMode) {
			if (gameManager.InGame) {
				// HideUiForPlayMode()
				gameManager.LeavePlayMode();
			} else {
				EnterPlayMode();
			}
		}
	}

	private void EnterPlayMode() {
		// ShowPlayModeInputs()
		gameManager.EnterPlayMode(participation.isOn);
	}

	public void ToggleBuildMode() {
		inBuildMode = !inBuildMode;
		if (inBuildMode) {
			DisplayUiForBuildMode();
			trackMaker.EnterBuildMode();
		} else {
			HideUiForBuildMode();
			trackMaker.LeaveBuildMode();
		}
	}

	private void DisplayUiForBuildMode() {
		foreach (var input in _toDisableInBuildMode) {
			input.enabled = false;
		}

		menu.alpha = 0;
		menu.interactable = false;

		//buildShortcuts.alpha = 1;
	}

	private void HideUiForBuildMode() {
		foreach (var input in _toDisableInBuildMode) {
			input.enabled = true;
		}

		menu.alpha = 1;
		menu.interactable = true;

		//buildShortcuts.alpha = 0;
	}
}
