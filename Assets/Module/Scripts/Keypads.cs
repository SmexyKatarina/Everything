using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public class Keypads : PanelInterface
{

	Everything _module;
	int _modID;
	int _solvedIndex;
	KMSelectable[] _buttons;
	TextMesh[] _buttonTexts;
	MeshRenderer[] _buttonMeshes;

	private static char[][] _keypadColumns = new char[][]{
		new char[]{'Ϙ', 'Ѧ', 'ƛ', 'Ϟ', 'Ѭ', 'ϗ', 'Ͽ'},
		new char[]{'Ӭ', 'Ϙ', 'Ͽ', 'Ҩ', '☆', 'ϗ', '¿'},
		new char[]{'©', 'Ѽ', 'Ҩ', 'Җ', 'Ԇ', 'ƛ', '☆'},
		new char[]{'Ϭ', '¶', 'Ѣ', 'Ѭ', 'Җ', '¿', 'ټ'},
		new char[]{'ψ', 'ټ', 'Ѣ', 'Ͼ', '¶', 'Ѯ', '★'},
		new char[]{'Ϭ', 'Ӭ', '҂', 'æ', 'ψ', 'Ҋ', 'Ω'}
	};

	char[] _chosenColumn;
	int _correctDigit;

	List<List<int>> _correctFinalPresses = new List<List<int>>();
	int _currentColumn;
	List<int> _currentList;

	public Keypads(Everything _module, int _modID, int _solvedIndex, KMSelectable[] _buttons, TextMesh[] _buttonTexts, MeshRenderer[] _buttonMeshes)
	{
		this._module = _module;
		this._modID = _modID;
		this._buttons = _buttons;
		this._buttonTexts = _buttonTexts;
		this._solvedIndex = _solvedIndex;
		this._buttonMeshes = _buttonMeshes;
	}

	public override void GeneratePanel()
	{
		int column = rnd.Range(0, 6);
		_chosenColumn = _keypadColumns[column];

		List<int> symbols = new List<int>();
		for (int i = 0; i <= 3; i++)
		{
			int symbol = rnd.Range(0, _chosenColumn.Length);
			while (symbols.Contains(symbol))
			{
				symbol = rnd.Range(0, _chosenColumn.Length);
			}
			symbols.Add(symbol);
		}
		symbols.Shuffle();
		List<int> positions = new List<int> { 0, 1, 2, 3 };
		char[] chars = new char[4];
		positions.Shuffle();
		int count = 0;
		foreach (int i in symbols)
		{
			int pos = positions[count];
			_buttonTexts[pos].text = _chosenColumn[i].ToString();
			chars[pos] = _chosenColumn[i];
			count++;
		}

		symbols.Sort();
		List<int> correctPresses = new List<int>();
		foreach (int i in symbols)
		{
			char c = _chosenColumn[i];
			int pos = Array.IndexOf(chars, c) + 1;
			correctPresses.Add(pos);
		}
		char[] base10 = ConvertToBase10(correctPresses.ToArray()).ToString().ToCharArray();
		_correctDigit = int.Parse(base10[base10.Length - 1].ToString());
		Debug.LogFormat("[Everything #{0}]: The Keypads panel was generated with the characters from column {1} in order on the buttons as {2}. The base-5 number is {3} which in base-10 results to {4}. The correct digit for this panel is: {5}.", _modID, column + 1, chars.Join(", "), correctPresses.Join(""), ConvertToBase10(correctPresses.ToArray()), _correctDigit);
		HandlePanelSolve();
	}

	public override void GenerateFinalPanel()
	{
		int[] digits = _module.GetCorrectDigits().Select(x => int.Parse(x.ToString())).ToArray();

		for (int i = 0; i <= 3; i++)
		{
			int m = digits[i];
			m %= 6;
			while (i >= 1)
			{
				int[] dig = digits.Join("").Substring(0, i).Select(x => int.Parse(x.ToString())).ToArray();
				while (dig.Any(x => x == m))
				{
					m++;
					if (m == 6)
					{
						m = 0;
					}
				}
				break;
			}
			digits[i] = m;
		}
		List<int> chosenPositions = new List<int>();
		List<char> chosenCharacters = new List<char>();
		foreach (int col in digits)
		{
			char[] column = _keypadColumns[col];

			for (int i = 0; i <= 3; i++)
			{
				int pos = rnd.Range(4, 20);
				while (chosenPositions.Any(x => x == pos))
				{
					pos = rnd.Range(4, 20);
				}
				int symbol = rnd.Range(0, 7);
				chosenPositions.Add(pos);
				chosenCharacters.Add(column[symbol]);
				_buttonTexts[pos].text = column[symbol].ToString();
			}
		}
		List<int> selectedAlready = new List<int>();

		for (int i = 0; i <= 3; i++)
		{
			char[] column = _keypadColumns[digits[i]];
			List<int> correctPositionsColumn = new List<int>();
			foreach (char c in column)
			{
				for (int x = 0; x <= 15; x++)
				{
					if (chosenCharacters[x] == c && !selectedAlready.Contains(x))
					{
						selectedAlready.Add(x);
						correctPositionsColumn.Add(chosenPositions[x]);
					}
				}
			}
			_correctFinalPresses.Add(correctPositionsColumn);
		}

		_currentColumn = 0;
		_currentList = _correctFinalPresses[_currentColumn].Select(x => x - 3).ToList();

		Debug.LogFormat("[Everything #{0}]: The final panel was generated as Keypads, the chosen columns (from left to right) are: {1}. The correct button order to press in is: {2}.", _modID, digits.Select(x => x + 1).Join(", "), _correctFinalPresses.Select(x => x.Select(y => y - 3).Join(", ")).Join(" | "));

	}

	public override void Interact(KMSelectable km)
	{
		if (_module._isAnimating || _module._modSolved) { return; }

		int index = Array.IndexOf(_buttons, km);
		km.AddInteractionPunch();
		_module._audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, km.transform);
		StringBuilder sb = new StringBuilder();
		if (!_currentList.Contains(index + 1))
		{
			_module.Strike();
			for (int i = 0; i < _currentList.Count(); i++)
			{
				if (i == _currentList.Count() - 1 && _currentList.Count() > 1)
				{
					sb.Append("or " + _currentList[i]);
					continue;
				}
				sb.Append(_currentList[i] + ", ");
			}
			Debug.LogFormat("[Everything #{0}]: Keypads unintended button pressed. Expected {1} but was given {2}.", _modID, _currentList.Count() == 1 ? _currentList[0].ToString() : sb.ToString(), index + 1);
			return;
		}
		// 92 167 93
		km.GetComponent<Renderer>().material.color = new Color32(92, 167, 93, 255);

		_currentList.Remove(index + 1);

		if (_currentList.Count() == 0)
		{
			_currentColumn++;
			if (_currentColumn > 3)
			{
				_module.GetModule().HandlePass();
				_module._modSolved = true;
				Debug.LogFormat("[Everything #{0}]: All buttons have been correctly pressed. Module solved.", _modID);
				return;
			}
			_currentList = _correctFinalPresses[_currentColumn].Select(x => x - 3).ToList();
		}

		for (int i = 0; i < _currentList.Count(); i++)
		{
			if (i == _currentList.Count() - 1 && _currentList.Count() > 1)
			{
				sb.Append("or " + _currentList[i]);
				continue;
			}
			sb.Append(_currentList[i] + ", ");
		}

		Debug.LogFormat("[Everything #{0}]: Correct button pressed expecting {1} next.", _modID, _currentList.Count() == 1 ? _currentList[0].ToString() : sb.ToString());
	}

	public override void InteractEnd()
	{
		if (_module._isAnimating || _module._modSolved) { return; }
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
			for (int i = 0; i < _buttons.Length; i++)
			{
				_buttons[i].GetComponent<Renderer>().enabled = true;
				_buttonTexts[i + 4].GetComponent<Renderer>().enabled = true;
				yield return new WaitForSeconds(0.05f);
			}
			foreach (KMHighlightable kmh in _buttons.Select(x => x.Highlight))
			{
				kmh.gameObject.SetActive(true);
			}
			_module.StartNextPanelAnimation();
			yield break;
		}
		for (int i = 0; i < _buttonMeshes.Length; i++)
		{
			_buttonMeshes[i].enabled = true;
			_buttonTexts[i].GetComponent<Renderer>().enabled = true;
			yield return new WaitForSeconds(0.1f);
		}
		_module.StartNextPanelAnimation();
		yield break;
	}

	public override IEnumerator DisableComponents()
	{

		if (_module.GetFinalState())
		{
			foreach (KMHighlightable kmh in _buttons.Select(x => x.Highlight))
			{
				kmh.gameObject.SetActive(false);
			}
			for (int i = _buttons.Length - 1; i >= 0; i--)
			{
				_buttons[i].GetComponent<Renderer>().enabled = false;
				_buttonTexts[i + 4].GetComponent<Renderer>().enabled = false;
				yield return new WaitForSeconds(0.05f);
			}
			_module.StartNextPanelAnimation();
			yield break;
		}
		for (int i = _buttonMeshes.Length - 1; i >= 0; i--)
		{
			_buttonMeshes[i].enabled = false;
			_buttonTexts[i].GetComponent<Renderer>().enabled = false;
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
		if (_module.GetFinalState()) return new Vector3(0.12f, 0.01f, 0.12f);
		return new Vector3(0.09f, 0.01f, 0.09f);
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
