using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public class WireSequence : PanelInterface
{

	Everything _module;
	int _modID;
	int _solvedIndex;
	KMSelectable[] _wires;
	MeshRenderer[] _cutWires;
	Material[] _wireColors;
	TextMesh[] _wireNumbers;
	TextMesh[] _wireLetters;
	MeshRenderer[] _additional;

	int _correctDigit;
	int _stagePanel = 0;
	bool _updating = false;

	string[] _redCuts = new string[] { "C", "B", "A", "AC", "B", "AC", "ABC", "AB", "B" };
	string[] _blueCuts = new string[] { "B", "AC", "B", "A", "B", "BC", "C", "AC", "A" };
	string[] _blackCuts = new string[] { "ABC", "AC", "B", "AC", "B", "BC", "AB", "C", "C" };

	List<Material> _chosenColors = new List<Material>();
	List<TextMesh> _chosenLetters = new List<TextMesh>();
	List<int> _numbers = new List<int>();


	int[] _tableOffsets = new int[3];
	int _redTable = 0;
	int _blueTable = 0;
	int _blackTable = 0;

	List<int> _correctCuts = new List<int>();
	List<int> _correctCutsCheck = new List<int>();
	bool[] _wiresCut = new bool[12];

	public WireSequence(Everything _module, int _modID, int _solvedIndex, KMSelectable[] _wires, MeshRenderer[] _cutWires, Material[] _wireColors, TextMesh[] _wireNumbers, TextMesh[] _wireLetters, MeshRenderer[] _additional)
	{
		this._module = _module;
		this._modID = _modID;
		this._wires = _wires;
		this._cutWires = _cutWires;
		this._wireColors = _wireColors;
		this._solvedIndex = _solvedIndex;
		this._wireNumbers = _wireNumbers;
		this._wireLetters = _wireLetters;
		this._additional = _additional;
	}

	public override void GeneratePanel()
	{
		bool[] booled = new bool[] { true, true, true, true, true, true, true, true, false, false, false, false };

		booled.Shuffle();

		int r = 0;
		int b = 0;
		int k = 0;
		int correctCuts = 0;
		List<int> correctNumbers = new List<int>();

		for (int i = 0; i < booled.Count(); i++)
		{
			if (!booled[i]) { _numbers.Add(-1); _chosenColors.Add(null); _chosenLetters.Add(null); continue; }
			int color = rnd.Range(0, 3);
			int letter = rnd.Range(0, 3);
			Material m = _wireColors[color];
			TextMesh let = _wireLetters[letter];
			_chosenColors.Add(m);
			_chosenLetters.Add(let);
			_numbers.Add(i + 1);
			switch (m.name)
			{
				case "Red":
					if (_redCuts[r].Any(x => x == let.text[0]))
					{
						correctNumbers.Add(i + 1);
						correctCuts++;
						r++;
						continue;
					}
					r++;
					break;
				case "Blue":
					if (_blueCuts[b].Any(x => x == let.text[0]))
					{
						correctNumbers.Add(i + 1);
						correctCuts++;
						b++;
						continue;
					}
					b++;
					break;
				case "Black":
					if (_blackCuts[k].Any(x => x == let.text[0]))
					{
						correctNumbers.Add(i + 1);
						correctCuts++;
						k++;
						continue;
					}
					k++;
					break;
				default:
					break;
			}
		}

		for (int i = 0; i <= 2; i++)
		{
			Material m = _chosenColors[i];
			TextMesh let = _chosenLetters[i];
			int number = _numbers[i];
			if (m == null || let == null || number == -1) continue;
			int[] wire = WiresByLetter(let.text);
			_wires[wire[i]].GetComponent<Renderer>().material = m;
		}
		char[] chars = correctNumbers.Sum().ToString().ToCharArray();
		_correctDigit = int.Parse(chars[chars.Length - 1].ToString());

		Debug.LogFormat("[Everything #{0}]: The Wire Sequence panel was generated with {1} possible cuts. The numbers of these wires are {2} and all of that added together is {3}. The correct digit for this panel is: {4}.", _modID, correctCuts, correctNumbers.Join(", "), correctNumbers.Sum(), _correctDigit);
		HandlePanelSolve();
	}

	public override void GenerateFinalPanel()
	{

		_chosenColors.Clear();
		_chosenLetters.Clear();
		_numbers.Clear();

		foreach (MeshRenderer mr in _wires.Select(x => x.GetComponent<Renderer>()))
		{
			if (mr.gameObject.name.EqualsAny("Up", "Down")) continue;
			mr.material.color = new Color32(255, 255, 255, 255);
		}

		int[] digits = _module.GetCorrectDigits().Select(x => int.Parse(x.ToString())).ToArray();

		bool[] generated = new bool[12];

		for (int i = 0; i <= rnd.Range(8, 11); i++)
		{
			generated[i] = true;
		}

		generated.Shuffle();

		for (int i = 0; i <= 2; i++)
		{
			_tableOffsets[i] += ((digits[i] + digits[3]) + 1) % 10;
		}

		for (int i = 0; i < generated.Count(); i++)
		{
			if (!generated[i]) { _numbers.Add(-1); _chosenColors.Add(null); _chosenLetters.Add(null); continue; }
			int color = rnd.Range(0, 3);
			int letter = rnd.Range(0, 3);
			Material m = _wireColors[color];
			TextMesh let = _wireLetters[letter];
			_chosenColors.Add(m);
			_chosenLetters.Add(let);
			_numbers.Add(i + 1);
			switch (m.name)
			{
				case "Red":
					if (_redCuts[(_redTable + _tableOffsets[0]) % 9].Any(x => x == let.text[0]))
					{
						_correctCuts.Add(i + 1);
						_redTable++;
						continue;
					}
					_redTable++;
					break;
				case "Blue":
					if (_blueCuts[(_blueTable + _tableOffsets[1]) % 9].Any(x => x == let.text[0]))
					{
						_correctCuts.Add(i + 1);
						_blueTable++;
						continue;
					}
					_blueTable++;
					break;
				case "Black":
					if (_blackCuts[(_blackTable + _tableOffsets[2]) % 9].Any(x => x == let.text[0]))
					{
						_correctCuts.Add(i + 1);
						_blackTable++;
						continue;
					}
					_blackTable++;
					break;
				default:
					break;
			}
		}

		for (int i = 0; i <= 2; i++)
		{
			Material m = _chosenColors[i];
			TextMesh let = _chosenLetters[i];
			int number = _numbers[i];
			if (m == null || let == null || number == -1) continue;
			int[] wire = WiresByLetter(let.text);
			_wires[wire[i]].GetComponent<Renderer>().material = m;
		}

		_correctCutsCheck = _correctCuts;

		Debug.LogFormat("[Everything #{0}]: The final panel was generated as Wire Sequence. A total of {1} wires were generated. The offsets for each table (Red, Blue and Black) are {2}. The correct wires to cut are {3}.", _modID, generated.Where(x => x).Count(), _tableOffsets.Join(", "), _correctCuts.Join(", "));
	}

	public override void Interact(KMSelectable km)
	{
		int index = Array.IndexOf(_wires, km);
		if (_updating || _module._modSolved) return;

		int baseNum = 0;
		int[] nums = new int[] { 1, 2, 3 };

		switch (index)
		{

			case 0:
			case 1:
			case 2:
				baseNum = 1;
				break;
			case 3:
			case 4:
			case 5:
				baseNum = 2;
				break;
			case 6:
			case 7:
			case 8:
				baseNum = 3;
				break;
			case 9:
				if (_stagePanel == 0) return;
				if (_correctCutsCheck.Any(x => nums.Select(y => y + (_stagePanel * 3)).Contains(x)))
				{
					_module.Strike();
					Debug.LogFormat("[Everything #{0}]: Can't switch panels due to wires still not cut from this panel.", _modID);
					return;
				}
				_stagePanel--;
				_module.StartCoroutine(UpdatePanel());
				return;
			case 10:
				if (_stagePanel == 3) return;
				if (_correctCutsCheck.Any(x => nums.Select(y => y + (_stagePanel * 3)).Contains(x)))
				{
					_module.Strike();
					Debug.LogFormat("[Everything #{0}]: Can't switch panels due to wires still not cut from this panel.", _modID);
					return;
				}
				_stagePanel++;
				_module.StartCoroutine(UpdatePanel());
				return;
		}

		int num = baseNum + (_stagePanel * 3);
		if (_wiresCut[num - 1]) return;
		_module._audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSnip, km.transform);
		if (!_correctCutsCheck.Contains(num))
		{
			_module.Strike();
			UpdateWire(index);
			_wiresCut[num - 1] = true;
			Debug.LogFormat("[Everything #{0}]: Incorrect wire cut. Expected {1} but was given {2}.", _modID, _correctCutsCheck.Join(", "), num);
			return;
		}
		UpdateWire(index);
		_wiresCut[num - 1] = true;
		_correctCutsCheck.Remove(num);
		if (_correctCutsCheck.Count() == 0)
		{
			Debug.LogFormat("[Everything #{0}]: All correct wires have been cut. Module solved.", _modID);
			_module._modSolved = true;
			_module.GetModule().HandlePass();
			return;
		}
		Debug.LogFormat("[Everything #{0}]: Correct wire cut expecting one of the following: {1}.", _modID, _correctCutsCheck.Join(", "));
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

		foreach (MeshRenderer mr in _additional)
		{
			mr.enabled = true;
			yield return new WaitForSeconds(0.005f);
		}
		foreach (TextMesh tm in _wireLetters)
		{
			tm.GetComponent<Renderer>().enabled = true;
			yield return new WaitForSeconds(.025f);
		}
		foreach (KMSelectable km in _wires.Where(x => x.name.EqualsAny("Up", "Down")).ToArray())
		{
			km.Highlight.gameObject.SetActive(true);
		}
		_module.StartCoroutine(UpdatePanel());
		while (_updating) yield return new WaitForSeconds(0.01f);
		_module.StartNextPanelAnimation();
		yield break;
	}

	public override IEnumerator DisableComponents()
	{

		foreach (KMHighlightable kh in _wires.Select(x => x.Highlight))
		{
			kh.gameObject.SetActive(false);
		}
		foreach (MeshRenderer mr in _wires.Select(x => x.GetComponent<MeshRenderer>()))
		{
			mr.enabled = false;
			yield return new WaitForSeconds(0.025f);
		}
		foreach (MeshRenderer mr in _cutWires)
		{
			mr.enabled = false;
			yield return new WaitForSeconds(0.025f);
		}
		foreach (TextMesh tm in _wireNumbers)
		{
			tm.GetComponent<Renderer>().enabled = false;
			yield return new WaitForSeconds(.025f);
		}
		foreach (TextMesh tm in _wireLetters)
		{
			tm.GetComponent<Renderer>().enabled = false;
			yield return new WaitForSeconds(.025f);
		}
		foreach (MeshRenderer mr in _additional.Reverse())
		{
			mr.enabled = false;
			yield return new WaitForSeconds(0.005f);
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
		Debug.LogFormat("[Everything #{0}]: After the recent panel solve, there are {1} panels left to solve.", _modID, 4-_module.GetSolvedPanels().Select(x => x).Count());
	}

	public override Vector3 GetBaseSize()
	{
		return new Vector3(0.085f, 0.01f, 0.085f);
	}

	int[] WiresByLetter(string letter)
	{
		switch (letter)
		{
			case "A":
				return new int[] { 0, 3, 6 };
			case "B":
				return new int[] { 1, 4, 7 };
			case "C":
				return new int[] { 2, 5, 8 };
			default:
				return null;
		}
	}

	IEnumerator UpdatePanel()
	{
		_updating = true;
		foreach (KMHighlightable kh in _wires.Select(x => x.Highlight))
		{
			if (kh.gameObject.name.EqualsAny("UpHL", "DownHL")) continue;
			kh.gameObject.SetActive(false);
		}
		int count = 1;
		foreach (TextMesh tm in _wireNumbers)
		{
			tm.GetComponent<Renderer>().enabled = false;
			tm.text = (count + (_stagePanel * 3)).ToString();
			count++;
			yield return new WaitForSeconds(.1f);
		}
		for (int i = 0; i <= 8; i++)
		{
			_wires[i].GetComponent<Renderer>().enabled = false;
			_cutWires[i].enabled = false;
			yield return new WaitForSeconds(.1f);
		}

		for (int i = 0 + (_stagePanel * 3); i <= 2 + (_stagePanel * 3); i++)
		{
			Material m = _chosenColors[i];
			TextMesh let = _chosenLetters[i];
			int number = _numbers[i];
			if (m == null || let == null || number == -1) continue;
			int[] wire = WiresByLetter(let.text);
			_wires[wire[i - (_stagePanel * 3)]].GetComponent<Renderer>().material = m;
			_cutWires[wire[i - (_stagePanel * 3)]].material = m;
			if (_wiresCut[i])
			{
				_cutWires[wire[i - (_stagePanel * 3)]].enabled = true;
			}
			else
			{
				_wires[wire[i - (_stagePanel * 3)]].GetComponent<Renderer>().enabled = true;
				if (_module.GetFinalState()) { _wires[wire[i - (_stagePanel * 3)]].Highlight.gameObject.SetActive(true); }
			}
			yield return new WaitForSeconds(.1f);
		}

		foreach (TextMesh tm in _wireNumbers)
		{
			tm.GetComponent<Renderer>().enabled = true;
			yield return new WaitForSeconds(.1f);
		}
		_updating = false;
		yield break;
	}

	void UpdateWire(int index)
	{
		_wires[index].Highlight.gameObject.SetActive(true);
		_wires[index].GetComponent<Renderer>().enabled = false;
		_cutWires[index].GetComponent<Renderer>().enabled = true;
	}

}
