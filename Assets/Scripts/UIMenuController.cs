using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMenuController : MonoBehaviour
{
    public TrackMaker trackMaker;
    public SimpleGameManager gameManager;

    public Button playButton;
    public Button editButton;
    public Toggle participation;
    public Slider timeSpeed;

    private List<Selectable> _toDisableInGame = new List<Selectable>();


    private bool _isActive;

    void Start()
    {
        playButton.onClick.AddListener(EnterPlayMode);
        editButton.onClick.AddListener(EnterBuildMode);

        _toDisableInGame.Add(participation);
    }

    void Update()
    {
        // TODO: improve handling input

        if (Input.GetButtonDown("EnterBuildMode"))
        {
            if (!_isActive)
            {
                EnterBuildMode();
            }
            else
            {
                // Based on current logic in TrackMaker
                EnableConfiguration();
            }
        }
    }

    private void EnterBuildMode()
    {
        DisableConfiguration();

        // TODO: gib public "edit" method pls
        //trackMaker.EnterBuildMode()
    }

    private void EnterPlayMode()
    {
        DisableConfiguration();

        // TODO: gib public "play" method pls
        //gameManager.EnterPlayMode()
    }

    public void DisableConfiguration()
    {
        _isActive = false;

        foreach (var input in _toDisableInGame)
        {
            input.enabled = false;
        }

        // canvasGroup.alpha = 0.8f;
    }


    // TODO: call from some sort of "back" or "restart" action
    public void EnableConfiguration()
    {
        _isActive = true;

        foreach (var input in _toDisableInGame)
        {
            input.enabled = true;
        }

        var canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1;
    }
}
