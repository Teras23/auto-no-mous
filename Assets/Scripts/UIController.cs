using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {
	public Slider timeSpeed;
	public Button playButton;
	public Toggle wallToggle;
	public InputField levelField;
	public Button editButton;
	public Text generationText;
	public Text bestTimeText;
	public Text bestScoreText;
	public Text collapseButtonText;

	public InputField carCountField;
	public InputField hiddenLayerField;
	public InputField layer1Field;
	public InputField layer2Field;
	public InputField layer3Field;

	public TrackMaker trackMaker;
	bool inBuildMode = false;
	bool menuVisible = true;
	public GameManager gameManager;
	public NeuralNetwork carPrefab;

	public RectTransform menu;
	public GameObject networkGroup;

	public void UpdateCarCount() {
		if (int.TryParse(carCountField.text, out int carCount)) {
			if (carCount > 0 && carCount <= 200) {
				gameManager.nrOfCars = carCount;
			}
		}
	}

	public void UpdateNetworkValues() {
		if (!int.TryParse(hiddenLayerField.text, out int hiddenLayers)) {
			return;
		}
		if (hiddenLayers < 0) {
			hiddenLayers = 0;
		} else if (hiddenLayers > 3) {
			hiddenLayers = 3;
		}

		if (!int.TryParse(layer1Field.text, out int layer1Size) && hiddenLayers >= 1) {
			return;
		}
		if (!int.TryParse(layer2Field.text, out int layer2Size) && hiddenLayers >= 2) {
			return;
		}
		if (!int.TryParse(layer3Field.text, out int layer3Size) && hiddenLayers >= 3) {
			return;
		}

		switch (hiddenLayers) {
			case 0:
				carPrefab.hiddenLayers = new int[0];
				break;
			case 1:
				carPrefab.hiddenLayers = new int[] { layer1Size };
				break;
			case 2:
				carPrefab.hiddenLayers = new int[] { layer1Size, layer2Size };
				break;
			case 3:
				carPrefab.hiddenLayers = new int[] { layer1Size, layer2Size, layer3Size };
				break;
		}
	}

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
			if (gameManager.started) {
				LeavePlayMode();
			} else {
				EnterPlayMode();
			}
		}
	}

	private void EnterPlayMode() {
		playButton.GetComponentInChildren<Text>().text = "Stop";
		networkGroup.SetActive(false);
		gameManager.EnterPlayMode();
	}

	private void LeavePlayMode() {
		playButton.GetComponentInChildren<Text>().text = "Start training";
		networkGroup.SetActive(true);
		gameManager.LeavePlayMode();
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
