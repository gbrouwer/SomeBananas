using UnityEngine;

namespace StoatVsVole
{
    public class FloatingLabel : MonoBehaviour
    {
        private TextMesh textMesh;
        public GlobalSettings globalSettings;
        void Start()
        {
            // Create a new GameObject for the label
            GameObject label = new GameObject("FloatingLabel");
            label.transform.SetParent(this.transform);
            label.transform.localPosition = new Vector3(0, 2f, 0); // Slightly above the agent

            textMesh = label.AddComponent<TextMesh>();
            textMesh.text = gameObject.tag;
            textMesh.fontSize = 32;
            textMesh.characterSize = 0.1f;
            textMesh.color = Color.white;
            textMesh.alignment = TextAlignment.Center;
            textMesh.anchor = TextAnchor.MiddleCenter;

            // Optional: make sure it always faces the camera
            label.AddComponent<FaceCamera>();
        }

        public void SetLabelText(string newText)
        {
            if (textMesh != null)
            {
                
                textMesh.text = newText;
            }
        }  


        void Update()
        {
            // if (textMesh != null)
            // {
            //     textMesh.text = gameObject.tag; // Update dynamically if tag changes
            // }
        }
    }
}
