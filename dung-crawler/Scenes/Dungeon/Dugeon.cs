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
	private List<Cell> _tiles;
	// private List<DungeonTile> _tileSet;
	// private int _width;
	// private int _depth;
	// private Random _random = new Random();

	public override void _Ready()
	{

		if (GridMap == null) return;

        for (int x = 0; x < MapSize; x++)
        {
            for (int z = 0; z < MapSize; z++)
            {
				int orientation = 0;
                // Set cell position (x, y, z) and tile ID
				if ((z == 0 && x == 0) || (z == MapSize - 1 && x == MapSize - 1))
				{		
					orientation = ((z == MapSize - 1 && x == MapSize - 1) ? 10 : 0);
                	GridMap.SetCellItem(new Vector3I(x, 0, z), 2, orientation);
				}
				else if ((z == 0 && x == MapSize - 1) || (z == MapSize - 1 && x == 0))
				{		
					orientation = ((z == 0 && x == MapSize - 1) ? 22 : 16);
                	GridMap.SetCellItem(new Vector3I(x, 0, z), 2, orientation);
				}
				else if (z == 0 || x == 0)
				{
					orientation = (z == 0 ? 22 : 0);
                	GridMap.SetCellItem(new Vector3I(x, 0, z), 1, orientation);				
				}
				else
				{	
                	GridMap.SetCellItem(new Vector3I(x, 0, z), orientation);
				}

            }
        }

		// InitializeMapData();
	}


	// public void InitializeMapData()
	// {
	// 	_grid = new int[MapSize, MapSize];
	// 	for (int x = 0; x < MapSize; x++)
	// 	{
	// 		for (int z = 0; z < MapSize; z++)
	// 		{
	// 			_grid[x, z] = -1;
	// 		}
	// 	}

	// 	DefineTileSet();
	// 	// RunWaveFunctionCollapse();
	// 	// GenerateDungeonMeshes();
	// }

	// private void DefineTileSet()
	// {
	// 	_tiles = new List<Cell>();

	// 	_tiles.Add(new Cell(
	// 		gridMapIndex: -1,
	// 		posXRules: new List<int>(),
	// 		negXRules: new List<int>(),
	// 		posZRules: new List<int>(),
	// 		negZRules: new List<int>(),
	// 		weight: 0
	// 	));

	// 	_tiles.Add(new Cell(
	// 		gridMapIndex: 0,
	// 		posXRules: new List<int> { 0, 1, 2, 3, 4, 5 },
	// 		negXRules: new List<int> { 0, 1, 2, 3, 4, 5 },
	// 		posZRules: new List<int> { 0, 1, 2, 3, 4, 5 },
	// 		negZRules: new List<int> { 0, 1, 2, 3, 4, 5 },
	// 		weight: 2
	// 	));

	// 	_tiles.Add(new Cell(
	// 		gridMapIndex: 1,
	// 		posXRules: new List<int> { 0, 1, 2, 3, 4, 5 },
	// 		negXRules: new List<int> { 0, 1, 2, 3, 4, 5 },
	// 		posZRules: new List<int> { 0, 1, 2, 3, 4, 5 },
	// 		negZRules: new List<int> { 0, 1, 2, 3, 4, 5 },
	// 		weight: 2
	// 	));

	// 	_tiles.Add(new Cell(
	// 		gridMapIndex: 2,
	// 		posXRules: new List<int> { 0, 1, 2, 3, 4, 5 },
	// 		negXRules: new List<int> { 0, 1, 2, 3, 4, 5 },
	// 		posZRules: new List<int> { 0, 1, 2, 3, 4, 5 },
	// 		negZRules: new List<int> { 0, 1, 2, 3, 4, 5 },
	// 		weight: 2
	// 	));

	// 	_tiles.Add(new Cell(
	// 		gridMapIndex: 3,
	// 		posXRules: new List<int> { 0, 1, 2, 3, 4, 5 },
	// 		negXRules: new List<int> { 0, 1, 2, 3, 4, 5 },
	// 		posZRules: new List<int> { 0, 1, 2, 3, 4, 5 },
	// 		negZRules: new List<int> { 0, 1, 2, 3, 4, 5 },
	// 		weight: 2
	// 	));

	// 	_tiles.Add(new Cell(
	// 		gridMapIndex: 4,
	// 		posXRules: new List<int> { 0, 1, 2, 3, 4, 5 },
	// 		negXRules: new List<int> { 0, 1, 2, 3, 4, 5 },
	// 		posZRules: new List<int> { 0, 1, 2, 3, 4, 5 },
	// 		negZRules: new List<int> { 0, 1, 2, 3, 4, 5 },
	// 		weight: 2
	// 	));

	// 	_tiles.Add(new Cell(
	// 		gridMapIndex: 5,
	// 		posXRules: new List<int> { 0, 1, 2, 3, 4, 5 },
	// 		negXRules: new List<int> { 0, 1, 2, 3, 4, 5 },
	// 		posZRules: new List<int> { 0, 1, 2, 3, 4, 5 },
	// 		negZRules: new List<int> { 0, 1, 2, 3, 4, 5 },
	// 		weight: 2
	// 	));

	// 	_tileSet = _tiles.Skip(1).ToList();
	// }
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
