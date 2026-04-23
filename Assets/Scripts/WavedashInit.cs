using UnityEngine;
using System.Collections.Generic;

public class WavedashInit : MonoBehaviour
{
    void Awake()
    {
        Wavedash.SDK.Init(new Dictionary<string, object> { { "debug", true } });
    }
}
