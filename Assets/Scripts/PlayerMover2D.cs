using UnityEngine;

public class PlayerMover2D : ExtendedCustomMonoBehaviour2D
{
	[SerializeField] private float moveSpeed = 0.5f;
	[SerializeField] private KeyboardInput keyboardInput;
	[SerializeField] private Vector3 moveDirection;
	
	private void Awake()
	{
		if (!keyboardInput)
		{
			keyboardInput = GetComponent<KeyboardInput>();
		}
	}

	protected override void Start()
	{
		base.Start();
		
		if (keyboardInput)
		{
			moveDirection = new Vector3(keyboardInput.GetHorizontal(), keyboardInput.GetVertical(), 0).normalized;
		}
	}

	private void Update()
	{
		if (keyboardInput)
		{
			moveDirection = new Vector3(keyboardInput.GetHorizontal(), keyboardInput.GetVertical(), 0).normalized;
		}

		if (moveDirection != Vector3.zero)
		{
			myTransform.position = Vector3.Lerp(myTransform.position, myTransform.position + moveDirection,
				Time.deltaTime * moveSpeed);
		}
	}
}
