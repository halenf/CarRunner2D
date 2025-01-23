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

		private float m_lastAcceleration;
		public float LastAcceleration { get { return m_lastAcceleration; } }

#if UNITY_EDITOR
		private bool m_isInitialised = false;
		public bool IsInitialised { get { return m_isInitialised; } }
#endif

		public float Speed
		{
			get { return m_rb.angularVelocity; }
			set
			{
				if (IsLocked)
					return;
				
				float actualSpeed = Mathf.Clamp(value, -m_maxAngularVelocity, m_maxAngularVelocity);
				m_lastAcceleration = actualSpeed - m_rb.angularVelocity;
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

#if UNITY_EDITOR
			m_isInitialised = true;
#endif
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
