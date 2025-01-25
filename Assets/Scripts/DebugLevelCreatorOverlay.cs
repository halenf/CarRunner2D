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

        private string m_currentTag;

        public DebugLevelCreatorOverlay()
        {
            RefreshPopup();

            // get what the current tag is
            if (DebugLevelCreator.Instance != null)
                m_currentTag = DebugLevelCreator.Instance.LevelPointTag;
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
                    EGL.LabelField("No DebugLevelCreator exists in this scene!");
                    return;
                }
                // if it does exist but wasn't initialised, initialise the Instance
                else
                {
                    creator.Initialise();
                    m_currentTag = creator.LevelPointTag;
                }
            }

            // Select the tag used for the level points
            EGL.BeginHorizontal();
            EGL.LabelField("Level Point object Tag");
            m_currentTag = EGL.TagField(m_currentTag);
            if (m_currentTag != DebugLevelCreator.Instance.LevelPointTag)
                DebugLevelCreator.Instance.SetNewLevelPointTag(m_currentTag);
            EGL.EndHorizontal();

            // show if there is a currently selected point
            if (DebugLevelCreator.Instance.SelectedPoint == -1)
                EGL.LabelField("Selected Point Id: null.");
            else                         
                EGL.LabelField("Selected Point Id: " + DebugLevelCreator.Instance.SelectedPoint.ToString());

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
