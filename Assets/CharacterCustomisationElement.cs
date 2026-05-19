using UnityEngine;
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
    public GameObject highlightBorder;

    private int currentIndex = 0;
    private string prefsKey;

    public void Initialise(string playerKey)
    {
        prefsKey = playerKey + "_" + elementName;

        // Load saved index, default to 0
        currentIndex = PlayerPrefs.GetInt(prefsKey, 0);

        // Clamp in case options list changed since last save
        currentIndex = Mathf.Clamp(currentIndex, 0, options.Count - 1);

        if (options.Count == 0)
        {
            Debug.LogError(gameObject.name + " HAS NO OPTIONS");
            return;
        }

        for (int i = 0; i < options.Count; i++)
        {
            if (options[i] != null)
                options[i].SetActive(i == currentIndex);
        }

        if (nameText != null)
            nameText.text = elementName;

        SetHighlight(false);
    }

    public void Next()
    {
        if (CustomisationManager.Instance.IsCustomisationLocked()) return;
        if (options.Count == 0) return;

        options[currentIndex].SetActive(false);
        currentIndex = (currentIndex + 1) % options.Count;
        options[currentIndex].SetActive(true);

        Save();
    }

    public void Previous()
    {
        if (CustomisationManager.Instance.IsCustomisationLocked()) return;
        if (options.Count == 0) return;

        options[currentIndex].SetActive(false);
        currentIndex--;
        if (currentIndex < 0) currentIndex = options.Count - 1;
        options[currentIndex].SetActive(true);

        Save();
    }

    public void SetHighlight(bool active)
    {
        if (highlightBorder != null)
            highlightBorder.SetActive(active);
    }

    void Save()
    {
        if (!string.IsNullOrEmpty(prefsKey))
            PlayerPrefs.SetInt(prefsKey, currentIndex);
    }
}