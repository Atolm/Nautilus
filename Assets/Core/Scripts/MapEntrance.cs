using UnityEngine;
using System.Collections;

public class MapEntrance : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void OnTriggerEnter(Collider c) {
		if(c.gameObject.tag=="Player") {
			GPlayer player = c.gameObject.GetComponent<GPlayer>();
			if(player) {
				player.m_at_entrance = true;
			}
		}
	}
	
	void OnTriggerExit(Collider c) {
		if(c.gameObject.tag=="Player") {
			GPlayer player = c.gameObject.GetComponent<GPlayer>();
			if(player) {
				player.m_at_entrance = false;
			}
		}
	}
}
