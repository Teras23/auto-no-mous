using UnityEngine;
using UnityEngine.UI;

public class TimeController : MonoBehaviour {
	public Text text;
	public Slider slider;

	public void UpdateTime() {
		float speed = slider.value * slider.value;
		text.text = "Time Speed\n" + System.Math.Round(speed, 2) + "x";
		Time.timeScale = speed;
	}
}
