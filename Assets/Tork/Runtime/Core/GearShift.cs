using Adrenak.Tork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearShift : MonoBehaviour {
    public Motor motor;
    public List<float> ratios;
    public int current;
    public float finalDrive;
    public float axleRatio;
    public float torque;

    void Update() {
        if (Input.GetKeyDown(KeyCode.UpArrow) && current != ratios.Count - 1)
            current++;
        if (Input.GetKeyDown(KeyCode.DownArrow) && current != 0)
            current--;

        motor.maxTorque = torque * ratios[current] * finalDrive * axleRatio;

    }
}
