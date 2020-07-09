using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public class PanelInterface
{

	public virtual void GeneratePanel()
	{

	}

	public virtual void GenerateFinalPanel() 
	{ 
		
	}

	public virtual void Interact()
	{

	}

	public virtual void Interact(KMSelectable km)
	{

	}

	public virtual void InteractEnd()
	{

	}

	public virtual void OnHover()
	{

	}

	public virtual void OnDehover()
	{

	}

	public virtual IEnumerator EnableComponents()
	{
		yield break;
	}

	public virtual IEnumerator DisableComponents()
	{
		yield break;
	}

	public virtual IEnumerator ChangeBaseSize(float speed)
	{
		yield break;
	}

	public virtual int GetCorrectDigit()
	{
		return -1;
	}

	public virtual void HandlePanelSolve() 
	{ 
	
	}

	public virtual Vector3 GetBaseSize()
	{
		return new Vector3(0f, 0f, 0f);
	}

}
