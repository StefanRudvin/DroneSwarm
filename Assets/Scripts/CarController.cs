using UnityEngine;

namespace a
{
    public class CarController : MonoBehaviour
    {
        public float speed = 20f;
        void Update()
        {
            // Move the object forward along its z axis 1 unit/second.
            transform.Translate(Vector3.forward * Time.deltaTime * speed);

            // Move the object upward in world space 1 unit/second.
            // transform.Translate(Vector3.up * Time.deltaTime, Space.World);
        }
    }
}