using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public class WhosOnFirst : PanelInterface
{

	Everything _module;
	int _modID;
	int _solvedIndex;
	KMSelectable[] _buttons;
	TextMesh[] _buttonTexts;
	TextMesh _display;
	MeshRenderer[] _additionalMeshes;
	GameObject _finalStageIndicators;

	int _correctDigit;

	string[] _displays = new string[] {
	   "", "BLANK", "C", "CEE", "DISPLAY", "FIRST", "HOLD ON", "LEAD", "LED", "LEED", "NO", "NOTHING", "OKAY", "READ", "RED", "REED", "SAYS", "SEE", "THEIR", "THERE", "THEY ARE", "THEY'RE", "UR", "YES", "YOU", "YOU ARE", "YOU'RE", "YOUR"
	};
	string[] _possibleWords = new string[] {
		"BLANK", "DONE", "FIRST", "HOLD", "LEFT", "LIKE", "MIDDLE", "NEXT", "NO", "NOTHING", "OKAY", "PRESS", "READY", "RIGHT", "SURE", "U", "UH HUH", "UH UH", "UHHH", "UR", "WAIT", "WHAT", "WHAT?", "YES", "YOU ARE", "YOU", "YOUR", "YOU'RE" };
	string[][] _chooseableWords = new string[][]
	{
		new string[] { "WAIT", "RIGHT", "OKAY", "MIDDLE", "BLANK" },
		new string[] { "SURE", "UH HUH", "NEXT", "WHAT?", "YOUR", "UR", "YOU'RE", "HOLD", "LIKE", "YOU", "U", "YOU ARE", "UH UH", "DONE" },
		new string[] { "LEFT", "OKAY", "YES", "MIDDLE", "NO", "RIGHT", "NOTHING", "UHHH", "WAIT", "READY", "BLANK", "WHAT", "PRESS", "FIRST" },
		new string[] { "YOU ARE", "U", "DONE", "UH UH", "YOU", "UR", "SURE", "WHAT?", "YOU'RE", "NEXT", "HOLD" },
		new string[] { "RIGHT", "LEFT" },
		new string[] { "YOU'RE", "NEXT", "U", "UR", "HOLD", "DONE", "UH UH", "WHAT?", "UH HUH", "YOU", "LIKE" },
		new string[] { "BLANK", "READY", "OKAY", "WHAT", "NOTHING", "PRESS", "NO", "WAIT", "LEFT", "MIDDLE" },
		new string[] { "WHAT?", "UH HUH", "UH UH", "YOUR", "HOLD", "SURE", "NEXT" },
		new string[] { "BLANK", "UHHH", "WAIT", "FIRST", "WHAT", "READY", "RIGHT", "YES", "NOTHING", "LEFT", "PRESS", "OKAY", "NO" },
		new string[] { "UHHH", "RIGHT", "OKAY", "MIDDLE", "YES", "BLANK", "NO", "PRESS", "LEFT", "WHAT", "WAIT", "FIRST", "NOTHING" },
		new string[] { "MIDDLE", "NO", "FIRST", "YES", "UHHH", "NOTHING", "WAIT", "OKAY" },
		new string[] { "RIGHT", "MIDDLE", "YES", "READY", "PRESS" },
		new string[] { "YES", "OKAY", "WHAT", "MIDDLE", "LEFT", "PRESS", "RIGHT", "BLANK", "READY" },
		new string[] { "YES", "NOTHING", "READY", "PRESS", "NO", "WAIT", "WHAT", "RIGHT" },
		new string[] { "YOU ARE", "DONE", "LIKE", "YOU'RE", "YOU", "HOLD", "UH HUH", "UR", "SURE" },
		new string[] { "UH HUH", "SURE", "NEXT", "WHAT?", "YOU'RE", "UR", "UH UH", "DONE", "U" },
		new string[] { "UH HUH" },
		new string[] { "UR", "U", "YOU ARE", "YOU'RE", "NEXT", "UH UH" },
		new string[] { "READY", "NOTHING", "LEFT", "WHAT", "OKAY", "YES", "RIGHT", "NO", "PRESS", "BLANK", "UHHH" },
		new string[] { "DONE", "U", "UR" },
		new string[] { "UHHH", "NO", "BLANK", "OKAY", "YES", "LEFT", "FIRST", "PRESS", "WHAT", "WAIT" },
		new string[] { "UHHH", "WHAT" },
		new string[] { "YOU", "HOLD", "YOU'RE", "YOUR", "U", "DONE", "UH UH", "LIKE", "YOU ARE", "UH HUH", "UR", "NEXT", "WHAT?" },
		new string[] { "OKAY", "RIGHT", "UHHH", "MIDDLE", "FIRST", "WHAT", "PRESS", "READY", "NOTHING", "YES" },
		new string[] { "YOUR", "NEXT", "LIKE", "UH HUH", "WHAT?", "DONE", "UH UH", "HOLD", "YOU", "U", "YOU'RE", "SURE", "UR", "YOU ARE" },
		new string[] { "SURE", "YOU ARE", "YOUR", "YOU'RE", "NEXT", "UH HUH", "UR", "HOLD", "WHAT?", "YOU" },
		new string[] { "UH UH", "YOU ARE", "UH HUH", "YOUR" },
		new string[] { "YOU", "YOU'RE" }
	};

	string[][] _correctRows = new string[][]
	{
		new string[] { "READY", "MIDDLE", "UH HUH" },
		new string[] { "FIRST", "OKAY", "UH UH" },
		new string[] { "NO", "WAIT", "WHAT?" },
		new string[] { "BLANK", "PRESS", "DONE" },
		new string[] { "NOTHING", "YOU", "NEXT" },
		new string[] { "YES", "YOU ARE", "HOLD" },
		new string[] { "WHAT", "YOUR", "SURE" },
		new string[] { "UHHH", "YOU'RE", "LIKE" },
		new string[] { "LEFT", "UR" },
		new string[] { "RIGHT", "U" },
	};


	string[] _unorderedDisplays = new string[]
	{
		"YES", "FIRST", "DISPLAY", "OKAY", "SAYS", "NOTHING", "", "BLANK", "NO", "LED", "LEAD", "READ", "RED", "REED", "LEED", "HOLD ON", "YOU", "YOU ARE", "YOUR", "YOU'RE", "UR", "THERE", "THEY'RE", "THEIR", "THEY ARE", "SEE", "C", "CEE"
	};
	string[] _allFinalDisplays = new string[4];

	int _buttonPressHolder;
	int _stage = 0;

	public WhosOnFirst(Everything _module, int _modID, int _solvedIndex, KMSelectable[] _buttons, TextMesh[] _buttonTexts, TextMesh _display, MeshRenderer[] _additionalMeshes, GameObject _finalStageIndicators)
	{
		this._module = _module;
		this._modID = _modID;
		this._solvedIndex = _solvedIndex;
		this._buttons = _buttons;
		this._buttonTexts = _buttonTexts;
		this._display = _display;
		this._additionalMeshes = _additionalMeshes;
		this._finalStageIndicators = _finalStageIndicators;
	}

	public override void GeneratePanel()
	{
		string display = _displays[rnd.Range(0, _displays.Length)];
		_display.text = display;
		List<string> words = new List<string>();
		for (int i = 0; i <= 5; i++)
		{
			int rand = rnd.Range(0, _possibleWords.Length);
			while (words.Contains(_possibleWords[rand]))
			{
				rand = rnd.Range(0, _possibleWords.Length);
			}
			words.Add(_possibleWords[rand]);
			_buttonTexts[i].text = _possibleWords[rand];
		}
		string[] row = _chooseableWords[Array.IndexOf(_possibleWords, _buttonTexts[GetPositionFromDisplay(display)].text)];
		string chosenWord = "";
		foreach (string word in row)
		{
			if (_buttonTexts.Any(x => x.text == word))
			{
				chosenWord = word;
				break;
			}
		}
		for (int c = 0; c <= 9; c++)
		{
			string[] r = _correctRows[c];
			if (r.Any(x => x == chosenWord))
			{
				_correctDigit = c;
				break;
			}
		}
		Debug.LogFormat("[Everything #{0}]: The Who's on First panel was generated with the display {1} and the buttons {2}. The correct digit for this panel is: {3}.", _modID, display, words.Join(", "), _correctDigit);
		HandlePanelSolve();
	}

	public override void GenerateFinalPanel()
	{
		_display.text = "";

		List<int> sum = new List<int>();

		for (int i = 0; i <= 3; i++)
		{
			int[] digits = _module.GetCorrectDigits().Select(x => int.Parse(x.ToString())).ToArray();
			sum.Add(digits[i]);
			_allFinalDisplays[i] = _unorderedDisplays[sum.Sum() % 28];
		}
		Debug.LogFormat("[Everything #{0}]: The final panel was generated as Who’s on First. The displays from the numbers are {1}.", _modID, _allFinalDisplays.Join(", "));
		GenerateStage(_allFinalDisplays[_stage]);
	}

	public override void Interact(KMSelectable km)
	{
		int index = Array.IndexOf(_buttons, km);

		if (index != _buttonPressHolder)
		{
			_module.Strike();
			Debug.LogFormat("[Everything #{0}]: Incorrect button press. Expected {1} but was given {2}.", _modID, _buttonTexts[_buttonPressHolder].text, _buttonTexts[index].text);
			return;
		}
		_stage++;
		if (_stage > 3)
		{
			Debug.LogFormat("[Everything #{0}]: All stages have been solved. Module Solved.", _modID);
			_module.GetModule().HandlePass();
			_module._modSolved = true;
			_finalStageIndicators.GetComponentsInChildren<Renderer>()[_stage - 1].material.color = new Color32(43, 158, 41, 255);
			_display.text = "SOLVED!";
			return;
		}
		_finalStageIndicators.GetComponentsInChildren<Renderer>()[_stage - 1].material.color = new Color32(43, 158, 41, 255);
		Debug.LogFormat("[Everything #{0}]: Correct button pressed, generating next stage...", _modID);
		GenerateStage(_allFinalDisplays[_stage]);
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
		foreach (MeshRenderer mr in _additionalMeshes)
		{
			mr.enabled = true;
			yield return new WaitForSeconds(0.1f);
		}
		if (_module.GetFinalState())
		{
			foreach (MeshRenderer mr in _finalStageIndicators.GetComponentsInChildren<Renderer>())
			{
				mr.enabled = true;
				yield return new WaitForSeconds(0.1f);
			}
		}
		_display.GetComponent<Renderer>().enabled = true;
		yield return new WaitForSeconds(0.1f);
		for (int i = 0; i <= 5; i++)
		{
			_buttons[i].GetComponent<Renderer>().enabled = true;
			_buttonTexts[i].GetComponent<Renderer>().enabled = true;
			yield return new WaitForSeconds(0.1f);
		}
		if (_module.GetFinalState())
		{
			foreach (KMHighlightable kh in _buttons.Select(x => x.Highlight))
			{
				kh.gameObject.SetActive(true);
			}
		}
		_module.StartNextPanelAnimation();
		yield break;
	}

	public override IEnumerator DisableComponents()
	{

		if (_module.GetFinalState())
		{
			foreach (KMHighlightable kh in _buttons.Select(x => x.Highlight))
			{
				kh.gameObject.SetActive(false);
			}
		}
		for (int i = 5; i >= 0; i--)
		{
			_buttons[i].GetComponent<Renderer>().enabled = false;
			_buttonTexts[i].GetComponent<Renderer>().enabled = false;
			yield return new WaitForSeconds(0.1f);
		}
		_display.GetComponent<Renderer>().enabled = false;
		yield return new WaitForSeconds(0.1f);
		if (_module.GetFinalState())
		{
			foreach (MeshRenderer mr in _finalStageIndicators.GetComponentsInChildren<Renderer>())
			{
				mr.enabled = true;
				yield return new WaitForSeconds(0.1f);
			}
		}
		foreach (MeshRenderer mr in _additionalMeshes)
		{
			mr.enabled = false;
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
		Debug.LogFormat("[Everything #{0}]: After the recent panel solve, there are {1} panels left to solve.", _modID, 4 - _module.GetSolvedPanels().Select(x => x).Count());
	}

	public override Vector3 GetBaseSize()
	{
		return new Vector3(0.11f, 0.01f, 0.085f);
	}

	int GetPositionFromDisplay(string display)
	{
		switch (display)
		{
			case "UR":
				return 0;
			case "C":
			case "FIRST":
			case "OKAY":
				return 1;
			case "LED":
			case "NOTHING":
			case "THEY ARE":
			case "YES":
				return 2;
			case "BLANK":
			case "READ":
			case "RED":
			case "THEIR":
			case "YOU":
			case "YOU'RE":
			case "YOUR":
				return 3;
			case "":
			case "LEED":
			case "REED":
			case "THEY'RE":
				return 4;
			case "CEE":
			case "DISPLAY":
			case "HOLD ON":
			case "LEAD":
			case "NO":
			case "SAYS":
			case "SEE":
			case "THERE":
			case "YOU ARE":
				return 5;
			default:
				return -1;
		}
	}

	void GenerateStage(string display)
	{
		List<string> words = new List<string>();
		for (int i = 0; i <= 5; i++)
		{
			int rand = rnd.Range(0, _possibleWords.Length);
			while (words.Contains(_possibleWords[rand]))
			{
				rand = rnd.Range(0, _possibleWords.Length);
			}
			words.Add(_possibleWords[rand]);
			_buttonTexts[i].text = _possibleWords[rand];
		}
		string[] row = _chooseableWords[Array.IndexOf(_possibleWords, _buttonTexts[GetPositionFromDisplay(display)].text)];
		string chosenWord = "";
		foreach (string word in row)
		{
			if (_buttonTexts.Any(x => x.text == word))
			{
				chosenWord = word;
				break;
			}
		}

		_buttonPressHolder = Array.IndexOf(_buttonTexts.Select(x => x.text).ToArray(), chosenWord);

		Debug.LogFormat("[Everything #{0}]: The display for this WoF stage is: {1}. The correct button to press is {2}.", _modID, display, chosenWord);
	}

}
