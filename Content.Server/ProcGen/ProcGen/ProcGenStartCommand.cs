using Robust.Shared.Console;

namespace Content.Server.ProcGen.ProcGen;

public sealed partial class ProcGenStartCommand : LocalizedCommands
{
    [Dependency] private IEntityManager _entityManager = default!;

    public override string Command => "lc_generate";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (!_entityManager.TrySystem(out ProcGenSystem? procGen))
            return;

        procGen.GenerateNow();
    }
}
