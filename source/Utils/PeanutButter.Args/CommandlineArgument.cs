using System.Reflection;
using PeanutButter.Utils;

namespace PeanutButter.Args
{
    public class CommandlineArgument
    {
        public string LongName { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }

        public string Type =>
            _type ??= GrokType();

        private string GrokType()
        {
            if (Property is null)
            {
                return "";
            }

            if (Property.PropertyType == typeof(bool) ||
                Property.PropertyType == typeof(bool?))
            {
                return "flag";
            }

            return Property.PropertyType.IsNumericType()
                ? "number"
                : "text";
        }

        private string _type;
        public PropertyInfo Property { get; set; }

        public string Key
        {
            get => _explicitKey ?? Property.Name;
            set => _explicitKey = value;
        }

        private string _explicitKey;
        public object Default { get; set; }
        public bool IsImplicit { get; set; }
        public string[] ConflictsWith { get; set; }

        public bool AllowMultipleValues
            => _allowMultipleValues ??= Property?.PropertyType?.IsCollection() ?? false;

        public bool IsFlag
        {
            get => _explicitFlag ?? (
                Property.PropertyType == typeof(bool) ||
                Property.PropertyType == typeof(bool?)
            );
            set => _explicitFlag = true;
        }

        public const string HELP_FLAG_KEY = "$help$";
        public bool IsHelpFlag => Key == HELP_FLAG_KEY;

        private bool? _explicitFlag;

        private bool? _allowMultipleValues;

        public CommandlineArgument Negate()
        {
            var result = new CommandlineArgument();
            this.CopyPropertiesTo(result, deep: false);
            result.LongName = $"no-{result.LongName}";
            result.ConflictsWith = new[] { Key };
            try
            {
                result.Default = !(bool) Default;
            }
            catch
            {
                result.Default = false;
            }

            return result;
        }
    }
}