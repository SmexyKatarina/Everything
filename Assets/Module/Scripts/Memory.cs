using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public class Memory : PanelInterface
{

	Everything _module;
	int _modID;
	int _solvedIndex;
	KMSelectable[] _buttons;
	TextMesh[] _buttonTexts;
	TextMesh _display;
	MeshRenderer[] _additionalMeshes;
	TextMesh _countdownText;

	int _correctDigit;

	int[] _stageNumbers = new int[5];
	int[][] _stageLabels = new int[5][];
	List<int> _correctPositions = new List<int>();
	List<int> _correctLabels = new List<int>();
	int _stage = 0;
	int _timer = 16;

	bool _memoryBeingUpdated;

	Color32 unlit = new Color32(19, 19, 19, 255);
	Color32 lit = new Color32(15, 197, 0, 255);

	public Memory(Everything _module, int _modID, int _solvedIndex, KMSelectable[] _buttons, TextMesh[] _buttonTexts, TextMesh _display, MeshRenderer[] _additionalMeshes, TextMesh _countdownText)
	{
		this._module = _module;
		this._modID = _modID;
		this._solvedIndex = _solvedIndex;
		this._buttons = _buttons;
		this._buttonTexts = _buttonTexts;
		this._display = _display;
		this._additionalMeshes = _additionalMeshes;
		this._countdownText = _countdownText;
	}

	public override void GeneratePanel()
	{
		for (int i = 0; i <= 4; i++)
		{
			_stageNumbers[i] = rnd.Range(1, 5);
			int[] labels = new int[] { 1, 2, 3, 4 };
			labels.Shuffle();
			_stageLabels[i] = labels;
		}
		foreach (int stage in _stageNumbers)
		{
			int[] press = GetCorrectPress(stage);
			_correctPositions.Add(press[0]);
			_correctLabels.Add(press[1]);
			_stage++;
		}
		_stage = 0;
		int base5 = int.Parse(_correctLabels.Join(""));
		string base10 = ConvertToBase10(_correctLabels.ToArray()).ToString();

		_correctDigit = int.Parse(base10[base10.Length - 1].ToString());

		_display.text = _stageNumbers[0].ToString();
		for (int i = 0; i <= 3; i++)
		{
			_buttonTexts[i].text = _stageLabels[0][i].ToString();
		}

		Debug.LogFormat("[Everything #{0}]: The Memory panel was generated with the displays {1} and the labels {2}. The base-5 number is {3} and the base-10 number is {4}. The correct digit for this panel is: {5}.", _modID, _stageNumbers.Join(", "), _stageLabels.Select(x => x.Join(", ")).Join(" | "), base5, base10, _correctDigit);
	}

	public override void GenerateFinalPanel()
	{

		int[] digits = (_module.GetCorrectDigits() + _module.GetBombInfo().GetSerialNumberNumbers().Last().ToString()).Select(x => int.Parse(x.ToString())).ToArray();

		for (int i = 2; i <= 6; i++)
		{
			_additionalMeshes[i].material.color = unlit;
		}

		_stage = 0;
		_correctPositions.Clear();
		_correctLabels.Clear();
		_display.text = "";

		for (int i = 0; i <= 4; i++)
		{
			_stageNumbers[i] = (digits[i] % 4) + 1;
			int[] labels = new int[] { 1, 2, 3, 4 };
			labels.Shuffle();
			_stageLabels[i] = labels;
		}

		foreach (int stage in _stageNumbers)
		{
			int[] press = GetCorrectPress(stage);
			_correctPositions.Add(press[0]);
			_correctLabels.Add(press[1]);
			_stage++;
		}

		_stage = 0;

		for (int i = 0; i <= 3; i++)
		{
			_buttonTexts[i].text = _stageLabels[0][i].ToString();
		}

		Debug.LogFormat("[Everything #{0}]: The final panel was generated as Memory. The displays for the stages are {1}. The labels for each stage are {2}. The correct button presses are {3}.", _modID, _stageNumbers.Join(", "), _stageLabels.Select(x => x.Join(", ")).Join(" | "), _correctPositions.Select(x => x + 1).Join(", "));
	}

	public override void Interact(KMSelectable km)
	{
		int correct = _correctPositions[_stage];
		int pos = Array.IndexOf(_buttons, km);
		if (_memoryBeingUpdated) return;
		if (correct != pos)
		{
			_module.Strike();
			Debug.LogFormat("[Everything #{0}]: Incorrect button press on Memory. Pressed position {1} but expected position {2}.", _modID, pos + 1, correct + 1);
			return;
		}
		else
		{
			_additionalMeshes[_stage + 2].material.color = lit;
			_stage++;
			if (_module.GetFinalState() && _stage == 5)
			{
				_module.GetModule().HandlePass();
				_module._modSolved = true;
				Debug.LogFormat("[Everything #{0}]: All buttons have been pressed correctly. Module solved.", _modID);
				return;
			}

			_module.StartCoroutine(UpdateStage(0.25f, 0.25f, 0.35f, _stageNumbers[_stage], _stageLabels[_stage]));

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
		foreach (MeshRenderer mr in _additionalMeshes)
		{
			mr.enabled = true;
			yield return new WaitForSeconds(0.1f);
		}
		_display.GetComponent<Renderer>().enabled = true;
		yield return new WaitForSeconds(0.1f);
		for (int i = 0; i <= 3; i++)
		{
			_buttons[i].GetComponent<Renderer>().enabled = true;
			_buttonTexts[i].GetComponent<Renderer>().enabled = true;
			yield return new WaitForSeconds(0.1f);
		}
		foreach (KMHighlightable kh in _buttons.Select(x => x.Highlight))
		{
			kh.gameObject.SetActive(true);
		}
		_module.StartNextPanelAnimation();
		yield break;
	}

	public override IEnumerator DisableComponents()
	{
		foreach (KMHighlightable kh in _buttons.Select(x => x.Highlight))
		{
			kh.gameObject.SetActive(false);
		}
		for (int i = 3; i >= 0; i--)
		{
			_buttons[i].GetComponent<Renderer>().enabled = false;
			_buttonTexts[i].GetComponent<Renderer>().enabled = false;
			yield return new WaitForSeconds(0.1f);
		}
		_display.GetComponent<Renderer>().enabled = false;
		yield return new WaitForSeconds(0.1f);
		foreach (MeshRenderer mr in _additionalMeshes.Reverse())
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
			else if (z > baseSize.z) { baseTrans.localScale = new Vector3(x, y, z - 0.001f); } // 10-15 -0.06 -0.03
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

	IEnumerator UpdateStage(float butDelay, float disDelay, float loadNewDelay, int stageNum, int[] stageButtons)
	{
		_memoryBeingUpdated = true;
		_display.text = "";
		yield return new WaitForSeconds(disDelay);
		for (int i = 3; i >= 0; i--)
		{
			_buttons[i].GetComponent<Renderer>().enabled = false;
			_buttons[i].Highlight.gameObject.SetActive(false);
			_buttonTexts[i].text = "";
			yield return new WaitForSeconds(butDelay);
		}

		yield return new WaitForSeconds(loadNewDelay);

		for (int i = 0; i <= 3; i++)
		{
			_buttons[i].GetComponent<Renderer>().enabled = true;
			if (_module.GetFinalState())
			{
				_buttons[i].Highlight.gameObject.SetActive(true);
			}
			else if (!_module.GetFinalState())
			{
				if (_stage != 4)
				{
					_buttons[i].Highlight.gameObject.SetActive(true);
				}
				else
				{
					_buttons[i].Highlight.gameObject.SetActive(false);
				}
			}
			_buttonTexts[i].text = stageButtons[i].ToString();
			yield return new WaitForSeconds(butDelay);
		}
		yield return new WaitForSeconds(disDelay);
		if (!_module.GetFinalState()) _display.text = stageNum.ToString();
		if (_stage == 4 && !_module.GetFinalState())
		{ 
			_module.StartCoroutine(DelayPanelSolve());
		}
		_memoryBeingUpdated = false;
		yield break;
	}

	IEnumerator DelayPanelSolve()
	{
		_module._isAnimating = true;
		_countdownText.GetComponent<Renderer>().enabled = true;
		while (_timer != 0)
		{
			_timer--;
			_countdownText.text = _timer.ToString();
			yield return new WaitForSeconds(1.0f);
		}
		_countdownText.GetComponent<Renderer>().enabled = false;
		HandlePanelSolve();
		_module._isAnimating = false;
		yield break;
	}

	// Get the correct Memory press based on stage and stage display.
	// Returns in format of "position, label"
	int[] GetCorrectPress(int stageNum)
	{
		int pos0 = _stageLabels[_stage][0];
		int pos1 = _stageLabels[_stage][1];
		int pos2 = _stageLabels[_stage][2];
		int pos3 = _stageLabels[_stage][3];
		int pof4 = Array.IndexOf(_stageLabels[_stage], 4);
		switch (_stage)
		{
			case 0:
				switch (stageNum)
				{
					case 1:
						return new int[] { 1, pos1 };
					case 2:
						return new int[] { 1, pos1 };
					case 3:
						return new int[] { 2, pos2 };
					case 4:
						return new int[] { 3, pos3 };
					default:
						return null;
				}
			case 1:
				switch (stageNum)
				{
					case 1:
						return new int[] { pof4, 4 };
					case 2:
						return new int[] { _correctPositions[0], _stageLabels[_stage][_correctPositions[0]] };
					case 3:
						return new int[] { 0, pos0 };
					case 4:
						return new int[] { _correctPositions[0], _stageLabels[_stage][_correctPositions[0]] };
					default:
						return null;
				}
			case 2:
				switch (stageNum)
				{
					case 1:
						return new int[] { Array.IndexOf(_stageLabels[_stage], _correctLabels[1]), _correctLabels[1] };
					case 2:
						return new int[] { Array.IndexOf(_stageLabels[_stage], _correctLabels[0]), _correctLabels[0] };
					case 3:
						return new int[] { 2, pos2 };
					case 4:
						return new int[] { pof4, 4 };
					default:
						return null;
				}
			case 3:
				switch (stageNum)
				{
					case 1:
						return new int[] { _correctPositions[0], _stageLabels[_stage][_correctPositions[0]] };
					case 2:
						return new int[] { 0, pos0 };
					case 3:
						return new int[] { _correctPositions[1], _stageLabels[_stage][_correctPositions[1]] };
					case 4:
						return new int[] { _correctPositions[1], _stageLabels[_stage][_correctPositions[1]] };
					default:
						return null;
				}
			case 4:
				switch (stageNum)
				{
					case 1:
						return new int[] { Array.IndexOf(_stageLabels[_stage], _correctLabels[0]), _correctLabels[0] };
					case 2:
						return new int[] { Array.IndexOf(_stageLabels[_stage], _correctLabels[1]), _correctLabels[1] };
					case 3:
						return new int[] { Array.IndexOf(_stageLabels[_stage], _correctLabels[3]), _correctLabels[3] };
					case 4:
						return new int[] { Array.IndexOf(_stageLabels[_stage], _correctLabels[2]), _correctLabels[2] };
					default:
						return null;
				}
			default:
				return null;
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
