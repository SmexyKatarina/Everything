using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public class CompWires : PanelInterface
{

	Everything _module;
	int _modID;
	int _solvedIndex;
	KMSelectable[] _wires;
	MeshRenderer[] _cutWires;
	Material[] _wireColors;
	TextMesh[] _stars;
	MeshRenderer[] _leds;

	Color32 lit = new Color32(239, 244, 197, 255);
	Color32 unlit = new Color32(13, 23, 30, 255);

	int _correctDigit;
	string _chosenBinary;

	enum Condition
	{
		BATTERY,
		SERIAL,
		PARALLEL
	}

	List<string> _chosenWires = new List<string>();

	string[] _allCompPossiblities = new string[] // color (index; white, red, blue, purple):led:star:condition
	{
		"0:0:0:1",
		"0:1:0:0",
		"0:0:1:1",
		"0:1:1:BATTERY",

		"1:0:0:SERIAL",
		"1:1:0:BATTERY",
		"1:0:1:1",
		"1:1:1:BATTERY",

		"2:0:0:SERIAL",
		"2:1:0:PARALLEL",
		"2:0:1:0",
		"2:1:1:PARALLEL",

		"3:0:0:SERIAL",
		"3:1:0:SERIAL",
		"3:0:1:PARALLEL",
		"3:1:1:0"
	};

	List<string> _finalWires = new List<string>();
	bool[] _wiresToBeCut = new bool[6];
	bool[] _wiresCut = new bool[6];
	int[] _correctPos = new int[6];

	public CompWires(Everything _module, int _modID, int _solvedIndex, KMSelectable[] _wires, MeshRenderer[] _cutWires, Material[] _wireColors, TextMesh[] _stars, MeshRenderer[] _leds)
	{
		this._module = _module;
		this._modID = _modID;
		this._wires = _wires;
		this._cutWires = _cutWires;
		this._wireColors = _wireColors;
		this._solvedIndex = _solvedIndex;
		this._stars = _stars;
		this._leds = _leds;
	}

	public override void GeneratePanel()
	{
		int num = rnd.Range(0, 64);
		_chosenBinary = GenerateBinaryNumber(num);
		foreach (char bit in _chosenBinary)
		{
			string[] wire = _allCompPossiblities[rnd.Range(0, _allCompPossiblities.Length)].Split(':');
			if (bit.ToString() == "1")
			{
				while (CheckWire(wire[3], false) || !CheckWire(wire[3], true)) wire = _allCompPossiblities[rnd.Range(0, _allCompPossiblities.Length)].Split(':');
				_chosenWires.Add(wire.Join(":"));
			}
			else if (bit.ToString() == "0")
			{
				while (!CheckWire(wire[3], false) || CheckWire(wire[3], true)) wire = _allCompPossiblities[rnd.Range(0, _allCompPossiblities.Length)].Split(':');
				_chosenWires.Add(wire.Join(":"));
			}
		}
		for (int i = 0; i <= 5; i++)
		{
			string[] wire = _chosenWires[i].Split(':');
			Material mat = _wireColors[int.Parse(wire[0])];
			bool led = wire[1] == "0" ? false : true;
			bool star = wire[2] == "0" ? false : true;
			_wires[i].GetComponent<Renderer>().material = mat;
			if (led) { _leds[i].material.color = lit; }
			if (!star) { _stars[i].text = ""; }
			wire[0] = mat.name;
			wire[1] = led ? "Lit" : "Unlit";
			wire[2] = star ? "Yes" : "No";
			wire[3] = !wire[3].EqualsAny("0", "1") ? char.ToUpper(wire[3][0]) + wire[3].Substring(1).ToLower() : wire[3] == "0" ? "Don't Cut" : "Cut";
			_chosenWires[i] = "(" + wire.Join(", ") + ")";
		}

		_correctDigit = int.Parse(num.ToString()[num.ToString().Length - 1].ToString());

		Debug.LogFormat("[Everything #{0}]: The Complicated Wires panel was generated with wires: {1}. The chosen binary number was {2} which turns into {3} in base-10. The correct digit for this panel is: {4}.", _modID, _chosenWires.Join(", "), _chosenBinary, num, _correctDigit);
		HandlePanelSolve();
	}

	public override void GenerateFinalPanel()
	{
		foreach (MeshRenderer led in _leds)
		{
			led.material.color = unlit;
		}
		foreach (TextMesh tm in _stars)
		{
			tm.text = "";
		}
		for (int i = 0; i <= 5; i++)
		{
			_wires[i].GetComponent<Renderer>().material = _wireColors[0];
			_cutWires[i].GetComponent<Renderer>().material = _wireColors[0];
		}

		int[] sNum = _module.GetBombInfo().GetSerialNumberNumbers().ToArray();
		string digits = _module.GetCorrectDigits() + sNum[0].ToString() + sNum[1].ToString();

		int[] wireIndices = new int[] { 14, 6, 8, 13, 7, 11, 3, 0, 1, 10 };

		for (int i = 0; i <= 5; i++)
		{
			int[] indices = digits.Select(x => int.Parse(x.ToString())).ToArray();
			string[] wire = _allCompPossiblities[wireIndices[indices[i]]].Split(':');
			_wiresToBeCut[i] = CheckWire(wire[3], true);
			Material mat = _wireColors[int.Parse(wire[0])];
			bool led = wire[1] == "0" ? false : true;
			bool star = wire[2] == "0" ? false : true;
			wire[0] = mat.name;
			wire[1] = led ? "Lit" : "Unlit";
			wire[2] = star ? "Yes" : "No";
			wire[3] = !wire[3].EqualsAny("0", "1") ? char.ToUpper(wire[3][0]) + wire[3].Substring(1).ToLower() : wire[3] == "0" ? "Don't Cut" : "Cut";
			_finalWires.Add("(" + wire.Join(", ") + ")");
			_correctPos[i] = !_wiresToBeCut[i] ? -1 : i + 1;
		}

		Debug.LogFormat("[Everything #{0}]: The final panel was generated as Complicated Wires. The total string of digits is {1}. The wires from left to right are: {2}. The correct wires to cut are {3}.", _modID, digits, _finalWires.Join(", "), _correctPos.Where(x => x != -1).Join(", "));
	}

	public override void Interact(KMSelectable km)
	{
		int index = Array.IndexOf(_wires, km);
		if (_wiresCut[index] || _module._modSolved) return;
		_module._audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSnip, km.transform);
		if (!_wiresToBeCut[index] && !_wiresCut[index])
		{
			_module.Strike();
			_wiresCut[index] = true;
			UpdateWire(index);
			List<int> tocut = new List<int>();
			for (int i = 0; i <= 5; i++)
			{
				if (_wiresCut[i]) continue;
				if (_wiresToBeCut[i] && !_wiresCut[i]) tocut.Add(i + 1);
			}
			Debug.LogFormat("[Everything #{0}]: Incorrect wire cut. Expected {1} to be cut but {2} was cut.", _modID, tocut.Join(", "), index + 1);
			return;
		}
		UpdateWire(index);
		_wiresCut[index] = true;
		int total = 0;
		int cut = _wiresToBeCut.Where(x => x == true).Count();
		foreach (int x in _correctPos.Where(x => x != -1).Select(x => x - 1))
		{
			if (_wiresToBeCut[x] && _wiresCut[x]) total++;
		}
		if (total == cut)
		{
			Debug.LogFormat("[Everything #{0}]: All wires have been successfully cut. Module solved.", _modID);
			_module._modSolved = true;
			_module.GetModule().HandlePass();
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
		foreach (GameObject go in _wires.Select(x => x.gameObject))
		{
			go.GetComponent<Renderer>().enabled = true;
			foreach (MeshRenderer mr in go.GetComponentsInChildren<Renderer>().Where(x => x.name != "Cut"))
			{
				mr.enabled = true;
			}
			yield return new WaitForSeconds(.1f);
		}
		if (_module.GetFinalState())
		{
			foreach (KMHighlightable kh in _wires.Select(x => x.Highlight))
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
			foreach (KMHighlightable kh in _wires.Select(x => x.Highlight))
			{
				kh.gameObject.SetActive(false);
			}
		}
		foreach (GameObject go in _wires.Select(x => x.gameObject))
		{
			go.GetComponent<Renderer>().enabled = false;
			foreach (MeshRenderer mr in go.GetComponentsInChildren<Renderer>())
			{
				mr.enabled = false;
			}
			yield return new WaitForSeconds(.1f);
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
		return new Vector3(0.1f, 0.01f, 0.1f);
	}

	string GenerateBinaryNumber(int num)
	{
		string s = "";
		for (int pow = 5; pow >= 0; pow--)
		{
			if (num - Math.Pow(2, pow) < 0)
			{
				s += "0";
			}
			else
			{
				num -= (int)Math.Pow(2, pow);
				s += "1";
			}
		}
		return s;
	}

	bool ConditionTrue(Condition con)
	{
		if (con == Condition.BATTERY) { if (_module.GetBombInfo().GetBatteryCount() >= 2) return true; }
		if (con == Condition.PARALLEL) { if (_module.GetBombInfo().GetSerialNumberNumbers().Last() % 2 == 0) return true; }
		if (con == Condition.SERIAL) { if (_module.GetBombInfo().GetPortCount(Port.Serial) >= 1) return true; }
		return false;
	}

	bool CheckWire(string condition, bool state)
	{
		if (state) return condition == "0" ? false : condition == "1" ? true : ConditionTrue((Condition)Enum.Parse(typeof(Condition), condition));
		return condition == "1" ? false : condition == "0" ? true : !ConditionTrue((Condition)Enum.Parse(typeof(Condition), condition));
	}

	void UpdateWire(int index)
	{
		_wires[index].GetComponent<Renderer>().enabled = false;
		_wires[index].Highlight.gameObject.SetActive(false);
		_cutWires[index].enabled = true;
	}

}
