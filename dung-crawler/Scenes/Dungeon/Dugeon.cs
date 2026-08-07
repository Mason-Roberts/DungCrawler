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
			gridMapIndex: 1,
			posXRules: new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
			negXRules: new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
			posZRules: new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
			negZRules: new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
			weight: 30
		));

		_tiles.Add(new DungeonTile(
			gridMapIndex: 2,
			posXRules: new List<int> { 3 },
			negXRules: new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
			posZRules: new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
			negZRules: new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
			weight: 10
		));

		_tiles.Add(new DungeonTile(
			gridMapIndex: 3,
			posXRules: new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
			negXRules: new List<int> { 2 },
			posZRules: new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
			negZRules: new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
			weight: 10
		));

		_tiles.Add(new DungeonTile(
			gridMapIndex: 4,
			posXRules: new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
			negXRules: new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
			posZRules: new List<int> { 5 },
			negZRules: new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
			weight: 10
		));

		_tiles.Add(new DungeonTile(
			gridMapIndex: 5,
			posXRules: new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
			negXRules: new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
			posZRules: new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
			negZRules: new List<int> { 4 },
			weight: 10
		));

		_tiles.Add(new DungeonTile(
			gridMapIndex: 6,
			posXRules: new List<int> { 3 },
			negXRules: new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
			posZRules: new List<int> { 5 },
			negZRules: new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
			weight: 5
		));

		_tiles.Add(new DungeonTile(
			gridMapIndex: 7,
			posXRules: new List<int> { 3 },
			negXRules: new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
			posZRules: new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
			negZRules: new List<int> { 4 },
			weight: 5
		));

		_tiles.Add(new DungeonTile(
			gridMapIndex: 8,
			posXRules: new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
			negXRules: new List<int> { 2 },
			posZRules: new List<int> { 5 },
			negZRules: new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
			weight: 5
		));

		_tiles.Add(new DungeonTile(
			gridMapIndex: 9,
			posXRules: new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
			negXRules: new List<int> { 2 },
			posZRules: new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
			negZRules: new List<int> { 4 },
			weight: 5
		));

		_tiles.Add(new DungeonTile(
			gridMapIndex: 10,
			posXRules: new List<int> { 3 },
			negXRules: new List<int> { 2 },
			posZRules: new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
			negZRules: new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
			weight: 8
		));

		_tiles.Add(new DungeonTile(
			gridMapIndex: 11,
			posXRules: new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
			negXRules: new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
			posZRules: new List<int> { 5 },
			negZRules: new List<int> { 4 },
			weight: 8
		));

		_tiles.Add(new DungeonTile(
			gridMapIndex: 12,
			posXRules: new List<int> { 3 },
			negXRules: new List<int> { 2 },
			posZRules: new List<int> { 5 },
			negZRules: new List<int> { 4 },
			weight: 6
		));

		_tiles.Add(new DungeonTile(
			gridMapIndex: 13,
			posXRules: new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
			negXRules: new List<int> { 2 },
			posZRules: new List<int> { 5 },
			negZRules: new List<int> { 4 },
			weight: 4
		));

		_tiles.Add(new DungeonTile(
			gridMapIndex: 14,
			posXRules: new List<int> { 3 },
			negXRules: new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
			posZRules: new List<int> { 5 },
			negZRules: new List<int> { 4 },
			weight: 4
		));

		_tiles.Add(new DungeonTile(
			gridMapIndex: 15,
			posXRules: new List<int> { 3 },
			negXRules: new List<int> { 2 },
			posZRules: new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
			negZRules: new List<int> { 4 },
			weight: 4
		));

		_tiles.Add(new DungeonTile(
			gridMapIndex: 16,
			posXRules: new List<int> { 3 },
			negXRules: new List<int> { 2 },
			posZRules: new List<int> { 5 },
			negZRules: new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
			weight: 4
		));

		_tiles.Add(new DungeonTile(
			gridMapIndex: 17,
			posXRules: new List<int> { 3 },
			negXRules: new List<int> { 2 },
			posZRules: new List<int> { 5 },
			negZRules: new List<int> { 4 },
			weight: 3
		));

		_tiles.Add(new DungeonTile(
			gridMapIndex: 18,
			posXRules: new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
			negXRules: new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
			posZRules: new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
			negZRules: new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
			weight: 20
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
		if (GridMap == null)
		{
			GridMap = new GridMap();
			AddChild(GridMap);
		}

		GridMap.Clear();

		MeshLibrary meshLibrary = new MeshLibrary();
		DefineGridMapMeshLibrary(meshLibrary);
		GridMap.MeshLibrary = meshLibrary;
		GridMap.CellSize = new Vector3(1, 2, 1);
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

	private void DefineGridMapMeshLibrary(MeshLibrary meshLibrary)
	{
		StandardMaterial3D floorMaterial = new StandardMaterial3D
		{
			AlbedoColor = new Color(0f, 1f, 0.5f),
			Metallic = 0.3f,
			Roughness = 0.8f
		};

		StandardMaterial3D wallMaterial = new StandardMaterial3D
		{
			AlbedoColor = new Color(1f, 0f, 0f),
			Metallic = 0.2f,
			Roughness = 0.9f
		};

		StandardMaterial3D floorEdgeMaterial = new StandardMaterial3D
		{
			AlbedoColor = new Color(0f, 1f, 0f),
			Metallic = 0.3f,
			Roughness = 0.8f
		};

		StandardMaterial3D roomMaterial = new StandardMaterial3D
		{
			AlbedoColor = new Color(0f, 0f, 1f),
			Metallic = 0.4f,
			Roughness = 0.7f
		};

		AddMeshItem(meshLibrary, 0, "Floor", new BoxMesh { Size = Vector3.One }, floorMaterial);
		AddMeshItem(meshLibrary, 1, "Wall+X", new BoxMesh { Size = new Vector3(0.5f, 2f, 1f) }, wallMaterial);
		AddMeshItem(meshLibrary, 2, "Wall-X", new BoxMesh { Size = new Vector3(0.5f, 2f, 1f) }, wallMaterial);
		AddMeshItem(meshLibrary, 3, "Wall+Z", new BoxMesh { Size = new Vector3(1f, 2f, 0.5f) }, wallMaterial);
		AddMeshItem(meshLibrary, 4, "Wall-Z", new BoxMesh { Size = new Vector3(1f, 2f, 0.5f) }, wallMaterial);
		AddMeshItem(meshLibrary, 5, "Corner+X+Z", new BoxMesh { Size = new Vector3(0.5f, 2f, 0.5f) }, wallMaterial);
		AddMeshItem(meshLibrary, 6, "Corner+X-Z", new BoxMesh { Size = new Vector3(0.5f, 2f, 0.5f) }, wallMaterial);
		AddMeshItem(meshLibrary, 7, "Corner-X+Z", new BoxMesh { Size = new Vector3(0.5f, 2f, 0.5f) }, wallMaterial);
		AddMeshItem(meshLibrary, 8, "Corner-X-Z", new BoxMesh { Size = new Vector3(0.5f, 2f, 0.5f) }, wallMaterial);
		AddMeshItem(meshLibrary, 9, "CorridorXX", new BoxMesh { Size = new Vector3(0.5f, 2f, 1f) }, wallMaterial);
		AddMeshItem(meshLibrary, 10, "CorridorZZ", new BoxMesh { Size = new Vector3(1f, 2f, 0.5f) }, wallMaterial);
		AddMeshItem(meshLibrary, 11, "RoomCenter", new BoxMesh { Size = new Vector3(0.5f, 2f, 0.5f) }, wallMaterial);
		AddMeshItem(meshLibrary, 12, "TJunction+X", new BoxMesh { Size = new Vector3(0.5f, 2f, 0.5f) }, wallMaterial);
		AddMeshItem(meshLibrary, 13, "TJunction-X", new BoxMesh { Size = new Vector3(0.5f, 2f, 0.5f) }, wallMaterial);
		AddMeshItem(meshLibrary, 14, "TJunction+Z", new BoxMesh { Size = new Vector3(0.5f, 2f, 0.5f) }, wallMaterial);
		AddMeshItem(meshLibrary, 15, "TJunction-Z", new BoxMesh { Size = new Vector3(0.5f, 2f, 0.5f) }, wallMaterial);
		AddMeshItem(meshLibrary, 16, "Cross", new BoxMesh { Size = new Vector3(0.5f, 2f, 0.5f) }, wallMaterial);
		AddMeshItem(meshLibrary, 17, "LargeRoom", new BoxMesh { Size = Vector3.One }, roomMaterial);
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

public struct DungeonTile
{
	public int GridMapIndex { get; set; }
	public List<int> PosXRules { get; set; }
	public List<int> NegXRules { get; set; }
	public List<int> PosZRules { get; set; }
	public List<int> NegZRules { get; set; }
	public int Weight { get; set; }

	public DungeonTile(
		int gridMapIndex,
		List<int> posXRules,
		List<int> negXRules,
		List<int> posZRules,
		List<int> negZRules,
		int weight = 1
	)
	{
		GridMapIndex = gridMapIndex;
		PosXRules = posXRules;
		NegXRules = negXRules;
		PosZRules = posZRules;
		NegZRules = negZRules;
		Weight = weight;
	}
}
