using Adrenak.Tork;
using UnityEngine;

public class PhysicsDemo : MonoBehaviour {
	public Vehicle vehicle;

	void Start () {
		vehicle.SetPlayer(new KeyboardPlayer());
	}
}
