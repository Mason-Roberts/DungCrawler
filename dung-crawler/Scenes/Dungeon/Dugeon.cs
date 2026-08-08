using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Dugeon : Node3D
{
	[Export]
	public GridMap GridMap { get; set; }

	[Export]
	public int MapSize { get; set; } = 30;

	private int[,] _grid;
	private List<DungeonTile> _tiles;
	private List<DungeonTile> _tileSet;
	private int _width;
	private int _depth;
	private Random _random = new Random();

	public override void _Ready()
	{
		InitializeMapData();
	}

	public void InitializeMapData()
	{
		_width = MapSize;
		_depth = MapSize;

		_grid = new int[_width, _depth];
		for (int x = 0; x < _width; x++)
		{
			for (int z = 0; z < _depth; z++)
			{
				_grid[x, z] = -1;
			}
		}

		DefineTileSet();
		RunWaveFunctionCollapse();
		GenerateDungeonMeshes();
	}

	private void DefineTileSet()
	{
		_tiles = new List<DungeonTile>();

		_tiles.Add(new DungeonTile(
			gridMapIndex: -1,
			posXRules: new List<int>(),
			negXRules: new List<int>(),
			posZRules: new List<int>(),
			negZRules: new List<int>(),
			weight: 0
		));

		_tiles.Add(new DungeonTile(
			gridMapIndex: 0,
			posXRules: new List<int> { 0, 1, 2, 3, 4, 5 },
			negXRules: new List<int> { 0, 1, 2, 3, 4, 5 },
			posZRules: new List<int> { 0, 1, 2, 3, 4, 5 },
			negZRules: new List<int> { 0, 1, 2, 3, 4, 5 },
			weight: 2
		));

		_tiles.Add(new DungeonTile(
			gridMapIndex: 1,
			posXRules: new List<int> { 0, 1, 2, 3, 4, 5 },
			negXRules: new List<int> { 0, 1, 2, 3, 4, 5 },
			posZRules: new List<int> { 0, 1, 2, 3, 4, 5 },
			negZRules: new List<int> { 0, 1, 2, 3, 4, 5 },
			weight: 2
		));

		_tiles.Add(new DungeonTile(
			gridMapIndex: 2,
			posXRules: new List<int> { 0, 1, 2, 3, 4, 5 },
			negXRules: new List<int> { 0, 1, 2, 3, 4, 5 },
			posZRules: new List<int> { 0, 1, 2, 3, 4, 5 },
			negZRules: new List<int> { 0, 1, 2, 3, 4, 5 },
			weight: 2
		));

		_tiles.Add(new DungeonTile(
			gridMapIndex: 3,
			posXRules: new List<int> { 0, 1, 2, 3, 4, 5 },
			negXRules: new List<int> { 0, 1, 2, 3, 4, 5 },
			posZRules: new List<int> { 0, 1, 2, 3, 4, 5 },
			negZRules: new List<int> { 0, 1, 2, 3, 4, 5 },
			weight: 2
		));

		_tiles.Add(new DungeonTile(
			gridMapIndex: 4,
			posXRules: new List<int> { 0, 1, 2, 3, 4, 5 },
			negXRules: new List<int> { 0, 1, 2, 3, 4, 5 },
			posZRules: new List<int> { 0, 1, 2, 3, 4, 5 },
			negZRules: new List<int> { 0, 1, 2, 3, 4, 5 },
			weight: 2
		));

		_tiles.Add(new DungeonTile(
			gridMapIndex: 5,
			posXRules: new List<int> { 0, 1, 2, 3, 4, 5 },
			negXRules: new List<int> { 0, 1, 2, 3, 4, 5 },
			posZRules: new List<int> { 0, 1, 2, 3, 4, 5 },
			negZRules: new List<int> { 0, 1, 2, 3, 4, 5 },
			weight: 2
		));

		_tileSet = _tiles.Skip(1).ToList();
	}

	private void RunWaveFunctionCollapse()
	{
		int[,,] grid = new int[_width, _tileSet.Count, _depth];
		bool[,,] collapsed = new bool[_width, _tileSet.Count, _depth];

		for (int x = 0; x < _width; x++)
		{
			for (int z = 0; z < _depth; z++)
			{
				for (int t = 0; t < _tileSet.Count; t++)
				{
					grid[x, t, z] = 1;
					collapsed[x, t, z] = false;
				}
			}
		}

		int totalCells = _width * _depth;
		int cellsCollapsed = 0;

		while (cellsCollapsed < totalCells)
		{
			int bestX = -1, bestZ = -1, bestState = -1;
			int lowestEntropy = int.MaxValue;

			for (int x = 0; x < _width; x++)
			{
				for (int z = 0; z < _depth; z++)
				{
					int stateCount = 0;
					int firstState = -1;
					for (int t = 0; t < _tileSet.Count; t++)
					{
						if (grid[x, t, z] == 1)
						{
							stateCount++;
							if (firstState == -1)
								firstState = t;
						}
					}

					if (stateCount == 0)
					{
						RunWaveFunctionCollapse();
						return;
					}

					if (stateCount < lowestEntropy)
					{
						lowestEntropy = stateCount;
						bestX = x;
						bestZ = z;
						bestState = firstState;
					}
				}
			}

			List<(int x, int z, int stateCount)> candidates = new List<(int, int, int)>();
			for (int x = 0; x < _width; x++)
			{
				for (int z = 0; z < _depth; z++)
				{
					int stateCount = 0;
					for (int t = 0; t < _tileSet.Count; t++)
					{
						if (grid[x, t, z] == 1)
							stateCount++;
					}

					if (stateCount == lowestEntropy)
						candidates.Add((x, z, stateCount));
				}
			}

			(int x, int z, int stateCount) chosen = candidates[_random.Next(candidates.Count)];
			bestX = chosen.x;
			bestZ = chosen.z;

			List<int> possibleStates = new List<int>();
			for (int t = 0; t < _tileSet.Count; t++)
			{
				if (grid[bestX, t, bestZ] == 1)
					possibleStates.Add(t);
			}

			int totalWeight = possibleStates.Sum(t => _tileSet[t].Weight);
			int roll = _random.Next(totalWeight);
			int cumulativeWeight = 0;
			int selectedState = possibleStates[0];

			foreach (int t in possibleStates)
			{
				cumulativeWeight += _tileSet[t].Weight;
				if (roll < cumulativeWeight)
				{
					selectedState = t;
					break;
				}
			}

			for (int t = 0; t < _tileSet.Count; t++)
			{
				if (t == selectedState)
					grid[bestX, t, bestZ] = 1;
				else
					grid[bestX, t, bestZ] = 0;
			}

			if (!Propagate(grid, bestX, selectedState, bestZ))
			{
				RunWaveFunctionCollapse();
				return;
			}

			cellsCollapsed++;
		}

		for (int x = 0; x < _width; x++)
		{
			for (int z = 0; z < _depth; z++)
			{
				for (int t = 0; t < _tileSet.Count; t++)
				{
					if (grid[x, t, z] == 1)
					{
						_grid[x, z] = t + 1;
						break;
					}
				}
			}
		}
	}

	private bool Propagate(int[,,] grid, int x, int tileIndex, int z)
	{
		List<(int nx, int nz, int dirX, int dirZ)> neighbors = new List<(int, int, int, int)>();

		if (x + 1 < _width)
			neighbors.Add((x + 1, z, 1, 0));
		if (x - 1 >= 0)
			neighbors.Add((x - 1, z, -1, 0));
		if (z + 1 < _depth)
			neighbors.Add((x, z + 1, 0, 1));
		if (z - 1 >= 0)
			neighbors.Add((x, z - 1, 0, -1));

		DungeonTile currentTile = _tileSet[tileIndex];

		foreach (var (nx, nz, dirX, dirZ) in neighbors)
		{
			List<int> allowedOppositeRules = GetOppositeRules(currentTile, dirX, dirZ);

			bool changed = false;
			for (int t = 0; t < _tileSet.Count; t++)
			{
				DungeonTile neighborTile = _tileSet[t];
				List<int> neighborRules = GetRulesForDirection(neighborTile, -dirX, -dirZ);

				bool compatible = false;
				foreach (int rule in allowedOppositeRules)
				{
					if (neighborRules.Contains(rule))
					{
						compatible = true;
						break;
					}
				}

				if (!compatible && grid[nx, t, nz] == 1)
				{
					grid[nx, t, nz] = 0;
					changed = true;
				}
			}

			if (changed)
			{
				bool hasStates = false;
				for (int t = 0; t < _tileSet.Count; t++)
				{
					if (grid[nx, t, nz] == 1)
					{
						hasStates = true;
						break;
					}
				}

				if (!hasStates)
					return false;

				for (int t = 0; t < _tileSet.Count; t++)
				{
					if (grid[nx, t, nz] == 1)
					{
						if (!Propagate(grid, nx, t, nz))
							return false;
					}
				}
			}
		}

		return true;
	}

	private List<int> GetRulesForDirection(DungeonTile tile, int dirX, int dirZ)
	{
		if (dirX == 1) return tile.PosXRules;
		if (dirX == -1) return tile.NegXRules;
		if (dirZ == 1) return tile.PosZRules;
		if (dirZ == -1) return tile.NegZRules;
		return new List<int>();
	}

	private List<int> GetOppositeRules(DungeonTile tile, int dirX, int dirZ)
	{
		return GetRulesForDirection(tile, dirX, dirZ);
	}

	private void GenerateDungeonMeshes()
	{
		GridMap.Clear();
		GridMap.CellSize = new Vector3(1, 1, 1);
		GridMap.CellCenterX = true;
		GridMap.CellCenterY = true;
		GridMap.CellCenterZ = true;

		for (int x = 0; x < _width; x++)
		{
			for (int z = 0; z < _depth; z++)
			{
				int tileIndex = _grid[x, z];
				if (tileIndex <= 0)
					continue;

				int gridItemIndex = GetGridMapItemIndex(tileIndex);
				if (gridItemIndex < 0)
					continue;

				GridMap.SetCellItem(
					new Vector3I(x, 0, z),
					gridItemIndex,
					0
				);
			}
		}
	}

	private void AddMeshItem(MeshLibrary meshLibrary, int id, string name, Mesh mesh, StandardMaterial3D material)
	{
		meshLibrary.CreateItem(id);
		meshLibrary.SetItemName(id, name);
		meshLibrary.SetItemMesh(id, mesh);
	}

	private int GetGridMapItemIndex(int tileIndex)
	{
		if (tileIndex >= 1 && tileIndex <= 18)
			return tileIndex - 1;

		return -1;
	}
}

public partial class Cell: RefCounted
{
	public Cell(Vector3I pos, List<int> totalTiles)
	{
		Position = pos;
		ValidNeighbors = totalTiles;
	}

	public int GridMapIndex { get; set; }
	public Vector3I Position { get; set; }
	public string PosX { get; set; }
	public string NegX { get; set; }
	public string PosZ { get; set; }
	public string NegZ { get; set; }
	public int Weight { get; set; }

	public bool IsCollapsed { get; set; }

	// List of grid map indeces that are still valid for this cell
	public List<int> ValidNeighbors { get; set; }
	
	public int GetEntropy
	{
		get
		{
			return ValidNeighbors.Count();
		}
	}

}
