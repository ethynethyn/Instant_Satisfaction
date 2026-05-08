using UnityEngine;
using System.Collections.Generic;

public class CharacterCustomisation : MonoBehaviour
{
    [Header("Elements")]
    public List<CustomisationElement> elements;

    private int selectedElementIndex = 0;
    private PlayerController2D controller;

    void Start()
    {
        controller = GetComponent<PlayerController2D>();

        foreach (CustomisationElement element in elements)
        {
            if (element != null)
                element.Initialise();
        }

        if (elements.Count > 0)
            elements[selectedElementIndex].SetHighlight(true);
    }

    void Update()
    {
        if (!gameObject.activeSelf)
            return;

        if (controller == null)
            return;

        // Check if customization is locked
        if (CustomisationManager.Instance.IsCustomisationLocked())
            return;

        HandleInput();
    }

    void HandleInput()
    {
        // Move selection UP
        if (Input.GetKeyDown(controller.jumpKey))
            MoveElementSelection(-1);

        // Move selection DOWN
        if (Input.GetKeyDown(controller.dashKey))
            MoveElementSelection(1);

        // Previous option
        if (Input.GetKeyDown(controller.moveLeft))
        {
            if (elements.Count > 0)
                elements[selectedElementIndex].Previous();
        }

        // Next option
        if (Input.GetKeyDown(controller.moveRight))
        {
            if (elements.Count > 0)
                elements[selectedElementIndex].Next();
        }
    }

    void MoveElementSelection(int direction)
    {
        if (elements.Count == 0)
            return;

        elements[selectedElementIndex].SetHighlight(false);

        selectedElementIndex += direction;

        if (selectedElementIndex >= elements.Count)
            selectedElementIndex = 0;

        if (selectedElementIndex < 0)
            selectedElementIndex = elements.Count - 1;

        elements[selectedElementIndex].SetHighlight(true);
    }

    public void ResetSelections()
    {
        selectedElementIndex = 0;

        foreach (CustomisationElement element in elements)
        {
            if (element != null)
                element.Initialise();
        }

        if (elements.Count > 0)
            elements[selectedElementIndex].SetHighlight(true);
    }
}