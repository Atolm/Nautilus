using UnityEngine;
using System.Collections;

public class UIEnergy : MonoBehaviour {
	Vector2 m_base_size;
	RectTransform rtransform;

	void Start () {
		rtransform = GetComponent<RectTransform>();
		m_base_size = rtransform.sizeDelta;
	
	}
	
	void Update () {
		if(Game.player==null) {return;}
		float perc = 0.1f+(Game.player.m_energy.GetPercent()*0.9f);
		Vector2 new_size = new Vector2(m_base_size.x*perc,m_base_size.y);
		rtransform.sizeDelta = new_size;
	}
}
