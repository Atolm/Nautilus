using UnityEngine;
using System.Collections;

[System.Serializable]
public class GameTimer {
	public float time = 0;
	public float interval = 1;
	bool active = true;
	bool looping = true;
	bool callback_done = false;
	CTCallback callback;
	
	public GameTimer(float interval, bool looping, CTCallback callback) {
		this.interval = interval;
		this.looping = looping;

		if(callback!=null){
			this.callback = callback;
		}
	}
	
	public void Update () {
		Update (1);
	}

	public void Update(float multiplier) {
		if(!active) { return; }
		
		time += Time.deltaTime*multiplier;
		if(IsComplete()) {
			if(callback!=null && ! callback_done){
				callback();
				callback_done = true;
			}

			if(looping) {
				ResetTime();
			}
		}
	}
	
	public bool IsComplete() {
		return (time >= interval);
	}
	
	public void SetTimeComplete(bool do_callback) {
		this.time = interval;
		callback_done = !do_callback;
	}
	
	public void SetTime(float time) {
		this.time = time;
	}

	public float GetTime() {
		return time;
	}

	public float GetTimeRemaining() {
		return (interval - time);
	}

	public void ResetTime() {
		SetTime(0);
		callback_done = false;
	}

	public void SetInterval(float interval, bool reset_progress) {
		this.interval = interval;

		if(reset_progress) {
			ResetTime ();
		}
	}

	public float GetInterval() {
		return interval;
	}

	public float GetProgress() {
		return (time / interval);
	}
	
	public void SetActive(bool state) {
		active = state;
	}
}

public delegate void CTCallback();

