using UnityEngine;
using System.Collections;

//Control the animation and other cosmetic visual elements of the entity.
public class GActor : MonoBehaviour {
	public GEntity m_entity;
	public Transform m_root;
	public Animator m_animator;
	public SpriteRenderer m_renderer;
	public Transform m_shadow_root;
	public Animator m_shadow_animator;
	public SpriteRenderer m_shadow_renderer;
	[SerializeField] string m_last_played = "";
	string m_default_anim = "Stand";
	bool m_blocking_anim = false;
	bool m_turned = false;

	void Start () {
		m_entity = GetComponent<GEntity>();
		m_root = transform.Find("Appearance");
		m_animator = transform.Find("Appearance").gameObject.GetComponent<Animator>();
		m_renderer = transform.Find("Appearance").gameObject.GetComponent<SpriteRenderer>();
		m_shadow_root = transform.Find("Shadow");
		if(m_shadow_root!=null) {
			m_shadow_animator = transform.Find("Shadow").gameObject.GetComponent<Animator>();
			m_shadow_renderer = transform.Find("Shadow").gameObject.GetComponent<SpriteRenderer>();
		}
		
	}
	
	void Update () {
		if(m_entity.m_ai_type==AITYPE.INANIMATE) {
			return;
		}
		GMobile mob = m_entity.m_mobile;
		GCombatant com = m_entity.m_combatant;

		//Vector3 dir = Quaternion.AngleAxis(-Camera.main.transform.rotation.eulerAngles.y,Vector3.up) * mob.GetMovementDirection();
		Vector3 dir = Util.RotateDirectionToMatchCamera(mob.GetMovementDirection());

		if (
			m_renderer.flipX!=m_turned
			&& m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 
			&& !m_animator.IsInTransition(0)
			) {
			m_renderer.flipX = m_turned;
			if(m_shadow_animator!=null) {
				m_shadow_renderer.flipX = m_turned;
			}
		}
		if(mob.IsStationary() && !IsPlaying("Turn")) {
			PlayNormal("Stand");
		}
		else if(dir.x > 0) {
			if(m_turned==false) {
				PlayNormal("Turn");
				m_turned = true;
			}
			else if(!IsPlaying("Turn")) {
				PlayNormal("Walk");
			}
		}
		else if(dir.x < 0) {
			if(m_turned==true) {
				PlayNormal("Turn");
				m_turned = false;
			}
			else if(!IsPlaying("Turn")) {
				PlayNormal("Walk");
			}
		}
	}
	
	public void PlayNormal(string anim) {
		if(IsPlayingBlockingAnim()) {return;}
		Play(anim, false);
	}
	
	public void PlayBlocking(string anim) {
		if(IsPlayingBlockingAnim()) {return;}
		Play(anim, true);
	}

	public void Play(string anim, bool is_blocking) {
		if(!m_animator || !m_animator.gameObject.activeSelf) {return;}
		if(IsPlaying(anim)) {
			return;
		}

		m_animator.Play(anim);
		if(m_shadow_animator!=null) {
			m_shadow_animator.Play(anim);
		}

		m_last_played = anim;
		m_blocking_anim = is_blocking;
	}
	
	public bool IsPlayingBlockingAnim() {
		return m_blocking_anim && IsPlaying(m_last_played);
	}
	
	public bool IsPlaying(string anim) {
		return m_animator.GetCurrentAnimatorStateInfo(0).IsName(anim);
	}

    bool AnimatorIsPlaying() {
        return m_animator.GetCurrentAnimatorStateInfo(0).length > m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }
}
