///<summary>
/// Author: Halen
///
///
///
///</summary>

using System.Linq;
using UnityEngine;

namespace CarRunner2D
{
	[System.Serializable]
	public class TerrainData
	{
		private string m_name;
		public string Name
		{
			get { return m_name; }
			set
			{
				if (value.All(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)))
					m_name = value;
				else
					Debug.LogWarning("Terrain Data name can only contain letters, numbers, or spaces. Terrain Id: " + m_terrainId);
			}
		}
		private int m_terrainId;
		public int TerrainId
		{
			get { return m_terrainId; }
			set
			{
				if (value < 0)
				{
					Debug.LogError("Id for a Terrain Data cannot be less than 0, as it represents an index. Terrain Id: " + m_terrainId);
					return;
				}

				m_terrainId = value;
			}
		}
		
		private Vector2[] m_points;

		public TerrainData(string name, int terrainId)
		{
			m_name = name;
			this.m_terrainId = terrainId;
		}

		public void SavePoints(Vector2[] points)
		{
			m_points = points;
		}

		public Vector2[] GetPoints() { return m_points; }

		public static void SaveTerrainData(TerrainData terrainData, string path)
		{

		}
	}
}
