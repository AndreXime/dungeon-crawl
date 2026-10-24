using ChamberCrawl.Data;
using ChamberCrawl.Models;

namespace ChamberCrawl.Tests;

public class EnemiesTests
{
    [Fact]
    public void AtDepth_1_MatchesBaseDefs()
    {
        foreach (EnemyKind kind in Enum.GetValues<EnemyKind>())
        {
            var bas = Enemies.Defs[kind];
            var scaled = Enemies.AtDepth(kind, 1);
            Assert.Equal(bas.Hp, scaled.Hp);
            Assert.Equal(bas.Damage, scaled.Damage);
            Assert.Equal(bas.Speed, scaled.Speed);
        }
    }

    [Fact]
    public void AtDepth_ScalesHpAndDamageByFloor()
    {
        var bat1 = Enemies.AtDepth(EnemyKind.Bat, 1);
        var bat5 = Enemies.AtDepth(EnemyKind.Bat, 5);
        Assert.True(bat5.Hp > bat1.Hp);
        Assert.Equal(bat1.Damage + 3, bat5.Damage);
        Assert.Equal(bat1.Speed, bat5.Speed);

        var ogre3 = Enemies.AtDepth(EnemyKind.Ogre, 3);
        Assert.Equal(MathF.Round(Enemies.Defs[EnemyKind.Ogre].Hp * 1.3f), ogre3.Hp);
        Assert.Equal(Enemies.Defs[EnemyKind.Ogre].Damage + 1, ogre3.Damage);
    }
}
