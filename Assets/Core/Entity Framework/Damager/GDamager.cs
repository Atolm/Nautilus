using UnityEngine;
using System.Collections;

public class GDamager : MonoBehaviour {
	int m_damage = 10;
	bool m_active = true;
	[SerializeField] bool m_destroy_on_enter = true;
	
	[SerializeField] bool m_damage_on_enter = true;
	[SerializeField] bool m_damage_on_exit = false;
	[SerializeField] bool m_damage_on_stay = false;
	
	[SerializeField] float m_damage_begin_time = 0;
	[SerializeField] float m_damage_end_time = -1; //-1==never
	[SerializeField] float m_stay_damage_repeat_time = -1; //-1 = never repeat
	[SerializeField] float m_lifetime = 1; //-1 = infinite
	[SerializeField] bool friendly_fire = false;
	
	[SerializeField] GameObject m_hit_particle;
	
	GameTimer m_stay_damage_begin_timer;
	GameTimer m_stay_damage_repeat_timer;
	GameTimer m_life_timer;
	
	GEntity m_owner = null;

	protected virtual void Start () {
		
		m_stay_damage_begin_timer = new GameTimer(m_damage_begin_time,false,null);
		
		m_stay_damage_repeat_timer = new GameTimer(m_stay_damage_repeat_time,true,null);
		m_stay_damage_repeat_timer.SetTimeComplete(false);
		if(m_stay_damage_repeat_time==-1) {
			m_stay_damage_repeat_timer.SetActive(false);
		}
		
		m_life_timer = new GameTimer(m_lifetime,false,EndLifetime);
		if(m_lifetime==-1) {
			m_life_timer.SetActive(false);
		}
	}
	
	protected virtual void Update () {
		m_life_timer.Update();
		m_stay_damage_begin_timer.Update();
		if(m_stay_damage_begin_timer.IsComplete()) {
			m_stay_damage_repeat_timer.Update();
		}
	}
	
	void OnTriggerEnter(Collider c) {
		if(!m_active || !m_damage_on_enter) {return;}
		
		GEntity target = c.gameObject.GetComponent<GEntity>();
		
		if(target!=null && (target.m_team_id!=m_owner.m_team_id || friendly_fire)) {
			target.Damage(m_damage);
		}
		
		//Destroy on collision with anything but the owner, if destroy_on_enter.
		if(m_destroy_on_enter && (target==null || target!=m_owner)) {
			
			if(m_hit_particle) {
				Instantiate(m_hit_particle,transform.position,Quaternion.identity);
			}
			
			Destroy(gameObject);
		}
	}
	
	void OnTriggerStay(Collider c) {
		if(!m_active 
			|| !m_damage_on_stay 
			|| !m_stay_damage_begin_timer.IsComplete() 
			|| !m_stay_damage_repeat_timer.IsComplete()
		) { return; }
		
		GEntity target = c.GetComponent<GEntity>();
		if(target!=null && (target.m_team_id!=m_owner.m_team_id || friendly_fire)) {
			target.Damage(m_damage);
		}
	}
	
	void OnTriggerExit(Collider c) {
		if(!m_active || !m_damage_on_exit) {return;}
		
		GEntity target = c.GetComponent<GEntity>();
		if(target!=null && (target.m_team_id!=m_owner.m_team_id || friendly_fire)) {
			target.Damage(m_damage);
		}
	}
	
	public void SetDamage(int amount) {
		m_damage = amount;
	}
	
	public void SetActive(bool state) {
		m_active = state;
	}
	
	public void EndLifetime() {
		Destroy(gameObject);
	}
	
	public void SetOwner(GEntity owner) {
		m_owner = owner;
	}
}
