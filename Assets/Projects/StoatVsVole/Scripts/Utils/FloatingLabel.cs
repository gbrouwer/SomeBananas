using UnityEngine;
using TMPro;

namespace StoatVsVole
{
    /// <summary>
    /// Creates and manages a floating 3D text label that follows an agent, useful for debugging and visualization.
    /// </summary>
    public class FloatingLabel : MonoBehaviour
    {
        #region Private Fields

        public TextMeshPro textMesh;

        #endregion

        #region Public Fields

        public GlobalSettings globalSettings;
        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            globalSettings = FindAnyObjectByType<GlobalSettings>();
            CreateFloatingLabel();

        }

        private void Update()
        {


        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the floating label's displayed text.
        /// </summary>
        public void SetLabel(string newText, Color color, float fontSize, Vector3 position)
        {
            if (textMesh != null)
            {
                textMesh.text = newText;
                textMesh.color = color;
                textMesh.fontSize = fontSize;
                textMesh.transform.localPosition = position;
            }
        }    

        #endregion

        #region Private Methods

        private void CreateFloatingLabel()
        {
        // Create label object
        GameObject label = new GameObject("FloatingLabel");
        label.transform.SetParent(this.transform);
        label.transform.localPosition = new Vector3(0, 1.0f, 0); // Slightly above the agent

        // Add and configure TextMeshPro
        textMesh = label.AddComponent<TextMeshPro>();
        textMesh.text = "";
        textMesh.fontSize = 6;
        textMesh.color = Color.white;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.enableAutoSizing = false;
        textMesh.overflowMode = TextOverflowModes.Overflow;
        textMesh.fontSharedMaterial.EnableKeyword("OUTLINE_ON");
        textMesh.outlineColor = Color.black;
        textMesh.outlineWidth = 0.4f; // Range: 0 (none) to 1 (thick)

        // Optional: assign a specific font if desiredvvcc
        textMesh.font = Resources.Load<TMP_FontAsset>("Fonts/Raleway-Regular SDF"); // Adjust path if needed

        // Shrink if text looks massive
        label.transform.localScale = Vector3.one * 0.3f;

        // Add script to make it face the camera
        label.AddComponent<FaceCamera>();
        }

        #endregion
    }
}
