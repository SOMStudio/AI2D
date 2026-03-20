using UnityEngine;
using AIStates;

[AddComponentMenu("Base/AI Controller")]
public class BaseAIController2D : ExtendedCustomMonoBehaviour2D
{
	private Vector3 tempDirVec;
	private Vector3 moveVec;

	[Header("Result direction Move")] [SerializeField]
	protected float horz;

	[SerializeField] protected float vert;

	[Header("AIState")] [SerializeField] protected AIState currentAIState;
	private int obstacleFinderResult;

	[Header("Layer block see + layer Player")] [SerializeField]
	protected LayerMask layerBlockSee;

	[Header("Settings for Target")] [SerializeField]
	protected Transform followTarget;

	[SerializeField] protected LayerMask layerBlockTargey;
	[SerializeField] protected bool seeTarget;
	[SerializeField] protected float wallAvoidDistance = 1f;
	[SerializeField] protected float minChaseDistance = 0.5f;
	[SerializeField] protected float maxChaseDistance = 3.0f;
	
	private float distanceToChaseTarget;

	[Header("Settings for Waypoints")] [SerializeField]
	protected Waypoints_Controller myWayControl;

	[SerializeField] protected LayerMask layerBlockWaypoint;
	[SerializeField] protected bool seePoint;
	[SerializeField] protected int currentWaypointNum;
	[SerializeField] protected float waypointDistance = 5f;
	[SerializeField] protected float pathSmoothing = 2f;
	[SerializeField] protected bool shouldReversePathFollowing;
	[SerializeField] protected bool loopPath;
	[SerializeField] protected bool destroyAtEndOfWaypoints;
	[SerializeField] protected bool startAtFirstWaypoint;
	
	private int totalWaypoints;
	private Transform currentWaypointTransform;
	private Vector3 nodePosition;
	private Vector3 myPosition;
	private float currentWayDist;
	private bool reachedLastWaypoint;

	private int obstacleFinding;

	private void Update()
	{
		if (!didInit)
			Init();
		
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
		horz = moveVec.x;
		vert = moveVec.y;

		int obstacleFinderResult = IsObstacleAhead();

		switch (currentAIState)
		{
			case AIState.moving_looking_for_target:
				if (followTarget != null)
					LookAroundFor(followTarget);
				
				if (obstacleFinderResult == 1)
				{
					SetAIState(AIState.stopped_turning_left);
				}

				if (obstacleFinderResult == 2)
				{
					SetAIState(AIState.stopped_turning_right);
				}

				if (obstacleFinderResult == 3)
				{
					SetAIState(AIState.backing_up_looking_for_target);
				}

				if (moveVec.magnitude != 1)
				{
					moveVec = Vector3.Lerp(moveVec, moveVec.normalized, Time.deltaTime * pathSmoothing);
				}

				if (myWayControl != null)
				{
					seePoint = CanSeePoint(currentWaypointTransform);
					if (seePoint == true)
					{
						SetAIState(AIState.translate_along_waypoint_path);
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
			case AIState.chasing_target:
				if (followTarget == null) SetAIState(AIState.moving_looking_for_target);
				
				TurnTowardTarget(followTarget);
				
				distanceToChaseTarget = Vector3.Distance(myTransform.position, followTarget.position);
				
				if (distanceToChaseTarget > minChaseDistance)
				{
					MoveForward();
				}
				
				seeTarget = CanSee(followTarget);
				if (distanceToChaseTarget > maxChaseDistance || seeTarget == false)
				{
					if (myWayControl != null)
					{
						seePoint = CanSeePoint(currentWaypointTransform);
						if (seePoint)
						{
							SetAIState(AIState.translate_along_waypoint_path);
						}
						else
						{
							SetAIState(AIState.moving_looking_for_target);
						}
					}
					else
					{
						SetAIState(AIState.moving_looking_for_target);
					}
				}

				break;

			case AIState.backing_up_looking_for_target:
				if (followTarget != null) LookAroundFor(followTarget);
				
				MoveBack();

				if (obstacleFinderResult < 3)
				{
					if (Random.Range(0, 100) > 50)
					{
						SetAIState(AIState.stopped_turning_left);
					}
					else
					{
						SetAIState(AIState.stopped_turning_right);
					}
				}

				break;
			case AIState.stopped_turning_left:
				if (followTarget != null)
					LookAroundFor(followTarget);
				
				if (moveVec.magnitude > 0.5f)
				{
					moveVec *= (1 - Time.deltaTime);
				}

				TurnLeft();

				if (obstacleFinderResult == 0)
				{
					SetAIState(AIState.moving_looking_for_target);
				}

				break;

			case AIState.stopped_turning_right:
				if (followTarget != null)
					LookAroundFor(followTarget);
				
				if (moveVec.magnitude > 0.5f)
				{
					moveVec *= (1 - Time.deltaTime);
				}

				TurnRight();
				
				if (obstacleFinderResult == 0)
				{
					SetAIState(AIState.moving_looking_for_target);
				}

				break;
			case AIState.paused_looking_for_target:
				if (followTarget != null)
					LookAroundFor(followTarget);
				break;

			case AIState.translate_along_waypoint_path:
				if (followTarget != null)
				{
					LookAroundFor(followTarget);
					if (currentAIState != AIState.translate_along_waypoint_path)
					{
						return;
					}
				}
				
				if (currentWaypointTransform != null)
				{
					seePoint = CanSeePoint(currentWaypointTransform);
					if (seePoint)
					{
						SetAIState(AIState.translate_along_waypoint_path);
					}
					else
					{
						SetAIState(AIState.moving_looking_for_target);
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

			case AIState.paused_no_target:
				break;

			default:
				break;
		}
	}

	protected virtual void TurnLeft()
	{
		moveVec = (Quaternion.Euler(0, 0, -1 * pathSmoothing) * moveVec);

		horz = moveVec.x;
		vert = moveVec.y;
	}

	protected virtual void TurnRight()
	{
		moveVec = (Quaternion.Euler(0, 0, 1 * pathSmoothing) * moveVec);

		horz = moveVec.x;
		vert = moveVec.y;
	}

	protected virtual void MoveForward()
	{
		horz = moveVec.x;
		vert = moveVec.y;
	}

	protected virtual void MoveBack()
	{
		horz = -moveVec.x;
		vert = -moveVec.y;
	}

	protected virtual void NoMove()
	{
		vert = 0;
	}

	public virtual void LookAroundFor(Transform aTransform)
	{
		if (Vector3.Distance(myTransform.position, aTransform.position) < maxChaseDistance)
		{
			seeTarget = CanSee(followTarget);
			if (seeTarget == true)
			{
				SetAIState(AIState.chasing_target);
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
		
		Vector3 left45Dir = (Quaternion.Euler(0, 0, 45) * moveVec);
		Vector3 right45Dir = (Quaternion.Euler(0, 0, -45) * moveVec);
		Debug.DrawRay(myTransform.position + left45Dir.normalized * minChaseDistance, left45Dir * wallAvoidDistance);
		Debug.DrawRay(myTransform.position + right45Dir.normalized * minChaseDistance, right45Dir * wallAvoidDistance);
		
		Debug.DrawRay(myTransform.position + moveVec.normalized * minChaseDistance,
			moveVec.normalized * maxChaseDistance);
		
		RaycastHit2D hitLeft = Physics2D.Raycast(myTransform.position + left45Dir.normalized * minChaseDistance,
			left45Dir, wallAvoidDistance, layerBlockTargey);
		if (hitLeft.transform != null)
		{
			if (hitLeft.transform.gameObject != myGO)
			{
				obstacleHitType = 1;
			}
		}

		RaycastHit2D hitRight = Physics2D.Raycast(myTransform.position + right45Dir.normalized * minChaseDistance,
			right45Dir, wallAvoidDistance, layerBlockTargey);
		if (hitRight.transform != null)
		{
			if (hitRight.transform.gameObject != myGO)
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

		tempDirVec = Vector3.Normalize(aTarget.position - myTransform.position);
		if (moveVec == Vector3.zero)
		{
			moveVec = tempDirVec;
		}
		else
		{
			moveVec = Vector3.Lerp(moveVec, tempDirVec, Time.deltaTime * pathSmoothing);
		}
	}

	private bool CanSee(Transform aTarget)
	{
		tempDirVec = Vector3.Normalize(aTarget.position - myTransform.position);
		
		RaycastHit2D hit = Physics2D.Raycast(myTransform.position + (minChaseDistance * tempDirVec), tempDirVec,
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
		tempDirVec = Vector3.Normalize(tempVector);
		
		RaycastHit2D hit = Physics2D.Raycast(myTransform.position + (minChaseDistance * tempDirVec), tempDirVec,
			magTempVec, layerBlockWaypoint);
		if (hit.transform == null)
		{
			return true;
		}
		
		return false;
	}

	public void SetWayController(Waypoints_Controller aControl)
	{
		myWayControl = aControl;
		
		totalWaypoints = myWayControl.GetTotal();
		
		if (shouldReversePathFollowing)
		{
			currentWaypointNum = totalWaypoints - 1;
		}
		else
		{
			currentWaypointNum = 0;
		}

		Init();
		
		currentWaypointTransform = myWayControl.GetWaypoint(currentWaypointNum);

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
		if (myWayControl == null)
			return;

		if (reachedLastWaypoint && destroyAtEndOfWaypoints)
		{
			Destroy(gameObject);
			return;
		}
		else if (reachedLastWaypoint)
		{
			currentWaypointNum = 0;
			reachedLastWaypoint = false;
		}
		
		if (totalWaypoints == 0)
		{
			totalWaypoints = myWayControl.GetTotal();
		}

		if (currentWaypointTransform == null)
		{
			currentWaypointTransform = myWayControl.GetWaypoint(currentWaypointNum);
			return;
		}

		myPosition = myTransform.position;
		myPosition.z = 0;
		
		nodePosition = currentWaypointTransform.position;
		nodePosition.z = 0;
		
		currentWayDist = Vector3.Distance(nodePosition, myPosition);

		if (currentWayDist < waypointDistance)
		{
			if (shouldReversePathFollowing)
			{
				currentWaypointNum--;
				if (currentWaypointNum < 0)
				{
					currentWaypointNum = 0;
					reachedLastWaypoint = true;
					
					if (loopPath)
					{
						currentWaypointNum = totalWaypoints - 1;
						
						currentWaypointTransform = myWayControl.GetWaypoint(currentWaypointNum);
						
						reachedLastWaypoint = false;
					}
					
					return;
				}
			}
			else
			{
				currentWaypointNum++;
				
				if (currentWaypointNum >= totalWaypoints)
				{
					reachedLastWaypoint = true;
					
					if (loopPath)
					{
						currentWaypointNum = 0;
						
						currentWaypointTransform = myWayControl.GetWaypoint(currentWaypointNum);
						
						reachedLastWaypoint = false;
					}
					
					return;
				}
			}
			
			currentWaypointTransform = myWayControl.GetWaypoint(currentWaypointNum);
		}
	}

	public float GetHorizontal()
	{
		return horz;
	}

	public float GetVertical()
	{
		return vert;
	}
	#endregion
}
