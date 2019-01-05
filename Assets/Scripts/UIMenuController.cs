using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMenuController : MonoBehaviour
{
    public TrackMaker trackMaker;
    public SimpleGameManager gameManager;

    public Button playButton;
    public Button editButton;


    private bool _isHidden;

    void Start()
    {
        playButton.onClick.AddListener(EnterPlayMode);
        editButton.onClick.AddListener(EnterBuildMode);
    }

    void Update()
    {
        // TODO: improve handling input

        if (Input.GetButtonDown("EnterBuildMode"))
        {
            if (!_isHidden)
            {
                EnterBuildMode();
            }
            else
            {
                // Based on current logic in TrackMaker
                EnableAndShow();
            }
        }
    }

    private void EnterBuildMode()
    {
        DisableAndHide();

        // TODO: gib public "edit" method pls
        //trackMaker.EnterBuildMode()
    }

    private void EnterPlayMode()
    {
        DisableAndHide();

        // TODO: gib public "play" method pls
        //gameManager.EnterPlayMode()
    }

    public void DisableAndHide()
    {
        _isHidden = true;

        foreach (var button in GetComponentsInChildren<Button>())
        {
            button.enabled = false;
        }

        var canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        canvasGroup.alpha = 0;
    }


    // TODO: call from some sort of "back" or "restart" action
    public void EnableAndShow()
    {
        _isHidden = false;

        foreach (var button in GetComponentsInChildren<Button>())
        {
            button.enabled = true;
        }

        var canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        canvasGroup.alpha = 1;
    }
}
