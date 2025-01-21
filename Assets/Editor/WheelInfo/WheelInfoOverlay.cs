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
    [Overlay(typeof(SceneView), Title)]
    [Icon("Assets/Editor/WheelInfo/wheel.png")]
	public class WheelInfoOverlay : IMGUIOverlay, ITransientOverlay
	{
        private Wheel target;

        public WheelInfoOverlay()
        {
            //defaultSize = new Vector2(280, 130);
            RefreshPopup();
        }

        private void DisplayContent()
        {
            EGL.LabelField(target.name);

            if (!target.IsInitialised)
                return;

            EGL.FloatField("Current Speed", -target.Speed);
            EGL.FloatField("Acceleration", -target.LastAcceleration);
        }

        #region Transient Overlay
        const string Title = "Wheel Info";
        public bool visible => isVisible;
        private bool isVisible = false;

        public override void OnGUI()
        {
            DisplayContent();
            Update();
        }

        public override void OnCreated()
        {
            Selection.selectionChanged += Update;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            Update();
        }

        public override void OnWillBeDestroyed()
        {
            Selection.selectionChanged -= Update;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            Update();
        }
        private void Update()
        {
            if (Selection.activeGameObject == null)
                isVisible = false;
            else if (Selection.activeGameObject == (target != null ? target.gameObject : null))
                isVisible = true;
            else if (!Selection.activeGameObject.TryGetComponent(out target))
                isVisible = false;
            else
                isVisible = true;
        }
        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                case PlayModeStateChange.ExitingPlayMode:
                    isVisible = false;
                    break;
            }
        }
        #endregion
    }
}
