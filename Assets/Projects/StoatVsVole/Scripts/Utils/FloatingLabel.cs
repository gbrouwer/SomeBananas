using UnityEngine;

namespace StoatVsVole
{
    /// <summary>
    /// Creates and manages a floating 3D text label that follows an agent, useful for debugging and visualization.
    /// </summary>
    public class FloatingLabel : MonoBehaviour
    {
        #region Private Fields

        private TextMesh textMesh;

        #endregion

        #region Public Fields

        public GlobalSettings globalSettings;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            CreateFloatingLabel();
        }

        private void Update()
        {

            // if (textMesh != null)
            // {
            //     textMesh.text = gameObject.tag; // Example: dynamic tag update
            // }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the floating label's displayed text.
        /// </summary>
        public void SetLabelText(string newText)
        {
            if (textMesh != null)
            {
                textMesh.text = newText;
            }
        }

        /// <summary>
        /// Toggles the visibility of the floating label.
        /// </summary>
        /// <param name="isVisible">Whether the label should be visible.</param>
        public void ToggleVisibility(bool isVisible)
        {
            if (textMesh != null)
            {
                textMesh.gameObject.SetActive(isVisible);
            }
        }

        #endregion

        #region Private Methods

        private void CreateFloatingLabel()
        {
            // Create a new GameObject for the label
            GameObject label = new GameObject("FloatingLabel");
            label.transform.SetParent(this.transform);
            label.transform.localPosition = new Vector3(0, 2f, 0); // Slightly above the agente

            textMesh = label.AddComponent<TextMesh>();
            // textMesh.text = gameObject.tag;
            textMesh.fontSize = 32;
            textMesh.characterSize = 0.1f;
            textMesh.color = Color.white;
            textMesh.alignment = TextAlignment.Center;
            textMesh.anchor = TextAnchor.MiddleCenter;

            // Make the label always face the camera
            label.AddComponent<FaceCamera>();
        }

        #endregion
    }
}
