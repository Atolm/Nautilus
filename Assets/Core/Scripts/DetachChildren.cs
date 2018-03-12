using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class DetachChildren : MonoBehaviour {
	[SerializeField] float m_detach_after_time = 1;
	[SerializeField] bool m_destroy_on_detach = true;
	GameTimer m_detach_timer;

	// Use this for initialization
	void Start () {
		m_detach_timer = new GameTimer(m_detach_after_time,false,Detach);
	}
	
	// Update is called once per frame
	void Update () {
		m_detach_timer.Update();
	}
	
	void Detach() {
		List<Transform> children = new List<Transform>();
		foreach(Transform t in transform) {
			children.Add(t);
		}
		foreach(Transform t in children) {
			t.parent=null;
		}
		
		if(m_destroy_on_detach) {
			Destroy(gameObject);
		}
	}
}
