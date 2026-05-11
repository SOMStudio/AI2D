using UnityEngine;
using AIStates;

[AddComponentMenu("SOMStudio/AI2D/Base/BaseAIController2D")]
public class BaseAIController2D : ExtendedCustomMonoBehaviour2D
{
	[Header("Result direction Move")]
	[SerializeField] protected float horizontal;
	[SerializeField] protected float vertical;

	[Header("AIState")]
	[SerializeField] protected AIState currentAIState;

	[Header("Layer block see + layer Player")]
	[SerializeField] protected LayerMask layerBlockSee;

	[Header("Settings for Target")]
	[SerializeField] protected Transform followTarget;
	[SerializeField] protected LayerMask layerBlockTarget;
	[SerializeField] protected bool seeTarget;
	[SerializeField] protected float wallAvoidDistance = 1f;
	[SerializeField] protected float minChaseDistance = 0.5f;
	[SerializeField] protected float maxChaseDistance = 3.0f;
	
	[Header("Settings for Waypoints")]
	[SerializeField] protected WaypointsController2D wayController;
	[SerializeField] protected LayerMask layerBlockWaypoint;
	[SerializeField] protected bool seePoint;
	[SerializeField] protected int currentWaypointNumber;
	[SerializeField] protected float waypointDistance = 5f;
	[SerializeField] protected float pathSmoothing = 2f;
	[SerializeField] protected bool shouldReversePathFollowing;
	[SerializeField] protected bool loopPath;
	[SerializeField] protected bool destroyAtEndOfWaypoints;
	[SerializeField] protected bool startAtFirstWaypoint;
	
	private Vector3 moveVector;
	
	private float distanceToChaseTarget;
	
	private int totalWaypoints;
	private Transform currentWaypointTransform;
	private bool reachedLastWaypoint;

	private void Update()
	{
		if (!canControl)
			return;
		
		UpdateAI();
	}
	
	public void SetAIControl(bool state)
	{
		canControl = state;
	}

	public void SetWallAvoidDistance(float aNum)
	{
		wallAvoidDistance = aNum;
	}

	public void SetWaypointDistance(float aNum)
	{
		waypointDistance = aNum;
	}

	public void SetMinChaseDistance(float aNum)
	{
		minChaseDistance = aNum;
	}

	public void SetMaxChaseDistance(float aNum)
	{
		maxChaseDistance = aNum;
	}

	public void SetPathSmoothing(float aNum)
	{
		pathSmoothing = aNum;
	}

	#region MainLogic
	public virtual void SetAIState(AIState newState)
	{
		currentAIState = newState;
	}

	public AIState GetAIState()
	{
		return currentAIState;
	}

	public virtual void SetChaseTarget(Transform theTransform)
	{
		followTarget = theTransform;
	}

	protected virtual void UpdateAI()
	{
		horizontal = moveVector.x;
		vertical = moveVector.y;

		int obstacleFinderResult = IsObstacleAhead();

		switch (currentAIState)
		{
			case AIState.MovingLookingForTarget:
				if (followTarget != null)
					LookAroundFor(followTarget);
				
				if (obstacleFinderResult == 1)
				{
					SetAIState(AIState.StoppedTurningLeft);
				}

				if (obstacleFinderResult == 2)
				{
					SetAIState(AIState.StoppedTurningRight);
				}

				if (obstacleFinderResult == 3)
				{
					SetAIState(AIState.BackingUpLookingForTarget);
				}

				if (moveVector.magnitude != 1)
				{
					moveVector = Vector3.Lerp(moveVector, moveVector.normalized, Time.deltaTime * pathSmoothing);
				}

				if (wayController != null)
				{
					seePoint = CanSeePoint(currentWaypointTransform);
					if (seePoint)
					{
						SetAIState(AIState.TranslateAlongWaypointPath);
					}
					else
					{
						MoveForward();
					}
				}
				else
				{
					MoveForward();
				}
				break;
			case AIState.ChasingTarget:
				if (followTarget == null) SetAIState(AIState.MovingLookingForTarget);
				
				TurnTowardTarget(followTarget);
				
				distanceToChaseTarget = Vector3.Distance(myTransform.position, followTarget.position);
				
				if (distanceToChaseTarget > minChaseDistance)
				{
					MoveForward();
				}
				
				seeTarget = CanSee(followTarget);
				if (distanceToChaseTarget > maxChaseDistance || seeTarget == false)
				{
					if (wayController != null)
					{
						seePoint = CanSeePoint(currentWaypointTransform);
						if (seePoint)
						{
							SetAIState(AIState.TranslateAlongWaypointPath);
						}
						else
						{
							SetAIState(AIState.MovingLookingForTarget);
						}
					}
					else
					{
						SetAIState(AIState.MovingLookingForTarget);
					}
				}
				break;
			case AIState.BackingUpLookingForTarget:
				if (followTarget != null) LookAroundFor(followTarget);
				
				MoveBack();

				if (obstacleFinderResult < 3)
				{
					if (Random.Range(0, 100) > 50)
					{
						SetAIState(AIState.StoppedTurningLeft);
					}
					else
					{
						SetAIState(AIState.StoppedTurningRight);
					}
				}
				break;
			case AIState.StoppedTurningLeft:
				if (followTarget != null)
					LookAroundFor(followTarget);
				
				if (moveVector.magnitude > 0.5f)
				{
					moveVector *= (1 - Time.deltaTime);
				}

				TurnLeft();

				if (obstacleFinderResult == 0)
				{
					SetAIState(AIState.MovingLookingForTarget);
				}
				break;
			case AIState.StoppedTurningRight:
				if (followTarget != null)
					LookAroundFor(followTarget);
				
				if (moveVector.magnitude > 0.5f)
				{
					moveVector *= (1 - Time.deltaTime);
				}

				TurnRight();
				
				if (obstacleFinderResult == 0)
				{
					SetAIState(AIState.MovingLookingForTarget);
				}

				break;
			case AIState.PausedLookingForTarget:
				if (followTarget != null)
					LookAroundFor(followTarget);
				break;
			case AIState.TranslateAlongWaypointPath:
				if (followTarget != null)
				{
					LookAroundFor(followTarget);
					if (currentAIState != AIState.TranslateAlongWaypointPath)
					{
						return;
					}
				}
				
				if (currentWaypointTransform != null)
				{
					seePoint = CanSeePoint(currentWaypointTransform);
					if (seePoint)
					{
						SetAIState(AIState.TranslateAlongWaypointPath);
					}
					else
					{
						SetAIState(AIState.MovingLookingForTarget);
					}
				}
				
				if (!didInit && !reachedLastWaypoint)
					return;

				UpdateWaypoints();
				
				if (currentWaypointTransform != null)
				{
					TurnTowardTarget(currentWaypointTransform);
					MoveForward();
				}

				break;
			case AIState.PausedNoTarget:
			default:
				break;
		}
	}

	protected virtual void TurnLeft()
	{
		moveVector = Quaternion.Euler(0, 0, -1 * pathSmoothing) * moveVector;

		horizontal = moveVector.x;
		vertical = moveVector.y;
	}

	protected virtual void TurnRight()
	{
		moveVector = Quaternion.Euler(0, 0, 1 * pathSmoothing) * moveVector;

		horizontal = moveVector.x;
		vertical = moveVector.y;
	}

	protected virtual void MoveForward()
	{
		horizontal = moveVector.x;
		vertical = moveVector.y;
	}

	protected virtual void MoveBack()
	{
		horizontal = -moveVector.x;
		vertical = -moveVector.y;
	}

	protected virtual void NoMove()
	{
		vertical = 0;
	}

	public virtual void LookAroundFor(Transform aTransform)
	{
		if (Vector3.Distance(myTransform.position, aTransform.position) < maxChaseDistance)
		{
			seeTarget = CanSee(followTarget);
			if (seeTarget)
			{
				SetAIState(AIState.ChasingTarget);
			}
		}
	}

	protected virtual int IsObstacleAhead()
	{
		int obstacleHitType = 0;
		
		if (myTransform == null)
		{
			return 0;
		}
		
		Vector3 left45Dir = (Quaternion.Euler(0, 0, 45) * moveVector);
		Vector3 right45Dir = (Quaternion.Euler(0, 0, -45) * moveVector);
		Debug.DrawRay(myTransform.position + left45Dir.normalized * minChaseDistance, left45Dir * wallAvoidDistance);
		Debug.DrawRay(myTransform.position + right45Dir.normalized * minChaseDistance, right45Dir * wallAvoidDistance);
		
		Debug.DrawRay(myTransform.position + moveVector.normalized * minChaseDistance,
			moveVector.normalized * maxChaseDistance);
		
		RaycastHit2D hitLeft = Physics2D.Raycast(myTransform.position + left45Dir.normalized * minChaseDistance,
			left45Dir, wallAvoidDistance, layerBlockTarget);
		if (hitLeft.transform != null)
		{
			if (hitLeft.transform.gameObject != myGameObject)
			{
				obstacleHitType = 1;
			}
		}

		RaycastHit2D hitRight = Physics2D.Raycast(myTransform.position + right45Dir.normalized * minChaseDistance,
			right45Dir, wallAvoidDistance, layerBlockTarget);
		if (hitRight.transform != null)
		{
			if (hitRight.transform.gameObject != myGameObject)
			{
				if (obstacleHitType == 0)
				{
					obstacleHitType = 2;
				}
				else if (obstacleHitType == 1)
				{
					obstacleHitType = 3;
				}
			}
		}

		return obstacleHitType;
	}

	private void TurnTowardTarget(Transform aTarget)
	{
		if (aTarget == null)
			return;

		Vector3 tempMoveVector = Vector3.Normalize(aTarget.position - myTransform.position);
		if (moveVector == Vector3.zero)
		{
			moveVector = tempMoveVector;
		}
		else
		{
			moveVector = Vector3.Lerp(moveVector, tempMoveVector, Time.deltaTime * pathSmoothing);
		}
	}

	private bool CanSee(Transform aTarget)
	{
		Vector3 tempMoveVector = Vector3.Normalize(aTarget.position - myTransform.position);
		
		RaycastHit2D hit = Physics2D.Raycast(myTransform.position + minChaseDistance * tempMoveVector, tempMoveVector,
			maxChaseDistance, layerBlockSee);
		if (hit.transform != null)
		{
			if (hit.transform.gameObject == aTarget.gameObject)
			{
				Debug.DrawLine(myTransform.position, aTarget.position);

				return true;
			}
		}
		
		return false;
	}

	private bool CanSeePoint(Transform aTarget)
	{
		Vector3 tempVector = aTarget.position - myTransform.position;
		float magTempVec = tempVector.magnitude;
		Vector3 tempMoveVector = Vector3.Normalize(tempVector);
		
		RaycastHit2D hit = Physics2D.Raycast(myTransform.position + minChaseDistance * tempMoveVector, tempMoveVector,
			magTempVec, layerBlockWaypoint);
		if (hit.transform == null)
		{
			return true;
		}
		
		return false;
	}

	public void SetWayController(WaypointsController2D aControl)
	{
		wayController = aControl;
		
		totalWaypoints = wayController.GetTotal();
		
		if (shouldReversePathFollowing)
		{
			currentWaypointNumber = totalWaypoints - 1;
		}
		else
		{
			currentWaypointNumber = 0;
		}

		Init();
		
		currentWaypointTransform = wayController.GetWaypoint(currentWaypointNumber);

		if (startAtFirstWaypoint)
		{
			myTransform.position = currentWaypointTransform.position;
		}
	}

	public void SetReversePath(bool shouldRev)
	{
		shouldReversePathFollowing = shouldRev;
	}

	public void SetPathSmoothingRate(float aRate)
	{
		pathSmoothing = aRate;
	}

	private void UpdateWaypoints()
	{
		if (wayController == null)
			return;

		if (reachedLastWaypoint && destroyAtEndOfWaypoints)
		{
			Destroy(gameObject);
			return;
		}
		else if (reachedLastWaypoint)
		{
			currentWaypointNumber = 0;
			reachedLastWaypoint = false;
		}
		
		if (totalWaypoints == 0)
		{
			totalWaypoints = wayController.GetTotal();
		}

		if (currentWaypointTransform == null)
		{
			currentWaypointTransform = wayController.GetWaypoint(currentWaypointNumber);
			return;
		}

		Vector3 myPositionWithoutZ = myTransform.position;
		myPositionWithoutZ.z = 0;
		
		Vector3 currentWaypointPosition = currentWaypointTransform.position;
		currentWaypointPosition.z = 0;
		
		float currentWayDistance = Vector3.Distance(currentWaypointPosition, myPositionWithoutZ);

		if (currentWayDistance < waypointDistance)
		{
			if (shouldReversePathFollowing)
			{
				currentWaypointNumber--;
				
				if (currentWaypointNumber < 0)
				{
					currentWaypointNumber = 0;
					reachedLastWaypoint = true;
					
					if (loopPath)
					{
						currentWaypointNumber = totalWaypoints - 1;
						
						currentWaypointTransform = wayController.GetWaypoint(currentWaypointNumber);
						
						reachedLastWaypoint = false;
					}
					
					return;
				}
			}
			else
			{
				currentWaypointNumber++;
				
				if (currentWaypointNumber >= totalWaypoints)
				{
					reachedLastWaypoint = true;
					
					if (loopPath)
					{
						currentWaypointNumber = 0;
						
						currentWaypointTransform = wayController.GetWaypoint(currentWaypointNumber);
						
						reachedLastWaypoint = false;
					}
					
					return;
				}
			}
			
			currentWaypointTransform = wayController.GetWaypoint(currentWaypointNumber);
		}
	}

	public float GetHorizontal()
	{
		return horizontal;
	}

	public float GetVertical()
	{
		return vertical;
	}
	#endregion
}
