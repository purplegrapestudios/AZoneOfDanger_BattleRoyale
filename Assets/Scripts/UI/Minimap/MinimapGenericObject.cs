using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapGenericObject : MonoBehaviour
{
    public void Init()
    {
        Minimap.Instance.RegisterStormCircleObject(this);
    }
}
