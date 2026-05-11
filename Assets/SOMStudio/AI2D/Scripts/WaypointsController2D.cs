using System.Collections.Generic;
using UnityEngine;

namespace SOMStudio.AI2D.Scripts
{
	[AddComponentMenu("SOMStudio/AI2D/WaypointsController2D")]
	public class WaypointsController2D : MonoBehaviour
	{
		[SerializeField] private float radiusGizmo = 0.3f;
	
		[SerializeField] protected bool closed = true;
		[SerializeField] protected bool shouldReverse;
	
		private List<Transform> transforms;
		private int totalTransforms;

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
		
			for (int i = 1; i < totalTransforms; i++)
			{
				if (transforms[i] == null) return;

				Gizmos.color = Color.green;
				Gizmos.DrawSphere(transforms[i].position, radiusGizmo);
			
				Gizmos.color = Color.red;
				Gizmos.DrawLine(transforms[i - 1].position, transforms[i].position);
			
				transforms[i - 1].LookAt(transforms[i].position);
			}
		
			if (closed)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawLine(transforms[totalTransforms - 1].position, transforms[0].position);
			}
		}

		private void GetTransforms()
		{
			transforms = new List<Transform> { };
			
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

		public int FindNearestWaypoint(Vector3 fromPosition, float maxRange)
		{
			if (transforms == null)
				GetTransforms();

			float closestDistance = Mathf.Infinity;
			Transform closestTransform = null;
			int closestIndex = 0;
		
			for (int i = 0; i < transforms.Count; i++)
			{
				var activeTransform = transforms[i];
			
				Vector3 difference = activeTransform.position - fromPosition;
				float currentDistance = difference.sqrMagnitude;
			
				if (currentDistance < closestDistance)
				{
					if (Mathf.Abs(activeTransform.position.y - fromPosition.y) < maxRange)
					{
						closestTransform =  activeTransform;
					
						closestIndex = i;
					
						closestDistance = currentDistance;
					}
				}
			}
		
			if (closestTransform)
			{
				return closestIndex;
			}

			return -1;
		}

		public int FindNearestWaypoint(Vector3 fromPosition, Transform exceptThis, float maxRange)
		{
			if (transforms == null)
				GetTransforms();

			float distance = Mathf.Infinity;
			Transform closestTransform = null;
			int closestIndex = 0;
		
			for (int i = 0; i < totalTransforms; i++)
			{
				var activeTransform = transforms[i];
			
				Vector3 difference = activeTransform.position - fromPosition;
				float currentDistance = difference.sqrMagnitude;
			
				if (currentDistance < distance && activeTransform != exceptThis)
				{
					if (Mathf.Abs(activeTransform.position.y - fromPosition.y) < maxRange)
					{
						closestTransform = activeTransform;
					
						closestIndex = i;
					
						distance = currentDistance;
					}
				}
			}
		
			if (closestTransform)
			{
				return closestIndex;
			}

			return -1;
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

			return transforms?[index];
		}

		public int GetTotal()
		{
			return totalTransforms;
		}
	}
}
