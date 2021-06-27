using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public class PianoKeys : PanelInterface {

	Everything _module;
	int _modID;
	int _solvedIndex;
	TextMesh _symbolText;
	MeshRenderer _symbolDisplay;
	KMSelectable[] _keys;
	MeshRenderer[] _keyRenders;

	int _correctDigit;

	char[] _pianoKeyCharacters = new char[] { 'b', 'c', '#', 'n', 'U', 'C', 'T', 'B', 'm' };
	string[] _pianoKeyNotes = new string[] { "C", "C#/Db", "D", "D#/Eb", "E", "F", "F#/Gb", "G", "G#/Ab", "A", "A#/Bb", "B" };
	int[][] _pianoKeySequences = new int[][]
	{
		// C 0 - C#/Db 1 - D 2 - D#/Eb 3 - E 4 - F 5 - F#/Gb 6 - G 7 - G#/Ab 8 - A 9 - A#/Bb 10 - B 11
		new int[] { 11, 2, 9, 7, 9, 11, 2, 9 }, // B D A G A B D A
		new int[] { 10, 10, 10, 10, 6, 8, 10, 8, 10 }, // Bb Bb Bb Bb Gb Ab Bb Ab Bb
		new int[] { 3, 3, 2, 2, 3, 3, 2, 3, 3, 2, 2, 3 }, // Eb Eb D D Eb Eb D Eb Eb D D Eb
		new int[] { 4, 6, 6, 6, 6, 4, 4, 4 }, // E F# F# F# F# E E E
		new int[] { 10, 9, 10, 5, 3, 10, 9, 10, 5, 3 }, // Bb A Bb F Eb Bb A Bb F Eb
		new int[] { 4, 4, 4, 0, 4, 7, 7 }, // E E E C E G G
		new int[] { 1, 2, 4, 5, 1, 2, 4, 5, 10, 9 }, // C# D E F C# D E F Bb A
		new int[] { 7, 7, 0, 7, 7, 0, 7, 0 }, // G G C G G C G C
		new int[] { 9, 4, 5, 7, 5, 4, 2, 2, 5, 9 }, // A E F G F E D D F A
		new int[] { 7, 7, 7, 3, 10, 7, 3, 10, 7 }, // G G G Eb Bb G Eb Bb G
	};

	char[] _chosenCharacters = new char[3];

	List<int[]> _finalSequences;
	int[] _currentSequence;
	int _sequenceIndex = 0;
	int _currentSequenceIndex = 0;

	public PianoKeys(Everything _module, int _modID, int _solvedIndex, TextMesh _symbolText, MeshRenderer _symbolDisplay, KMSelectable[] _keys, MeshRenderer[] _keyRenders)
	{
		this._module = _module;
		this._modID = _modID;
		this._solvedIndex = _solvedIndex;
		this._symbolText = _symbolText;
		this._symbolDisplay = _symbolDisplay;
		this._keys = _keys;
		this._keyRenders = _keyRenders;
	}

	public override void GeneratePanel()
	{

		for (int i = 0; i <= 2; i++) 
		{
			int rndChar = rnd.Range(0, _pianoKeyCharacters.Length);

			while (_chosenCharacters.Any(x => x == _pianoKeyCharacters[rndChar])) 
			{
				rndChar = rnd.Range(0, _pianoKeyCharacters.Length);
			}

			_chosenCharacters[i] = _pianoKeyCharacters[rndChar];
		}
		
		_correctDigit = GetAnswer();
		_symbolText.text = _chosenCharacters[0] + "      " + _chosenCharacters[1] + "      " + _chosenCharacters[2];

		Debug.LogFormat("[Everything #{0}]: The Piano Keys panel was generated with {1} as the characters. The correct digit for this panel is: {2}.", _modID, _chosenCharacters.Select(x => "'" + GetName(x) + "'").Join(", "), _correctDigit);
		HandlePanelSolve();
	}

	public override void GenerateFinalPanel()
	{
		int[] digits = _module.GetCorrectDigits().Select(x => int.Parse(x.ToString())).ToArray();

		_finalSequences = digits.Select(x => _pianoKeySequences[x]).ToList();

		_currentSequence = _finalSequences[_currentSequenceIndex];

		Debug.LogFormat("[Everything #{0}]: The final panel was generated as Piano Keys. The sequences that were chosen were: {1}.", _modID, _finalSequences.Select(x => x.Select(y => _pianoKeyNotes[y]).Join(", ")).Join(" | "));
	}

	public override void Interact(KMSelectable km)
	{
		int index = Array.IndexOf(_keys, km);

		if (index != _currentSequence[_currentSequenceIndex]) 
		{
			Debug.LogFormat("[Everything #{0}]: Incorrect key was pressed. Expected {1} but was given {2}.", _modID, _pianoKeyNotes[_currentSequence[_currentSequenceIndex]], _pianoKeyNotes[index]);
			_currentSequenceIndex = 0;
			_module.Strike();
			return;
		}

		_currentSequenceIndex++;

		if (_currentSequenceIndex == _currentSequence.Length) 
		{
			_sequenceIndex++;
			Debug.LogFormat("[Everything #{0}]: Sequence {1} has been inputted correctly.", _modID, _sequenceIndex);
			if (_sequenceIndex == 4) 
			{
				Debug.LogFormat("[Everything #{0}]: All seqences have been inputted correctly. Module Solved.", _modID);
				_module.GetModule().HandlePass();
				_module._modSolved = true;
				return;
			}
			_currentSequence = _finalSequences[_sequenceIndex];
			_currentSequenceIndex = 0;
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
		if (_module.GetFinalState())
		{
			// Keys
			foreach (MeshRenderer mr in _keyRenders) 
			{
				mr.enabled = true;
				yield return new WaitForSeconds(0.075f);
			}
			_module.StartNextPanelAnimation();
			yield break;
		}
		// Symbols
		_symbolDisplay.enabled = true;
		yield return new WaitForSeconds(0.25f);
		_symbolText.GetComponent<Renderer>().enabled = true;
		_module.StartNextPanelAnimation();
		yield break;
	}

	public override IEnumerator DisableComponents()
	{
		if (_module.GetFinalState())
		{
			// Keys
			foreach (MeshRenderer mr in _keyRenders.Reverse())
			{
				mr.enabled = false;
				yield return new WaitForSeconds(0.075f);
			}
			_module.StartNextPanelAnimation();
			yield break;
		}
		// Symbols
		_symbolText.GetComponent<Renderer>().enabled = false;
		yield return new WaitForSeconds(0.25f);
		_symbolDisplay.enabled = false;
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

	int GetAnswer() 
	{
		if (_chosenCharacters.Contains('b') && _module.GetBombInfo().GetSerialNumberNumbers().Last() % 2 == 0)
		{
			return 1;
		}
		else if ((_chosenCharacters.Contains('c') || _chosenCharacters.Contains('#')) && _module.GetBombInfo().GetBatteryHolderCount() >= 2)
		{
			return 2;
		}
		else if (_chosenCharacters.Contains('n') && _chosenCharacters.Contains('U'))
		{
			return 3;
		}
		else if ((_chosenCharacters.Contains('C') || _chosenCharacters.Contains('T')) && _module.GetBombInfo().IsPortPresent(Port.StereoRCA))
		{
			return 4;
		}
		else if (_chosenCharacters.Contains('B') && _module.GetBombInfo().IsIndicatorOn(Indicator.SND))
		{
			return 5;
		}
		else if ((_chosenCharacters.Contains('m') || _chosenCharacters.Contains('U') || _chosenCharacters.Contains('c')) && _module.GetBombInfo().GetBatteryCount() >= 3)
		{
			return 6;
		}
		else if (_chosenCharacters.Contains('b') && _chosenCharacters.Contains('#'))
		{
			return 7;
		}
		else if ((_chosenCharacters.Contains('C') || _chosenCharacters.Contains('m')) && _module.GetBombInfo().GetSerialNumberNumbers().Any(x => new int[] { 3, 7, 8 }.Any(y => y == x)))
		{
			return 8;
		}
		else if (_chosenCharacters.Contains('n') || _chosenCharacters.Contains('T') || _chosenCharacters.Contains('B'))
		{
			return 9;
		}
		else
		{
			return 0;
		}
	}

	string GetName(char c) 
	{
		switch (c) 
		{
			case 'b':
				return "flat";
			case 'c':
				return "common time";
			case '#':
				return "sharp";
			case 'n':
				return "natural";
			case 'U':
				return "fermata";
			case 'C':
				return "cut time";
			case 'T':
				return "turn";
			case 'B':
				return "alto clef";
			case 'm':
				return "mordent";
			default:
				return null;
		}
	}

}
