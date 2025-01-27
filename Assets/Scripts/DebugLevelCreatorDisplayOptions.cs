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
	public class DebugLevelCreatorDisplayOptions
	{
		public float pointRadius;
		public Color pointColour;
		public Color lineColour;

		public DebugLevelCreatorDisplayOptions(float pointRadius = 0.03f, Color pointColour = default, Color lineColour = default)
		{
			this.pointRadius = pointRadius;
			this.pointColour = (pointColour == default ? Color.white : pointColour);
			this.lineColour = (lineColour == default ? Color.white : lineColour);
		}

		public void Reset()
		{
			var newDisplayOptions = new DebugLevelCreatorDisplayOptions();
			pointRadius = newDisplayOptions.pointRadius;
			pointColour = newDisplayOptions.pointColour;
			lineColour = newDisplayOptions.lineColour;
		}
    }
}
