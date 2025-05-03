// AwesomeInclusiveAgent.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AwesomeInclusiveAgent : MonoBehaviour
{
    public Color agentColor;
    public float neighborSearchRadius = 15f;
    public int numNeighbors = 5;
    public float forceMultiplier = 5f;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        ApplyNeighborForce();
    }

    void ApplyNeighborForce()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, neighborSearchRadius);
        List<Vector3> neighborVectors = new List<Vector3>();

        foreach (var hit in hits)
        {
            if (hit.gameObject != this.gameObject && hit.CompareTag("agent"))
            {
                Vector3 toNeighbor = hit.transform.position - transform.position;
                neighborVectors.Add(toNeighbor);
            }
        }

        if (neighborVectors.Count == 0)
            return;

        // Take closest numNeighbors
        neighborVectors = neighborVectors.OrderBy(v => v.sqrMagnitude).Take(numNeighbors).ToList();

        // Average vectors
        Vector3 averageDirection = Vector3.zero;
        foreach (var v in neighborVectors)
            averageDirection += v.normalized;

        averageDirection /= neighborVectors.Count;

        // Apply force
        rb.AddForce(averageDirection * forceMultiplier, ForceMode.Force);
    }
}