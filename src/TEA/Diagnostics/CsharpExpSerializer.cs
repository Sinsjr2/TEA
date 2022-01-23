using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace TEA.Diagnostics {

    /// <summary>
    /// C#の構文でインスタンス化するようの文字列を返します。
    /// IUpdateのテストを作成するために使用します。
    /// </summary>
    public static class CsharpExpSerializer {

        /// <summary>
        /// 一致する型が無い場合はnullを返します。
        /// 引数がnullの場合は"null"を返します。
        /// </summary>
        static string? GetExpForBuiltInType(object? obj) {
            return obj switch {
                bool b => b ? "true" : "false",
                byte b => b.ToString(),
                sbyte s => s.ToString(),
                char c => '\'' + c.ToString() + '\'',
                decimal d => d.ToString(),
                double d => d.ToString(CultureInfo.InvariantCulture),
                float f => f.ToString(CultureInfo.InvariantCulture) + "f",
                int i => i.ToString(),
                uint ui => ui.ToString() + "u",
                nint ni => ni.ToString(),
                nuint nui => nui.ToString() + "u",
                long l => l.ToString() + "L",
                ulong ul => ul.ToString() + "L",
                short s => s.ToString(),
                ushort us => us.ToString(),
                string str => "\"" + str + "\"",
                null => "null",
                _ => null
            };
        }

        /// <summary>
        /// リスト用の初期化処理をの文字列を返します。
        /// </summary>
        static string? GetExpForCollection(object obj) {
            static IEnumerable<string> GetExps(object o) {
                return ((IEnumerable)o).Cast<object>().Select(x => GetExp(x));
            }
            var type = obj.GetType();
            return obj switch {
                var xs when type.IsArray => GetExps(obj).ArrayLiteral(),
                var list when type.IsGenericType &&
                    type.GetGenericTypeDefinition() == typeof(List<>) => GetExps(obj).ListLiteral(type.GetGenericArguments()[0].TypeFullName()),
                _ => null
            };
        }

        static string TypeFullName(this Type type) {
            return type.FullName?.Replace('+', '.') ?? "";
        }

        static string ArrayLiteral(this IEnumerable<string> xs) {
            var e = string.Join(", ", xs);
            return $"new[] {{ {e} }}";
        }

        static string ListLiteral(this IEnumerable<string> xs, string genericType ) {
            var e = string.Join(", ", xs);
            return $"new List<{genericType}> {{ {e} }}";
        }

        /// <summary>
        /// 初期化式の作り方が不明なオブジェクトの式を人が分かる形で出力します。
        /// </summary>
        static string UnknownExp(this object obj) {
            return "/* " + obj.ToString() + " */";
        }

        /// <summary>
        /// getプロパティ フィールドの順番でメンバの値を作る式の文字列を返します。
        /// 見つからなければnullを返します。
        /// </summary>
        static string? TryGetMemberValueExp(object obj, string memberName) {
            var type = obj.GetType();
            var propInfo = type.GetProperty(memberName,
                                            BindingFlags.GetProperty |
                                            BindingFlags.Instance |
                                            BindingFlags.Public);
            if (propInfo is not null) {
                return GetExp(propInfo.GetValue(obj));
            }

            var fieldInfo = type.GetField(memberName,
                                          BindingFlags.Instance |
                                          BindingFlags.Public);
            return fieldInfo is null
                ? null
                : GetExp(fieldInfo.GetValue(obj));
        }

        /// <summary>
        /// 見つからなければnullを返します。
        /// </summary>
        static string? GetExpForConstructor(object obj) {
            var type = obj.GetType();
            var constructors = type.GetConstructors();

            var constructorParam =
                (from c in constructors
                 where c.IsPublic
                 let paramExp = (from param in c.GetParameters()
                                 select param.Name is null ? null : new { exp = TryGetMemberValueExp(obj, param.Name) , name = param.Name })
                 .ToArray()
                 where !paramExp.Any(x => x is null)
                 select paramExp)
                // とりあえず初めに発見したコンストラクタを使用する
                .FirstOrDefault();
            if (!type.IsValueType && constructorParam is null || type.FullName is null) {
                return null;
            }

            var t = constructorParam is not null
                ? ( exp : NewExp(type.TypeFullName(), constructorParam.Select(x => x.exp)),
                    setterExp: GetSetterPropertyExp(obj, constructorParam.Select(x => x.name)))
                : (exp : NewExp(type.TypeFullName(), Array.Empty<string>()),
                   setterExp : GetSetterPropertyExp(obj, Array.Empty<string>()));
            return t.exp + (t.setterExp == "" ? "" : " " + t.setterExp);
        }

        static string NewExp(string typeName, IEnumerable<string> parameters) {
            var paramExp = string.Join(", ", parameters);
            return $"new {typeName}({paramExp})";
        }

        /// <summary>
        /// セッターのプロパティの初期化式を返します。
        /// コンストラクタの引数で指定した値を再度代入しないように名前を指定している。
        /// 初期化する値が無い場合は空文字を返します。
        /// ex: { Hoge = "foo", ... }
        /// </summary>
        static string GetSetterPropertyExp(this object obj, IEnumerable<string> excludeMembers) {
            var type = obj.GetType();
            var members = new HashSet<string>(excludeMembers);

            var props = from prop in type.GetProperties(BindingFlags.GetProperty |
                                                        BindingFlags.Instance |
                                                        BindingFlags.Public)
                where !members.Contains(prop.Name) &&
                      prop.GetAccessors().Any(x => x.ReturnType == typeof(void) && x.IsPublic)
                select SetterExp(prop.Name, GetExp(prop.GetValue(obj)));

            var fields = from prop in type.GetFields(BindingFlags.Instance |
                                                     BindingFlags.Public |
                                                     BindingFlags.Instance)
                where !members.Contains(prop.Name) && prop.IsPublic
                select SetterExp(prop.Name, GetExp(prop.GetValue(obj)));

            var exp = string.Join(", ", props.Concat(fields));
            return exp == "" ? "" : "{ " + exp  + " }";
        }

        static string SetterExp(string varName, string value) {
            return $"{varName} = {value}";
        }

        public static string GetExp(this object? obj) {
            return GetExpForBuiltInType(obj) ??
                GetExpForCollection(obj!) ??
                GetExpForConstructor(obj!) ??
                UnknownExp(obj!);
        }
    }
}
