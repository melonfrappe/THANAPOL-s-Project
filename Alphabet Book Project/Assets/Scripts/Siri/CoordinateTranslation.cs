using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Siri
{
	public class CoordinateTranslation
	{
		public static Vector3 ScreenToWord(Vector2 screenCoordinates,float distanceFromCamera,Camera customCamera = null)
		{
			if (customCamera == null)
				customCamera = Camera.main;

			Ray ray  = customCamera.ScreenPointToRay(screenCoordinates);
			return ray.origin + (ray.direction * distanceFromCamera);
		}

		public static Vector2 WorldToScreen(Vector3 worldPos,Camera customCamera = null)
		{
			if (customCamera == null)
				customCamera = Camera.main;

			return customCamera.WorldToScreenPoint(worldPos);
		}
		public static Vector2 Vector2FromAngle(float a)
		{
			a *= Mathf.Deg2Rad;
			return new Vector2(Mathf.Cos(a),Mathf.Sin(a));

		}

		public static Vector2 WorldToCanvasPosition(RectTransform refRect,Camera camera,Vector3 position)
		{
			Vector2 temp = camera.WorldToViewportPoint(position);

			temp.x *= refRect.sizeDelta.x;
			temp.y *= refRect.sizeDelta.y;

			temp.x -= refRect.sizeDelta.x * refRect.pivot.x;
			temp.y -= refRect.sizeDelta.y * refRect.pivot.y;

			return temp;
		}
	}
}