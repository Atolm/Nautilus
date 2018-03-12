using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GController : MonoBehaviour {
	public GameObject player_prefab;
	public GPlayer player;
	public List<GPlayer> m_player_units;
	public List<GEntityAI> m_enemy_units;
	public GTeam[] m_teams = new GTeam[]{
		new GTeam(0),
		new GTeam(1),
	};
	
	float m_time_mod;
	
	void Awake() {
		Game.controller = this;
        Application.targetFrameRate = 60;

    }

	void Start () {
		//Game.controller = this;
		player_prefab = (GameObject)Resources.Load("Player");
		
		/*
		foreach(GameObject go in GameObject.FindGameObjectsWithTag("Player")) {
			m_player_units.Add(go.GetComponent<GPlayer>());
		}
		foreach(GameObject go in GameObject.FindGameObjectsWithTag("Enemy")) {
			m_enemy_units.Add(go.GetComponent<GEnemy>());
		}
		*/
	}
	
	void Update () {
		if(Game.player==null) {
			FindPlayer();
		}
	}
	
	void LateUpdate() {
		TurnTime.deltaTime = Time.deltaTime;
		TurnTime.timePassed = true;
	}
	
	public void SetTimeModifier(float modifier) {
		m_time_mod = modifier;
	}
	
	public float DeltaTime() {
		return Time.deltaTime * m_time_mod;
	}
	
	public void SetReservePlayer(GEntity reserve_player) {
			reserve_player.tag="ReservePlayer";
			Game.reserve_player=reserve_player;
			reserve_player.gameObject.SetActive(false);
	}
	
	public void ChangePlayer(GEntity new_player) {
		Game.player = new_player;
		Game.player.tag="Player";
		GameObject.DontDestroyOnLoad(Game.player.gameObject);
	}
	
	public void FindPlayer() {
		ChangePlayer(GameObject.FindGameObjectWithTag("Player").GetComponent<GEntity>());
	}

	public GTeam GetTeam(int id) {
		if(id >= m_teams.Length) { return null; }
		return m_teams[id];
	}
}


public class GTeam {
	public int m_id;
	public GEntity m_leader;
	public GEntity[] m_members;
	public Vector3 m_waypoint;
	public bool m_waypoint_set = false;
	public AITYPE m_team_ai = AITYPE.FIGHT;
	public GTeam(int id) {
		m_id = id;
	}
	public void RefreshMemberList() {
		List<GEntity> list = new List<GEntity>();
		int i = 0;
		foreach(GEntity entity in GameObject.FindObjectsOfType<GEntity>()) {
			if(entity.m_team_id==m_id && entity.m_is_team_leader==false) {
				list.Add(entity);
				entity.m_team_member_id = i;
				i++;
			}
		}
		m_members = list.ToArray();
	}
	public void ToggleTeamAI() {
		if(m_team_ai==AITYPE.FIGHT) {
			SetTeamAI(AITYPE.FLEE);
		}
		else if(m_team_ai==AITYPE.FLEE) {
			SetTeamAI(AITYPE.FIGHT);
		}
	}
	public void SetTeamAI(AITYPE ai_type) {
		m_team_ai = ai_type;
		foreach(GEntity entity in m_members) {
			if(!entity.m_is_team_leader && entity.m_ai_type!=AITYPE.INANIMATE) {
				entity.m_ai_type = m_team_ai;
			}
		}
	}
	public GEntity GetLeader() {
		return m_leader;
	}
	public void SetLeader(GEntity entity) {
		m_leader = entity;
	}
	public Vector3 GetWaypoint()
	{
		return m_waypoint;
	}
	public void SetWaypoint(Vector3 waypoint) {
		m_waypoint_set = true;
		m_waypoint = waypoint;
	}
	public void GetWaypointOffset(int team_memeber_id) {
	}
	public Vector3 GetOrbitPosition(int id) {
		return GetOrbitPosition(id,6,2,1,4,0);
	}
	public Vector3 GetOrbitPosition(
		int id,
		int ring_size, 
		int ring_size_increase, 
		int ring_number,
		int ring_spacing,
		int cumulative_size
	) {
		float angle = 0;
		float dist = 0;
		cumulative_size += ring_size;
		if(id >= ring_size) {
			ring_number++;
			return GetOrbitPosition(
				id,
				ring_size+ring_size_increase,
				ring_size_increase,
				ring_number,
				ring_spacing,
				cumulative_size
			);
		}
		angle = (360/(float)ring_size)*id;
		dist = ring_number * ring_spacing;
		return Quaternion.Euler(0,angle,0) * Vector3.forward * dist;
	}
} 