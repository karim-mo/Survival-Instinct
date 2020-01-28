using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentFix : MonoBehaviour
{
    
    void Update()
    {
        if (transform.parent != null)
        {
            Vector3 local = transform.localScale;
            local.x = transform.parent.localScale.x < 0 ? -1 * local.x : local.x;
            transform.localScale = local;
        }
    }
}
