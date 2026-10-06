using Content.Shared.Fluid;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.GameStates;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;

namespace Content.Client.Fluid;


/// <summary>
/// visuzalies fluid for my health
/// my iv drip
/// </summary>
public sealed partial class FluidOverlay : Overlay
{
    [Dependency] private IEntityManager _entityManager = default!;
    [Dependency] private IPrototypeManager _prototypeManager = default!;


    private List<Entity<MapGridComponent>> _grids = new();

    private ChunkEntitySystem _chunkEntity;
    private SharedMapSystem _map;
    private SharedTransformSystem _transform;

    private EntityQuery<FluidChunkComponent> _chunkQuery;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    public FluidOverlay()
    {
        IoCManager.InjectDependencies(this);

        _chunkEntity = _entityManager.System<ChunkEntitySystem>();
        _map = _entityManager.System<SharedMapSystem>();
        _transform = _entityManager.System<SharedTransformSystem>();

        _chunkQuery = _entityManager.GetEntityQuery<FluidChunkComponent>();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {

        var handle = args.WorldHandle;

        _grids.Clear();
        _map.FindGridsIntersecting(args.MapId, args.WorldBounds, ref _grids);

        foreach (var grid in _grids)
        {
            if (!_entityManager.TryGetComponent(grid, out TransformComponent? xform))
                continue;

            var gridAABB = _transform.GetInvWorldMatrix(xform).TransformBox(args.WorldBounds.Enlarged(1f));
            var gridEntToWorld = _transform.GetWorldMatrix(xform);

            var chunks = _chunkEntity.GetChunksIntersecting(grid, gridAABB, _chunkQuery);

            while (chunks.MoveNext(out var chunkEnt))
            {
                var chunk = chunkEnt.Value.Comp2;

                foreach (var (pos, tileSolution) in chunk.Tiles)
                {
                    var box2 = new Box2(pos, pos + Vector2i.One);
                    handle.DrawRect(box2, tileSolution.Solution.GetColor(_prototypeManager));
                }
            }
        }
    }
}
