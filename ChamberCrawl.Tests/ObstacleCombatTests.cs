using ChamberCrawl.Models;
using ChamberCrawl.Systems;

namespace ChamberCrawl.Tests;

public class ObstacleCombatTests
{
    private static RunState BareRun()
    {
        var run = new RunState { Status = RunStatus.Playing, TimeMs = 0 };
        for (var y = 0; y < GameConstants.RoomH; y++)
            for (var x = 0; x < GameConstants.RoomW; x++)
                run.Room.Tiles[y, x] = GameConstants.TileFloor;
        run.PlayerStats = new PlayerStats { MaxHp = 6, Hp = 6, Damage = 1, FireRate = 4, Range = 320 };
        run.Entities.Add(new PlayerEntity
        {
            Id = "player", X = 48, Y = 48, Radius = GameConstants.PlayerRadius, Hp = 6,
        });
        return run;
    }

    [Fact]
    public void PlayerProjectile_DestroysCrate()
    {
        var run = BareRun();
        run.Room.Tiles[2, 3] = GameConstants.TileCrate;
        run.Entities.Add(new ProjectileEntity
        {
            Id = "p1", FromPlayer = true, X = 3.5f * GameConstants.Cell, Y = 2.5f * GameConstants.Cell,
            Vx = 0, Vy = 0, Radius = 4, Damage = 1, Life = 1,
        });
        Combat.Update(run, new InputSnapshot(), 0.016f);
        Assert.Equal(GameConstants.TileFloor, run.Room.Tiles[2, 3]);
        Assert.DoesNotContain(run.Entities, e => e.Id == "p1");
    }

    [Fact]
    public void PlayerProjectile_StopsOnSpike_WithoutDestroying()
    {
        var run = BareRun();
        run.Room.Tiles[2, 3] = GameConstants.TileSpike;
        run.Entities.Add(new ProjectileEntity
        {
            Id = "p1", FromPlayer = true, X = 3.5f * GameConstants.Cell, Y = 2.5f * GameConstants.Cell,
            Vx = 0, Vy = 0, Radius = 4, Damage = 1, Life = 1,
        });
        Combat.Update(run, new InputSnapshot(), 0.016f);
        Assert.Equal(GameConstants.TileSpike, run.Room.Tiles[2, 3]);
        Assert.DoesNotContain(run.Entities, e => e.Id == "p1");
    }

    [Fact]
    public void EnemyProjectile_DoesNotDestroyCrate()
    {
        var run = BareRun();
        run.Room.Tiles[2, 3] = GameConstants.TileCrate;
        run.Entities.Add(new ProjectileEntity
        {
            Id = "e1", FromPlayer = false, X = 3.5f * GameConstants.Cell, Y = 2.5f * GameConstants.Cell,
            Vx = 0, Vy = 0, Radius = 4, Damage = 1, Life = 1,
        });
        Combat.Update(run, new InputSnapshot(), 0.016f);
        Assert.Equal(GameConstants.TileCrate, run.Room.Tiles[2, 3]);
        Assert.DoesNotContain(run.Entities, e => e.Id == "e1");
    }
}
