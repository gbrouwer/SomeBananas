using UnityEngine;

public class SoftBodyCompute : MonoBehaviour
{
    public ComputeShader computeShader;
    private Mesh mesh;
    private Vector3[] vertices, originalVertices;
    private ComputeBuffer vertexBuffer, originalVertexBuffer;

    public float deformStrength = 0.1f;
    public float recoverySpeed = 0.02f; // Controls how fast it returns to original shape

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        originalVertices = (Vector3[])vertices.Clone(); // Store original shape

        vertexBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 3);
        vertexBuffer.SetData(vertices);

        originalVertexBuffer = new ComputeBuffer(originalVertices.Length, sizeof(float) * 3);
        originalVertexBuffer.SetData(originalVertices);
    }

    void OnCollisionEnter(Collision collision)
    {
        Vector3 impactPoint = transform.InverseTransformPoint(collision.contacts[0].point);

        computeShader.SetVector("_ImpactPoint", new Vector4(impactPoint.x, impactPoint.y, impactPoint.z, 1));
        computeShader.SetFloat("_DeformStrength", deformStrength);
        computeShader.SetFloat("_RecoverySpeed", recoverySpeed);

        computeShader.SetBuffer(0, "vertices", vertexBuffer);
        computeShader.SetBuffer(0, "originalVertices", originalVertexBuffer);

        computeShader.Dispatch(0, vertices.Length / 64, 1, 1);

        vertexBuffer.GetData(vertices);
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }

    void OnDestroy()
    {
        vertexBuffer.Release();
        originalVertexBuffer.Release();
    }
}
