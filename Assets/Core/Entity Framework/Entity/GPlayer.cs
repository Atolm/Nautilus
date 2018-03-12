using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class GPlayer : MonoBehaviour{
	public GEntity m_entity;
	bool m_progess_time = false;
	[SerializeField] MinMax m_yclamp = new MinMax(0,100);
	
	public bool m_at_entrance = false;
	public bool m_at_exit = false;

	public bool m_has_waypoint = false;
	public Vector3 m_player_waypoint;
	GameTimer m_waypoint_set_timer = new GameTimer(0.2f,false,null);
	
	GameObject m_canvas;
	
	 void Start() {
		m_entity = GetComponent<GEntity>();
		m_canvas = GameObject.Find("Canvas");
		m_entity.GetTeam().RefreshMemberList();
		m_entity.GetTeam().SetTeamAI(AITYPE.FIGHT);
	}
	
	void OnLevelWasLoaded() {
		m_at_entrance = false;
		m_at_exit = false;
	}
	
	void Update () {
		if(gameObject.tag!="Player") {return;}
		
		UpdateCanvas();
		UpdateEnergyDrain();
		UpdateMovementMouse();
		//UpdateMovementWASD();
		UpdateTimeProgression();

		if(Input.GetButtonDown("Fire1")) {
			//m_entity.GetTeam().ToggleTeamAI();
		}
		
		if(Input.GetKeyDown("left shift")) {
			if(m_at_entrance) {
				SceneManager.LoadScene("test_scene");
				Game.floor_id--;
			}
			else if(m_at_exit) {
				SceneManager.LoadScene("test_scene");
				Game.floor_id++;
			}
		}
		
		if(m_entity.m_combatant.IsUsingAction()){m_progess_time=true;}
	}
	
	void UpdateCanvas() {
		if(m_canvas==null) {
			m_canvas = GameObject.Find("Canvas");
		}
		UpdateUI();
	}

	void UpdateMovementMouse() {
		bool is_moving = false;
		Vector2 move_direction = Vector2.zero;

		RaycastHit hit;
		if(Input.GetMouseButton(1)) {
			is_moving = Physics.Raycast(
				Camera.main.ScreenPointToRay(Input.mousePosition),
				out hit,
				1000,
				LayerMask.GetMask("Floor")
			);
			move_direction = new Vector2(hit.point.x,hit.point.z);
			m_entity.m_controller.GetTeam(m_entity.m_team_id).SetWaypoint(hit.point);
			m_waypoint_set_timer.Update();
			m_has_waypoint = false;
		}
		else if (Input.GetMouseButtonUp(1) && !m_waypoint_set_timer.IsComplete()) {
			m_has_waypoint = Physics.Raycast(
				Camera.main.ScreenPointToRay(Input.mousePosition),
				out hit,
				1000,
				LayerMask.GetMask("Floor")
			);
			if (m_has_waypoint) {
				m_player_waypoint = hit.point;
			}
			move_direction = new Vector2(m_player_waypoint.x,m_player_waypoint.z);
			m_waypoint_set_timer.ResetTime();
			is_moving = m_has_waypoint;
		}
		else if(m_has_waypoint) {
			move_direction = new Vector2(m_player_waypoint.x,m_player_waypoint.z);
			m_entity.m_controller.GetTeam(m_entity.m_team_id).SetWaypoint(move_direction);
			if(Vector3.Distance(transform.position,m_player_waypoint) < 0.1f) {
				m_has_waypoint = false;
			}
			is_moving = m_has_waypoint;
		}
		else {
			m_waypoint_set_timer.ResetTime();
		}

		
		if(!m_entity.m_combatant.IsUsingActionThatBlocksMovement() && !m_entity.m_actor.IsPlayingBlockingAnim()) {
			if(is_moving) {
				m_entity.m_mobile.MoveTowards(m_entity.m_move_speed,move_direction);
				m_progess_time = true;
			}
		}
	}

	void UpdateMovementWASD() {
		Vector3 move_direction_3d = Camera.main.transform.rotation * new Vector3(Input.GetAxis("Horizontal"),0,Input.GetAxis("Vertical"));
		Vector2 move_direction = new Vector2(move_direction_3d.x,move_direction_3d.z);
		if(!m_entity.m_combatant.IsUsingActionThatBlocksMovement() && !m_entity.m_actor.IsPlayingBlockingAnim()) {
			if(move_direction.magnitude > 0) {
				//m_entity.m_actor.PlayNormal("Walk");
				m_progess_time = true;
			}
			else {
				//m_entity.m_actor.PlayNormal("Stand");
			}
			m_entity.m_mobile.Move(m_entity.m_move_speed,move_direction);
		}
	}
	
	void UpdateEnergyDrain() {
		m_entity.m_energy.ChangeValue(-1*TurnTime.deltaTime);
	}
	
	void UpdateTimeProgression() {
		if(m_progess_time) {
			m_entity.m_controller.SetTimeModifier(1);
		}
		else {
			m_entity.m_controller.SetTimeModifier(0);
		}
		m_progess_time = false;
	}
	
	void SetAttackDirection() {
		//Includes mod to move it towards centre of pointer
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition-(Vector3.up*20));
		Vector3 hit_pos = Vector3.zero;
		
		RaycastHit[] hits = Physics.RaycastAll(ray);
		foreach (RaycastHit h in hits) {
			if(h.collider.gameObject.tag=="Floor") {
				hit_pos = h.point;
			}
		}
		
		float attack_angle = Util.GetAngle3D(transform.position,hit_pos);
		Quaternion attack_quaternion = Quaternion.Euler(0,attack_angle,0);
		
		Vector3 attack_position = new Vector3(0,0.5f,1);
		attack_position = attack_quaternion * attack_position;
		
		m_entity.m_combatant.SetAttackRotation(attack_quaternion);
		m_entity.m_combatant.SetAttackPosition(attack_position);
	}
	
	void UpdateUI() {
		//m_canvas.transform.Find("Actions/Action1/ActIcon1").gameObject.GetComponent<Image>().overrideSprite = m_entity.m_combatant.GetAction(m_entity.m_action_1).m_icon;
		//m_canvas.transform.Find("Actions/Action2/ActIcon2").gameObject.GetComponent<Image>().overrideSprite = m_entity.m_combatant.GetAction(m_entity.m_action_2).m_icon;
	}
	
}
