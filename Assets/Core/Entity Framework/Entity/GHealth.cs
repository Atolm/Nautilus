using UnityEngine;
using System.Collections;

//Component of the generic 'Entity' type used for player/monsters
//Used to track health, handles death/hit effects.
public class GHealth : MonoBehaviour {
	GameObject owner;

	public GameObject respawn_point;	//The position where the mob will respawn when killed.
	public int max_health = 100;		//Mobs maximum health.
	public float invuln_time = 0;		//Gives a timed (seconds) invulnverability when the mob is hurt.
	public bool damagable = true;		//If this is False, then the mob cannot be damage at all.

	public float death_delay = 0;
	public float death_counter = 0;
	public bool destroy_on_death = true;

	public GameObject hit_effect;				//Effect to play when mob is damaged.
	public GameObject predeath_effect;			//Effect to play when mob is reduced to 0 hp.
	public GameObject death_effect;			//Effect to play when mob is killed.
	public GameObject heal_effect;
	
	public float invuln_counter = 0;	//Counter to time invulnerability periods.
	public int health = 10;			//Starting health will be set to equal max_health.

	/* You can also remove the monobehaviour extention and instead instantiate these...
	public GHealth(GameObject owner, int health) {
		this.owner = owner;
		this.health = health;
		max_health = health;
	}*/
	
	public bool health_set = false;
	void Start () {
		owner = gameObject;
		if(!health_set) {
			health = max_health;
			health_set = true;
		}
	}
	
	void Update () {
		float dt = Time.deltaTime;
		Invulnerable(dt);
		
		if(health <= 0){
			death_counter += dt;

			if(death_counter >= death_delay) {
				Die();
			}
		}
	}

	public void SetHealth(int value) {
		max_health = value;
		health = value;
	}
	
	void Invulnerable(float dt){
		if(invuln_counter > 0){
			invuln_counter -= dt;
		}
		if(invuln_counter < 0){
			invuln_counter = 0;
		}
	}
	
	void DamagePercent(float percent) {
		int dmg = (int)percent*max_health;
		Damage(dmg);
	}
	
	public void Damage(int amount) {
		if(!damagable){
			return;
		}
		
		if(invuln_counter > 0){
			return;
		}
		
		if(hit_effect && amount > 0){
			PlayHitSplash();
		}
		
		health -= amount;
		invuln_counter = invuln_time;
		
		if(health <= 0){
			health = 0;
			if(predeath_effect) {
				GameObject pd = (GameObject)GameObject.Instantiate(predeath_effect,owner.transform.position,owner.transform.rotation);
				pd.transform.parent = owner.transform;
			}
		}
	}
	
	public void Repair(int amount) {
		if(heal_effect != null) {
			GameObject.Instantiate(heal_effect,owner.transform.position,owner.transform.rotation);
		}
		health += amount;
		if(health > max_health){
			health = max_health;
		}
	}

	public void SetDamagable(bool state) {
		damagable = state;
	}

	public bool CanTakeDamage() {
		if(invuln_counter <= 0 && damagable) {
			return true;
		}
		return false;
	}
	
	public float HealthPerc(){
		float hp = (float)health/(float)max_health;
		return hp;
	}
	
	void Die() {
		if(death_effect){
			GameObject.Instantiate(death_effect,owner.transform.position,owner.transform.rotation);
		}
		
		if(respawn_point) {
			Respawn();
		}
		else if(destroy_on_death) {
			GameObject.Destroy(owner);
		}
	}
	
	void PlayHitSplash() {
		Vector3 pos = new Vector3 (owner.transform.position.x, owner.transform.position.y + owner.transform.localScale.y * 0.5f, owner.transform.position.z);
		GameObject.Instantiate(hit_effect,pos,owner.transform.rotation);
	}
	
	void Respawn() {
		owner.transform.position = respawn_point.transform.position;
		Repair(max_health);
	}
}
