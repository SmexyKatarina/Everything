using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public class Maze : PanelInterface
{

	Everything _module;
	int _modID;
	int _solvedIndex;
	GameObject _mazeBase;
	MeshRenderer[] _allIndicators;
	MeshRenderer[][] _rowdMaze = new MeshRenderer[6][];
	KMSelectable[] _mazeArrows;
	MeshRenderer _mazeLED;
	TextMesh _mazeText;
	KMSelectable _mazeReset;

	Color32 _currentPos = new Color32(255, 255, 255, 255);
	Color32 _base = new Color32(25, 36, 47, 255);
	Color32 _nonStrike = new Color32(47, 47, 47, 255);
	Color32 _strike = new Color32(255, 0, 0, 255);
	Color32 _indiColor = new Color32(36, 102, 28, 255);

	int _correctDigit;
	int _curPosition = 0;
	int _statPosition = 0;

	int _finalStartingPosition = 0;

	string[] _AllMazes = new string[] //Left to right (,), top to bottom (:)
	{
			"12,13,23,12,13,3:02,12,03,01,13,23:02,01,23,12,13,023:02,1,013,03,1,023:012,13,23,13,3,02:01,3,01,03,1,03",
			"1,123,3,12,123,3:12,3,12,03,01,23:02,12,03,12,13,023:012,03,12,03,2,02:02,2,02,2,03,02:0,01,03,01,13,03",
			"12,13,23,2,12,23:0,2,02,01,03,02:12,023,02,12,23,02:02,02,02,02,02,02:02,01,03,02,02,02:01,13,13,03,01,03",
			"12,23,1,13,13,23:02,02,12,13,13,023:02,01,03,12,3,02:02,1,13,013,13,023:012,13,13,13,23,02:01,13,3,1,03,0",
			"1,13,13,13,123,23:12,13,13,123,03,0:012,23,1,03,12,23:02,01,13,23,0,02:02,12,13,013,3,02:0,01,13,13,13,03",
			"2,12,23,1,123,23:02,02,02,12,03,02:012,03,0,02,12,03:01,23,12,023,02,2:12,03,0,02,01,023:01,13,13,03,1,03",
			"12,13,13,23,12,23:02,12,3,01,03,02:01,03,12,3,12,03:12,23,012,13,03,2:02,0,01,13,23,02:01,13,13,13,013,03",
			"2,12,13,23,12,23:012,013,3,01,03,02:02,12,13,13,23,02:02,01,23,1,013,03:02,2,01,13,13,3:01,013,13,13,13,3",
			"2,12,13,13,123,23:02,02,12,3,02,02:02,013,03,23,03,02:02,2,12,03,1,023:02,02,02,12,23,0:01,03,01,03,01,3"
	};

	string[] _chosenMazeGrid;

	int _chosenMaze;

	int[][] _mazeIndicators = new int[][]
	{
		new int[] { 6, 17 },
		new int[] { 10, 19 },
		new int[] { 21, 23 },
		new int[] { 0, 18 },
		new int[] { 16, 33 },
		new int[] { 4, 26 },
		new int[] { 1, 31 },
		new int[] { 3, 20 },
		new int[] { 8, 24 }
	};

	int _redTriangle = 0;

	public Maze(Everything _module, int _modID, int _solvedIndex, GameObject _mazeBase, MeshRenderer[] _allIndicators, KMSelectable[] _mazeArrows, MeshRenderer _mazeLED, TextMesh _mazeText, KMSelectable _mazeReset)
	{
		this._module = _module;
		this._modID = _modID;
		this._solvedIndex = _solvedIndex;
		this._mazeBase = _mazeBase;
		this._allIndicators = _allIndicators;
		this._mazeArrows = _mazeArrows;
		this._mazeLED = _mazeLED;
		this._mazeText = _mazeText;
		this._mazeReset = _mazeReset;
	}

	public override void GeneratePanel()
	{
		_curPosition = rnd.Range(0, 36);
		for (int x = 0; x < 5; x++)
		{
			MeshRenderer[] mr = new MeshRenderer[5];
			for (int y = 0; y < 5; y++)
			{
				mr[y] = _allIndicators[y + (6 * x)];
			}
			_rowdMaze[x] = mr;
		}
		_allIndicators[_curPosition].material.color = _currentPos;
		_chosenMaze = rnd.Range(0, 9);
		_chosenMazeGrid = _AllMazes[_chosenMaze].Split(':');

		_correctDigit = _chosenMaze + 1;

		Debug.LogFormat("[Everything #{0}]: The Maze panel was generated with the maze {1}. The correct digit for this panel is: {2}.", _modID, _chosenMaze + 1, _correctDigit);
		HandlePanelSolve();
	}

	public override void GenerateFinalPanel()
	{
		int[] digits = _module.GetCorrectDigits().Select(x => int.Parse(x.ToString())).ToArray();

		int maze = rnd.Range(0, 9);
		_chosenMazeGrid = _AllMazes[maze].Split(':');

		_finalStartingPosition = GetPosition(digits[0] % 6, digits[1] % 6);
		_curPosition = GetPosition(digits[0] % 6, digits[1] % 6);
		_redTriangle = GetPosition(digits[2] % 6, digits[3] % 6);

		foreach (MeshRenderer mr in _allIndicators)
		{
			mr.material.color = _base;
		}

		int[] indicators = _mazeIndicators[maze];
		_allIndicators[indicators[0]].material.color = _indiColor;
		_allIndicators[indicators[1]].material.color = _indiColor;

		Debug.LogFormat("[Everything #{0}]: The final panel was generated as Maze. Maze generated was {1}. Red triangle and White LED are at positions in reading order {2} and {3}, respectively.", _modID, maze + 1, _redTriangle + 1, _curPosition + 1);
	}

	public override void Interact(KMSelectable km)
	{
		int index = Array.IndexOf(_mazeArrows, km);
		if (_module._modSolved || _module._isAnimating) return;
		if (_module.GetFinalState() && km.gameObject.name == "MazeReset")
		{
			_curPosition = _finalStartingPosition;
			Debug.LogFormat("[Everything #{0}]: Maze reset button pressed. Reverting to starting position...", _modID);
			return;
		}
		_statPosition = _curPosition;

		bool wall = HandleMazeMovement(_curPosition, index);
		if (!wall)
		{
			if (_module.GetFinalState())
			{
				_module.Strike();
				Debug.LogFormat("[Everything #{0}]: Struck! Hit a wall when trying to move.", _modID);
				return;
			}
			UpdateStrike(index, true);
			return;
		}
		UpdateStrike(index, false);
		switch (index)
		{
			case 0:
				if (_curPosition - 6 < 0) break;
				_curPosition -= 6;
				if (!_module.GetFinalState())
				{
					_allIndicators[_statPosition].material.color = _base;
					_allIndicators[_curPosition].material.color = _currentPos;
				}
				break;
			case 1:
				if ((_curPosition % 6) + 1 > 5) break;
				_curPosition++;
				if (!_module.GetFinalState())
				{
					_allIndicators[_statPosition].material.color = _base;
					_allIndicators[_curPosition].material.color = _currentPos;
				}
				break;
			case 2:
				if (_curPosition + 6 > 35) break;
				_curPosition += 6;
				if (!_module.GetFinalState())
				{
					_allIndicators[_statPosition].material.color = _base;
					_allIndicators[_curPosition].material.color = _currentPos;
				}
				break;
			case 3:
				if ((_curPosition % 6) - 1 < 0) break;
				_curPosition--;
				if (!_module.GetFinalState())
				{
					_allIndicators[_statPosition].material.color = _base;
					_allIndicators[_curPosition].material.color = _currentPos;
				}
				break;
			default:
				break;
		}

		if (_curPosition == _redTriangle)
		{
			Debug.LogFormat("[Everything #{0}]: LED was moved to the red triangle successfully. Module solved.", _modID);
			_module._modSolved = true;
			_module.GetModule().HandlePass();
			foreach (MeshRenderer mr in _allIndicators)
			{
				mr.material.color = new Color32(0, 255, 0, 255);
			}
			return;
		}

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
		_mazeBase.SetActive(true);
		yield return new WaitForSeconds(.1f);
		foreach (KMSelectable km in _mazeArrows)
		{
			km.GetComponent<Renderer>().enabled = true;
			yield return new WaitForSeconds(.1f);
		}
		foreach (KMHighlightable kh in _mazeArrows.Select(x => x.Highlight))
		{
			kh.gameObject.SetActive(true);
		}
		if (!_module.GetFinalState())
		{
			_mazeLED.enabled = true;
			_mazeText.GetComponent<Renderer>().enabled = true;
		}
		else
		{
			_mazeReset.GetComponent<Renderer>().enabled = true;
			_mazeReset.Highlight.gameObject.SetActive(true);
		}
		_module.StartNextPanelAnimation();
		yield break;
	}

	public override IEnumerator DisableComponents()
	{
		_mazeLED.enabled = false;
		_mazeText.GetComponent<Renderer>().enabled = false;
		yield return new WaitForSeconds(.1f);
		_mazeReset.Highlight.gameObject.SetActive(false);
		foreach (KMHighlightable kh in _mazeArrows.Select(x => x.Highlight))
		{
			kh.gameObject.SetActive(false);
		}
		foreach (KMSelectable km in _mazeArrows)
		{
			km.GetComponent<Renderer>().enabled = false;
			yield return new WaitForSeconds(.1f);
		}
		_mazeReset.GetComponent<Renderer>().enabled = false;
		_mazeBase.SetActive(false);
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

	bool HandleMazeMovement(int position, int direction) // if false, cant move in direction and if true, can move in direction.
	{
		int row = position / 6;
		int col = position % 6;
		string movement = _chosenMazeGrid[row].Split(',')[col];
		return movement.Any(x => int.Parse(x.ToString()) == direction);
	}

	void UpdateStrike(int direction, bool strike)
	{
		if (strike)
		{
			_mazeText.text = "URDL"[direction].ToString();
			_mazeLED.material.color = _strike;
			return;
		}
		_mazeText.text = "";
		_mazeLED.material.color = _nonStrike;
	}

	int GetPosition(int row, int column)
	{
		return (row * 6) + column;
	}

}
