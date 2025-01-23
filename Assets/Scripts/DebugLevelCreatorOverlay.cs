///<summary>
/// Author: Halen
///
///
///
///</summary>

using UnityEngine;
using UnityEditor;
using UnityEditor.Overlays;

using EGL = UnityEditor.EditorGUILayout;

namespace CarRunner2D
{
    [Overlay(typeof(SceneView), k_Title)]
	public class DebugLevelCreatorOverlay : IMGUIOverlay
	{
        const string k_Title = "Level Creator Tools";

        public DebugLevelCreatorOverlay()
        {
            RefreshPopup();
        }
        
        public override void OnGUI()
        {
            if (DebugLevelCreator.Instance == null)
            {
                // if the instance is not set, try and find one in the scene
                DebugLevelCreator creator = GameObject.FindAnyObjectByType<DebugLevelCreator>();

                // if it doesn't exist, then stop
                if (creator == null)
                {
                    EGL.LabelField("No DebugLevelCreator static instance exists in this scene!");
                    return;
                }
                // if it does exist but wasn't initialised, initialise the Instance
                else
                {
                    creator.Initialise();
                }
            }

            // create buttons
            if (GUILayout.Button("Create Point"))
            {
                DebugLevelCreator.Instance.CreatePoint();
            }
            if (GUILayout.Button("Delete Selected Point"))
            {
                DebugLevelCreator.Instance.DeletePoint();
            }
            if (GUILayout.Button("Delete ALL Points"))
            {
                DebugLevelCreator.Instance.ClearPoints();
            }
        }
    }
}
