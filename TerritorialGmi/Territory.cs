using lgBase;
using UnityEngine;

namespace FrontlineLogistics.TerritorialGmi
{
    internal static class Territory
    {
        private const int WreckageBuildingId = 50057;

        internal static Holdings Count()
        {
            var result = new Holdings();
            var map = MapManager.MapManagerInstance;
            var camp = InputController.CampType;
            if (camp == TypeManager.CampType.None) return result;
            foreach (var pair in MapManager.MapTileDic)
            {
                var tile = pair.Value;
                if (tile.battleLineCamp != camp) continue;
                result.Tiles++;
                var data = map.tileMapDataDic[pair.Key];
                var attributes = BuildingAttributes(tile, data, map);
                if (data.roadIndex >= 0 || (attributes & TypeManager.BuildingAttribute.GravelRoad) != 0)
                    result.Roads++;
                if (data.railwayIndex >= 0) result.Rails++;
                if (map.CheckIsBridge(pair.Key) || (attributes & TypeManager.BuildingAttribute.Bridge) != 0)
                    result.Bridges++;
                if ((attributes & TypeManager.BuildingAttribute.Building) != 0
                    && (attributes & TypeManager.BuildingAttribute.Forest) == 0)
                    result.Buildings++;
            }
            return result;
        }

        private static TypeManager.BuildingAttribute BuildingAttributes(OverlayTile tile, TileMapData data, MapManager map)
        {
            // BuildingOnTile is a lazy, world-mutating getter. Inspect its backing field instead.
            var live = tile._buildingOnTile;
            if (live != null)
            {
                var manager = live.buildingManager;
                if (manager.currentBuildingHp <= 0
                    && (live.IsConstruction || live.BuildingData.cellMaxHp != -1)) return 0;
                return Attributes(live.BuildingTypeID);
            }
            if (tile.hasBuildingInstantiated) return 0;

            // Uninstantiated scenery still earns income. Overlay saved replacements/tombstones first.
            Vector2Int origin = data.mainBuildingTilePos;
            if (origin == new Vector2Int(-1, -1)) origin = tile.gridLocation;
            if (MapManager.MapTileDic.TryGetValue(origin, out var originTile)
                && SaveManager.saveManagerInstance.isModifiedBuildingInstantiateDic.TryGetValue(originTile.tileWorldPosition, out var modified))
                return Attributes(modified.x);
            if (map.mapBuildingSpawnDic.TryGetValue(origin, out var spawn))
                return Attributes((int)spawn.x);
            return 0;
        }

        private static TypeManager.BuildingAttribute Attributes(int id)
        {
            if (id <= 0 || id == WreckageBuildingId) return 0;
            return ConfigsTable.BuildingDataDic.TryGetValue(id, out var config) ? config.buildingAttribute : 0;
        }
    }
}
