using UnityEngine;
using I2.Loc;

/* Scriptable Object�� ��������� ���������̺��� �ʿ��� ��츦 �����
 * �ϳ��ϳ��� �����͸� �����ϱⰡ ���� */
[CreateAssetMenu(fileName = "FacilityInfo", menuName = "Scriptable Object Asset/FacilityInfo", order = 0)]
public class FacilityInfo : ScriptableObject
{
    public eFacility Type;
    public int VisitableCustomerCnt;
    public float UsingTime;
    public OpenCondition[] OpenConditions;
}
