using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempItem : MonoBehaviour, ITempObj
{
    Block attachedBlock;
    public Block AttachedBlock
    {
        get { return attachedBlock; }
        set {
            attachedBlock = value;
            transform.position 
                = attachedBlock.transform.position 
                    + Vector3.up * (attachedBlock.transform.localScale[1] / 2 + transform.localScale[1] / 2);
        }
    }

    void Start()
    {
        TempObjManager.Instance.tempObjs.Add(this);
    }

    void OnDestroy()
    {
        TempObjManager.Instance.tempObjs.Remove(this);
    }
}
