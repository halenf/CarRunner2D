/// <summary>
/// Author: Halen
/// 
/// 
/// 
/// </summary>

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace CarRunner2D
{
    public class TerrainSaveLoad
    {
        [System.Serializable]
        private class SaveData
        {
            public SaveData(string name, Vector2[] points)
            {
                this.name = name;

                pointsX = new float[points.Length];
                pointsY = new float[points.Length];
                for (int i = 0; i < points.Length; i++)
                {
                    pointsX[i] = points[i].x;
                    pointsY[i] = points[i].y;
                }
            }
            
            public string name;
            public float[] pointsX;
            public float[] pointsY;
        }

        public static void SerialiseTerrainData(TerrainData terrain)
        {
            SaveData data = new SaveData(terrain.Name, terrain.Points);
            
            BinaryFormatter bin = new BinaryFormatter();
            FileStream stream = new FileStream(DebugLevelCreator.k_TerrainDataSavePath + "/" + terrain.Name + ".cter", FileMode.Create);

            bin.Serialize(stream, data);
            stream.Close();

            Debug.Log("Saved Terrain Data: " + terrain.Name);
        }

        public static TerrainData LoadSerialisedTerrainData(string fileName)
        {
            string saveDirectory = DebugLevelCreator.k_TerrainDataSavePath;
            string savePath = saveDirectory + "/" + fileName;
            if (File.Exists(savePath))
            {
                BinaryFormatter bin = new BinaryFormatter();
                FileStream stream = new FileStream(savePath, FileMode.Open);

                SaveData data = (SaveData)bin.Deserialize(stream);

                stream.Close();

                Vector2[] points = new Vector2[data.pointsX.Length];
                for (int i = 0; i < data.pointsX.Length; i++)
                    points[i] = new Vector2(data.pointsX[i], data.pointsY[i]);

                TerrainData terrain = new TerrainData(data.name, points);

                Debug.Log("Terrain with name " + terrain.Name + " was loaded!");
                return terrain;
            }
            else
            {
                Debug.LogWarning("Terrain Data at path '" + saveDirectory + "/" + fileName + "' does not exist!");
                return null;
            }
        }
    }
}
