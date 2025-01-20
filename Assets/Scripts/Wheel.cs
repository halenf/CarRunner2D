///<summary>
/// Author: Halen
///
///
///
///</summary>

using UnityEngine;

namespace CarRunner2D
{
	public class Wheel : MonoBehaviour
	{
		[SerializeField] private WheelJoint2D m_joint;
		private Rigidbody2D m_rb;
		private CircleCollider2D m_circleCollider;

		private float m_maxAngularVelocity;

		public float Speed
		{
			get { return m_rb.angularVelocity; }
			set
			{
				if (IsLocked)
					return;
				
				float actualSpeed = Mathf.Clamp(value, -m_maxAngularVelocity, m_maxAngularVelocity);
				m_rb.angularVelocity = actualSpeed;
			}
		}

		public bool IsLocked
		{
			get { return m_rb.freezeRotation; }
			set { m_rb.freezeRotation = value; }
		}

        private void Awake()
        {
			m_rb = GetComponent<Rigidbody2D>();
			m_circleCollider = GetComponent<CircleCollider2D>();
        }

		public void SetMaxSpeed(float speed)
		{
			m_maxAngularVelocity = speed;
		}

        public bool IsTouchingGround()
		{
			if (Physics2D.OverlapCircle(new Vector2(transform.position.x, transform.position.y), m_circleCollider.radius, LayerMask.NameToLayer("Ground")))
				return true;
			else return false;
		}
	}
}
