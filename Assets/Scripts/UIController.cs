using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {
	public Slider timeSpeed;
	public Button playButton;
	public Toggle wallToggle;
	public InputField levelField;
	public Button editButton;
    public Text generationText;
	public Text collapseButtonText;

	public TrackMaker trackMaker;
	bool inBuildMode = false;
	bool menuVisible = true;
	public GameManager gameManager;

	public RectTransform menu;

	public void UpdateInfoPanel() {
		generationText.text = $"Generation: {gameManager.Generation}";
	}

	public void SetDeadlyWalls() {
		CarController.wallsAreDeadly = wallToggle.isOn;
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
		playButton.GetComponentInChildren<Text>().text = "Stop";
		gameManager.EnterPlayMode();
	}

	private void LeavePlayMode() {
		playButton.GetComponentInChildren<Text>().text = "Start training";
		gameManager.LeavePlayMode();
	}

	public void LoadLevel() {
		trackMaker.BuildTrack(int.Parse(levelField.text));
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
			//menu.localPosition = Vector3.zero;
		} else {
			collapseButtonText.text = ">";
			menu.anchoredPosition = new Vector2(-360, 0);
			//menu.localPosition = new Vector3(-360, 0);
		}
	}
}
