using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public class Button : PanelInterface
{

	Everything _module;
	int _modID;
	int _solvedIndex;
	KMSelectable _button;
	MeshRenderer _strip;
	Material[] _stripColors;
	TextMesh _buttonText;

	int _correctDigit;
	Material _chosenColor;

	bool _requestingTap;
	int _x;
	int _holdx;
	int _finalStripColor;
	int _finalColor;
	int _finalWord;

	bool holding;

	public Button(Everything _module, int _modID, int _solvedIndex, KMSelectable _button, MeshRenderer _strip, Material[] _stripColors, TextMesh _buttonText)
	{
		this._module = _module;
		this._modID = _modID;
		this._button = _button;
		this._strip = _strip;
		this._stripColors = _stripColors;
		this._solvedIndex = _solvedIndex;
		this._buttonText = _buttonText;
	}

	public override void GeneratePanel()
	{
		int rand = rnd.Range(0, 10);
		_chosenColor = _stripColors[rand];
		_correctDigit = rand;
		_strip.GetComponent<Renderer>().material = _chosenColor;
		Debug.LogFormat("[Everything #{0}]: The Button panel was generated with the strip color of {1}. The correct digit for this panel is: {2}.", _modID, _stripColors[rand].name, _correctDigit);
	}

	public override void GenerateFinalPanel()
	{
		_x = _module.GetDigitalRoot(int.Parse(_module.GetCorrectDigits()));
		_finalColor = rnd.Range(0, 4);
		_finalWord = rnd.Range(0, 3);

		Color32[] colors = new Color32[]
		{
			new Color32(200,0,0,255),
			new Color32(0,0,200,255),
			new Color32(255,255,255,255),
			new Color32(227, 224, 77, 255)
		};
		string[] colorNames = new string[] { "Red", "Blue", "White", "Yellow" };
		string[] buttonWords = new string[] { "HOLD:0.0004", "PRESS:0.0004", "DETONATE:0.000225", "ABORT:0.00035" };
		if (colorNames[_finalColor] == "White") 
		{
			_buttonText.color = new Color32(0, 0, 0, 255);
		}

		_button.GetComponent<Renderer>().material.color = colors[_finalColor];

		_buttonText.text = buttonWords[_finalWord].Split(':')[0];
		_buttonText.characterSize = float.Parse(buttonWords[_finalWord].Split(':')[1]);

		_requestingTap = GetButtonRule();

		Debug.LogFormat("[Everything #{0}]: The final panel was generated as The Button and is colored {1} and has a label of {2}. The variable 'X' is equal to {3}. The panel is requesting a {4}.", _modID, colorNames[_finalColor], buttonWords[_finalWord].Split(':')[0], _x, _requestingTap ? "tap" : "hold");
	}

	public override void Interact()
	{
		if (_module._isAnimating || _module._modSolved) { return; }

		_module._audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, _module.GetComponent<KMBombModule>().transform);

		holding = false;
		_module.StartCoroutine(HoldDelay(0.8f));
	}

	public override void InteractEnd()
	{
		if (_module._isAnimating || _module._modSolved) { return; }
		_module._audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, _module.GetComponent<KMBombModule>().transform);
		_module.StopAllCoroutines();
		int sec = (int)_module.GetBombInfo().GetTime() % 10;
		_module.StartCoroutine(LowerStrip());
		if (_module.GetFinalState()) 
		{
			if (_requestingTap && holding)
			{
				_module.GetModule().HandleStrike();
				Debug.LogFormat("[Everything #{0}]: The Button was requesting a tap and not a hold.", _modID);
				return;
			}
			else if (_requestingTap && !holding)
			{
				if (sec == _x)
				{
					Debug.LogFormat("[Everything #{0}]: The Button has been tapped at the correct time. Module Solved!", _modID);
					_module.GetModule().HandlePass();
					_module._modSolved = true;
					_buttonText.text = "SOLVED!";
					_buttonText.characterSize = 0.000325f;
					return;
				}
				else
				{
					_module.GetModule().HandleStrike();
					Debug.LogFormat("[Everything #{0}]: The Button expected a tap at {1} but was tapped at {2}.", _modID, _x, sec);
					return;
				}
			}
			else if (!_requestingTap && holding)
			{
				if (sec == _holdx)
				{
					Debug.LogFormat("[Everything #{0}]: The Button has been released at the correct time. Module Solved!", _modID);
					_module.GetModule().HandlePass();
					_module._modSolved = true;
					_buttonText.text = "SOLVED!";
					_buttonText.characterSize = 0.000325f;
					return;
				}
				else
				{
					_module.GetModule().HandleStrike();
					Debug.LogFormat("[Everything #{0}]: The Button expected a releasing at {1} but was released at {2}.", _modID, _holdx, sec);
					return;
				}
			}
			else 
			{
				_module.GetModule().HandleStrike();
				Debug.LogFormat("[Everything #{0}]: The Button was requesting a hold and not a tap.", _modID);
				return;
			}
		}
		if (holding && (sec == _correctDigit))
		{
			Debug.LogFormat("[Everything #{0}]: Button released at the correct time. Panel solved.", _modID, sec, _correctDigit);
			HandlePanelSolve();
			return;
		}
		else
		{
			_module.GetModule().HandleStrike();
			Debug.LogFormat("[Everything #{0}]: There was a strike on the Button panel. Released at {1} but expected {2}.", _modID, sec, _correctDigit);
			return;
		}
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

		if (_module.GetFinalState()) 
		{
			_buttonText.GetComponent<Renderer>().enabled = true;
		}

		_button.GetComponent<Renderer>().enabled = true;
		_button.Highlight.gameObject.SetActive(true);
		_strip.GetComponent<Renderer>().enabled = true;
		yield return new WaitForSeconds(0.25f);
		_module._isAnimating = false;
		yield break;
	}

	public override IEnumerator DisableComponents()
	{
		_module._isAnimating = true;

		if (_module.GetFinalState())
		{
			_buttonText.GetComponent<Renderer>().enabled = false;
		}

		_button.GetComponent<Renderer>().enabled = false;
		_button.Highlight.gameObject.SetActive(false);
		yield return new WaitForSeconds(0.5f);
		_strip.GetComponent<Renderer>().enabled = false;
		yield return new WaitForSeconds(0.25f);
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
		return new Vector3(0.11f, 0.01f, 0.085f);
	}

	IEnumerator HoldDelay(float delay)
	{
		yield return new WaitForSeconds(delay);
		holding = true;
		if (_module.GetFinalState())
		{
			_finalStripColor = rnd.Range(0, 4);
			int[] nums = new int[] { 0, 1, 4, 5 };
			int[] time = new int[] { 1, 1, 4, 5  };
			string[] names = new string[] { "White", "Red", "Blue", "Yellow" };
			Debug.Log(
				_requestingTap ? 
				String.Format("[Everything #{0}]: Unfortunately, holding was not the action the module asked for.", _modID) : 
				String.Format("[Everything #{0}]: Upon the button being held, the strip color is {1} which is release on {2}+{3} which the last digit is a {4}.", _modID, names[_finalStripColor], _x, time[_finalStripColor], (_x+time[_finalStripColor])%10));
			_holdx = (_x + time[_finalStripColor])%10;
			_strip.material = _stripColors[nums[_finalStripColor]];
		}
		_module.StartCoroutine(RaiseStrip());
		yield break;
	}

	IEnumerator RaiseStrip()
	{
		Transform strip = _strip.transform;

		while (!(strip.localPosition.y >= 0.017f))
		{
			float y = strip.localPosition.y;
			y += 0.0025f;
			strip.localPosition = new Vector3(strip.localPosition.x, y, strip.localPosition.z);
			yield return new WaitForSeconds(0.25f);
		}
		strip.localPosition = new Vector3(strip.localPosition.x, 0.017f, strip.localPosition.z);
		yield break;
	}

	IEnumerator LowerStrip()
	{

		Transform strip = _strip.transform;

		while (!(strip.localPosition.y <= 0.01f))
		{
			float y = strip.localPosition.y;
			y -= 0.001f;
			strip.localPosition = new Vector3(strip.localPosition.x, y, strip.localPosition.z);
			yield return new WaitForSeconds(0.25f);
		}
		strip.localPosition = new Vector3(strip.localPosition.x, 0.01f, strip.localPosition.z);
		yield break;
	}

	bool GetButtonRule() 
	{
		/*string[] colorNames = new string[] { "Red", "Blue", "White", "Yellow" };
		string[] buttonWords = new string[] { "HOLD:0.0004", "PRESS:0.0004", "DETONATE:0.000225", "ABORT:0.00035" };*/
		bool litfrk3bat = false;
		bool car = false;
		bool bat2 = false;
		if (_module.GetBombInfo().GetOnIndicators().Any(x => x == "FRK") && _module.GetBombInfo().GetBatteryCount() >= 3) litfrk3bat = true;
		if (_module.GetBombInfo().GetBatteryCount() >= 2) bat2 = true;
		if (_module.GetBombInfo().GetIndicators().Any(x => x == "CAR")) car = true;
		switch (_finalWord) 
		{
			case 0:
				switch (_finalColor) 
				{
					case 0:
						return true;
					case 1:
						return litfrk3bat;
					case 2:
						if (!car) return litfrk3bat;
						return false;
					case 3:
						return litfrk3bat;
					default:
						return false;
				}
			case 1:
				switch (_finalColor) 
				{
					case 0:
					case 1:
					case 3:
						return litfrk3bat;
					case 2:
						if (!car) return litfrk3bat;
						return false;
					default:
						return false;
				}
			case 2:
				if (bat2) return true;
				return false;
			case 3:
				switch (_finalColor)
				{
					case 0:
						return litfrk3bat;
					case 1:
						return false;
					case 2:
						if (!car) return litfrk3bat;
						return false;
					case 3:
						return litfrk3bat;
					default:
						return false;
				}
			default:
				return false;
		}
	}

}
