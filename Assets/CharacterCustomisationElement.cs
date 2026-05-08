using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CustomisationElement : MonoBehaviour
{
    [Header("Label")]
    public string elementName;
    public TextMeshProUGUI nameText;

    [Header("Options")]
    public List<GameObject> options;

    [Header("Highlight")]
    public Image highlightBorder;

    private int currentIndex = 0;

    public void Initialise()
    {
        currentIndex = 0;

        if (options.Count == 0)
        {
            Debug.LogError(
                gameObject.name + " HAS NO OPTIONS"
            );
            return;
        }

        for (int i = 0; i < options.Count; i++)
        {
            if (options[i] != null)
                options[i].SetActive(i == 0);
        }

        if (nameText != null)
            nameText.text = elementName;

        SetHighlight(false);
    }

    public void Next()
    {
        // Check if customization is locked
        if (CustomisationManager.Instance.IsCustomisationLocked())
            return;

        if (options.Count == 0)
            return;

        options[currentIndex].SetActive(false);
        currentIndex++;

        if (currentIndex >= options.Count)
            currentIndex = 0;

        options[currentIndex].SetActive(true);
    }

    public void Previous()
    {
        // Check if customization is locked
        if (CustomisationManager.Instance.IsCustomisationLocked())
            return;

        if (options.Count == 0)
            return;

        options[currentIndex].SetActive(false);
        currentIndex--;

        if (currentIndex < 0)
            currentIndex = options.Count - 1;

        options[currentIndex].SetActive(true);
    }

    public void SetHighlight(bool active)
    {
        if (highlightBorder != null)
            highlightBorder.enabled = active;
    }
}