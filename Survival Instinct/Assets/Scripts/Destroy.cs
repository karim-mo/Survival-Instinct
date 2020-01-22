using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy : MonoBehaviour
{
    public float timer;
    void Start()
    {
        Destroy(this.gameObject, timer);
        
    }
}
