using UnityEngine;
using System.Collections;

public class GCamera : MonoBehaviour {
	Camera m_camera;
	//MinMax m_x_extents = new MinMax(-30,130);
	//MinMax m_z_extents = new MinMax(20,240);
	MinMax m_x_extents = new MinMax(-30,130);
	MinMax m_z_extents = new MinMax(20,240);

	void Start () {
		m_camera = transform.Find("Main Camera").GetComponent<Camera>();
	}
	
	void Update () {
	
	}
	
	void LateUpdate() {
		if(!Game.player) {return;}
		//transform.position = Game.player.transform.position;

		MinMax x_extents = Game.map.m_x_extents;
		MinMax z_extents = Game.map.m_z_extents;

		Vector3 bl = Util.RotateDirectionToMatchCamera(Game.map.m_bottomleft+new Vector3(10,0,10));
		Vector3 br = Util.RotateDirectionToMatchCamera(Game.map.m_bottomright+new Vector3(-10,0,10));
		Vector3 tl = Util.RotateDirectionToMatchCamera(Game.map.m_topleft+new Vector3(10,0,-10));
		Vector3 tr = Util.RotateDirectionToMatchCamera(Game.map.m_topright+new Vector3(-10,0,-10));
		
		Vector3 pos = Game.player.transform.position;
		pos = Util.RotateDirectionToMatchCamera(pos);
		pos = new Vector3(
			Mathf.Clamp(pos.x,tl.x,br.x),
			Mathf.Clamp(pos.y,-1000,1000),
			Mathf.Clamp(pos.z,bl.z,tr.z)
		);
		pos = Util.RotateCameraSpaceToMatchWorld(pos);
		transform.position = pos;
	}
}
