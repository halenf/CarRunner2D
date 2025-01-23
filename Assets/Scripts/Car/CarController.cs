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
		[SerializeField] private Wheel m_frontWheel;
		[SerializeField] private Wheel m_backWheel;

		private Rigidbody2D m_carBody;

        [Header("Driving")]
		[SerializeField, Min(0)] private float m_maxDriveSpeed;
		[SerializeField, Min(0)] private float m_acceleration;
        [SerializeField, Min(0)] private float m_decceleration;
        [SerializeField, Range(0, 1)] private float m_inAirAccelerationFactor;

        [Header("Car Body Spin")]
        [SerializeField, Min(0)] private float m_maxBodySpinSpeed;
		[SerializeField, Min(0)] private float m_bodySpinAcceleration;

		// inputs values
		private bool m_isAccelerating = false;
		private bool m_isBraking = false;
		private float m_spinDirection = 0;

        private void Awake()
        {
            m_carBody = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            m_frontWheel.SetMaxSpeed(m_maxDriveSpeed);
            m_backWheel.SetMaxSpeed(m_maxDriveSpeed);
        }

        private void FixedUpdate()
        {
            // reduce speed when braking
            if (m_isBraking)
            {
                float decceleration = m_decceleration * Time.fixedDeltaTime;
                m_frontWheel.Speed = Mathf.MoveTowards(m_frontWheel.Speed, 0, decceleration);
                m_backWheel.Speed = Mathf.MoveTowards(m_backWheel.Speed, 0, decceleration);   
                
                // lock wheels at some certain value
                /*
                if (Mathf.Abs(m_frontWheel.Speed) < decceleration)
                    m_frontWheel.IsLocked = true;
                if (Mathf.Abs(m_backWheel.Speed) < decceleration)
                    m_backWheel.IsLocked = true;
                */
            }

            // increase speed when accelerating
            else if (m_isAccelerating)
            {
                float frontAcceleration;
                float backAcceleration;
                frontAcceleration = backAcceleration = -m_acceleration * Time.fixedDeltaTime;

                // if wheel is not touching the ground, apply a different amount of acceleration
                if (!m_frontWheel.IsTouchingGround())
                    frontAcceleration *= m_inAirAccelerationFactor;
                if (!m_backWheel.IsTouchingGround())
                    backAcceleration *= m_inAirAccelerationFactor;

                m_frontWheel.Speed += frontAcceleration;
                m_backWheel.Speed += backAcceleration;
            }

            /// car body spin
            if (m_spinDirection != 0)
            {
                float carBodySpinSpeed = m_carBody.angularVelocity + m_spinDirection * m_bodySpinAcceleration * Time.fixedDeltaTime;
                carBodySpinSpeed = Mathf.Clamp(carBodySpinSpeed, -m_maxBodySpinSpeed, m_maxBodySpinSpeed);
                m_carBody.angularVelocity = carBodySpinSpeed;
            }
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
