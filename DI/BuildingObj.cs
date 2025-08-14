using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingObj : MonoBehaviour
{
    protected BuildingBehaviorInjector _injector;

    public eBuildingType BuildingType;
    protected BuildingData _data;

    public BoundsInt Area;

    public GameObject _selectedObj;
    public SpriteRenderer sr_icon;  // Flip을 위한.

    private void Awake()
    {
        _selectedObj.SetActive(false);
    }

    // Todo 처음 데이터 초기화 날림.

    public virtual void Inject()
    {
        _injector = GetComponent<BuildingBehaviorInjector>();
        _injector.InjectFlippable(new BuildingFlippable(sr_icon));
        _injector.InjectPosable(new BuildingPosable(transform));
        _injector.InjectPlaceable(new BuildingPlaceable(_selectedObj));
        _injector.InjectInitTileInfo(new BuildingInitTileInfo(this));
    }

    public virtual void OnCompleteLoad(BuildingData data) 
    {
        _data = data;
        _injector.SetPos(_data.PlacedPos);
        _injector.SetFlip(_data.IsFlip);
    }

    public void CancelEdit(BoundsInt prevArea)
    {
        Area = prevArea;
        _injector.SetPos(Area.position);
    }

    #region BuildingFlippable
    public void OnFlip() => _injector.OnFlip();
    public void SetFlip(bool isFlip) => _injector.SetFlip(isFlip);
    #endregion

    #region BuildingPosable
    public void SetAreaPos(Vector3Int pos) => _injector.SetAreaPos(pos);
    public void SetPos(Vector3Int pos) => _injector.SetPos(pos);
    public void CancelEdit(BoundsInt prevArea, BoundsInt area) => _injector.CancelEdit(prevArea, area);
    #endregion

    #region BuildingPlaceable
    public void OnPlace(BuildingObj obj) => _injector.OnPlace(this);
    public void OnTakeBuild() => _injector.SetActiveSelectedObj(true);
    public void SetActiveSelectedObj(bool on) => _injector.SetActiveSelectedObj(on);
    #endregion

    #region BuildingInitTileInfo
    public void InitObjDetectTile(BuildingObj obj) => _injector.InitObjDetectTile(this);
    #endregion


    public virtual void OnPlace(eBuildingSystemState systemState) { }
    public virtual void OnClickEvent() { }
}
