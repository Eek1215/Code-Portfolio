using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;


public static class AttributeUtil
{
    #region 오브젝트에 컴포넌트 자동 바인딩
    // 2중 포문으로도 표현이 되지만, 재귀로 작성.
    public static Transform FindChild(string name, Transform trs)
    {
        if (name == trs.name)
            return trs;

        int cnt = trs.childCount;
        for (int i = 0; i < cnt; i++)
        {
            Transform findTrs = FindChild(name, trs.GetChild(i));
            if (findTrs != null)
            {
                return findTrs;
            }
        }

        return null;
    }

    public static void InjectComponents(object obj)
    {
        Type type = obj.GetType();
        MonoBehaviour rootScript = obj as MonoBehaviour;
        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            // 어떤 타입일지 모르기 때문에 미리 타입을 읽어만 두고 후에 비교하기.
            var attribute = field.GetCustomAttributes().FirstOrDefault(a => a is FindComponentAttribute || a is FindComponentsAttribute);

            if (attribute == null)
            {
                continue;
            }

            Type fieldType = field.FieldType;
            if(fieldType == typeof(GameObject)) // !typeof(Component).IsAssignableFrom(fieldType) 컴포넌트 체크가 필요한 경우.
            {
                Debug.LogError($"필드 [{field.Name}] : FindComponent에 GameObject는 사용할 수 없습니다.");
                continue;
            }
            
            if (attribute is FindComponentAttribute)
            {
                FindComponentAttribute findAttribute = attribute as FindComponentAttribute;
                SetComponent(field, fieldType, findAttribute, rootScript);
            }
            else if (attribute is FindComponentsAttribute)
            {
                // 필드가 배열인 경우.
                if (fieldType.IsArray)
                {
                    // 배열이 어떤 배열인지 내부 요소를 구한다.
                    Type elementType = fieldType.GetElementType();
                    if (elementType == typeof(GameObject)) // !typeof(Component).IsAssignableFrom(fieldType) 컴포넌트 체크가 필요한 경우.
                    {
                        Debug.LogError($"필드 [{field.Name}] : FindComponent에 GameObject는 사용할 수 없습니다.");
                        continue;
                    }

                    FindComponentsAttribute arrayAttribute = attribute as FindComponentsAttribute;

                    // 배열을 만들고 그 배열에 집어넣고 마지막에 SetValue.                    
                    Array arr = Array.CreateInstance(elementType, arrayAttribute.ObjNames.Length);
                    int i = 0;
                    foreach (string objName in arrayAttribute.ObjNames)
                    {
                        Transform findChildTrs = FindChild(objName, rootScript.transform);

                        if (findChildTrs == null)
                        {
                            Debug.Log($"필드 변수 {field.Name}의 게임오브젝트를 찾지 못했습니다.");
                            continue;
                        }

                        // 찾아야할 컴포넌트의 타입을 알기.
                        Component findChildComponent = findChildTrs.GetComponent(elementType);

                        if (findChildComponent == null)
                        {
                            Debug.Log($"필드 변수 {field.Name}의 {fieldType} 컴포넌트를 찾지 못했습니다.");
                            continue;
                        }

                        arr.SetValue(findChildComponent, i);
                        i++;
                    }

                    field.SetValue(rootScript, arr);
                }
                // 타입이 리스트인 경우.
                else if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    Type elementType = fieldType.GetGenericArguments()[0];
                    if (elementType == typeof(GameObject)) // !typeof(Component).IsAssignableFrom(fieldType) 컴포넌트 체크가 필요한 경우.
                    {
                        Debug.LogError($"필드 [{field.Name}] : FindComponent에 GameObject는 사용할 수 없습니다.");
                        continue;
                    }

                    FindComponentsAttribute arrayAttribute = attribute as FindComponentsAttribute;

                    // 배열을 만들고 그 배열에 집어넣고 마지막에 SetValue.                    
                    IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
                    foreach (string objName in arrayAttribute.ObjNames)
                    {
                        Transform findChildTrs = FindChild(objName, rootScript.transform);

                        if (findChildTrs == null)
                        {
                            Debug.Log($"필드 변수 {field.Name}의 게임오브젝트를 찾지 못했습니다.");
                            continue;
                        }

                        // 찾아야할 컴포넌트의 타입을 알기.
                        Component findChildComponent = findChildTrs.GetComponent(elementType);

                        if (findChildComponent == null)
                        {
                            Debug.Log($"필드 변수 {field.Name}의 {fieldType} 컴포넌트를 찾지 못했습니다.");
                            continue;
                        }

                        list.Add(findChildComponent);
                    }

                    field.SetValue(rootScript, list);
                }
            }
        }
    }

    static void SetComponent(FieldInfo field, Type fieldType, FindComponentAttribute attribute, MonoBehaviour rootMono)
    {
        // 필드를 찾았으면 찾아야할 Transform을 가진 부모에서 Find로 찾기.
        Transform findChildTrs = FindChild(attribute.Name, rootMono.transform);

        if (findChildTrs == null)
        {
            Debug.Log($"필드 변수 {field.Name}의 게임오브젝트를 찾지 못했습니다.");
        }

        // 찾아야할 컴포넌트의 타입을 알기.
        Component findChildComponent = findChildTrs.GetComponent(fieldType);

        if (findChildComponent == null)
        {
            Debug.Log($"필드 변수 {field.Name}의 {fieldType} 컴포넌트를 찾지 못했습니다.");
        }

        field.SetValue(rootMono, findChildComponent);
    }
    #endregion


    #region 데이터 검증
    [Tooltip("숫자 자료형(int, float, double, long, short ...), 참조타입(클래스, 배열, Object ...)에 사용가능")]
    public static bool ValidateObject(object obj)
    {
        Type type = obj.GetType();
        FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        bool isValid = true;

        foreach (FieldInfo field in fields)
        {
            ValidateRangeAttribute rangeAttr = field.GetCustomAttribute<ValidateRangeAttribute>();
            if (rangeAttr == null)
            {
                object value = field.GetValue(obj);
                if (value is IConvertible)
                {
                    double convertValue = Convert.ToDouble(value);
                    if (convertValue < rangeAttr.Min || convertValue > rangeAttr.Max)
                    {
                        Debug.LogError($"범위 초과 : {type.Name}.{field.Name} = {convertValue} (범위 : {rangeAttr.Min}~{rangeAttr.Max})");
                        isValid = false;
                    }
                }
            }

            ValidateNotNullAttribute notNullAttr = field.GetCustomAttribute<ValidateNotNullAttribute>();
            if (notNullAttr != null)
            {
                object value = field.GetValue(obj);
                if (value == null || value is UnityEngine.Object unityobj && unityobj == null)
                {
                    Debug.LogError($"값 Null :  {type.Name}.{field.Name}이 Null입니다");
                    isValid = false;
                }
            }
        }

        return isValid;
    }
    #endregion
}