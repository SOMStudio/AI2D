using UnityEngine;

public class BaseInputController : MonoBehaviour
{
	[Header("Fire")]
	[SerializeField] protected bool fire1;
	
	[Header("Weapon Slot")]
	[SerializeField] protected bool slot1;
	[SerializeField] protected bool slot2;
	[SerializeField] protected bool slot3;
	[SerializeField] protected bool slot4;
	[SerializeField] protected bool slot5;
	[SerializeField] protected bool slot6;
	[SerializeField] protected bool slot7;
	[SerializeField] protected bool slot8;
	[SerializeField] protected bool slot9;
	
	[Header("Shift direction")]
	[SerializeField] protected float vertical;
	[SerializeField] protected float horizontal;
	[SerializeField] protected bool shouldRespawn;
	
	protected virtual void CheckInput()
	{
		horizontal = Input.GetAxis("Horizontal");
		vertical = Input.GetAxis("Vertical");
	}

	public virtual float GetHorizontal()
	{
		return horizontal;
	}

	public virtual float GetVertical()
	{
		return vertical;
	}

	public bool Up => vertical > 0;

	public bool Down => vertical < 0;

	public bool Right => horizontal > 0;

	public bool Left => horizontal < 0;

	public virtual bool GetFire()
	{
		return fire1;
	}

	public bool GetRespawn()
	{
		return shouldRespawn;
	}

	public virtual Vector3 GetMovementDirectionVector()
	{
		var res = Vector3.zero;

		res.x = horizontal;
		res.y = vertical;
		
		return res;
	}
}
