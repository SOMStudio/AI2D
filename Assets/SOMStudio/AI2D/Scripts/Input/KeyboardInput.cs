using UnityEngine;

[AddComponentMenu("SOMStudio/AI2D/Input/Keyboard Input Controller")]
public class KeyboardInput : BaseInputController
{
	private void LateUpdate()
	{
		CheckInput();
	}
	
	protected override void CheckInput()
	{
		base.CheckInput();
		
		fire1 = Input.GetButton("Fire1");
		shouldRespawn = Input.GetButton("Fire3");
	}
}
