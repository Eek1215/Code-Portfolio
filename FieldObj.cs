using System;
using System.Collections.Generic;
using UnityEngine;

public class FieldObj : BuildingObj
{
    public FieldData Data { get; private set; }
    private IFieldObjClickCheck FieldObjClickCheck;

    private int _stepIdx;

    public override void Inject(BuildingBehaviorInjector injector)
    {
        base.Inject(injector);
        FieldObjClickCheck = new FieldObjClickCheck();
    }

    public void Init()
    {
        _injector.SetPos(Data.PlacedPos);
        _injector.SetFlip(Data.IsFlip);

        _injector.SetAreaPos(Data.PlacedPos);

        _injector.InitObjDetectTile(this);

        // 지어진 게 있으며 종료가 되지 않았을 경우.
        if (Data.IsPlantedCrop)
        {
            _stepIdx = Data.HasFinishedGrowing() ? 0 : -1;
            CheckImgPerSec(TimeMgr.Instance.CurrentTime);

            if (_stepIdx != 1)
            {
                TimeMgr.Instance.AddUpdatePerSecEvent(CheckImgPerSec);
            }
        }
        else
        {
            _stepIdx = -1;
        }
    }

    public override void OnCompleteLoad(BuildingData data)
    {
        base.OnCompleteLoad(data);
        Data = data as FieldData;
    }

    public override void OnPlace(eBuildingSystemState systemState)
    {
        GridBuildingSystem system = GridMgr.Instance.GridBuildingSystem;
        Vector3Int pos = system.Grid.LocalToCell(transform.position);
        _injector.SetPos(pos);

        system.ClearArea();

        _injector.SetActiveSelectedObj(false);
        _injector.OnPlace(this);

        if (systemState == eBuildingSystemState.Building)
        {
            Data = DataMgr.Instance.InGameData.AddBuildingData<FieldData>(BuildingType, pos, sr_icon.flipX);
        }
        else if (systemState == eBuildingSystemState.Edit)
        {
            DataMgr.Instance.InGameData.EditBuildingData(Data, pos, sr_icon.flipX);
        }
    }

    public void OnPlant(eCrop cropType)
    {
        Data.Plant(cropType);
        _stepIdx = -1;

        CheckImgPerSec(TimeMgr.Instance.CurrentTime);
        TimeMgr.Instance.AddUpdatePerSecEvent(CheckImgPerSec);
    }

    public void OnHarvest()
    {
        _stepIdx = -1;

        UIMgr.Instance.UIMain.GetWealthEffect.ShowWealthEffect((eWealth)Enum.Parse(typeof(eWealth), Data.PlantedCrop.ToString())
                                                                , Camera.main.WorldToScreenPoint(transform.position));
        Data.Harvest();
        sr_icon.sprite = ResourceMgr.Instance.GetRef<FieldObjInfo>(this, ResourceKey.FieldInfo, eResourcePath.Prefab)
                                                                                    .spr_emptyField;

        TimeMgr.Instance.RemoveUpdatePerSecEvent(CheckImgPerSec);
        ResourceMgr.Instance.RemoveRef<FieldObjInfo>(this, ResourceKey.FieldInfo);
    }

    public void CheckImgPerSec(DateTime curTime)
    {
        if(_stepIdx == -1)
        {
            _stepIdx = 0;

            sr_icon.sprite = (ResourceMgr.Instance.GetRef<FieldObjInfo>(this, ResourceKey.FieldInfo, eResourcePath.Prefab))
                                                                                    .GetFieldSpr(Data.PlantedCrop, _stepIdx);
        }
        else if (_stepIdx == 0)
        {
            if(Data.HasFinishedGrowing(curTime))
            {
                _stepIdx = 1;
                sr_icon.sprite = (ResourceMgr.Instance.GetRef<FieldObjInfo>(this, ResourceKey.FieldInfo, eResourcePath.Prefab))
                                                                                    .GetFieldSpr(Data.PlantedCrop, _stepIdx);
            }
        }
    }

    public override void OnClickEvent()
    {
        // 아직 배치전에 배치중인 상태.
        if (Data == null)
            return;

        UIMgr uiMgr = UIMgr.Instance;

        if (!Data.IsPlantedCrop)
        {
            uiMgr.UIMain.ShowPlantCropPanel(ePlantCropPanel.Crop);
            uiMgr.UIWorld.HideFieldCropStatusPanel();
        }
        else
        {
            if (Data.HasFinishedGrowing())
            {
                uiMgr.UIMain.ShowPlantCropPanel(ePlantCropPanel.Harvest);
                uiMgr.UIWorld.HideFieldCropStatusPanel();
            }
            else
            {
                uiMgr.UIWorld.ShowFieldCropStatusPanel(this);
                uiMgr.UIMain.HidePlantCropPanel();
            }
        }
    }
}
