using UnityEngine;
using System.Collections;

[System.Serializable]
public class GActionTimer {
	public ACTION m_action_id;
	[SerializeField] GameTimer m_windup_timer;
	[SerializeField] GameTimer m_action_timer;
	[SerializeField] GameTimer m_cooldown_timer;
	[SerializeField] GameTimer m_disable_timer;
	GACallback m_act_start_callback;
	GACallback m_act_end_callback;
	bool m_deactivated = false;
	
	public GActionTimer(
		ACTION action_id,
		float windup, 
		float action,
		float cooldown, 
		float disable, 
		GACallback act_start_callback, 
		GACallback act_end_callback
	) {
		m_action_id = action_id;
		m_windup_timer = new GameTimer(windup,false,BeginAction);
		m_action_timer = new GameTimer(action,false,EndAction);
		m_cooldown_timer = new GameTimer(cooldown,false,null);
		m_disable_timer = new GameTimer(disable,false,null);
		m_windup_timer.SetTimeComplete(false);
		m_action_timer.SetTimeComplete(false);
		m_cooldown_timer.SetTimeComplete(false);
		m_disable_timer.SetTimeComplete(false);
		m_act_start_callback = act_start_callback;
		m_act_end_callback = act_end_callback;
	}
	
	public void Update() {
		if(!m_windup_timer.IsComplete()) {
			m_windup_timer.Update();
			return;
		}
		
		if(!m_action_timer.IsComplete()) {
			m_action_timer.Update();
			return;
		}
		
		m_cooldown_timer.Update();
		m_disable_timer.Update();
	}
	
	public bool IsReady() {
		return m_windup_timer.IsComplete()
		&& m_action_timer.IsComplete() 
		&& m_cooldown_timer.IsComplete() 
		&& m_disable_timer.IsComplete() 
		&& !m_deactivated;
	}
	
	public bool InUse() {
		return !m_windup_timer.IsComplete() || !m_action_timer.IsComplete();
	}
	
	public bool InUseOrCooldown() {
		return !m_windup_timer.IsComplete() || !m_action_timer.IsComplete() || !m_cooldown_timer.IsComplete();
	}
	
	public bool AttemptAction() {
		//Begin action, return whether action started successfully.
		if(IsReady()) {
			if(m_windup_timer.interval==0) {
				//For instant actions, begin immediately.
				BeginAction();
			}
			else {
				//Otherwise set the windup.
				m_windup_timer.ResetTime();
			}
			return true;
		}
		return false;
	}
	
	void BeginAction() {
		if(m_act_start_callback!=null) {
			m_act_start_callback(m_action_id);
		}
		if(m_action_timer.interval==0) {
			//If the action ends instantly, go straight to the end callback.
			EndAction();
		}
		else {
			//Otherwise, set a timer for it!
			m_action_timer.ResetTime();
		}
	}
	
	void EndAction() {
		if(m_act_end_callback!=null) {
			m_act_end_callback(m_action_id);
		}
		m_cooldown_timer.ResetTime();
	}
	
	public void Sustain() {
		if(!IsActionComplete()) {
			m_action_timer.ResetTime();
		}
	}
	
	public void Interrupt(bool force_cooldown) {
		m_windup_timer.SetTimeComplete(false);
		m_action_timer.SetTimeComplete(false);
		if(force_cooldown) {
			m_cooldown_timer.ResetTime();
		}
	}
	
	public void Disable() {
		m_disable_timer.ResetTime();
	}
	
	public void SetDisableDuration(float time) {
		m_disable_timer.SetInterval(time,false);
		m_disable_timer.SetTimeComplete(false);
	}
	
	public void SetActive(bool active) {
		m_deactivated = !active;
	}
	
	public float GetTimeInCast() {
		return m_action_timer.GetTime();
	}
	
	public float GetActionProgress() {
		return m_action_timer.GetProgress();
	}
	
	public bool IsActionComplete() {
		return m_action_timer.IsComplete();
	}
	
	public float GetTimeInCooldown() {
		return m_cooldown_timer.GetTime();
	}
	
	public float GetCooldownProgress() {
		return m_cooldown_timer.GetProgress();
	}
	
	public bool IsCooldownComplete() {
		return m_cooldown_timer.IsComplete();
	}
}

//public delegate void GACallback(); //Future projects might not need the ID in the callback.
public delegate void GACallback(ACTION action_id);