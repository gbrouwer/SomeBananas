using UnityEngine;

namespace StoatVsVole
{
    /// <summary>
    /// Makes a GameObject constantly face the main camera.
    /// Useful for labels, UI elements, or billboard objects.
    /// </summary>
    public class FaceCamera : MonoBehaviour
    {
        #region Private Fields

        private Camera mainCamera;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {

            mainCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (mainCamera != null)
            {
                mainCamera = Camera.main;
                transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
            }
        }



        #endregion
    }
}
