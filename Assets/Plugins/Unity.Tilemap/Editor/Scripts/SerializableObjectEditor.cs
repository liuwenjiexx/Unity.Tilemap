using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using static Unity.Tilemaps.SerializableObject.SerializableItemValue;
using System.Linq;
using UnityEditor;

namespace Unity.Tilemaps
{
    interface ICustomFieldEditor
    {
        void GUIField(SerializedObject serializedObject, FieldInfo field, PropertyAttribute attribute, GUIContent label, object thisObj);
    }


    class CustomFieldEditor
    {

        public static string ToDisplayName(string name)
        {
            if (name == null || name.Length == 0)
                return "";

            if ('a' <= name[0] && name[0] <= 'z')
            {
                if (name.Length > 1)
                    name = name[0].ToString().ToUpper() + name.Substring(1);
                else
                    name = name[0].ToString().ToUpper();

            }

            for (int i = 1; i < name.Length; i++)
            {
                char ch = name[i];
                if ('A' <= ch && ch <= 'Z')
                {
                    name = name.Substring(0, i) + " " + name.Substring(i);
                    i += 2;
                }
            }
            return name;
        }

        public static GUIContent GetFieldLabel(FieldInfo field)
        {
            string name = field.Name;
            string tooltip = null;
            name = ToDisplayName(name);
            var tooltipAttr = field.GetCustomAttribute<TooltipAttribute>();
            if (tooltipAttr != null)
                tooltip = tooltipAttr.tooltip;
            GUIContent label = new GUIContent(name, tooltip);
            return label;
        }

        public static void GUIObject(SerializedProperty property)
        {
            object thisObj = property.GetObjectOfProperty();
            if (thisObj == null)
            {
                EditorGUILayout.LabelField(property.LabelContent(), "null, type:" + property.type);
                return;
            }
            Type type = thisObj.GetType();
            bool changed = false;
            bool firstDrawer = false;
            Action<MemberInfo> drawMember = (mInfo) =>
            {
                FieldInfo field = mInfo as FieldInfo;
                if (field != null)
                {
                    GUIContent label = GetFieldLabel(field);

                    using (new GUILayout.HorizontalScope())
                    {
                        object fieldValue = field.GetValue(thisObj), newValue;

                        PropertyAttribute attribute;
                        var drawer = GUIDrawerInfo.FindGUIDrawer(field, out attribute) as ICustomFieldEditor;
                        if (drawer != null)
                        {
                            if (firstDrawer)
                            {
                                firstDrawer = false;
                            }
                            drawer.GUIField(property.serializedObject, field, attribute, label, thisObj);
                        }
                        else
                        {

                            EditorGUILayout.PrefixLabel(label);
                            newValue = LayoutValueField(fieldValue, field.FieldType);
                            if (!object.Equals(newValue, fieldValue))
                            {
                                Undo.RecordObject(property.serializedObject.targetObject, null);
                                field.SetValue(thisObj, newValue);
                                changed = true;
                                //SetTargetDirty();
                                //node.OnAfterDeserialize();

                            }
                        }
                    }
                }
            };
            using (new GUILayout.VerticalScope())
            {
                foreach (var field in SerializableObject.GetSerializeFields(type))
                {
                    SerializedProperty fieldProperty = property.FindPropertyRelative(field.Name);
                    if (fieldProperty == null)
                    {
                        drawMember(field);
                        continue;
                    }
                    EditorGUILayout.PropertyField(fieldProperty);
                }
            }
            if (GUI.changed && !Application.isPlaying)
            {
                property.serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(property.serializedObject.targetObject);
                property.serializedObject.UpdateIfRequiredOrScript();
                GUI.changed = false;
                //Debug.Log("change"+property.serializedObject.targetObject);
            }
        }




        public static object LayoutValueField(object value, Type type, params GUILayoutOption[] options)
        {
            return ValueField(value, type);
        }

        static object ValueField(object value, Type type, params GUILayoutOption[] options)
        {
            bool handle = false;
            object newValue = value;
            GUIContent labelContent = GUIContent.none;
            if (type.IsEnum)
            {
                if (type.IsDefined(typeof(FlagsAttribute)))
                {
                    //if (hasLabel)
                    //    newValue = EditorGUILayout.EnumFlagsField(label, (Enum)value, GUILayout.ExpandWidth(true));
                    //else
                    newValue = EditorGUILayout.EnumFlagsField((Enum)value, options);
                    handle = true;
                }
                else
                {
                    //if (hasLabel)
                    //    newValue = EditorGUILayout.EnumPopup(label, (Enum)value, GUILayout.ExpandWidth(true));
                    //else
                    newValue = EditorGUILayout.EnumPopup((Enum)value, options);
                    handle = true;
                }
                return newValue;
            }

            var typeCode = SerializableObject.SerializableValue.TypeToSerializableTypeCode(type);
            switch (typeCode)
            {
                case SerializableTypeCode.String:
                    {
                        string str = value as string;
                        str = str ?? "";
                        newValue = EditorGUILayout.DelayedTextField(str, options);
                    }
                    break;
                case SerializableTypeCode.Int32:
                    {
                        int n = (int)value;
                        newValue = EditorGUILayout.DelayedIntField(n, options);
                    }
                    break;
                case SerializableTypeCode.Single:
                    {
                        float f = (float)value;
                        newValue = EditorGUILayout.DelayedFloatField(f, options);
                    }
                    break;
                case SerializableTypeCode.Boolean:
                    {
                        bool b;
                        b = (bool)value;
                        newValue = EditorGUILayout.Toggle(b, options);
                    }
                    break;
                case SerializableTypeCode.UnityObject:
                    {
                        UnityEngine.Object obj = value as UnityEngine.Object;
                        newValue = EditorGUILayout.ObjectField(obj, type, true, options);
                    }
                    break;
                case SerializableTypeCode.Vector2:
                    {
                        Vector2 v = (Vector2)value;
                        newValue = EditorGUILayout.Vector2Field(labelContent, v, options);
                    }
                    break;
                case SerializableTypeCode.Vector2Int:
                    {
                        Vector2Int v = (Vector2Int)value;
                        newValue = EditorGUILayout.Vector2IntField(labelContent, v, options);
                    }
                    break;
                case SerializableTypeCode.Vector3:
                    {
                        Vector3 v = (Vector3)value;
                        newValue = EditorGUILayout.Vector3Field(labelContent, v, options);
                    }
                    break;
                case SerializableTypeCode.Vector3Int:
                    {
                        Vector3Int v = (Vector3Int)value;
                        newValue = EditorGUILayout.Vector3IntField(labelContent, v, options);
                    }
                    break;
                case SerializableTypeCode.Vector4:
                    {
                        Vector4 v = (Vector4)value;
                        newValue = EditorGUILayout.Vector4Field(labelContent, v, options);
                    }
                    break;
                case SerializableTypeCode.Color:
                    {
                        Color v = (Color)value;
                        newValue = EditorGUILayout.ColorField(labelContent, v, options);
                    }
                    break;
                case SerializableTypeCode.Rect:
                    {
                        Rect v = (Rect)value;
                        newValue = EditorGUILayout.RectField(labelContent, v, options);
                    }
                    break;
                case SerializableTypeCode.RectInt:
                    {
                        RectInt v = (RectInt)value;
                        newValue = EditorGUILayout.RectIntField(labelContent, v, options);
                    }
                    break;
                case SerializableTypeCode.RectOffset:
                    {
                        RectOffset v = value as RectOffset;
                        if (v == null)
                            v = new RectOffset();
                        using (new GUILayout.VerticalScope())
                        {
                            using (new GUILayout.HorizontalScope())
                            {
                                GUILayout.Label("L", GUILayout.ExpandWidth(false));
                                v.left = EditorGUILayout.DelayedIntField(v.left);
                                GUILayout.Label("R", GUILayout.ExpandWidth(false));
                                v.right = EditorGUILayout.DelayedIntField(v.right);
                            }
                            using (new GUILayout.HorizontalScope())
                            {
                                GUILayout.Label("T", GUILayout.ExpandWidth(false));
                                v.top = EditorGUILayout.DelayedIntField(v.top);
                                GUILayout.Label("B", GUILayout.ExpandWidth(false));
                                v.bottom = EditorGUILayout.DelayedIntField(v.bottom);
                            }
                        }
                        newValue = v;
                    }
                    break;
                case SerializableTypeCode.RectOffsetSerializable:
                    {
                        RectOffsetSerializable v = value as RectOffsetSerializable;
                        if (v == null)
                            v = new RectOffsetSerializable();
                        using (new GUILayout.VerticalScope())
                        {
                            using (new GUILayout.HorizontalScope())
                            {
                                GUILayout.Label("L", GUILayout.ExpandWidth(false));
                                v.left = EditorGUILayout.DelayedIntField(v.left);
                                GUILayout.Label("R", GUILayout.ExpandWidth(false));
                                v.right = EditorGUILayout.DelayedIntField(v.right);
                            }
                            using (new GUILayout.HorizontalScope())
                            {
                                GUILayout.Label("T", GUILayout.ExpandWidth(false));
                                v.top = EditorGUILayout.DelayedIntField(v.top);
                                GUILayout.Label("B", GUILayout.ExpandWidth(false));
                                v.bottom = EditorGUILayout.DelayedIntField(v.bottom);
                            }
                        }
                        newValue = v;
                    }
                    break;
                case SerializableTypeCode.Bounds:
                    {
                        Bounds v = (Bounds)value;
                        newValue = EditorGUILayout.BoundsField(labelContent, v, options);
                    }
                    break;
                case SerializableTypeCode.AnimationCurve:
                    {
                        AnimationCurve v = (AnimationCurve)value;
                        newValue = EditorGUILayout.CurveField(labelContent, v, options);
                    }
                    break;
            }
            return newValue;
        }

        static object ValueField(Rect rect, object value, Type type)
        {
            object newValue = value;
            GUIContent labelContent = GUIContent.none;
            if (type.IsEnum)
            {
                if (type.IsDefined(typeof(FlagsAttribute)))
                {
                    //if (hasLabel)
                    //    newValue = EditorGUILayout.EnumFlagsField(label, (Enum)value, GUILayout.ExpandWidth(true));
                    //else
                    newValue = EditorGUI.EnumFlagsField(rect, (Enum)value);
                }
                else
                {
                    //if (hasLabel)
                    //    newValue = EditorGUILayout.EnumPopup(label, (Enum)value, GUILayout.ExpandWidth(true));
                    //else
                    newValue = EditorGUI.EnumPopup(rect, (Enum)value);
                }
                return newValue;
            }

            var typeCode = SerializableObject.SerializableValue.TypeToSerializableTypeCode(type);
            switch (typeCode)
            {
                case SerializableTypeCode.String:
                    {
                        string str = value as string;
                        str = str ?? "";
                        newValue = EditorGUI.DelayedTextField(rect, str);
                    }
                    break;
                case SerializableTypeCode.Int32:
                    {
                        int n = (int)value;
                        newValue = EditorGUI.DelayedIntField(rect, n);
                    }
                    break;
                case SerializableTypeCode.Single:
                    {
                        float f = (float)value;
                        newValue = EditorGUI.DelayedFloatField(rect, f);
                    }
                    break;
                case SerializableTypeCode.Boolean:
                    {
                        bool b;
                        b = (bool)value;
                        newValue = EditorGUI.Toggle(rect, b);
                    }
                    break;
                case SerializableTypeCode.UnityObject:
                    {
                        UnityEngine.Object obj = value as UnityEngine.Object;
                        newValue = EditorGUI.ObjectField(rect, obj, type, true);
                    }
                    break;
                case SerializableTypeCode.Vector2:
                    {
                        Vector2 v = (Vector2)value;
                        newValue = EditorGUI.Vector2Field(rect, labelContent, v);
                    }
                    break;
                case SerializableTypeCode.Vector2Int:
                    {
                        Vector2Int v = (Vector2Int)value;
                        newValue = EditorGUI.Vector2IntField(rect, labelContent, v);
                    }
                    break;
                case SerializableTypeCode.Vector3:
                    {
                        Vector3 v = (Vector3)value;
                        newValue = EditorGUI.Vector3Field(rect, labelContent, v);
                    }
                    break;
                case SerializableTypeCode.Vector3Int:
                    {
                        Vector3Int v = (Vector3Int)value;
                        newValue = EditorGUI.Vector3IntField(rect, labelContent, v);
                    }
                    break;
                case SerializableTypeCode.Vector4:
                    {
                        Vector4 v = (Vector4)value;
                        newValue = EditorGUI.Vector4Field(rect, labelContent, v);
                    }
                    break;
                case SerializableTypeCode.Color:
                    {
                        Color v = (Color)value;
                        newValue = EditorGUI.ColorField(rect, labelContent, v);
                    }
                    break;
                case SerializableTypeCode.Rect:
                    {
                        Rect v = (Rect)value;
                        newValue = EditorGUI.RectField(rect, labelContent, v);
                    }
                    break;
                case SerializableTypeCode.RectInt:
                    {
                        RectInt v = (RectInt)value;
                        newValue = EditorGUI.RectIntField(rect, labelContent, v);
                    }
                    break;
                case SerializableTypeCode.Bounds:
                    {
                        Bounds v = (Bounds)value;
                        newValue = EditorGUI.BoundsField(rect, labelContent, v);
                    }
                    break;
                case SerializableTypeCode.AnimationCurve:
                    {
                        AnimationCurve v = (AnimationCurve)value;
                        newValue = EditorGUI.CurveField(rect, labelContent, v);
                    }
                    break;
            }
            return newValue;
        }

        class GUIDrawerInfo
        {
            public Type drawerType;
            public PropertyDrawer instance;
            public PropertyAttribute attribute;
            public bool children;
            static Dictionary<Type, GUIDrawerInfo> cachedGUIDrawers;

            static FieldInfo typeFIeld;
            static FieldInfo useForChildrenField;

            public static GUIDrawer FindGUIDrawer(FieldInfo field, out PropertyAttribute attribute)
            {
                GUIDrawerInfo info;
                attribute = null;

                if (field.IsDefined(typeof(HideInInspector), true))
                    return null;

                if (cachedGUIDrawers == null)
                {
                    cachedGUIDrawers = new Dictionary<Type, GUIDrawerInfo>();

                    typeFIeld = typeof(CustomPropertyDrawer).GetField("m_Type", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    useForChildrenField = typeof(CustomPropertyDrawer).GetField("m_UseForChildren", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                    foreach (var guiType in AppDomain.CurrentDomain.GetAssemblies()
                         .Referenced(new Assembly[] { typeof(PropertyDrawer).Assembly })
                         .SelectMany(o => o.GetTypes())
                         .Where(o => !o.IsAbstract && o.IsSubclassOf(typeof(PropertyDrawer))))
                    {

                        foreach (var customAttr in guiType.GetCustomAttributes(typeof(CustomPropertyDrawer), true)
                            .Select(o => (CustomPropertyDrawer)o))
                        {
                            info = new GUIDrawerInfo()
                            {
                                drawerType = guiType,
                            };
                            info.children = (bool)useForChildrenField.GetValue(customAttr);
                            Type attrType = (Type)typeFIeld.GetValue(customAttr);
                            cachedGUIDrawers[attrType] = info;
                        }
                    }
                }

                info = null;

                foreach (var attr in field.GetCustomAttributes(typeof(PropertyAttribute), true))
                {
                    if (cachedGUIDrawers.TryGetValue(attr.GetType(), out info))
                    {
                        attribute = (PropertyAttribute)attr;
                        break;
                    }
                }

                //Type parent = type;
                //while (parent != null)
                //{
                //    if (cachedGUIDrawers.TryGetValue(parent, out info))
                //    {
                //        if (!info.children && parent != type)
                //        {
                //            info = null;
                //            continue;
                //        }
                //        break;
                //    }
                //    parent = parent.BaseType;
                //}
                if (info == null)
                    return null;
                if (info.instance == null)
                    info.instance = Activator.CreateInstance(info.drawerType) as PropertyDrawer;

                return info.instance;
            }

        }



    }
}