///<summary>
/// Author: Halen
///
///
///
///</summary>

using UnityEngine;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.SceneManagement;

namespace CarRunner2D
{
	[Overlay(typeof(EditorWindow), "Edit Tools")]
    [Icon("Assets/Editor/EditTools/spanner.png")]
	public class EditToolsOverlay : IMGUIOverlay
	{
        public override void OnGUI()
        {
            if (Application.isPlaying)
            {
                if (GUILayout.Button("Restart Level"))
                    EditorSceneManager.LoadScene(EditorSceneManager.GetActiveScene().buildIndex);
            }
            else
            {
                if (GUILayout.Button("Reload Current Scene"))
                    EditorSceneManager.OpenScene("Assets/Scenes/" + EditorSceneManager.GetActiveScene().name + ".unity");
            }
        }
    }
}
