using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EditorFieldInfo
{
    public string FieldLabel;
    public SerializedProperty Property;

    public EditorFieldInfo(string label, SerializedProperty property)
    {
        FieldLabel = label;
        Property = property;
    }   
}

[CustomEditor(typeof(StudentUnitData))]
public class StudentUnitDataEditor : Editor
{
    private HashSet<string> prefabGroupNames = new HashSet<string>
    {
        "WorldPrefab", "BattlePrefab"
    };
    private EditorFieldInfo[] prefabGroup;

    private HashSet<string> spriteGroupNames = new HashSet<string>
    {
        "spr_icon", "spr_weapon"
    };
    private EditorFieldInfo[] spriteGroup;

    private HashSet<string> statGroupNames = new HashSet<string>
    {
        "maxHP", "attack", "defense", "moveSpeed", "attackSpeed"
    };
    private EditorFieldInfo[] statGroup;

    private HashSet<string> varianceGroupNames = new HashSet<string>
    {
        "hpVariance", "attackVariance", "defenseVariance", "speedVariance", "lvVariance"
    };
    private EditorFieldInfo[] varianceGroup;

    private HashSet<string> levelScalingGroupNames = new HashSet<string>
    {
        "hpPerLv", "attackPerLv", "defensePerLv"
    };
    private EditorFieldInfo[] levelScalingGroup;


    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        SerializedProperty properties = serializedObject.GetIterator();
        if (properties.NextVisible(true))
        {
            while (properties.NextVisible(false))
            {
                if (!prefabGroupNames.Contains(properties.name) && !spriteGroupNames.Contains(properties.name) &&
                    !statGroupNames.Contains(properties.name) && !varianceGroupNames.Contains(properties.name) &&
                    !levelScalingGroupNames.Contains(properties.name))
                {
                    EditorGUILayout.PropertyField(properties, true);
                }
            }
        }

        SetFieldInfos(prefabGroupNames, ref prefabGroup);
        DrawColumn("Prefab", 2, prefabGroup);

        SetFieldInfos(spriteGroupNames, ref spriteGroup);
        DrawColumn("Sprite", 2, spriteGroup);

        SetFieldInfos(statGroupNames, ref statGroup);
        DrawColumn("Stat", 2, statGroup);

        if(GUILayout.Button("Reset Stat to Default"))
        {
            ResetStatToDefault();
        }

        SetFieldInfos(varianceGroupNames, ref varianceGroup);
        DrawColumn("Stat Variance %", 2, varianceGroup);

        if (GUILayout.Button("Reset Stat Variance to Default"))
        {
            ResetStatVarianceToDefault();
        }

        SetFieldInfos(levelScalingGroupNames, ref levelScalingGroup);
        DrawColumn("Level Scaling : Increase Per Lv", 2, levelScalingGroup);

        //// 바뀐 값을 적용시킴.
        serializedObject.ApplyModifiedProperties();
    }

    void SetFieldInfos(HashSet<string> groupNames, ref EditorFieldInfo[] infos)
    {
        EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

        infos = new EditorFieldInfo[groupNames.Count];
        int idx = 0;
        foreach (var fieldName in groupNames)
        {
            infos[idx] = new EditorFieldInfo(fieldName, serializedObject.FindProperty(fieldName));
            idx++;
        }
    }

    void DrawColumn(string groupLabel, int columnCnt, EditorFieldInfo[] fields)
    {
        EditorGUILayout.LabelField(groupLabel, EditorStyles.boldLabel);

        for (int i = 0; i < fields.Length; i++)
        {
            if (i % columnCnt == 0)
            {
                EditorGUILayout.BeginHorizontal();
            }
            EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(fields[i].FieldLabel), GUILayout.Width(100));

            EditorGUILayout.PropertyField(fields[i].Property, GUIContent.none);

            // 항목 사이의 간격추가
            EditorGUILayout.Space(5);

            if ((i + 1) % columnCnt == 0 || i == fields.Length - 1)
            {
                EditorGUILayout.EndHorizontal();
            }
        }
    }

    void ResetStatToDefault()
    {
        serializedObject.FindProperty("maxHP").floatValue = StudentUnitData.DEFAULT_MAXHP;
        serializedObject.FindProperty("attack").floatValue = StudentUnitData.DEFAULT_ATTACK;
        serializedObject.FindProperty("defense").floatValue = StudentUnitData.DEFAULT_DEFENSE;
        serializedObject.FindProperty("moveSpeed").floatValue = StudentUnitData.DEFAULT_MOVE_SPEED;
        serializedObject.FindProperty("attackSpeed").floatValue = StudentUnitData.DEFAULT_ATTACK_SPEED;
    }

    void ResetStatVarianceToDefault()
    {
        serializedObject.FindProperty("hpVariance").floatValue = StudentUnitData.DEFAULT_MAXHP_VARIANCE;
        serializedObject.FindProperty("attackVariance").floatValue = StudentUnitData.DEFAULT_ATTACK_VARIANCE;
        serializedObject.FindProperty("defenseVariance").floatValue = StudentUnitData.DEFAULT_DEFENSE_VARIANCE;
        serializedObject.FindProperty("speedVariance").floatValue = StudentUnitData.DEFAULT_SPEED_VARIANCE;
        serializedObject.FindProperty("lvVariance").intValue = StudentUnitData.DEFAULT_LV_VARIANCE;
    }
}
