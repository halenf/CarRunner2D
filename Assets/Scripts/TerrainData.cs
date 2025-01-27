///<summary>
/// Author: Halen
///
///
///
///</summary>

using UnityEngine;

namespace CarRunner2D
{
	[System.Serializable]
	public class TerrainData
	{
		public readonly int terrainId;
		
		private Vector2[] m_points;

		public TerrainData(int terrainId)
		{
			this.terrainId = terrainId;
		}

		public void SavePoints(Vector2[] points)
		{
			m_points = points;
		}

		public Vector2[] GetPoints() { return m_points; }
	}
}
