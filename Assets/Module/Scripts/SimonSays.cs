using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public class SimonSays : PanelInterface
{

	Everything _module;
	int _modID;
	int _solvedIndex;
	KMSelectable[] _buttons;
	Light[] _buttonLights; // Blue, Red, Yellow, Green
	TextMesh _strikes;

	int _correctDigit;

	string[][] _colors = new string[2][];
	string[] _possibleColors = new string[] { "Blue", "Red", "Yellow", "Green" };
	string[] _currentSequence;
	int _chosenStrikes = 0;
	int _input = 0;
	bool _stopFlashes = false;
	bool _nextStage = false;

	string[] _finalSequence;
	string[] _finalFlashes;

	public SimonSays(Everything _module, int _modID, int _solvedIndex, KMSelectable[] _buttons, Light[] _buttonLights, TextMesh _strikes)
	{
		this._module = _module;
		this._modID = _modID;
		this._solvedIndex = _solvedIndex;
		this._buttons = _buttons;
		this._buttonLights = _buttonLights;
		this._strikes = _strikes;
	}

	public override void GeneratePanel()
	{
		int amount = rnd.Range(3, 6);
		int strikes = rnd.Range(0, 3);
		_chosenStrikes = strikes;
		string[] flashes = new string[amount];

		for (int i = 0; i < amount; i++)
		{
			int color = rnd.Range(0, 4);
			flashes[i] = _possibleColors[color];
		}

		_colors[0] = flashes;
		_colors[1] = GetPressingColors();
		_strikes.text = strikes.ToString();

		_currentSequence = new string[1];
		_currentSequence[0] = _colors[0][0];

		int[] base5;
		List<int> temp = new List<int>();
		foreach (string color in _colors[1])
		{
			temp.Add(Array.IndexOf(_possibleColors, color) + 1);
		}
		base5 = temp.ToArray();
		char[] base10 = ConvertToBase10(base5).ToString().ToCharArray();

		_correctDigit = int.Parse(base10[base10.Length - 1].ToString());

		Debug.LogFormat("[Everything #{0}]: The Simon Says panel was generated with color flashes of {1} and {2} strikes. The correct presses are {3} which translate to {4} in base-5 and in base-10 is {5}. The correct digit for this panel is: {6}.", _modID, _colors[0].Join(", "), _chosenStrikes, _colors[1].Join(", "), base5.Join(""), base10.Join(""), _correctDigit);
	}

	public override void GenerateFinalPanel()
	{
		int digital = _module.GetDigitalRoot(int.Parse(_module.GetCorrectDigits()));

		foreach (MeshRenderer mr in _buttons.Select(x => x.GetComponent<Renderer>()))
		{
			mr.material.color = new Color32(33, 33, 33, 255);
		}

		foreach (Light l in _buttonLights)
		{
			l.color = new Color32(50, 199, 199, 255);
		}

		_finalSequence = GetColorSequence(digital);

		int flashes = rnd.Range(3, 6);
		int strikes = rnd.Range(0, 3);
		_chosenStrikes = strikes;
		_finalFlashes = new string[flashes];

		for (int i = 0; i < flashes; i++)
		{
			int color = rnd.Range(0, 4);
			_finalFlashes[i] = _possibleColors[color];
		}

		_colors[0] = _finalFlashes;
		_colors[1] = GetPressingColors();
		_strikes.text = strikes.ToString();

		_currentSequence = new string[1];
		_currentSequence[0] = _colors[0][0];

		Debug.LogFormat("[Everything #{0}]: The final panel was generated as Simon Says, the chosen digital root of the 4 digits is {1}. The determined color order going clockwise is {2}. The flash sequence is {3} and strikes is {4} meaning that buttons presses are {5}.", _modID, digital, _finalSequence.Join(", "), _colors[0].Join(", "), _chosenStrikes, _colors[1].Join(", "));

	}

	public override void Interact(KMSelectable km)
	{
		_module.StopAllCoroutines();
		foreach (Light l in _buttonLights)
		{
			l.enabled = false;
		}
		_module._audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, _module.GetComponent<KMBombModule>().transform);
		int index = Array.IndexOf(_buttons, km);
		_module.StartCoroutine(PressFlash(0.6f, index));

		if (_module.GetFinalState())
		{
			string actual = _finalSequence[GetActualColor(index)];

			if (actual == _colors[1][_input] && _input == _currentSequence.Length - 1)
			{
				if (_currentSequence.Length == _colors[1].Length)
				{
					Debug.LogFormat("[Everything #{0}]: All correct buttons have been pressed. Module solved", _modID);
					_module._modSolved = true;
					_module.GetModule().HandlePass();
					return;
				}
				List<string> seq = _currentSequence.ToList();
				seq.Add(_colors[0][_input + 1]);
				_currentSequence = seq.ToArray();
				_input = 0;
				_module.StartCoroutine(SequenceFlashes(0.5f, 0.5f, 1.75f, _currentSequence));
				return;
			}
			else if (actual == _colors[1][_input])
			{
				_input++;
				_module.StartCoroutine(InputReset(3.0f));
				return;
			}
			else
			{
				_module.Strike();
				Debug.LogFormat("[Everything #{0}]: Incorrect button press on the Simon Says panel. Expected {1} but recieved {2}.", _modID, _colors[1][_input], actual);
				_input = 0;
				_module.StartCoroutine(SequenceFlashes(0.5f, 0.5f, 1.75f, _currentSequence));
				return;
			}
		}

		string color = _buttonLights[index].gameObject.name;

		if (color == _colors[1][_input] && _input == _currentSequence.Length - 1)
		{
			if (_currentSequence.Length == _colors[1].Length)
			{
				Debug.LogFormat("[Everything #{0}]: Simon Says Panel Solved.", _modID);
				HandlePanelSolve();
				return;
			}
			List<string> seq = _currentSequence.ToList();
			seq.Add(_colors[0][_input + 1]);
			_currentSequence = seq.ToArray();
			_input = 0;
			_nextStage = true;
			_module.StartCoroutine(SequenceFlashes(0.5f, 0.5f, 1.75f, _currentSequence));
			return;
		}
		else if (color == _colors[1][_input])
		{
			_input++;
			_module.StartCoroutine(InputReset(3.0f));
			return;
		}
		else
		{
			_module.Strike();
			Debug.LogFormat("[Everything #{0}]: Incorrect button press on the Simon Says panel. Expected {1} but recieved {2}.", _modID, _colors[1][_input], color);
			_input = 0;
			_module.StartCoroutine(SequenceFlashes(0.5f, 0.5f, 1.75f, _currentSequence));
			return;
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

		_strikes.GetComponent<Renderer>().enabled = true;
		yield return new WaitForSeconds(0.25f);
		foreach (KMSelectable km in _buttons)
		{
			km.GetComponent<Renderer>().enabled = true;
			km.Highlight.gameObject.SetActive(true);
			yield return new WaitForSeconds(0.25f);
		}
		_nextStage = true;
		_module.StartCoroutine(SequenceFlashes(0.5f, 0.5f, 1.75f, _currentSequence));
		_module.StartNextPanelAnimation();
		yield break;
	}

	public override IEnumerator DisableComponents()
	{
		_stopFlashes = true;

		foreach (Light l in _buttonLights)
		{
			l.enabled = false;
		}
		_strikes.GetComponent<Renderer>().enabled = false;
		yield return new WaitForSeconds(0.25f);
		foreach (KMSelectable km in _buttons)
		{
			km.GetComponent<Renderer>().enabled = false;
			km.Highlight.gameObject.SetActive(false);
			yield return new WaitForSeconds(0.25f);
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
		Debug.LogFormat("[Everything #{0}]: After the recent panel solve, there are {1} panels left to solve.", _modID, 4 - _module.GetSolvedPanels().Select(x => x).Count());
	}

	public override Vector3 GetBaseSize()
	{
		return new Vector3(0.09f, 0.01f, 0.09f);
	}

	IEnumerator PressFlash(float delay, int color)
	{
		_buttonLights[color].enabled = true;
		yield return new WaitForSeconds(delay);
		_buttonLights[color].enabled = false;
		yield break;
	}

	IEnumerator SequenceFlashes(float on, float off, float reset, string[] colors)
	{
		if (_nextStage) yield return new WaitForSeconds(reset);
		while (true)
		{
			foreach (string c in colors)
			{
				int light = Array.IndexOf(_possibleColors, c);
				if (_stopFlashes) { yield break; }
				_buttonLights[light].enabled = true;
				yield return new WaitForSeconds(on);
				if (_stopFlashes) { yield break; }
				_buttonLights[light].enabled = false;
				yield return new WaitForSeconds(off);
			}
			if (_stopFlashes) { yield break; }
			yield return new WaitForSeconds(reset);
		}
	}

	IEnumerator InputReset(float delay)
	{
		yield return new WaitForSeconds(delay);
		_input = 0;
		_module.StartCoroutine(SequenceFlashes(0.5f, 0.5f, 1.75f, _currentSequence));
		yield break;
	}

	string[] GetPressingColors()
	{
		List<string> colors = new List<string>();
		foreach (string c in _colors[0])
		{
			if (_module.GetBombInfo().GetSerialNumberLetters().Any(x => x.EqualsAny('A', 'E', 'I', 'O', 'U')))
			{
				switch (c)
				{
					case "Red":
						if (_chosenStrikes == 0)
						{
							colors.Add("Blue");
						}
						else if (_chosenStrikes == 1)
						{
							colors.Add("Yellow");
						}
						else
						{
							colors.Add("Green");
						}
						break;
					case "Blue":
						if (_chosenStrikes == 0)
						{
							colors.Add("Red");
						}
						else if (_chosenStrikes == 1)
						{
							colors.Add("Green");
						}
						else
						{
							colors.Add("Red");
						}
						break;
					case "Green":
						if (_chosenStrikes == 0)
						{
							colors.Add("Yellow");
						}
						else if (_chosenStrikes == 1)
						{
							colors.Add("Blue");
						}
						else
						{
							colors.Add("Yellow");
						}
						break;
					case "Yellow":
						if (_chosenStrikes == 0)
						{
							colors.Add("Green");
						}
						else if (_chosenStrikes == 1)
						{
							colors.Add("Red");
						}
						else
						{
							colors.Add("Blue");
						}
						break;
					default:
						break;
				}
			}
			else
			{
				switch (c)
				{
					case "Red":
						if (_chosenStrikes == 0)
						{
							colors.Add("Blue");
						}
						else if (_chosenStrikes == 1)
						{
							colors.Add("Red");
						}
						else
						{
							colors.Add("Yellow");
						}
						break;
					case "Blue":
						if (_chosenStrikes == 0)
						{
							colors.Add("Yellow");
						}
						else if (_chosenStrikes == 1)
						{
							colors.Add("Blue");
						}
						else
						{
							colors.Add("Green");
						}
						break;
					case "Green":
						if (_chosenStrikes == 0)
						{
							colors.Add("Green");
						}
						else if (_chosenStrikes == 1)
						{
							colors.Add("Yellow");
						}
						else
						{
							colors.Add("Blue");
						}
						break;
					case "Yellow":
						if (_chosenStrikes == 0)
						{
							colors.Add("Red");
						}
						else if (_chosenStrikes == 1)
						{
							colors.Add("Green");
						}
						else
						{
							colors.Add("Red");
						}
						break;
					default:
						break;
				}
			}
		}
		return colors.ToArray();
	}

	string[] GetColorSequence(int index)
	{
		switch (index)
		{
			case 0:
				return new string[] { "Blue", "Yellow", "Green", "Red" };
			case 1:
				return new string[] { "Red", "Blue", "Yellow", "Green" };
			case 2:
				return new string[] { "Blue", "Red", "Green", "Yellow" };
			case 3:
				return new string[] { "Yellow", "Blue", "Red", "Green" };
			case 4:
				return new string[] { "Red", "Yellow", "Green", "Blue" };
			case 5:
				return new string[] { "Red", "Blue", "Green", "Yellow" };
			case 6:
				return new string[] { "Green", "Yellow", "Blue", "Red" };
			case 7:
				return new string[] { "Blue", "Green", "Yellow", "Red" };
			case 8:
				return new string[] { "Yellow", "Red", "Green", "Blue" };
			case 9:
				return new string[] { "Yellow", "Green", "Red", "Blue" };
			default:
				return null;
		}
	}

	int GetActualColor(int index)
	{
		switch (index)
		{
			case 0:
				return 0;
			case 1:
				return 3;
			case 2:
				return 1;
			case 3:
				return 2;
			default:
				return -1;
		}
	}

	int ConvertToBase10(int[] base5)
	{
		int pow = 0;
		int total = 0;
		foreach (int i in base5.Reverse())
		{
			total += (int)Math.Pow(5, pow) * i;
			pow++;
		}
		return total;
	}

}
