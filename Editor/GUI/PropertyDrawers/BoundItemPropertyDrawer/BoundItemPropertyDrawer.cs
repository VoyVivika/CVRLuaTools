﻿#if UNITY_EDITOR && CVR_CCK_EXISTS
using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NAK.LuaTools
{
    [CustomPropertyDrawer(typeof(NAKLuaClientBehaviourWrapper.BoundItem))]
    public class BoundItemDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);
            rect.y += 2;
            rect.height = EditorGUIUtility.singleLineHeight;
            
            SerializedProperty nameProp = property.FindPropertyRelative("name");
            SerializedProperty typeProp = property.FindPropertyRelative("type");

            var halfWidth = rect.width * 0.5f;
            var nameWidth = halfWidth * 0.65f;
            var typeWidth = halfWidth * 0.35f;
            var quarterWidth = halfWidth * 0.25f;
            var txtOffset = 14;
            
            const float buttonWidth = 19;

            Rect nameRect = new(rect.x, rect.y, nameWidth, rect.height);
            Rect typeRect = new(rect.x + nameWidth + 2, rect.y, typeWidth - 2, rect.height);
            Rect valueRect = new(rect.x + halfWidth + 2, rect.y, halfWidth - 2, rect.height);
            Rect buttonRect = new(rect.x + rect.width - buttonWidth, rect.y, buttonWidth, rect.height);

            // Why couldn't Vector 4s just be Properly Supported in EditorGUI.PropertyField
            // If there's a better way to do this, let me know, cause this is what I thought to do.
            Rect vec4xRect = new(rect.x + halfWidth + 2 + txtOffset, rect.y, (quarterWidth - txtOffset) - 2, rect.height);
            Rect vec4yRect = new((rect.x + halfWidth + 2 + txtOffset) + (quarterWidth), rect.y, (quarterWidth - txtOffset) - 2, rect.height);
            Rect vec4zRect = new((rect.x + halfWidth + 2 + txtOffset) + ((quarterWidth) * 2), rect.y, (quarterWidth - txtOffset) - 2, rect.height);
            Rect vec4wRect = new((rect.x + halfWidth + 2 + txtOffset) + ((quarterWidth) * 3), rect.y, (quarterWidth - txtOffset) - 2, rect.height);
            Rect txt4xRect = new(rect.x + halfWidth + 2, rect.y, txtOffset, rect.height);
            Rect txt4yRect = new(rect.x + halfWidth + 2 + (quarterWidth), rect.y, txtOffset, rect.height);
            Rect txt4zRect = new(rect.x + halfWidth + 2 + (quarterWidth * 2), rect.y, txtOffset, rect.height);
            Rect txt4wRect = new(rect.x + halfWidth + 2 + (quarterWidth * 3), rect.y, txtOffset, rect.height);

            EditorGUI.PropertyField(nameRect, nameProp, GUIContent.none);
            EditorGUI.PropertyField(typeRect, typeProp, GUIContent.none);
            
            var itemType = (NAKLuaClientBehaviourWrapper.BoundItemType)typeProp.enumValueIndex;
            switch (itemType)
            {
                case NAKLuaClientBehaviourWrapper.BoundItemType.Object:
                    SerializedProperty objectReference = property.FindPropertyRelative("objectReference");

                    if (DragAndDropHelper.CheckDragAndDrop(nameRect))
                        DragAndDropHelper.ApplyDragAndDropToProperty(objectReference, nameProp);

                    bool showComponentDropdown = objectReference.objectReferenceValue is GameObject or Component;
                    if (showComponentDropdown)
                    {
                        valueRect.width -= buttonWidth; // c is for cock
                        if (GUI.Button(buttonRect, "C")) ShowComponentDropdown(objectReference);
                    }
                    
                    EditorGUI.PropertyField(valueRect, objectReference, GUIContent.none);
                    break;
                case NAKLuaClientBehaviourWrapper.BoundItemType.Integer:
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("intValue"), GUIContent.none);
                    break;
                case NAKLuaClientBehaviourWrapper.BoundItemType.Boolean:
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("boolValue"), GUIContent.none);
                    break;
                case NAKLuaClientBehaviourWrapper.BoundItemType.Float:
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("floatValue"), GUIContent.none);
                    break;
                case NAKLuaClientBehaviourWrapper.BoundItemType.String:
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("stringValue"), GUIContent.none);
                    break;
                case NAKLuaClientBehaviourWrapper.BoundItemType.Color:
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("colorValue"), GUIContent.none);
                    break;
                case NAKLuaClientBehaviourWrapper.BoundItemType.LayerMask:
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("layerMaskValue"),
                        GUIContent.none);
                    break;
                case NAKLuaClientBehaviourWrapper.BoundItemType.Vector2:
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("vector2Value"), GUIContent.none);
                    break;
                case NAKLuaClientBehaviourWrapper.BoundItemType.Vector3:
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("vector3Value"), GUIContent.none);
                    break;
                case NAKLuaClientBehaviourWrapper.BoundItemType.Rect:
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("rectValue"), GUIContent.none);
                    break;
                case NAKLuaClientBehaviourWrapper.BoundItemType.Bounds:
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("boundsValue"), GUIContent.none);
                    break;
                case NAKLuaClientBehaviourWrapper.BoundItemType.Quaternion:
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("quaternionValue"),
                        GUIContent.none);
                    break;
                case NAKLuaClientBehaviourWrapper.BoundItemType.Vector2Int:
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("vector2IntValue"),
                        GUIContent.none);
                    break;
                case NAKLuaClientBehaviourWrapper.BoundItemType.Vector3Int:
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("vector3IntValue"),
                        GUIContent.none);
                    break;
                case NAKLuaClientBehaviourWrapper.BoundItemType.RectInt:
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("rectIntValue"), GUIContent.none);
                    break;
                case NAKLuaClientBehaviourWrapper.BoundItemType.BoundsInt:
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("boundsIntValue"),
                        GUIContent.none);
                    break;
                case NAKLuaClientBehaviourWrapper.BoundItemType.Vector4:

                    // If unity doesn't wanna directly give me a Vector 4 Editor, then I'll make one, and it'll be HACKY!!!
                    // Update: so earlier doing something like FindPropertyRelative("vector4Value.x") didn't work, so I assumed I couldn't... and then it just decides to work.
                    // I don't even
                    // www.youtube.com/watch?v=QV-DZtN2IMU
                    // sincerely, VoyVivika
                    EditorGUI.PropertyField(vec4xRect, property.FindPropertyRelative("vector4Value.x"), GUIContent.none);
                    EditorGUI.LabelField(txt4xRect, "X");
                    EditorGUI.PropertyField(vec4yRect, property.FindPropertyRelative("vector4Value.y"), GUIContent.none);
                    EditorGUI.LabelField(txt4yRect, "Y");
                    EditorGUI.PropertyField(vec4zRect, property.FindPropertyRelative("vector4Value.z"), GUIContent.none);
                    EditorGUI.LabelField(txt4zRect, "Z");
                    EditorGUI.PropertyField(vec4wRect, property.FindPropertyRelative("vector4Value.w"), GUIContent.none);
                    EditorGUI.LabelField(txt4wRect, "W");

                    // Leaving this here whenever they actually add proper Vector 4 support to EditorGUI.PropertyField
                    //EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("vector4Value"), GUIContent.none);
                    break;
                case NAKLuaClientBehaviourWrapper.BoundItemType.Table:
                    EditorGUI.LabelField(valueRect, "Table has entries");
                    rect.y += EditorGUIUtility.singleLineHeight + 2f;
                    SerializedProperty boundEntriesProp = property.FindPropertyRelative("boundEntries");
                    EditorGUI.PropertyField(rect, boundEntriesProp, GUIContent.none);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            
            SerializedProperty typeProp = property.FindPropertyRelative("type");
            switch ((NAKLuaClientBehaviourWrapper.BoundItemType)typeProp.enumValueIndex)
            {
                case NAKLuaClientBehaviourWrapper.BoundItemType.Rect:
                case NAKLuaClientBehaviourWrapper.BoundItemType.Bounds:
                case NAKLuaClientBehaviourWrapper.BoundItemType.RectInt:
                case NAKLuaClientBehaviourWrapper.BoundItemType.BoundsInt:
                    return (height * 2) + 4;
                case NAKLuaClientBehaviourWrapper.BoundItemType.Table:
                    return height + 4 + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("boundEntries"));
                default:
                    return height + 4;
            }
        }
        
        private static void ShowComponentDropdown(SerializedProperty objectReference)
        {
            int instanceId = objectReference.objectReferenceInstanceIDValue;
            GameObject gameObject = objectReference.objectReferenceValue switch
            {
                GameObject go => go,
                Component component => component.gameObject,
                _ => null
            };

            if (gameObject == null) 
                return;
            
            // collect components on the GameObject and show a dropdown menu
            
            GenericMenu menu = new();
            menu.AddItem(new GUIContent("GameObject"), gameObject.GetInstanceID() == instanceId, () =>
            {
                objectReference.objectReferenceValue = gameObject;
                objectReference.serializedObject.ApplyModifiedProperties();
            });
            
            menu.AddSeparator(string.Empty); // looks nice to separate root object from components
            
            var components = gameObject.GetComponents<Component>();
            for (var i = 0; i < components.Length; i++)
            {
                Component component = components[i]; // appending index so object with multiple of same component can be selected
                menu.AddItem(new GUIContent($"{i}. {component.GetType().Name}"), component.GetInstanceID() == instanceId, () =>
                {
                    objectReference.objectReferenceValue = component;
                    objectReference.serializedObject.ApplyModifiedProperties();
                });
            }

            menu.ShowAsContext();
        }
    }
}
#endif