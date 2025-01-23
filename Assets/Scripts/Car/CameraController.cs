///<summary>
/// Author: Halen
///
///
///
///</summary>

using UnityEngine;

namespace CarRunner2D
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform m_target;
        [SerializeField] private Vector2 m_offset;

        [ContextMenu("Update Position")]
        private void Update()
        {
            transform.position = m_target.position + new Vector3(m_offset.x, m_offset.y);
            transform.position += new Vector3(0, 0, -8);
        }
    }
}
