using UnityEngine;

public class ExtendedCustomMonoBehaviour2D : MonoBehaviour
{
	[Header("Base")]
	[SerializeField] protected bool didInit;
	[SerializeField] protected bool canControl;

	protected Transform myTransform;
	protected GameObject myGameObject;
	protected Rigidbody2D myBody;

	protected virtual void Start()
	{
		Init();
	}

	protected void Init()
	{
		myTransform = transform;

		myGameObject = gameObject;

		myBody = GetComponent<Rigidbody2D>();

		didInit = true;
	}
}
