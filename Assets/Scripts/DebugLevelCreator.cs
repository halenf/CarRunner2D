///<summary>
/// Author: Halen
///
///
///
///</summary>

using UnityEngine;

namespace CarRunner2D
{
	[ExecuteInEditMode]
	public class DebugLevelCreator : MonoBehaviour
	{
        #region Singleton
		public static DebugLevelCreator Instance;

        private void Awake()
        {
            if (Instance)
                Destroy(gameObject);
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }
        #endregion

        private Vector3 m_points;



        private void Start()
        {
            
        }
    }
}
