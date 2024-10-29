using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System;

namespace Unity.Tilemaps
{

    public static class Extensions
    {
        public static GUIContent LabelContent(this SerializedProperty property)
        {
            return new GUIContent(property.displayName, property.tooltip);
        }

        public static void GUIFillRect(this Rect position, Color color)
        {
            Event evt = Event.current;
            if (evt.type == EventType.Repaint)
            {
                var old = GUI.color;
                GUI.color = color;
                GUI.DrawTexture(position, EditorGUIUtility.whiteTexture);
                GUI.color = old;
            }
        }
        public static void GUIDrawBorder(this Rect position, float borderWidth, Color color)
        {
            Event evt = Event.current;
            if (evt.type == EventType.Repaint)
            {
                var old = GUI.color;
                GUI.color = color;
                var img = EditorGUIUtility.whiteTexture;
                GUI.DrawTexture(new Rect(position.x, position.y, position.width, borderWidth), img);
                GUI.DrawTexture(new Rect(position.x, position.yMax - borderWidth, position.width, borderWidth), img);
                GUI.DrawTexture(new Rect(position.xMax - borderWidth, position.yMin + borderWidth, borderWidth, position.height - borderWidth * 2), img);
                GUI.DrawTexture(new Rect(position.xMin, position.yMin + borderWidth, borderWidth, position.height - borderWidth * 2), img);
                GUI.color = old;
            }
        }

        public static T[] InsertArrayElementAtIndex<T>(this T[] array, T item)
        {
            return InsertArrayElementAtIndex(array, array.Length, item);
        }

        public static T[] InsertArrayElementAtIndex<T>(this T[] array, int index, T item)
        {
            List<T> list;
            if (array != null)
                list = new List<T>(array);
            else
                list = new List<T>();
            list.Insert(index, item);
            return list.ToArray();
        }

        public static T[] DeleteArrayElementAtIndex<T>(this T[] array, int index)
        {
            List<T> list;
            if (array != null)
                list = new List<T>(array);
            else
                list = new List<T>();
            list.RemoveAt(index);
            return list.ToArray();
        }
        public static void Swap<T>(this T[] array, int from, int to)
        {
            var tmp = array[from];
            array[from] = array[to];
            array[to] = tmp;
        }
        public static IEnumerable<Assembly> Referenced(this IEnumerable<Assembly> assemblies, IEnumerable<Assembly> referenced)
        {

            foreach (var ass in assemblies)
            {
                if (referenced.Where(o => o == ass).FirstOrDefault() != null)
                {
                    yield return ass;
                }
                else
                {
                    foreach (var refAss in ass.GetReferencedAssemblies())
                    {
                        if (referenced.Where(o => o.FullName == refAss.FullName).FirstOrDefault() != null)
                        {
                            yield return ass;
                            break;
                        }
                    }
                }
            }
        }
        public static object GetObjectOfProperty(this SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }
            return obj;
        }


        private static object GetValue_Imp(object source, string name, int index)
        {
            var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
            if (enumerable == null) return null;
            var enm = enumerable.GetEnumerator();
            //while (index-- >= 0)
            //    enm.MoveNext();
            //return enm.Current;

            for (int i = 0; i <= index; i++)
            {
                if (!enm.MoveNext()) return null;
            }
            return enm.Current;
        }

        private static MemberInfo GetValue_ImpMember(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                    return f;

                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                    return p;

                type = type.BaseType;
            }
            return null;
        }

        private static object GetValue_Imp(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                    return f.GetValue(source);

                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                    return p.GetValue(source, null);

                type = type.BaseType;
            }
            return null;
        }

        public static void SetObjectOfProperty(this SerializedProperty prop, object value)
        {
            var owner = GetObjectOfPropertyOwner(prop);
            if (owner == null)
                return;
            var member = GetValue_ImpMember(owner, prop.name);
            if (member is FieldInfo)
                ((FieldInfo)member).SetValue(owner, value);
            else
                ((PropertyInfo)member).SetValue(owner, value, null);
            //var path = prop.propertyPath.Replace(".Array.data[", "[");
            //object obj = prop.serializedObject.targetObject;
            //var elements = path.Split('.');
            //for (int i = 0; i < elements.Length; i++)
            //{
            //    string element = elements[i];
            //    if (element.Contains("["))
            //    {
            //        var elementName = element.Substring(0, element.IndexOf("["));
            //        var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));

            //        obj = GetValue_Imp(obj, elementName, index);

            //    }
            //    else
            //    {
            //        if (i == elements.Length - 1)
            //        {
            //            var member = GetValue_ImpMember(obj, element);
            //            if (member is FieldInfo)
            //                ((FieldInfo)member).SetValue(obj, value);
            //            else
            //                ((PropertyInfo)member).SetValue(obj, value, null);
            //        }
            //        else
            //            obj = GetValue_Imp(obj, element);
            //    }
            //}
            //return obj;
        }

        public static object GetObjectOfPropertyOwner(this SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            for (int i = 0, len = elements.Length - 1; i < len; i++)
            {

                string element = elements[i];
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }
            return obj;
        }
        public static void GUIDrawGrid(this Color lineColor, Vector2Int gridSize, Vector2 cellSize, float lineWidth)
        {
            Vector2 pos = new Vector2();
            for (int x = 0; x <= gridSize.x; x++)
            {
                pos.x = x * cellSize.x;
                pos.y = 0;
                new Rect(pos.x, pos.y, lineWidth, gridSize.y * cellSize.y)
                    .GUIFillRect(lineColor);
            }
            for (int y = 0; y <= gridSize.y; y++)
            {
                pos.x = 0;
                pos.y = y * cellSize.y;
                new Rect(pos.x, pos.y, gridSize.x * cellSize.x, lineWidth)
                    .GUIFillRect(lineColor);
            }
        }

        public static void GUIFillGrid(this Color fillColor, Vector2 cellSize, int x, int y)
        {
            new Rect(x * cellSize.x, y * cellSize.y, cellSize.x, cellSize.y).GUIFillRect(fillColor);
        } 
     
    
        static Dictionary<Type, object[][]> cachedTypes;
        public static object[][] GetCachedTypes(Type baseType, Func<Type, string> getDisplayName)
        {
            if (cachedTypes == null)
                cachedTypes = new Dictionary<Type, object[][]>();
            object[][] types;
            if (!cachedTypes.TryGetValue(baseType, out types))
            {

                var tmp = AppDomain.CurrentDomain.GetAssemblies()
                     .Referenced(new Assembly[] { baseType.Assembly })
                     .SelectMany(o => o.GetTypes())
                     .Where(o => !o.IsAbstract && o.IsSubclassOf(baseType))
                     .Select(o => new object[] { o, new GUIContent(getDisplayName == null ? o.Name : getDisplayName(o)) })
                     .OrderBy(o => ((GUIContent)o[1]).text)
                     .ToArray();

                tmp = new object[][] { new object[] { null, new GUIContent("None") } }
                .Concat(tmp)
                .ToArray();

                Type[] _types = tmp.Select(o => (Type)o[0]).ToArray();
                GUIContent[] displays = tmp.Select(o => (GUIContent)o[1]).ToArray();
                types = new object[][] {  _types,displays                    };
                cachedTypes[baseType] = types;
            }
            return types;
        }



        public static object TypePopup(this GUIContent label, object value, Type baseType, Func<Type, string> getDisplayName)
        {
            var cached = GetCachedTypes(baseType, getDisplayName);

            int selectedIndex = -1;
            Type selectedType = null;
            if (value != null)
                selectedType = value.GetType();
            Type[] types =(Type[]) cached[0];
            GUIContent[] displays =(GUIContent[]) cached[1];

            for (int i = 0; i < types.Length; i++)
            {
                if ( types[i] == selectedType|| (types[i]==null&& selectedType==baseType))
                {
                    selectedIndex = i;
                    break;
                }
            }

 
            int newIndex = EditorGUILayout.Popup(label, selectedIndex, displays);
            if (newIndex != selectedIndex)
            {
                if (newIndex < 0 || types[newIndex] == null)
                {
                    value = null;
                }
                else
                {
                    selectedType = types[newIndex];

                    value = Activator.CreateInstance(selectedType);
                    GUI.changed = true;
                }
            }
            return value;
        }

   
   
    }
}