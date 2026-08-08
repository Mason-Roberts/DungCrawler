using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Dungeon : Node3D
{
	[Export]
	public GridMap GridMap { get; set; }

	[Export]
	public int MapSize { get; set; } = 30;

	public override void _Ready()
	{
		if (GridMap == null) return;

		WfcGenerator wfc = new WfcGenerator(GridMap, MapSize);
		wfc.Generate();
	}
}

public class WfcGenerator
{
	private readonly GridMap _gridMap;
	private readonly int _size;
	private readonly int _depth = 0;

	// Tile IDs from the mesh library
	private const int TileFloor = 0;   // 0W - no walls
	private const int Tile1W = 1;       // 1W - one wall
	private const int Tile2W = 2;       // 2W - two adjacent walls
	private const int Tile2WS = 3;      // 2WS - two walls short
	private const int Tile3W = 4;       // 3W - three walls
	private const int Tile4W = 5;       // 4W - four walls (corner)

	// Y-axis only rotations: 0=0°, 1=90°, 2=180°, 3=270°
	// Map to GridMap orientations: 0, 6, 12, 18
	private const int NumOrientations = 4;
	private static readonly int[] GridMapOrientations = new int[] { 0, 6, 12, 18 };

	private int[,] _state;          // -1 = uncollapsed, otherwise tile ID
	private int[,,] _orientations;  // possible orientations per cell (tile << 4 | orientIndex)
	private bool[,] _collapsed;    // has this cell been collapsed?

	public WfcGenerator(GridMap gridMap, int size)
	{
		_gridMap = gridMap;
		_size = size;
		_state = new int[size, size];
		_orientations = new int[size, size, NumOrientations];
		_collapsed = new bool[size, size];
	}

	public void Generate()
	{
		// Clear grid
		for (int x = 0; x < _size; x++)
		{
			for (int z = 0; z < _size; z++)
			{
				_state[x, z] = -1;
				_collapsed[x, z] = false;
			}
		}

		// Initialize all cells with all tiles and all orientations
		for (int x = 0; x < _size; x++)
		{
			for (int z = 0; z < _size; z++)
			{
				for (int tile = 0; tile < 6; tile++)
				{
					for (int orient = 0; orient < NumOrientations; orient++)
					{
						_orientations[x, z, orient] = (tile << 4) | orient;
					}
				}
			}
		}

		int iterations = 0;
		int maxIterations = _size * _size * 100;

		while (!IsComplete() && iterations < maxIterations)
		{
			int[] cell = GetLowestEntropyCell();
			if (cell == null)
			{
				// Contradiction - restart
				GD.Print($"WFC contradiction at iteration {iterations}, restarting...");
				Generate();
				return;
			}

			CollapseCell(cell[0], cell[1]);
			Propagate();
			iterations++;
		}

		// Write to grid
		for (int x = 0; x < _size; x++)
		{
			for (int z = 0; z < _size; z++)
			{
				if (_state[x, z] >= 0)
				{
					int orientIndex = _orientations[x, z, 0] & 0xF;
					int gridMapOrient = GridMapOrientations[orientIndex];
					_gridMap.SetCellItem(new Vector3I(x, _depth, z), _state[x, z], gridMapOrient);
				}
			}
		}

		GD.Print($"WFC generated {_size}x{_size} dungeon in {iterations} iterations");
	}

	private bool IsComplete()
	{
		for (int x = 0; x < _size; x++)
		{
			for (int z = 0; z < _size; z++)
			{
				if (!_collapsed[x, z]) return false;
			}
		}
		return true;
	}

	private int[] GetLowestEntropyCell()
	{
		int minEntropy = int.MaxValue;
		List<int[]> candidates = new List<int[]>();

		for (int x = 0; x < _size; x++)
		{
			for (int z = 0; z < _size; z++)
			{
				if (_collapsed[x, z]) continue;

				int entropy = CountValidOptions(x, z);
				if (entropy == 0) return null; // contradiction
				if (entropy < minEntropy)
				{
					minEntropy = entropy;
					candidates.Clear();
					candidates.Add(new int[] { x, z });
				}
				else if (entropy == minEntropy)
				{
					candidates.Add(new int[] { x, z });
				}
			}
		}

		if (candidates.Count == 0) return null;
		int[] chosen = candidates[(int)(GD.Randi() % candidates.Count)];
		return chosen;
	}

	private int CountValidOptions(int x, int z)
	{
		int count = 0;
		for (int i = 0; i < NumOrientations; i++)
		{
			if (_orientations[x, z, i] >= 0) count++;
		}
		return count;
	}

	private void CollapseCell(int x, int z)
	{
		// Collect all valid options
		List<int[]> options = new List<int[]>();
		for (int i = 0; i < NumOrientations; i++)
		{
			if (_orientations[x, z, i] >= 0)
			{
				options.Add(new int[] { _orientations[x, z, i] >> 4, _orientations[x, z, i] & 0xF });
			}
		}

		// Weighted selection: prefer more open tiles (fewer walls) for corridors
		List<(int tile, int orient, float weight)> weighted = new List<(int, int, float)>();
		foreach (var opt in options)
		{
			float weight = GetTileWeight(opt[0]);
			weighted.Add((opt[0], opt[1], weight));
		}

		float totalWeight = weighted.Sum(w => w.weight);
		int roll = (int)(GD.Randi() % totalWeight);
		float cumulative = 0;

		int chosenTile = options[0][0];
		int chosenOrient = options[0][1];

		foreach (var w in weighted)
		{
			cumulative += w.weight;
			if (roll < cumulative)
			{
				chosenTile = w.tile;
				chosenOrient = w.orient;
				break;
			}
		}

		_state[x, z] = chosenTile;
		_orientations[x, z, 0] = (chosenTile << 4) | chosenOrient;
		for (int i = 1; i < NumOrientations; i++)
		{
			_orientations[x, z, i] = -1;
		}
		_collapsed[x, z] = true;
	}

	private float GetTileWeight(int tile)
	{
		// Weight by inverse wall count to prefer open spaces
		return 1.0f / (tile + 1);
	}

	private void Propagate()
	{
		Queue<int[]> queue = new Queue<int[]>();

		// Add all cells to queue initially
		for (int x = 0; x < _size; x++)
		{
			for (int z = 0; z < _size; z++)
			{
				if (_collapsed[x, z])
				{
					queue.Enqueue(new int[] { x, z });
				}
			}
		}

		int[] dx = new int[] { 1, -1, 0, 0 };
		int[] dz = new int[] { 0, 0, 1, -1 };

		while (queue.Count > 0)
		{
			int[] cell = queue.Dequeue();
			int x = cell[0];
			int z = cell[1];

			for (int d = 0; d < 4; d++)
			{
				int nx = x + dx[d];
				int nz = z + dz[d];

				if (nx < 0 || nx >= _size || nz < 0 || nz >= _size) continue;
				if (!_collapsed[nx, nz]) continue;

				bool changed = ApplyConstraints(x, z, d, nx, nz, queue);
				if (changed)
				{
					// Also constrain the reverse direction
					int reverseDir = (d + 2) % 4;
					ApplyConstraints(nx, nz, reverseDir, x, z, queue);
				}
			}
		}
	}

	private bool ApplyConstraints(int fx, int fz, int fromDir, int tx, int tz, Queue<int[]> queue)
	{
		bool changed = false;

		for (int fi = 0; fi < NumOrientations; fi++)
		{
			if (_orientations[fx, fz, fi] < 0) continue;

			int tileA = _orientations[fx, fz, fi] >> 4;
			int orientA = _orientations[fx, fz, fi] & 0xF;

			// Get valid neighbors for this tile+orientation in the given direction
			List<int> validNeighborTiles = GetValidNeighbors(tileA, orientA, fromDir);

			// Filter neighbor orientations
			List<int> newNeighborOrientations = new List<int>();
			for (int ti = 0; ti < NumOrientations; ti++)
			{
				if (_orientations[tx, tz, ti] < 0) continue;

				int tileB = _orientations[tx, tz, ti] >> 4;
				int orientB = _orientations[tx, tz, ti] & 0xF;

				if (validNeighborTiles.Contains(tileB))
				{
					newNeighborOrientations.Add(ti);
				}
			}

			if (newNeighborOrientations.Count == 0)
			{
				// This orientation of the source cell is invalid
				_orientations[fx, fz, fi] = -1;
				changed = true;
			}
		}

		// Remove invalid tiles from source cell
		int remaining = 0;
		for (int i = 0; i < NumOrientations; i++)
		{
			if (_orientations[fx, fz, i] >= 0) remaining++;
		}

		if (remaining == 0) return false; // contradiction

		// If source was collapsed, it can't change - skip
		if (_collapsed[fx, fz]) return changed;

		if (changed && queue != null)
		{
			queue.Enqueue(new int[] { fx, fz });
		}

		return changed;
	}

	private List<int> GetValidNeighbors(int tile, int orientation, int direction)
	{
		// Directions: 0=+X(right), 1=-X(left), 2=+Z(front), 3=-Z(back)
		// orientation: 0=0°, 1=90°, 2=180°, 3=270° (Y-axis rotations only)

		List<int> validNeighbors = new List<int>();

		// Determine which wall faces this tile has in its base orientation (0)
		// Then rotate based on the orientation parameter
		bool[] walls = GetWallsForTile(tile);

		// Rotate walls based on Y-axis rotation
		bool[] rotatedWalls = RotateWalls(walls, orientation);

		// Check which wall face is in the direction of the neighbor
		bool hasWallInDirection = GetWallInDirection(rotatedWalls, direction);

		if (hasWallInDirection)
		{
			// Neighbor must have a wall on the opposite face
			validNeighbors = GetTilesWithWallOnAnySide();
		}
		else
		{
			// Neighbor can be any tile (no wall constraint)
			validNeighbors = new List<int> { TileFloor, Tile1W, Tile2W, Tile2WS, Tile3W, Tile4W };
		}

		return validNeighbors;
	}

	private bool[] GetWallsForTile(int tile)
	{
		// Returns [posX, negX, posZ, negZ] wall presence in base orientation
		switch (tile)
		{
			case TileFloor: // 0W - no walls
				return new bool[] { false, false, false, false };

			case Tile1W: // 1W - one wall on +X
				return new bool[] { true, false, false, false };

			case Tile2W: // 2W - two adjacent walls (+X and +Z, forming inner corner)
				return new bool[] { true, false, true, false };

			case Tile2WS: // 2WS - two walls short (same topology as 2W for constraints)
				return new bool[] { true, false, true, false };

			case Tile3W: // 3W - three walls (only negX missing)
				return new bool[] { true, true, true, false };

			case Tile4W: // 4W - all four walls (corner piece, but acts as enclosed)
				return new bool[] { true, true, true, true };

			default:
				return new bool[] { false, false, false, false };
		}
	}

	private bool[] RotateWalls(bool[] walls, int steps)
	{
		// Rotate the wall array clockwise by steps * 90 degrees (Y-axis rotation)
		// walls = [posX, negX, posZ, negZ]
		// After 90 deg Y-rotation: posX<-negZ, negX<-posZ, posZ<-posX, negZ<-negX
		bool[] rotated = new bool[4];
		steps = steps % 4;

		for (int i = 0; i < 4; i++)
		{
			rotated[(i + steps) % 4] = walls[i];
		}

		return rotated;
	}

	private bool GetWallInDirection(bool[] walls, int direction)
	{
		// direction: 0=+X, 1=-X, 2=+Z, 3=-Z
		return walls[direction];
	}

	private List<int> GetTilesWithWallOnAnySide()
	{
		// All tiles except floor have at least one wall
		return new List<int> { Tile1W, Tile2W, Tile2WS, Tile3W, Tile4W };
	}
}
