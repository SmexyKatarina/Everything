using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public class Password : PanelInterface
{

	Everything _module;
	int _modID;
	int _solvedIndex;
	MeshRenderer[] _arrowRenderers;
	MeshRenderer[] _displayRenderers;
	TextMesh[] _letters;
	KMSelectable[] _upArrows;
	KMSelectable[] _downArrows;
	KMSelectable _submit;

	string[] _possibleWords = new string[]
	{
		"about", "after", "again", "below", "could",
		"every", "first", "found", "great", "house",
		"large", "learn", "never", "other", "place",
		"plant", "point", "right", "small", "sound",
		"spell", "still", "study", "their", "there",
		"these", "thing", "think", "three", "water",
		"where", "which", "world", "would", "write",
	};

	int _correctDigit;

	int[] _indices = new int[] { 0, 0, 0, 0, 0 };
	char[][] _displayColumns = new char[5][];
	string _chosenWord;

	public Password(Everything _module, int _modID, int _solvedIndex, MeshRenderer[] _arrowRenderers, MeshRenderer[] _displayRenderers, TextMesh[] _letters, KMSelectable[] _upArrows, KMSelectable[] _downArrows, KMSelectable _submit)
	{
		this._module = _module;
		this._modID = _modID;
		this._solvedIndex = _solvedIndex;
		this._arrowRenderers = _arrowRenderers;
		this._displayRenderers = _displayRenderers;
		this._letters = _letters;
		this._upArrows = _upArrows;
		this._downArrows = _downArrows;
		this._submit = _submit;
	}

	public override void GeneratePanel()
	{
		int index = rnd.Range(0, _possibleWords.Length);
		_chosenWord = _possibleWords[index];
		int wordIndex = 0;

		char[] alphabet = Enumerable.Range(0, 26).Select(z => (char)(z + 'A')).ToArray();

		for (int x = 0; x <= 4; x++) 
		{
			char[] c = new char[5];
			for (int y = 0; y <= 4; y++)
			{
				if (y == 0) { c[y] = char.ToUpper(_chosenWord[wordIndex]); wordIndex++; continue; }
				char ch = alphabet.PickRandom();
				while (c.Any(z => z == ch)) 
				{
					ch = alphabet.PickRandom();
				}
				c[y] = ch;
			}
			_displayColumns[x] = c.Shuffle();
		}

		for (int i = 0; i <= 4; i++) 
		{
			_letters[i].text = _displayColumns[i][0].ToString();
		}

		char[] num = (index + 1).ToString().ToCharArray();

		_correctDigit = int.Parse(num[num.Length - 1].ToString());

		Debug.LogFormat("[Everything #{0}]: The Password panel was generated with the word {1}. The index for this word in the table is {2}. The correct digit for this panel is: {3}.", _modID, _chosenWord.ToUpper(), index+1, _correctDigit);
		HandlePanelSolve();
	}

	public override void GenerateFinalPanel()
	{
		_displayColumns = new char[5][];
		_chosenWord = _possibleWords[int.Parse(_module.GetCorrectDigits()) % 35];
		int wordIndex = 0;

		char[] alphabet = Enumerable.Range(0, 26).Select(z => (char)(z + 'A')).ToArray();

		for (int x = 0; x <= 4; x++)
		{
			char[] c = new char[5];
			for (int y = 0; y <= 4; y++)
			{
				if (y == 0) { c[y] = char.ToUpper(_chosenWord[wordIndex]); wordIndex++; continue; }
				char ch = alphabet.PickRandom();
				while (c.Any(z => z == ch))
				{
					ch = alphabet.PickRandom();
				}
				c[y] = ch;
			}
			_displayColumns[x] = c.Shuffle();
		}

		for (int i = 0; i <= 4; i++)
		{
			_letters[i].text = _displayColumns[i][0].ToString();
		}

		Debug.LogFormat("[Everything #{0}]: The final panel was generated as Password. The four-digit number % 35 + 1 = {1}. The correct word to submit is {2}.", _modID, _chosenWord, (int.Parse(_module.GetCorrectDigits()) % 35) + 1);
	}

	public override void Interact(KMSelectable km)
	{
		if (km.gameObject.name == "PassSubmit" && !_module._modSolved) 
		{
			string word = "";
			foreach (string s in _letters.Select(x => x.text)) 
			{
				word += s;
			}
			if (word != _chosenWord.ToUpper())
			{
				_module.GetModule().HandleStrike();
				Debug.LogFormat("[Everything #{0}]: The word {1} is incorrect, expecting {2}.", _modID, word, _chosenWord.ToUpper());
				return;
			}
			else
			{
				Debug.LogFormat("[Everything #{0}]: The correct word has been submitted. Module solved.", _modID);
				_module._modSolved = true;
				_module.GetModule().HandlePass();
				for (int i = 0; i <= 4; i++) 
				{
					_letters[i].text = "SOLVE"[i].ToString();
				}
				return;
			}
		}
		string name = km.gameObject.name;
		bool down = name.ToLower().Contains("down");
		int index = int.Parse(name[5].ToString()) - 1;
		int colPos = _indices[index];
		if (down) 
		{
			colPos++;
			if (colPos == 5) { colPos = 0; }
			_letters[index].text = _displayColumns[index][colPos].ToString();
			_indices[index] = colPos;
			return;
		 }
		colPos--;
		if (colPos == -1) { colPos = 4; }
		_letters[index].text = _displayColumns[index][colPos].ToString();
		_indices[index] = colPos;
		return;
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
		_module._isAnimating = true;
		foreach (MeshRenderer mr in _displayRenderers) 
		{
			mr.enabled = true;
			yield return new WaitForSeconds(.05f);
		}
		foreach (MeshRenderer mr in _arrowRenderers) 
		{
			mr.enabled = true;
			yield return new WaitForSeconds(.05f);
		}
		foreach (TextMesh tm in _letters) 
		{
			tm.GetComponent<Renderer>().enabled = true;
			yield return new WaitForSeconds(.1f);
		}
		if (_module.GetFinalState()) 
		{
			_submit.GetComponent<Renderer>().enabled = true;
			_submit.GetComponentInChildren<TextMesh>().GetComponent<Renderer>().enabled = true;
			yield return new WaitForSeconds(.1f);
		}
		for (int i = 0; i <= 4; i++) 
		{
			_upArrows[i].Highlight.gameObject.SetActive(true);
			_downArrows[i].Highlight.gameObject.SetActive(true);
		}
		_module._isAnimating = false;
		yield break;
	}

	public override IEnumerator DisableComponents()
	{
		_module._isAnimating = true;
		for (int i = 0; i <= 4; i++)
		{
			_upArrows[i].Highlight.gameObject.SetActive(false);
			_downArrows[i].Highlight.gameObject.SetActive(false);
		}
		if (_module.GetFinalState())
		{
			_submit.GetComponent<Renderer>().enabled = false;
			_submit.GetComponentInChildren<TextMesh>().GetComponent<Renderer>().enabled = false;
			yield return new WaitForSeconds(.1f);
		}
		foreach (TextMesh tm in _letters)
		{
			tm.GetComponent<Renderer>().enabled = false;
			yield return new WaitForSeconds(.1f);
		}
		foreach (MeshRenderer mr in _arrowRenderers)
		{
			mr.enabled = false;
			yield return new WaitForSeconds(.05f);
		}
		foreach (MeshRenderer mr in _displayRenderers)
		{
			mr.enabled = false;
			yield return new WaitForSeconds(.05f);
		}
		_module._isAnimating = false;
		yield break;
	}

	public override IEnumerator ChangeBaseSize(float delay)
	{
		_module._isAnimating = true;
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
		_module._isAnimating = false;
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
	}

	public override Vector3 GetBaseSize()
	{
		return new Vector3(0.085f, 0.01f, 0.1f);
	}

}
