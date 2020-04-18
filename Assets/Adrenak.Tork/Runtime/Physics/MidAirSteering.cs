using UnityEngine;

public class MidAirSteering : MonoBehaviour {
    [Header("Mid Air Steer")]
    public float midAirSteerTorque = 1500;
    public float midAirSteerInput;
    public Rigidbody m_Rigidbody;

    void FixedUpdate() {
        SteerMidAir();
    }

    void SteerMidAir() {
        if (!Mathf.Approximately(midAirSteerInput, 0))
            m_Rigidbody.AddTorque(new Vector3(0, midAirSteerInput * midAirSteerTorque, 0));
    }
}
