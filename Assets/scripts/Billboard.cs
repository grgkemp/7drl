using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Billboard: MonoBehaviour
{
	void LateUpdate()
    {
		//align to camera
		transform.rotation = Camera.main.transform.rotation;
    }
}