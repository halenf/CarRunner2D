///<summary>
/// Author: Halen
///
///
///
///</summary>

using UnityEngine;
using UnityEditor;
using UnityEditor.Overlays;
using System.Linq;

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

        private bool m_goingBack = false;

        private bool m_willGenerateFloor = true;

        private bool m_nameCanBeSaved = false;

        private string m_terrainNameBuffer = "";

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
                case DebugLevelCreatorEditMode.TerrainCreator:
                    TerrainCreatorGUI();
                    ConfirmBackButton(DebugLevelCreatorEditMode.Menu);
                    break;
                case DebugLevelCreatorEditMode.TerrainLoader:
                    TerrainLoaderGUI();
                    BackButton(DebugLevelCreatorEditMode.Menu);
                    break;
                case DebugLevelCreatorEditMode.TerrainEditor:
                    TerrainEditorGUI();
                    ConfirmBackButton(DebugLevelCreatorEditMode.TerrainLoader);
                    break;
                case DebugLevelCreatorEditMode.DisplayOptions:
                    DisplayOptionsGUI();
                    BackButton(DebugLevelCreatorEditMode.Menu);
                    break;
            }
        }

        #region QuickGUI
        private void BackButton(DebugLevelCreatorEditMode targetMode)
        {
            GUIL.Space(k_Spacing);
            if (GUIL.Button(new GUIC("Back", "Go back to the menu.")))
            {
                target.SetEditMode(targetMode);
            }
        }

        private void ConfirmBackButton(DebugLevelCreatorEditMode targetMode)
        {
            GUIL.Space(k_Spacing);
            // if the user has not yet pressed the back button
            if (!m_goingBack)
            {
                if (GUIL.Button(new GUIC("Back", "Go back to the menu.")))
                {
                    m_goingBack = true;
                    RefreshPopup();
                }
            }
            // if the user has not yet confirmed they want to go back
            else
            {
                GUIL.Label(new GUIC("Are you sure? Unsaved changes will be lost."));
                EGUIL.BeginHorizontal();
                if (GUIL.Button(new GUIC("Yes")))
                {
                    m_goingBack = false;
                    target.SetEditMode(targetMode);
                }
                if (GUIL.Button(new GUIC("No")))
                {
                    m_goingBack = false;
                }
                EGUIL.EndHorizontal();
            }
        }
        #endregion

        private void MenuGUI()
        {
            GUIL.Label("Main Menu", EditorStyles.boldLabel);
            
            for (int m = 1; m < m_editModeNames.Length; m++)
            {
                if (GUIL.Button(m_editModeNames[m]))
                {
                    target.SetEditMode((DebugLevelCreatorEditMode)m);
                }
            }
        }

        private void TerrainCreatorGUI()
        {
            GUIL.Label(new GUIC("Create New Terrain"), EditorStyles.boldLabel);

            EGUIL.BeginHorizontal();
            GUIL.Label(new GUIC("Terrain Name", "The name used to indentify this Terrain."));
            string newName = GUIL.TextField(m_terrainNameBuffer);

            // on field changed, ensure new value is valid
            if (newName != m_terrainNameBuffer)
            {
                m_terrainNameBuffer = newName;

                // check if its empty
                // if its not empty, check values are okay for file
                if (m_terrainNameBuffer == string.Empty)
                {
                    m_nameCanBeSaved = false;
                }
                // keep length of name short
                else if (m_terrainNameBuffer.Length > 32)
                {
                    m_nameCanBeSaved = false;
                }
                // check for non letters, numbers, or white space
                else if (!m_terrainNameBuffer.All(char.IsLetterOrDigit))
                {
                    m_nameCanBeSaved = false;
                }
                else
                    m_nameCanBeSaved = true;
            }
            EGUIL.EndHorizontal();

            // if name can't be saved, then show the create button greyed out
            if (!m_nameCanBeSaved)
            {
                GUIL.Label(new GUIC("Invalid name! Must not be longer than 32 characters\nand must only contain letters and/or numbers."));
                GUI.enabled = false;
            }
            if (GUIL.Button(new GUIC("Create")))
            {
                target.CreateTerrainData(m_terrainNameBuffer);
                target.SetEditMode(DebugLevelCreatorEditMode.TerrainEditor);
            }
            if (!GUI.enabled)
                GUI.enabled = true;
        }

        private void TerrainLoaderGUI()
        {
            GUIL.Label(new GUIC("Terrain Loader"), EditorStyles.boldLabel);

            EGUIL.BeginHorizontal();
            string terrainName = GUIL.TextField(m_terrainNameBuffer);
            if (terrainName != m_terrainNameBuffer)
            {
                m_terrainNameBuffer = terrainName;
            }
            if (GUIL.Button(new GUIC("Load")))
            {
                if (target.LoadTerrainData(terrainName + ".cter"))
                    target.SetEditMode(DebugLevelCreatorEditMode.TerrainEditor);
                else
                    Debug.LogWarning("A Terrain with the name " + terrainName + " does not exist in this project!");
            }
            EGUIL.EndHorizontal();

            if (GUIL.Button(new GUIC("Load ALL Existing Terrains", "Load all the Terrains in the TerrainData folder.")))
            {
                if (target.LoadAllTerrainData())
                    target.SetEditMode(DebugLevelCreatorEditMode.TerrainEditor);
                else
                    Debug.LogWarning("No Terrains exist in this project. Go make some!");
            }

            if (GUIL.Button(new GUIC("Unload all Currently Loaded Terrains", "Unload all the loaded Terrains from the Debug Level Creator.")))
            {
                target.UnloadAllTerrainData();
            }
        }

        private void TerrainEditorGUI()
        {
            // show the selected terrain
            if (target.SelectedTerrain == -1)
            {
                GUIL.Label(new GUIC("Create or Load a Terrain First!"), EditorStyles.boldLabel);
                return;
            }

            GUIL.Label(new GUIC("Selected Terrain:"), EditorStyles.boldLabel);

            EGUIL.BeginHorizontal();
            if (GUIL.Button(new GUIC("<", "Select the previous loaded Terrain")))
            {
                target.SelectPrevTerrain();
            }
            GUIL.FlexibleSpace();
            GUIL.Label(new GUIC(target.SelectedTerrainName));
            GUIL.FlexibleSpace();
            if (GUIL.Button(new GUIC(">", "Select the next loaded Terrain")))
            {
                target.SelectNextTerrain();
            }
            EGUIL.EndHorizontal();

            // Display the currently selected Level Point's Id
            GUIL.Label(new GUIC("Selected Point Id:"), EditorStyles.boldLabel);

            // add controls to change the selected point from the Overlay
            // and display null if there is no selected point
            EGUIL.BeginHorizontal();
            if (target.NumberOfPoints > 1)
            {
                if (GUIL.Button(new GUIC("<", "Select the previous Level Point")))
                {
                    target.SelectPrevPoint();
                }
            }
            GUIL.FlexibleSpace();
            if (target.SelectedPoint == -1)
                GUIL.Label(new GUIC("null"));
            else
                GUIL.Label(new GUIC(target.SelectedPoint.ToString()));
            GUIL.FlexibleSpace();
            if (target.NumberOfPoints > 1)
            {
                if (GUIL.Button(new GUIC(">", "Select the next Level Point")))
                {
                    target.SelectNextPoint();
                }
            }
            EGUIL.EndHorizontal();

            // level point editing
            GUIL.Space(k_Spacing);
            GUIL.Label(new GUIC("Edit Level"), EditorStyles.boldLabel); 

            // create buttons
            if (GUIL.Button(new GUIC("Add New Point","Adds a new Level Point to the end of the list.")))
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
            GUIL.Label(new GUIC("Terrain Data"), EditorStyles.boldLabel);
            
            EGUIL.BeginHorizontal();
            if (GUIL.Button(new GUIC("Generate Terrain")))
            {
                if (m_willGenerateFloor)
                    target.GenerateFloorTerrain();
                else
                    target.GenerateTerrain();
            }
            bool willGenerateFloor = GUIL.Toggle(m_willGenerateFloor, new GUIC("Generate As Floor"));
            if (willGenerateFloor != m_willGenerateFloor)
            {
                target.SetDrawingMode(willGenerateFloor);
                m_willGenerateFloor = willGenerateFloor;
            }
            EGUIL.EndHorizontal();

            if (GUIL.Button(new GUIC("Save Terrain to File")))
            {
                target.SaveCurrentTerrainData();
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
