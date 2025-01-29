///<summary>
/// Author: Halen
///
///
///
///</summary>

using UnityEngine;
using UnityEngine.U2D;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.Overlays;
using System.Linq;

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
        public const string k_LevelPointTag = "LevelPoint";
		public const string k_pointContainerTag = "PointContainer";

		private Overlay m_overlay;

		private DebugLevelCreatorEditMode m_editMode = DebugLevelCreatorEditMode.Menu;
		public DebugLevelCreatorEditMode EditMode => m_editMode;

		public SpriteShape defaultSpriteShape;
		public float floorHeight = 8f;

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

		// container for Level Point GameObjects
		private Transform m_pointContainer;

		// currently selected Level Point Id
		private int m_selectedPoint;
		public int SelectedPoint => m_selectedPoint;

		// currently selected Terrain Id
		private int m_selectedTerrain;
		public int SelectedTerrain => m_selectedTerrain;

		// List of all Level Point Transforms
        private List<Transform> m_points;

		// list of all Terrain Objects
		private List<TerrainData> m_terrainObjects;

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
            GameObject creator = new GameObject("Debug Level Creator");

            // add the component
            creator.AddComponent<DebugLevelCreator>();

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(creator, "Create " + creator.name);

            // select the object
            Selection.activeObject = creator;
        }

        private void Awake()
        {
            Initialise();
        }

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

            // setup Point Container
            if (!m_pointContainer)
                CreatePointContainer();

            // add the tags to the project if they do not already exist in the project
            AddTagsToProject(new string[2] { k_LevelPointTag, k_pointContainerTag });
			// AddLayersToProject(new string[1] { k_TerrainLayer });

            Debug.Log(name + " was initialised!");
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

        private void CreatePointContainer()
        {
            // check if the point container exists
			for (int t = 0; t < transform.childCount; t++)
			{
				// point container exists
				if (transform.GetChild(t).CompareTag(k_pointContainerTag))
				{
					m_pointContainer = transform.GetChild(t);
					return;
				}
			}

            m_pointContainer = new GameObject("Point Container").transform;
			m_pointContainer.tag = k_pointContainerTag;
            m_pointContainer.SetParent(transform);
        }

        public void SetEditMode(DebugLevelCreatorEditMode mode)
		{
			m_editMode = mode;
		}

        private void OnDestroy()
        {
            Selection.selectionChanged -= ProcessSelection;
        }

		/// <summary>
		/// Mainly deletion checks in the hierarchy.
		/// </summary>
        private void Update()
        {		
			// if point container has been deleted
			if (m_pointContainer == null)
            {
				CreatePointContainer();
            }

			if (m_pointContainer.childCount != m_points.Count)
			{			
				// remove any null points in the points list
				// may have been deleted from the hierarchy
				m_points.RemoveAll(point => point == null);
				RenamePoints(0);
			}

			// stop the addition of extra children to the creator
			if (transform.childCount > 1)
			{
				bool foundPointContainer = false;
				List<Transform> fakeChildren = new List<Transform>();
				foreach (Transform t in transform)
				{
					// if the point container has been found,
					// then every other child is fake
					if (foundPointContainer)
					{
						fakeChildren.Add(t);
						continue;
					}
					
					// if the child is the point container,
					// we can safely start deleting all other points
					if (t == m_pointContainer)
					{
						foundPointContainer = true;
						continue;
					}
					// check if the child is the point container before trying to destroy it
					// if the point container has not yet been found
					else
						fakeChildren.Add(t);
				}

				for (int t = 0; t < fakeChildren.Count; t++)
					DestroyImmediate(fakeChildren[t].gameObject);
			}
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
				m_selectedPoint = int.Parse(selection.name[selection.name.Length - 1].ToString());
				m_editMode = DebugLevelCreatorEditMode.TerrainCreator;
			}
			else
			{
				m_selectedPoint = -1;
			}
		}

		public void NewPoint_PushBack()
		{
			m_points.Add(NewPoint(m_points.Count));
		}

		public void NewPoint_AtSelected()
		{
			m_points.Insert(m_selectedPoint, NewPoint(m_selectedPoint));
			RenamePoints(m_selectedPoint + 1);
			OrderLevelPointTransforms();
		}

		public void NewPoint_AfterSelected()
		{
			m_points.Insert(m_selectedPoint + 1, NewPoint(m_selectedPoint + 1));
			RenamePoints(m_selectedPoint + 1);
            OrderLevelPointTransforms();
        }

        private Transform NewPoint(int pointId)
        {
			// put the point in the centre of the screen
			Camera sceneCamera = SceneView.lastActiveSceneView.camera;
			Vector3 position = sceneCamera.ViewportToWorldPoint(new Vector2(0.5f, 0.5f));
			position.z = 0;

			// create the new point in the scene
			m_selectedPoint = pointId;
			GameObject newPoint = new GameObject("Point " + m_selectedPoint.ToString());
			newPoint.transform.SetParent(m_pointContainer);
			
			// add point details
			newPoint.transform.position = position;
			newPoint.tag = k_LevelPointTag;

            // select the new point
            Selection.activeGameObject = newPoint;

			return newPoint.transform;
        }

        public void DeletePoint()
        {
			if (m_selectedPoint != -1)
			{
				// destroy the point's game object
				DestroyImmediate(m_pointContainer.GetChild(m_selectedPoint).gameObject);

				// remove the point from the list
				m_points.RemoveAt(m_selectedPoint);

				// update the remaining points' names with the correct Id
				// only the points after the deleted one will have incorrect Ids
				RenamePoints(m_selectedPoint);

				// deselect the point
				m_selectedPoint = -1;
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
			for (int t = m_pointContainer.childCount; t > 0; t--)
            {
				DestroyImmediate(m_pointContainer.GetChild(t - 1).gameObject);
            }

			// empty the list of points
			m_points.Clear();

			// deselect any point and reset the tools canvas
			m_selectedPoint = -1;
        }

		public void CreateTerrainData(string name)
		{
			m_selectedTerrain = GetFirstFreeTerrainId();
			TerrainData newTerrain = new TerrainData(name, m_selectedTerrain);

			// insert the terrain at the index matching its Id
			m_terrainObjects.Insert(m_selectedTerrain, newTerrain);

            m_editMode = DebugLevelCreatorEditMode.TerrainCreator;
        }

        public void LoadTerrainData()
		{

		}

		private int GetFirstFreeTerrainId()
		{
			int newTerrainId = 0;

			while (true)
			{
				for (int t = 0; t < m_terrainObjects.Count; t++)
				{
					if (m_terrainObjects[t].TerrainId == newTerrainId)
					{
						newTerrainId++;
						break;
					}
				}

				return newTerrainId;
			}
		}

		public void SavePointsToTerrain()
		{

		}

		public void SelectTerrain(int terrainId)
		{

		}

		public void SelectNextPoint()
		{
			m_selectedPoint++;
			if (m_selectedPoint == m_points.Count)
				m_selectedPoint = 0;

			Selection.activeGameObject = m_points[m_selectedPoint].gameObject;
		}

		public void SelectPrevPoint()
		{
			m_selectedPoint--;
			if (m_selectedPoint < 0)
				m_selectedPoint = m_points.Count - 1;

			Selection.activeGameObject = m_points[m_selectedPoint].gameObject;
		}

		public void GenerateFloorTerrain()
		{
            Vector2[] points = m_points.Select(point => (Vector2)point.position).ToArray();

			GameObject floor = GroundGenerator.GenerateFloorObject(points, defaultSpriteShape, floorHeight);
        }

		public void GenerateTerrain()
		{
			Vector2[] points = m_points.Select(point => (Vector2)point.position).ToArray();

			GameObject terrain = GroundGenerator.GenerateGroundObject(points, defaultSpriteShape, m_selectedTerrain);
        }

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
    }
}
