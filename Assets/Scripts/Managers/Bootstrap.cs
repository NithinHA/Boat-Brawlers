using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    private IEnumerator Start()
    {
        yield return new WaitForSeconds(1);
        AsyncOperation op = SceneManager.LoadSceneAsync(Constants.SceneNames.Main_Menu);
        op.completed += (op1) =>
        {
            if (op1.isDone)
            {
                // play sfx mainMenuBg
            }
        };
    }
}
