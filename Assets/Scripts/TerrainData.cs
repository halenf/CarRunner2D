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
		public TerrainData(string name, Vector2[] points)
		{
			m_name = name;
			m_points = points;
		}

		private string m_name;
		public string Name
		{
			get { return m_name; }
			set
			{
				if (value.All(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)))
					m_name = value;
				else
					Debug.LogWarning("Terrain Data name can only contain letters, numbers, or spaces. Terrain's Current Name: " + m_name);
			}
		}
		
		private Vector2[] m_points;
		public Vector2[] Points
		{
			get { return m_points; }
			set
			{
				m_points = value;
			}
		}

	}
}
