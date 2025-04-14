using UnityEngine;

namespace TrafficJam
{

    public class TrafficJamStateChanger : MonoBehaviour
    {
        Material WallMaterial;
        BoxCollider WallCollider;
        Vector3 WallLocalPosition;
        Quaternion WallLocalRotation;
        Vector3 WallLocalScale;
        public Material exitMaterial;

        void Awake()
        {

            WallMaterial = GetComponent<Renderer>().material;
            WallCollider = GetComponent<BoxCollider>();
            WallLocalPosition = transform.localPosition;
            WallLocalRotation = transform.localRotation;
            WallLocalScale = transform.localScale;

        }

        public void Reset()
        {
            GetComponent<Renderer>().material = WallMaterial;
            GetComponent<BoxCollider>().isTrigger = WallCollider.isTrigger;
            GetComponent<Renderer>().transform.localPosition = WallLocalPosition;
            GetComponent<Renderer>().transform.localRotation = WallLocalRotation;
            GetComponent<Renderer>().transform.localScale = WallLocalScale;
            this.tag = "wall";
        }

        public void SetToExit()
        {
            GetComponent<Renderer>().material = exitMaterial;
            GetComponent<BoxCollider>().isTrigger = true;
            //GetComponent<Renderer>().transform.localScale -= new Vector3(0f, 0.9999f, 0f);
            //GetComponent<Renderer>().transform.localPosition -= new Vector3(0f, 1f, 0f);
            this.tag = "goal";

        }
    }
}
