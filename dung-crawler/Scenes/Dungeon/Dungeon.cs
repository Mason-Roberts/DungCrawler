using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

public partial class Dungeon : Node3D
{
	[Export]
	public GridMap GridMap { get; set; }

	[Export]
	public int MapSize { get; set; } = 30;

	private Constraints _constraints { get; set; }

	public override void _Ready()
	{
		if (GridMap == null)
		{		
			GD.PrintErr("GridMap is null");
			return;
		}

		FileAccess file = FileAccess.Open("res://assets/constraints/tile_constraints.json", FileAccess.ModeFlags.Read);
		if (file == null)
		{
			GD.PrintErr($"Could not open constraint file: res://assets/constraints/tile_constraints.json");
			return;
		}

		string text = file.GetAsText();

		_constraints = JsonSerializer.Deserialize<Constraints>(file.GetAsText());

		WfcGenerator wfc = new WfcGenerator(GridMap, MapSize, _constraints, 9, -1);
		wfc.Generate();
	}
}

public class WfcGenerator
{
	private readonly GridMap _gridMap;
	private readonly int _size;
	private readonly Constraints _constraints;
	private DungeonCell[,] _grid;
	private int _gateTileId;
	private int _blankTileId;
	private int _collapsedCells;
	private int _totalCells;
	private Random _rand;

	public WfcGenerator(GridMap gridMap, int size, Constraints constraints, int gateTileId, int blankTileId)
	{
		_gridMap = gridMap;
		_size = size;
		_grid = new DungeonCell[_size, _size];
		_constraints = constraints;
		_blankTileId = blankTileId;
		_gateTileId = gateTileId;
		_totalCells = _size * _size;
		_collapsedCells = 0;
		_rand = new Random();
	}

	private void InitializeMapData()
	{
		_gridMap.Clear();
		var usableTiles = _constraints.Tiles.Where(x => x.Key != _blankTileId.ToString() && x.Key != _gateTileId.ToString()).Select(x => x.Value.GridMapIndex).ToList();

		List<string> specialTiles = new List<string>() { _blankTileId.ToString(), _gateTileId.ToString() };
		List<int> borderStraightTiles = new List<int>(_constraints.Tiles.Where(x => 
			!specialTiles.Contains(x.Key) && x.Value.Sockets.Any(w => w == "W"))
			.Select(x => x.Value.GridMapIndex).ToList());

		List<int> borderCornerTiles = new List<int>(_constraints.Tiles.Where(x => 
			!specialTiles.Contains(x.Key) && x.Value.Sockets[2] == "W" && x.Value.Sockets[3] == "W")
			.Select(x => x.Value.GridMapIndex).ToList());

		for (int x = 0; x < _size; x++)
		{
			for (int z = 0; z < _size; z++)
			{
				// Corners have even less usable tiles due to border constraints
				if ((z == 0 && x == 0) || (z == _size -1 && x == _size - 1) || (z == 0 && x == _size -1) || (z == _size -1 && x == 0))
				{				
					var tiles = new List<int>(usableTiles.Where(x => borderCornerTiles.Contains(x)));
					var cell = new DungeonCell()
					{
						Orientation = 0,
						AvailableTiles = tiles
					};
					_grid[x,z] = cell;
				}
				else if (z == 0 || z == _size -1 || x == 0 || x == _size - 1) // Border straights need to have a wall
				{
					var tiles = new List<int>(usableTiles.Where(x => borderStraightTiles.Contains(x)));
					var cell = new DungeonCell()
					{
						Orientation = 0,
						AvailableTiles = tiles
					};
					_grid[x,z] = cell;
				}
				else
				{
					var tiles = new List<int>(usableTiles);
					var cell = new DungeonCell()
					{
						Orientation = 0,
						AvailableTiles = tiles
					};
					_grid[x,z] = cell;
				}
			}
		}
	}

	private void CollapseCell()
	{
		// initial cell should always be the gate from a random part of the border
		if (_collapsedCells == 0)
		{
			bool useX = _rand.Next(0, 1) == 1;
			bool useZero = _rand.Next(0, 1) == 1;

			int coord = _rand.Next(1, _size - 2); 
			int side = useZero ? 0 : _size - 1;

			if (useX)
			{
				_grid[coord, side].Collapsed = _gateTileId;
				_grid[coord, side].Orientation = side == _size - 1 ? 270 : 90;
			}
			else
			{
				_grid[side, coord].Collapsed = _gateTileId;
				_grid[side, coord].Orientation = side == _size - 1 ? 180 : 0;
			}

		}
	}

	public void Generate()
	{
		InitializeMapData();
		
		while (_collapsedCells < _totalCells)
		{
			CollapseCell();
		}
	}

}

// public class WfcGenerator
// {
// 	private readonly GridMap _gridMap;
// 	private readonly int _size;
// 	private readonly int _depth = 0;

// 	// Tile IDs from the mesh library
// 	private const int TileFloor = 0;   // 0W - no walls
// 	private const int Tile1W = 1;       // 1W - one wall
// 	private const int Tile2W = 2;       // 2W - two adjacent walls
// 	private const int Tile2WS = 3;      // 2WS - two walls short
// 	private const int Tile3W = 4;       // 3W - three walls
// 	private const int Tile4W = 5;       // 4W - four walls (corner)

// 	// Y-axis only rotations: 0=0°, 1=90°, 2=180°, 3=270°
// 	// Map to GridMap orientations: 0, 6, 12, 18
// 	private const int NumOrientations = 4;
// 	private static readonly int[] GridMapOrientations = new int[] { 0, 6, 12, 18 };

// 	private int[,] _state;          // -1 = uncollapsed, otherwise tile ID
// 	private int[,,] _orientations;  // possible orientations per cell (tile << 4 | orientIndex)
// 	private bool[,] _collapsed;    // has this cell been collapsed?

// 	public WfcGenerator(GridMap gridMap, int size)
// 	{
// 		_gridMap = gridMap;
// 		_size = size;
// 		_state = new int[size, size];
// 		_orientations = new int[size, size, NumOrientations];
// 		_collapsed = new bool[size, size];
// 	}

// 	public void Generate()
// 	{
// 		// Clear grid
// 		for (int x = 0; x < _size; x++)
// 		{
// 			for (int z = 0; z < _size; z++)
// 			{
// 				_state[x, z] = -1;
// 				_collapsed[x, z] = false;
// 			}
// 		}

// 		// Initialize all cells with all tiles and all orientations
// 		for (int x = 0; x < _size; x++)
// 		{
// 			for (int z = 0; z < _size; z++)
// 			{
// 				for (int tile = 0; tile < 6; tile++)
// 				{
// 					for (int orient = 0; orient < NumOrientations; orient++)
// 					{
// 						_orientations[x, z, orient] = (tile << 4) | orient;
// 					}
// 				}
// 			}
// 		}

// 		// Pre-collapse boundary cells to sealed walls
// 		SetBoundaryWalls();

// 		int iterations = 0;
// 		int maxIterations = _size * _size * 100;

// 		while (!IsComplete() && iterations < maxIterations)
// 		{
// 			int[] cell = GetLowestEntropyCell();
// 			if (cell == null)
// 			{
// 				// Contradiction - restart
// 				GD.Print($"WFC contradiction at iteration {iterations}, restarting...");
// 				Generate();
// 				return;
// 			}

// 			CollapseCell(cell[0], cell[1]);
// 			Propagate();
// 			iterations++;
// 		}

// 		// Write to grid
// 		for (int x = 0; x < _size; x++)
// 		{
// 			for (int z = 0; z < _size; z++)
// 			{
// 				if (_collapsed[x, z])
// 				{
// 					int orientIndex = _orientations[x, z, 0] & 0x3;
// 					int gridMapOrient = GridMapOrientations[orientIndex];
// 					_gridMap.SetCellItem(new Vector3I(x, _depth, z), _state[x, z], gridMapOrient);
// 				}
// 			}
// 		}

// 		GD.Print($"WFC generated {_size}x{_size} dungeon in {iterations} iterations");
// 	}

// 	private void SetBoundaryWalls()
// 	{
// 		// Use Tile4W (4 walls) for all boundary cells to create a solid wall ring.
// 		// Tile4W has walls on all sides so orientation doesn't matter.
// 		// Tile4W can only neighbor floor tiles, forcing the inner perimeter to be floor.

// 		for (int x = 0; x < _size; x++)
// 		{
// 			CollapseBoundaryCell(x, 0, Tile4W, 0);
// 			CollapseBoundaryCell(x, _size - 1, Tile4W, 0);
// 		}

// 		for (int z = 0; z < _size; z++)
// 		{
// 			CollapseBoundaryCell(0, z, Tile4W, 0);
// 			CollapseBoundaryCell(_size - 1, z, Tile4W, 0);
// 		}
// 	}

// 	private void CollapseBoundaryCell(int x, int z, int tile, int orientIndex)
// 	{
// 		_state[x, z] = tile;
// 		_orientations[x, z, 0] = (tile << 4) | orientIndex;
// 		for (int i = 1; i < NumOrientations; i++)
// 		{
// 			_orientations[x, z, i] = -1;
// 		}
// 		_collapsed[x, z] = true;
// 	}

// 	private bool IsComplete()
// 	{
// 		for (int x = 0; x < _size; x++)
// 		{
// 			for (int z = 0; z < _size; z++)
// 			{
// 				if (!_collapsed[x, z]) return false;
// 			}
// 		}
// 		return true;
// 	}

// 	private int[] GetLowestEntropyCell()
// 	{
// 		int minEntropy = int.MaxValue;
// 		List<int[]> candidates = new List<int[]>();

// 		for (int x = 0; x < _size; x++)
// 		{
// 			for (int z = 0; z < _size; z++)
// 			{
// 				if (_collapsed[x, z]) continue;

// 				int entropy = CountValidOptions(x, z);
// 				if (entropy == 0) return null; // contradiction
// 				if (entropy < minEntropy)
// 				{
// 					minEntropy = entropy;
// 					candidates.Clear();
// 					candidates.Add(new int[] { x, z });
// 				}
// 				else if (entropy == minEntropy)
// 				{
// 					candidates.Add(new int[] { x, z });
// 				}
// 			}
// 		}

// 		if (candidates.Count == 0) return null;
// 		int[] chosen = candidates[(int)(GD.Randi() % candidates.Count)];
// 		return chosen;
// 	}

// 	private int CountValidOptions(int x, int z)
// 	{
// 		int count = 0;
// 		for (int i = 0; i < NumOrientations; i++)
// 		{
// 			if (_orientations[x, z, i] >= 0) count++;
// 		}
// 		return count;
// 	}

// 	private void CollapseCell(int x, int z)
// 	{
// 		// Collect all valid options
// 		List<int[]> options = new List<int[]>();
// 		for (int i = 0; i < NumOrientations; i++)
// 		{
// 			if (_orientations[x, z, i] >= 0)
// 			{
// 				options.Add(new int[] { _orientations[x, z, i] >> 4, _orientations[x, z, i] & 0x3 });
// 			}
// 		}

// 		// Weighted selection: prefer more open tiles (fewer walls) for corridors
// 		List<(int tile, int orient, float weight)> weighted = new List<(int, int, float)>();
// 		foreach (var opt in options)
// 		{
// 			float weight = GetTileWeight(opt[0]);
// 			weighted.Add((opt[0], opt[1], weight));
// 		}

// 		float totalWeight = weighted.Sum(w => w.weight);
// 		int roll = (int)(GD.Randi() % totalWeight);
// 		float cumulative = 0;

// 		int chosenTile = options[0][0];
// 		int chosenOrient = options[0][1];

// 		foreach (var w in weighted)
// 		{
// 			cumulative += w.weight;
// 			if (roll < cumulative)
// 			{
// 				chosenTile = w.tile;
// 				chosenOrient = w.orient;
// 				break;
// 			}
// 		}

// 		_state[x, z] = chosenTile;
// 		_orientations[x, z, 0] = (chosenTile << 4) | chosenOrient;
// 		for (int i = 1; i < NumOrientations; i++)
// 		{
// 			_orientations[x, z, i] = -1;
// 		}
// 		_collapsed[x, z] = true;
// 	}

// 	private float GetTileWeight(int tile)
// 	{
// 		// Weight by inverse wall count to prefer open spaces
// 		return 1.0f / (tile + 1);
// 	}

// 	private void Propagate()
// 	{
// 		Queue<int[]> queue = new Queue<int[]>();

// 		// Add all cells to queue initially
// 		for (int x = 0; x < _size; x++)
// 		{
// 			for (int z = 0; z < _size; z++)
// 			{
// 				if (_collapsed[x, z])
// 				{
// 					queue.Enqueue(new int[] { x, z });
// 				}
// 			}
// 		}

// 		int[] dx = new int[] { 1, -1, 0, 0 };
// 		int[] dz = new int[] { 0, 0, 1, -1 };

// 		while (queue.Count > 0)
// 		{
// 			int[] cell = queue.Dequeue();
// 			int x = cell[0];
// 			int z = cell[1];

// 			for (int d = 0; d < 4; d++)
// 			{
// 				int nx = x + dx[d];
// 				int nz = z + dz[d];

// 				if (nx < 0 || nx >= _size || nz < 0 || nz >= _size) continue;
// 				if (!_collapsed[nx, nz]) continue;

// 				bool changed = ApplyConstraints(x, z, d, nx, nz, queue);
// 				if (changed)
// 				{
// 					// Also constrain the reverse direction
// 					int reverseDir = (d + 2) % 4;
// 					ApplyConstraints(nx, nz, reverseDir, x, z, queue);
// 				}
// 			}
// 		}
// 	}

// 	private bool ApplyConstraints(int fx, int fz, int fromDir, int tx, int tz, Queue<int[]> queue)
// 	{
// 		bool changed = false;

// 		for (int fi = 0; fi < NumOrientations; fi++)
// 		{
// 			if (_orientations[fx, fz, fi] < 0) continue;

// 			int tileA = _orientations[fx, fz, fi] >> 4;
// 			int orientA = _orientations[fx, fz, fi] & 0x3;

// 			// Filter neighbor orientations using orientation-aware constraint
// 			List<int> newNeighborOrientations = new List<int>();
// 			for (int ti = 0; ti < NumOrientations; ti++)
// 			{
// 				if (_orientations[tx, tz, ti] < 0) continue;

// 				int tileB = _orientations[tx, tz, ti] >> 4;
// 				int orientB = _orientations[tx, tz, ti] & 0x3;

// 				if (IsValidNeighbor(tileA, orientA, fromDir, tileB, orientB))
// 				{
// 					newNeighborOrientations.Add(ti);
// 				}
// 			}

// 			if (newNeighborOrientations.Count == 0)
// 			{
// 				// This orientation of the source cell is invalid
// 				_orientations[fx, fz, fi] = -1;
// 				changed = true;
// 			}
// 		}

// 		// Remove invalid tiles from source cell
// 		int remaining = 0;
// 		for (int i = 0; i < NumOrientations; i++)
// 		{
// 			if (_orientations[fx, fz, i] >= 0) remaining++;
// 		}

// 		if (remaining == 0) return false; // contradiction

// 		// If source was collapsed, it can't change - skip
// 		if (_collapsed[fx, fz]) return changed;

// 		if (changed && queue != null)
// 		{
// 			queue.Enqueue(new int[] { fx, fz });
// 		}

// 		return changed;
// 	}

// 	private bool IsValidNeighbor(int tileA, int orientA, int direction, int tileB, int orientB)
// 	{
// 		// Determine wall faces for both tiles
// 		bool[] wallsA = GetWallsForTile(tileA);
// 		bool[] wallsB = GetWallsForTile(tileB);
// 		bool[] rotatedA = RotateWalls(wallsA, orientA);
// 		bool[] rotatedB = RotateWalls(wallsB, orientB);

// 		// Check if tileA has a wall facing the neighbor
// 		if (rotatedA[direction])
// 		{
// 			// TileA has a wall facing tileB - tileB can be anything
// 			// (wall blocks passage, tileB's opening or wall both valid)
// 			return true;
// 		}

// 		// TileA has an opening facing tileB.
// 		// TileB is valid if it has a wall on the side facing tileA.
// 		// TileA is at (fx,fz), tileB is at direction 'direction' from tileA.
// 		// The face of tileB that faces tileA is the opposite side: (direction + 2) % 4.
// 		// But in our wall array, direction 0=+X means the wall is on the +X side of the tile.
// 		// TileB's side facing tileA is the side toward tileA's position.
// 		// If tileB is at +X from tileA (direction 0), tileB's -X side faces tileA.
// 		// In our array, -X = index 1. So we check rotatedB[(direction + 2) % 4].
// 		int facingSideOfB = (direction + 2) % 4;
// 		return rotatedB[facingSideOfB];
// 	}

// 	private bool[] GetWallsForTile(int tile)
// 	{
// 		// Returns [posX, negX, posZ, negZ] wall presence in base orientation
// 		switch (tile)
// 		{
// 			case TileFloor: // 0W - no walls
// 				return new bool[] { false, false, false, false };

// 			case Tile1W: // 1W - one wall on +X
// 				return new bool[] { true, false, false, false };

// 			case Tile2W: // 2W - two adjacent walls (+X and +Z, forming inner corner)
// 				return new bool[] { true, false, true, false };

// 			case Tile2WS: // 2WS - two walls short (same topology as 2W for constraints)
// 				return new bool[] { true, false, true, false };

// 			case Tile3W: // 3W - three walls (only negX missing)
// 				return new bool[] { true, true, true, false };

// 			case Tile4W: // 4W - all four walls (corner piece, but acts as enclosed)
// 				return new bool[] { true, true, true, true };

// 			default:
// 				return new bool[] { false, false, false, false };
// 		}
// 	}

// 	private bool[] RotateWalls(bool[] walls, int steps)
// 	{
// 		// Rotate the wall array clockwise by steps * 90 degrees (Y-axis rotation)
// 		// walls = [posX, negX, posZ, negZ]
// 		// After 90 deg Y-rotation: posX<-negZ, negX<-posZ, posZ<-posX, negZ<-negX
// 		bool[] rotated = new bool[4];
// 		steps = steps % 4;

// 		for (int i = 0; i < 4; i++)
// 		{
// 			rotated[(i + steps) % 4] = walls[i];
// 		}

// 		return rotated;
// 	}

// }

public partial class Constraints
{
	[JsonPropertyName("allowedSockets")]
	public Dictionary<string, List<string>> AllowedSockets { get; set; }

	[JsonPropertyName("tiles")]
	public Dictionary<string, DungeonTile> Tiles { get; set; }
}

public partial class DungeonTile
{
	[JsonPropertyName("gridMapIndex")]
	public int GridMapIndex { get; set; }
	[JsonPropertyName("weight")]
	public int Weight { get; set; }

	// 0, 16, 10, 22
	// 0, 90, 180, 270
	[JsonPropertyName("allowedRotations")]
	public List<int> AllowedRotations { get; set; }

	// All sockets for position at rotation 0
	// +X, +Z, -X, -Z
	// Decrement index per 90 degress wrap around
	[JsonPropertyName("sockets")]
	public List<string> Sockets { get; set; }
}

public partial class DungeonCell
{
	public List<int> AvailableTiles { get; set; }

	// null if not collapsed and int for the grid map index selected
	public int? Collapsed { get; set; }

	public int Orientation { get; set; }

	public int Entropy 
	{ 
		get
		{
			return this.AvailableTiles.Count();
		}
	}
}