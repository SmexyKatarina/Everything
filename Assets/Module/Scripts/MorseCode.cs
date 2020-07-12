using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public class MorseCode : PanelInterface
{

	Everything _module;
	int _modID;
	int _solvedIndex;
	MeshRenderer _morseLight;
	GameObject _finalObject;
	KMSelectable[] _morseButtons;

	Color32 unlit = new Color32(69, 69, 69, 255);
	Color32 lit = new Color32(173, 173, 173, 255);

	Coroutine c;

	int _correctDigit;

	string[] _possibleWords = new string[] { "shell:3.505", "halls:3.515", "slick:3.522", "trick:3.532", "boxes:3.535", "leaks:3.542", "strobe:3.545", "bistro:3.552", "flick:3.555", "bombs:3.565", "break:3.572", "brick:3.575", "steak:3.582", "sting:3.592", "beats:3.600" };

	string[] _morseCodes = new string[] { "01", "1000", "1010", "100", "0", "0010", "110", "0000", "00", "0111", "101", "0100", "11", "10", "111", "0110", "1101", "010", "000", "1", "001", "0001", "011", "1001", "1011", "1100", "01111", "00111", "00011", "00001", "00000", "10000", "11000", "11100", "11110", "11111" };

	List<string> _morseCodeForWord = new List<string>();
	string[] _chosenWord;

	List<int> _multipliedNumbers = new List<int>();

	int _selectedNumber = 0;
	List<float> _selectedFrequences = new List<float>();
	List<string> _morseCodeForNumber = new List<string>();

	int _freqIndex = 0;

	public MorseCode(Everything _module, int _modID, int _solvedIndex, MeshRenderer _morseLight, KMSelectable[] _morseButtons, GameObject _finalObject)
	{
		this._module = _module;
		this._modID = _modID;
		this._solvedIndex = _solvedIndex;
		this._morseLight = _morseLight;
		this._morseButtons = _morseButtons;
		this._finalObject = _finalObject;
	}

	public override void GeneratePanel()
	{
		_chosenWord = _possibleWords[rnd.Range(0, _possibleWords.Length)].Split(':');
		char[] alphabet = Enumerable.Range(0, 26).Select(x => (char)(x + 'a')).ToArray();
		foreach (char c in _chosenWord[0])
		{
			_morseCodeForWord.Add(_morseCodes[Array.IndexOf(alphabet, c)]);
		}

		_correctDigit = int.Parse(_chosenWord[1][3].ToString());

		Debug.LogFormat("[Everything #{0}]: The Morse Code panel was generated with the word {1}. The frequency for this word is {2}. The correct digit for this panel is: {3}.", _modID, _chosenWord[0].ToUpper(), _chosenWord[1], _correctDigit);
		HandlePanelSolve();
	}

	public override void GenerateFinalPanel()
	{
		int digits = int.Parse(_module.GetCorrectDigits());

		int rndNumber = rnd.Range(1000, 10000);
		foreach (char c in rndNumber.ToString())
		{
			char[] nums = new char[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };
			_morseCodeForNumber.Add(_morseCodes[26 + Array.IndexOf(nums, c)]);
		}

		_multipliedNumbers = (digits + rndNumber).ToString().Replace("0", "").Select(x => int.Parse(x.ToString())).ToList();
		int size = _multipliedNumbers.Count() - 1;
		for (int i = 0; i < size; i++)
		{
			int num = _multipliedNumbers[0];
			_multipliedNumbers[0] = num * _multipliedNumbers[1];
			_multipliedNumbers.RemoveAt(1);
		}

		_selectedNumber = _multipliedNumbers[0] % 100;

		int[] allFreq = new int[_possibleWords.Length];
		int[] allDiff = new int[_possibleWords.Length];
		int count = 0;
		foreach (string word in _possibleWords)
		{
			string[] split = word.Split(':');
			if (count == _possibleWords.Length - 1)
			{
				allFreq[count] = 100;
				allDiff[count] = Math.Abs(_selectedNumber - allFreq[count]);
				continue;
			}
			allFreq[count] = int.Parse(split[1][3].ToString() + split[1][4].ToString());
			allDiff[count] = Math.Abs(_selectedNumber - allFreq[count]);
			count++;
		}
		List<int> sortedDiff = allDiff.ToList();
		sortedDiff.Sort();
		for (int i = 0; i < allDiff.Length; i++)
		{
			if (allDiff[i] == sortedDiff[0])
			{
				_selectedFrequences.Add(float.Parse(_possibleWords[i].Split(':')[1]));
			}
		}
		Debug.LogFormat("[Everything #{0}]: The final panel was generated as Morse Code. The morse light is flashing {1}. The new number (removing all 0's) is {2}. The possible frequencies to submit is: {3}", _modID, rndNumber, _multipliedNumbers.Join(""), _selectedFrequences.Join(", "));
	}

	public override void Interact(KMSelectable km)
	{
		int index = Array.IndexOf(_morseButtons, km);
		TextMesh freq = _finalObject.GetComponentsInChildren<TextMesh>().Where(x => x.gameObject.name == "FrequencyText").ToList()[0];
		switch (index)
		{
			case 0:
				if (_freqIndex == 0) return;
				_freqIndex--;
				freq.text = _possibleWords[_freqIndex].Split(':')[1];
				break;
			case 1:
				if (_freqIndex == _possibleWords.Length - 1) return;
				_freqIndex++;
				freq.text = _possibleWords[_freqIndex].Split(':')[1];
				break;
			case 2:
				if (!_selectedFrequences.Contains(float.Parse(freq.text)))
				{
					_module.Strike();
					Debug.LogFormat("[Everything #{0}]: The frequency of {1} is incorrect, expecting one of the following: {2}.", _modID, freq.text, _selectedFrequences.Join(", "));
					return;
				}
				else
				{
					Debug.LogFormat("[Everything #{0}]: Correct frequency selected. Module solved.", _modID);
					_module.GetModule().HandlePass();
					_module._modSolved = true;
					freq.text = "SOLVED!";
					_module.StopCoroutine(c);
					c = null;
					_morseLight.material.color = unlit;
					return;
				}
			default:
				break;
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
		_morseLight.material.color = unlit;
		_morseLight.enabled = true;
		if (_module.GetFinalState())
		{
			TextMesh freq = _finalObject.GetComponentsInChildren<TextMesh>().Where(x => x.gameObject.name == "FrequencyText").ToList()[0];
			freq.text = _possibleWords[_freqIndex].Split(':')[1];
			foreach (MeshRenderer mr in _finalObject.GetComponentsInChildren<MeshRenderer>())
			{
				mr.enabled = true;
				yield return new WaitForSeconds(.1f);
			}

		}
		yield return new WaitForSeconds(1.5f);
		if (_module.GetFinalState())
		{
			foreach (KMSelectable km in _finalObject.GetComponentsInChildren<KMSelectable>())
			{
				km.Highlight.gameObject.SetActive(true);
			}
			c = _module.StartCoroutine(SendMorse(_morseCodeForNumber));
		}
		else
		{
			c = _module.StartCoroutine(SendMorse(_morseCodeForWord));
		}
		_module.StartNextPanelAnimation();
		yield break;
	}

	public override IEnumerator DisableComponents()
	{

		if (_module.GetFinalState())
		{
			foreach (KMSelectable km in _finalObject.GetComponentsInChildren<KMSelectable>())
			{
				km.Highlight.gameObject.SetActive(true);
			}
			foreach (MeshRenderer mr in _finalObject.GetComponentsInChildren<MeshRenderer>())
			{
				mr.enabled = false;
				yield return new WaitForSeconds(.1f);
			}
		}
		if (c != null)
		{
			_module.StopCoroutine(c);
			c = null;
		}
		_morseLight.enabled = false;
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
		return new Vector3(0.085f, 0.01f, 0.085f);
	}

	IEnumerator SendMorse(List<string> word)
	{
		while (true)
		{
			foreach (string let in word)
			{

				foreach (char bit in let)
				{
					switch (bit)
					{
						case '0':
							_morseLight.material.color = lit;
							yield return new WaitForSeconds(.25f);
							_morseLight.material.color = unlit;
							break;
						case '1':
							_morseLight.material.color = lit;
							yield return new WaitForSeconds(.6f);
							_morseLight.material.color = unlit;
							break;
						default:
							break;
					}
					yield return new WaitForSeconds(0.45f);
				}
				yield return new WaitForSeconds(.75f);
			}
			yield return new WaitForSeconds(1.5f);
		}
	}

}
