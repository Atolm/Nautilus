using UnityEngine;
using System.Collections;

public class MapExit : MonoBehaviour {
	//float m_activate_range = 3;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		/* OLD
		GameObject target = GameObject.FindGameObjectWithTag("Player");
		if(target==null) {return;}
		
		if(Vector3.Distance(transform.position,target.transform.position)<m_activate_range) {
			//Application.LoadLevel("test_scene");
		}
		*/
	}
	
	void OnTriggerEnter(Collider c) {
		if(c.gameObject.tag=="Player") {
			GPlayer player = c.gameObject.GetComponent<GPlayer>();
			if(player) {
				player.m_at_exit = true;
			}
		}
	}
	
	void OnTriggerExit(Collider c) {
		if(c.gameObject.tag=="Player") {
			GPlayer player = c.gameObject.GetComponent<GPlayer>();
			if(player) {
				player.m_at_exit = false;
			}
		}
	}
}
