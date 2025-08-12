using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Export.Attribute
{
    /// <summary>
    /// 高级的绘制器系统，能够协调多个属性绘制器的执行顺序
    /// </summary>
    public abstract class PropertyDrawerEX : PropertyDrawer
    {
        private static readonly Dictionary<Type, PropertyDrawer> _drawerCache =
            new Dictionary<Type, PropertyDrawer>();
        private static readonly Dictionary<string, FieldInfo> _fieldInfoCache =
            new Dictionary<string, FieldInfo>();
        private static readonly Dictionary<Type, Type> _drawerTypeCache =
            new Dictionary<Type, Type>();

        static PropertyDrawerEX()
        {
            // 预加载常用drawer类型
            var drawerTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(PropertyDrawer).IsAssignableFrom(t));

            foreach (var type in drawerTypes)
            {
                var attrType = type.GetCustomAttribute<CustomPropertyDrawer>()?.GetType();
                if (attrType != null)
                {
                    _drawerTypeCache[attrType] = type;
                }
            }
        }

        protected PropertyDrawer GetNextDrawer(SerializedProperty property, GUIContent label)
        {
            var fieldInfo = GetCachedFieldInfo(property);
            if (fieldInfo == null) return null;

            var attributes = fieldInfo.GetCustomAttributes(false);
            foreach (var attr in attributes)
            {
                if (attr == attribute) continue;

                if (!_drawerCache.TryGetValue(attr.GetType(), out var drawer))
                {
                    drawer = CreateDrawerInstance(attr, fieldInfo);
                    if (drawer != null) _drawerCache[attr.GetType()] = drawer;
                }
                return drawer;
            }
            return null;
        }

        private FieldInfo GetCachedFieldInfo(SerializedProperty property)
        {
            string cacheKey = $"{property.serializedObject.targetObject.GetType().FullName}.{property.propertyPath}";
            if (!_fieldInfoCache.TryGetValue(cacheKey, out var fieldInfo))
            {
                fieldInfo = GetFieldInfo(property);
                if (fieldInfo != null) _fieldInfoCache[cacheKey] = fieldInfo;
            }
            return fieldInfo;
        }

        private FieldInfo GetFieldInfo(SerializedProperty property)
        {
            string cacheKey = $"{property.serializedObject.targetObject.GetType().FullName}.{property.propertyPath}";
            if (!_fieldInfoCache.TryGetValue(cacheKey, out var fieldInfo))
            {
                fieldInfo = GetFieldInfoRecursive(
                    property.serializedObject.targetObject.GetType(),
                    property.propertyPath);

                if (fieldInfo != null)
                    _fieldInfoCache[cacheKey] = fieldInfo;
            }
            return fieldInfo;
        }

        private FieldInfo GetFieldInfoRecursive(Type type, string path)
        {
            FieldInfo field = null;
            string[] parts = path.Split('.');

            foreach (string part in parts)
            {
                field = type.GetField(part,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

                if (field == null) return null;

                type = field.FieldType;

                // 处理数组/列表情况
                if (part == "Array" && type.IsArray)
                {
                    type = type.GetElementType();
                }
                else if (type.IsGenericType &&
                        type.GetGenericTypeDefinition() == typeof(List<>))
                {
                    type = type.GetGenericArguments()[0];
                }
            }
            return field;
        }

        private PropertyDrawer CreateDrawerInstance(object attr, FieldInfo fieldInfo)
        {
            if (!_drawerTypeCache.TryGetValue(attr.GetType(), out var drawerType))
                return null;

            var drawer = (PropertyDrawer)Activator.CreateInstance(drawerType);

            // 使用预编译的委托设置字段
            PropertyDrawerHelper.SetFieldInfo(drawer, fieldInfo);
            PropertyDrawerHelper.SetAttribute(drawer, attr);

            return drawer;
        }
    }

    public static class PropertyDrawerHelper
    {
        private static readonly Action<PropertyDrawer, FieldInfo> _setFieldInfo;
        private static readonly Action<PropertyDrawer, object> _setAttribute;

        static PropertyDrawerHelper()
        {
            _setFieldInfo = CreateFieldSetter<FieldInfo>("m_FieldInfo");
            _setAttribute = CreateFieldSetter<object>("m_Attribute");
        }

        private static Action<PropertyDrawer, T> CreateFieldSetter<T>(string fieldName)
        {
            var param = Expression.Parameter(typeof(PropertyDrawer));
            var valueParam = Expression.Parameter(typeof(T));
            var field = Expression.Field(param, fieldName);
            var assign = Expression.Assign(field, valueParam);
            return Expression.Lambda<Action<PropertyDrawer, T>>(assign, param, valueParam).Compile();
        }

        public static void SetFieldInfo(PropertyDrawer drawer, FieldInfo fieldInfo)
            => _setFieldInfo(drawer, fieldInfo);

        public static void SetAttribute(PropertyDrawer drawer, object attribute)
            => _setAttribute(drawer, attribute);
    }
}
