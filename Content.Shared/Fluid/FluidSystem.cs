using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Robust.Shared.GameStates;
using Robust.Shared.Map;

namespace Content.Shared.Fluid;

/// <summary>
/// This handles...
/// </summary>
public abstract partial class FluidSystem : EntitySystem
{
    [Dependency] protected ChunkEntitySystem ChunkEntities = default!;
    [Dependency] protected EntityQuery<FluidChunkComponent> TileChunkQuery = default!;

    [Dependency] private SharedTransformSystem _transform = default!;

    public void AddFluid(EntityUid gridUid, Vector2i coordinates, Solution solution)
    {
        var tileSolution  = GetOrCreateTileSolution(gridUid, coordinates);

        tileSolution.Solution.AddSolution(solution, ProtoMan);
    }

    public TileSolution GetOrCreateTileSolution(EntityUid gridUid, Vector2i coordinates)
    {
        var chunk  = GetOrCreateFluidChunk(gridUid, coordinates);

        if (chunk.Comp2.Tiles.TryGetValue(coordinates, out var tileSolution))
            return tileSolution;

        chunk.Comp2.Tiles[coordinates] = tileSolution = new TileSolution(gridUid, coordinates);
        return tileSolution;
    }

    public Entity<ChunkEntityComponent, FluidChunkComponent> GetOrCreateFluidChunk(EntityUid gridUid, Vector2i coordinates)
    {
        var chunk = ChunkEntities.GetOrCreateChunk(gridUid, ChunkEntitySystem.GetChunkIndices(coordinates));
        return (chunk.Owner, chunk.Comp, EnsureComp<FluidChunkComponent>(chunk.Owner));
    }

    public bool TryGetFluidChunk(EntityUid gridUid, Vector2i coordinates, [NotNullWhen(true)] out Entity<ChunkEntityComponent, FluidChunkComponent>? fluidChunk)
    {
        fluidChunk = null;

        if (ChunkEntities.TryGetChunk(gridUid, ChunkEntitySystem.GetChunkIndices(coordinates), out var chunk)
            && TileChunkQuery.TryComp(chunk, out var fluidChunkComp))
        {
            fluidChunk = (chunk.Value.Owner, chunk.Value.Comp, fluidChunkComp);
            return true;
        }

        return false;
    }

}
