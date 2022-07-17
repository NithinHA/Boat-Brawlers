using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private Transform m_CameraHolder;
        [SerializeField] private float m_TransitTime = 1f;

#region Button OnClicks

        public void OnClick_Quit()
        {
            Application.Quit();
        }

        public void OnClick_MoveCamToLevelIndex(int index)
        {
            float posX = index * 150;
            m_CameraHolder.DOMoveX(posX, m_TransitTime).SetEase(Ease.InFlash);
        }

        public void OnClick_LevelIndex(int index)
        {
            string levelName = String.Empty;
            switch (index)
            {
                case 1: levelName = Constants.SceneNames.Level_1;
                    break;
                case 2: levelName = Constants.SceneNames.Level_2;
                    break;
                default:
                    Debug.LogError("Invalid scene index " + index);
                    break;
            }

            SceneManager.LoadScene(levelName);
        }

#endregion

    }
}