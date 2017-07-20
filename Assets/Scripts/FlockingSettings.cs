using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockingSettings : MonoBehaviour {

    public static Transform target;

    [SerializeField]
    Transform _target;

    /// <summary>
    /// Save the settings in the static variable
    /// </summary>
    private void Awake()
    {
        target = _target;
    }
}
