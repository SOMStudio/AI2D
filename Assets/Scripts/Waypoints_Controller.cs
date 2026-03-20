using UnityEngine;
using System.Collections;

[AddComponentMenu("Utility/Waypoints Controller 2D")]
public class Waypoints_Controller : MonoBehaviour
{
	[ExecuteInEditMode] [SerializeField] private float radiusGismo = 1;
	
	private ArrayList transforms;
	private Vector3 firstPoint;
	private float distance;
	private Transform TEMPtrans;
	private int TEMPindex;
	private int totalTransforms;

	private Vector3 diff;
	private float curDistance;
	private Transform closest;

	private Vector3 currentPos;
	private Vector3 lastPos;
	private Transform pointT;

	[SerializeField] protected bool closed = true;
	[SerializeField] protected bool shouldReverse;

	private void Start()
	{
		GetTransforms();
	}

	private void OnDrawGizmos()
	{
		if (Application.isPlaying)
			return;

		GetTransforms();
		
		if (totalTransforms < 2)
			return;
		
		TEMPtrans = (Transform)transforms[0];
		lastPos = TEMPtrans.position;
		
		pointT = (Transform)transforms[0];
		
		firstPoint = lastPos;
		
		for (int i = 1; i < totalTransforms; i++)
		{
			TEMPtrans = (Transform)transforms[i];
			if (TEMPtrans == null)
			{
				GetTransforms();
				return;
			}
			
			currentPos = TEMPtrans.position;

			Gizmos.color = Color.green;
			Gizmos.DrawSphere(currentPos, radiusGismo);
			
			Gizmos.color = Color.red;
			Gizmos.DrawLine(lastPos, currentPos);
			
			pointT.LookAt(currentPos);
			
			lastPos = currentPos;
			
			pointT = (Transform)transforms[i];
		}
		
		if (closed)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(currentPos, firstPoint);
		}
	}

	public void GetTransforms()
	{
		transforms = new ArrayList();
		
		foreach (Transform t in transform)
		{
			transforms.Add(t);
		}

		totalTransforms = transforms.Count;
	}

	public void SetReverseMode(bool rev)
	{
		shouldReverse = rev;
	}

	public int FindNearestWaypoint(Vector3 fromPos, float maxRange)
	{
		if (transforms == null)
			GetTransforms();
		
		distance = Mathf.Infinity;
		
		for (int i = 0; i < transforms.Count; i++)
		{
			TEMPtrans = (Transform)transforms[i];
			
			diff = TEMPtrans.position - fromPos;
			curDistance = diff.sqrMagnitude;
			
			if (curDistance < distance)
			{
				if (Mathf.Abs(TEMPtrans.position.y - fromPos.y) < maxRange)
				{
					
					closest = TEMPtrans;
					
					TEMPindex = i;
					
					distance = curDistance;
				}
			}
		}
		
		if (closest)
		{
			return TEMPindex;
		}
		else
		{
			return -1;
		}
	}

	public int FindNearestWaypoint(Vector3 fromPos, Transform exceptThis, float maxRange)
	{
		if (transforms == null)
			GetTransforms();
		
		distance = Mathf.Infinity;
		
		for (int i = 0; i < totalTransforms; i++)
		{
			TEMPtrans = (Transform)transforms[i];
			
			diff = (TEMPtrans.position - fromPos);
			curDistance = diff.sqrMagnitude;
			
			if (curDistance < distance && TEMPtrans != exceptThis)
			{
				if (Mathf.Abs(TEMPtrans.position.y - fromPos.y) < maxRange)
				{
					closest = TEMPtrans;
					
					TEMPindex = i;
					
					distance = curDistance;
				}
			}
		}
		
		if (closest)
		{
			return TEMPindex;
		}
		else
		{
			return -1;
		}
	}

	public Transform GetWaypoint(int index)
	{
		if (shouldReverse)
		{
			index = (totalTransforms - 1) - index;

			if (index < 0)
				index = 0;
		}
		
		if (transforms == null)
			GetTransforms();
		
		if (index > totalTransforms - 1)
			return null;

		return (Transform)transforms[index];
	}

	public int GetTotal()
	{
		return totalTransforms;
	}
}
