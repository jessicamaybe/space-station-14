using System.Linq;
using Content.Shared.Atmos;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;

namespace Content.Server.ProcGen.ProcGen;

public sealed partial class ProcGenSystem : EntitySystem
{
    [Dependency] private IRobustRandom _random = default!;

    [Dependency] private SharedMapSystem _mapSystem = default!;
    [Dependency] private SharedTransformSystem _transform = default!;


    private const int RoomTries = 75;
    private const int RoomExtraSize = 0;

    private const int DungeonHeight = 96;
    private const int DungeonWidth = 96;

    private const int ExtraConnectorChance = 20;

    private Dictionary<Vector2i, int> _regions = new();

    private List<Box2i> _rooms = new();

    /// The index of the current region being carved.
    int _currentRegion = -1;

    public override void Initialize()
    {
        base.Initialize();
    }

    public void GenerateNow()
    {
        var map = _mapSystem.CreateMap();

        var grid = _mapSystem.CreateGridEntity(map);

        _rooms.Clear();

        // Fill in all of the empty space with floor
        for (var y = 1; y < DungeonHeight; y += 1)
        {
            for (var x = 1; x < DungeonWidth; x += 1)
            {
                _mapSystem.SetTile(grid, new Vector2i(x, y), new Tile(1));
            }
        }

        CreateRooms(map, grid);

        // Fill in all of the empty space with mazes.
        for (var y = 1; y < DungeonHeight; y += 2)
        {
            for (var x = 1; x < DungeonWidth; x += 2)
            {
                var pos = new Vector2i(x, y);

                if (_mapSystem.TryGetTile(grid, pos, out Tile tile) && !TileAvailable(tile))
                    continue;

                GrowMaze(map, grid, pos);
            }
        }

        ConnectRegions(grid);
        RemoveDeadEnds(grid);



        foreach (var pos in GetBounds(DungeonHeight, DungeonWidth))
        {
            if (_mapSystem.TryGetTile(grid, pos, out Tile tile) && tile.TypeId != 1)
                continue;

            var coords = _mapSystem.ToCoordinates(grid, pos);

            Spawn("WallSolid", coords);
        }

    }

    private void ConnectRegions(Entity<MapGridComponent> grid)
    {
        var connectorRegions = new Dictionary<Vector2i, HashSet<int>>();

        foreach (var pos in GetBounds(DungeonHeight - 1, DungeonHeight - 1))
        {
            if (_mapSystem.TryGetTile(grid, pos, out Tile tile) && !TileAvailable(tile))
                continue;

            var adjacentRegions = new HashSet<int>();

            foreach (var dir in CardinalDirections)
            {
                if (_regions.TryGetValue(pos + dir, out var region))
                    adjacentRegions.Add(region);
            }

            if (adjacentRegions.Count < 2)
                continue;

            connectorRegions[pos] = adjacentRegions;
        }

        var connectors = connectorRegions.Keys.ToList();

        // 2. Initialize disjoint-set/region merge mappings.
        var merged = new Dictionary<int, int>();
        var openRegions = new HashSet<int>();

        for (var i = 0; i <= _currentRegion; i++)
        {
            merged[i] = i;
            openRegions.Add(i);
        }

        // 3. Connect regions until all areas are unified.
        while (openRegions.Count > 1 && connectors.Count > 0)
        {
            var connector = _random.Pick(connectors);

            AddJunction(grid, connector);

            var connected= connectorRegions[connector]
                .Select(r => merged[r])
                .Distinct()
                .ToList();

            var dest = connected.First();
            var sources = connected.Skip(1).ToHashSet();

            // Check every region, since some may have already been merged
            // into one of the sources we're merging now.
            for (var i = 0; i <= _currentRegion; i++)
            {
                if (sources.Contains(merged[i]))
                    merged[i] = dest;
            }

            openRegions.ExceptWith(sources);

            // Prune obsolete or neighboring connectors
            connectors.RemoveAll(pos =>
            {
                // Prevent two adjacent connectors from forming 2x2 holes
                if ((pos - connector).Length <= 1)
                    return true;

                var regions = connectorRegions[pos]
                    .Select(r => merged[r])
                    .ToHashSet();

                // If the connector spans distinct regions, keep it
                if (regions.Count > 1)
                    return false;

                // Optional extra connections to create loops
                if (_random.Prob(1f / ExtraConnectorChance))
                    AddJunction(grid, pos);

                return true;
            });
        }
    }

    private void RemoveDeadEnds(Entity<MapGridComponent> grid)
    {
        var done = false;

        while (!done)
        {
            done = true;

            foreach (var pos in GetBounds(DungeonHeight - 1, DungeonHeight - 1))
            {
                if (_mapSystem.TryGetTile(grid, pos, out var tile) && tile.TypeId == 1)
                    continue;

                var exits = 0;

                foreach (var dir in CardinalDirections)
                {
                    if (_mapSystem.TryGetTile(grid, pos + dir, out var neightborTile) && neightborTile.TypeId != 1)
                        exits++;
                }

                if (exits != 1)
                    continue;

                done = false;
                _mapSystem.SetTile(grid, pos, new Tile(1));
            }

        }
    }

    private void AddJunction(Entity<MapGridComponent> grid, Vector2i pos)
    {
        // Set floor (Tile 2) or custom door tile
        _mapSystem.SetTile(grid, pos, new Tile(2));

        var coords = _mapSystem.ToCoordinates(grid, pos);
        Spawn("WoodDoor", coords);
    }

    private static readonly Vector2i[] CardinalDirections =
    {
        new(0, 1),   // North
        new(0, -1),  // South
        new(1, 0),   // East
        new(-1, 0)   // West
    };

    private IEnumerable<Vector2i> GetBounds(int height, int width)
    {
        for (var y = 1; y < height; y++)
        {
            for (var x = 1; x < width; x++)
            {
                yield return new Vector2i(x, y);
            }
        }
    }

    private bool TileAvailable(Tile tile)
    {
        if (tile.IsEmpty)
            return false;

        if (tile.TypeId == 1)
            return true;

        return false;
    }

    private void GrowMaze(EntityUid map, Entity<MapGridComponent> grid, Vector2i vec)
    {
        _currentRegion++;
        Carve(grid, vec, new Tile(2));

        var cells = new List<Vector2i>();

        Direction lastDir = Direction.Invalid;

        cells.Add(vec);

        while (cells.Count > 0)
        {
            var cell = cells.Last();

            var unmadeCells = new List<Direction>();

            foreach (var direction in DirectionExtensions.AllDirections)
            {
                if ((int)direction%2 == 0 && CanCarve(grid, cell, direction))
                    unmadeCells.Add(direction);
            }

            if (unmadeCells.Count > 0)
            {
                Direction carveDir;

                if (unmadeCells.Contains(lastDir) && _random.Prob(0.15f))
                {
                    carveDir = lastDir;
                }
                else
                {
                    carveDir = unmadeCells[_random.Next(unmadeCells.Count)];
                }

                Carve(grid, cell + carveDir.ToIntVec(), new Tile(2));
                Carve(grid, cell + (carveDir.ToIntVec() * 2), new Tile(2));

                cells.Add(cell + carveDir.ToIntVec() * 2);

                lastDir = carveDir;
            }
            else
            {
                // No adjacent uncarved cells.
                cells.RemoveAt(cells.Count - 1);

                // This path has ended.
                lastDir = Direction.Invalid;
            }
        }
    }

    private bool CanCarve(Entity<MapGridComponent> grid, Vector2i cell, Direction dir)
    {
        var bounds = Box2i.FromDimensions(new Vector2i(0, 0), new Vector2i(96, 96));

        if (!bounds.Contains(cell + dir.ToIntVec() * 3))
            return false;

        if (_mapSystem.TryGetTile(grid, cell + dir.ToIntVec(), out Tile tile) && !TileAvailable(tile))
            return false;

        if (_mapSystem.TryGetTile(grid, cell + (dir.ToIntVec() * 2), out Tile tile2) && !TileAvailable(tile2))
            return false;

        if (_mapSystem.TryGetTile(grid, cell + dir.ToIntVec() * 3, out Tile tile3) && !TileAvailable(tile3))
            return false;

        return true;
    }

    private void Carve(Entity<MapGridComponent> grid, Vector2i pos, Tile tile)
    {
        _mapSystem.SetTile(grid, pos, tile);
        _regions[pos] = _currentRegion;
    }

    private void CreateRooms(EntityUid map, Entity<MapGridComponent> grid)
    {
        for (int i = 0; i < RoomTries; i++)
        {
            var size = _random.Next(1, 3 + RoomExtraSize) * 2 + 1;

            var rectangularity = _random.Next(0, 1 + size / 2) * 2;
            var width = size;
            var height = size;
            if (_random.Prob(0.5f))
            {
                width += rectangularity;
            }
            else
            {
                height += rectangularity;
            }

            var x = _random.Next((DungeonWidth - width) / 2) * 2 + 1;
            var y = _random.Next((DungeonHeight - height) / 2) * 2 + 1;

            var room = Box2i.FromDimensions(x, y, width, height);

            var overlaps = false;

            foreach (var otherRoom in _rooms)
            {
                if (room.Intersects(otherRoom))
                {
                    overlaps = true;
                    break;
                }
            }

            if (overlaps)
                continue;

            _rooms.Add(room);


            _currentRegion++;
            for (var tileX = room.Left; tileX < room.Right; tileX++)
            {
                for (var tileY = room.Bottom; tileY < room.Top; tileY++)
                {
                    var point = new Vector2i(tileX, tileY);
                    Carve(grid, point, new Tile(2));
                }
            }

            // // Fill in all of the empty space with mazes.
            // for (var y = 1; y < DungeonHeight; y += 2)
            // {
            //     for (var x = 1; x < DungeonWidth; x += 2)
            //     {
            //         var pos = new Vec(x, y);
            //         if (getTile(pos) != Tiles.wall)
            //             continue;
            //         _growMaze(pos);
            //     }
            // }
        }

    }
}
