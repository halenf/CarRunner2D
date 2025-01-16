///<summary>
/// Author: Halen
///
///
///
///</summary>

using UnityEngine;

namespace CarRunner2D
{
    [ExecuteAlways]
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform m_target;
        [SerializeField] private Vector3 m_offset;
        [SerializeField] private float m_verticalRange;
        [SerializeField] private bool m_canGoBackwards = false;

        private void Update()
        {
            // update the camera's position
            Vector3 newPosition = Vector3.zero;
            UpdateHorizontalPosition(ref newPosition);
            UpdateVerticalPosition(ref newPosition);
            newPosition += m_offset;
            transform.position = newPosition;
        }

        private void OnValidate()
        {
            // check if target exists before continuing
            if (!m_target)
                return;

            Update();
        }

        private void UpdateHorizontalPosition(ref Vector3 position)
        {
            position += new Vector3(m_target.position.x, 0);
        }

        private void UpdateVerticalPosition(ref Vector3 position)
        {
            position += new Vector3(0, m_target.position.y);
        }

        private bool CameraIsBehindTarget()
        {
            return (transform.position - m_offset).x <= m_target.position.x;
        }

        private bool CameraIsOutsideVerticalRange()
        {
            return transform.position.y - m_verticalRange > m_target.position.y ||
                transform.position.y + m_verticalRange < m_target.position.y;
        }
    }
}
