using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMenuController : MonoBehaviour
{
    private CustomTrackMaker _customTrackMaker;
    private SimpleGameManager _gameManager;

    public Button playButton;
    public Button levelButton;
    public Button editButton;
    public Toggle participation;
    public Slider timeSpeed;

    public CanvasGroup uiBuildShortcuts;

    private List<Selectable> _toDisableInBuildMode = new List<Selectable>();


    private bool _isActive;

    void Start()
    {
        _customTrackMaker = FindObjectOfType<CustomTrackMaker>();
        _gameManager = FindObjectOfType<SimpleGameManager>();

        playButton.onClick.AddListener(EnterPlayMode);
        levelButton.onClick.AddListener(DisplayLevelSelectionMenu);
        editButton.onClick.AddListener(EnterBuildMode);

        _toDisableInBuildMode.AddRange(new List<Selectable>
        {
            playButton, levelButton, editButton, participation, timeSpeed
        });
    }

    void Update()
    {
        // TODO: improve handling input

        if (Input.GetButtonDown("EnterBuildMode"))
        {
            if (_customTrackMaker.InBuildMode)
            {
                LeaveBuildMode();
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

        // TODO: gib public "play" method pls
        //gameManager.EnterPlayMode()
    }

    private void LeavePlayMode()
    {
        // HidePlayModeInputs()

        //gameManager.LeavePlayMode()
    }

    private void DisplayLevelSelectionMenu()
    {

    }

    private void HideLevelSelectionMenu()
    {

    }

    private void EnterBuildMode()
    {
        DisplayUiForBuildMode();

        _customTrackMaker.EnterBuildMode();
    }

    private void LeaveBuildMode()
    {
        HideUiForBuildMode();

        _customTrackMaker.CommitAndLeaveBuildMode();
    }

    public void DisplayUiForBuildMode()
    {
        _isActive = false;

        foreach (var input in _toDisableInBuildMode)
        {
            input.enabled = false;
        }

        var menu = GetComponent<CanvasGroup>();
        menu.alpha = 0;
        menu.interactable = false;

        uiBuildShortcuts.alpha = 1;
    }


    // TODO: call from some sort of "back" or "restart" action
    public void HideUiForBuildMode()
    {
        _isActive = true;

        foreach (var input in _toDisableInBuildMode)
        {
            input.enabled = true;
        }

        var menu = GetComponent<CanvasGroup>();
        menu.alpha = 1;
        menu.interactable = true;

        uiBuildShortcuts.alpha = 0;
    }
}
