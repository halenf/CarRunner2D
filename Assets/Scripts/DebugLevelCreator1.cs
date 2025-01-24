/*
///<summary>
/// Author: Halen
///
///
///
///</summary>

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

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
			// clear any existing values
			ClearPoints();

			// create singleton
			if (Instance && Instance != this)
			{
#if UNITY_EDITOR
				DestroyImmediate(gameObject);
#else
				Destroy(gameObject);
#endif
			}
			else
			{
				Instance = this;
				if (Application.isPlaying)
					DontDestroyOnLoad(gameObject);
			}

			Debug.Log(name + " was initialised!");
		}
#endregion

		[SerializeField] private GameObject m_pointPrefab;
		[SerializeField] private Transform m_pointContainer;
		[SerializeField] private RectTransform m_pointToolsTransform;

		private int m_selectedPoint;
		public int SelectedPoint => m_selectedPoint;

        private List<Transform> m_points;

		//private bool m_mouseIsHeld = false;
		private bool m_movingPoint = false;

        private void Update()
        {
            // move the canvas with the point when it is selected
			if (m_selectedPoint != -1)
            {
				UpdateCanvasPosition();
            }

			// when moving a point with the mouse
			if (m_movingPoint)
            {
				Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
				m_points[m_selectedPoint].position = new Vector3(worldPosition.x, worldPosition.y, 0);
            }

			// check for mouse
			// can't use Unity Events to detect these in editor,
			// will be changed for in-game version
			if (Mouse.current.leftButton.wasPressedThisFrame)
            {
				OnLeftMouseButtonPressed();
            }
			if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
				OnLeftMouseButtonPressed();
            }
        }

        public void ProcessUIEvent(Vector2 mousePosition)
		{
			Vector3 rayPosition = new(mousePosition.x, mousePosition.y, 8);
			Ray click = Camera.main.ScreenPointToRay(rayPosition);

			// raycast into screen to get info of UI object that was found
			RaycastHit2D hit = Physics2D.GetRayIntersection(click);
			if (hit)
			{
				Debug.Log("Hit UI object with name \"" + hit.transform.name + "\".");
				switch (hit.transform.name)
                {
					case string a when a.Contains("Point"):
						if (int.TryParse(hit.transform.name[-1].ToString(), out int pointId))
							SelectPoint(pointId);
						break;
					case "Move":
						m_movingPoint = true;
						break;
					case "Delete":
						DeletePoint();
						break;
                }

				// did not click a UI object that matched
				InvalidClick();
			}
		}

		private void OnLeftMouseButtonPressed()
		{
			//m_mouseIsHeld = true;

			// check if the mouse is over a UI object
			// the Event System can only detect UI objects
			if (EventSystem.current.IsPointerOverGameObject())
			{
				ProcessUIEvent(Mouse.current.position.ReadValue());
			}
			// mouse is not over UI object, therefore cannot be over a point
			else
			{
				InvalidClick();
			}
		}

		private void OnLeftMouseButtonReleased()
        {
			//m_mouseIsHeld = false;
			m_movingPoint = false;
		}

		public void CreatePoint()
        {
			// put the point in the centre of the screen
			Vector3 position = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));

			// create the point with a new Id at the position
			int pointId = m_points.Count;
			Transform newPoint = Instantiate(m_pointPrefab, m_pointContainer).transform;
			newPoint.position = position;
			newPoint.name = "Point " + pointId;
			m_points.Add(newPoint);

			// define the collider
			CircleCollider2D newPointCollider = newPoint.gameObject.AddComponent<CircleCollider2D>();
			// TODO: add logic that scales this with the scene camera's orthographic size
			newPointCollider.radius = sceneCamera.orthographicSize * 0.1f;

			// select the point after creation
			SelectPoint(pointId);

			Debug.Log("Created point at " + position.ToString() + " with Id " + pointId.ToString() + "!");
        }

		public void DeletePoint()
        {
			if (m_selectedPoint != -1)
			{
				// remove the point from the list
				m_points.RemoveAt(m_selectedPoint);

				// update the remaining points' names with the correct Id
				// only the points after the deleted one will have incorrect Ids
				for (int t = m_selectedPoint; t < m_points.Count; t++)
                {
					m_points[t].name = "Point " + t;
                }

				// deselect the point
				InvalidClick();
			}
			else
				Debug.LogWarning("Tried to delete a point without one selected! Id: " + m_selectedPoint.ToString());
        }

		public void ClearPoints()
        {
			// destroy all the point objects
			for (int t = m_pointContainer.childCount; t > 0; t--)
            {
#if UNITY_EDITOR
				DestroyImmediate(m_pointContainer.GetChild(t - 1).gameObject);
#else
				Destroy(m_pointContainer.GetChild(t - 1).gameObject);
#endif
            }

			// empty the list of points
			m_points.Clear();

			// deselect any point and reset the tools canvas
			InvalidClick();
        }

		private void SelectPoint(int pointId)
        {
			m_selectedPoint = pointId;
			m_pointToolsTransform.gameObject.SetActive(true);

			Debug.Log("Selected point Id: " + m_selectedPoint.ToString() + ",\n" +
				"at position: " + m_points[pointId].ToString());
		}

		private void InvalidClick()
        {
			m_selectedPoint = -1;
			m_pointToolsTransform.gameObject.SetActive(false);
			m_movingPoint = false;
        }

		private void UpdateCanvasPosition()
        {
			Vector3 position = RectTransformUtility.WorldToScreenPoint(Camera.main, m_points[m_selectedPoint].position);

			m_pointToolsTransform.position = position;
        }

        private void OnDrawGizmos()
        {
			// draw lines between the points
			if (m_points.Count < 2)
				return;

			for (int p = 0; p < m_points.Count - 1; p++)
            {
				Gizmos.DrawLine(m_points[p].position, m_points[p + 1].position);
            }
        }
    }
}
*/