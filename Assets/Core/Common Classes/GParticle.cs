using UnityEngine;
using System.Collections;

public class GParticle : MonoBehaviour {
	ParticleSystem m_particle;
	[SerializeField] string m_sorting_layer = "";
	[SerializeField] int m_sorting_order = 0;
	[SerializeField] bool m_detach_from_parent = false;
	[SerializeField] float m_duration=-1;
	
	GameTimer m_timer;

	void Start () {
		m_particle = GetComponent<ParticleSystem>();
		if(m_detach_from_parent) {
			transform.parent=null;
		}
		m_particle.GetComponent<Renderer>().sortingLayerName = m_sorting_layer;
		m_particle.GetComponent<Renderer>().sortingOrder = m_sorting_order;
		
		if(m_duration>=0) {
			m_timer = new GameTimer(m_duration,false,EndTimer);
		}
	}
	
	void Update () {
		if (!GetComponent<ParticleSystem> ().isPlaying) {
			//GameObject.Destroy(gameObject);
		}
		if(m_timer!=null) {m_timer.Update();}
	}
	
	void EndTimer() {
		GameObject.Destroy(gameObject);
	}
}
