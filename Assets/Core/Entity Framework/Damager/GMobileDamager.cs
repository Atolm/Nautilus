using UnityEngine;
using System.Collections;

public class GMobileDamager : GDamager {
	GMobile m_mobile;
	[SerializeField] float m_move_speed;
	[SerializeField] float m_target_arc_offset = 0;
	
	[SerializeField] Vector2 m_move_direction;
	
	Vector2 m_target_point;
	
	MOVE_TYPE m_move_type = MOVE_TYPE.NONE;

	protected override void Start () {
		base.Start();
		m_mobile = gameObject.AddComponent<GMobile>();
	}
	
	protected override void Update () {
		base.Update();
		
		switch(m_move_type) {
			case MOVE_TYPE.DIRECTION:
				m_mobile.Move(m_move_speed,m_move_direction);
				break;
			case MOVE_TYPE.TARGET_POINT:
				m_mobile.MoveArcX(m_move_speed,m_target_point,m_target_arc_offset);
				break;
		}
	}
	
	public void SetTargetPoint(Vector2 point) {
		m_target_point = point;
		m_move_type = MOVE_TYPE.TARGET_POINT;
	}
	
	public void SetDirection(Vector2 direction) {
		m_move_direction = direction;
		m_move_type = MOVE_TYPE.DIRECTION;
	}
}
