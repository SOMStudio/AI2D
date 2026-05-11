using UnityEngine;

[AddComponentMenu("SOMStudio/AI2D/SimpleAIController2D")]
public class SimpleAIMover2D : ExtendedCustomMonoBehaviour2D
{
	[Header("Simple Mover 2D")]
	[SerializeField] protected BaseAIController2D aiController;
	[SerializeField] protected float moveSpeed = 0.5f;
	[SerializeField] protected float chaseSpeed = 0.9f;
	[SerializeField] protected Vector3 moveDirection;

	private void Awake()
	{
		if (aiController == null) aiController = transform.GetComponent<BaseAIController2D>();
	}

	protected override void Start()
	{
		base.Start();
		
		if (aiController)
		{
			moveDirection = new Vector3(aiController.GetHorizontal(), aiController.GetVertical(), 0).normalized;
		}
	}

	private void Update()
	{
		if (aiController)
		{
			moveDirection = new Vector3(aiController.GetHorizontal(), aiController.GetVertical(), 0).normalized;
		}

		if (moveDirection != Vector3.zero)
		{
			myTransform.position = Vector3.Lerp(myTransform.position, myTransform.position + moveDirection,
				Time.deltaTime * GetSpeed());
		}
	}
	
	public float GetSpeed()
	{
		if (aiController)
		{
			if (aiController.GetAIState() == AIStates.AIState.ChasingTarget)
			{
				return chaseSpeed;
			}

			return moveSpeed;
		}

		return moveSpeed;
	}
}
