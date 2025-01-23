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
			if (m_points.Count != 0)
				m_points.Clear();
			
			if (Instance)
				Destroy(gameObject);
			else
			{
				Instance = this;
				if (Application.isPlaying)
					DontDestroyOnLoad(gameObject);
			}
		}
		#endregion

		[SerializeField] private GameObject m_pointPrefab; 
		[SerializeField] private Canvas m_pointToolsCanvas;
		private int m_selectedPoint;

        private List<Transform> m_points;

		//private bool m_mouseIsHeld = false;
		private bool m_movingPoint = false;

        private void Update()
        {
            if (m_movingPoint)
            {
				UpdateCanvasPosition();
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
							EnableCanvasForPoint(pointId);
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
			Vector3 position = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
			Transform newPoint = Instantiate(m_pointPrefab, position, Quaternion.identity).transform;
			newPoint.name = "Point " + m_points.Count;
			m_points.Add(newPoint);
			Debug.Log("Created point at " + position.ToString() + " with Id " + newPoint.name[-1].ToString() + "!");
        }

		public void DeletePoint()
        {
			// TODO: Update the point names with the correct ID
			
			if (m_selectedPoint != -1)
				m_points.RemoveAt(m_selectedPoint);
			else
				Debug.LogWarning("Tried to delete a point without one selected! Id: " + m_selectedPoint.ToString());
        }

		public void ClearPoints()
        {
			m_points.Clear();
        }

		private void EnableCanvasForPoint(int pointId)
        {
			m_selectedPoint = pointId;
			m_pointToolsCanvas.gameObject.SetActive(true);
			Debug.Log("Selected point Id: " + m_selectedPoint.ToString() + ",\n" +
				"at position: " + m_points[pointId].ToString());
		}

		private void InvalidClick()
        {
			m_selectedPoint = -1;
			m_pointToolsCanvas.gameObject.SetActive(false);
        }

		private void UpdateCanvasPosition()
        {
			m_pointToolsCanvas.transform.position = m_points[m_selectedPoint].transform.position;
        }

        private void OnDrawGizmos()
        {
            foreach (Transform point in m_points)
            {
				
            }
        }
    }
}
