///<summary>
/// Author: Halen
///
///
///
///</summary>

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.Overlays;

namespace CarRunner2D
{
    public enum DebugLevelCreatorEditMode
    {
        Menu,
        EditPoints,
        DisplayOptions
    }

    [ExecuteInEditMode, DisallowMultipleComponent]
	[AddComponentMenu("")] // removes this from the menu
	public class DebugLevelCreator : MonoBehaviour
	{
		public static DebugLevelCreator Instance;
        public const string k_LevelPointTag = "LevelPoint";

		private Overlay m_overlay;

		private DebugLevelCreatorEditMode m_editMode = DebugLevelCreatorEditMode.Menu;
		public DebugLevelCreatorEditMode EditMode => m_editMode;

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

		// List of all Level Point Transforms
        private List<Transform> m_points;

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

            // add the LevelPoint tag if it does not already exist in the project
            AddTagToProject();

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

        public static void AddTagToProject()
        {
            // get the tag manager object from the project settings
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

            // get the tags array
            SerializedProperty tags = tagManager.FindProperty("tags");

            // check each existing tag
            for (int t = 0; t < tags.arraySize; t++)
            {
				Debug.Log(tags.GetArrayElementAtIndex(t).stringValue);
				// if the level point tag already exists, then stop
				if (tags.GetArrayElementAtIndex(t).stringValue == k_LevelPointTag)
				{
					Debug.LogWarning("The " + k_LevelPointTag + " tag already exists in this project.");
					return;
				}
            }

            // increase the size of the tag array
            tags.arraySize++;

            // get a reference to the newly added array element
            SerializedProperty newTag = tags.GetArrayElementAtIndex(tags.arraySize - 1);

            // set the value/tag name to the level point tag
            newTag.stringValue = k_LevelPointTag;

            // apply changes to the tag manager so the user cannot undo the change
            tagManager.ApplyModifiedPropertiesWithoutUndo();
        }

        private void CreatePointContainer()
        {
            // should only ever have one child, the point container, so delete it if it exists
            if (transform.childCount != 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }

            m_pointContainer = new GameObject("Point Container").transform;
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
        }

        private void ProcessSelection()
		{
			if (!Selection.activeGameObject)
				return;

			// if the selection is a LevelPoint
			// get the Id of the point from its name
			GameObject selection = Selection.activeGameObject;
			if (selection.CompareTag("LevelPoint"))
			{
				m_selectedPoint = int.Parse(selection.name[selection.name.Length - 1].ToString());
			}
			else
				m_selectedPoint = -1;
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
