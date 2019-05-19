using UnityEngine;

[AddComponentMenu("Base/Input Controller")]

public class BaseInputController : MonoBehaviour {

	[Header("Fire")]
	[SerializeField]
	protected bool Fire1;

	[Header("Weapon Slot")]
	[SerializeField]
	protected bool Slot1;
	[SerializeField]
	protected bool Slot2;
	[SerializeField]
	protected bool Slot3;
	[SerializeField]
	protected bool Slot4;
	[SerializeField]
	protected bool Slot5;
	[SerializeField]
	protected bool Slot6;
	[SerializeField]
	protected bool Slot7;
	[SerializeField]
	protected bool Slot8;
	[SerializeField]
	protected bool Slot9;

	[Header("Shift dir")]
	[SerializeField]
	protected float vert;
	[SerializeField]
	protected float horz;
	[SerializeField]
	protected bool shouldRespawn;

	// main logic
	protected virtual void CheckInput ()
	{	
		// override with your own code to deal with input
		horz = Input.GetAxis ("Horizontal");
		vert = Input.GetAxis ("Vertical");
	}
	
	public virtual float GetHorizontal()
	{
		// returns our cached horizontal input axis value
		return horz;
	}
	
	public virtual float GetVertical()
	{
		// returns our cached vertical input axis value
		return vert;
	}

	public bool Up {get { return vert>0; }}
	public bool Down {get { return vert<0; }}
	public bool Right {get { return horz>0; }}
	public bool Left {get { return horz<0; }}

	public virtual bool GetFire()
	{
		return Fire1;
	}
	
	public bool GetRespawn()
	{
		return shouldRespawn;	
	}
	
	public virtual Vector3 GetMovementDirectionVector()
	{
		var res = Vector3.zero;
		
		res.x = horz;
		res.y = vert;

		// return the movement vector
		return res;
	}

}
