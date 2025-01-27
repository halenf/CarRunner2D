///<summary>
/// Author: Halen
///
///
///
///</summary>

using UnityEngine;
using UnityEditor;
using UnityEditor.Overlays;

namespace CarRunner2D
{
    using EGUIL = EditorGUILayout;
    using GUIL = GUILayout;
    using GUIC = GUIContent;

    //[Icon("Assets/Editor/WheelInfo/wheel.png")]
	public class DebugLevelCreatorOverlay : IMGUIOverlay
	{
        public const string k_Title = "Level Creator Tools";
        private const int k_Spacing = 8;

        private readonly DebugLevelCreator target;

        private string[] m_editModeNames;

        public DebugLevelCreatorOverlay(DebugLevelCreator target)
        {
            this.target = target;
            displayName = k_Title;
            m_editModeNames = System.Enum.GetNames(typeof(DebugLevelCreatorEditMode));
            RefreshPopup();
        }
        
        public override void OnGUI()
        {
            switch (target.EditMode)
            {
                case DebugLevelCreatorEditMode.Menu:
                    MenuGUI();
                    break;
                case DebugLevelCreatorEditMode.CreateTerrain:
                    CreateTerrainGUI();
                    break;
                case DebugLevelCreatorEditMode.EditTerrain:
                    EditTerrainGUI();
                    BackButton(DebugLevelCreatorEditMode.Menu);
                    break;
                case DebugLevelCreatorEditMode.DisplayOptions:
                    DisplayOptionsGUI();
                    BackButton(DebugLevelCreatorEditMode.Menu);
                    break;
            }
        }

        private void BackButton(DebugLevelCreatorEditMode targetMode)
        {
            GUIL.Space(k_Spacing);
            if (GUIL.Button(new GUIC("Back", "Go back to the menu.")))
            {
                target.SetEditMode(targetMode);
            }
        }

        private void MenuGUI()
        {
            GUIL.Label("Main Menu", EditorStyles.boldLabel);
            
            for (int m = 1; m < m_editModeNames.Length; m++)
            {
                if (GUIL.Button(m_editModeNames[m]))
                    target.SetEditMode((DebugLevelCreatorEditMode)m);
            }
        }

        private void CreateTerrainGUI()
        {
            GUIL.Label(new GUIC("Creating Terrains"), EditorStyles.boldLabel);

            if (!GUIL.Button(new GUIC("Create new Terrain")))
            {
                target.CreateTerrain();
            }
            if (!GUIL.Button(new GUIC("Load existing Terrain")))
            {
                
            }
        }

        private void EditTerrainGUI()
        {
            // Display the currently selected Level Point's Id
            GUIL.Label(new GUIC("Selected Point Id:"), EditorStyles.boldLabel);

            // add controls to change the selected point from the Overlay
            // and display null if there is no selected point
            EGUIL.BeginHorizontal();
            if (GUIL.Button(new GUIC("<", "Select the previous Level Point")))
            {
                target.SelectPrevPoint();
            }
            GUIL.FlexibleSpace();
            if (target.SelectedPoint == -1)
                GUIL.Label(new GUIC("null"));
            else
                GUIL.Label(new GUIC(target.SelectedPoint.ToString()));
            GUIL.FlexibleSpace();
            if (GUIL.Button(new GUIC(">", "Select the next Level Point")))
            {
                target.SelectNextPoint();
            }
            EGUIL.EndHorizontal();

            // level point editing
            GUIL.Space(k_Spacing);
            GUIL.Label(new GUIC("Editing Level Points"), EditorStyles.boldLabel); 

            // create buttons
            if (GUIL.Button(new GUIC("Add New Point", "Adds a new Level Point to the end of the list.")))
            {
                target.NewPoint_PushBack();
            }
            if (GUIL.Button(new GUIC("Insert New Point Here", "Adds a new Level Point at the selected index, then increments the index of all following Level Points.")))
            {
                target.NewPoint_AtSelected();
            }
            if (GUIL.Button(new GUIC("Insert New Point After", "Adds a new Level Point after the selected index, then increments the index of all following Level Points.")))
            {
                target.NewPoint_AfterSelected();
            }
            if (GUIL.Button(new GUIC("Delete Selected Point", "Removes the selected Level Point from the list.")))
            {
                target.DeletePoint();
            }
            if (GUIL.Button(new GUIC("Delete ALL Points", "Removes ALL Level Points from the list.")))
            {
                target.ClearPoints();
            }
            if (GUIL.Button(new GUIC("Rename Point Objects", "If the name of any of the Level Point Game Objects is incorrect, press this. If the names are already correct, this will do nothing.")))
            {
                target.RenamePoints();
            }
            if (GUIL.Button(new GUIC("Reorder Point Objects", "If the order of the Level Point Game Objects in the Point Container is incorrect, press this. If the order is already correct, this will do nothing.")))
            {
                target.OrderLevelPointTransforms();
            }

            // ground generation
            GUIL.Space(k_Spacing);
            GUIL.Label(new GUIC("Terrain Generation"), EditorStyles.boldLabel);

            if (GUIL.Button(new GUIC("Generate Floor")))
            {
                target.GenerateFloor();
            }
        }

        private void DisplayOptionsGUI()
        {
            GUIL.Label(new GUIC("Display Options"), EditorStyles.boldLabel);

            float newPointRadius = Mathf.Clamp01(EGUIL.FloatField(new GUIC("Radius of drawn points", "Set the size of drawn Level Points."), target.DisplayOptions.pointRadius));
            if (newPointRadius != target.DisplayOptions.pointRadius)
                target.DisplayOptions.pointRadius = newPointRadius;

            Color newPointColour = EGUIL.ColorField(new GUIC("Point Colour", "Set the colour of drawn Level Points"), target.DisplayOptions.pointColour);
            if (newPointColour != target.DisplayOptions.pointColour)
                target.DisplayOptions.pointColour = newPointColour;

            Color newLineColour = EGUIL.ColorField(new GUIC("Line Colour", "Set the colour of lines drawn between connected Level Points"), target.DisplayOptions.lineColour);
            if (newLineColour != target.DisplayOptions.lineColour)
                target.DisplayOptions.lineColour = newLineColour;

            // reset display options
            if (GUIL.Button(new GUIC("Reset Display Options", "Reset the Display Options back to their default values.")))
                target.DisplayOptions.Reset();
        }
    }
}
