using UnityEngine;

public class ExtendedCustomMonoBehaviour2D : MonoBehaviour 
{
	[Header("Base")]
	[SerializeField]
	protected bool didInit;
	[SerializeField]
	protected bool canControl;
	
	protected int id;

	protected Transform myTransform;
	protected GameObject myGO;
	protected Rigidbody2D myBody;

	protected Vector3 tempVEC;
	protected Transform tempTR;

	// main events
	void Start () {
		Init ();
	}

	// main Logic

	/// <summary>
	/// Init main instance (myTransform, myGO, myBody2D), def. in Start.
	/// </summary>
	public virtual void Init() {
		// cache refs to our transform and gameObject
		if (!myTransform) {
			myTransform = transform;
		}
		if (!myGO) {
			myGO = gameObject;
		}
		if (!myBody) {
			myBody = GetComponent<Rigidbody2D> ();
		}

		didInit = true;
	}

	public virtual void SetID( int anID )
	{
		id = anID;
	}
}
