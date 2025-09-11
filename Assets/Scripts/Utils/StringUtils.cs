using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System;
using UnityEditor;

public static class StringUtils
{
    public static string Replace(string input, string pattern, string replacement)
    {
        List<string> inputList = new List<string>(input.Split(pattern));
        for(int i = 0; i < inputList.Count; i++)
        {
            if(i < inputList.Count-1)
            {
                inputList[i] += replacement;
            }
        }
        return String.Join("", inputList.ToArray());
    }
}

public class ListToPopUpAttribute : PropertyAttribute
{
    public Type myType;
    public string propertyName;
    public ListToPopUpAttribute(Type _myType, string _propertyName)
    {
        myType = _myType;
        propertyName = _propertyName;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ListToPopUpAttribute))]
public class ListToPopUpDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ListToPopUpAttribute atb = attribute as ListToPopUpAttribute;
        List<string> stringList = null;

        if(atb.myType.GetField(atb.propertyName) != null)
        {
            stringList = atb.myType.GetField(atb.propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).GetValue(property.serializedObject.targetObject) as List<string>;
        }

        if (stringList != null && stringList.Count != 0)
        {
            int selectedIndex = Mathf.Max(stringList.IndexOf(property.stringValue), 0);
            selectedIndex = EditorGUI.Popup(position, property.name, selectedIndex, stringList.ToArray());
            property.stringValue = stringList[selectedIndex];
        }
        else EditorGUI.PropertyField(position, property, label);
    }
}
#endif