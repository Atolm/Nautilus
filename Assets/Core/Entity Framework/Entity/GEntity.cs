using UnityEngine;
using System.Collections;

public class GEntity : MonoBehaviour {
	//GENTITY BASE INITIALIZERS
	public GController m_controller;
	public GMobile m_mobile;
	public GCombatant m_combatant;
	public GActor m_actor;
	protected GameObject m_appearance_root;
	
	//BEHAVIOUR
	public ACTION[] m_actions;
	public AITYPE m_ai_type = AITYPE.DEFAULT;

	//CONTROL TYPES
	public GPlayer m_player_control;
	public GEntityAI m_ai_control;
	
	[SerializeField] protected GameObject m_basic_attack_prefab;
	[SerializeField] protected GameObject m_death_effect_prefab;
	[SerializeField] GameObject m_hit_effect = null;
	
	//OBJECT DROPS
	[SerializeField] ItemDrop[] m_drop_table;
	
	//STATS
	[SerializeField] int m_max_health = 30;
	public float m_attack_range = 5f;
	public int m_attack_damage = 10;
	public float m_move_speed = 16;
	public float m_accuracy_mod = 0;
	
	//RESOURCES
	public GameResource m_health;
	public GameResource m_energy;
	
	public int m_team_id = 0;
	public int m_team_member_id = 0;
	public bool m_is_team_leader;
	public bool m_is_base_player;
	
	protected virtual void Start () {
		m_controller = Game.controller;
		if(m_is_team_leader) {
			m_controller.GetTeam(m_team_id).SetLeader(this);
		}
		
		m_mobile = gameObject.AddComponent<GMobile>();
		m_combatant = gameObject.AddComponent<GCombatant>();
		m_actor = gameObject.AddComponent<GActor>();
		
		m_health = new GameResource(m_max_health,0,m_max_health);
		m_energy = new GameResource(100,0,100);
		
		m_player_control = gameObject.AddComponent<GPlayer>();
		m_ai_control = gameObject.AddComponent<GEntityAI>();
		
		m_appearance_root = transform.Find("Appearance").gameObject;
		m_mobile.SetFlip(m_appearance_root,true,false);
		
		m_combatant.FillDummyActions();
		
		m_combatant.SetAction(
			"Attack","A deadly attack.",
			new GActionTimer(ACTION.ATTACK,0.5f,0.25f,0.25f,1f,m_combatant.GenericActionStart,null),
			m_basic_attack_prefab,
			null, 
			new ACTION_EFFECT[1] {ACTION_EFFECT.TELEPORT},
			Resources.Load<Sprite>("Icons/ui_2x_laser_icon"),
			m_attack_range, m_attack_damage, true, true, true, true
		);
		m_combatant.SetAction(
			"Block","An impenetrable block.",
			new GActionTimer(ACTION.BLOCK,0.1f,0.1f,0f,1f,m_combatant.BlockActionStart,m_combatant.BlockActionEnd),
			m_basic_attack_prefab, 
			null, 
			new ACTION_EFFECT[0],
			Resources.Load<Sprite>("Icons/ui_2x_laser_icon"), 
			10, 10, true, true, true, true
		);
		m_combatant.SetAction(
			"Single Shot","Pew.",
			new GActionTimer(ACTION.SINGLE_SHOT,0.1f,0.1f,0.2f,1f,m_combatant.GenericActionStart,null),
			Resources.Load<GameObject>("Weapons/OLD/PlayerLaserSmall"), 
			null, 
			new ACTION_EFFECT[0],
			Resources.Load<Sprite>("Icons/ui_single_laser_icon"), 
			10, 10, true, true, true, true
		);
		m_combatant.SetAction(
			"Double Shot","Pew pew.",
			new GActionTimer(ACTION.DOUBLE_SHOT,0f,0f,0.25f,1f,m_combatant.GenericActionStart,null),
			Resources.Load<GameObject>("Weapons/PlayerDoubleLaser"), 
			null, 
			new ACTION_EFFECT[0],
			Resources.Load<Sprite>("Icons/ui_2x_laser_icon"), 
			10, 10, true, true, true, true
		);
	}
	
	void OnLevelWasLoaded() {
		if(m_controller==null) {m_controller=Game.controller;}
	}
	
	protected virtual void Update () {
		UpdateHealth();
		UpdateEnergy();
	}
	
	public void Damage(int amount) {
		if(m_health==null) { return; }
		
		if(gameObject.tag=="Player") {
			//Player damage reduction.
			amount = Mathf.FloorToInt(amount * 0.5f);
		}
		
		m_health.ChangeValue(-amount);
		
		//Visual Component
		m_actor.PlayBlocking("Hit");
		if(amount > 0 && m_hit_effect!=null) {
			Instantiate(m_hit_effect,transform.position,transform.rotation);
		}
	}
	
	public void UpdateHealth() {
		if(m_health.GetValue()>0) { return; }
			
		if(gameObject.tag=="Player"){
			if(!m_is_base_player) {
				m_controller.ChangePlayer(Game.reserve_player);
				Game.player.transform.position = transform.position;
				Game.player.gameObject.SetActive(true);
				Game.player.m_controller=m_controller;
				Destroy(gameObject);
			}
			return;
		}
		
		//Enemies do this.
		Instantiate(m_death_effect_prefab,transform.position,Quaternion.identity);
		
		Vector3 drop_pos;
		foreach(ItemDrop d in m_drop_table) {
			if(Random.value*100<d.chance) {
				drop_pos = transform.position + new Vector3(Random.Range(-1,2),0,Random.Range(-1,2));
				Instantiate(d.item,drop_pos,Quaternion.identity);
			}
		}
		foreach(GEntity e in GameObject.FindObjectsOfType<GEntity>()) {
			e.NotifyOfDeath(this);
		}
		Destroy(gameObject);
	}

	public void NotifyOfDeath(GEntity entity) {
		m_ai_control.OnSeeDeath(entity);
	}
	
	public void UpdateEnergy() {
		if(gameObject.tag=="Player"){
			return;
		}
		
		if(m_energy.GetValue()==0) {
			Destroy(gameObject);
		}
	}

	public GTeam GetTeam() {
		if(m_team_id >= 0 && m_team_id < m_controller.m_teams.Length) {
			return m_controller.m_teams[m_team_id];
		}
		return null;
	}
}

public enum ACTION {
	NONE,
	ATTACK,
	BLOCK,
	SINGLE_SHOT,
	DOUBLE_SHOT,
	count
};

[System.Serializable]
public class ItemDrop {
	public GameObject item;
	public float chance;
}