using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Project.Utilities.Editor
{

    public class EditorUtilities : MonoBehaviour
    {
        public static bool FindEditorWindow<WindowType>(string windowName) where WindowType : EditorWindow
        {
            WindowType[] windows = Resources.FindObjectsOfTypeAll<WindowType>().Where(window => window.titleContent.text == windowName).ToArray();

            //for (int i = 0; i < windows.Length; i++)
            //{
            //    print(windows[i].titleContent.text);
            //}

            return windows != null && windows.Length > 0;
        }


        public static WindowType FindEditorWindow<WindowType>(string windowName, int windowIndex) where WindowType : EditorWindow
        {
            WindowType[] windows = Resources.FindObjectsOfTypeAll<WindowType>().Where(window => window.titleContent.text == windowName).ToArray();
            return windows != null && windows.Length > 0 ? windows[windowIndex] : null;
        }
    }
}