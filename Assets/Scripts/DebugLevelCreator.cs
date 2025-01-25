///<summary>
/// Author: Halen
///
///
///
///</summary>

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace CarRunner2D
{
	[ExecuteInEditMode]
	public class DebugLevelCreator : MonoBehaviour
	{
        #region Singleton
		public static DebugLevelCreator Instance;

        private void Awake()
        {
			Initialise();
        }

		[ContextMenu("Initialise")]
		public void Initialise()
        {
			// create singleton
			if (Instance && Instance != this)
			{
				DestroyImmediate(gameObject);
			}
			else
			{
				Instance = this;
				if (Application.isPlaying)
					DontDestroyOnLoad(gameObject);
			}

			// setup Point Container
			if (!m_pointContainer)
			{
				CreatePointContainer();
			}

			// remove before adding to prevent adding more than once
			Selection.selectionChanged -= ProcessSelection;
			Selection.selectionChanged += ProcessSelection;

			Debug.Log(name + " was initialised!");
		}
		#endregion

		// Level Point object Tag
		private string m_levelPointTag;
		public string LevelPointTag => m_levelPointTag;

		// container for Level Point GameObjects
		private Transform m_pointContainer;

		// currently selected Level Point Id
		private int m_selectedPoint;
		public int SelectedPoint => m_selectedPoint;

		// List of all Level Point Transforms
        private List<Transform> m_points;

        private void Update()
        {		
			// if point container has been deleted
			if (m_pointContainer == null)
            {
				CreatePointContainer();
            }
			
			if (m_pointContainer.childCount != m_points.Count)
			{
				// DEBUG
				for (int p = 0; p < m_points.Count; p++)
				{
					if (m_points[p] == null)
						Debug.Log("Point with Id " + p.ToString() + " was deleted!");
				}
				
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
				Debug.Log("Selected point Id: " + m_selectedPoint.ToString() +
				" at position: " + ((Vector2)m_points[m_selectedPoint].position).ToString());
			}
			else
				m_selectedPoint = -1;
		}

		public void CreatePoint()
        {
			// put the point in the centre of the screen
			Camera sceneCamera = SceneView.lastActiveSceneView.camera;
			Vector3 position = sceneCamera.ViewportToWorldPoint(new Vector2(0.5f, 0.5f));
			position.z = 0;

			// create the new point in the scene
			m_selectedPoint = m_points.Count;
			GameObject newPoint = new GameObject("Point " + m_selectedPoint.ToString());
			newPoint.transform.SetParent(m_pointContainer);
			
			// add point details
			newPoint.transform.position = position;
			newPoint.tag = m_levelPointTag.ToString();

			// add the new point to the list
			m_points.Add(newPoint.transform);

			Debug.Log("Created point at " + position.ToString() + " with Id " + m_selectedPoint.ToString());
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="removedPointId">0 will rename all points</param>
		private void RenamePoints(int removedPointId)
        {
			// keep range to avoid errors
			if (removedPointId < 0)
				removedPointId = 0;

			for (int t = removedPointId; t < m_points.Count; t++)
			{
				m_points[t].name = "Point " + t;
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

			Debug.Log("Deleted ALL points!");
        }

		private void CreatePointContainer()
        {
			m_pointContainer = new GameObject("Point Container").transform;
			m_pointContainer.SetParent(transform);
		}

		public void SetNewLevelPointTag(string tag)
        {
			m_levelPointTag = tag;
			Debug.Log("The Level Point GameObject tag was changed to " + tag + "!");

			// change the tags on all the existing level points
			foreach (Transform t in m_pointContainer)
				t.tag = tag;
        }

        private void OnDestroy()
        {
			Selection.selectionChanged -= ProcessSelection;
        }

        private void OnDrawGizmos()
        {
			// scale the size of the gizmo with the 'zoom' level
			Camera sceneCamera = SceneView.lastActiveSceneView.camera;
			float discRadius = sceneCamera.orthographicSize * 0.1f;
			
			// draw the first point
			if (m_points.Count != 0)
				Handles.DrawSolidDisc(m_points[0].position, Vector3.back, discRadius);

			// draw lines between the points
			for (int p = 0; p < m_points.Count - 1; p++)
            {
				Gizmos.DrawLine(m_points[p].position, m_points[p + 1].position);
				Handles.DrawSolidDisc(m_points[p + 1].position, Vector3.back, discRadius);
			}
		}
    }
}
