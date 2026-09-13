using System.Linq;
using System.Numerics;
using Content.Shared.FunnyWall;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Utility;

namespace Content.Client.FunnyWall;

/// <summary>
/// This handles...
/// </summary>
public sealed partial class FunnyWallOverlay : Overlay
{
    [Dependency] private IEntityManager _entityManager = default!;

    private EntityLookupSystem _lookup;
    private SharedTransformSystem _transform;


    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    private HashSet<Entity<FunnyWallComponent>> _entities = new();


    public FunnyWallOverlay()
    {
        IoCManager.InjectDependencies(this);

        _lookup = _entityManager.System<EntityLookupSystem>();
        _transform = _entityManager.System<SharedTransformSystem>();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (args.Viewport.Eye == null)
            return;


        var handle = args.WorldHandle;

        var eyePos = args.Viewport.Eye.Position.Position;

        _entities.Clear();
        _lookup.GetEntitiesIntersecting(args.Viewport.Eye.Position.MapId, args.WorldAABB, _entities);

        var size = new Vector2(1, 1);

        var entitiesSorted = new SortedList<float, Entity<TransformComponent>>();

        //
        foreach (var ent in _entities)
        {
            var xform = _entityManager.GetComponent<TransformComponent>(ent);
            var entCoords = _transform.GetWorldPosition(ent);
            var distance = (entCoords - eyePos);

            var distanceNormalize = distance.Normalize();

            entitiesSorted.Add(distanceNormalize, (ent.Owner, xform));
        }


        foreach (var (_, ent) in entitiesSorted.Reverse())
        {
            var (entCoords, entRotation) = _transform.GetWorldPositionRotation(ent.Comp);

            var distance = entCoords - eyePos;

            var box = new Box2(entCoords - size / 2, entCoords + size / 2);
            var boxRotated = new Box2Rotated(box, entRotation, entCoords);

            var topPos = entCoords + distance;

            var boxTop = box.Translated(distance);
            var boxTopRotated = new Box2Rotated(boxTop, entRotation, topPos).Enlarged(0.5f);


            handle.DrawRect(boxRotated, Color.Black);

            //handle.DrawLine(eyePos, entCoords, Color.Red);

            var side1Bottom = boxRotated.TopRight;
            var side2Bottom = boxRotated.BottomRight;
            var side3Bottom = boxRotated.TopLeft;
            var side4Bottom = boxRotated.BottomLeft;

            var topRightTop = boxTopRotated.TopRight;
            var topRightBottom = boxTopRotated.BottomRight;
            var topLeftTop = boxTopRotated.TopLeft;
            var topLeftBottom = boxTopRotated.BottomLeft;


            //this sucks ass but idk the math to d o help me
            var topNormal    = new Vector2(0, 1);
            var rightNormal  = new Vector2(1, 0);
            var bottomNormal = new Vector2(0, -1);
            var leftNormal   = new Vector2(-1, 0);
            var backWorld  = entRotation.RotateVec(topNormal);
            var rightWorld = entRotation.RotateVec(rightNormal);
            var frontWorld = entRotation.RotateVec(bottomNormal);
            var leftWorld  = entRotation.RotateVec(leftNormal);
            var toEye = eyePos - boxRotated.Center;
            bool showBack  = Vector2.Dot(backWorld,  toEye) > 0;
            bool showRight = Vector2.Dot(rightWorld, toEye) > 0;
            bool showFront = Vector2.Dot(frontWorld, toEye) > 0;
            bool showLeft  = Vector2.Dot(leftWorld,  toEye) > 0;


            Dictionary<Color, List<Vector2>> quads = new();
            // Top face
            if (showBack)
                AddQuad(quads, topLeftBottom, topLeftTop, topRightTop, topRightBottom, Color.Pink);

            // Right face
            if (showRight)
                AddQuad(quads, side1Bottom, side2Bottom, topRightBottom, topRightTop, Color.Green);

            // Left face
            if (showLeft)
                AddQuad(quads, side3Bottom, side4Bottom, topLeftBottom, topLeftTop, Color.Cyan);

            // Back face
            if (showBack)
                AddQuad(quads, side3Bottom, side1Bottom, topRightTop, topLeftTop, Color.Red);

            // Front face
            if (showFront)
                AddQuad(quads, side4Bottom, side2Bottom, topRightBottom, topLeftBottom, Color.Yellow);

            foreach (var (color, vectorList) in quads)
            {
                handle.DrawPrimitives(DrawPrimitiveTopology.TriangleList, vectorList, color);
            }


            //draw the top i forgot what order shit goes into addquad already so fuck it fuck my life
            var list = new List<Vector2>();
            list.Add(topLeftBottom);
            list.Add(topLeftTop);
            list.Add(topRightBottom);
            list.Add(topLeftTop);
            list.Add(topRightBottom);
            list.Add(topRightTop);

            handle.DrawPrimitives(DrawPrimitiveTopology.TriangleList, list, Color.DarkBlue);
        }
    }
    private static void AddQuad(Dictionary<Color, List<Vector2>> verts, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color color)
    {
        var list = verts.GetOrNew(color);

        list.Add(a);
        list.Add(b);
        list.Add(c);

        list.Add(a);
        list.Add(c);
        list.Add(d);
    }
}
