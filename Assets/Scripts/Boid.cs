﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Boid : MonoBehaviour {

    /// <summary>
    /// Rotation speed of the boid
    /// </summary>
    [SerializeField]
    float turnSensitivity = 1;

    /// <summary>
    /// Speed of the boid
    /// </summary>
    [SerializeField]
    float speed = 10;

    [Header("Cohesion")]

    /// <summary>
    /// How much the boid should move toward the center
    /// </summary>
    [SerializeField]
    float cohesionValue = 1;

    [Header("Avoidance")]

    /// <summary>
    /// Minimum distance the boids are allowed to be from each other (this distance isn't guaranted, they will work to keep this distance)
    /// </summary>
    [SerializeField]
    float minDistance = 1.5f;

    /// <summary>
    /// How much the boid will avoid its neighbours
    /// </summary>
    [SerializeField]
    float avoidanceValue = 1;

    [Header("Obstacle Avoidance")]

    /// <summary>
    /// How much should this boid tend avoid obstacles
    /// </summary>
    [SerializeField]
    float obstacleAvoidanceValue = 5;

    [Header("Alignement")]

    /// <summary>
    /// How much should this boid tend to match neighbours velocity
    /// </summary>
    [SerializeField]
    float alignementValue = 1;

    [Header("Target")]

    /// <summary>
    /// How much should this boid tend to reach the target
    /// </summary>
    [SerializeField]
    float targetValue = 1;

    /// <summary>
    /// Minimum distance from the target at which the boid take it into account, to avoid them getting all stuck in a point
    /// </summary>
    [SerializeField]
    float distanceToTarget = 2;

    /// <summary>
    /// Neighbours boids
    /// </summary>
    List<Transform> flock = new List<Transform>();

    /// <summary>
    /// Neighbours boids
    /// </summary>
    List<Transform> obstacles = new List<Transform>();

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

    /// <summary>
    /// Called every frame, move the boid according to the rules
    /// </summary>
    /// <param name="target"></param>
    void Move(Vector3 target = default(Vector3))
    {
        //Apply the rules to determinate a new direction
        Vector3 newDirection = Rule_Alignement() + Rule_Cohesion() + Rule_Separation()+ Rule_Obstacle() + Rule_Target(target);

        //Smooth this direction
        newDirection = Vector3.Slerp(this.transform.forward, newDirection, Time.deltaTime * turnSensitivity);
        newDirection.Normalize();

        //Rotate the boid (In case the direction is 0, it means rules couldn't be applied, in this case keep going forward)
        if (newDirection != Vector3.zero)
            transform.forward = newDirection;

        //Move the boid forward
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

        return displacement.normalized*avoidanceValue;
    }

    /// <summary>
    /// Same as separation but applied to the obstacle list
    /// </summary>
    /// <returns></returns>
    Vector3 Rule_Obstacle()
    {
        Vector3 displacementObstacle = Vector3.zero;

        foreach (Transform t in obstacles)
        {
            displacementObstacle += (transform.position - t.position);
        }

        return displacementObstacle.normalized * obstacleAvoidanceValue;
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

    /// <summary>
    /// Tend to move toward a target
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Add the object hit to the flock if it has the same tag, otherwise adds it to the obstacle list
    /// </summary>
    /// <param name="other">Collider hit</param>
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag==gameObject.tag)
        {
            AddBoidToFlock(other.transform);
        }
        else
        {
            obstacles.Add(other.transform);
        }
    }

    /// <summary>
    /// Remove the object hit from the list he is in
    /// </summary>
    /// <param name="other">Collider hit</param>
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == gameObject.tag)
        {
            RemoveBoidFromFlock(other.transform);
        }
        else
        {
            obstacles.Remove(other.transform);
        }
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
