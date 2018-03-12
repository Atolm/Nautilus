using UnityEngine;
using System.Collections;

//Component of the generic 'Entity' type used for player/monsters
//Use this to store and perform combat related actions (attacking, blocking etc) from the Entity.
public class GCombatant : MonoBehaviour {
	GEntity m_entity;
	
	[SerializeField] GAction[] m_actions;
	
	GameObject m_basic_attack_prefab;
	int m_basic_attack_damage;

	GEntity m_main_hostile_target = null;
	GEntity m_main_friendly_target = null;
	Vector3 m_main_teleport_target = Vector3.zero;
	
	Vector3 m_attack_position = Vector3.zero;
	Quaternion m_attack_rotation;

	void Start () {
		m_entity = GetComponent<GEntity>();
	}
	
	public void FillDummyActions() {
		m_actions = new GAction[(int)ACTION.count];
		for(int i = 0;i < (int)ACTION.count; i++){
			m_actions[i] = new GAction("","",null,null,null,0,0,new ACTION_EFFECT[0],false,false,false,false,null);
		}
	}
	
	public void SetActions(GAction[] actions) {
		m_actions = actions;
	}
	
	public void SetAction(
		string name,
		string desc,
		GActionTimer action_timer,
		GameObject start_prefab,
		GameObject end_prefab,
		ACTION_EFFECT[] effects,
		Sprite icon,
		float range,
		int damage,
		bool is_hostile,
		bool requires_target,
		bool centers_on_target,
		bool blocks_movement
		){
		m_actions[(int)action_timer.m_action_id] = new GAction(
			name,
			desc,
			action_timer,
			start_prefab,
			end_prefab,
			range,
			damage,
			effects,
			is_hostile,
			requires_target,
			centers_on_target,
			blocks_movement,
			icon
		);
	}
	
	void Update () {
		UpdateActions();
	}
	
	void UpdateActions() {
		for(int i=0; i < m_actions.Length; i++) {
			m_actions[i].Update();
		}
	}

	void UpdatePassives() {

	}
	
	public GAction GetAction(ACTION action) {
		if((int)action < m_actions.Length) {
			return m_actions[(int)action];
		}
		Util.Print("ERROR: no such action: " + (int)action);
		return null;
	}

	public float GetShortestActionRange() {
		float shortest = Mathf.Infinity;
		foreach(GAction action in m_actions) {
			if(action.GetActionID()!=ACTION.NONE && action.m_range < shortest) {
				shortest = action.m_range;
			}
		}
		return shortest;
	}

	///Attempt all actions triggered by this event.
	public void AttemptAllActions(TRIGGER trigger) {
		foreach(ACTION available_action in m_entity.m_actions) {
			foreach(GAction action in m_actions) {
				if(action.m_action_timer!=null && action.m_action_timer.m_action_id==available_action) {
					AttemptAction(available_action,trigger);
				}
			}
		}
	}
	
	///This will begin an action - if possible.
	public void AttemptAction(ACTION action, TRIGGER trigger) {
		GAction g_action = GetAction(action);
		if(g_action==null) {return;}

		float target_distance = Mathf.Infinity;

		if(g_action.m_is_hostile) {
			if(g_action.m_requires_target && !HasHostileTarget()) {return;}
			if(m_main_hostile_target!=null) {
				target_distance = Vector3.Distance(
					transform.position,
					m_main_hostile_target.transform.position
				);
			}
		}
		else {
			if(g_action.m_requires_target && !HasFriendlyTarget()) {return;}
			if(m_main_hostile_target!=null) {
				target_distance = Vector3.Distance(
					transform.position,
					m_main_friendly_target.transform.position);
			}
		}

		if(g_action.IsReadyConditionalAtRange(trigger,target_distance)){
			g_action.m_action_timer.AttemptAction();
			//m_entity.m_actor.PlayNormal(g_action.m_name + "Windup");
		}
	}
	
	///This will begin/refresh a sustained action - if possible.
	public void AttemptSustainedAction(ACTION action){
		GAction g_action = GetAction(action);
		if(g_action !=null && g_action.IsReady()) {
			AttemptAction(ACTION.BLOCK,TRIGGER.READY);
		}
		else {
			g_action.m_action_timer.Sustain();
		}
	}
	
	public bool IsUsingAction() {
		bool acting = false;
		for(int i=0;i<m_actions.Length;i++){
			if(m_actions[i].InUse()){
				acting = true;
			}
		}
		return acting;
	}
	
	public bool IsUsingActionThatBlocksMovement() {
		bool acting = false;
		for(int i=0;i<m_actions.Length;i++){
			if(m_actions[i].InUse() && m_actions[i].m_block_movement){
				acting = true;
			}
		}
		return acting;
	}
	
	public void GenericActionStart(ACTION action_id) {
		GAction ac = m_actions[(int)action_id];
		GameObject action_object = null;
		if(ac.m_attack_start_prefab) {
			action_object = CreateActionObject(action_id, ac.m_attack_start_prefab,ac.m_damage);
		}
		else {
			Util.Print("Missing damager/appearance for attack!!");
		}
		if(action_object!=null) {
			m_entity.m_actor.PlayBlocking("Attack");
		}
	}
	
	public void GenericActionEnd(ACTION action_id) {
		GAction ac = m_actions[(int)action_id];
		if(ac.m_attack_end_prefab) {
			CreateActionObject(action_id, ac.m_attack_end_prefab,ac.m_damage);
		}
		else {
			Util.Print("Missing damager/appearance for attack!!");
		}
	}
	
	public void BlockActionStart(ACTION action_id) {
		//m_entity.m_actor.PlayNormal("BlockWindup");
	}
	
	public void BlockActionEnd(ACTION action_id) {
		//m_entity.m_actor.PlayBlocking("BlockEnd");
	}
	
	///Creates a game object as part of an action
	public GameObject CreateActionObject(ACTION action_id, GameObject action_prefab, int damage) {
		GAction action = GetAction(action_id);

		//why are we checking this here??? You really shouldn't be here if this wasn't true.
		//if(!action.IsReady()) { return; }

		//Create GameObject associated with this action.
		GameObject action_object = null;
		if(action.m_center_on_target && action.m_is_hostile && m_main_hostile_target!=null) {
			action_object = (GameObject)Instantiate(action_prefab,m_main_hostile_target.transform.position+m_attack_position,m_attack_rotation);
			action_object.transform.localScale = m_entity.m_actor.m_animator.transform.localScale;
		}
		else if(action.m_center_on_target && !action.m_is_hostile && m_main_friendly_target!=null) {
			action_object = (GameObject)Instantiate(action_prefab,m_main_friendly_target.transform.position+m_attack_position,m_attack_rotation);
			action_object.transform.localScale = m_entity.m_actor.m_animator.transform.localScale;
		}
		else {
			action_object = (GameObject)Instantiate(action_prefab,transform.position+m_attack_position,m_attack_rotation);
			action_object.transform.localScale = m_entity.m_actor.m_animator.transform.localScale;
		}
		
		//Set visual effect movement (might not be needed for this game).
		if(action_object!=null) {
			Vector2 x_facing = new Vector2(m_entity.m_mobile.GetFacing().x,0);
			GameVisual action_effect = action_object.GetComponent<GameVisual>();
			if(action_effect!=null) {
				action_effect.move_direction = new Vector2(
					action_effect.move_direction.x * x_facing.x,
					action_effect.move_direction.y
				);
			}
		}

		//Maybe this should be a feature of the skill?
		bool enable_autoaim = false;
		
		//Set the damage on the shot (or shots) fired by this attack.
		GDamager root_dmg = action_object.GetComponent<GDamager>();
		if(root_dmg!=null) {
			root_dmg.SetDamage(damage);
			root_dmg.SetOwner(m_entity);
			
			if(root_dmg.GetComponent<GBullet>() && enable_autoaim) {
				root_dmg.GetComponent<GBullet>().SetAutoaimEnabled(enable_autoaim);
			}
		}
		foreach(GDamager dmg in action_object.transform.GetComponentsInChildren<GDamager>()) {
			dmg.SetDamage(damage);
			dmg.SetOwner(m_entity);
			
			if(dmg.GetComponent<GBullet>() && enable_autoaim) {
				dmg.GetComponent<GBullet>().SetAutoaimEnabled(enable_autoaim);
			}
		}

		//If this attack summoned a creature, that creature joins the summoner's team.
		GEntity summoned_entity = action_object.GetComponent<GEntity>();
		if(summoned_entity!=null) {
			summoned_entity.m_team_id = m_entity.m_team_id;
		}
		
		//m_entity.m_actor.PlayNormal("Attack");
		return action_object;
	}

	public bool HasHostileTarget() {
		return m_main_hostile_target!=null;
	}

	public bool HasFriendlyTarget() {
		return m_main_friendly_target!=null;
	}
	
	public void SetAttackRotation(Quaternion new_rotation) {
		m_attack_rotation = new_rotation;
	}
	
	public void SetAttackPosition(Vector3 new_position) {
		m_attack_position = new_position;
	}

	public void SetMainHostileTarget(GEntity entity) {
		m_main_hostile_target = entity;
	}

	public void SetMainFriendlyTarget(GEntity entity) {
		m_main_friendly_target = entity;
	}

}

[System.Serializable]
public class GAction {
	public string m_name;
	public string m_desc;
	public GActionTimer m_action_timer;
	public TRIGGER m_action_trigger = TRIGGER.READY;
	public GameObject m_attack_start_prefab;
	public GameObject m_attack_end_prefab;
	public float m_range;
	public int m_damage;
	public ACTION_EFFECT[] m_action_effects;
	public bool m_is_hostile;
	public bool m_requires_target;
	public bool m_center_on_target;
	public bool m_block_movement;
	public Sprite m_icon;
	public GAction(
		string name,
		string desc,
		GActionTimer action,
		GameObject attack_start_prefab,
		GameObject attack_end_prefab,
		float range,
		int damage,
		ACTION_EFFECT[] effects,
		bool is_hostile,
		bool requires_target,
		bool center_on_target,
		bool blocks_movement,
		Sprite icon
	) {
		m_action_timer = action;
		m_attack_start_prefab = attack_start_prefab;
		m_attack_end_prefab = attack_end_prefab;
		m_range = range;
		m_damage = damage;
		m_action_effects = effects;
		m_is_hostile = is_hostile;
		m_requires_target = requires_target;
		m_center_on_target = center_on_target;
		m_block_movement = blocks_movement;
		m_icon = icon;
	}
	public void Update() {
		if(m_action_timer!=null){
			if(TurnTime.timePassed || !m_action_timer.IsActionComplete()) {
				m_action_timer.Update();
			}
		}
	}
	public ACTION GetActionID() {
		return m_action_timer.m_action_id;
	}
	
	public bool InUse() {
		if(m_action_timer!=null) {
			return m_action_timer.InUse();
		}
		return false;
	}

	///Returns if the action is ready (for actions WITHOUT a target requirement)
	public bool IsReady() {
		return IsReadyConditionalAtRange(TRIGGER.READY, Mathf.Infinity);
	}

	///Returns if the action is ready at the specified range.
	public bool IsReadyAtRange(float target_distance) {
		return IsReadyConditionalAtRange(TRIGGER.READY, target_distance);
	}

	///Returns if a conditional action is ready at the specified range.
	public bool IsReadyConditionalAtRange(TRIGGER condition, float target_distance) {
		if(m_action_trigger!=condition) { return false; }
		if(m_requires_target && target_distance >= m_range) {return false;}
		return m_action_timer.IsReady();
	}
}

public enum TRIGGER {
	READY,
	DEATH,
	count //count this enum's size
}

public enum ACTION_EFFECT {
	NONE,
	TELEPORT
}