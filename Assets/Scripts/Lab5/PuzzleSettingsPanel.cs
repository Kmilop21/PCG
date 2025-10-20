using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleSettingsPanel : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    } 
}
