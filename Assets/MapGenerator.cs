using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour {
	Vector2 m_map_size = new Vector2(40,30);
	float m_map_square_size = 5;
	float m_map_base_height = 1.25f;
	string[,] m_terrain_map;
	Vector2 m_section_size = new Vector2(10,10);
	List<MapSection> m_section_list = new List<MapSection>();
	MinMax m_room_count_rnd = new MinMax(6,12);
	MinMax m_room_size_rnd = new MinMax(2,5);
	int m_min_corridors = 12;
	
	
	//Set during runtime;
	int m_room_count = 0;
	int m_corridor_count = 0;
	bool m_exit_path_found = false;
	
	GameObject m_wall_prefab;
	GameObject m_entrance_prefab;
	GameObject m_exit_prefab;
	GameObject m_player_prefab;
	GameObject[] m_map_features;
	
	//Mob List
	List<GameObject> m_spawn_table = new List<GameObject>();
	List<GameObject> m_loot_table = new List<GameObject>();

	public MinMax m_x_extents = new MinMax(-30,130);
	public MinMax m_z_extents = new MinMax(20,240);

	public Vector3 m_topleft;
	public Vector3 m_topright;
	public Vector3 m_bottomleft;
	public Vector3 m_bottomright;
	

	// Use this for initialization
	void Start () {
		Game.map = this;

		m_map_features = new GameObject[]{
			(GameObject)Resources.Load("Environment/Outcrop")
		};
		
		m_wall_prefab = (GameObject)Resources.Load("BasicWall_02");
		m_entrance_prefab = (GameObject)Resources.Load("EntranceMarker");
		m_exit_prefab = (GameObject)Resources.Load("ExitMarker");
		
		m_player_prefab = (GameObject)Resources.Load("Mobiles/Player");
		
		PopulateSpawnTable();
		PopulateLootTable();
		
		GenerateMap();

		m_x_extents = new MinMax(0,m_map_size.x*m_map_square_size);
		m_z_extents = new MinMax(0,m_map_size.y*m_map_square_size);

		m_bottomleft = new Vector3(0,0,0);
		m_bottomright = new Vector3(m_map_size.x*m_map_square_size,0,0);
		m_topleft = new Vector3(0,0,m_map_size.y*m_map_square_size);
		m_topright = new Vector3(m_map_size.x*m_map_square_size,0,m_map_size.y*m_map_square_size);
	}
	
	void PopulateSpawnTable() {
		m_spawn_table.Add(Resources.Load<GameObject>("Mobiles/Enemy7"));
		m_spawn_table.Add(Resources.Load<GameObject>("Mobiles/Enemy8"));
		m_spawn_table.Add(Resources.Load<GameObject>("Mobiles/Enemy9"));
		m_spawn_table.Add(Resources.Load<GameObject>("Mobiles/Enemy10"));
		m_spawn_table.Add(Resources.Load<GameObject>("Mobiles/Enemy11"));
	}
	
	void PopulateLootTable() {
		m_loot_table.Add(Resources.Load<GameObject>("Items/HealthUp"));
		m_loot_table.Add(Resources.Load<GameObject>("Items/EnergyUp"));
		m_loot_table.Add(Resources.Load<GameObject>("Items/SupplyCrate"));
	}
	
	void GenerateMap() {
		m_terrain_map = new string[(int)m_map_size.x,(int)m_map_size.y];
		
		//Flood-fill the map with walls.
		for (int y = 0; y < m_terrain_map.GetLength(1); y++) {
			for (int x = 0; x < m_terrain_map.GetLength(0); x++) {
				m_terrain_map[x,y] = "#";
			}
		}
		
		//Create a number of 'sections' representing distinct areas of the map.
		Vector2 pos;
		int n = 0;
		List<int> rnd_section_ids = new List<int>();
		
		for (int y = 0; y < Mathf.Floor(m_terrain_map.GetLength(1)/m_section_size.y); y++) {
			for (int x = 0; x < Mathf.Floor(m_terrain_map.GetLength(0)/m_section_size.x); x++) {
				pos = new Vector2(x*m_section_size.x,y*m_section_size.y);
				m_section_list.Add(new MapSection(pos,m_section_size));
				rnd_section_ids.Add(n);
				n++;
			}
		}

		int room_size;
		room_size = Random.Range((int)m_room_size_rnd.min,(int)m_room_size_rnd.max);
		m_section_list[0].m_room = new MapRoom(room_size,RoomType.ENTRANCE);
		room_size = Random.Range((int)m_room_size_rnd.min,(int)m_room_size_rnd.max);
		m_section_list[m_section_list.Count-1].m_room = new MapRoom(room_size,RoomType.EXIT);
		
		//Create a random id order for the sections!
		rnd_section_ids.Shuffle();

		//Add rooms to random sections, and assign a random ENTRANCE and EXIT.
		m_room_count = Random.Range((int)m_room_count_rnd.min,(int)m_room_count_rnd.max+1);
		m_room_count = (int)m_room_count_rnd.max;
		
		for(int i = 0; i < m_section_list.Count;i++) {
			
			room_size = Random.Range((int)m_room_size_rnd.min,(int)m_room_size_rnd.max);
			/* 
			if(i==0) {
				m_section_list[rnd_section_ids[i]].m_room = new MapRoom(room_size,RoomType.ENTRANCE);
			}
			else if(i==1) {
				m_section_list[rnd_section_ids[i]].m_room = new MapRoom(room_size,RoomType.EXIT);
			}
			else if(i < m_room_count) {
				m_section_list[rnd_section_ids[i]].m_room = new MapRoom(room_size,RoomType.NORMAL);
			}
			*/
			if(i < m_room_count && m_section_list[rnd_section_ids[i]].m_room==null) {
				m_section_list[rnd_section_ids[i]].m_room = new MapRoom(room_size,RoomType.NORMAL);
			}
		}


		
		//Next let's do a drunken walk from the start to the exit, creating corridors as we go.
		foreach(MapSection s in m_section_list) {
			if(s.m_room!=null && s.m_room.m_type==RoomType.ENTRANCE) {
				SetupRandomExit(s);
				break;
			}
		}
		
		//Cleanup inaccessible rooms
		foreach(MapSection s in m_section_list) {
			if(s.m_room!=null && !s.m_corridors.HasCorridors()) {
				//s.m_room=null; ONLY MATTERS IF YOU HAVE CORRIDORS!
			}
		}
		
		//Then dig out those corridors of the map!!
		foreach(MapSection s in m_section_list) {
			DigCorridors(s);
		}
		
		//Now let's actually dig out those created rooms to the map grid!!
		foreach(MapSection s in m_section_list) {
			DigRoom(s);
		}
		
		//Now let's spawn the walls based on our final map grid!!!!!!!!!!
		/*
		GameObject wall;
		for (int y = 0; y < m_terrain_map.GetLength(1); y++) {
			for (int x = 0; x < m_terrain_map.GetLength(0); x++) {
				if(m_terrain_map[x,y] == "#") {
					Vector3 wall_pos = new Vector3(x*m_map_square_size,m_map_base_height,y*m_map_square_size);
					wall = (GameObject)GameObject.Instantiate(m_wall_prefab,wall_pos,Quaternion.identity);
					wall.transform.parent=transform;
				}
			}
		}*/

		//Place a feature in this room.
		foreach(MapSection s in m_section_list) {
			PlaceRandomFeature(s);
		}
		
		//ADD MARKERS
		GameObject marker;
		foreach(MapSection s in m_section_list) {
			if(s.m_room!=null && s.m_room.m_type==RoomType.ENTRANCE && Game.floor_id!=0) {
				Vector3 marker_pos = new Vector3(s.GetMidpoint().x*m_map_square_size,m_map_base_height,s.GetMidpoint().y*m_map_square_size);
				marker = (GameObject)Instantiate(m_entrance_prefab,marker_pos,Quaternion.identity);
				marker.transform.parent=transform;
			}
			if(s.m_room!=null && s.m_room.m_type==RoomType.EXIT) {
				Vector3 marker_pos = new Vector3(s.GetMidpoint().x*m_map_square_size,m_map_base_height,s.GetMidpoint().y*m_map_square_size);
				marker = (GameObject)Instantiate(m_exit_prefab,marker_pos,Quaternion.identity);
				marker.transform.parent=transform;
			}
		}
		
		//ADD ITEMS:
		int num_items;
		foreach(MapSection s in m_section_list) {
			if(s.m_room!=null) {
				num_items = Random.Range(0,3);
			}
			else if(Random.value<0.3) {
				num_items = 1;
			}
			else {
				num_items = 0;
			}
			for(int i = 0; i < num_items; i++) {
				Instantiate(GetRandomItem(),GetRandomSquareInSection(s)*m_map_square_size,Quaternion.identity);
			}
		}
		
		//Add the player to the map.
		PlacePlayer();
		
		//Add enemies to the map.
		foreach(MapSection s in m_section_list) {
			//PlaceEnemy(s);
		}
		
	}
	
	void DigRoom(MapSection s) {
		if(s.m_room==null) {return;}
		
		int ox = (int)s.m_position.x;
		int oy = (int)s.m_position.y;
		int mx = (int)s.GetMidpoint().x;
		int my = (int)s.GetMidpoint().y;
		int sx = (int)s.m_size.x;
		int sy = (int)s.m_size.y;
		
		for (int y = oy; y < oy+sy; y++) {
			for (int x = ox; x < ox+sx; x++) {
				if(Mathf.Abs(x-mx) < s.m_room.m_size && Mathf.Abs(y-my) < s.m_room.m_size) {
					m_terrain_map[x,y] = ".";
				}
			}
		}
	}
	
	void DigCorridors(MapSection s) {
		int ox = (int)s.m_position.x;
		int oy = (int)s.m_position.y;
		int mx = (int)s.GetMidpoint().x;
		int my = (int)s.GetMidpoint().y;
		int sx = (int)s.m_size.x;
		int sy = (int)s.m_size.y;
		
		for (int y = oy; y < oy+sy; y++) {
			for (int x = ox; x < ox+sx; x++) {
				if(s.m_corridors.north && x==mx && y >= my) {
					m_terrain_map[x,y] = ".";
				}
				if(s.m_corridors.south && x==mx && y <= my) {
					m_terrain_map[x,y] = ".";
				}
				if(s.m_corridors.east && y==my && x >= mx) {
					m_terrain_map[x,y] = ".";
				}
				if(s.m_corridors.west && y==my && x <= mx) {
					m_terrain_map[x,y] = ".";
				}
			}
		}
	}
	
	void SetupRandomExit(MapSection s) {
		m_corridor_count++;
		
		if(s.m_room!=null && s.m_room.m_type==RoomType.EXIT) {
			m_exit_path_found = true;
			//If you've made enough corridors, already, back out...
			if(m_corridor_count > m_min_corridors) {return;}
		}
		
		List<string> exits = new List<string>();
		if(s.GetMidpoint().x-s.m_size.x > 0) {exits.Add("west");}
		if(s.GetMidpoint().x+s.m_size.x < m_map_size.x) {exits.Add("east");}
		if(s.GetMidpoint().y-s.m_size.y > 0) {exits.Add("south");}
		if(s.GetMidpoint().y+s.m_size.y < m_map_size.y) {exits.Add("north");}
		exits.Shuffle();
		
		MapSection next_section;
		foreach(string e in exits) {
			if(e=="north" 
					&& GetSectionAtCoords(s.GetMidpoint() + Vector2.up*s.m_size.y)!=null
					&& GetSectionAtCoords(s.GetMidpoint() + Vector2.up*s.m_size.y).m_corridors.south==false) {
				next_section = GetSectionAtCoords(s.GetMidpoint() + Vector2.up*s.m_size.y);
				if(next_section==null) {continue;}
				
				s.m_corridors.north=true;
				next_section.m_corridors.south=true;
				SetupRandomExit(next_section);
			}
			else if(e=="south" 
					&& GetSectionAtCoords(s.GetMidpoint() - Vector2.up*s.m_size.y)!=null
					&& GetSectionAtCoords(s.GetMidpoint() - Vector2.up*s.m_size.y).m_corridors.north==false) {
				next_section = GetSectionAtCoords(s.GetMidpoint() - Vector2.up*s.m_size.y);
				if(next_section==null) {continue;}
				
				s.m_corridors.south=true;
				next_section.m_corridors.north=true;
				SetupRandomExit(next_section);
			}
			else if(e=="east" 
					&& GetSectionAtCoords(s.GetMidpoint() + Vector2.right*s.m_size.x)!=null
					&& GetSectionAtCoords(s.GetMidpoint() + Vector2.right*s.m_size.x).m_corridors.west==false) {
				next_section = GetSectionAtCoords(s.GetMidpoint() + Vector2.right*s.m_size.x);
				if(next_section==null) {continue;}
				
				s.m_corridors.east=true;
				next_section.m_corridors.west=true;
				SetupRandomExit(next_section);
			}
			else if(e=="west" 
					&& GetSectionAtCoords(s.GetMidpoint() - Vector2.right*s.m_size.x)!=null
					&& GetSectionAtCoords(s.GetMidpoint() - Vector2.right*s.m_size.x).m_corridors.east==false) {
				next_section = GetSectionAtCoords(s.GetMidpoint() - Vector2.right*s.m_size.x);
				if(next_section==null) {continue;}
				
				s.m_corridors.west=true;
				next_section.m_corridors.east=true;
				SetupRandomExit(next_section);
			}
			
			if(m_exit_path_found==true && m_corridor_count > m_min_corridors) {break;}
		}
	}
	
	void PlacePlayer() {
		foreach(MapSection s in m_section_list) {
			if(s.m_room==null) {continue;}
			if(s.m_room.m_type==RoomType.ENTRANCE) {
				Vector3 pos = new Vector3(s.GetMidpoint().x*m_map_square_size,0,s.GetMidpoint().y*m_map_square_size);
				
				if(Game.player==null) {
					GameObject p = (GameObject)Instantiate(m_player_prefab,pos,Quaternion.identity);
					p.GetComponent<GEntity>().m_is_base_player=true;
				}
				else {
					Game.player.transform.position = pos;
				}
				return;
			}
		}
	}
	
	void PlaceEnemy(MapSection s) {
		if(s.m_room==null) {return;}
		if(s.m_room.m_type==RoomType.ENTRANCE) {return;}
		
		float rnd_x = Mathf.Floor(s.GetMidpoint().x + Random.Range(1-s.m_room.m_size,s.m_room.m_size-1)) * m_map_square_size;
		float rnd_y = Mathf.Floor(s.GetMidpoint().y + Random.Range(1-s.m_room.m_size,s.m_room.m_size-1)) * m_map_square_size;
		
		Vector3 pos = new Vector3(rnd_x,0,rnd_y);
		Instantiate(GetRandomSpawn(),pos,Quaternion.identity);
	}

	void PlaceRandomFeature(MapSection s) {
		if(s.m_room==null) {return;}
		if(m_map_features.Length==0) {
			return;
		}
		
		float rnd_x = Mathf.Floor(s.GetMidpoint().x + Random.Range(1-s.m_room.m_size,s.m_room.m_size-1)) * m_map_square_size;
		float rnd_y = Mathf.Floor(s.GetMidpoint().y + Random.Range(1-s.m_room.m_size,s.m_room.m_size-1)) * m_map_square_size;
		
		Vector3 pos = new Vector3(rnd_x,0,rnd_y);
		GameObject rnd_feature = m_map_features[Random.Range(0,m_map_features.Length)];
		Instantiate(rnd_feature,pos,Quaternion.identity);
	}
	

	public float GetMapSquareSize() {
		return m_map_square_size;
	}
	
	MapSection GetSectionAtCoords(Vector2 coords) {
		foreach(MapSection s in m_section_list) {
			if(coords.x >= s.m_position.x
				&& coords.x < s.m_position.x+s.m_size.x
				&& coords.y >= s.m_position.y
				&& coords.y < s.m_position.y+s.m_size.y) {
					return s;
				}
		}
		return null;
	}
	
	public MapSection GetSectionAtPosition(Vector3 position) {
		Vector2 pos = new Vector2(Mathf.FloorToInt(position.x/m_map_square_size),Mathf.FloorToInt(position.z/m_map_square_size));
		return GetSectionAtCoords(pos);
	}
	
	public Vector3 GetAdjacentRoomPosition(Vector3 position) {
		//Vector2 pos = new Vector2(Mathf.FloorToInt(position.x/m_map_square_size),Mathf.FloorToInt(position.z/m_map_square_size));
		//MapSection current_section = GetSectionAtCoords(pos);
		MapSection current_section = GetSectionAtPosition(position);
		
		List<MapSection> rnd_sections = new List<MapSection>();
		Vector3 section_pos;
		if(current_section.m_corridors.east) {
			section_pos = new Vector3(current_section.GetMidpoint().x+m_section_size.x,current_section.GetMidpoint().y);
			rnd_sections.Add(GetSectionAtCoords(section_pos));
		}
		if(current_section.m_corridors.west) {
			section_pos = new Vector3(current_section.GetMidpoint().x-m_section_size.x,current_section.GetMidpoint().y);
			rnd_sections.Add(GetSectionAtCoords(section_pos));
		}
		if(current_section.m_corridors.north) {
			section_pos = new Vector3(current_section.GetMidpoint().x,current_section.GetMidpoint().y+m_section_size.y);
			rnd_sections.Add(GetSectionAtCoords(section_pos));
		}
		if(current_section.m_corridors.south) {
			section_pos = new Vector3(current_section.GetMidpoint().x,current_section.GetMidpoint().y-m_section_size.y);
			rnd_sections.Add(GetSectionAtCoords(section_pos));
		}
		//rnd_sections.Shuffle();
		
		MapSection rnd_sec = rnd_sections[Random.Range(0,rnd_sections.Count-1)];
		
		if(rnd_sections.Count==0) {return Vector3.zero;}
		
		return new Vector3(rnd_sec.GetMidpoint().x*m_map_square_size,0,rnd_sec.GetMidpoint().y*m_map_square_size);
	}
	
	Vector3 GetRandomSquareInSection(MapSection sec) {
		Vector3 rnd_pos = new Vector3(sec.GetMidpoint().x,0,sec.GetMidpoint().y);
		if(sec.m_room!=null) {
			float rnd_x = sec.GetMidpoint().x + Random.Range(-sec.m_room.m_size,sec.m_room.m_size);
			float rnd_y = sec.GetMidpoint().y + Random.Range(-sec.m_room.m_size,sec.m_room.m_size);
			rnd_pos = new Vector3(rnd_x,0,rnd_y);
		}
		return rnd_pos;
	}
	
	GameObject GetRandomSpawn() {
		int rnd_spawn = Game.floor_id;
		rnd_spawn += Random.Range(-1,2);
		rnd_spawn += Random.Range(-1,2);
		rnd_spawn += Random.Range(-1,2);
		rnd_spawn = Mathf.Clamp(rnd_spawn,0,m_spawn_table.Count-1);
		return m_spawn_table[rnd_spawn];
	}
	
	GameObject GetRandomItem() {
		return m_loot_table[Random.Range(0,m_loot_table.Count)];
	}
}

public class MapSection {
	internal Vector2 m_position;
	internal Vector2 m_size;
	internal MapRoom m_room;
	internal MapCorridors m_corridors = new MapCorridors();
	
	public MapSection(Vector2 position,Vector2 size) {
		m_position = position;
		m_size = size;
	}
	
	public Vector2 GetMidpoint() {
		return new Vector2(Mathf.Round(m_position.x+(m_size.x/2)),Mathf.Round(m_position.y+(m_size.y/2)));
	}
}

public class MapRoom {
	internal int m_size;
	internal RoomType m_type = RoomType.NORMAL;
	
	public MapRoom(int size,RoomType type) {
		m_size = size;
		m_type = type;
	}
}

public class MapCorridors {
	internal bool north = false;
	internal bool east = false;
	internal bool south = false;
	internal bool west = false;
	public bool HasCorridors() {
		return north || south || east || west;
	}
}

public enum RoomType {
	NORMAL,
	ENTRANCE,
	EXIT,
}