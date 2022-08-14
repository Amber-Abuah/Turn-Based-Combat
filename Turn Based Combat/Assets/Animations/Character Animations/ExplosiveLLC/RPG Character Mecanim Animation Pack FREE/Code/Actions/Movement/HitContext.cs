using UnityEngine;

namespace RPGCharacterAnims.Actions
{
    public class HitContext
    {
        public int number;
        public Vector3 direction;
        public float force;
        public float variableForce;
        public bool relative;

        public HitContext()
        {
            number = -1;
            direction = Vector3.zero;
            force = 8f;
            variableForce = 4f;
            relative = true;
        }

        public HitContext(int number, Vector3 direction, float force = 8f, float variableForce = 4f, bool relative = true)
        {
            this.number = number;
            this.direction = direction;
            this.force = force;
            this.variableForce = variableForce;
            this.relative = relative;
        }
    }
}