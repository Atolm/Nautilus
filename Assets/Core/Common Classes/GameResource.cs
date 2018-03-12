using UnityEngine;
using System.Collections;

public class GameResource {
	float m_value = 0;
	float m_min_value = 0;
	float m_max_value = 0;

	public GameResource(float value, float min_value, float max_value) {
		SetMinMaxValues(min_value,max_value);
		SetValue(value);
	}
	
	public float GetValue() {
		return m_value;
	}
	
	public float GetMaxValue() {
		return m_max_value;
	}
	
	public float GetMinValue() {
		return m_min_value;
	}
	
	public float GetPercent() {
		return m_value/(m_max_value-m_min_value);
	}
	
	public void SetValue(float value) {
		m_value = value;
		ClampValue();
	}
	
	public void SetMaxValue(float max_value) {
		m_value = max_value;
		ClampValue();
	}
	
	public void SetMinValue(float min_value) {
		m_value = min_value;
		ClampValue();
	}
	
	public void SetMinMaxValues(float min_value,float max_value) {
		m_min_value = min_value;
		m_max_value = max_value;
		ClampValue();
	}
	
	public void ChangeValue(float amount) {
		m_value += amount;
		ClampValue();
	}
	
	public void ChangePercent(float amount) {
		float change_amount = amount * (m_max_value-m_min_value);
		ChangeValue(change_amount);
	}
	
	void ClampValue() {
		if(m_value > m_max_value) {m_value = m_max_value;}
		if(m_value < m_min_value) {m_value = m_min_value;}
	}
	
	public bool IsMax() {
		return m_value==m_max_value;
	}
	
	public bool IsMin() {
		return m_value==m_min_value;
	}
}
