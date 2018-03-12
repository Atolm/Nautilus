using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Util {

	public static void Print(string text) {
		Debug.Log (text);
	}

	public static float GetAngle2D (Vector2 start, Vector2 end) {
		//return the angle between two vector 2 points.
		return Mathf.Atan2(start.x-end.x, start.y-end.y) * Mathf.Rad2Deg+180;
	}

	public static float GetAngle3D (Vector3 start, Vector3 end) {
		//return the angle between two vector 3 points. Angle returned is top-down 2D angle.
		return Mathf.Atan2(start.x-end.x, start.z-end.z) * Mathf.Rad2Deg+180;
	}
	
	public static Vector3 GetDirectionFromRotation (Quaternion rotation, Vector3 axis) {
		return rotation * axis;
	}

	///Turn a Vector2 into a Vector3 in the obvious way (x=x, y=z)
	public static Vector3 ToVector3(Vector2 vector2) {
		return new Vector3(vector2.x,0,vector2.y);
	}

	///Turn a Vector3 into a Vector2 in the obvious way (x=x, z=y)
	public static Vector2 ToVector2(Vector3 vector3) {
		return new Vector2(vector3.x,vector3.z);
	}

	///Used in 2.5D games - rotates a direction to compensate for the camera's y rotation.
	public static Vector3 RotateDirectionToMatchCamera(Vector3 vector3) {
		return Quaternion.AngleAxis(-Camera.main.transform.rotation.eulerAngles.y,Vector3.up)* vector3;
	}
	public static Vector3 RotateDirectionToMatchCamera(Vector2 vector2) {
		Vector3 v3 = ToVector3(vector2);
		return RotateDirectionToMatchCamera(v3);
	}

	///Used in 2.5D games - rotates a direction to compensate for the camera's y rotation.
	public static Vector3 RotateCameraSpaceToMatchWorld(Vector3 vector3) {
		return Quaternion.AngleAxis(Camera.main.transform.rotation.eulerAngles.y,Vector3.up)* vector3;
	}
	public static Vector3 RotateCameraSpaceToMatchWorld(Vector2 vector2) {
		Vector3 v3 = ToVector3(vector2);
		return RotateDirectionToMatchCamera(v3);
	}

	public static GameObject[] GetAllGameObjects(Component comp) {
		List<GameObject> targets = new List<GameObject>();

		object[] obj = GameObject.FindSceneObjectsOfType(typeof (GameObject));
		foreach (object o in obj)
		{
			GameObject g = (GameObject) o;
			targets.Add(g);
		}

		GameObject[] output = targets.ToArray ();
		return output;
	}

	public static GameObject[] GetObjectsInRadius(Vector3 point, float radius) {
		List<GameObject> targets = new List<GameObject>();

		object[] obj = GameObject.FindSceneObjectsOfType(typeof (GameObject));
		foreach (object o in obj)
		{
			GameObject g = (GameObject) o;
			if(Vector3.Distance(point,g.transform.position)<=radius){
				targets.Add(g);
			}
		}
		GameObject[] output = targets.ToArray ();
		return output;
	}

	public static void Shuffle<T>(this IList<T> list)  {
		
		int n = list.Count;  
		while (n > 1) {  
			n--;  
			int k = Random.Range(0,n);
			T value = list[k];  
			list[k] = list[n];  
			list[n] = value;  
		}  
	}
}

//Two value min/max class.
[System.Serializable] 
public class MinMax {
	[SerializeField] public float min;
	[SerializeField] public float max;
	public MinMax(float min,float max) {
		this.min = min;
		this.max = max;
	}
}

