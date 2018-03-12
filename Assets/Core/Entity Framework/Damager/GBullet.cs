using UnityEngine;
using System.Collections;

public class GBullet : MonoBehaviour {
	[SerializeField] float m_speed;
	//Rigidbody m_rigidbody;
	
	bool m_autoaim_enabled = false;
	[SerializeField] float m_autoaim_max_angle;
	[SerializeField] float m_autoaim_distance;
	[SerializeField] float m_autoaim_turn_speed;

	void Start () {
		//m_rigidbody = GetComponent<Rigidbody>();
	}
	
	void Update () {
		transform.position += transform.forward * (m_speed * TurnTime.deltaTime);
		
		if(m_autoaim_enabled) {
			AutoAim();
		}
	}
	
	void AutoAim() {
		foreach(GameObject go in GameObject.FindGameObjectsWithTag("Enemy")) {
			if(Vector3.Distance(transform.position,go.transform.position) > m_autoaim_distance) { continue; } 
			
			float rot = Util.GetAngle3D(transform.position,go.transform.position);
			
			float rot_diff = Mathf.Abs(transform.rotation.eulerAngles.y-rot);
			
			if(rot_diff > 180) {
				rot_diff = 360-Mathf.Max(transform.rotation.eulerAngles.y-rot)+Mathf.Min(transform.rotation.eulerAngles.y-rot);
			}
			
			if(rot_diff > m_autoaim_max_angle ) { continue; }
			
			transform.rotation = Quaternion.Lerp(
				transform.rotation,
				Quaternion.Euler(0,rot,0),
				Mathf.Min(TurnTime.deltaTime*m_autoaim_turn_speed,1)
			); 
			return;
		}
	}
	
	public void SetAutoaimEnabled(bool state) {
		m_autoaim_enabled = state;
	}
}
