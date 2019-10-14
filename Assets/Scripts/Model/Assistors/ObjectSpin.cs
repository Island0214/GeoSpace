using UnityEngine;
using System.Collections;

public class ObjectSpin : MonoBehaviour{
	
	public float RotateSpeed = 30f;

	void Update(){
		transform.Rotate(Vector3.up * RotateSpeed * Time.deltaTime);
		if(transform.localEulerAngles.y > 355){
			DestroyGameObject();
		}
	}
	void DestroyGameObject()
    {
        Destroy(gameObject);
    }
}