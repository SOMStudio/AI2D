using UnityEngine;

public class SimpleAIMover2D : MonoBehaviour
{
	[SerializeField]
	protected BaseAIController2D AIController;

	[SerializeField]
	protected float moveSpeed= 0.5f;
	[SerializeField]
	protected float chaseSpeed= 0.6f;

	[SerializeField]
	protected Vector3 moveDirection;

	[SerializeField]
	protected Transform myTransform;

	// main event
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
			moveDirection = new Vector3 (AIController.GetHorizontal(), AIController.GetVertical(), 0).normalized;
		}
	}
	
	void Update () 
	{
		if (AIController) {
			moveDirection = new Vector3 (AIController.GetHorizontal(), AIController.GetVertical(), 0).normalized;
		}
		if(moveDirection != Vector3.zero) {
			myTransform.position = Vector3.Lerp (myTransform.position, myTransform.position + moveDirection, Time.deltaTime * GetSpeed ());
		}
	}

	// main logic
	public float GetSpeed() {

		if (AIController) {
			if (AIController.GetAIState() == AIStates.AIState.chasing_target) {
				return chaseSpeed;
			} else {
				return moveSpeed;
			}
		} else {
			return moveSpeed;
		}
	}
}
