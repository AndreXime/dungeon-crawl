using System.Collections.Generic;
using System.Linq;
using ChamberCrawl.Models;

namespace ChamberCrawl.Systems;

public static class Doors
{
    public static bool IsRoomCleared(IEnumerable<Entity> entities) =>
        !entities.Any(e => e is EnemyEntity { Hp: > 0 });
}
