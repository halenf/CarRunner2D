///<summary>
/// Author: Halen
///
///
///
///</summary>

using UnityEngine;
using UnityEngine.U2D;
using UnityEditor;
using UnityEditor.Overlays;

using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace CarRunner2D
{
    public enum DebugLevelCreatorEditMode
    {
        Menu,
		TerrainCreator,
		TerrainLoader,
        TerrainEditor,
        DisplayOptions
    }

    [ExecuteInEditMode, DisallowMultipleComponent]
	[AddComponentMenu("")] // removes this from the menu
	public class DebugLevelCreator : MonoBehaviour
	{
		public static DebugLevelCreator Instance;

        #region Properties
        // CONSTANTS
        public const string k_DebugLevelCreatorName = "Debug Level Creator";
        public const string k_LevelPointTag = "LevelPoint";
		public const string k_TerrainDataSavePath = "Assets/TerrainData";

		// OBJECT REFERENCES
		private Overlay m_overlay;
		private DebugLevelCreatorDisplayOptions m_displayOptions;
		public DebugLevelCreatorDisplayOptions DisplayOptions
		{
			get
			{
				if (m_displayOptions == null)
					m_displayOptions = new DebugLevelCreatorDisplayOptions();
				return m_displayOptions;
			}
		}

		// EDIT MODE
		private DebugLevelCreatorEditMode m_editMode = DebugLevelCreatorEditMode.Menu;
		public DebugLevelCreatorEditMode EditMode => m_editMode;

        // DEBUG
        public Texture m_pointTexture;
		public SpriteShape defaultSpriteShape;
		public float floorHeight = 8f;
		
		// TERRAINS AND POINTS
        private List<Transform> m_points;
		private List<TerrainData> m_terrains;

		private int m_selectedPoint;
		public int SelectedPoint => m_selectedPoint;
        public int NumberOfPoints => m_points.Count;

		private int m_selectedTerrain;
		public int SelectedTerrain => m_selectedTerrain;
		public string SelectedTerrainName => m_terrains[m_selectedTerrain].Name;

        // DRAW
        private bool m_drawModeFloor = true;
        #endregion

        [ContextMenu("Initialise")]
        public void Initialise()
        {
            // create singleton
            if (Instance != null && Instance != this)
            {
                DestroyImmediate(gameObject);
                return;
            }
            else
                Instance = this;

            // enter the default mode
            m_editMode = DebugLevelCreatorEditMode.Menu;

            // check if display options are initialised
            m_displayOptions ??= new DebugLevelCreatorDisplayOptions();

            // setup points
            if (m_points == null)
                m_points = new List<Transform>();

            SelectLevelPoint(-1);

            // setup terrains
            if (m_terrains == null)
                m_terrains = new List<TerrainData>();

            m_selectedTerrain = -1;

            // add the tags to the project if they do not already exist in the project
            AddTagsToProject(new string[1] { k_LevelPointTag });
            // AddLayersToProject(new string[1] { k_TerrainLayer });

            Debug.Log(name + " was initialised!");
        }
        public void SetEditMode(DebugLevelCreatorEditMode mode)
        {
            m_editMode = mode;
        }
        private void ProcessSelection()
        {
            if (!Selection.activeGameObject)
                return;

            GameObject selection = Selection.activeGameObject;

            // if the selection is a LevelPoint
            // get the Id of the point from its name
            if (selection.CompareTag("LevelPoint"))
            {
                SelectLevelPoint(int.Parse(selection.name[selection.name.Length - 1].ToString()));
                m_editMode = DebugLevelCreatorEditMode.TerrainEditor;
            }
            else
            {
                SelectLevelPoint(-1);
            }
        }
        
        #region StaticMethods
        [MenuItem("Level Creator/Create Debug Level Creator")]
        private static void CreateDebugLevelCreator()
        {
            // check if a creator already exists
            if (Instance != null)
            {
                Debug.LogError("Debug Level Creator already exists in the scene! If you want to reset it, use the " +
                    "tools provided in the overlay.", Instance);
                return;
            }

            // create the new creator
            GameObject creator = new GameObject(k_DebugLevelCreatorName);

            // add the component
            creator.AddComponent<DebugLevelCreator>();

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(creator, "Create " + creator.name);

            // select the object
            Selection.activeObject = creator;
        }

        public static void AddTagsToProject(string[] tagsToAdd)
        {
            // get the tag manager object from the project settings
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

            // get the tags array
            SerializedProperty tags = tagManager.FindProperty("tags");

            // add each string
            foreach (string tag in tagsToAdd)
            {
                bool shouldAddTag = true;

                // check each existing tag
                for (int t = 0; t < tags.arraySize; t++)
                {
                    // if the level point tag already exists, then stop
                    if (tags.GetArrayElementAtIndex(t).stringValue == tag)
                    {
                        Debug.LogWarning("The " + tag + " tag already exists in this project.");
                        shouldAddTag = false;
                        break;
                    }
                }

                if (!shouldAddTag)
                    continue;

                // increase the size of the tag array
                tags.arraySize++;

                // get a reference to the newly added array element
                SerializedProperty newTag = tags.GetArrayElementAtIndex(tags.arraySize - 1);

                // set the value/tag name to the level point tag
                newTag.stringValue = tag;

                // apply changes to the tag manager so the user cannot undo the change
                tagManager.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        #endregion

        #region MonoBehaviourMethods
        private void Awake()
        {
            Initialise();
        }

        /// <summary>
        /// Mainly deletion checks in the hierarchy.
        /// </summary>
        private void Update()
        {
            if (transform.childCount != m_points.Count)
            {
                // remove any null points in the points list
                // may have been deleted from the hierarchy
                m_points.RemoveAll(point => point == null);
                RenamePoints();

                // delete any objects that are not level points
                List<GameObject> invalidObjects = new List<GameObject>();

                foreach (Transform t in  transform)
                {
                    if (!t.CompareTag(k_LevelPointTag))
                        invalidObjects.Add(t.gameObject);
                }

                for (int t = 0; t < invalidObjects.Count; t++)
                    DestroyImmediate(invalidObjects[t]);

                // also take this oppurtunity to check that children weren't added to the Level Points
                foreach (Transform t in transform)
                {
                    if (t.childCount != 0)
                    {
                        foreach (Transform invalid in t)
                        {
                            DestroyImmediate(t.gameObject);
                        }
                    }
                }

                // remove any now deleted points,
                // in case one was accidentally parented to another Level Point, then rename
                m_points.RemoveAll(point => point == null);
                RenamePoints(0);
            }
        }

        private void OnEnable()
        {
            // enable overlay
            m_overlay = new DebugLevelCreatorOverlay(this);
            SceneView.AddOverlayToActiveView(m_overlay);

            // add callbacks
            Selection.selectionChanged += ProcessSelection;
        }

        private void OnDisable()
        {
            // disable overlay
            SceneView.RemoveOverlayFromActiveView(m_overlay);
            m_overlay = null;

            // remove callbacks
            Selection.selectionChanged -= ProcessSelection;
        }

        private void OnDestroy()
        {
            Selection.selectionChanged -= ProcessSelection;
        }
        #endregion

        #region Terrains
        public void CreateTerrainData(string name)
        {
            TerrainData newTerrain = new TerrainData(name, new Vector2[0]);

            // insert the terrain at the index matching its Id
            m_terrains.Add(newTerrain);

            // select the new terrain
            SelectTerrain(m_terrains.Count - 1);

            Debug.Log("Created a new Terrain with name " + name + "!");
        }

        public void SaveCurrentTerrainData()
        {
            Vector2[] points = m_points.Select(point => (Vector2)point.position).ToArray();
            m_terrains[m_selectedTerrain].Points = points;

            TerrainSaveLoad.SerialiseTerrainData(m_terrains[m_selectedTerrain]);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// <para>Will not load TerrainDatas that are already loaded into the DebugLevelCreator's memory.</para>
        /// <para>Note: loadingMultiple is only used for methods that load multiple Terrains in a row to avoid selecting them each time they load.</para>
        /// </summary>
        /// <returns>If the TerrainData was successfully found and loaded.</returns>
        public bool LoadTerrainData(string fileName, bool loadingMultiple = false)
        {
            string terrainName = fileName.Substring(0, fileName.Length - 5);

            foreach (TerrainData terrain in m_terrains)
            {
                if (terrainName == terrain.Name)
                {
                    Debug.LogWarning("Terrain with name " + terrainName + " is already loaded!");
                    return false;
                }
            }

            m_terrains.Add(TerrainSaveLoad.LoadSerialisedTerrainData(fileName));

            if (!loadingMultiple)
                SelectTerrain(m_terrains.Count - 1);

            return true;
        }

        public bool LoadAllTerrainData()
        {
            bool loadedATerrain = false;
            string[] terrainFiles = Directory.GetFiles(k_TerrainDataSavePath, "*.cter", SearchOption.TopDirectoryOnly);
            foreach (string terrain in terrainFiles)
            {
                if (LoadTerrainData(Path.GetFileName(terrain), true))
                    loadedATerrain = true;
            }

            if (loadedATerrain)
            {
                SelectTerrain(m_terrains.Count - 1);
            }

            return loadedATerrain;
        }

        public void UnloadAllTerrainData()
        {
            m_terrains.Clear();
            ClearPoints();
            SelectTerrain(-1);
        }

        public void SelectTerrain(int terrainId)
        {
            m_selectedTerrain = terrainId;

            // delete all the existing points and replace them with the new terrain's points
            ClearPoints();

            if (m_selectedTerrain == -1)
                return;

            TerrainData terrain = m_terrains[m_selectedTerrain];
            foreach (Vector2 point in terrain.Points)
            {
                NewPoint_PushBack(point);
            }

            // select the first point
            if (m_points.Count != 0)
                SelectLevelPoint(0);
        }

        public void SelectNextTerrain()
        {
            m_selectedTerrain++;
            if (m_selectedTerrain == m_terrains.Count)
                m_selectedTerrain = 0;

            SelectTerrain(m_selectedTerrain);
        }

        public void SelectPrevTerrain()
        {
            m_selectedTerrain--;
            if (m_selectedTerrain < 0)
                m_selectedTerrain = m_terrains.Count - 1;

            SelectTerrain(m_selectedTerrain);
        }

        public void GenerateFloorTerrain()
        {
            Vector2[] points = m_points.Select(point => (Vector2)point.position).ToArray();

            GameObject floor = GroundGenerator.GenerateFloorObject(points, defaultSpriteShape, floorHeight);
            floor.name = m_terrains[m_selectedTerrain].Name + " Floor Generation";

            Selection.activeGameObject = floor;
        }

        public void GenerateTerrain()
        {
            Vector2[] points = m_points.Select(point => (Vector2)point.position).ToArray();

            GameObject terrain = GroundGenerator.GenerateGroundObject(points, defaultSpriteShape, m_selectedTerrain);
            terrain.name = m_terrains[m_selectedTerrain].Name + " Terrain Generation";
            
            Selection.activeGameObject = terrain;
        }
        #endregion

        #region Points
        public void NewPoint_PushBack()
        {
            m_points.Add(NewPoint(m_points.Count));
            SelectLevelPoint(m_points.Count - 1);
        }

        private void NewPoint_PushBack(Vector2 position)
        {
            m_points.Add(NewPoint(m_points.Count, position));
            SelectLevelPoint(m_points.Count - 1);
        }

        public void NewPoint_AtSelected()
        {
            m_points.Insert(m_selectedPoint, NewPoint(m_selectedPoint));
            RenamePoints(m_selectedPoint + 1);
            OrderLevelPointTransforms();
            SelectLevelPoint(m_selectedPoint);
        }

        public void NewPoint_AfterSelected()
        {
            m_points.Insert(m_selectedPoint + 1, NewPoint(m_selectedPoint + 1));
            RenamePoints(m_selectedPoint + 1);
            OrderLevelPointTransforms();
            SelectLevelPoint(m_selectedPoint + 1);
        }

        private Transform NewPoint(int pointId)
        {
            // put the point in the centre of the screen
            Camera sceneCamera = SceneView.lastActiveSceneView.camera;
            Vector3 position = sceneCamera.ViewportToWorldPoint(new Vector2(0.5f, 0.5f));
            position.z = 0;

            // create the new point in the scene
            GameObject newPoint = new GameObject("Point " + pointId.ToString());
            newPoint.transform.SetParent(transform);

            // add point details
            newPoint.transform.position = position;
            newPoint.tag = k_LevelPointTag;

            return newPoint.transform;
        }

        private Transform NewPoint(int pointId, Vector2 position)
        {
            // put the point in the centre of the screen
            Camera sceneCamera = SceneView.lastActiveSceneView.camera;
            Vector3 _position = new Vector3(position.x, position.y);

            // create the new point in the scene
            GameObject newPoint = new GameObject("Point " + pointId.ToString());
            newPoint.transform.SetParent(transform);

            // add point details
            newPoint.transform.position = _position;
            newPoint.tag = k_LevelPointTag;

            return newPoint.transform;
        }

        public void DeletePoint()
        {
            if (m_selectedPoint != -1)
            {
                // destroy the point's game object
                DestroyImmediate(transform.GetChild(m_selectedPoint).gameObject);

                // remove the point from the list
                m_points.RemoveAt(m_selectedPoint);

                // update the remaining points' names with the correct Id
                // only the points at and after the deleted index will have incorrect Ids
                RenamePoints(m_selectedPoint);

                // find a new point to select, if it exists
                // if there are no points left, deselect
                if (m_points.Count == 0)
                    SelectLevelPoint(-1);
                // if there is one point left, select i
                else if (m_points.Count == 1)
                    SelectLevelPoint(0);
                // if a point exists at the previous index, select it
                else if (m_selectedPoint - 1 >= 0)
                    m_selectedPoint--;
                // if a point still exists at the current index (after the old one was deleted),
                // then we don't have to change the selected point index at all
                // this is also the last possible case
            }
            else
                Debug.LogWarning("Tried to delete a point without one selected!");
        }

        public void RenamePoints(int updateIndex = 0)
        {
            // keep range to avoid errors
            if (updateIndex < 0)
                updateIndex = 0;

            for (int t = updateIndex; t < m_points.Count; t++)
            {
                m_points[t].name = "Point " + t;
            }
        }

        public void OrderLevelPointTransforms()
        {
            // set the sibling index on m_pointContainer to the point's index in m_points
            for (int p = 0; p < m_points.Count; p++)
            {
                m_points[p].SetSiblingIndex(p);
            }
        }

        public void ClearPoints()
        {
            // destroy all the point objects
            for (int t = transform.childCount; t > 0; t--)
            {
                DestroyImmediate(transform.GetChild(t - 1).gameObject);
            }

            // empty the list of points
            m_points.Clear();

            // deselect any point and reset the tools canvas
            SelectLevelPoint(-1);
        }

        public void SelectLevelPoint(int pointId)
        {
            m_selectedPoint = pointId;

            if (m_selectedPoint != -1)
                Selection.activeGameObject = m_points[m_selectedPoint].gameObject;
        }

        public void SelectNextPoint()
        {
            m_selectedPoint++;
            if (m_selectedPoint == m_points.Count)
                SelectLevelPoint(0);

            Selection.activeGameObject = m_points[m_selectedPoint].gameObject;
        }

        public void SelectPrevPoint()
        {
            m_selectedPoint--;
            if (m_selectedPoint < 0)
                SelectLevelPoint(m_points.Count - 1);
        }
        #endregion

        #region Drawing
        private void OnDrawGizmos()
        {
            // check if m_points has points to draw
            if (m_points.Count == 0)
                return;

            // scale the size of the gizmos with the 'zoom' level
            // also ensure the last active scene view object exists
            Camera sceneCamera = SceneView.lastActiveSceneView.camera;
            if (!sceneCamera)
                return;

            // scale the radius of the discs
            float discRadius = sceneCamera.orthographicSize * m_displayOptions.pointRadius;

            // draw lines between the points
            using (new Handles.DrawingScope(m_displayOptions.lineColour))
            {               
                for (int p = 0; p < m_points.Count - 1; p++)
                {
                    Handles.DrawLine(m_points[p].position, m_points[p + 1].position);
                }

                // if drawing the floor, draw lines to represent the height of the floor
                if (m_drawModeFloor)
                {
                    Handles.DrawLine(m_points[0].position, m_points[0].position - new Vector3(0, floorHeight));
                    Handles.DrawLine(m_points[m_points.Count - 1].position, m_points[m_points.Count - 1].position - new Vector3(0, floorHeight));
                }
                // if drawing as a standalone terrain, draw line between first and last point
                else
                    Handles.DrawLine(m_points[m_points.Count - 1].position, m_points[0].position);
            }

            // draw all points on top of lines
            using (new Handles.DrawingScope(m_displayOptions.pointColour))
            {
                for (int p = 0; p < m_points.Count; p++)
                {
                    Handles.DrawSolidDisc(m_points[p].position, Vector3.back, discRadius);
                }
            }
        }

        public void SetDrawingMode(bool value)
        {
            m_drawModeFloor = value;
        }
        #endregion
    }
}
