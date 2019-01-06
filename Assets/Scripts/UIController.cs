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
		playButton.onClick.AddListener(EnterPlayMode);
		levelButton.onClick.AddListener(DisplayLevelSelectionMenu);
		editButton.onClick.AddListener(EnterBuildMode);

		_toDisableInBuildMode.AddRange(new Selectable[]
		{
			playButton, levelButton, editButton, participation, timeSpeed
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

		if (Input.GetButtonDown("EnterBuildMode") && (gameManager == null || !gameManager.InGame)) {
			if (inBuildMode) {
				HideUiForBuildMode();
				trackMaker.LeaveBuildMode();
			} else {
				EnterBuildMode();
			}
		}
	}

	private void EnterPlayMode() {
		// ShowPlayModeInputs()

		gameManager.EnterPlayMode(participation.isOn);
	}

	private void DisplayLevelSelectionMenu() {
		//TODO: stuff
	}

	private void EnterBuildMode() {
		DisplayUiForBuildMode();
		trackMaker.EnterBuildMode();
	}



	private void DisplayUiForBuildMode() {
		foreach (var input in _toDisableInBuildMode) {
			input.enabled = false;
		}

		menu.alpha = 0;
		menu.interactable = false;

		buildShortcuts.alpha = 1;
	}

	private void HideUiForBuildMode() {
		foreach (var input in _toDisableInBuildMode) {
			input.enabled = true;
		}

		menu.alpha = 1;
		menu.interactable = true;

		buildShortcuts.alpha = 0;
	}
}
