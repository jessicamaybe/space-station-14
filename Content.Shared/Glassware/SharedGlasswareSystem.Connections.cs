using System.Diagnostics.CodeAnalysis;
using Content.Shared.Glassware.Components;

namespace Content.Shared.Glassware;

public abstract partial class SharedGlasswareSystem
{
    /// <summary>
    /// creates a glassware network
    /// </summary>
    public Entity<GlasswareNetworkComponent> CreateNetwork()
    {
        var networkEnt = Spawn();
        var networkComp = EnsureComp<GlasswareNetworkComponent>(networkEnt);

        return (networkEnt, networkComp);
    }

    public bool TryGetNetwork(Entity<GlasswareComponent> ent, [NotNullWhen(true)] out Entity<GlasswareNetworkComponent>? network)
    {
        network = null;

        if (ent.Comp.Network != null)
        {
            if (!TryComp<GlasswareNetworkComponent>(ent.Comp.Network, out var networkComp))
                return false;

            network = (ent.Comp.Network.Value, networkComp);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Connects two devices to eachother, merging the networks if needed.
    /// </summary>
    public void ConnectDevice(Entity<GlasswareComponent> originNode, Entity<GlasswareComponent> targetNode)
    {
        Entity<GlasswareNetworkComponent>? network = null;
        var hasOrigin = TryGetNetwork(originNode, out var originNetwork);
        var hasTarget = TryGetNetwork(targetNode, out var targetNetwork);

        if (hasOrigin && hasTarget)
        {
            if (originNetwork != targetNetwork)
            {
                MergeNetwork(originNetwork!.Value, targetNetwork!.Value);
            }

            network = originNetwork;
        }
        else if (hasOrigin)
        {
            network = originNetwork;
        }
        else if (hasTarget)
        {
            network = targetNetwork;
        }
        if (network == null)
            network = CreateNetwork();

        network.Value.Comp.Map.TryAdd(originNode.Owner, new HashSet<EntityUid>());
        network.Value.Comp.Map.TryAdd(targetNode.Owner, new HashSet<EntityUid>());

        network.Value.Comp.Map[originNode.Owner].Add(targetNode.Owner);

        originNode.Comp.Network = network;
        targetNode.Comp.Network = network;

        var tube = CreateEdgeVisuals(network.Value, originNode, targetNode);

        if (tube != null)
            network.Value.Comp.TubeVisuals.TryAdd((originNode, targetNode), tube.Value);

        Dirty(originNode);
        Dirty(targetNode);
    }

    public void RemoveDevice(Entity<GlasswareComponent> ent)
    {
        if (!TryGetNetwork(ent, out var network))
            return;

        if (TryGetOutlets(ent, out var outlets))
        {
            foreach (var outlet in outlets)
            {
                DeleteEdgeVisuals(network.Value, ent.Owner, outlet);
            }
        }

        if (TryGetInlets(ent, out var inlets))
        {
            foreach (var inlet in inlets)
            {
                DeleteEdgeVisuals(network.Value, inlet, ent.Owner);
            }
        }

        network.Value.Comp.Map.Remove(ent.Owner);
        foreach (var (node, neighbors) in network.Value.Comp.Map)
        {
            DeleteEdgeVisuals(network.Value, node, ent.Owner);
            neighbors.Remove(ent.Owner);

            foreach (var neighbor in neighbors)
            {
                if (TryComp<GlasswareComponent>(neighbor, out var neighborComp))
                {
                    if (!HasConnections((neighbor, neighborComp)))
                    {
                        neighborComp.Network = null;
                        Dirty(neighbor, neighborComp);
                    }
                }

            }
        }

        ent.Comp.Network = null;
        Dirty(ent);
    }

    public bool HasConnections(Entity<GlasswareComponent> ent)
    {
        if (!TryGetNetwork(ent, out _))
            return false;

        if (TryGetOutlets(ent, out _))
        {
            return true;
        }

        if (TryGetInlets(ent, out _))
        {
            return true;
        }

        return false;
    }

    public bool TryGetInlets(Entity<GlasswareComponent> ent, [NotNullWhen(true)] out HashSet<EntityUid>? inlets)
    {
        if (!TryGetNetwork(ent, out var network))
        {
            inlets = null;
            return false;
        }

        inlets = new HashSet<EntityUid>();

        foreach (var (from, neighbors) in network.Value.Comp.Map)
        {
            if (neighbors.Contains(ent.Owner))
                inlets.Add(from);
        }

        return inlets.Count > 0;
    }

    public bool TryGetOutlets(Entity<GlasswareComponent> ent, [NotNullWhen(true)] out HashSet<EntityUid>? outlets)
    {
        outlets = null;

        if (!TryGetNetwork(ent, out var network))
            return false;

        if (!network.Value.Comp.Map.TryGetValue(ent.Owner, out outlets))
            return false;

        if (outlets.Count != 0)
            return true;

        return false;
    }

    public bool HasOutLets(Entity<GlasswareComponent> ent)
    {

        return true;
    }

    public void MergeNetwork(Entity<GlasswareNetworkComponent> network, Entity<GlasswareNetworkComponent> targetNetwork)
    {
        foreach (var (node, neighbors) in network.Comp.Map)
        {
            if (TryComp<GlasswareComponent>(node, out var nodeComp))
                nodeComp.Network = targetNetwork;

            targetNetwork.Comp.Map.TryAdd(node, neighbors);

            foreach (var neighbor in neighbors)
            {
                targetNetwork.Comp.Map[node].Add(neighbor);
                targetNetwork.Comp.Map.TryAdd(neighbor, new HashSet<EntityUid>());
            }
        }
        foreach (var visualEnt in network.Comp.TubeVisuals)
        {
            targetNetwork.Comp.TubeVisuals.TryAdd(visualEnt.Key, visualEnt.Value);
        }
        Del(network);
    }

    /// <summary>
    /// Connects two devices to each other
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="target"></param>
    public void ConnectGlassware(Entity<GlasswareComponent> ent, Entity<GlasswareComponent> target)
    {
        ConnectDevice(ent, target);
    }

    /// <summary>
    /// Removes a piece of glassware from the network
    /// </summary>
    /// <param name="ent"></param>
    public void RemoveGlassware(Entity<GlasswareComponent> ent)
    {
        RemoveDevice(ent);
    }
}
