using UnityEngine;
using System.Collections;

//Component of the generic 'Entity' type used for player/monsters
//Move the entity using this component.
public class GMobile : MonoBehaviour {
	Rigidbody m_rigidbody;
	CharacterController m_char_controller;
	public bool m_has_moved = false;
	public bool m_is_stationary = true;
	float m_current_speed = 0;
	
	Vector3 m_current_direction = Vector2.zero;
	Vector2 m_current_facing = Vector3.right;
	
	bool m_xclamp_active = false;
	MinMax m_xclamp = new MinMax(0,100);
	
	bool m_yclamp_active = false;
	MinMax m_yclamp = new MinMax(0,100);
	
	GameObject m_flip_node;
	bool m_flip_to_xfacing = false;
	bool m_flip_to_yfacing = false;
	
	//float m_gravity = -9.8f;

	void Start () {
		m_char_controller = GetComponent<CharacterController>();
		//m_rigidbody = GetComponent<Rigidbody>();
	}
	
	void Update () {
		UpdateMovement();
		UpdateClamp();
	}
	
	void UpdateMovement () {
		//Ends movement if Move() is not called.
		if(m_has_moved) {
			m_has_moved=false;
		}
		else {
			m_is_stationary = true;
		}
	}
	
	public void Move(float speed, Vector2 direction_2d) {
		//Transform 2D input direction in 3D movement directions.
		//Vector3 direction = Quaternion.Euler(0, 45, 0) * new Vector3(direction_2d.x,0,direction_2d.y);
		Vector3 direction = new Vector3(direction_2d.x,0,direction_2d.y);
		
		//This ensures speed does not increase in angular movement
		if(direction.magnitude > 1) {
			direction = direction/direction.magnitude;
		}
		
		m_char_controller.Move(TurnTime.deltaTime * speed * direction);
		
		m_current_speed = speed;
		m_current_direction = direction;

		if(direction.magnitude>0.1 && speed!=0) {
			SetFacing(direction);
			m_has_moved = true;
			m_is_stationary = false;
		}
	}
	
	public void MoveAngle(float speed, float angle) {
		Quaternion rotation = Quaternion.Euler(0,0,angle);
		Vector2 direction = Util.GetDirectionFromRotation(rotation,Vector3.up);
		Move(speed,direction);
	}
	
	public void MoveTowards(float speed, Vector2 point) {
		Vector2 direction = new Vector2 (
			point.x-transform.position.x,
			point.y-transform.position.z
		);
		Move(speed,direction);
	}
	
	public void MoveArcX(float speed, Vector2 target, float offset) {
		float xdist = Mathf.Abs(target.x - transform.position.x);
		float yoffset = offset * xdist;
		Vector2 arc_target = new Vector2 (target.x,target.y+yoffset);
		MoveTowards(speed,arc_target);
	}
	
	public bool HasMoved() {
		return m_has_moved;
	}

	public bool IsStationary() {
		return m_is_stationary;
	}
	
	public Vector3 GetMovementDirection() {
		return m_current_direction;
	}
	
	public float GetMovementSpeed() {
		return m_current_speed;
	}
	
	public void SetFacing(Vector3 facing) {
		m_current_facing = facing;
	}
	
	public Vector2 GetFacing() {
		return m_current_facing;
	}
	
	public Quaternion GetFacingAngle() {
		return Quaternion.LookRotation(m_current_facing);
	}
	
	public void UpdateClamp() {
		if(m_xclamp_active) {
			transform.position = new Vector3(
				Mathf.Clamp(transform.position.x,m_xclamp.min,m_xclamp.max),
				transform.position.y,
				transform.position.z
			);
		}
		if(m_yclamp_active) {
			transform.position = new Vector3(
				transform.position.x,
				Mathf.Clamp(transform.position.y,m_yclamp.min,m_yclamp.max),
				transform.position.z
			);
		}
	}
	
	public void SetClampX(float min, float max, bool active) {
		m_xclamp = new MinMax(min,max);
		m_xclamp_active = active;
	}
	
	public void SetClampY(float min, float max, bool active) {
		m_yclamp = new MinMax(min,max);
		m_yclamp_active = active;
	}
	
	void UpdateFacing() {
	}
	
	
	public void SetFlip(GameObject node,bool xflip, bool yflip) {
		//Sets a specific "node" gameobject (eg the appearance) to face the direction of travel.
		m_flip_node = node;
		m_flip_to_xfacing = xflip;
		m_flip_to_yfacing = yflip;
	}
}
