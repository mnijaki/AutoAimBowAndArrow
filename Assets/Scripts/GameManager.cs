using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Awake()
    {
        SetGameWindowToFullScreen();
    }

    private static void SetGameWindowToFullScreen()
    {
    #if UNITY_EDITOR
        EditorWindow[] windows = (EditorWindow[])Resources.FindObjectsOfTypeAll(typeof(EditorWindow));
        foreach(var window in windows)
        {
            if(window == null || window.GetType().FullName != "UnityEditor.GameView")
            {
                continue;
            }
            
            window.maximized = true;
            break;
        }
 
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    #endif
    }
}
