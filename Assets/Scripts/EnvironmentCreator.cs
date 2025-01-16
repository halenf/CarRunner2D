///<summary>
/// Author: Halen
///
///
///
///</summary>

using UnityEngine;
using UnityEngine.U2D;

namespace CarRunner2D
{
	[ExecuteInEditMode]
	public class EnvironmentCreator : MonoBehaviour
	{
		[SerializeField] private SpriteShapeController m_spriteShapeController;

		[SerializeField, Min(3)] private int m_levelPointLength = 50;
		[SerializeField, Range(0.1f, 50)] private float m_xMultiplier = 2f;
		[SerializeField, Range(0.1f, 50)] private float m_yMultiplier = 2f;
		[SerializeField, Range(0, 1)] private float m_curveSmoothness = 0.5f;
		[SerializeField] private float m_noiseStep = 0.5f;
		[SerializeField] private float m_bottom = 10f;

		private Vector3 m_lastPos;

		[ContextMenu("Generate")]
        private void OnValidate()
        {
			// check it has sprite shape controller before continuing
			if (!m_spriteShapeController)
				return;
			
			m_spriteShapeController.spline.Clear();

			// add a point on the spline for the level length
			for (int p = 0; p < m_levelPointLength; p++)
			{
				// create a point using perlin noise
				m_lastPos = transform.position + new Vector3(p * m_xMultiplier, Mathf.PerlinNoise(0, p * m_noiseStep) * m_yMultiplier);
				m_spriteShapeController.spline.InsertPointAt(p, m_lastPos);

				// set the tangent mode to continous if the point is not the first or last point
				if (p != 0 && p != m_levelPointLength - 1)
				{
					m_spriteShapeController.spline.SetTangentMode(p, ShapeTangentMode.Continuous);
					m_spriteShapeController.spline.SetLeftTangent(p, m_curveSmoothness * m_xMultiplier * Vector3.left); ;
					m_spriteShapeController.spline.SetRightTangent(p, m_curveSmoothness * m_xMultiplier * Vector3.right);
				}
			}

			// insert the bottom right point
			m_spriteShapeController.spline.InsertPointAt(m_levelPointLength, new(m_lastPos.x, transform.position.y - m_bottom));

			// insert the bottom left point
			m_spriteShapeController.spline.InsertPointAt(m_levelPointLength + 1, new(transform.position.x, transform.position.y - m_bottom));
        }
    }
}
