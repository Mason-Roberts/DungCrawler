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

		WfcGenerator wfc = new WfcGenerator(GridMap, MapSize, _constraints, 7, -1);
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
		_rand = new Random(Guid.NewGuid().GetHashCode());
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

	private void CollapseCell(DungeonCell cell = null)
	{
		// initial cell should always be the gate from a random part of the border

	}

	private void RenderGrid()
	{
		for (int x = 0; x < _size; x++)
		{
			for (int z = 0; z < _size; z++)
			{
				if (_grid[x, z].Collapsed.HasValue)
				{		
					_gridMap.SetCellItem(new Vector3I(x, 0, z), _grid[x, z].Collapsed.Value, TranslateDegreeToOrthagonalIndex(_grid[x, z].Orientation));
				}
			}
		}
	}

	public void Generate()
	{
		InitializeMapData();
		
		while (_collapsedCells < _totalCells)
		{
			if (_collapsedCells != 0)
			{				
				var candidates = GetLowestEntropyCells();

				if (candidates == null || candidates.Count() == 0)
				{
					// Contradiction - restart
					GD.Print($"WFC contradiction restarting...");
					Generate();
					return;
				}

				int randCandidate = _rand.Next(0, candidates.Count());
				CollapseCell(candidates[randCandidate]);		
				// Propogate();
			}
			else
			{
				bool useX = _rand.Next(0, 2) == 1;
				bool useZero = _rand.Next(0, 2) == 1;

				int coord = _rand.Next(1, _size - 2); 
				int side = useZero ? 0 : _size - 1;

				if (useX)
				{
					_grid[coord, side].Collapsed = _gateTileId;
					_grid[coord, side].Orientation = side == _size - 1 ? 90 : 270;
					_grid[coord, side].AvailableTiles = new List<int> { _gateTileId };
					_collapsedCells = _collapsedCells + 1;
				}
				else
				{
					_grid[side, coord].Collapsed = _gateTileId;
					_grid[side, coord].Orientation = side == _size - 1 ? 180 : 0;
					_grid[side, coord].AvailableTiles = new List<int> { _gateTileId };
					_collapsedCells = _collapsedCells + 1;
				}
				// Propagate();
			}

			break;
		}

		RenderGrid();
	}

	private int TranslateDegreeToOrthagonalIndex(int degree)
	{
		switch (degree)
		{
			case 0:
				return 0;
			case 90:
				return 16;
			case 180:
				return 10;
			case 270:
				return 22;
			default:
				return 0;
		}
	}

	private List<DungeonCell> GetLowestEntropyCells()
	{
		List<DungeonCell> candidates = new List<DungeonCell>();
		int currentMin = int.MaxValue;
		for (int x = 0; x < _size; x++)
		{
			for (int z = 0; z < _size; z++)
			{
				if (_grid[x, z].Collapsed.HasValue)
				{
					continue;
				}

				if (_grid[x, z].Entropy <= 0)
				{
					return null;
				}

				if (_grid[x, z].Entropy < currentMin)
				{
					currentMin = _grid[x, z].Entropy;
					candidates.Clear();
					candidates.Add(_grid[x, z]);
				}
				else if (_grid[x, z].Entropy == currentMin)
				{
					candidates.Add(_grid[x, z]);
				}
			}
		}

		return candidates;
	}

	// private void Propagate()
	// {
	// 	Queue<int[]> queue = new Queue<int[]>();

	// 	// Add all cells to queue initially
	// 	for (int x = 0; x < _size; x++)
	// 	{
	// 		for (int z = 0; z < _size; z++)
	// 		{
	// 			if (!_grid[x, z].Collapsed.HasValue)
	// 			{
	// 				queue.Enqueue(new int[] { x, z });
	// 			}
	// 		}
	// 	}

	// 	// neighbor coord adjusts
	// 	// 0: PosX, 1: NegX, 2: PosZ, 3: NegZ
	// 	int[] dx = new int[] { 1, -1, 0, 0 };
	// 	int[] dz = new int[] { 0, 0, 1, -1 };

	// 	while (queue.Count > 0)
	// 	{
	// 		int[] cell = queue.Dequeue();
	// 		int x = cell[0];
	// 		int z = cell[1];

	// 		for (int d = 0; d < 4; d++)
	// 		{
	// 			int nx = x + dx[d];
	// 			int nz = z + dz[d];

	// 			// Check border and potentially fuck off
	// 			if (nx < 0 || nx >= _size || nz < 0 || nz >= _size) continue;

	// 			// Check collapsed and fuck off
	// 			if (_grid[x, z].Collapsed.HasValue) continue;

	// 			bool changed = ApplyConstraints(x, z, d, nx, nz, queue);
	// 			if (changed)
	// 			{
	// 				// Also constrain the reverse direction
	// 				int reverseDir = (d + 2) % 4;
	// 				ApplyConstraints(nx, nz, reverseDir, x, z, queue);
	// 			}
	// 		}
	// 	}
	// }

	// private bool ApplyConstraints(int x, int z, int d, int nx, int nz, Queue<int[]> queue)
	// {
	// 	bool changed = false;
	// 	DungeonCell cell = _grid[x, z];
	// 	DungeonCell neighborCell = _grid[nx, nz];

	// 	// 0: PosX, 1: NegX, 2: PosZ, 3: NegZ
	// 	List<int> badNeighborTiles = new List<int>();
	// 	foreach (var tile in cell.AvailableTiles)
	// 	{
	// 		var dTile = _constraints.Tiles[tile.ToString()];
	// 		foreach (var rotation in dTile.AllowedRotations)
	// 		{		
	// 			int startIndex = rotation == 0 ? 0 : 3 - (rotation / 90);

	// 			string socketType = "";
	// 			switch (d)
	// 			{
	// 				case 0:
	// 					// posx
	// 					socketType = dTile.Sockets[startIndex];
	// 					break;
	// 				case 1:
	// 					// negx
	// 					socketType = dTile.Sockets[startIndex + 2];
	// 					break;
	// 				case 2:
	// 					// posz
	// 					socketType = dTile.Sockets[startIndex + 1];
	// 					break;
	// 				case 3:
	// 					// negz
	// 					socketType = dTile.Sockets[startIndex + 3];
	// 					break;
	// 				default:
	// 					break;
	// 			}
	// 		}

	// 	}
		
	// 	for (int o = 0; o < 4; o++)
	// 	{
			
	// 	}

	// 	bool changed = false;

	// 	bool isCellBorderCorner = IsBorderCorner(x, z);
	// 	bool isCellBorderStraight = IsBorderStraight(x, z);

	// 	bool isNCellBorderCorner = IsBorderCorner(nx, nz);
	// 	bool isNCellBorderStraight = IsBorderStraight(nx, nz);

	// 	if (isCellBorderCorner)
	// 	{
			
	// 	}

	// 	if (isCellBorderStraight)
	// 	{
			
	// 	}
		
	// 	foreach (var index in cell.AvailableTiles)
	// 	{
	// 		var tile = _constraints.Tiles[index.ToString()];
	// 		tile.Sockets
	// 	}

	// 	cell.

	// 	return changed;
	// }

	private bool IsBorderCorner(int x, int z)
	{
		if ((z == 0 && x == 0) || (z == _size -1 && x == _size - 1) || (z == 0 && x == _size -1) || (z == _size -1 && x == 0))
		{				
			return true;
		}
		return false;
	}

	private bool IsBorderStraight(int x, int z)
	{
		if ((z == 0 || z == _size -1 || x == 0 || x == _size - 1) && !IsBorderCorner(x, z)) // Border straights need to have a wall
		{
			return true;
		}
		return false;
	}

}

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