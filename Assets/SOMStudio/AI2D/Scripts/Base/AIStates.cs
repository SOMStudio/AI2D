namespace SOMStudio.AI2D.Scripts.Base
{
	public enum AIState
	{
		MovingLookingForTarget,
		ChasingTarget,
		BackingUpLookingForTarget,
		StoppedTurningLeft,
		StoppedTurningRight,
		PausedLookingForTarget,
		TranslateAlongWaypointPath,
		PausedNoTarget,
	}
}
