using UnityEngine;
using System.Collections;

public static class Game {
	public static GController controller;
	public static GameObject player_prefab;
	public static GEntity player;
	public static GEntity reserve_player;
	public static GCamera camera;
	public static MapGenerator map;
	public static int floor_id = 0;
}

enum MOVE_TYPE {
	NONE,
	TARGET_POINT,
	DIRECTION
}