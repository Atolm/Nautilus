using UnityEngine;
using System.Collections;

public class HealthUp : MonoBehaviour {
	public int m_healing_value = 10;
	public bool m_used = false;
	
	GameVisual m_visual;
	
	void Start() {
		m_visual = GetComponent<GameVisual>();
	}
	
	void OnTriggerEnter(Collider c) {
		if(m_used) { return;}
		
		if(c.gameObject.tag=="Player") {
			c.gameObject.GetComponent<GEntity>().m_health.ChangeValue(m_healing_value);
			m_used=true;
			
			if(m_visual) {
				m_visual.Activate(true);
			}
		}
	}
}
