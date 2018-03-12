using UnityEngine;
using System.Collections;

public class GEntityAI : MonoBehaviour {
	public GEntity m_entity;
	
	float m_sight_range = 16;
	MinMax m_combat_range = new MinMax(4,6);
	bool m_at_ideal_range = false;
	GEntity m_combat_target;
	
	GameTimer m_wander_move_timer = new GameTimer(2,false,null);
	GameTimer m_wander_wait_timer = new GameTimer(1,false,null);
	bool m_has_wander_target = false;
	Vector3 m_wander_target = Vector3.zero;
	float m_wander_speed_mod = 0.5f;
	
	
	MinMax m_avoid_range = new MinMax(2,4);
	bool m_is_avoiding = false;
	GameTimer m_avoid_timer = new GameTimer(0.5f,false,null);
	float m_avoid_speed_mod = 0.5f;
	
	bool m_patrol_set = false;
	Vector3 m_patrol_target = Vector3.zero;
	float m_patrol_speed_mod = 0.5f;

	bool m_follow_point_set = false;
	bool m_follow_point_reached = false;
	Vector3 m_follow_point = Vector3.zero;
	float m_follow_distance = 3;
	float m_max_pursue_distance = 12;
	GameTimer m_pursue_return_timer;
	
	GameTimer m_aim_lag_timer;
	Vector3 m_target_position = new Vector3(0,10000,0);
	
	GameTimer m_transfer_stun_timer = new GameTimer(0.2f,false,null);



    void Start() {
		m_entity = GetComponent<GEntity>();
		m_aim_lag_timer = new GameTimer(1f,true,RecordTargetPosition);
        /*
		m_combat_range = new MinMax(
			m_entity.m_combatant.GetShortestActionRange()*0.65f,
			m_entity.m_combatant.GetShortestActionRange()
		);
		 */
    }
	
	void Update () {
		if(gameObject.tag!="Enemy") { return; }
		m_transfer_stun_timer.Update();
		
		switch(m_entity.m_ai_type) {
			case AITYPE.FIGHT:
				AIStateFight();
				break;
			case AITYPE.FLEE:
				AIStateFlee();
				break;
			case AITYPE.DEFAULT:
				AIStateDefault();
				break;
		}
	}
	
	void AIStateFight() {
		GEntity nearby_enemy = GetClosestEnemyInSight();
		if(nearby_enemy!=null) {
			AIPursue(nearby_enemy);
		}
		else {
			GTeam team = m_entity.m_controller.GetTeam(m_entity.m_team_id);
			if(team!=null && team.GetLeader()!=null) {
				AIFollow(team.GetLeader());
			}
		}
		AIUseActions();
	}

	void AIStateFlee() {
		GTeam team = m_entity.m_controller.GetTeam(m_entity.m_team_id);
		if(team!=null && team.GetLeader()!=null) {
			AIFollow(team.GetLeader());
		}
	}

	void AIStateDefault() {
		if(!AIAvoid()) {
			if(m_has_wander_target) {
				AIWander();
			}
			else if(!m_patrol_set && Random.value>0.2f) {
				AIWander();
			}
			else {
				AIPatrol();
			}
		}
	}
	
	void AIWander() {
		//Clear target if it has been reached.
		if(m_has_wander_target) {
			if(Vector3.Distance(transform.position,m_wander_target)<1) {
				m_has_wander_target=false;
				return;
			}
		}
		
		//Move towards target.
		if(m_has_wander_target) {
			Vector3 dir = m_wander_target-transform.position;
			Vector2 dir2d = new Vector3(dir.x,dir.z);
			m_entity.m_mobile.Move(m_entity.m_move_speed*m_wander_speed_mod,dir2d);
			return;
		}
		
		//Get a new target.
		MapSection current_section = Game.map.GetSectionAtPosition(transform.position);
		if(current_section.m_room==null) {return;}
		
		Vector2 rnd_tile = new Vector2(
			current_section.GetMidpoint().x+Random.Range(-current_section.m_room.m_size+1, current_section.m_room.m_size),
			current_section.GetMidpoint().y+Random.Range(-current_section.m_room.m_size+1, current_section.m_room.m_size)
		);
		Vector3 rnd_position = new Vector3(rnd_tile.x*Game.map.GetMapSquareSize(),0,rnd_tile.y*Game.map.GetMapSquareSize());
		m_wander_target = rnd_position;
		m_has_wander_target = true;
	}
	
	bool AIAvoid() {
		Transform avoid_target = null;
		
		Transform pt = GameObject.FindGameObjectWithTag("Player").transform;
		
		if(Vector3.Distance(transform.position, pt.position) < m_avoid_range.max) {
			avoid_target = pt;
		}
		
		foreach(GameObject go in GameObject.FindGameObjectsWithTag("Enemy")) {
			if(go==gameObject) {
				continue;
			}
			if(Vector3.Distance(transform.position,go.transform.position) < m_avoid_range.max) {
				if(avoid_target==null 
					|| Vector3.Distance(transform.position,go.transform.position)<Vector3.Distance(transform.position,avoid_target.position)) {
					avoid_target = go.transform;
				}
			}
		}
		
		//Avoid the target.
		if(avoid_target!=null) {
			if(m_is_avoiding && Vector3.Distance(transform.position,avoid_target.position)<m_avoid_range.max) {
				Vector3 avoid_move = Quaternion.Euler(0,60,0)*(transform.position-avoid_target.position);
				Vector2 avoid_move_2d = new Vector2(avoid_move.x,avoid_move.z);
				m_entity.m_mobile.Move(m_entity.m_move_speed*m_avoid_speed_mod,avoid_move_2d);
				m_is_avoiding = true;
				return true;
			}
			
			if(!m_is_avoiding && Vector3.Distance(transform.position,avoid_target.position)<m_avoid_range.min) {
				Vector3 avoid_move = Quaternion.Euler(0,60,0)*(transform.position-avoid_target.position);
				Vector2 avoid_move_2d = new Vector2(avoid_move.x,avoid_move.z);
				m_entity.m_mobile.Move(m_entity.m_move_speed*m_avoid_speed_mod,avoid_move_2d);
				m_is_avoiding = true;
				return true;
			}
		}
		
		m_is_avoiding = false;
		return false;
	}
	
	void AIPatrol() {
		if(m_patrol_set==false) {
			m_patrol_target = Game.map.GetAdjacentRoomPosition(transform.position);
			m_patrol_set = true;
			return;
		}
		
		if(Vector3.Distance(transform.position,m_patrol_target)<1) {
			m_patrol_set = false;
			return;
		}
		
		Vector3 dir = m_patrol_target-transform.position;
		
		if(Physics.Raycast(transform.position,dir,1)) {
			if(Mathf.Abs(dir.x)>Mathf.Abs(dir.z)) {
				dir.x=0;
			}
			else {
				dir.z=0;
			}
		}
		
		Vector2 dir2d = new Vector3(dir.x,dir.z);
		m_entity.m_mobile.Move(m_entity.m_move_speed*m_patrol_speed_mod,dir2d);
	}
	
	void AIPursue(GEntity target) {
		//AI to chase the player once sighted.
		float target_distance = Vector3.Distance(transform.position,target.transform.position);
		
		if(!m_entity.m_combatant.IsUsingActionThatBlocksMovement() 
			&&  target_distance > m_combat_range.min
			&& !m_at_ideal_range)
		{
			Vector3 dir = target.transform.position-transform.position;
			Vector2 target_direction = new Vector2(dir.x,dir.z);
			m_entity.m_mobile.Move(m_entity.m_move_speed,target_direction);
			m_at_ideal_range = target_distance <= m_combat_range.min;
		}
		else if (!m_entity.m_combatant.IsUsingActionThatBlocksMovement() 
				&&  target_distance > m_combat_range.max
				&& m_at_ideal_range){
			m_at_ideal_range = false;
		}
	}

	void AIFollow(GEntity target) {
		GTeam team = m_entity.m_controller.GetTeam(m_entity.m_team_id);
		if(team==null || team.GetLeader()==null || team.m_waypoint_set==false) {
			return;
		}

		Vector3 waypoint = team.GetLeader().transform.position + team.GetOrbitPosition(m_entity.m_team_member_id);
		float waypoint_distance = Vector3.Distance(transform.position,waypoint);
		float target_distance = Vector3.Distance(transform.position,team.GetLeader().transform.position);
		
		if(!m_entity.m_combatant.IsUsingActionThatBlocksMovement() 
			&& (target_distance > m_combat_range.max*2 && waypoint_distance > .8f)
			&& !m_at_ideal_range)
		{
			Vector3 dir = waypoint-transform.position;
			Vector2 target_direction = new Vector2(dir.x,dir.z);
			m_entity.m_mobile.Move(m_entity.m_move_speed*2,target_direction);
		}
		else if(!m_entity.m_combatant.IsUsingActionThatBlocksMovement() 
			&& (target_distance > m_combat_range.min || waypoint_distance > .8f)
			&& !m_at_ideal_range)
		{
			Vector3 dir = waypoint-transform.position;
			Vector2 target_direction = new Vector2(dir.x,dir.z);
			m_entity.m_mobile.Move(m_entity.m_move_speed,target_direction);
		}
		m_follow_point_reached = waypoint_distance <= .8f;

	}
	
	void AIUseActions() {
		if(!TurnTime.timePassed) {return;}

		m_aim_lag_timer.Update();
		m_entity.m_combatant.SetMainHostileTarget(GetClosestEnemyInSight());
		m_entity.m_combatant.SetMainFriendlyTarget(GetClostestAllyInSight());
		
		float attack_angle = Util.GetAngle3D(transform.position,m_target_position)+Random.Range(-m_entity.m_accuracy_mod,m_entity.m_accuracy_mod);
		Quaternion attack_quaternion = Quaternion.Euler(0,attack_angle,0);
		Vector3 attack_position = new Vector3(0,0.5f,1);
		attack_position = attack_quaternion * attack_position;
		attack_position = Vector3.zero;
		
		m_entity.m_combatant.SetAttackRotation(attack_quaternion);
		m_entity.m_combatant.SetAttackPosition(attack_position);

		m_entity.m_combatant.AttemptAllActions(TRIGGER.READY);
	}

	public void OnSeeDeath(GEntity entity) {
		if(Vector3.Distance(transform.position,entity.transform.position) > m_sight_range) {
			return;
		}
		m_entity.m_combatant.AttemptAllActions(TRIGGER.DEATH);
	}

	GEntity GetClosestEnemyInSight() {
		GEntity target = null;
		float target_distance = m_sight_range;
		GEntity[] entities = GameObject.FindObjectsOfType<GEntity>();
		foreach(GEntity ge in entities) {
			if(m_entity.m_team_id!=ge.m_team_id 
				&& Vector3.Distance(transform.position,ge.transform.position) <= target_distance){
					target_distance = Vector3.Distance(transform.position,ge.transform.position);
					target = ge;
			}
		}
		return target;
	}

	GEntity GetClostestAllyInSight() {
		GEntity target = null;
		float target_distance = m_sight_range;
	    GEntity[] entities = GameObject.FindObjectsOfType<GEntity>();
		foreach(GEntity ge in entities) {
			if(m_entity.m_team_id==ge.m_team_id 
				&& Vector3.Distance(transform.position,ge.transform.position) <= target_distance){
					target_distance = Vector3.Distance(transform.position,ge.transform.position);
					target = ge;
			}
		}
		return target;
	}
	
	void RecordTargetPosition() {
		m_target_position = Game.player.transform.position;
	}
	
	public void SetTransferStun() {
		m_transfer_stun_timer.ResetTime();
	}

	public void SetFollowPoint(Vector3 follow_point) {
		m_follow_point_set = true;
		m_follow_point = follow_point;
	}
}

public enum AITYPE {
	DEFAULT,
	FIGHT,
	FLEE,
	PATROL,
	INANIMATE,
	count
}