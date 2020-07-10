using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public class Everything : MonoBehaviour
{

	public KMBombInfo _bomb;
	public KMAudio _audio;

	public KMSelectable[] _moduleSelectors;
	public SpriteRenderer[] _moduleRenderers;
	public Sprite[] _moduleIcons;
	public MeshRenderer _moduleBasePanel;
	public TextMesh _panelUnlockCounter;
	public TextMesh _failSafeText;

	// Wires Panel

	public KMSelectable[] _wireSelectables;
	public MeshRenderer[] _cutWires;
	public Material[] _wireColors;

	// Button Panel

	public KMSelectable _button;
	public MeshRenderer _buttonStrip;
	public Material[] _buttonStripColors;
	public TextMesh _buttonWordText;

	// Keypad Panel

	public KMSelectable[] _keypadButtons;
	public TextMesh[] _keypadButtonTexts;
	public MeshRenderer[] _keypadButtonMeshes;

	// Simon Says Panel

	public KMSelectable[] _simonSaysButtons;
	public Light[] _simonSaysLights;
	public TextMesh _simonSaysStrikes;

	// WOF Panel

	public KMSelectable[] _WOFButtons;
	public TextMesh[] _WOFButtonTexts;
	public TextMesh _WOFDisplayText;
	public MeshRenderer[] _WOFAdditional;
	public GameObject _WOFStageIndicators;

	// Memory Panel

	public KMSelectable[] _memButtons;
	public TextMesh[] _memButtonTexts;
	public TextMesh _memDisplayText;
	public MeshRenderer[] _memAdditional;
	public TextMesh _memCountdownText;

	// Complicated Wires Panel

	public KMSelectable[] _compWireSelectables;
	public MeshRenderer[] _compCutWires;
	public Material[] _compWireColors;
	public TextMesh[] _compWireStars;
	public MeshRenderer[] _compWireLEDS;

	// Wire Sequence Panel

	public KMSelectable[] _wireSeqSelectables;
	public MeshRenderer[] _wireSeqCutWires;
	public Material[] _wireSeqColors;
	public TextMesh[] _wireSeqNumbers;
	public TextMesh[] _wireSeqLetters;
	public MeshRenderer[] _wireSeqAdditional;

	// Morse Code
	public MeshRenderer _morseCodeLight;
	public GameObject _morseFinalObject;
	public KMSelectable[] _morseButtons;

	// Password
	public MeshRenderer[] _pwArrowRenderers;
	public MeshRenderer[] _pwDisplayRenderers;
	public TextMesh[] _pwLetterTexts;
	public KMSelectable[] _pwUpArrows;
	public KMSelectable[] _pwDownArrows;
	public KMSelectable _pwSubmit;

	// Maze
	public GameObject _mazeBase;
	public MeshRenderer[] _mazeAllIndicators;
	public KMSelectable[] _mazeDirections;
	public MeshRenderer _mazeLED;
	public TextMesh _mazeLEDDirection;
	public KMSelectable _mazeReset;

	// Specifically for Logging
	static int _modIDCount = 1;
	int _modID;
	public bool _modSolved;


	//
	List<PanelInterface> _chosenPanels = new List<PanelInterface>();
	List<string> _chosenModules = new List<string>();

	int _activePanel = -1;

	public bool _isAnimating = false;
	public bool _finalOpen = false;
	string[] _ignoreHighlights = new string[] { "ModuleHighlight" };
	string[] _possibleModuleNames = new string[] { "Wires", "The Button", "Keypads", "Simon Says", "Who’s on First", "Memory", "Complicated Wires", "Wire Sequence", "Morse Code", "Password", "Maze" };
	string _correctDigits = "";
	bool[] _solvedPanels = new bool[4];
	Coroutine _controller = null;
    Coroutine _panelController = null;

	string _chosenFinal;
	PanelInterface _chosenFinalPanel;

	List<Vector3> _buttonPos = new List<Vector3>();
	List<Vector3> _renderPos = new List<Vector3>();

	bool _clickedFinalButton;
	bool _inShuffle;
	bool _failSafe;
	public bool _skipEnable;

	int _moduleCounter = 0;
	int _solveableModules;

	string[] _ignoreList = new string[]
	{
		"Everything", "14", "A>N<D", "Bamboozling Time Keeper", "Brainf---", "Busy Beaver", "Forget Enigma",
		"Forget Everything", "Forget Infinity", "Forget It Not", "Forget Me Not", "Forget Me Later", "Forget Perspective",
		"Forget The Colors", "Forget Them All", "Forget This", "Forget Us Not", "Iconic", "Kugelblitz", "Multitask", "OmegaForget",
		"Organization", "Password Destroyer", "Purgatory", "RPS Judging", "Simon Forgets", "Simon's Stages", "Souvenir", "Tallordered Keys",
		"The Time Keeper", "The Troll", "The Twin", "The Very Annoying Button", "Timing Is Everything", "Turn The Key",
		"Ultimate Custom Night", "Übermodule"
	};

	Color32[] _unlockTextColors = new Color32[]
	{
		new Color32(255, 0, 0, 255), // red
		new Color32(224, 224, 38, 255), // yellow
		new Color32(0, 255, 0, 255), // green
	};

	void Awake()
	{
		_modID = _modIDCount++;

		foreach (KMSelectable km in _moduleSelectors)
		{
			km.OnInteract += delegate () { if (_modSolved || _isAnimating) { return false; } ModuleButton(km); return false; };
		}
		_ignoreList = GetComponent<KMBossModule>().GetIgnoredModules(GetModule(), _ignoreList);
	}

	void Start()
	{
		StartCoroutine(DisableHighlights());
		ChooseModules();
		foreach (PanelInterface pi in _chosenPanels)
		{
			if (pi == null) { Debug.LogFormat("[Everything #{0}]: A panel was unable to be found. This is most likely a bug, please report it!", _modID); continue; }
			pi.GeneratePanel();
			_correctDigits += pi.GetCorrectDigit().ToString();
		}
		/*int test = Array.IndexOf(_chosenModules.ToArray(), "Password");
		if (test != -1) { _chosenPanels[test].GenerateFinalPanel(); _finalOpen = true; }*/
		Debug.LogFormat("[Everything #{0}]: Taking all of the correct digits from each panel gives: {1}.", _modID, _correctDigits);
		_solveableModules = _bomb.GetSolvableModuleNames().Where(x => !_ignoreList.Contains(x)).Count();
		_panelUnlockCounter.text = _moduleCounter.ToString();
		Debug.LogFormat("[Everything #{0}]: The final panel will open after all panels and its detected {1} solvable modules have been solved.", _modID, _solveableModules);

		if (_solveableModules == 0 && _solvedPanels.All(x => x))
		{
			_failSafe = true;
			Debug.LogFormat("[Everything #{0}]: Entering into failsafe due to all panels being instantly solved and no solveable modules.", _modID);
		}

		foreach (Light l in GetComponentsInChildren<Light>())
		{
			l.range *= transform.lossyScale.x;
		}

		foreach (Vector3 vec in _moduleSelectors.Select(x => x.transform.localPosition))
		{
			_buttonPos.Add(vec);
		}

		foreach (Vector3 vec in _moduleRenderers.Select(x => x.transform.localPosition))
		{
			_renderPos.Add(vec);
		}

	}

	void FixedUpdate()
	{
		if (!_modSolved && !_finalOpen)
		{
			if (_solvedPanels.All(x => x) && _solveableModules - _moduleCounter == 0)
			{
				_finalOpen = true;
				_panelUnlockCounter.GetComponent<Renderer>().enabled = false;

				StartCoroutine(StartFinalAnimation());
				return;
			}
			if (_moduleCounter != _bomb.GetSolvedModuleNames().Count())
			{
				_moduleCounter = _bomb.GetSolvedModuleNames().Count();
				_panelUnlockCounter.text = _moduleCounter.ToString();
			}
			if (_solvedPanels.All(x => x) || _solveableModules - _moduleCounter == 0 && _panelUnlockCounter.color != _unlockTextColors[1])
			{
				_panelUnlockCounter.color = _unlockTextColors[1];
			}
		}
	}

	//

	void ModuleButton(KMSelectable but)
	{
		int index = Array.IndexOf(_moduleSelectors, but);

		if (_isAnimating) return;

		if (_finalOpen && index == 4 && !_isAnimating || _inShuffle)
		{
			if (_chosenFinal == null)
			{
				_failSafeText.text = "";
				_clickedFinalButton = true;

				List<string> possChooses = new List<string>();

				foreach (string s in _possibleModuleNames)
				{
					if (_bomb.GetSolvableModuleNames().Where(x => !_ignoreList.Contains(x)).ToList().Contains(s))
					{
						possChooses.Add(s);
					}
				}

				if (possChooses.Count() == 0)
				{
					Debug.LogFormat("[Everything #{0}]: No possible modules on the bomb that it can choose from. Choosing at random from supported modules...", _modID);
					_chosenFinal = _possibleModuleNames[rnd.Range(0, _possibleModuleNames.Length)];
					_chosenFinalPanel = GetPanel(_chosenFinal, 5);
				}
				else
				{
					_chosenFinal = possChooses.PickRandom();
					_chosenFinalPanel = GetPanel(_chosenFinal, 5);
				}

				_moduleRenderers[4].sprite = _moduleIcons[Array.IndexOf(GetModuleSpriteNames(), _chosenFinal)];

				Debug.LogFormat("[Everything #{0}]: The module chosen for the final panel is: {1}", _modID, _chosenFinal);

				StartCoroutine(DelayComponents(index, true));
				UpdateModuleButton(index, _activePanel);
				_chosenFinalPanel.GenerateFinalPanel();
				_activePanel = index;
				return;
			}
			StartCoroutine(DelayComponents(index, true));
			UpdateModuleButton(index, _activePanel);
			_activePanel = index;
			return;
		}

		if (index > 3 || _chosenPanels.ElementAt(index) == null || _activePanel == index || _isAnimating) { return; }
		StartCoroutine(DelayComponents(index, false));
		UpdateModuleButton(index, _activePanel);
		_activePanel = index;
	}

	void ChooseModules()
	{
		List<string> temp = new List<string>();
		foreach (string moduleName in _bomb.GetSolvableModuleNames())
		{
			if (_possibleModuleNames.Any(x => x == moduleName))
			{
				if (_chosenModules.Contains(moduleName)) continue;
				_chosenModules.Add(moduleName);
				temp.Add(moduleName);
			}
		}
		Debug.Log(_chosenModules.Count() != 0 ? String.Format("[Everything #{0}]: Found {1} modules on the bomb that Everything supports: {2}.", _modID, _chosenModules.Count(), _chosenModules.Join(", ")) : String.Format("[Everything #{0}]: Found no supported modules.", _modID));
		if (_chosenModules.Count() != 4)
		{
			int co = _chosenModules.Count();
			for (int i = 0; i < 4 - co; i++)
			{
				int module = rnd.Range(0, _possibleModuleNames.Length);
				while (_chosenModules.Contains(_possibleModuleNames[module]))
				{
					module = rnd.Range(0, _possibleModuleNames.Length);
				}
				_chosenModules.Add(_possibleModuleNames[module]);
			}
			Debug.LogFormat("[Everything #{0}]: Didn't find enough modules on the bomb that Everything supports. Adding {1} more from random vanillas: {2}", _modID, 4 - temp.Count(), _chosenModules.Where(x => !temp.Contains(x)).Join(", "));
		}

		int count = 0;
		foreach (string mod in _chosenModules)
		{
			PanelInterface pi = GetPanel(mod, count);
			_chosenPanels.Add(pi);
			_moduleRenderers[count].sprite = _moduleIcons[Array.IndexOf(GetModuleSpriteNames(), mod)];
			count++;
		}

	}

	void UpdateModuleButton(int n, int o)
	{
		foreach (KMSelectable km in _moduleSelectors)
		{
			if (km.GetComponent<Renderer>().material.color == new Color32(0, 169, 0, 255)) { continue; }
			km.GetComponent<Renderer>().material.color = new Color32(77, 77, 77, 255);
		}
		_moduleSelectors[n].GetComponent<Renderer>().material.color = new Color32(39, 39, 39, 255);
		if (o == -1 || o == 4) return;
		_moduleSelectors[o].GetComponent<Renderer>().material.color = _solvedPanels[o] ? new Color32(0, 169, 0, 255) : new Color32(77, 77, 77, 255);
		return;
	}

	PanelInterface GetPanel(string name, int solvedIndex)
	{
		switch (name)
		{
			case "Wires":
				Wires wires = new Wires(this, _modID, solvedIndex, _wireSelectables, _cutWires, _wireColors);
				foreach (KMSelectable km in _wireSelectables)
				{
					km.OnInteract = delegate () { wires.Interact(km); return false; };
				}
				return wires;
			case "The Button":
				Button button = new Button(this, _modID, solvedIndex, _button, _buttonStrip, _buttonStripColors, _buttonWordText);
				_button.OnInteract = delegate () { button.Interact(); return false; };
				_button.OnInteractEnded = delegate () { button.InteractEnd(); return; };
				return button;
			case "Keypads":
				Keypads keypads = new Keypads(this, _modID, solvedIndex, _keypadButtons, _keypadButtonTexts, _keypadButtonMeshes);
				foreach (KMSelectable km in _keypadButtons)
				{
					km.OnInteract = delegate () { keypads.Interact(km); return false; };
				}
				return keypads;
			case "Simon Says":
				SimonSays ss = new SimonSays(this, _modID, solvedIndex, _simonSaysButtons, _simonSaysLights, _simonSaysStrikes);
				foreach (KMSelectable km in _simonSaysButtons)
				{
					km.OnInteract = delegate () { ss.Interact(km); return false; };
				}
				return ss;
			case "Who’s on First":
				WhosOnFirst wof = new WhosOnFirst(this, _modID, solvedIndex, _WOFButtons, _WOFButtonTexts, _WOFDisplayText, _WOFAdditional, _WOFStageIndicators);
				foreach (KMSelectable km in _WOFButtons)
				{
					km.OnInteract = delegate () { wof.Interact(km); return false; };
				}
				return wof;
			case "Memory":
				Memory mem = new Memory(this, _modID, solvedIndex, _memButtons, _memButtonTexts, _memDisplayText, _memAdditional, _memCountdownText);
				foreach (KMSelectable km in _memButtons)
				{
					km.OnInteract = delegate () { mem.Interact(km); return false; };
				}
				return mem;
			case "Complicated Wires":
				CompWires compwires = new CompWires(this, _modID, solvedIndex, _compWireSelectables, _compCutWires, _compWireColors, _compWireStars, _compWireLEDS);
				foreach (KMSelectable km in _compWireSelectables)
				{
					km.OnInteract = delegate () { compwires.Interact(km); return false; };
				}
				return compwires;
			case "Wire Sequence":
				WireSequence wireSeq = new WireSequence(this, _modID, solvedIndex, _wireSeqSelectables, _wireSeqCutWires, _wireSeqColors, _wireSeqNumbers, _wireSeqLetters, _wireSeqAdditional);
				foreach (KMSelectable km in _wireSeqSelectables)
				{
					km.OnInteract = delegate () { wireSeq.Interact(km); return false; };
				}
				return wireSeq;
			case "Morse Code":
				MorseCode morseCode = new MorseCode(this, _modID, solvedIndex, _morseCodeLight, _morseButtons, _morseFinalObject);
				foreach (KMSelectable km in _morseButtons)
				{
					km.OnInteract = delegate () { morseCode.Interact(km); return false; };
				}
				return morseCode;
			case "Password":
				Password password = new Password(this, _modID, solvedIndex, _pwArrowRenderers, _pwDisplayRenderers, _pwLetterTexts, _pwUpArrows, _pwDownArrows, _pwSubmit);
				foreach (KMSelectable km in _pwUpArrows)
				{
					km.OnInteract = delegate () { password.Interact(km); return false; };
				}
				foreach (KMSelectable km in _pwDownArrows)
				{
					km.OnInteract = delegate () { password.Interact(km); return false; };
				}
				_pwSubmit.OnInteract = delegate () { password.Interact(_pwSubmit); return false; };
				return password;
			case "Maze":
				Maze maze = new Maze(this, _modID, solvedIndex, _mazeBase, _mazeAllIndicators, _mazeDirections, _mazeLED, _mazeLEDDirection, _mazeReset);
				foreach (KMSelectable km in _mazeDirections)
				{
					km.OnInteract = delegate () { maze.Interact(km); return false; };
				}
				_mazeReset.OnInteract = delegate () { maze.Interact(_mazeReset); return false; };
				return maze;
			default:
				return null;
		}
	}

	string[] GetModuleSpriteNames()
	{
		List<string> names = new List<string>();
		foreach (Sprite s in _moduleIcons)
		{
			names.Add(s.name);
		}
		return names.ToArray();
	}

	//

	IEnumerator DelayComponents(int index, bool final)
	{
		_isAnimating = true;
		if (final)
		{
			if (!_skipEnable) 
			{
				while (_panelController != null) { yield return null; }
				_panelController = StartCoroutine(_chosenFinalPanel.ChangeBaseSize(0.025f));
				while (_panelController != null) { yield return null; }
				_panelController = StartCoroutine(_chosenFinalPanel.EnableComponents());
				while (_panelController != null) { yield return null; }
				_isAnimating = false;
			}
			yield break;
		}
        if (_activePanel != -1) { _panelController = StartCoroutine(_chosenPanels[_activePanel].DisableComponents()); }
		while (_panelController != null) { yield return null; }
		if (!_skipEnable)
		{
			_panelController = StartCoroutine(_chosenPanels[index].ChangeBaseSize(0.025f));
			while (_panelController != null) { yield return null; }
			_panelController = StartCoroutine(_chosenPanels[index].EnableComponents());
			while (_panelController != null) { yield return null; }
		}
		_isAnimating = false;
		yield break;
	}

	IEnumerator DisableHighlights()
	{
		yield return new WaitForSeconds(0.7f);
		foreach (KMSelectable km in GetComponent<KMSelectable>().Children)
		{
			if (_ignoreHighlights.Contains(km.Highlight.gameObject.name)) { continue; }
			km.Highlight.gameObject.SetActive(false);
		}
		yield break;
	}

	public KMBombInfo GetBombInfo()
	{
		return _bomb;
	}

	public string GetCorrectDigits()
	{
		return _correctDigits;
	}

	public bool GetFinalState()
	{
		return _finalOpen;
	}

	public KMBombModule GetModule()
	{
		return GetComponent<KMBombModule>();
	}

	public int GetDigitalRoot(int num)
	{
		while (num.ToString().Length != 1)
		{
			num = num.ToString().Select(x => int.Parse(x.ToString())).ToList().Sum();
		}
		return num;
	}

	public bool[] GetSolvedPanels()
	{
		return _solvedPanels;
	}

	public void SetSolvedPanel(int index, bool boolean)
	{
		_solvedPanels[index] = boolean;
		return;
	}

	public void Strike()
	{
		GetModule().HandleStrike();
		if (_finalOpen)
		{
			for (int i = 0; i <= 4; i++)
			{
				_moduleSelectors[i].GetComponent<Renderer>().enabled = true;
				_moduleRenderers[i].enabled = true;
				_moduleSelectors[i].Highlight.gameObject.SetActive(true);
				_controller = StartCoroutine(MoveModuleButton(new Transform[] { _moduleSelectors[i].transform, _moduleRenderers[i].transform }, 3f, 0.01f, 1f, 0.01f, new Vector3[] { _buttonPos[i], _renderPos[i] }));
			}
		}
	}

	// Animations

	void MoveButtons(Transform[] buttons, Vector3 target, float speed)
	{
		foreach (Transform t in buttons)
		{
			t.localPosition = Vector3.Lerp(t.localPosition, target, speed * Time.deltaTime);
		}
	}

	void MoveButtons(Transform t, Vector3 target, float speed)
	{
		t.localPosition = Vector3.Lerp(t.localPosition, target, speed * Time.deltaTime);
	}

	void StartNextAnimation()
	{
		if (_controller != null)
		{
			StopCoroutine(_controller);
			_controller = null;
		}
	}

	public void StartNextPanelAnimation()
	{
		if (_panelController != null)
		{
			StopCoroutine(_panelController);
			_panelController = null;
		}
	}

	IEnumerator StartFinalAnimation()
	{
		if (_failSafe)
		{
			_failSafeText.text = "Fail";
			yield return new WaitForSeconds(0.5f);
			_failSafeText.text = "safe";
			yield return new WaitForSeconds(0.5f);
			_failSafeText.text = _correctDigits;
		}

		if (_activePanel != -1)
		{
			_controller = StartCoroutine(_chosenPanels[_activePanel].DisableComponents());
		}

		StartCoroutine(ChangeBaseSize(0.025f));

		while (_isAnimating != false) yield return null;

		_controller = null;

		_controller = StartCoroutine(MoveModuleButtons(3f, 0.01f, 1f, 0.01f, new Vector3[] { new Vector3(-0.021f, 0.015f, 0.0676f), new Vector3(-0.021f, 0.0176f, 0.0676f) }));

		while (_controller != null) yield return null;

		for (int i = 0; i <= 3; i++)
		{
			_moduleRenderers[i].enabled = false;
			_moduleSelectors[i].GetComponent<Renderer>().enabled = false;
			_moduleSelectors[i].Highlight.gameObject.SetActive(false);
		}

		yield return new WaitForSeconds(0.75f);

		_moduleRenderers[4].enabled = true;
		_moduleSelectors[4].Highlight.gameObject.SetActive(true);

		_controller = StartCoroutine(ShuffleModuleIcons(2f / _moduleIcons.Length, _moduleRenderers[4], _moduleIcons));

		yield break;
	}

	IEnumerator MoveModuleButtons(float stopSpeed, float delayIncrease, float speed, float speedIncrease, Vector3[] targets)
	{
		_isAnimating = true;
		Transform[] buttons = _moduleSelectors.Where(x => x.gameObject.name != "Module3").Select(x => x.transform).ToArray();
		Transform[] sprites = _moduleRenderers.Where(x => x.gameObject.name != "MSprite3").Select(x => x.transform).ToArray();
		while (speed < stopSpeed)
		{
			MoveButtons(buttons, targets[0], speed);
			MoveButtons(sprites, targets[1], speed);
			speed += speedIncrease;
			yield return new WaitForSeconds(delayIncrease);
		}
		_isAnimating = false;
		StartNextAnimation();
		yield break;
	}

	IEnumerator MoveModuleButton(Transform[] ts, float stopSpeed, float delayIncrease, float speed, float speedIncrease, Vector3[] targets)
	{
		_isAnimating = true;
		while (speed < stopSpeed)
		{
			MoveButtons(ts[0], targets[0], speed);
			MoveButtons(ts[1], targets[1], speed);
			speed += speedIncrease;
			yield return new WaitForSeconds(delayIncrease);
		}
		_isAnimating = false;
	}

	IEnumerator ShuffleModuleIcons(float switchDelay, SpriteRenderer renderer, Sprite[] sprites)
	{
		int index = 0;
		_inShuffle = true;
		while (!_clickedFinalButton)
		{
			renderer.sprite = sprites[index];
			index++;
			if (index >= sprites.Length)
			{
				index = 0;
			}
			yield return new WaitForSeconds(switchDelay);
		}
		StartNextAnimation();
		yield break;
	}

	IEnumerator ChangeBaseSize(float delay)
	{
		_isAnimating = true;
		Vector3 baseSize = new Vector3(0.085f, 0.01f, 0.085f);
		Transform baseTrans = _moduleBasePanel.transform;
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
		_isAnimating = false;
		yield break;
	}

}