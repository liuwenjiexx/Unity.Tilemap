using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Unity.Tilemaps
{

    [Serializable]
    public class SerializableObject : ISerializationCallbackReceiver
    {
        [SerializeField]
        private string typeName;
        [SerializeField]
        private List<FieldValue> fields;

        private object value;

        public string DeserializeError { get; set; }

        public SerializableObject()
        {

        }
        public SerializableObject(object value)
        {
            this.Value = value;
        }



        public object Value
        {
            get { return value; }
            set { this.value = value; }
        }

        static Dictionary<Type, FieldInfo[]> cachedFields;
        static Dictionary<string, Type> cachedTypes;

        public static Type FindType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return null;
            if (cachedTypes == null)
                cachedTypes = new Dictionary<string, Type>();
            Type t;
            if (!cachedTypes.TryGetValue(typeName, out t))
            {
                t = typeName.GetTypeInAllAssemblies(false);
                cachedTypes[typeName] = t;
                return t;
            }
            return t;
        }

        static FieldInfo[] EmptyFields = new FieldInfo[0];

        public static FieldInfo[] GetSerializeFields(Type type)
        {
            if (cachedFields == null)
                cachedFields = new Dictionary<Type, FieldInfo[]>();
            FieldInfo[] fields;
            if (cachedFields.TryGetValue(type, out fields))
                return fields;


            fields = EmptyFields;
            List<FieldInfo> list = null;
            list = new List<FieldInfo>();



            Type parent = type.BaseType;
            if (parent != null)
            {
                var tmp = GetSerializeFields(parent);
                if (tmp != null)
                {
                    list.InsertRange(0, tmp);
                }
            }


            foreach (var field in type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                if (field.DeclaringType != type)
                    continue;
                if (field.IsDefined(typeof(NonSerializedAttribute), true))
                    continue;
                if (!field.IsPublic && !field.IsDefined(typeof(SerializeField), true))
                    continue;
                if (!list.Contains(field))
                {
                    list.Add(field);
                }
            }


            if (list != null && list.Count > 0)
            {
                fields = list.ToArray();
            }
            cachedFields[type] = fields;
            return fields;
        }

        public virtual void OnBeforeSerialize()
        {
            if (value != null)
            {
                if (value is ISerializationCallbackReceiver)
                    ((ISerializationCallbackReceiver)value).OnBeforeSerialize();
                var type = value.GetType();
                this.typeName = type.FullName;
                if (this.fields != null)
                    this.fields.Clear();

                var fields = GetSerializeFields(type);
                if (fields != null)
                {
                    foreach (var field in fields)
                    {
                        FieldValue fieldValue = new FieldValue();
                        fieldValue.field = field.Name;
                        object value = field.GetValue(this.value);

                        //Debug.Log("SerializeField:" + field + ", type:" + field.DeclaringType+","+value);
                        fieldValue.value = new SerializableValue(field.FieldType) { Value = value };

                        if (this.fields == null)
                            this.fields = new List<FieldValue>();

                        this.fields.Add(fieldValue);
                    }
                }

            }
        }
        public virtual void OnAfterDeserialize()
        {
            Type type = null;
            value = null;

            DeserializeError = null;

            if (string.IsNullOrEmpty(typeName))
            {
                //DeserializeError = "type name null";
                //Debug.LogError(DeserializeError);
                return;
            }

            type = FindType(typeName);
            if (type == null)
            {
                DeserializeError = "Not Found Type:" + typeName;
                Debug.LogError(DeserializeError);
                return;
            }

            try
            {
                object target = Activator.CreateInstance(type);

                var fieldValues = this.fields;

                if (fieldValues != null)
                {
                    var fields = GetSerializeFields(type);

                    if (fields != null)
                    {
                        object value;
                        foreach (var fieldValue in fieldValues)
                        {
                            var field = fields.Where(o => o.Name == fieldValue.field).FirstOrDefault();

                            if (field == null)
                                continue;
                            value = fieldValue.value.Value;
                            //Debug.Log("OnAfterDeserialize :" + field + ", type:" + field.DeclaringType + "," + value);
                            if (fieldValue.value.Value == null)
                                value = null;
                            try
                            {
                                field.SetValue(target, value);
                            }
                            catch (Exception ex)
                            {
                                Debug.LogException(ex);
                            }
                        }
                    }

                }
                if (target is ISerializationCallbackReceiver)
                    ((ISerializationCallbackReceiver)target).OnAfterDeserialize();
                this.value = target;
            }
            catch (Exception ex)
            {
                DeserializeError = ex.Message;
                Debug.LogException(ex);
            }
        }

        [Serializable]
        class FieldValue
        {
            public string field;
            public SerializableValue value;
            public override string ToString()
            {
                return string.Format("{0}={1}", field, value);
            }
        }


        [Serializable]
        public class SerializableValue : SerializableItemValue
        {
            [SerializeField]
            private SerializableItemValue[] array;

            public SerializableValue(Type type)
               : base(type)
            {
            }
            public SerializableValue(TypeCode typeCode)
                : base(typeCode)
            {
            }
            public SerializableValue(SerializableTypeCode typeCode)
            {
                this.typeCode = typeCode;
            }
            public SerializableValue(SerializableTypeCode typeCode, object value)
            {
                this.typeCode = typeCode;
                this.value = value;
            }

            public override void OnBeforeSerialize()
            {
                array = null;
                if (((typeCode & SerializableTypeCode.Array) == SerializableTypeCode.Array) || ((typeCode & SerializableTypeCode.List) == SerializableTypeCode.List))
                {

                    if (value != null)
                    {

                        if ((typeCode & SerializableTypeCode.Array) == SerializableTypeCode.Array)
                        {
                            SerializableTypeCode elemType = typeCode & (~SerializableTypeCode.Array);
                            var arr = value as Array;
                            if (arr != null)
                            {
                                array = new SerializableValue[arr.Length];
                                for (int i = 0; i < arr.Length; i++)
                                {
                                    array[i] = new SerializableValue(elemType, arr.GetValue(i));
                                }
                            }
                        }

                        if ((typeCode & SerializableTypeCode.List) == SerializableTypeCode.List)
                        {
                            SerializableTypeCode elemType = typeCode & (~SerializableTypeCode.List);
                            var list = value as IList;
                            if (list != null)
                            {
                                array = new SerializableValue[list.Count];
                                for (int i = 0; i < list.Count; i++)
                                {
                                    array[i] = new SerializableValue(elemType, list[i]);
                                }
                            }
                        }
                    }

                    if (array != null && array.Length > 0)
                    {
                        for (int i = 0; i < array.Length; i++)
                        {
                            array[i].OnBeforeSerialize();
                        }
                    }

                    return;
                }

                base.OnBeforeSerialize();
            }

            public override void OnAfterDeserialize()
            {
                if (array != null && array.Length > 0)
                {
                    for (int i = 0; i < array.Length; i++)
                    {
                        array[i].OnAfterDeserialize();
                    }
                }

                if (((typeCode & SerializableTypeCode.Array) == SerializableTypeCode.Array) || ((typeCode & SerializableTypeCode.List) == SerializableTypeCode.List))
                {
                    if (array != null && array.Length > 0)
                    {

                        if ((typeCode & SerializableTypeCode.Array) == SerializableTypeCode.Array)
                        {
                            var elemTypeCode = typeCode & (~SerializableTypeCode.Array);
                            Type elemType = SerializableTypeCodeToType(elemTypeCode);
                            var arr = Array.CreateInstance(elemType, array.Length);
                            for (int i = 0; i < array.Length; i++)
                            {
                                arr.SetValue(array[i].Value, i);
                            }
                            value = arr;
                        }
                        else
                        {
                            var elemTypeCode = typeCode & (~SerializableTypeCode.List);
                            Type elemType = SerializableTypeCodeToType(elemTypeCode);
                            var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elemType));
                            for (int i = 0; i < array.Length; i++)
                            {
                                list.Add(array[i].Value);
                            }
                            value = list;
                        }

                    }

                    return;
                }

                base.OnAfterDeserialize();

            }
        }


        [Serializable]
        public class SerializableItemValue : ISerializationCallbackReceiver
        {
            [SerializeField]
            protected SerializableTypeCode typeCode;
            [SerializeField]
            protected string stringValue;
            [SerializeField]
            protected UnityEngine.Object objectValue;

            [NonSerialized]
            protected object value;

            public SerializableItemValue() { }

            public SerializableItemValue(Type type)
           : this(TypeToSerializableTypeCode(type))
            {
            }
            public SerializableItemValue(TypeCode typeCode)
            {
                this.typeCode = (SerializableTypeCode)typeCode;
            }
            public SerializableItemValue(SerializableTypeCode typeCode)
            {
                this.typeCode = typeCode;
            }
            public SerializableItemValue(SerializableTypeCode typeCode, object value)
            {
                this.typeCode = typeCode;
                this.value = value;
            }
            public object Value
            {
                get { return this.value; }
                set { this.value = value; }
            }

            public SerializableTypeCode TypeCode
            {
                get { return typeCode; }
            }





            public virtual void OnBeforeSerialize()
            {
                objectValue = null;
                stringValue = null;

                if (((typeCode & SerializableTypeCode.Array) == SerializableTypeCode.Array) || ((typeCode & SerializableTypeCode.List) == SerializableTypeCode.List))
                {
                    return;
                }

                switch (typeCode)
                {
                    case SerializableTypeCode.UnityObject:
                        if (value != null)
                        {
                            objectValue = value as UnityEngine.Object;
                        }
                        else
                        {
                            objectValue = null;
                        }
                        break;
                    case SerializableTypeCode.Object:
                        break;
                    default:
                        stringValue = SerializeToString(typeCode, value);
                        break;
                }

                //Debug.Log(typeof(SerializableItemValue).Name + "OnBeforeSerialize objectValue:" + objectValue);
            }

            public virtual void OnAfterDeserialize()
            {
                value = null;

                if (((typeCode & SerializableTypeCode.Array) == SerializableTypeCode.Array) || ((typeCode & SerializableTypeCode.List) == SerializableTypeCode.List))
                {
                    return;
                }

                switch (typeCode)
                {
                    case SerializableTypeCode.UnityObject:
                        value = objectValue;
                        break;
                    case SerializableTypeCode.Object:
                        break;
                    default:
                        value = DeserializeFromString(typeCode, stringValue);
                        break;
                }
                //Debug.Log(typeof(SerializableItemValue).Name + "OnAfterDeserialize objectValue:" + objectValue);
            }
            public static SerializableTypeCode TypeToSerializableTypeCode(Type type)
            {
                var typeCode = (SerializableTypeCode)Type.GetTypeCode(type);
                SerializableTypeCode elemTypeCode;


                if (type.IsArray)
                {
                    elemTypeCode = TypeToSerializableTypeCode(type.GetElementType());
                    if (elemTypeCode != SerializableTypeCode.Object)
                        typeCode = SerializableTypeCode.Array | elemTypeCode;
                }
                if (type.IsGenericType)
                {
                    elemTypeCode = TypeToSerializableTypeCode(type.GetGenericArguments()[0]);
                    if (type.GetGenericTypeDefinition() == typeof(List<>) && elemTypeCode != SerializableTypeCode.Object)
                    {
                        typeCode = SerializableTypeCode.List | elemTypeCode;
                    }
                }

                if (typeCode == SerializableTypeCode.Object)
                {


                    if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                    {
                        typeCode = SerializableTypeCode.UnityObject;
                    }
                    else
                    {
                        if (type == typeof(Vector2))
                            typeCode = SerializableTypeCode.Vector2;
                        else if (type == typeof(Vector2Int))
                            typeCode = SerializableTypeCode.Vector2Int;
                        else if (type == typeof(Vector3))
                            typeCode = SerializableTypeCode.Vector3;
                        else if (type == typeof(Vector3Int))
                            typeCode = SerializableTypeCode.Vector3Int;
                        else if (type == typeof(Vector4))
                            typeCode = SerializableTypeCode.Vector4;
                        else if (type == typeof(Color))
                            typeCode = SerializableTypeCode.Color;
                        else if (type == typeof(Color32))
                            typeCode = SerializableTypeCode.Color32;
                        else if (type == typeof(Rect))
                            typeCode = SerializableTypeCode.Rect;
                        else if (type == typeof(RectInt))
                            typeCode = SerializableTypeCode.RectInt;
                        else if (type == typeof(RectOffset))
                            typeCode = SerializableTypeCode.RectOffset;
                        else if (type == typeof(RectOffsetSerializable))
                            typeCode = SerializableTypeCode.RectOffsetSerializable;
                        else if (type == typeof(AnimationCurve))
                            typeCode = SerializableTypeCode.AnimationCurve;
                    }
                }
                return typeCode;
            }

            public static Type SerializableTypeCodeToType(SerializableTypeCode typeCode)
            {

                if ((typeCode & SerializableTypeCode.Array) == SerializableTypeCode.Array)
                {
                    return SerializableTypeCodeToType(typeCode).MakeArrayType();
                }
                if ((typeCode & SerializableTypeCode.List) == SerializableTypeCode.List)
                {
                    return typeof(List<>).MakeGenericType(SerializableTypeCodeToType(typeCode));
                }

                switch (typeCode)
                {
                    case SerializableTypeCode.String:
                        return typeof(string);
                    case SerializableTypeCode.Int32:
                        return typeof(int);
                    case SerializableTypeCode.Single:
                        return typeof(float);
                    case SerializableTypeCode.Boolean:
                        return typeof(bool);
                    case SerializableTypeCode.DBNull:
                        return typeof(DBNull);
                    case SerializableTypeCode.Char:
                        return typeof(char);
                    case SerializableTypeCode.SByte:
                        return typeof(sbyte);
                    case SerializableTypeCode.Byte:
                        return typeof(byte);
                    case SerializableTypeCode.Int16:
                        return typeof(short);
                    case SerializableTypeCode.Int64:
                        return typeof(long);
                    case SerializableTypeCode.UInt16:
                        return typeof(ushort);
                    case SerializableTypeCode.UInt32:
                        return typeof(uint);
                    case SerializableTypeCode.UInt64:
                        return typeof(ulong);
                    case SerializableTypeCode.Double:
                        return typeof(double);
                    case SerializableTypeCode.Decimal:
                        return typeof(decimal);
                    case SerializableTypeCode.DateTime:
                        return typeof(DateTime);
                    case SerializableTypeCode.UnityObject:
                        return typeof(UnityEngine.Object);
                    case SerializableTypeCode.Vector2:
                        return typeof(Vector2);
                    case SerializableTypeCode.Vector2Int:
                        return typeof(Vector2Int);
                    case SerializableTypeCode.Vector3:
                        return typeof(Vector3);
                    case SerializableTypeCode.Vector3Int:
                        return typeof(Vector3Int);
                    case SerializableTypeCode.Vector4:
                        return typeof(Vector4);
                    case SerializableTypeCode.Color:
                        return typeof(Color);
                    case SerializableTypeCode.Color32:
                        return typeof(Color32);
                    case SerializableTypeCode.Rect:
                        return typeof(Rect);
                    case SerializableTypeCode.RectInt:
                        return typeof(RectInt);
                    case SerializableTypeCode.RectOffset:
                        return typeof(RectOffset);
                    case SerializableTypeCode.RectOffsetSerializable:
                        return typeof(RectOffsetSerializable);
                    case SerializableTypeCode.AnimationCurve:
                        return typeof(AnimationCurve);
                }
                return typeof(object);
            }


            public static string SerializeToString(SerializableTypeCode typeCode, object value)
            {
                string str = null;


                switch (typeCode)
                {
                    case SerializableTypeCode.String:
                        str = value as string;
                        break;
                    case SerializableTypeCode.Vector2:
                        {
                            Vector2 v = (Vector2)value;
                            str = v.x.ToString() + "," + v.y.ToString();
                        }
                        break;
                    case SerializableTypeCode.Vector2Int:
                        {
                            Vector2Int v = (Vector2Int)value;
                            str = v.x.ToString() + "," + v.y.ToString();
                        }
                        break;
                    case SerializableTypeCode.Vector3:
                        {
                            Vector3 v = (Vector3)value;
                            str = v.x + "," + v.y + "," + v.z;
                        }
                        break;
                    case SerializableTypeCode.Vector3Int:
                        {
                            Vector3Int v = (Vector3Int)value;
                            str = v.x + "," + v.y + "," + v.z;
                        }
                        break;
                    case SerializableTypeCode.Vector4:
                        {
                            Vector4 v = (Vector4)value;
                            str = v.x + "," + v.y + "," + v.z + "," + v.w;
                        }
                        break;
                    case SerializableTypeCode.Color:
                        {
                            Color v = (Color)value;
                            str = v.r + "," + v.g + "," + v.b + "," + v.a;
                        }
                        break;
                    case SerializableTypeCode.Color32:
                        {
                            Color32 v = (Color32)value;
                            str = v.r + "," + v.g + "," + v.b + "," + v.a;
                        }
                        break;
                    case SerializableTypeCode.Rect:
                        {
                            Rect v = (Rect)value;
                            str = v.x + "," + v.y + "," + v.width + "," + v.height;
                        }
                        break;
                    case SerializableTypeCode.RectInt:
                        {
                            RectInt v = (RectInt)value;
                            str = v.x + "," + v.y + "," + v.width + "," + v.height;
                        }
                        break;
                    case SerializableTypeCode.RectOffset:
                        {
                            RectOffset v = value as RectOffset;
                            if (v == null)
                                v = new RectOffset();
                            str = v.left + "," + v.right + "," + v.top + "," + v.bottom;
                        }
                        break;
                    case SerializableTypeCode.RectOffsetSerializable:
                        {
                            RectOffsetSerializable v = value as RectOffsetSerializable;
                            if (v == null)
                                v = new RectOffsetSerializable();
                            str = v.left + "," + v.right + "," + v.top + "," + v.bottom;
                        }
                        break;
                    case SerializableTypeCode.AnimationCurve:
                        AnimationCurve curve = (AnimationCurve)value;
                        str = ToCurveString(curve);
                        break;
                    default:

                        if (value != null)
                        {
                            if (value is Enum)
                            {
                                if (typeof(int).IsInstanceOfType(value))
                                {
                                    //str = ((int)value).ToString();
                                    str = Convert.ToInt32(value).ToString();
                                }
                                else
                                {
                                    //str = ((long)value).ToString();
                                    str = Convert.ToInt64(value).ToString();
                                }
                            }
                            else
                            {
                                str = value.ToString();
                            }
                        }
                        break;
                }
                return str;
            }

            public static object DeserializeFromString(SerializableTypeCode typeCode, string str)
            {
                object value = null;
                switch (typeCode)
                {
                    case SerializableTypeCode.String:
                        value = str;
                        break;
                    case SerializableTypeCode.Double:
                        {
                            double n;
                            if (double.TryParse(str, out n))
                                value = n;
                            else
                                value = 0d;
                        }
                        break;
                    case SerializableTypeCode.Single:
                        {
                            float n;
                            if (float.TryParse(str, out n))
                                value = n;
                            else
                                value = 0f;
                        }
                        break;
                    case SerializableTypeCode.Int32:
                        {
                            int n;
                            if (int.TryParse(str, out n))
                                value = n;
                            else
                                value = 0;
                        }
                        break;
                    case SerializableTypeCode.Int64:
                        {
                            long n;
                            if (long.TryParse(str, out n))
                                value = n;
                            else
                                value = 0;
                        }
                        break;
                    case SerializableTypeCode.Boolean:
                        {
                            bool b;
                            if (bool.TryParse(str, out b))
                                value = b;
                            else
                                value = false;
                        }
                        break;
                    case SerializableTypeCode.Vector2:
                        {
                            Vector2 v = new Vector2();
                            float n;
                            string[] parts = str.Split(',');
                            for (int i = 0; i < parts.Length; i++)
                            {
                                if (float.TryParse(parts[i], out n))
                                    v[i] = n;
                            }
                            value = v;
                        }
                        break;
                    case SerializableTypeCode.Vector2Int:
                        {
                            Vector2Int v = new Vector2Int();
                            int n;
                            string[] parts = str.Split(',');
                            for (int i = 0; i < parts.Length; i++)
                            {
                                if (int.TryParse(parts[i], out n))
                                    v[i] = n;
                            }
                            value = v;
                        }
                        break;
                    case SerializableTypeCode.Vector3:
                        {
                            Vector3 v = new Vector3();
                            float n;
                            string[] parts = str.Split(',');
                            for (int i = 0; i < parts.Length; i++)
                            {
                                if (float.TryParse(parts[i], out n))
                                    v[i] = n;
                            }
                            value = v;
                        }
                        break;
                    case SerializableTypeCode.Vector3Int:
                        {
                            Vector3Int v = new Vector3Int();
                            int n;
                            string[] parts = str.Split(',');
                            for (int i = 0; i < parts.Length; i++)
                            {
                                if (int.TryParse(parts[i], out n))
                                    v[i] = n;
                            }
                            value = v;
                        }
                        break;
                    case SerializableTypeCode.Vector4:
                        {
                            Vector4 v = new Vector4();
                            float n;
                            string[] parts = str.Split(',');
                            for (int i = 0; i < parts.Length; i++)
                            {
                                if (float.TryParse(parts[i], out n))
                                    v[i] = n;
                            }
                            value = v;
                        }
                        break;
                    case SerializableTypeCode.Color:
                        {
                            Color v = new Color();
                            float n;
                            string[] parts = str.Split(',');
                            for (int i = 0; i < parts.Length; i++)
                            {
                                if (float.TryParse(parts[i], out n))
                                    v[i] = n;
                            }
                            value = v;
                        }
                        break;
                    case SerializableTypeCode.Color32:
                        {
                            Color32 v = new Color32();
                            byte n;
                            string[] parts = str.Split(',');
                            for (int i = 0; i < parts.Length; i++)
                            {
                                if (byte.TryParse(parts[i], out n))
                                {
                                    switch (i)
                                    {
                                        case 0:
                                            v.r = n;
                                            break;
                                        case 1:
                                            v.g = n;
                                            break;
                                        case 2:
                                            v.b = n;
                                            break;
                                        case 3:
                                            v.a = n;
                                            break;
                                    }
                                }
                            }
                            value = v;
                        }
                        break;
                    case SerializableTypeCode.Rect:
                        {
                            Rect v = new Rect();
                            if (!string.IsNullOrEmpty(str))
                            {
                                float n;
                                string[] parts = str.Split(',');
                                for (int i = 0; i < parts.Length; i++)
                                {
                                    if (float.TryParse(parts[i], out n))
                                    {
                                        switch (i)
                                        {
                                            case 0:
                                                v.x = n;
                                                break;
                                            case 1:
                                                v.y = n;
                                                break;
                                            case 2:
                                                v.width = n;
                                                break;
                                            case 3:
                                                v.height = n;
                                                break;
                                        }
                                    }
                                }
                            }
                            value = v;
                        }
                        break;
                    case SerializableTypeCode.RectInt:
                        {
                            RectInt v = new RectInt();
                            if (!string.IsNullOrEmpty(str))
                            {
                                int n;
                                string[] parts = str.Split(',');
                                for (int i = 0; i < parts.Length; i++)
                                {
                                    if (int.TryParse(parts[i], out n))
                                    {
                                        switch (i)
                                        {
                                            case 0:
                                                v.x = n;
                                                break;
                                            case 1:
                                                v.y = n;
                                                break;
                                            case 2:
                                                v.width = n;
                                                break;
                                            case 3:
                                                v.height = n;
                                                break;
                                        }
                                    }
                                }
                            }
                            value = v;
                        }
                        break;
                    case SerializableTypeCode.RectOffset:
                        {
                            RectOffset v;

                            int l = 0, r = 0, t = 0, b = 0;
                            if (!string.IsNullOrEmpty(str))
                            {
                                int n;
                                string[] parts = str.Split(',');
                                for (int i = 0; i < parts.Length; i++)
                                {
                                    if (int.TryParse(parts[i], out n))
                                    {
                                        switch (i)
                                        {
                                            case 0:
                                                l = n;
                                                break;
                                            case 1:
                                                r = n;
                                                break;
                                            case 2:
                                                t = n;
                                                break;
                                            case 3:
                                                b = n;
                                                break;
                                        }
                                    }
                                }
                            }
                            v = new RectOffset(l, r, t, b);
                            value = v;
                        }
                        break;
                    case SerializableTypeCode.RectOffsetSerializable:
                        {
                            RectOffsetSerializable v;

                            int l = 0, r = 0, t = 0, b = 0;
                            if (!string.IsNullOrEmpty(str))
                            {
                                int n;
                                string[] parts = str.Split(',');
                                for (int i = 0; i < parts.Length; i++)
                                {
                                    if (int.TryParse(parts[i], out n))
                                    {
                                        switch (i)
                                        {
                                            case 0:
                                                l = n;
                                                break;
                                            case 1:
                                                r = n;
                                                break;
                                            case 2:
                                                t = n;
                                                break;
                                            case 3:
                                                b = n;
                                                break;
                                        }
                                    }
                                }
                            }
                            v = new RectOffsetSerializable(l, r, t, b);
                            value = v;
                        }
                        break;
                    case SerializableTypeCode.AnimationCurve:
                        AnimationCurve curve;
                        if (!TryParseCurve(str, out curve))
                            curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
                        value = curve;
                        break;

                }

                return value;
            }

            public override string ToString()
            {
                //if (typeCode == SerializableTypeCode.Object)
                //    return objectValue == null ? "null" : objectValue.ToString();
                //else if (typeCode == SerializableTypeCode.AnimationCurve)
                //    return curveValue.ToString();

                //return stringValue;
                return value == null ? "null" : value.ToString();
            }

            static string ToString(float f)
            {
                return f.ToString("0.##");
            }
            static string ToCurveString(AnimationCurve curve)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("[");
                var keys = curve.keys;

                for (int i = 0, len = keys.Length; i < len; i++)
                {
                    var key = keys[i];

                    if (i > 0)
                        sb.Append(",");
                    sb.Append('[')
                        .Append(ToString(key.time))
                        .Append(',')
                        .Append(ToString(key.value))
                        .Append(',')
                        .Append(ToString(key.inTangent))
                        .Append(',')
                        .Append(ToString(key.outTangent))
                        .Append(',')
                        .Append(ToString(key.inWeight))
                        .Append(',')
                        .Append(ToString(key.outWeight))
                        .Append(']');
                }
                sb.Append("]");
                return sb.ToString();
            }

            static AnimationCurve ParseCurve(string str)
            {
                AnimationCurve curve;

                if (str == null)
                    throw new ArgumentNullException("str");

                str = str.Trim();

                if (str.Length > 0 && str[0] == '[')
                    str = str.Substring(1);
                if (str.Length > 0 && str[str.Length - 1] == ']')
                    str = str.Substring(0, str.Length - 1);

                float time, value, inTangent, outTangent, inWeight, outWeight;
                Keyframe keyframe;
                List<Keyframe> keys = new List<Keyframe>();
                foreach (var part in str.Split(']'))
                {
                    string tmp = part.Trim();
                    if (tmp.Length == 0)
                        continue;
                    if (tmp[0] == ',')
                        tmp = tmp.Substring(2);
                    else
                        tmp = tmp.Substring(1);

                    string[] arr = tmp.Split(',');

                    time = float.Parse(arr[0]);
                    value = float.Parse(arr[1]);

                    if (arr.Length > 2)
                        inTangent = float.Parse(arr[2]);
                    else
                        inTangent = 0f;
                    if (arr.Length > 3)
                        outTangent = float.Parse(arr[3]);
                    else
                        outTangent = 0f;
                    if (arr.Length > 4)
                        inWeight = float.Parse(arr[4]);
                    else
                        inWeight = 0f;
                    if (arr.Length > 5)
                        outWeight = float.Parse(arr[5]);
                    else
                        outWeight = 0f;
                    keyframe = new Keyframe(time, value, inTangent, outTangent, inWeight, outWeight);
                    keys.Add(keyframe);
                }

                curve = new AnimationCurve(keys.ToArray());
                return curve;
            }

            static bool TryParseCurve(string str, out AnimationCurve curve)
            {
                if (string.IsNullOrEmpty(str))
                {
                    curve = new AnimationCurve();
                    return false;
                }

                try
                {
                    curve = ParseCurve(str);
                    return true;
                }
                catch
                {
                    curve = new AnimationCurve();
                    return false;
                }
            }


            public enum SerializableTypeCode
            {
                None = 0,

                #region Base Types

                Object = 1,
                DBNull = 2,
                Boolean = 3,
                Char = 4,
                SByte = 5,
                Byte = 6,
                Int16 = 7,
                UInt16 = 8,
                Int32 = 9,
                UInt32 = 10,
                Int64 = 11,
                UInt64 = 12,
                Single = 13,
                Double = 14,
                Decimal = 15,
                DateTime = 16,
                String = 18,

                #endregion

                #region Array

                Array = 64,
                List = 128,


                #endregion


                #region Unity Types


                UnityObject = 32,
                Vector2 = 33,
                Vector2Int = 34,
                Vector3 = 35,
                Vector3Int = 36,
                Vector4 = 37,
                Color = 38,
                Color32 = 39,
                Rect = 40,
                RectInt = 41,
                Bounds = 42,
                BoundsInt = 43,
                AnimationCurve = 44,
                RectOffset = 45,
                RectOffsetSerializable = 46,

                #endregion
            }

        }



    }

    [Serializable]
    public class SerializableArray
    {
        [SerializeField]
        private SerializableObject[] value;

        public object[] Value
        {
            get { return ToArray<object>(); }
            set { SetValue(value); }
        }

        public void SetValue<T>(T[] value)
        {
            if (value != null)
                this.value = value.Select(o => new SerializableObject(o)).ToArray();
            else
                this.value = null;
        }

        public T[] ToArray<T>()
        {
            if (value == null || value.Length == 0)
                return new T[0];
            int length = value.Length;
            T[] array = new T[length];
            for (int i = 0; i < length; i++)
            {
                if (value[i] != null)
                    array[i] = (T)value[i].Value;
            }
            return array;
        }

        public List<T> ToList<T>()
        {
            if (value == null || value.Length == 0)
                return new List<T>(0);
            int length = value.Length;
            List<T> list = new List<T>(length);
            for (int i = 0; i < length; i++)
            {
                if (value[i] == null)
                    list.Add(default(T));
                else
                    list.Add((T)value[i].Value);
            }
            return list;
        }



    }
    /// <summary>
    /// RectOffset UnityException: set_left is not allowed to be called during serialization, call it from OnEnable instead.
    /// </summary>
    [Serializable]
    public class RectOffsetSerializable
    {
        [SerializeField]
        public int left;
        [SerializeField]
        public int right;
        [SerializeField]
        public int top;
        [SerializeField]
        public int bottom;

        public RectOffsetSerializable()
        {
        }
        public RectOffsetSerializable(int left, int right, int top, int bottom)
        {
            this.left = left;
            this.right = right;
            this.top = top;
            this.bottom = bottom;
        }

        public RectOffset ToRectOffset()
        {
            return new RectOffset(left, right, top, bottom);
        }
        public override string ToString()
        {
            return "" + left + "," + right + "," + top + "," + bottom;
        }
    }


}