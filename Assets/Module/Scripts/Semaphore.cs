using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public class Semaphore : PanelInterface {

	Everything _module;
	int _modID;
	int _solvedIndex;
	MeshRenderer[] _flagRenders;
	Transform[] _flagTransforms;
	KMSelectable[] _buttons;
	bool _isRotating = false;
	bool _firstColor = true;
	Color32 _originalColor = new Color32(255, 135, 4, 255);

	int _correctDigit;

	string _chosenCharacters = "";

	List<string> _flagDirections = new List<string>();

	string[] _flagCharacters = new string[]
	{
		"SW.S", // A / 1
		"W.S", // B / 2
		"NW.S", // C / 3
		"N.S", // D / 4
		"S.NE", // E / 5
		"S.E",  // F / 6
		"S.SE", // G / 7
		"W.SW", // H / 8
		"SW.NW", // I / 9
		"N.E", // J
		"SW.N", // K / 0
		"SW.NE", // L
		"SW.E", // M
		"SW.SE", // N
		"W.NW", // O
		"W.N", // P
		"W.NE", // Q
		"W.E", // R
		"W.SE", // S
		"NW.N", // T
		"NW.NE", // U
		"N.SE", // V
		"NE.E", // W
		"NE.SE", // X
		"NW.E", // Y
		"SE.E", // Z
	};
	float[] _directionRotations = new float[] { 270f, 315f, 0f, 45f, 90f, 135f, 180f, 225f }; // North clockwise
	string[] _directionNames = new string[] { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };

	int _characterIndex = 0;
	int _finalCharacterIndex = 0;

	char[] _finalCharacters;
	List<string> _finalFlagDirections = new List<string>();
	int _finalCIndex = 0;

	public Semaphore(Everything _module, int _modID, int _solvedIndex, MeshRenderer[] _flagRenders, Transform[] _flagTransforms, KMSelectable[] _buttons)
	{
		this._module = _module;
		this._modID = _modID;
		this._solvedIndex = _solvedIndex;
		this._flagRenders = _flagRenders;
		this._flagTransforms = _flagTransforms;
		this._buttons = _buttons;
	}

	public override void GeneratePanel()
	{
		char[] alphabet = Enumerable.Range(0, 26).Select(x => (char)(x + 'A')).ToArray();

		int amount = 9;

		for (int i = 0; i < amount; i++)
		{
			int r = rnd.Range(0, 6);
			_chosenCharacters += _module.GetBombInfo().GetSerialNumber()[r];
		}

		bool nums = false;

		char c = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".PickRandom();

		while (_module.GetBombInfo().GetSerialNumber().Contains(c))
		{
			c = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".PickRandom();
		}

		_chosenCharacters += c;
		_chosenCharacters = _chosenCharacters.ToArray().Shuffle().Join("");

		foreach (char ch in _chosenCharacters)
		{
			if (alphabet.Contains(ch))
			{
				nums = false;
			}
			else
			{
				nums = true;
			}

			if (nums)
			{
				int[] nA = new int[] { 10, 0, 1, 2, 3, 4, 5, 6, 7, 8 };
				_flagDirections.Add("N.NE");
				_flagDirections.Add(_flagCharacters[nA[int.Parse(ch.ToString())]]);
				continue;
			}
			else
			{
				_flagDirections.Add("N.E");
				_flagDirections.Add(_flagCharacters[Array.IndexOf(alphabet, ch)]);
				continue;
			}
		}

		_correctDigit = (c.EqualsAny('0', '1', '2', '3', '4', '5', '6', '7', '8', '9') ? int.Parse(c.ToString()) : Array.IndexOf(alphabet, c) + 1) % 10;

		UpdateCharacter();

		Debug.LogFormat("[Everything #{0}]: The Semaphore panel was generated with {1} as the generated characters. The character to submit is {2}. The correct digit for this panel is: {3}.", _modID, _chosenCharacters, c, _correctDigit);
		HandlePanelSolve();
	}

	public override void GenerateFinalPanel()
	{
		int[] digits = _module.GetCorrectDigits().Select(x => int.Parse(x.ToString())).ToArray();
		char[] alphabet = Enumerable.Range(0, 26).Select(x => (char)(x + 'A')).ToArray();
		List<char> final = new List<char>();
		foreach (int d in digits.Distinct()) 
		{
			final.Add(char.Parse(d.ToString()));
		}

		for (int i = 0; i <= 3; i++) 
		{
			
			char ch = _module.GetBombInfo().GetSerialNumber()[i];
			int parsed;
			if (int.TryParse(ch.ToString(), out parsed))
			{
				final.Add(char.Parse(parsed.ToString()));
				continue;
			}
			else
			{
				int index = Array.IndexOf(alphabet, ch) + digits[i] + 1;
				final.Add(alphabet[index % 26]);
				continue;
			}
		}

		_finalCharacters = final.ToArray();

		int amount = 9;

		char c = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".PickRandom();
		while (_finalCharacters.Contains(c))
		{
			c = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".PickRandom();
		}

		char[] chars = new char[10];

		for (int i = 0; i < amount; i++) 
		{
			chars[i] = _finalCharacters[rnd.Range(0, _finalCharacters.Length)];
		}

		chars[9] = c;
		chars.Shuffle();

		bool nums;
		foreach (char ch in _finalCharacters)
		{
			if (alphabet.Contains(ch))
			{
				nums = false;
			}
			else
			{
				nums = true;
			}

			if (nums)
			{
				int[] nA = new int[] { 10, 0, 1, 2, 3, 4, 5, 6, 7, 8 };
				_finalFlagDirections.Add("N.NE");
				_finalFlagDirections.Add(_flagCharacters[nA[int.Parse(ch.ToString())]]);
				continue;
			}
			else
			{
				_finalFlagDirections.Add("N.E");
				_finalFlagDirections.Add(_flagCharacters[Array.IndexOf(alphabet, ch)]);
				continue;
			}
		}

		_finalCIndex = Array.IndexOf(chars, c);

		Debug.LogFormat("[Everything #{0}]: The final panel was generated as Semaphore. The characters that are being used as decoys are: {1}. The generated characters are {2}. The correct character to submit is {3}.", 
			_modID, _finalCharacters.Select(x => "'" + x + "'").Join(", "), chars.Join(""), c);
	}

	public override void Interact(KMSelectable km)
	{
		int index = Array.IndexOf(_buttons, km);

		if (_isRotating)
		{
			return;
		}

		if (index.EqualsAny(0, 1))
		{
			if (index == 0)
			{
				if (_module.GetFinalState())
				{
					if (_finalCharacterIndex == 0) return;
					_finalCharacterIndex--;
					if (!_firstColor)
					{
						_flagRenders[0].materials[1].color = _originalColor;
						_flagRenders[1].materials[1].color = _originalColor;
						_firstColor = true;
					}
					else
					{
						_flagRenders[0].materials[1].color = new Color32(99, 204, 99, 255);
						_flagRenders[1].materials[1].color = new Color32(99, 204, 99, 255);
						_firstColor = false;
					}
				}
				else
				{
					if (_characterIndex == 0) return;
					_characterIndex--;
					if (!_firstColor)
					{
						_flagRenders[0].materials[1].color = _originalColor;
						_flagRenders[1].materials[1].color = _originalColor;
						_firstColor = true;
					}
					else
					{
						_flagRenders[0].materials[1].color = new Color32(99, 204, 99, 255);
						_flagRenders[1].materials[1].color = new Color32(99, 204, 99, 255);
						_firstColor = false;
					}
				}
				UpdateCharacter();
				return;
			}
			else if (index == 1)
			{
				if (_module.GetFinalState())
				{
					if (_finalCharacterIndex == _finalFlagDirections.Count - 1) return;
					_finalCharacterIndex++;
					if (!_firstColor)
					{
						_flagRenders[0].materials[1].color = _originalColor;
						_flagRenders[1].materials[1].color = _originalColor;
						_firstColor = true;
					}
					else
					{
						_flagRenders[0].materials[1].color = new Color32(99, 204, 99, 255);
						_flagRenders[1].materials[1].color = new Color32(99, 204, 99, 255);
						_firstColor = false;
					}
				}
				else
				{
					if (_characterIndex == _flagDirections.Count - 1) return;
					_characterIndex++;
					if (!_firstColor)
					{
						_flagRenders[0].materials[1].color = _originalColor;
						_flagRenders[1].materials[1].color = _originalColor;
						_firstColor = true;
					}
					else
					{
						_flagRenders[0].materials[1].color = new Color32(99, 204, 99, 255);
						_flagRenders[1].materials[1].color = new Color32(99, 204, 99, 255);
						_firstColor = false;
					}
				}
				UpdateCharacter();
				return;
			}
		}

		if (_module.GetFinalState() && index == 2) 
		{
			if (_finalCharacterIndex == _finalCIndex)
			{
				Debug.LogFormat("[Everything #{0}]: Correct character submitted. Module Solved.", _modID);
				_module.GetModule().HandlePass();
				_module._modSolved = true;
				return;
			}
			else
			{
				_module.Strike();
				Debug.LogFormat("[Everything #{0}]: Incorrect character submitted. Expected {1} ({2}) but was given {3} ({4})", _modID, _finalCharacters[_finalCIndex], _finalCIndex, _finalCharacters[_finalCharacterIndex], _finalCharacterIndex);
				return;
			}
		}

	}

	public override void InteractEnd()
	{

	}

	public override void OnHover()
	{

	}

	public override void OnDehover()
	{

	}

	public override IEnumerator EnableComponents()
	{
		if (_module.GetFinalState())
		{
			_finalCharacterIndex = 0;
			_flagRenders[0].materials[1].color = _originalColor;
			_flagRenders[1].materials[1].color = _originalColor;
			_firstColor = true;
			UpdateCharacter();
			_flagRenders[0].enabled = true;
			_flagRenders[1].enabled = true;
			yield return new WaitForSeconds(0.1f);
			foreach (KMSelectable km in _buttons)
			{
				km.GetComponent<MeshRenderer>().enabled = true;
				km.GetComponentInChildren<TextMesh>().GetComponent<MeshRenderer>().enabled = true;
				km.Highlight.gameObject.SetActive(true);
				yield return new WaitForSeconds(0.1f);
			}
			_module.StartNextPanelAnimation();
			yield break;
		}
		_characterIndex = 0;
		_flagRenders[0].materials[1].color = _originalColor;
		_flagRenders[1].materials[1].color = _originalColor;
		_firstColor = true;
		UpdateCharacter();
		_flagRenders[0].enabled = true;
		_flagRenders[1].enabled = true;
		yield return new WaitForSeconds(0.1f);
		foreach (KMSelectable km in _buttons)
		{
			if (km.gameObject.name.ToLower() == "semasubmit") continue;
			km.GetComponent<MeshRenderer>().enabled = true;
			km.GetComponentInChildren<TextMesh>().GetComponent<MeshRenderer>().enabled = true;
			km.Highlight.gameObject.SetActive(true);
			yield return new WaitForSeconds(0.1f);
		}
		_module.StartNextPanelAnimation();
		yield break;
	}

	public override IEnumerator DisableComponents()
	{
		_flagRenders[0].enabled = false;
		_flagRenders[1].enabled = false;
		yield return new WaitForSeconds(0.1f);
		foreach (KMSelectable km in _buttons)
		{
			km.GetComponent<MeshRenderer>().enabled = false;
			km.GetComponentInChildren<TextMesh>().GetComponent<MeshRenderer>().enabled = false;
			km.Highlight.gameObject.SetActive(false);
			yield return new WaitForSeconds(0.1f);
		}
		_module.StartNextPanelAnimation();
		yield break;
	}

	public override IEnumerator ChangeBaseSize(float delay)
	{

		Vector3 baseSize = GetBaseSize();
		Transform baseTrans = _module._moduleBasePanel.transform;
		while (true)
		{
			float x = (float)Math.Round(baseTrans.localScale.x, 3);
			float y = (float)Math.Round(baseTrans.localScale.y, 3);
			float z = (float)Math.Round(baseTrans.localScale.z, 3);
			if ((x <= baseSize.x + 0.003f && x >= baseSize.x - 0.003f)
				&& (y <= baseSize.y + 0.003f && y >= baseSize.y - 0.003f)
				&& (z <= baseSize.z + 0.003f && z >= baseSize.z - 0.003f)) { break; }
			if (x < baseSize.x) { baseTrans.localScale = new Vector3(x + 0.001f, y, z); }
			else if (x > baseSize.x) { baseTrans.localScale = new Vector3(x - 0.001f, y, z); }
			if (y < baseSize.y) { baseTrans.localScale = new Vector3(x, y + 0.001f, z); }
			else if (y > baseSize.y) { baseTrans.localScale = new Vector3(x, y - 0.001f, z); }
			if (z < baseSize.z) { baseTrans.localScale = new Vector3(x, y, z + 0.001f); }
			else if (z > baseSize.z) { baseTrans.localScale = new Vector3(x, y, z - 0.001f); }
			yield return new WaitForSeconds(delay);
		}
		baseTrans.localScale = baseSize;
		_module.StartNextPanelAnimation();
		yield break;
	}

	public override int GetCorrectDigit()
	{
		return _correctDigit;
	}

	public override void HandlePanelSolve()
	{
		_module._moduleSelectors[_solvedIndex].GetComponent<Renderer>().material.color = new Color32(0, 169, 0, 255);
		_module.SetSolvedPanel(_solvedIndex, true);
		Debug.LogFormat("[Everything #{0}]: After the recent panel solve, there are {1} panels left to solve.", _modID, 4 - _module.GetSolvedPanels().Where(x => x).Count());
	}

	public override Vector3 GetBaseSize()
	{
		return new Vector3(0.085f, 0.01f, 0.1f);
	}

	void RotateFlag(Transform t, Quaternion target, float speed)
	{
		t.localRotation = Quaternion.RotateTowards(t.localRotation, target, speed * Time.deltaTime);
	}

	void UpdateCharacter() 
	{
		if (_module.GetFinalState()) 
		{
			string[] finalDirections = _finalFlagDirections[_finalCharacterIndex].Split('.');
			float fLeft = _directionRotations[Array.IndexOf(_directionNames, finalDirections[0])];
			float fRight = _directionRotations[Array.IndexOf(_directionNames, finalDirections[1])];

			Vector3 fLeftRot = _flagTransforms[0].localEulerAngles;
			Vector3 fRightRot = _flagTransforms[1].localEulerAngles;

			_module.StartCoroutine(Rotator(fLeft, fRight, fLeftRot, fRightRot));
			return;
		}

		string[] directions = _flagDirections[_characterIndex].Split('.');
		float left = _directionRotations[Array.IndexOf(_directionNames, directions[0])];
		float right = _directionRotations[Array.IndexOf(_directionNames, directions[1])];

		Vector3 leftRot = _flagTransforms[0].localEulerAngles;
		Vector3 rightRot = _flagTransforms[1].localEulerAngles;

		_module.StartCoroutine(Rotator(left, right, leftRot, rightRot));

	}

	IEnumerator Rotator(float left, float right, Vector3 leftRot, Vector3 rightRot) 
	{
		_isRotating = true;
		while (_flagTransforms[0].localEulerAngles.y != left || _flagTransforms[1].localEulerAngles.y != right)
		{
			RotateFlag(_flagTransforms[0], Quaternion.Euler(leftRot.x, left, leftRot.z), 500f);
			RotateFlag(_flagTransforms[1], Quaternion.Euler(rightRot.x, right, rightRot.z), 500f);
			yield return new WaitForSeconds(0.005f);
		}
		_isRotating = false;
		yield break;
	}

}
