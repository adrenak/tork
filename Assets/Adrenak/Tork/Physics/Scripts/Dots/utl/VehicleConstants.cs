using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleConstants : MonoBehaviour
{
	/// <summary>
	/// to adjust frictionForceaction on wheel on Dots Physics
	/// This needed because Dots Physics And Unity Physics are different
	/// </summary>
	[Tooltip("To adjust Friction Force Action on wheel on Dots Physics")]
	public float massMul;
	[Tooltip("Increase If vehicle is to Slow decrese if have jerk")]
	public float torqueMul;

}
