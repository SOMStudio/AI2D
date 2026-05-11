using SOMStudio.AI2D.Scripts.Base;
using UnityEngine;

namespace SOMStudio.AI2D.Scripts.Input
{
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
		
			fire1 = UnityEngine.Input.GetButton("Fire1");
			shouldRespawn = UnityEngine.Input.GetButton("Fire3");
		}
	}
}
