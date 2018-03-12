using UnityEngine;
using System.Collections;

public class UIHealth : MonoBehaviour {
	Vector2 m_base_size;
	float m_displayed_hp = 1;
	RectTransform m_rect;

	void Start () {
		m_rect = GetComponent<RectTransform>();
		m_base_size = m_rect.sizeDelta;
	}
	
	void Update () {
		if(Game.player==null) {return;}
		
		float perc = 0.1f+(Game.player.m_health.GetPercent()*0.9f);
		m_displayed_hp = Mathf.Lerp(m_displayed_hp,perc,Mathf.Min(TurnTime.deltaTime*2,1));
		
		Vector2 new_size = new Vector2(m_base_size.x*m_displayed_hp,m_base_size.y);
		m_rect.sizeDelta = new_size;
	}
}
