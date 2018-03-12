using UnityEngine;
using System.Collections;

public class GameVisual : MonoBehaviour {
	public float duration_time = -1; //-1 is infinite.
	private float duration_counter = 0;
	
	public float m_start_time = -1f;
	GameTimer m_start_timer;

	public float move_speed = 0;
	public Vector3 move_direction = Vector3.zero;
	public bool m_move_looping = false;
	bool m_move_has_looped = false;

	public float m_rotation_time;
	GameTimer m_rotation_timer;
	public bool m_rotation_looping = false;
	public bool m_rotation_has_looped = false;
	public Vector3 m_rotation = Vector3.zero;
	Quaternion m_initial_rotation = Quaternion.identity;
	
	//Scaling
	Vector3 m_scale_initial;
	public Vector3 m_scale_magnitude = Vector3.one;
	public float m_scale_time = -1;
	bool m_scale_has_looped = false;
	public bool m_scale_looping = false;
	public bool m_scale_stop = false;
	GameTimer m_scale_timer;

	public float detach_parent_time = -1;
	private float detach_counter = 0;

	public float m_fade_time = -1; //-1 is no fade.
	public float m_fade_start = 0; //Alpha % at fade start.
	public float m_fade_end = 0; //Alpha % at fade end.
	bool m_fade_has_looped = false; //Loop fading in/out.
	public bool m_fade_looping = false; //Loop fading in/out.
	private float m_fade_counter = 0;
	GameTimer m_fade_timer;
	
	public bool m_start_enabled = true;
	
	SpriteRenderer m_sprite_renderer;
	MeshRenderer m_mesh_renderer;
	
	void Start() {
		m_start_timer = new GameTimer(m_start_time,false,StartEnable);
		
		m_scale_initial = transform.localScale;
		m_initial_rotation = transform.rotation;
		m_scale_timer = new GameTimer(m_scale_time,m_scale_looping,EndScale);
		m_fade_timer = new GameTimer(m_fade_time,m_fade_looping,EndFade);
		m_rotation_timer = new GameTimer(m_rotation_time,m_rotation_looping,EndRotation);
		
		m_sprite_renderer = GetComponent<SpriteRenderer>();
		m_mesh_renderer = GetComponent<MeshRenderer>();
		
		if(m_sprite_renderer!=null) {
			//Color c = m_sprite_renderer.color;
			//m_sprite_renderer.color = new Color(c.r,c.g,c.b,m_fade_start);
		}
		if(m_mesh_renderer!=null) {
			//Color c = m_mesh_renderer.material.GetColor("_TintColor");
			//m_mesh_renderer.material.SetColor("_TintColor",new Color(c.r,c.g,c.b,m_fade_start));
		}
	}

	void Update () {
		UpdateStart();
		if(!m_start_enabled) {return;}
		
		var dt = Time.deltaTime;
		UpdateFade();
		DetachTimer (dt);
		Movement (dt);
		UpdateRotation();
		UpdateScale();
		LifeTimer (dt);
	}
	
	void UpdateStart() {
		if(m_start_time<0) { return; }
		m_start_timer.Update();
	}
	
	void StartEnable() {
		m_start_enabled = true;
	}
	
	void UpdateScale() {
		if(m_scale_stop && m_scale_timer.IsComplete()){ return; }
		if(m_scale_time<0) {return;}
		m_scale_timer.Update();
		if(m_scale_has_looped){
			transform.localScale = new Vector3(
				m_scale_initial.x + (m_scale_initial.x*m_scale_magnitude.x) * (1-m_scale_timer.GetProgress()),
				m_scale_initial.y + (m_scale_initial.y*m_scale_magnitude.y) * (1-m_scale_timer.GetProgress()),
				m_scale_initial.z + (m_scale_initial.z*m_scale_magnitude.z) * (1-m_scale_timer.GetProgress())
			);
		}
		else {
			transform.localScale = new Vector3(
				m_scale_initial.x + (m_scale_initial.x*m_scale_magnitude.x) * m_scale_timer.GetProgress(),
				m_scale_initial.y + (m_scale_initial.y*m_scale_magnitude.y) * m_scale_timer.GetProgress(),
				m_scale_initial.z + (m_scale_initial.z*m_scale_magnitude.z) * m_scale_timer.GetProgress()
			);
		}
	}
	
	void EndScale() {
		if(m_scale_looping) {
			m_scale_has_looped = !m_scale_has_looped;
		}
	}
	
	void UpdateRotation() {
		if(m_rotation_time<0) {return;}
		m_rotation_timer.Update();

		/* 
		if(m_rotation_timer.IsComplete()) {return;}
		if(!m_rotation_has_looped) {
			//transform.Rotate(m_rotation*m_rotation_timer.GetProgress());
			transform.rotation = m_initial_rotation * 
			Quaternion.Euler(
				m_rotation.x * m_rotation_timer.GetProgress(),
				m_rotation.y * m_rotation_timer.GetProgress(),
				m_rotation.z * m_rotation_timer.GetProgress()
			);
		}
		else {
			//transform.Rotate(-m_rotation*m_rotation_timer.GetProgress());
			transform.rotation = m_initial_rotation * 
			Quaternion.Euler(
				m_rotation.x * m_rotation_timer.GetProgress(),
				m_rotation.y * m_rotation_timer.GetProgress(),
				m_rotation.z * m_rotation_timer.GetProgress()
			);
		}*/
		if(m_rotation_timer.IsComplete()) {return;}

		float rotation_progress = 0;

		if(m_rotation_has_looped) {
			rotation_progress = 1-m_rotation_timer.GetProgress();
		}
		else {
			rotation_progress = m_rotation_timer.GetProgress();
		}

		transform.rotation = m_initial_rotation * 
		Quaternion.Euler(
			m_rotation.x * rotation_progress,
			m_rotation.y * rotation_progress,
			m_rotation.z * rotation_progress
		);
	}

	void EndRotation() {
		if(m_rotation_looping) {
			m_rotation_has_looped = !m_rotation_has_looped;
		}
	}
	
	void UpdateFade() {
		if(m_fade_time<0) {return;}
		m_fade_timer.Update();
		
		float fade_delta = m_fade_end - m_fade_start;

		float fade = 0;
		
		if(m_fade_has_looped){
			fade = m_fade_start + (fade_delta * (1-m_fade_timer.GetProgress()));
		}
		else {
			fade = m_fade_start + (fade_delta * m_fade_timer.GetProgress());
		}
		
		if(m_sprite_renderer!=null) {
			Color c = m_sprite_renderer.color;
			m_sprite_renderer.color = new Color(c.r,c.g,c.b,fade);
		}
		if(m_mesh_renderer!=null) {
			Color c = m_mesh_renderer.material.GetColor("_TintColor");
			m_mesh_renderer.material.SetColor("_TintColor",new Color(c.r,c.g,c.b,fade));
		}
	}
	
	void EndFade() {
		if(m_fade_looping) {
			m_fade_has_looped = !m_fade_has_looped;
		}
	}

	void LifeTimer(float dt) {
		if(duration_time < 0) { return; }
		
		duration_counter += dt;
		
		if(duration_counter > duration_time){
			Destroy(gameObject);
		}
	}

	void DetachTimer(float dt) {
		if(detach_parent_time < 0) { return; }
		
		detach_counter += dt;
		
		if(detach_counter > detach_parent_time){
			transform.parent = null;
		}
	}
	
	public void Movement(float dt) {
		//This ensures speed does not increase in angular movement
		if(move_direction.magnitude > 1) {
			move_direction = move_direction/move_direction.magnitude;
		}
		int loop_direction = 1;
		if(m_move_looping) {
			
		}
		transform.position += (Vector3)move_direction * move_speed * Time.deltaTime;
	}

	void FadeTimer(float dt) {
		if(m_fade_time < 0) { return; }
		//if(!GetComponent<SpriteRenderer> ()) { return; }
		
		m_fade_counter += dt;
		float fade_delta = m_fade_end - m_fade_start;
		float fade_perc = (m_fade_counter / m_fade_time);

		if (fade_perc > 1) {
			fade_perc = 1;
		}
		else if (fade_perc < 0) {
			fade_perc = 0;
		}

		float fade = (m_fade_start + (fade_delta * fade_perc));
		//Color c = GetComponent<SpriteRenderer> ().color;
		if(m_sprite_renderer!=null) {
			Color c = m_sprite_renderer.color;
			m_sprite_renderer.color = new Color(c.r,c.g,c.b,fade);
		}
		if(m_mesh_renderer!=null) {
			Color c = m_mesh_renderer.material.GetColor("_TintColor");
			m_mesh_renderer.material.SetColor("_TintColor",new Color(c.r,c.g,c.b,fade));
		}
		
		//GetComponent<SpriteRenderer> ().color = new Color(c.r,c.g,c.b,fade);
		//GetComponent<

		if(m_fade_looping && (fade_perc==1)) {
			float fs = m_fade_start;
			float fe = m_fade_end;
			m_fade_start = fe;
			m_fade_end = fs;
			m_fade_counter = 0; //NEW
		}
	}
	
	public void Activate(bool state) {
		m_start_enabled = state;
	}
}