using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public class Wires : PanelInterface
{

	Everything _module;
	int _modID;
	int _solvedIndex;
	KMSelectable[] _wires;
	MeshRenderer[] _cutWires;
	Material[] _wireColors;

	List<int> _chosenPos = new List<int>();
	List<int> _chosenColors = new List<int>();
	int _correctDigit;

	int _correctCut;

	bool[] _cut = new bool[6];

	public Wires(Everything _module, int _modID, int _solvedIndex, KMSelectable[] _wires, MeshRenderer[] _cutWires, Material[] _wireColors)
	{
		this._module = _module;
		this._modID = _modID;
		this._wires = _wires;
		this._cutWires = _cutWires;
		this._wireColors = _wireColors;
		this._solvedIndex = _solvedIndex;
	}

	public override void GeneratePanel()
	{
		int wireAmount = rnd.Range(3, 7);

		List<int> wirePos = new List<int>();
		List<int> wireColors = new List<int>();

		for (int i = 0; i < wireAmount; i++)
		{
			int randPos = rnd.Range(0, 6);
			int randColor = rnd.Range(0, _wireColors.Length);
			while (wirePos.Contains(randPos))
			{
				randPos = rnd.Range(0, 6);
			}
			wirePos.Add(randPos);
			wireColors.Add(randColor);
		}

		wirePos.Shuffle();
		wireColors.Shuffle();

		for (int x = 0; x < wireAmount; x++)
		{
			_wires[wirePos.ElementAt(x)].GetComponent<Renderer>().material = _wireColors[wireColors.ElementAt(x)];
			_cutWires[wirePos.ElementAt(x)].material = _wireColors[wireColors.ElementAt(x)];
		}

		_chosenPos = wirePos;
		_chosenColors = wireColors;
		StringBuilder sb = new StringBuilder();
		int count = 0;
		wirePos.Sort();
		foreach (int pos in wirePos)
		{
			sb.Append(count != wireColors.Count() - 1 ? _wires[pos].GetComponent<Renderer>().material.name.Replace(" (Instance)", "") + ", " : _wires[pos].GetComponent<Renderer>().material.name.Replace(" (Instance)", ""));
			count++;
		}
		_correctDigit = GetWireCut(wireAmount) + 1;
		if (_correctDigit == -1)
		{
			Debug.LogFormat("[Everything #{0}]: Unable to obtain the digit for this panel. This is most likely a bug, please report it!", _modID);
			return;
		}
		Debug.LogFormat("[Everything #{0}]: The Wires panel was generated with {1} wires and have colors: {2}. The correct digit for this panel is: {3}.", _modID, wireAmount, sb.ToString(), _correctDigit);
		HandlePanelSolve();
	}

		public override void GenerateFinalPanel()
	{
		int[] wireNumbers = new int[] { 1, 5, 5, 5, 5, 0, 0, 5, 1, 3, 0, 1, 0, 2, 3, 5, 3 };
		int rule = _module.GetCorrectDigits().Select(x => int.Parse(x.ToString())).ToList().Sum() % 17;
		bool done = false;

		while (!done)
		{
			switch (rule)
			{
				case 4:
				case 9:
				case 14:
					if (_module.GetBombInfo().GetSerialNumber()[5] % 2 == 1)
					{
						done = true;
						break;
					}
					rule++;
					break;
				default:
					done = true;
					break;
			}
		}

		foreach (MeshRenderer mr in _wires.Select(x => x.GetComponent<Renderer>()))
		{
			mr.material = _wireColors[0];
		}
		foreach (MeshRenderer mr in _cutWires)
		{
			mr.material = _wireColors[0];
		}

		_correctCut = wireNumbers[rule];

		Debug.LogFormat("[Everything #{0}]: The final panel was generated as Wires and will have rule {1} being applied meaning that the wire to be cut is {2}.", _modID, rule + 1, _correctCut + 1);
	}

	public override IEnumerator EnableComponents()
	{

		if (_module.GetFinalState())
		{
			for (int i = 0; i <= 5; i++)
			{
				_wires[i].GetComponent<Renderer>().enabled = true;
				foreach (MeshRenderer mr in _wires[i].GetComponentsInChildren<MeshRenderer>())
				{
					if (mr.gameObject.name.Contains("Filler") || mr.gameObject.name.Contains("Holder")) mr.enabled = true;
				}
				yield return new WaitForSeconds(0.1f);
			}
			foreach (KMHighlightable kh in _wires.Select(x => x.Highlight))
			{
				kh.gameObject.SetActive(true);
			}
			_module.StartNextPanelAnimation();
			yield break;
		}

		for (int i = 0; i <= 5; i++)
		{
			if (_chosenPos.Contains(i)) { _wires[i].GetComponent<Renderer>().enabled = true; }
			foreach (MeshRenderer mr in _wires[i].GetComponentsInChildren<MeshRenderer>())
			{
				if (mr.gameObject.name.Contains("Filler") || mr.gameObject.name.Contains("Holder")) mr.enabled = true;
			}
			yield return new WaitForSeconds(0.1f);
		}
		_module.StartNextPanelAnimation();
		yield break;
	}

	public override IEnumerator DisableComponents()
	{

		if (_module.GetFinalState())
		{
			foreach (KMHighlightable kh in _wires.Select(x => x.Highlight))
			{
				kh.gameObject.SetActive(false);
			}
			for (int i = 0; i <= 5; i++)
			{
				_wires[i].GetComponent<Renderer>().enabled = false;
				foreach (MeshRenderer mr in _wires[i].GetComponentsInChildren<MeshRenderer>())
				{
					if (mr.gameObject.name.Contains("Filler") || mr.gameObject.name.Contains("Holder")) mr.enabled = false;
				}
				yield return new WaitForSeconds(0.1f);
			}
			_module.StartNextPanelAnimation();
			yield break;
		}

		for (int i = 5; i >= 0; i--)
		{
			if (_chosenPos.Contains(i)) { _wires[i].GetComponent<Renderer>().enabled = false; }
			foreach (MeshRenderer mr in _wires[i].GetComponentsInChildren<MeshRenderer>())
			{
				if (mr.gameObject.name.Contains("Filler") || mr.gameObject.name.Contains("Holder")) mr.enabled = false;
			}
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

	public override void Interact(KMSelectable km)
	{
		if (_module._isAnimating || _module._modSolved) { return; }
		_module._audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSnip, km.transform);
		int index = Array.IndexOf(_wires, km);
		if (_module.GetFinalState())
		{
			if (_cut[index]) return;
			if (index != _correctCut)
			{
				_module.Strike();
				Debug.LogFormat("[Everything #{0}]: Wire cut was {1} when expected cut was {2}.", _modID, index + 1, _correctCut + 1);
				_cutWires[index].gameObject.SetActive(true);
				_wires[index].Highlight.gameObject.SetActive(false);
				_wires[index].GetComponent<Renderer>().enabled = false;
				_cut[index] = true;
				return;
			}
			_cutWires[index].gameObject.SetActive(true);
			_wires[index].Highlight.gameObject.SetActive(false);
			_wires[index].GetComponent<Renderer>().enabled = false;
			_cut[index] = true;
			Debug.LogFormat("[Everything #{0}]: Wire cut was {1} and that is correct! Module has been solved!", _modID, index + 1);
			_module.GetModule().HandlePass();
			_module._modSolved = true;
		}
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
		return new Vector3(0.11f, 0.01f, 0.085f);
	}

	int GetWireCut(int amount)
	{
		List<int> colors = new List<int>();
		int count;
		for (int i = 0; i < _wireColors.Length; i++)
		{
			count = 0;
			foreach (int c in _chosenColors)
			{
				if (c == i) { count++; }
			}
			colors.Add(count);
		}
		string lastWire = _wires[_chosenPos[_chosenPos.Count() - 1]].GetComponent<Renderer>().material.name.Replace(" (Instance)", "");
		bool isOdd = _module.GetBombInfo().GetSerialNumberNumbers().Last().EqualsAny(1, 3, 5, 7, 9);
		switch (amount)
		{
			case 3:
				if (colors[1] == 2 && colors[2] == 1)
				{
					return _chosenPos[1];
				}
				else if (colors[2] == 0)
				{
					return _chosenPos[1];
				}
				else
				{
					return _chosenPos[2];
				}
			case 4:
				if (colors[2] >= 2 && isOdd)
				{
					List<int> redPos = new List<int>();
					for (int i = 0; i < _chosenColors.Count(); i++)
					{
						if (_chosenColors[i] == 2) { redPos.Add(_chosenPos[i]); }
					}
					redPos.Sort();
					return redPos[redPos.Count() - 1];
				}
				else if (colors[2] == 0 && lastWire.Equals("Yellow"))
				{
					return _chosenPos[0];
				}
				else if (colors[1] == 1)
				{
					return _chosenPos[0];
				}
				else if (colors[4] >= 2)
				{
					return _chosenPos[3];
				}
				else
				{
					return _chosenPos[1];
				}
			case 5:
				if (lastWire.Equals("Black") && isOdd)
				{
					return _chosenPos[3];
				}
				else if (colors[2] == 1 && colors[3] >= 2)
				{
					return _chosenPos[0];
				}
				else if (colors[0] == 0)
				{
					return _chosenPos[1];
				}
				else
				{
					return _chosenPos[0];
				}
			case 6:
				if (colors[4] == 0 && isOdd)
				{
					return _chosenPos[2];
				}
				else if (colors[4] == 1 && colors[3] >= 2)
				{
					return _chosenPos[3];
				}
				else if (colors[2] == 0)
				{
					return _chosenPos[5];
				}
				else
				{
					return _chosenPos[3];
				}
			default:
				break;
		}
		return -2;
	}

}
