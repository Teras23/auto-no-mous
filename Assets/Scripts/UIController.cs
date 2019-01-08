using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {
	public Slider timeSpeed;
	public Button playButton;
	public Toggle wallToggle;
	public InputField levelField;
	public InputField importExportField;
	public GameObject importExportPanel;
	public Button editButton;
	public Text generationText;
	public Text bestTimeText;
	public Text bestScoreText;
	public Text collapseButtonText;

	public TrackMaker trackMaker;
	bool inBuildMode = false;
	bool menuVisible = true;
	public GameManager gameManager;

	public RectTransform menu;

	public void UpdateInfoPanel(int genNr, CarController bestCar) {
		generationText.text = $"Generation: {genNr}";
		var hasFinished = bestCar.points == trackMaker.checkpointCounter - 1;
		var scoreText = hasFinished ? $"{bestCar.points} (max)" : $"{bestCar.points}/{trackMaker.checkpointCounter - 1}";
		var timeText = hasFinished ? $"{bestCar.TotalTime:F1}s" : "None finished";

		bestScoreText.text = $"Best score: {scoreText}";
		bestTimeText.text = $"Best time: {timeText}";
	}

	public void UpdateInfoPanel(int genNr) {
		generationText.text = $"Generation: {genNr}";
		bestScoreText.text = $"Best score: -";
		bestTimeText.text = $"Best time: -";
	}

	public void SetDeadlyWalls() {
		CarController.wallsAreDeadly = wallToggle.isOn;
	}

	public void TogglePlayMode() {
		if (!inBuildMode) {
			if (gameManager.inGame) {
				LeavePlayMode();
			} else {
				EnterPlayMode();
			}
		}
	}

	private void EnterPlayMode() {
		playButton.GetComponentInChildren<Text>().text = "Stop";
		gameManager.EnterPlayMode();
	}

	private void LeavePlayMode() {
		playButton.GetComponentInChildren<Text>().text = "Start training";
		gameManager.LeavePlayMode();
	}

	public void Import() {
		try {
			gameManager.Import(importExportField.text);			
		}
		catch (JsonReaderException e) {
			importExportField.text = "Invalid neural network JSON!";
		}
	}

	public void Export() {
		var export = gameManager.ExportBest();

		if (export == null) {
			importExportField.text = "No best car yet!";
		}
		else {
			importExportField.text = export;
		}

		// Copying to clipboard
		try {
			TextEditor te = new TextEditor();
			te.text = export;
			te.SelectAll();
			te.Copy();
		}
		catch (Exception e) {
			
		}
	}

	public void ToggleImportExport() {
		importExportPanel.SetActive(!importExportPanel.activeSelf);
	}

	public void RemoveCustomCar() {
		gameManager.RemoveCustomCar();
	}
	
	public void LoadLevel() {		
		trackMaker.BuildTrack(int.Parse(levelField.text));
		if (gameManager.started)
			gameManager.Restart();
	}

	public void ToggleBuildMode() {
		inBuildMode = !inBuildMode;
		if (inBuildMode) {
			editButton.GetComponentInChildren<Text>().text = "Save & exit build";
			trackMaker.EnterBuildMode();
		} else {
			editButton.GetComponentInChildren<Text>().text = "Enter build mode";
			trackMaker.LeaveBuildMode();
		}
	}

	public void ToggleUI() {
		menuVisible = !menuVisible;
		if (menuVisible) {
			collapseButtonText.text = "<";
			menu.anchoredPosition = Vector2.zero;
		} else {
			collapseButtonText.text = ">";
			menu.anchoredPosition = new Vector2(-360, 0);
		}
	}
}
