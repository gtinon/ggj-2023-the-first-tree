using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Tree : MonoBehaviour
{
    public TreeConfig config;

    public TreeSegment overground;
    public TreeSegment underground;

    public float water;
    public float energy;
    public float minerals;

    void Start()
    {
        overground.Init(null);
    }
    
    void Update()
    {
        
    }
}