using UnityEngine;

namespace StoatVsVole
{
    public class CameraToggle : MonoBehaviour
    {
        public Camera cameraA;
        public Camera cameraB;
        public KeyCode toggleKey = KeyCode.C;

        void Start()
        {
            if (cameraA != null && cameraB != null)
            {
                cameraA.enabled = true;
                cameraB.enabled = false;
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                if (cameraA != null && cameraB != null)
                {
                    cameraA.enabled = !cameraA.enabled;
                    cameraB.enabled = !cameraB.enabled;
                }
            }
        }
    }
}
