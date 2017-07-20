using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Boid : MonoBehaviour {

    [SerializeField]
    float turnSensitivity;

    [SerializeField]
    float speed;

    [Header("Cohesion")]

    /// <summary>
    /// How much the boid should move toward the center
    /// </summary>
    [SerializeField]
    float cohesionValue;

    [Header("Avoidance")]

    /// <summary>
    /// Minimum distance the boids are allowed to be from each other (this distance isn't guaranted, they will work to keep this distance)
    /// </summary>
    [SerializeField]
    float minDistance;

    /// <summary>
    /// How much the boid will avoid its neighbours
    /// </summary>
    [SerializeField]
    float avoidanceValue;

    [Header("Alignement")]

    /// <summary>
    /// How much should this boid tend to match neighbours velocity
    /// </summary>
    [SerializeField]
    float alignementValue;

    [Header("Target")]

    [SerializeField]
    float targetValue;

    [SerializeField]
    float distanceToTarget;

    /// <summary>
    /// Neighbours boids
    /// </summary>
    List<Transform> flock = new List<Transform>();

    #region Components
    new Rigidbody rigidbody;

    #endregion


    // Use this for initialization
    void Awake () {
        rigidbody = GetComponent<Rigidbody>();
    }
	
	// Update is called once per frame
	void Update () {
        Move(FlockingSettings.target.position);	
	}

    void Move(Vector3 target = default(Vector3))
    {
        Vector3 newDirection = Rule_Alignement() + Rule_Cohesion() + Rule_Separation() + Rule_Target(target);
        newDirection = Vector3.Slerp(this.transform.forward, newDirection, Time.deltaTime * turnSensitivity);
        newDirection.Normalize();

        if (newDirection != Vector3.zero)
            transform.forward = newDirection;

        rigidbody.velocity = newDirection * speed;
    }

    /// <summary>
    /// Separation: steer to avoid crowding local flockmates
    /// </summary>
    Vector3 Rule_Separation()
    {
        Vector3 displacement = Vector3.zero;

        foreach (Transform t in flock)
        {
            if(Vector3.Distance(transform.position,t.position)<minDistance)
            {
                displacement += transform.position - t.position;
            }
        }

        return displacement.normalized * avoidanceValue;
    }

    /// <summary>
    /// Alignment: steer towards the average heading of local flockmates
    /// </summary>
    Vector3 Rule_Alignement()
    {
        Vector3 localAlignement = Vector3.zero;

        foreach (Transform t in flock)
        {
            localAlignement += t.forward;
        }

        return localAlignement.normalized * alignementValue;
    }

    /// <summary>
    /// Cohesion: steer to move toward the average position of local flockmates
    /// </summary>
    Vector3 Rule_Cohesion()
    {
        Vector3 centerOfMass = Vector3.zero;

        foreach(Transform t in flock)
        {
            centerOfMass += t.position; 
        }

        centerOfMass /= flock.Count;

        return (centerOfMass - transform.position).normalized * cohesionValue;
    }

    Vector3 Rule_Target(Vector3 target)
    {
        if (target == Vector3.zero)
            return Vector3.zero;

        Vector3 toTarget = target - transform.position;

        if (toTarget.magnitude <= distanceToTarget)
            return Vector3.zero;

        toTarget.Normalize();
        return toTarget * targetValue;
    }


    private void OnTriggerEnter(Collider other)
    {
        AddBoidToFlock(other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        RemoveBoidFromFlock(other.transform);
    }

    /// <summary>
    /// Add a boid to the flock
    /// </summary>
    /// <param name="boid">Boid to add</param>
    void AddBoidToFlock(Transform boid)
    {
        flock.Add(boid);
    }

    /// <summary>
    /// Remove a boid from the flock
    /// </summary>
    /// <param name="boid">Boid to remove</param>
    void RemoveBoidFromFlock(Transform boid)
    {
        flock.Remove(boid);
    }
}
