using UnityEngine;

public class BuildingBehaviorInjector : MonoBehaviour
{
    public IBuildingFlippable BuildingFlippable;
    public IBuildingPosable BuildingPosable;
    public IBuildingPlaceable BuildingPlaceable;
    public IBuildingInitTileInfo BuildingInitTileInfo;

    #region Inject
    public void InjectFlippable(IBuildingFlippable flippable) => BuildingFlippable = flippable;
    public void InjectPosable(IBuildingPosable posable) => BuildingPosable = posable;
    public void InjectPlaceable(IBuildingPlaceable placeable) => BuildingPlaceable = placeable;
    public void InjectInitTileInfo(IBuildingInitTileInfo initTileInfo) => BuildingInitTileInfo = initTileInfo;
    #endregion

    #region BuildingFlippable
    public void OnFlip() => BuildingFlippable.OnFlip();
    public void SetFlip(bool isFlip) => BuildingFlippable.SetFlip(isFlip);
    #endregion

    #region BuildingPosable
    public void SetAreaPos(Vector3Int pos) => BuildingPosable.SetAreaPos(pos);
    public void SetPos(Vector3Int pos) => BuildingPosable.SetPos(pos);
    public void CancelEdit(BoundsInt prevArea, BoundsInt area)
    {
        BuildingPosable.SetArea(prevArea);
        SetPos(area.position);
    }
    #endregion

    #region BuildingPlaceable
    public void OnPlace(BuildingObj obj) => BuildingPlaceable.OnPlace(obj);
    public void OnTakeBuild()=> BuildingPlaceable.SetActiveSelectedObj(true);
    public void SetActiveSelectedObj(bool on) => BuildingPlaceable.SetActiveSelectedObj(on);
    #endregion

    #region BuildingInitTileInfo
    public void InitObjDetectTile(BuildingObj obj) => BuildingInitTileInfo.InitObjDetectTile(obj);
    #endregion
}
