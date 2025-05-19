using UnityEngine;
using I2.Loc;

/* Scriptable Object를 사용함으로 데이터테이블이 필요한 경우를 대신함
 * 하나하나의 데이터를 관리하기가 편함 */
[CreateAssetMenu(fileName = "FacilityInfo", menuName = "Scriptable Object Asset/FacilityInfo", order = 0)]
public class FacilityInfo : ScriptableObject
{
    public eFacility Type;
    public int VisitableCustomerCnt;
    public float UsingTime;
    public OpenCondition[] OpenConditions;
}
