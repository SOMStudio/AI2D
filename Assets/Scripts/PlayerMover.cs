using UnityEngine;

public class PlayerMover : MonoBehaviour {

	[SerializeField]
	private float moveSpeed = 0.5f;
	[SerializeField]
	private Keyboard_Input keyboardInput; 
	[SerializeField]
	private Vector3 moveDirection;
	[SerializeField]
	private Transform myTransform;

	// main event
	void Awake () {
		// cache a ref to our transform
		myTransform= transform;

		if (!keyboardInput) {
			keyboardInput = GetComponent<Keyboard_Input> ();
		}
	}

	void Start () {
		if (keyboardInput) {
			moveDirection = new Vector3 (keyboardInput.GetHorizontal (), keyboardInput.GetVertical (), 0).normalized;
		}
	}

	void Update () {
		if (keyboardInput) {
			moveDirection = new Vector3 (keyboardInput.GetHorizontal (), keyboardInput.GetVertical (), 0).normalized;
		}
		if (moveDirection != Vector3.zero) {
			myTransform.position = Vector3.Lerp (myTransform.position, myTransform.position + moveDirection, Time.deltaTime * moveSpeed);
		}
	}
}
