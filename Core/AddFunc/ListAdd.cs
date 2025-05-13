using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Export.AddFunc
{
    /// <summary>
    /// 列表类型追加功能
    /// </summary>
    public static class ListAdd
    {
        /// <summary>
        /// 获取列表长度
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static int Length<T>(this List<T> list)
        {
            return list.Count;
        }

        /// <summary>
        /// 将列表转为字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objects"></param>
        /// <param name="tf"></param>
        /// <returns></returns>
        public static string ToString<T>(this T[] objects, bool tf)
        {
            if (tf)
            {
                var ret = typeof(T).ToString();
                ret += "[" + objects.Length + "] { ";
                for (int i = 0; i < objects.Length; i++)
                {
                    ret += objects[i].ToString();
                    if (i < objects.Length - 1)
                    {
                        ret += ", ";
                    }
                }
                ret += " }";
                return ret;
            }
            else
            {
                return objects.ToString();
            }
        }

        /// <summary>
        /// 将列表转为字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts"></param>
        /// <param name="tf"></param>
        /// <returns></returns>
        public static string ToString<T>(this List<T> ts, bool tf)
        {
            if (tf == false)
            {
                return ts.ToString();
            }
            else
            {
                var ret = "List<" + typeof(T).ToString() + "> ";
                ret += "[" + ts.Count + "] { ";
                for (int i = 0; i < ts.Count; i++)
                {
                    ret += ts[i].ToString();
                    if (i < ts.Count - 1)
                    {
                        ret += ", ";
                    }
                }
                ret += " }";
                return ret;
            }
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts"></param>
        /// <param name="add"></param>
        /// <returns></returns>
        public static string Join<T>(this List<T> ts, string add)
        {
            var ret = new StringBuilder();
            for (int i = 0; i < ts.Count - 1; i++)
            {
                ret.Append(ts[i]);
                ret.Append(add);
            }
            ret.Append(ts[ts.Count - 1]);
            return ret.ToString();
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts"></param>
        /// <param name="add"></param>
        /// <returns></returns>
        public static string Join<T>(this T[] ts, string add)
        {
            var ret = new StringBuilder();
            for (int i = 0; i < ts.Length - 1; i++)
            {
                ret.Append(ts[i]);
                ret.Append(add);
            }
            ret.Append(ts[ts.Length - 1]);
            return ret.ToString();
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts"></param>
        /// <param name="add"></param>
        /// <returns></returns>
        public static string Join<T>(this IEnumerator<T> ts, string add)
        {
            if (ts == null || !ts.MoveNext())
            {
                // 如果枚举器为null或没有元素，返回空字符串
                return string.Empty;
            }

            var ret = new StringBuilder();
            ret.Append(ts.Current); // 先添加第一个元素

            while (ts.MoveNext()) // 检查是否有更多元素
            {
                ret.Append(add); // 先添加分隔符
                ret.Append(ts.Current); // 再添加元素
            }

            return ret.ToString();
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts"></param>
        /// <param name="add"></param>
        /// <returns></returns>
        public static string Join<T>(this IEnumerable<T> ts, string add)
        {
            var ret = new StringBuilder();
            using (var enumerator = ts.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                {
                    return string.Empty;  // 处理空集合
                }

                ret.Append(enumerator.Current);  // 添加第一个元素，避免在它之前添加分隔符

                while (enumerator.MoveNext())
                {
                    ret.Append(add);  // 在元素之间添加分隔符
                    ret.Append(enumerator.Current);
                }
            }

            return ret.ToString();
        }

        /// <summary>
        /// 将列表平铺
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<T> Join<T>(this List<List<T>> list)
        {
            var ret = new List<T>();
            list.ForEach(l =>
            {
                ret.AddRange(l);
            });
            return ret;
        }

        /// <summary>
        /// 将列表进行转换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="ts"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static List<R> Amplify<T, R>(this List<T> ts, Func<T, R> func)
        {
            var ret = new List<R>();

            ts.ForEach(t =>
            {
                ret.Add(func(t));
            });

            return ret;
        }

        /// <summary>
        /// 将列表遍历转换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="ts"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static List<R> Map<T, R>(this List<T> ts, Func<T, R> func)
        {
            var ret = new List<R>();
            ts.ForEach((t) =>
            {
                ret.Add(func.Invoke(t));
            });
            return ret;
        }

        /// <summary>
        /// 将列表遍历转换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="ts"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static List<R> Map<T,R>(this List<T> ts,Func<T,int,R> func)
        {
            var ret = new List<R>();
            ts.ForEach((t, i) =>
            {
                ret.Add(func.Invoke(t, i));
            });
            return ret;
        }

        /// <summary>
        /// 对List的每个元素进行指定操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts"></param>
        /// <param name="action"></param>
        public static void ForEach<T>(this List<T> ts, Action<T, int> action)
        {
            for (int i = 0; i < ts.Count; i++)
            {
                action.Invoke(ts[i], i);
            }
        }

        /// <summary>
        /// 反转当前这个列表顺序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts"></param>
        /// <returns></returns>
        public static List<T> ReverseThis<T>(this List<T> ts)
        {
            ts.Reverse();
            return ts;
        }

        /// <summary>
        /// 转为字典
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts"></param>
        /// <returns></returns>
        public static Dictionary<int,T> ToDict<T>(this List<T> ts)
        {
            var ret = new Dictionary<int, T>();
            ts.ForEach((t, i) =>
            {
                ret.Add(i, t);
            });
            return ret;
        }

        /// <summary>
        /// 转为字典
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts"></param>
        /// <param name="getKey">获取Key方法</param>
        /// <returns></returns>
        public static Dictionary<TKey,T> ToDict<TKey,T>(this List<T> ts,Func<T,TKey> getKey)
        {
            var ret = new Dictionary<TKey, T>();
            ts.ForEach(t =>
            {
                ret.Add(getKey(t), t);
            });
            return ret;
        }

        /// <summary>
        /// 对列表进行过滤，根据提供的条件函数选择元素。
        /// </summary>
        /// <typeparam name="T">列表中元素的类型。</typeparam>
        /// <param name="list">要进行过滤的源列表。</param>
        /// <param name="isFilter">过滤函数，它接受列表中的元素（类型为 T）和元素的索引（int），并返回一个布尔值，指示是否应该包含当前元素。</param>
        /// <returns>一个新的列表，仅包含满足过滤条件的元素。</returns>
        public static List<T> Filter<T>(this List<T> list, Func<T, int, bool> isFilter)
        {
            var ret = new List<T>();
            for (int i = 0; i < list.Length(); i++)
            {
                var t = list[i];
                var r = isFilter(t, i);
                if (r)
                {
                    ret.Add(t);
                }
            }
            return ret;
        }

        /// <summary>
        /// 从泛型列表中获取第一个符合特定条件的元素。这些条件包括不是 null、不是空字符串、
        /// 对于数值类型不是 0、不是 NaN、不是无穷大。
        /// </summary>
        /// <typeparam name="T">列表中元素的类型。</typeparam>
        /// <param name="list">要检查的源列表。</param>
        /// <returns>
        /// 返回列表中的第一个符合条件的元素。如果没有找到符合条件的元素，返回类型 T 的默认值。
        /// 对于数值类型，检查元素是否为 0、NaN 或无穷大；对于字符串，检查是否为空字符串。
        /// </returns>
        public static T GetOnlyOne<T>(this List<T> list)
        {
            foreach (var item in list)
            {
                if (item == null) continue;

                if (item is string str && string.IsNullOrEmpty(str)) continue;

                if (item is IConvertible convertible)
                {
                    TypeCode typeCode = convertible.GetTypeCode();
                    switch (typeCode)
                    {
                        case TypeCode.Double:
                        case TypeCode.Single:
                            double d = convertible.ToDouble(null);
                            if (double.IsNaN(d) || double.IsInfinity(d)) continue;
                            break;
                        case TypeCode.Decimal:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Int16:
                        case TypeCode.Byte:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                        case TypeCode.UInt16:
                        case TypeCode.SByte:
                            if (convertible.ToInt64(null) == 0) continue;
                            break;
                            // 您可以根据需要添加更多的类型处理
                    }
                }

                return item; // 返回符合条件的第一个元素
            }

            return default(T); // 如果没有找到符合条件的元素，则返回默认值
        }

        /// <summary>
        /// 生成列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static List<int> Range<T>(this List<T> list, int end)
        {
            var ret = new List<int>();
            for (int i = 0; i < end; i++)
            {
                ret.Add(i);
            }
            return ret;
        }

        /// <summary>
        /// 生成列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static List<int> Range<T>(this List<T> list, int start, int end)
        {
            var ret = new List<int>();
            for (int i = start; i < end; i++)
            {
                ret.Add(i);
            }
            return ret;
        }

        /// <summary>
        /// 生成列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public static List<int> Range<T>(this List<T> list, int start, int end, int step)
        {
            var ret = new List<int>();
            for (int i = start; i < end; i += step)
            {
                ret.Add(i);
            }
            return ret;
        }
    }
}
