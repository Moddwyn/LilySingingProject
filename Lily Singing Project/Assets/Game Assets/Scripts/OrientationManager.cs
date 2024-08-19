using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class OrientationManager : MonoBehaviour
{
    void Start()
    {
        SetOrientation();
    }

    void SetOrientation()
    {
        // Check the scene name or use any other condition to set orientation
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex == 0)
        {
            Screen.orientation = ScreenOrientation.Portrait;
            #if UNITY_EDITOR
            SetAspectRatio("Portrait");
            #endif
        }
        else if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex == 1)
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            #if UNITY_EDITOR
            SetAspectRatio("Landscape");
            #endif
        }
    }

    #if UNITY_EDITOR
    void SetAspectRatio(string aspectName)
    {
        var gameView = GetMainGameView();
        var aspectRatios = gameView.GetType().GetProperty("m_AspectRatioChoices", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(gameView, null) as System.Array;

        for (int i = 0; i < aspectRatios.Length; i++)
        {
            if (aspectRatios.GetValue(i).ToString() == aspectName)
            {
                gameView.GetType().GetField("m_AspectRatioSelected", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(gameView, i);
                gameView.Repaint();
                break;
            }
        }
    }

    EditorWindow GetMainGameView()
    {
        System.Type gameViewType = System.Type.GetType("UnityEditor.GameView, UnityEditor");
        System.Reflection.MethodInfo getMainGameView = gameViewType.GetMethod("GetMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        System.Object res = getMainGameView.Invoke(null, null);
        return (EditorWindow)res;
    }
    #endif
}
