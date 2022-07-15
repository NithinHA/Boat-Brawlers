using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class MainMenuUI : MonoBehaviour
    {

        
#region Button OnClicks

        public void OnClick_Play()
        {
            SceneManager.LoadScene(1);
        }

        public void OnClick_Quit()
        {
            Application.Quit();
        }

#endregion

    }
}