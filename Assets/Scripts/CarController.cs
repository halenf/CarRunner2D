///<summary>
/// Author: Halen
///
/// make car go yay
///
///</summary>

using UnityEngine;
using UnityEngine.InputSystem;

namespace CarRunner2D
{
	public class CarController : MonoBehaviour
	{
		// references
		[SerializeField] private Rigidbody2D m_frontWheel;
		[SerializeField] private Rigidbody2D m_backWheel;

		private Rigidbody2D m_carBody;

		[Space]

		[SerializeField, Min(0)] private float m_acceleration;
		[SerializeField, Min(0)] private float m_maxDriveSpeed;
		[SerializeField, Min(0)] private float m_spinSpeed;

		// movement values
		private float m_targetWheelSpeed;
		private float m_currentAcceleration;

		// inputs values
		private bool m_isAccelerating;
		private bool m_isBraking;
		private float m_spinDirection;

        private void Awake()
        {
            m_carBody = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
			// wheel torque
			if (m_isAccelerating)
			{
				m_currentAcceleration += m_acceleration * Time.deltaTime;
				
				//float 
				float frontTorque = m_acceleration * m_maxDriveSpeed / m_frontWheel.angularVelocity;
				m_frontWheel.AddTorque(-frontTorque * Time.fixedDeltaTime);

				float backTorque = m_acceleration * m_maxDriveSpeed / m_backWheel.angularVelocity;
				m_backWheel.AddTorque(-backTorque * Time.fixedDeltaTime);
			}
            if (m_isBraking)
            {
				m_frontWheel.angularVelocity = 0;
				m_backWheel.angularVelocity = 0;
            }

            // car body spin
            m_carBody.AddTorque(m_spinDirection * m_spinSpeed * Time.fixedDeltaTime);
        }

        public void GetAccelerating(InputAction.CallbackContext context)
        {
			if (context.performed)
				m_isAccelerating = true;
			if (context.canceled)
				m_isAccelerating = false;
        }

		public void GetBraking(InputAction.CallbackContext context)
		{
            if (context.performed)
                m_isBraking = true;
            if (context.canceled)
                m_isBraking = false;
        }

		public void GetSpinDirection(InputAction.CallbackContext context)
		{
			m_spinDirection = context.ReadValue<float>();
		}
    }
}
