using Adrenak.Tork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPMViz : MonoBehaviour {
    public TorkWheel wheel;

    public void Update() {
        var radianPerSec = wheel.angularSlip;
        var degreesPerSec = radianPerSec * Mathf.Rad2Deg;

        transform.Rotate(degreesPerSec * Time.deltaTime, 0, 0);
    }
}
