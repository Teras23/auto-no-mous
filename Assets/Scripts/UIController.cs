using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    private CustomTrackMaker _customTrackMaker;
    private SimpleGameManager _gameManager;
    private readonly List<Selectable> _toDisableInBuildMode = new List<Selectable>();

    public Button playButton;
    public Button levelButton;
    public Button editButton;
    public Toggle participation;
    public Slider timeSpeed;

    public CanvasGroup buildShortcuts;
    public CanvasGroup playButtonGroup;

    void Start()
    {
        _customTrackMaker = FindObjectOfType<CustomTrackMaker>();
        _gameManager = FindObjectOfType<SimpleGameManager>();

        playButton.onClick.AddListener(EnterPlayMode);
        levelButton.onClick.AddListener(DisplayLevelSelectionMenu);
        editButton.onClick.AddListener(EnterBuildMode);

        _toDisableInBuildMode.AddRange(new Selectable[]
        {
            playButton, levelButton, editButton, participation, timeSpeed
        });
    }

    void Update()
    {
        if (Input.GetButtonDown("Play") && !_customTrackMaker.InBuildMode)
        {
            if (_gameManager.InGame)
            {
                // HideUiForPlayMode()
                _gameManager.LeavePlayMode();
            }
            else
            {
                EnterPlayMode();
            }
        }

        if (Input.GetButtonDown("EnterBuildMode") && !_gameManager.InGame)
        {
            if (_customTrackMaker.InBuildMode)
            {
                HideUiForBuildMode();
                _customTrackMaker.CommitAndLeaveBuildMode();
            }
            else
            {
                EnterBuildMode();
            }
        }
    }

    private void EnterPlayMode()
    {
        // ShowPlayModeInputs()

        _gameManager.EnterPlayMode(participation.isOn);
    }

    private void DisplayLevelSelectionMenu()
    {
        //TODO: stuff
    }

    private void EnterBuildMode()
    {
        DisplayUiForBuildMode();
        _customTrackMaker.EnterBuildMode();
    }



    private void DisplayUiForBuildMode()
    {
        foreach (var input in _toDisableInBuildMode)
        {
            input.enabled = false;
        }

        var menu = GetComponent<CanvasGroup>();
        menu.alpha = 0;
        menu.interactable = false;

        buildShortcuts.alpha = 1;
    }

    private void HideUiForBuildMode()
    {
        foreach (var input in _toDisableInBuildMode)
        {
            input.enabled = true;
        }

        var menu = GetComponent<CanvasGroup>();
        menu.alpha = 1;
        menu.interactable = true;

        buildShortcuts.alpha = 0;
    }
}
