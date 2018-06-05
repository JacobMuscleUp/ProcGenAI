using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempObjManager : MonoBehaviour
{
    public static TempObjManager Instance { get; private set; }
    public List<ITempObj> tempObjs = new List<ITempObj>();

    void Awake()
    {
        Instance = this;
    }
}
