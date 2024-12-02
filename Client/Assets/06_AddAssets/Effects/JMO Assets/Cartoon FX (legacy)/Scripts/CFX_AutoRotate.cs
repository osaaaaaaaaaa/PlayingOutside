using UnityEngine;
using System.Collections;

// Cartoon FX  - (c) 2015 Jean Moreno

// Indefinitely rotates an object at a constant defaultSpeed

public class CFX_AutoRotate : MonoBehaviour
{
	// Rotation defaultSpeed & axis
	public Vector3 rotation;
	
	// Rotation space
	public Space space = Space.Self;
	
	void Update()
	{
		this.transform.Rotate(rotation * Time.deltaTime, space);
	}
}
