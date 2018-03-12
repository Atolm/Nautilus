using UnityEngine;
using System.Collections;

public class EnergyUp : MonoBehaviour {
	public int m_energy_value = 10;
	public bool m_used = false;
	
	GameVisual m_visual;
	
	void Start() {
		m_visual = GetComponent<GameVisual>();
	}
	
	void OnTriggerEnter(Collider c) {
		if(m_used) { return;}
		
		if(c.gameObject.tag=="Player") {
			c.gameObject.GetComponent<GEntity>().m_energy.ChangeValue(m_energy_value);
			m_used=true;
			
			if(m_visual) {
				m_visual.Activate(true);
			}
		}
	}
}
