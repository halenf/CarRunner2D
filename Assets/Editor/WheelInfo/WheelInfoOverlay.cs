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
            // if there is no game object selected
            if (Selection.activeGameObject == null)
                isVisible = false;
            // if the selected game object is the target's gameObject, the "if" will succeed
            // if the overlay does not currently have a reference to the target, it will continue
            else if (Selection.activeGameObject == (target != null ? target.gameObject : null))
                isVisible = true;
            // try and get the component matching the target from the selection
            // if it fails, the "if" will succeed
            else if (!Selection.activeGameObject.TryGetComponent(out target))
                isVisible = false;
            // if getting the target succeeded, it will end up here
            else
                isVisible = true;
        }

        /// <summary>
        /// Disables the overlay when the Editor exits Edit or Play mode.
        /// Stops an error caused by the overlay trying to target a selected wheel in a scene that no longer exists upon changing the Editor mode.
        /// </summary>
        /// <param name="state"></param>
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
