using UnityEngine;
using System.Collections;

// use this script in conjunction with the BaseAIController to make a bot move around

public class SimpleAIMover2D : MonoBehaviour
{
	public BaseAIController2D AIController;

	public float moveSpeed= 0.5f;
	public float chaseSpeed= 0.6f;

	public Vector3 moveDirection;

	public Transform myTransform;

	float GetSpeed() {

		if (AIController) {
			if (AIController.currentAIState == AIStates.AIState.chasing_target) {
				return chaseSpeed;
			} else {
				return moveSpeed;
			}
		} else {
			return moveSpeed;
		}
	}

	void Awake () {
		// cache a ref to our transform
		myTransform= transform;

		// if it hasn't been set in the editor, let's try and find it on this transform
		if(AIController==null)
			AIController= myTransform.GetComponent<BaseAIController2D>();
	}

	void Start ()
	{
		if (AIController) {
			moveDirection = new Vector3 (AIController.horz, AIController.vert, 0).normalized;
		}
	}
	
	void Update () 
	{
		if (AIController) {
			moveDirection = new Vector3 (AIController.horz, AIController.vert, 0).normalized;
		}
		if(moveDirection != Vector3.zero) {
			myTransform.position = Vector3.Lerp (myTransform.position, myTransform.position + moveDirection, Time.deltaTime * GetSpeed ());
		}
	}
}
