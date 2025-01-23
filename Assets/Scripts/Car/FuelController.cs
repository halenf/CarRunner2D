///<summary>
/// Author: Halen
///
///
///
///</summary>

using UnityEngine;
using UnityEngine.UI;

namespace CarRunner2D
{
	public class FuelController : MonoBehaviour
	{
		[SerializeField, Min(0)] private float m_fuelDrainSpeed = 1f;
		[SerializeField] private float m_maxFuelAmount = 100f;

        [Header("UI Options")]
		[SerializeField] private Image m_fuelFillBar;
        [SerializeField] private Gradient m_fuelFillBarGradient;

		private float m_currentFuelAmount;

        private void Start()
        {
            m_currentFuelAmount = m_maxFuelAmount;
            UpdateUI();
        }

        private void Update()
        {
            m_currentFuelAmount -= m_fuelDrainSpeed * Time.deltaTime;
            UpdateUI();
        }

        private void UpdateUI()
        {
            m_fuelFillBar.fillAmount = m_currentFuelAmount / m_maxFuelAmount;
            m_fuelFillBar.color = m_fuelFillBarGradient.Evaluate(m_fuelFillBar.fillAmount);
        }
    }
}
