using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public class ColourFlash : PanelInterface
{
	Everything _module;
	int _modID;
	int _solvedIndex;
	MeshRenderer _display;
	TextMesh _displayText;
	KMSelectable[] _buttons;

	int _correctDigit;

	string[] _colors = new string[] { "Red", "Yellow", "Green", "Blue", "Magenta", "White" };

	int[] _chosenWords = new int[8];
	int[] _chosenColors = new int[8];

	bool _yes = false;
	int _correctPosition = 0;
	int _cWord = 0;
	int _cColor = 0;

	readonly int[] _leftTable = new int[] { 9, 8, 6, 7, 3, 4 };
	readonly int[] _rightTable = new int[] { 6, 4, 9, 8, 7, 3 };

	Coroutine displayCycle;

	int[] _finalWords = new int[8];
	int _finalCycle = 0;

	public ColourFlash(Everything _module, int _modID, int _solvedIndex, MeshRenderer _display, TextMesh _displayText, KMSelectable[] _buttons)
	{
		this._module = _module;
		this._modID = _modID;
		this._solvedIndex = _solvedIndex;
		this._display = _display;
		this._displayText = _displayText;
		this._buttons = _buttons;
	}

	public override void GeneratePanel()
	{
		for (int i = 0; i <= 7; i++)
		{
			int word = rnd.Range(0, _colors.Length);
			int color = rnd.Range(0, _colors.Length);

			_chosenWords[i] = word;
			_chosenColors[i] = color;

		}

		GenerateAnswer();

		_cWord = _leftTable[Array.IndexOf(_colors, _colors[_chosenWords[_correctPosition]])];
		_cColor = _rightTable[Array.IndexOf(_colors, _colors[_chosenColors[_correctPosition]])];

		if (_yes)
		{
			_correctDigit = (_cWord + _cColor + (_correctPosition + 1)) % 10;
		}
		else
		{
			_correctDigit = (_cWord * _cColor + (_correctPosition + 1)) % 10;
		}

		Debug.LogFormat("[Everything #{0}]: The Colour Flash panel was generated with {1} as words and {2} as colors. The answer is to press {3} on position {4}. The left table number is {5} and the right table number is {6}. They must be {7} together. The correct digit for this panel is: {8}.",
			_modID, _chosenWords.Select(x => _colors[x]).Join(", "), _chosenColors.Select(x => _colors[x]).Join(", "), _yes ? "YES" : "NO", _correctPosition + 1, _cWord, _cColor, _yes ? "added" : "multiplied", _correctDigit);
		HandlePanelSolve();
	}

	public override void GenerateFinalPanel()
	{
		int[] digits = _module.GetCorrectDigits().Select(x => int.Parse(x.ToString())).ToArray();
		string[] colors = new string[] { "Green:White", "Yellow:Magenta", "White:Yellow", "Blue:Green", "Red:Blue", "Magenta:Blue", "Magenta:Magenta", "White:Red", "Yellow:Green", "Red:Yellow" };

		string[] nc = new string[] { colors[digits[0]], colors[digits[1]], colors[digits[2]], colors[digits[3]] };

		for (int i = 0; i <= 7; i++) 
		{
			_finalWords[i] = rnd.Range(0, _colors.Length);
		}

		string[] newColors = new string[8];
		int count = 0;
		for (int i = 0; i <= 3; i++) 
		{
			string[] c = nc[i].Split(':');
			newColors[0 + count] = c[0];
			newColors[1 + count] = c[1];
			count += 2;
		}

		GenerateAnswer(_finalWords, newColors.Select(x => Array.IndexOf(_colors, x)).ToArray());

		Debug.LogFormat("[Everything #{0}]: The final panel was generated as Colour Flash. The words generated are {1}. Using the digits, the colors in order are {2}. The correct button to press is {3} and on position {4}.", _modID, _finalWords.Select(x => _colors[x]).Join(", "), newColors.Join(", "), _yes ? "YES" : "NO", _correctPosition+1);
	}

	public override void Interact(KMSelectable km)
	{
		int index = Array.IndexOf(_buttons, km);
		string button = index == 0 ? "yes" : "no";

		if (button == "yes")
		{
			if (!_yes || _correctPosition != _finalCycle - 1) 
			{
				_module.Strike();
				Debug.LogFormat("[Everything #{0}]: Incorrect button press. Expected a {1} press on {2} but was given a {3} on {4}.", _modID, _yes ? "YES" : "NO", _correctPosition+1, button.ToUpper(), _finalCycle);
				return;
			}
			if (displayCycle != null) 
			{
				_module.StopCoroutine(displayCycle);
				displayCycle = null;
			}
			Debug.LogFormat("[Everything #{0}]: Correct button pressed on correct position. Module solved.", _modID);
			_module.GetModule().HandlePass();
			_module._modSolved = true;
			_displayText.text = "SOLVED!";
		}
		else
		{
			if (_yes || _correctPosition != _finalCycle-1)
			{
				_module.Strike();
				Debug.LogFormat("[Everything #{0}]: Incorrect button press. Expected a {1} press on {2} but was given a {3} on {4}.", _modID, _yes ? "YES" : "NO", _correctPosition + 1, button.ToUpper(), _finalCycle);
				return;
			}
			if (displayCycle != null)
			{
				_module.StopCoroutine(displayCycle);
				displayCycle = null;
			}
			Debug.LogFormat("[Everything #{0}]: Correct button pressed on correct position. Module solved.", _modID);
			_module.GetModule().HandlePass();
			_module._modSolved = true;
			_displayText.text = "SOLVED!";
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
		_display.enabled = true;
		_displayText.GetComponent<MeshRenderer>().enabled = true;
		foreach (KMSelectable km in _buttons)
		{
			km.GetComponent<Renderer>().enabled = true;
			km.GetComponentInChildren<TextMesh>().GetComponent<MeshRenderer>().enabled = true;
			km.Highlight.gameObject.SetActive(true);
			yield return new WaitForSeconds(0.25f);
		}
		yield return new WaitForSeconds(1.25f);
		if (_module.GetFinalState())
		{
			displayCycle = _module.StartCoroutine(CycleDisplay(1.25f, 1.5f, _finalWords));
		}
		else
		{
			displayCycle = _module.StartCoroutine(CycleDisplay(1.25f, 1.5f));
			
		}
		_module.StartNextPanelAnimation();
		yield break;
	}

	public override IEnumerator DisableComponents()
	{
		if (displayCycle != null)
		{
			_module.StopCoroutine(displayCycle);
			displayCycle = null;
		}
		if (_module.GetFinalState()) 
		{
			foreach (KMSelectable km in _buttons)
			{
				km.GetComponent<Renderer>().enabled = false;
				km.GetComponentInChildren<TextMesh>().GetComponent<MeshRenderer>().enabled = false;
				km.Highlight.gameObject.SetActive(false);
			}
		}
		_displayText.GetComponent<MeshRenderer>().enabled = false;
		yield return new WaitForSeconds(0.25f);
		_display.enabled = false;
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
		return new Vector3(0.085f, 0.01f, 0.085f);
	}

	public void GenerateAnswer()
	{
		int last = _chosenColors[7];

		IEnumerable<int> wordSearch = Enumerable.Range(0, _chosenWords.Length);
		IEnumerable<int> colorSearch = Enumerable.Range(0, _chosenColors.Length);

		switch (_colors[last])
		{
			case "Red":
				if (_chosenWords.Where(x => _colors[x] == "Green").Count() >= 3)
				{
					_yes = true;
					int gWC = 0;
					int gCC = 0;

					for (int i = 0; i <= 7; i++)
					{
						if (gWC == 3 || gCC == 3) break;
						if (_colors[_chosenWords[i]] == "Green") gWC++;
						if (_colors[_chosenColors[i]] == "Green") gCC++;
					}

					if (gWC == 3)
					{
						List<int> gWords = wordSearch.Where(x => _colors[_chosenWords[x]] == "Green").ToList();
						_correctPosition = gWords[2];
					}
					else
					{
						List<int> gColors = colorSearch.Where(x => _colors[_chosenColors[x]] == "Green").ToList();
						_correctPosition = gColors[2];
					}
					return;
				}
				else if (_chosenColors.Where(x => _colors[x] == "Blue").Count() == 1)
				{
					_yes = false;
					_correctPosition = wordSearch.Where(x => _colors[_chosenWords[x]] == "Magenta").ToList().Last();
					return;
				}
				else
				{
					_yes = true;
					for (int i = 7; i >= 0; i--)
					{
						if (_colors[_chosenWords[i]] == "White" || _colors[_chosenColors[i]] == "White")
						{
							_correctPosition = i;
							break;
						}
					}
					return;
				}
			case "Yellow":

				if (_chosenWords.Any(x => _colors[x] == "Blue"))
				{
					List<int> indices = wordSearch.Where(x => _colors[_chosenWords[x]] == "Blue").ToList();

					for (int i = 0; i < indices.Count(); i++)
					{
						if (_colors[_chosenColors[indices[i]]] == "Green")
						{
							_yes = true;
							List<int> green = colorSearch.Where(x => _colors[_chosenColors[x]] == "Green").ToList();
							_correctPosition = green[0];
							break;
						}
					}
					return;
				}
				else if (_chosenWords.Any(x => _colors[x] == "White"))
				{
					List<int> white = wordSearch.Where(x => _colors[_chosenWords[x]] == "White").ToList();

					for (int i = 0; i < white.Count; i++)
					{
						if (_colors[_chosenColors[white[i]]] == "White" || _colors[_chosenColors[white[i]]] == "Red")
						{
							_yes = true;
							int mismatch = 0;
							for (int x = 0; x <= 7; x++)
							{
								if (mismatch == 2)
								{
									_correctPosition = x;
									break;
								}
								if (_chosenColors[x] != _chosenWords[x]) mismatch++;
							}
							break;
						}
					}
					return;
				}
				else
				{
					_yes = false;
					int magCount = 0;
					magCount += wordSearch.Where(x => _colors[_chosenWords[x]] == "Magenta" && _colors[_chosenColors[x]] != "Magenta").Count();
					magCount += wordSearch.Where(x => _colors[_chosenWords[x]] != "Magenta" && _colors[_chosenColors[x]] == "Magenta").Count();

					_correctPosition = magCount;
					return;
				}
			case "Green":

				for (int i = 0; i <= 6; i++)
				{
					if (_chosenWords[i] == _chosenWords[i + 1])
					{
						if (_chosenColors[i] != _chosenColors[i + 1])
						{
							_yes = false;
							_correctPosition = 4;
							return;
						}
					}
				}

				if (_chosenWords.Where(x => _colors[x] == "Magenta").Count() >= 3)
				{
					_yes = false;
					for (int i = 0; i <= 7; i++)
					{
						if (_colors[_chosenWords[i]] == "Yellow" || _colors[_chosenColors[i]] == "Yellow")
						{
							_correctPosition = i;
							return;
						}
					}
				}
				else
				{
					_yes = true;
					_correctPosition = wordSearch.Where(x => _chosenWords[x] == _chosenColors[x]).ToList().Last();
					return;
				}
				return;
			case "Blue":
				if (wordSearch.Where(x => _chosenWords[x] != _chosenColors[x]).Count() >= 3)
				{
					for (int i = 0; i <= 7; i++)
					{
						if (_chosenWords[i] != _chosenColors[i])
						{
							_yes = true;
							_correctPosition = i;
							return;
						}
					}
				}
				else if (wordSearch.Any(x => (_colors[_chosenWords[x]] == "Red" && _colors[_chosenColors[x]] == "Yellow") || (_colors[_chosenWords[x]] == "Yellow" && _colors[_chosenColors[x]] == "White")))
				{
					_yes = false;
					for (int i = 0; i <= 7; i++)
					{
						if (_colors[_chosenWords[i]] == "White" && _colors[_chosenColors[i]] == "Red")
						{
							_correctPosition = i;
							return;
						}
					}
				}
				else
				{
					_yes = true;
					for (int i = 7; i >= 0; i--)
					{
						if (_colors[_chosenWords[i]] == "Green" || _colors[_chosenColors[i]] == "Green")
						{
							_correctPosition = i;
							return;
						}
					}
				}
				return;
			case "Magenta":
				for (int i = 0; i <= 6; i++)
				{
					if (_chosenColors[i] == _chosenColors[i + 1])
					{
						if (_chosenWords[i] != _chosenWords[i + 1])
						{
							_yes = true;
							_correctPosition = 2;
							return;
						}
					}
				}

				if (_chosenWords.Where(x => _colors[x] == "Yellow").Count() > _chosenColors.Where(x => _colors[x] == "Blue").Count())
				{
					_yes = false;
					_correctPosition = wordSearch.Where(x => _colors[_chosenWords[x]] == "Yellow").ToList().Last();
					return;
				}
				else
				{
					_yes = false;
					int word = _chosenWords[6];
					_correctPosition = colorSearch.Where(x => _chosenColors[x] == word).ToList().First();
					return;
				}
			case "White":
				if (_chosenColors[2] == _chosenWords[3] || _chosenColors[2] == _chosenWords[4])
				{
					_yes = false;
					for (int i = 0; i <= 7; i++)
					{
						if (_colors[_chosenWords[i]] == "Blue" || _colors[_chosenColors[i]] == "Blue")
						{
							_correctPosition = i;
							return;
						}
					}
					return;
				}
				else if (wordSearch.Any(x => _colors[_chosenWords[x]] == "Yellow" && _colors[_chosenColors[x]] == "Red"))
				{
					_yes = true;
					_correctPosition = colorSearch.Where(x => _colors[_chosenColors[x]] == "Blue").ToList().Last();
					return;
				}
				else
				{
					_yes = false;
					_correctPosition = 7;
					return;
				}
			default:
				return;
		}

	}

	public void GenerateAnswer(int[] newWords, int[] newColors)
	{
		int last = newColors[7];

		IEnumerable<int> wordSearch = Enumerable.Range(0, newWords.Length);
		IEnumerable<int> colorSearch = Enumerable.Range(0, newColors.Length);

		switch (_colors[last])
		{
			case "Red":
				if (newWords.Where(x => _colors[x] == "Green").Count() >= 3)
				{
					_yes = true;
					int gWC = 0;
					int gCC = 0;

					for (int i = 0; i <= 7; i++)
					{
						if (gWC == 3 || gCC == 3) break;
						if (_colors[newWords[i]] == "Green") gWC++;
						if (_colors[newColors[i]] == "Green") gCC++;
					}

					if (gWC == 3)
					{
						List<int> gWords = wordSearch.Where(x => _colors[newWords[x]] == "Green").ToList();
						_correctPosition = gWords[2];
					}
					else
					{
						List<int> gColors = colorSearch.Where(x => _colors[newColors[x]] == "Green").ToList();
						_correctPosition = gColors[2];
					}
					return;
				}
				else if (newColors.Where(x => _colors[x] == "Blue").Count() == 1)
				{
					_yes = false;
					_correctPosition = wordSearch.Where(x => _colors[newWords[x]] == "Magenta").ToList().Last();
					return;
				}
				else
				{
					_yes = true;
					for (int i = 7; i >= 0; i--)
					{
						if (_colors[newWords[i]] == "White" || _colors[newColors[i]] == "White")
						{
							_correctPosition = i;
							break;
						}
					}
					return;
				}
			case "Yellow":

				if (newWords.Any(x => _colors[x] == "Blue"))
				{
					List<int> indices = wordSearch.Where(x => _colors[newWords[x]] == "Blue").ToList();

					for (int i = 0; i < indices.Count(); i++)
					{
						if (_colors[newColors[indices[i]]] == "Green")
						{
							_yes = true;
							List<int> green = colorSearch.Where(x => _colors[newColors[x]] == "Green").ToList();
							_correctPosition = green[0];
							break;
						}
					}
					return;
				}
				else if (newWords.Any(x => _colors[x] == "White"))
				{
					List<int> white = wordSearch.Where(x => _colors[newWords[x]] == "White").ToList();

					for (int i = 0; i < white.Count; i++)
					{
						if (_colors[newColors[white[i]]] == "White" || _colors[newColors[white[i]]] == "Red")
						{
							_yes = true;
							int mismatch = 0;
							for (int x = 0; x <= 7; x++)
							{
								if (mismatch == 2)
								{
									_correctPosition = x;
									break;
								}
								if (newColors[x] != newWords[x]) mismatch++;
							}
							break;
						}
					}
					return;
				}
				else
				{
					_yes = false;
					int magCount = 0;
					magCount += wordSearch.Where(x => _colors[newWords[x]] == "Magenta" && _colors[newColors[x]] != "Magenta").Count();
					magCount += wordSearch.Where(x => _colors[newWords[x]] != "Magenta" && _colors[newColors[x]] == "Magenta").Count();

					_correctPosition = magCount;
					return;
				}
			case "Green":

				for (int i = 0; i <= 6; i++)
				{
					if (newWords[i] == newWords[i + 1])
					{
						if (newColors[i] != newColors[i + 1])
						{
							_yes = false;
							_correctPosition = 4;
							return;
						}
					}
				}

				if (newWords.Where(x => _colors[x] == "Magenta").Count() >= 3)
				{
					_yes = false;
					for (int i = 0; i <= 7; i++)
					{
						if (_colors[newWords[i]] == "Yellow" || _colors[newColors[i]] == "Yellow")
						{
							_correctPosition = i;
							return;
						}
					}
				}
				else
				{
					_yes = true;
					_correctPosition = wordSearch.Where(x => newWords[x] == newColors[x]).ToList().Last();
					return;
				}
				return;
			case "Blue":
				if (wordSearch.Where(x => newWords[x] != newColors[x]).Count() >= 3)
				{
					for (int i = 0; i <= 7; i++)
					{
						if (newWords[i] != newColors[i])
						{
							_yes = true;
							_correctPosition = i;
							return;
						}
					}
				}
				else if (wordSearch.Any(x => (_colors[newWords[x]] == "Red" && _colors[newColors[x]] == "Yellow") || (_colors[newWords[x]] == "Yellow" && _colors[newColors[x]] == "White")))
				{
					_yes = false;
					for (int i = 0; i <= 7; i++)
					{
						if (_colors[newWords[i]] == "White" && _colors[newColors[i]] == "Red")
						{
							_correctPosition = i;
							return;
						}
					}
				}
				else
				{
					_yes = true;
					for (int i = 7; i >= 0; i--)
					{
						if (_colors[newWords[i]] == "Green" || _colors[newColors[i]] == "Green")
						{
							_correctPosition = i;
							return;
						}
					}
				}
				return;
			case "Magenta":
				for (int i = 0; i <= 6; i++)
				{
					if (newColors[i] == newColors[i + 1])
					{
						if (newWords[i] != newWords[i + 1])
						{
							_yes = true;
							_correctPosition = 2;
							return;
						}
					}
				}

				if (newWords.Where(x => _colors[x] == "Yellow").Count() > newColors.Where(x => _colors[x] == "Blue").Count())
				{
					_yes = false;
					_correctPosition = wordSearch.Where(x => _colors[newWords[x]] == "Yellow").ToList().Last();
					return;
				}
				else
				{
					_yes = false;
					int word = newWords[6];
					_correctPosition = colorSearch.Where(x => newColors[x] == word).ToList().First();
					return;
				}
			case "White":
				if (newColors[2] == newWords[3] || newColors[2] == newWords[4])
				{
					_yes = false;
					for (int i = 0; i <= 7; i++)
					{
						if (_colors[newWords[i]] == "Blue" || _colors[newColors[i]] == "Blue")
						{
							_correctPosition = i;
							return;
						}
					}
					return;
				}
				else if (wordSearch.Any(x => _colors[newWords[x]] == "Yellow" && _colors[newColors[x]] == "Red"))
				{
					_yes = true;
					_correctPosition = colorSearch.Where(x => _colors[newColors[x]] == "Blue").ToList().Last();
					return;
				}
				else
				{
					_yes = false;
					_correctPosition = 7;
					return;
				}
			default:
				return;
		}

	}

	IEnumerator CycleDisplay(float cycleDelay, float resetDelay)
	{
		Color32[] textColors = new Color32[]
		{
			new Color32(255, 0, 0, 255), // Red
			new Color32(255, 255, 0, 255), // Yellow
			new Color32(0, 255, 0, 255), // Green
			new Color32(0, 0, 255, 255), // Blue
			new Color32(255, 0, 255, 255), // Magenta
			new Color32(255, 255, 255, 255), // White			
		};
		int cycle = 0;
		while (true)
		{
			while (cycle != 8)
			{
				_displayText.text = _colors[_chosenWords[cycle]].ToUpper();
				_displayText.color = textColors[_chosenColors[cycle]];
				cycle++;
				yield return new WaitForSeconds(cycleDelay);
			}
			_displayText.text = "";
			_displayText.color = textColors[5];
			cycle = 0;
			yield return new WaitForSeconds(resetDelay);
		}

	}

	IEnumerator CycleDisplay(float cycleDelay, float resetDelay, int[] words)
	{
		Color32[] textColors = new Color32[]
		{
			new Color32(255, 0, 0, 255), // Red
			new Color32(255, 255, 0, 255), // Yellow
			new Color32(0, 255, 0, 255), // Green
			new Color32(0, 0, 255, 255), // Blue
			new Color32(255, 0, 255, 255), // Magenta
			new Color32(255, 255, 255, 255), // White			
		};
		while (true)
		{
			_finalCycle = 0;
			while (_finalCycle != 8)
			{
				_displayText.text = _colors[words[_finalCycle]].ToUpper();
				if (_finalCycle.EqualsAny(0, 2, 4, 6, 8)) _displayText.text += ".";
				_displayText.color = textColors[5];
				_finalCycle++;
				yield return new WaitForSeconds(cycleDelay);
			}
			_displayText.text = "";
			_displayText.color = textColors[5];
			_finalCycle = -1;
			yield return new WaitForSeconds(resetDelay);
		}

	}

}
