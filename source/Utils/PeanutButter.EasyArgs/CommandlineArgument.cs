using System.Reflection;
using PeanutButter.Utils;

namespace PeanutButter.EasyArgs
{
    /// <summary>
    /// Describes a command line argument
    /// </summary>
    public class CommandlineArgument
    {
        /// <summary>
        /// The long name of the argument (eg 'remote-server')
        /// </summary>
        public string LongName { get; set; }
        /// <summary>
        /// The short name of the argument (eg 'r'), constrained to
        /// one char by the [ShortName] attribute
        /// </summary>
        public string ShortName { get; set; }
        /// <summary>
        /// The description of the argument which is displayed in
        /// help text
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Provides a convenience reader for the --{LongName} switch
        /// </summary>
        public string LongSwitch => LongName is null ? null : $"--{LongName}";
        /// <summary>
        /// Provides a convenience reader for the -{ShortName} switch
        /// </summary>
        public string ShortSwitch => ShortName is null ? null : $"-{ShortName}";

        /// <summary>
        /// The type of argument for help display (typically "text" or "number")
        /// </summary>
        public string Type =>
            _type ??= GrokType();

        /// <summary>
        /// Minimum value for a numeric argument
        /// </summary>
        public decimal? MinValue { get; set; }
        
        /// <summary>
        /// Maximum value for a numeric argument
        /// </summary>
        public decimal? MaxValue { get; set; }

        /// <summary>
        /// shortcut to determine if the switch is either the long or short
        /// switch for this arg
        /// </summary>
        /// <param name="sw"></param>
        /// <returns></returns>
        public bool HasSwitch(string sw)
        {
            return LongSwitch == sw || ShortSwitch == sw;
        }

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
        /// <summary>
        /// The corresponding PropertyInfo for this argument
        /// </summary>
        public PropertyInfo Property { get; set; }

        /// <summary>
        /// The fully-qualified name of the argument
        /// - Property.Name when available
        /// - $help$ for the generated help argument
        /// </summary>
        public string Key
        {
            get => _explicitKey ?? Property.Name;
            set => _explicitKey = value;
        }

        private string _explicitKey;
        /// <summary>
        /// Default value for this argument
        /// </summary>
        public object Default { get; set; }
        /// <summary>
        /// Whether this argument was generated (eg negations / help)
        /// </summary>
        public bool IsImplicit { get; set; }
        /// <summary>
        /// Collection of arguments this conflicts with, by Key
        /// </summary>
        public string[] ConflictsWithKeys { get; set; } = new string[0];

        /// <summary>
        /// Whether this argument allows multiple values (and will consume all
        /// non-switch values up to the next switch, eg "-foo 1 2 3" would collect
        /// the array [ 1, 2, 3 ] if the Foo property is int[]
        /// </summary>
        public bool AllowMultipleValues
            => _allowMultipleValues ??= Property?.PropertyType?.IsCollection() ?? false;

        /// <summary>
        /// Whether or not this argument is a flag
        /// </summary>
        public bool IsFlag
        {
            get => _explicitFlag ?? (
                Property.PropertyType == typeof(bool) ||
                Property.PropertyType == typeof(bool?)
            );
            set => _explicitFlag = true;
        }

        /// <summary>
        /// Explicit key for the help argument
        /// </summary>
        public const string HELP_FLAG_KEY = "$help$";
        /// <summary>
        /// Whether or not this specific argument is the help one
        /// </summary>
        public bool IsHelpFlag => Key == HELP_FLAG_KEY;
        /// <summary>
        /// Whether or not this argument is required
        /// </summary>
        public bool IsRequired { get; set; }

        private bool? _explicitFlag;

        private bool? _allowMultipleValues;

        /// <summary>
        /// Produces a copy of the current argument, negated
        /// Used when generating --no-{arg} flags
        /// </summary>
        /// <returns></returns>
        public CommandlineArgument Negate()
        {
            var result = new CommandlineArgument();
            this.CopyPropertiesTo(result, deep: false);
            result.ShortName = null;
            result.LongName = $"no-{result.LongName}";
            result.ConflictsWithKeys = new[] { Key };
            
            result.IsRequired = false;
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