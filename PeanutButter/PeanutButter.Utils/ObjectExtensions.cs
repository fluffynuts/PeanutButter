using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeanutButter.Utils
{
    public static class ObjectExtensions
    {
        private static Type[] _simpleTypes;

        static ObjectExtensions()
        {
            _simpleTypes = new[] {
                typeof(int),
                typeof(char),
                typeof(byte),
                typeof(long),
                typeof(string),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(bool),
                typeof(DateTime)
            };
        }
        public static bool AllPropertiesMatch(this object objSource, object objCompare)
        {
            if (objSource == null && objCompare == null) return true;
            if (objSource == null || objCompare == null) return false;
            var srcPropInfos = objSource.GetType().GetProperties();
            var comparePropInfos = objCompare.GetType().GetProperties();
            foreach (var srcProp in srcPropInfos)
            {
                var comparePropInfo = comparePropInfos.FirstOrDefault(pi => pi.Name == srcProp.Name);
                if (comparePropInfo == null)
                {
                    Debug.WriteLine("Unable to find comparison property with name: '" + srcProp.Name + "'");
                    return false;
                }
                if (comparePropInfo.PropertyType != srcProp.PropertyType)
                {
                    Debug.WriteLine("Source property has type '" + srcProp.PropertyType.Name + "' but comparison property has type '" + comparePropInfo.PropertyType.Name + "'");
                    return false;
                }
                var srcValue = srcProp.GetValue(objSource);
                var compareValue = comparePropInfo.GetValue(objCompare);
                if (_simpleTypes.Any(st => st == srcProp.PropertyType))
                {
                    var srcString = StringOf(srcValue);
                    var compareString = StringOf(compareValue);
                    if (srcValue.ToString() != compareValue.ToString())
                    {
                        Debug.WriteLine(srcProp.Name + " value mismatch: (" + srcString + ") vs (" + compareString + ")");
                        return false;
                    }
                    continue;
                }
                if (!srcValue.AllPropertiesMatch(compareValue))
                    return false;
            }
            return true;
        }

        public static void CopyPropertiesTo(this object src, object dst, bool deep = true)
        {
            if (src == null || dst == null) return;
            var srcPropInfos = src.GetType().GetProperties();
            var dstPropInfos = dst.GetType().GetProperties();
            foreach (var srcPropInfo in srcPropInfos)
            {
                if (!srcPropInfo.CanRead) continue;
                var matchingTarget = dstPropInfos.FirstOrDefault(dp => dp.Name == srcPropInfo.Name && dp.PropertyType == srcPropInfo.PropertyType);
                if (matchingTarget == null) continue;
                if (!matchingTarget.CanWrite) continue;
                var srcVal = srcPropInfo.GetValue(src);
                if (!deep || _simpleTypes.Any(si => si == srcPropInfo.PropertyType))
                {
                    matchingTarget.SetValue(dst, srcVal);
                }
                else
                {
                    if (srcVal != null)
                    {
                        var targetVal = matchingTarget.GetValue(dst);
                        srcVal.CopyPropertiesTo(targetVal);
                    }
                    else
                    {
                        matchingTarget.SetValue(dst, null);
                    }
                }
            }
        }

        private static string StringOf(object srcValue)
        {
            return srcValue == null ? "[null]" : srcValue.ToString();
        }
    }
}
