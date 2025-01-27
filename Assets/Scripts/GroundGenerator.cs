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
	public static class GroundGenerator
	{
		public static GameObject GenerateFloorObject(Vector2[] points, SpriteShape spriteShape, float floorHeight)
		{
			GameObject floor = new GameObject("Floor")
			{
				layer = LayerMask.NameToLayer("Terrain")
			};

            SpriteShapeController spriteShapeController = floor.AddComponent<SpriteShapeController>();
            CreateFloorTerrain(spriteShapeController, points, spriteShape, floorHeight);
            floor.AddComponent<EdgeCollider2D>();
            spriteShapeController.autoUpdateCollider = true;

			return floor;
        }
		
		public static GameObject GenerateGroundObject(Vector2[] points, SpriteShape spriteShape, int groundId)
		{
            GameObject floor = new GameObject("Ground " + groundId)
            {
                layer = LayerMask.NameToLayer("Terrain")
            };

            SpriteShapeController spriteShapeController = floor.AddComponent<SpriteShapeController>();
            CreateTerrain(spriteShapeController, points, spriteShape);
            floor.AddComponent<EdgeCollider2D>();
            spriteShapeController.autoUpdateCollider = true;

            return floor;
        }

		private static void CreateFloorTerrain(SpriteShapeController controller, Vector2[] points, SpriteShape spriteShape, float floorHeight)
		{
			CreateTerrain(controller, points, spriteShape);

			// add floor points
			Vector2 firstPos = points[0];
			Vector2 lastPos = points[points.Length - 1];

			Vector2 lowestPos = (firstPos.y < lastPos.y) ? firstPos : lastPos;
			lowestPos.y -= floorHeight;

			// bottom right point
			controller.spline.InsertPointAt(points.Length, new Vector2(lastPos.x, lowestPos.y));
			controller.spline.InsertPointAt(points.Length + 1, new Vector2(firstPos.x, lowestPos.y));
		}
		
		private static void CreateTerrain(SpriteShapeController controller, Vector2[] points, SpriteShape spriteShape)
		{
			controller.spriteShape = spriteShape;
			controller.cornerAngleThreshold = 0;
			controller.colliderOffset = -0.5f;
			controller.fillPixelsPerUnit = 256;

			controller.spline.Clear();
			for (int p = 0; p < points.Length; p++)
			{
				controller.spline.InsertPointAt(p, points[p]);
				controller.spline.SetTangentMode(p, ShapeTangentMode.Linear);
			}
		}
	}
}
