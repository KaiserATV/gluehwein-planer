using UnityEngine;
using TMPro;
using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public Heatmap heatmapScript;
    private float scoreCount = 0;
    public SceneManager sceneManager;
  
    void Start()
    {
        this.gameObject.SetActive(false);
    }

    public void UpdateScore()
    {

        List<Bude> AlleBuden = sceneManager.allBudenScripts;
        int BusyBuden = 0;

        foreach (Bude bude in AlleBuden)
        {
            if (bude != null)
            {
                if (bude.CheckAuslastung()) BusyBuden++;
            }
        }

        scoreCount = CalcHeatMapScore() * 33 +
                 ((float)BusyBuden / AlleBuden.Count) * 33 +
                 ((float)sceneManager.agentsLostPatience / sceneManager.maxPlayerCount) * 33;
        scoreText.text = Math.Round(scoreCount, 2).ToString();
    }



    private float CalcHeatMapScore()
    {
        int[] array = heatmapScript.playMaxCount;

        if (array == null || array.Length == 0)
        {
            return 0;
        }

        int good = 0;
        int bad = 0;

        for (int i = 0; i < array.Length; i++)
        {
            int playerCount = array[i];
            if (playerCount > 0)
            {
                if (playerCount <= usageCat.medium)
                {
                    good++;
                }
                else
                {
                    bad++;
                }
            }
        }

        if (good == 0) return 0; // Vermeidung von Division durch Null

        return (float)bad / good;
    }

    public void ToggleEffizenzScore()
    {
        this.gameObject.SetActive(!this.gameObject.activeInHierarchy);
        if(this.gameObject.activeInHierarchy == true)
        {
            Transform referenceTransform = Camera.main.transform;
            float distance = 1.75f;

            Vector3 forwardDirection = Vector3.ProjectOnPlane(referenceTransform.forward, Vector3.up).normalized;
            this.transform.position = referenceTransform.position + forwardDirection * distance;

            Quaternion lookRotation = Quaternion.LookRotation(forwardDirection, Vector3.up);
            this.transform.rotation = lookRotation;
            UpdateScore();
        }
    }

}
