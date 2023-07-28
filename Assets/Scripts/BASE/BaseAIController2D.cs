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
		// make sure we have initialized before doing anything
		if (!didInit)
			Init();

		// check to see if we're supposed to be controlling the player
		if (!canControl)
			return;

		// do AI updates
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

	// -----------------------------------------

	public virtual void SetAIState(AIState newState)
	{
		// update AI state
		currentAIState = newState;
	}

	public AIState GetAIState()
	{
		return currentAIState;
	}

	public virtual void SetChaseTarget(Transform theTransform)
	{
		// set a target for this AI to chase, if required
		followTarget = theTransform;
	}

	protected virtual void UpdateAI()
	{
		// reset our inputs
		horz = moveVec.x;
		vert = moveVec.y;

		int obstacleFinderResult = IsObstacleAhead();

		switch (currentAIState)
		{
			// -----------------------------
			case AIState.moving_looking_for_target:
				// look for chase target
				if (followTarget != null)
					LookAroundFor(followTarget);

				// the AvoidWalls function looks to see if there's anything in-front. If there is,
				// it will automatically change the value of moveDirection before we do the actual move
				if (obstacleFinderResult == 1)
				{
					// GO LEFT
					SetAIState(AIState.stopped_turning_left);
				}

				if (obstacleFinderResult == 2)
				{
					// GO RIGHT
					SetAIState(AIState.stopped_turning_right);
				}

				if (obstacleFinderResult == 3)
				{
					// BACK UP
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
						//check if have path trajectory
						SetAIState(AIState.translate_along_waypoint_path);
					}
					else
					{
						// all clear! head forward
						MoveForward();
					}
				}
				else
				{
					// all clear! head forward
					MoveForward();
				}

				break;
			case AIState.chasing_target:
				// chasing
				// in case mode, we point toward the target and go right at it!

				// quick check to make sure that we have a target (if not, we drop back to patrol mode)
				if (followTarget == null)
					SetAIState(AIState.moving_looking_for_target);

				// the TurnTowardTarget function does just that, so to chase we just throw it the current target
				TurnTowardTarget(followTarget);

				// find the distance between us and the chase target to see if it is within range
				distanceToChaseTarget = Vector3.Distance(myTransform.position, followTarget.position);

				// check the range
				if (distanceToChaseTarget > minChaseDistance)
				{
					// keep charging forward
					MoveForward();
				}

				// here we do a quick check to test the distance between AI and target. If it's higher than
				// our maxChaseDistance variable, we drop out of chase mode and go back to patrolling.
				seeTarget = CanSee(followTarget);
				if (distanceToChaseTarget > maxChaseDistance || seeTarget == false)
				{
					if (myWayControl != null)
					{
						seePoint = CanSeePoint(currentWaypointTransform);
						if (seePoint)
						{
							//check if have path trajectory
							SetAIState(AIState.translate_along_waypoint_path);
						}
						else
						{
							// set our state to 1 - moving_looking_for_target
							SetAIState(AIState.moving_looking_for_target);
						}
					}
					else
					{
						// set our state to 1 - moving_looking_for_target
						SetAIState(AIState.moving_looking_for_target);
					}
				}

				break;
			// -----------------------------

			case AIState.backing_up_looking_for_target:

				// look for chase target
				if (followTarget != null)
					LookAroundFor(followTarget);

				// backing up
				MoveBack();

				if (obstacleFinderResult < 3)
				{
					// now we've backed up, lets randomize whether to go left or right
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
				// look for chase target
				if (followTarget != null)
					LookAroundFor(followTarget);

				// stopped, turning left
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
				// look for chase target
				if (followTarget != null)
					LookAroundFor(followTarget);

				// stopped, turning right
				if (moveVec.magnitude > 0.5f)
				{
					moveVec *= (1 - Time.deltaTime);
				}

				TurnRight();

				// check results from looking, to see if path ahead is clear
				if (obstacleFinderResult == 0)
				{
					SetAIState(AIState.moving_looking_for_target);
				}

				break;
			case AIState.paused_looking_for_target:
				// standing still, with looking for chase target
				// look for chase target
				if (followTarget != null)
					LookAroundFor(followTarget);
				break;

			case AIState.translate_along_waypoint_path:
				// following waypoints (moving toward them, not pointing at them) at the speed of

				// look for chase target
				if (followTarget != null)
				{
					LookAroundFor(followTarget);
					if (currentAIState != AIState.translate_along_waypoint_path)
					{
						return;
					}
				}

				// check see path point
				if (currentWaypointTransform != null)
				{
					seePoint = CanSeePoint(currentWaypointTransform);
					if (seePoint)
					{
						//check if have path trajectory
						SetAIState(AIState.translate_along_waypoint_path);
					}
					else
					{
						// set our state to 1 - moving_looking_for_target
						SetAIState(AIState.moving_looking_for_target);
					}
				}

				// make sure we have been initialized before trying to access waypoints
				if (!didInit && !reachedLastWaypoint)
					return;

				UpdateWaypoints();

				// move AI
				if (currentWaypointTransform != null)
				{
					TurnTowardTarget(currentWaypointTransform);
					MoveForward();
				}

				break;

			case AIState.paused_no_target:
				// paused_no_target
				break;

			default:
				// idle (do nothing)
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
		// here we do a quick check to test the distance between AI and target. If it's higher than
		// our maxChaseDistance variable, we drop out of chase mode and go back to patrolling.
		if (Vector3.Distance(myTransform.position, aTransform.position) < maxChaseDistance)
		{
			// check to see if the target is visible before going into chase mode
			seeTarget = CanSee(followTarget);
			if (seeTarget == true)
			{
				// set our state to chase the target
				SetAIState(AIState.chasing_target);
			}
		}
	}

	protected virtual int IsObstacleAhead()
	{
		int obstacleHitType = 0;

		// quick check to make sure that myTransform has been set
		if (myTransform == null)
		{
			return 0;
		}

		// draw this raycast so we can see what it is doing
		Vector3 left45Dir = (Quaternion.Euler(0, 0, 45) * moveVec);
		Vector3 right45Dir = (Quaternion.Euler(0, 0, -45) * moveVec);
		Debug.DrawRay(myTransform.position + left45Dir.normalized * minChaseDistance, left45Dir * wallAvoidDistance);
		Debug.DrawRay(myTransform.position + right45Dir.normalized * minChaseDistance, right45Dir * wallAvoidDistance);

		// lets have a debug line to check the distance where we look
		Debug.DrawRay(myTransform.position + moveVec.normalized * minChaseDistance,
			moveVec.normalized * maxChaseDistance);

		// cast a ray out forward from our AI and put the 'result' into the variable named hit
		RaycastHit2D hitLeft = Physics2D.Raycast(myTransform.position + left45Dir.normalized * minChaseDistance,
			left45Dir, wallAvoidDistance, layerBlockTargey);
		if (hitLeft.transform != null)
		{
			if (hitLeft.transform.gameObject != myGO)
			{
				// obstacle
				// it's a left hit, so it's a type 1 right now (though it could change when we check on the other side)
				obstacleHitType = 1;
			}
		}

		RaycastHit2D hitRight = Physics2D.Raycast(myTransform.position + right45Dir.normalized * minChaseDistance,
			right45Dir, wallAvoidDistance, layerBlockTargey);
		if (hitRight.transform != null)
		{
			if (hitRight.transform.gameObject != myGO)
			{
				// obstacle
				if (obstacleHitType == 0)
				{
					// if we haven't hit anything yet, this is a type 2
					obstacleHitType = 2;
				}
				else if (obstacleHitType == 1)
				{
					// if we have hits on both left and right raycasts, it's a type 3
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
		// first, let's get a vector to use for ray-casting by subtracting the target position from our AI position
		tempDirVec = Vector3.Normalize(aTarget.position - myTransform.position);

		// cast a ray from our AI, out toward the target passed in (use the tempDirVec magnitude as the distance to cast)
		RaycastHit2D hit = Physics2D.Raycast(myTransform.position + (minChaseDistance * tempDirVec), tempDirVec,
			maxChaseDistance, layerBlockSee);
		if (hit.transform != null)
		{
			// check to see if we hit the target
			if (hit.transform.gameObject == aTarget.gameObject)
			{
				//debugin line when we see target
				Debug.DrawLine(myTransform.position, aTarget.position);

				return true;
			}
		}

		// nothing found, so return false
		return false;
	}

	private bool CanSeePoint(Transform aTarget)
	{
		// first, let's get a vector to use for raycasting by subtracting the target position from our AI position
		Vector3 tempVect = aTarget.position - myTransform.position;
		float magTempVec = tempVect.magnitude;
		tempDirVec = Vector3.Normalize(tempVect);

		// cast a ray from our AI, out toward the target passed in (use the tempDirVec magnitude as the distance to cast)
		RaycastHit2D hit = Physics2D.Raycast(myTransform.position + (minChaseDistance * tempDirVec), tempDirVec,
			magTempVec, layerBlockWaypoint);
		if (hit.transform == null)
		{
			return true;
		}

		// nothing found, so return false
		return false;
	}

	public void SetWayController(Waypoints_Controller aControl)
	{
		myWayControl = aControl;

		// grab total waypoints
		totalWaypoints = myWayControl.GetTotal();

		// make sure that if you use SetReversePath to set shouldReversePathFollowing that you
		// call SetReversePath for the first time BEFORE SetWayController, otherwise it won't set the first waypoint correctly

		if (shouldReversePathFollowing)
		{
			currentWaypointNum = totalWaypoints - 1;
		}
		else
		{
			currentWaypointNum = 0;
		}

		Init();

		// get the first waypoint from the waypoint controller
		currentWaypointTransform = myWayControl.GetWaypoint(currentWaypointNum);

		if (startAtFirstWaypoint)
		{
			// position at the currentWaypointTransform position
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
		// If we don't have a waypoint controller, we safely drop out
		if (myWayControl == null)
			return;

		if (reachedLastWaypoint && destroyAtEndOfWaypoints)
		{
			// destroy myself(!)
			Destroy(gameObject);
			return;
		}
		else if (reachedLastWaypoint)
		{
			currentWaypointNum = 0;
			reachedLastWaypoint = false;
		}

		// because of the order that scripts run and are initialised, it is possible for this function
		// to be called before we have actually finished running the waypoints initialization, which
		// means we need to drop out to avoid doing anything silly or before it breaks the game.
		if (totalWaypoints == 0)
		{
			// grab total waypoints
			totalWaypoints = myWayControl.GetTotal();
		}

		if (currentWaypointTransform == null)
		{
			// grab our transform reference from the waypoint controller
			currentWaypointTransform = myWayControl.GetWaypoint(currentWaypointNum);
			return;
		}

		// now we check to see if we are close enough to the current waypoint
		// to advance on to the next one

		myPosition = myTransform.position;
		myPosition.z = 0;

		// get waypoint position and 'flatten' it
		nodePosition = currentWaypointTransform.position;
		nodePosition.z = 0;

		// check distance from this to the waypoint
		currentWayDist = Vector3.Distance(nodePosition, myPosition);

		if (currentWayDist < waypointDistance)
		{
			// we are close to the current node, so let's move on to the next one!

			if (shouldReversePathFollowing)
			{
				currentWaypointNum--;
				// now check to see if we have been all the way around
				if (currentWaypointNum < 0)
				{
					// just in case it gets referenced before we are destroyed, let's keep it to a safe index number
					currentWaypointNum = 0;
					// completed the route!
					reachedLastWaypoint = true;
					// if we are set to loop, reset the currentWaypointNum to 0
					if (loopPath)
					{
						currentWaypointNum = totalWaypoints - 1;

						// grab our transform reference from the waypoint controller
						currentWaypointTransform = myWayControl.GetWaypoint(currentWaypointNum);

						// the route keeps going in a loop, so we don't want reachedLastWaypoint to ever become true
						reachedLastWaypoint = false;
					}

					// drop out of this function before we grab another waypoint into currentWaypointTransform, as
					// we don't need one and the index may be invalid
					return;
				}
			}
			else
			{
				currentWaypointNum++;
				// now check to see if we have been all the way around
				if (currentWaypointNum >= totalWaypoints)
				{
					// completed the route!
					reachedLastWaypoint = true;
					// if we are set to loop, reset the currentWaypointNum to 0
					if (loopPath)
					{
						currentWaypointNum = 0;

						// grab our transform reference from the waypoint controller
						currentWaypointTransform = myWayControl.GetWaypoint(currentWaypointNum);

						// the route keeps going in a loop, so we don't want reachedLastWaypoint to ever become true
						reachedLastWaypoint = false;
					}

					// drop out of this function before we grab another waypoint into currentWaypointTransform, as
					// we don't need one and the index may be invalid
					return;
				}
			}

			// grab our transform reference from the waypoint controller
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
}
