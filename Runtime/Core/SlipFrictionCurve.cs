namespace Adrenak.Tork {
    [System.Serializable]
    public class SlipFrictionCurve {
        public float maxCoeff;
        public float minCoeff;
        public float softness;
        public float criticalSlip;

        public float Evaluate(float slip) {
            if (slip < criticalSlip - softness / 2)
                return maxCoeff;
            else if (slip > criticalSlip + softness / 2)
                return minCoeff;
            else {
                float delta = slip - (criticalSlip - softness / 2);
                return minCoeff + delta * (maxCoeff - minCoeff) / softness;
            }
        }
    }
}
