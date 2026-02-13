using Export.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

using Random = System.Random;

namespace Export.Attribute
{
    /// <summary>
    /// 高级的绘制器系统，能够协调多个属性绘制器的执行顺序
    /// </summary>
    public abstract class PropertyDrawerEX : PropertyDrawer
    {
        /// <summary>
        /// UUID
        /// </summary>
        private readonly string uuid;
        private static readonly Random r;
        private static readonly Dictionary<PropertyAttribute, PropertyDrawer> _drawerCache =
            new Dictionary<PropertyAttribute, PropertyDrawer>();
        private static readonly Dictionary<string, FieldInfo> _fieldInfoCache =
            new Dictionary<string, FieldInfo>();
        private static readonly Dictionary<Type, Type> _drawerTypeCache =
            new Dictionary<Type, Type>();

        public PropertyDrawerEX()
        {
            uuid = Item.NewUUID(r.NextDouble().ToString());
        }

        static PropertyDrawerEX()
        {
            // 生成随机数生成器
            r = new Random();

            // 预加载常用drawer类型
            var drawerTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(PropertyDrawer).IsAssignableFrom(t));

            // ======== 新增：注册Unity内置绘制器 ========
            var editorAssembly = typeof(UnityEditor.Editor).Assembly;
            var exportAssembly = Assembly.GetExecutingAssembly();
            RegisterBuiltinDrawer(editorAssembly, "UnityEditor.RangeDrawer", typeof(RangeAttribute));
            RegisterBuiltinDrawer(exportAssembly, "Export.Attribute.ReadOnlyAttributeDrawer", typeof(ReadOnlyAttribute));
            RegisterBuiltinDrawer(exportAssembly, "Export.Attribute.ConditionalHideDrawer", typeof(ReadOnlyAttribute));
            RegisterBuiltinDrawer(exportAssembly, "Export.Attribute.ConditionalShowDrawer", typeof(ReadOnlyAttribute));
            //RegisterBuiltinDrawer(editorAssembly, "UnityEditor.MinMaxDrawer", typeof(MinMaxAttribute));

            foreach (var type in drawerTypes)
            {
                var attrs = type.GetCustomAttributes<CustomPropertyDrawer>(false);
                foreach (var attr in attrs)
                {
                    var attrType = attr?.GetType();
                    if (attrType != null && !_drawerTypeCache.ContainsKey(attrType))
                    {
                        _drawerTypeCache[attrType] = type;
                    }
                }
            }
        }

        private static void RegisterBuiltinDrawer(Assembly editorAssembly, string drawerClassName, Type attributeType)
        {
            var drawerType = editorAssembly.GetType(drawerClassName);
            if (drawerType != null && !_drawerTypeCache.ContainsKey(attributeType))
            {
                _drawerTypeCache[attributeType] = drawerType;
            }
        }

        protected PropertyDrawer GetNextDrawer(SerializedProperty property, GUIContent label)
        {
            var fieldInfo = GetCachedFieldInfo(property);
            if (fieldInfo == null) return null;

            // 获取所有 PropertyAttribute 并按声明顺序排序
            var attributes = fieldInfo.GetCustomAttributes(typeof(PropertyAttribute), false)
                .Cast<PropertyAttribute>()
                .Where(attr => attr != attribute)  // 排除自身属性
                .OrderBy(attr => attr.GetType().Name) // 按类型名排序确保顺序一致
                .ToList();

            //var message = new StringBuilder();
            //message.Append("GetNextDrawer : " + attributes.Count + " : " + fieldInfo.Name + " : " + fieldInfo.DeclaringType.FullName + "\n");
            //message.Append("Attributes: " + string.Join(", ", attributes.Select(attr => attr.GetType().Name)) + "\n");
            //Debug.Log(message.ToString());

            // 尝试找到并返回第一个有效的绘制器
            foreach (var attr in attributes)
            {
                if (!_drawerCache.TryGetValue(attr, out var drawer))
                {
                    drawer = CreateDrawerInstance(attr, fieldInfo);
                    if (drawer != null)
                    {
                        _drawerCache[attr] = drawer;
                        return drawer; // 找到第一个有效绘制器即返回
                    }
                }
                else if (drawer != null)
                {
                    return drawer;
                }
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
            // ======== 修复：处理泛型列表和数组 ========
            if (type.IsGenericType && type.GetInterface("IList") != null)
            {
                type = type.GetGenericArguments()[0];
            }
            else if (type.IsArray)
            {
                type = type.GetElementType();
            }

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

        private PropertyDrawer CreateDrawerInstance(PropertyAttribute attr, FieldInfo fieldInfo)
        {
            if (!_drawerTypeCache.TryGetValue(attr.GetType(), out var drawerType))
                return null;

            try
            {
                var drawer = (PropertyDrawer)Activator.CreateInstance(drawerType);
                PropertyDrawerHelper.SetFieldInfo(drawer, fieldInfo);
                PropertyDrawerHelper.SetAttribute(drawer, attr);
                return drawer;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to create drawer for {attr.GetType()}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Unity编辑器脚本重载时清理缓存
        /// </summary>
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            _drawerCache.Clear();
            _fieldInfoCache.Clear();
        }
    }

    public static class PropertyDrawerHelper
    {
        private static readonly Action<PropertyDrawer, FieldInfo> _setFieldInfo;
        private static readonly Action<PropertyDrawer, PropertyAttribute> _setAttribute;

        static PropertyDrawerHelper()
        {
            _setFieldInfo = CreateFieldSetter<FieldInfo>("m_FieldInfo");
            _setAttribute = CreateFieldSetter<PropertyAttribute>("m_Attribute");
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
        {
            // 添加安全类型转换
            if (attribute is PropertyAttribute propAttr)
            {
                _setAttribute(drawer, propAttr);
            }
            else
            {
                Debug.LogError($"Invalid attribute type: {attribute?.GetType()}, expected PropertyAttribute");
            }
        }
    }
}
