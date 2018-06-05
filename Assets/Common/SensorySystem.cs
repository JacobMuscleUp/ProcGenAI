using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SensorySystem 
{

	//TODO: Highly inefficient, needs optimisation
	public static List<GameObject> Sight(float viewDistance, Vector3 eyePosition, Vector3 viewDirection, float fieldOfVision, string tag = "")
	{
		List<GameObject> visionList = new List<GameObject>();
		GameObject[] objectList;
		if (tag == "")
		{
			objectList = UnityEngine.Object.FindObjectsOfType<GameObject>(); 
		}
		else
		{
			objectList = GameObject.FindGameObjectsWithTag(tag);
		}
		foreach (GameObject gameObj in objectList)
		{
			//Check if the object is out of range of vision
			Vector3 velocity =  gameObj.transform.position - eyePosition;
			if (velocity.magnitude > viewDistance)
			{
				continue;
			}

			//Check if the object is within the gameobject's field of vision
			if (Vector3.Angle(velocity, viewDirection) > fieldOfVision)
			{
				continue;
			}

			//Check if the agent has line of sight to the object
			//TODO ensure that object hit is the target
			RaycastHit hit;
			if (Physics.Raycast(eyePosition, velocity, out hit, viewDistance))
			{
				visionList.Add(gameObj);
			}
		}
		return visionList;
	}

    // precondition: _vViewDir must be a unit vector
    public static List<GameObject> Sight2(float _viewDist, Vector3 _vEyePos, Vector3 _vViewDir, float _fov, List<GameObject> _objList)
    {
        List<GameObject> seenObjList = new List<GameObject>();
        var halfFov = _fov * 0.5f;
        
        foreach (GameObject obj in _objList) {
            if (obj != null) {
                var vEye2Obj = obj.transform.position - _vEyePos;
                var vEye2ObjProj = new Vector3(vEye2Obj[0], 0f, vEye2Obj[2]).normalized;

                vEye2ObjProj = Vector3.Dot(vEye2Obj, vEye2ObjProj) * vEye2ObjProj;
                
                if (!(vEye2ObjProj.magnitude > _viewDist) 
                    && !(Vector3.Angle(vEye2ObjProj, _vViewDir) > halfFov)) {
                    RaycastHit hit;
                    if (Physics.Raycast(_vEyePos, vEye2ObjProj, out hit, _viewDist)
                        && hit.collider.gameObject == obj)
                        seenObjList.Add(obj);
                }
            }

        }
        return seenObjList;
    }
}
