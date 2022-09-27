using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using static PeanutButter.RandomGenerators.RandomValueGen;

// ReSharper disable InconsistentNaming

namespace PeanutButter.TestUtils.AspNetCore.Fakes
{
    /// <summary>
    /// Interface for dealing with the backing properties of the FakeModelMetadata
    /// </summary>
    public interface IFakeModelMetadata
    {
        /// <summary>
        /// Sets the AdditionalValues collection
        /// </summary>
        IReadOnlyDictionary<object, object> _AdditionalValues { get; set; }

        /// <summary>
        /// Sets the ModelPropertyCollection
        /// </summary>
        ModelPropertyCollection _Properties { get; set; }

        /// <summary>
        /// Sets the BinderModelName
        /// </summary>
        string _BinderModelName { get; set; }

        /// <summary>
        /// Sets the Binder Type
        /// </summary>
        Type _BinderType { get; set; }

        /// <summary>
        /// Sets the BindingSource
        /// </summary>
        BindingSource _BindingSource { get; set; }

        /// <summary>
        /// Sets the ConvertEmptyStringToNull flag
        /// </summary>
        bool _ConvertEmptyStringToNull { get; set; }

        /// <summary>
        /// Sets the DataTimeName
        /// </summary>
        string _DataTypeName { get; set; }

        /// <summary>
        /// Sets the description
        /// </summary>
        string _Description { get; set; }

        /// <summary>
        /// Sets the DisplayFormatString
        /// </summary>
        string _DisplayFormatString { get; set; }

        /// <summary>
        /// Sets the DisplayName
        /// </summary>
        string _DisplayName { get; set; }

        /// <summary>
        /// Sets the EditFormatString
        /// </summary>
        string _EditFormatString { get; set; }

        /// <summary>
        /// Sets the Element Metadata
        /// </summary>
        ModelMetadata _ElementMetadata { get; set; }

        /// <summary>
        /// Sets the ENumGroupedDisplayNamesAndValues
        /// </summary>
        IEnumerable<KeyValuePair<EnumGroupAndName, string>> _EnumGroupedDisplayNamesAndValues { get; set; }

        /// <summary>
        /// Sets the EnumNamesAndValues
        /// </summary>
        IReadOnlyDictionary<string, string> _EnumNamesAndValues { get; set; }

        /// <summary>
        /// Sets the HasNonDefaultEditFormat flag
        /// </summary>
        bool _HasNonDefaultEditFormat { get; set; }

        /// <summary>
        /// Sets the HtmlEncode flag
        /// </summary>
        bool _HtmlEncode { get; set; }

        /// <summary>
        /// Sets the HideSurroundingHtml flag
        /// </summary>
        bool _HideSurroundingHtml { get; set; }

        /// <summary>
        /// Sets the IsBindingAllowed flag
        /// </summary>
        bool _IsBindingAllowed { get; set; }

        /// <summary>
        /// Sets the IsBindingRequired flag
        /// </summary>
        bool _IsBindingRequired { get; set; }

        /// <summary>
        /// Sets the IsEnum flag
        /// </summary>
        bool _IsEnum { get; set; }

        /// <summary>
        /// Sets the IsFlagsEnum flag
        /// </summary>
        bool _IsFlagsEnum { get; set; }

        /// <summary>
        /// Sets the IsReadOnly flag
        /// </summary>
        bool _IsReadOnly { get; set; }

        /// <summary>
        /// Sets the IsRequired flag
        /// </summary>
        bool _IsRequired { get; set; }

        /// <summary>
        /// Sets the ModelBindingMessageProvider
        /// </summary>
        ModelBindingMessageProvider _ModelBindingMessageProvider { get; set; }

        /// <summary>
        /// Sets the Order
        /// </summary>
        int _Order { get; set; }

        /// <summary>
        /// Sets the Placeholder text
        /// </summary>
        string _Placeholder { get; set; }

        /// <summary>
        /// Sets the NullDisplayText
        /// </summary>
        string _NullDisplayText { get; set; }

        /// <summary>
        /// Sets the PropertyFilterProvider
        /// </summary>
        IPropertyFilterProvider _PropertyFilterProvider { get; set; }

        /// <summary>
        /// Sets the ShowForDisplay flag
        /// </summary>
        bool _ShowForDisplay { get; set; }

        /// <summary>
        /// Sets the ShowForEdit flag
        /// </summary>
        bool _ShowForEdit { get; set; }

        /// <summary>
        /// Sets the SimpleDisplayProperty
        /// </summary>
        string _SimpleDisplayProperty { get; set; }

        /// <summary>
        /// Sets the TemplateHint
        /// </summary>
        string _TemplateHint { get; set; }

        /// <summary>
        /// Sets the ValidateChildren flag
        /// </summary>
        bool _ValidateChildren { get; set; }

        /// <summary>
        /// Sets the ValidatorMetadata
        /// </summary>
        IReadOnlyList<object> _ValidatorMetadata { get; set; }

        /// <summary>
        /// Sets the PropertyGetter func
        /// </summary>
        Func<object, object> _PropertyGetter { get; set; }

        /// <summary>
        /// Sets the PropertySetter action
        /// </summary>
        Action<object, object> _PropertySetter { get; set; }

        /// <summary>
        /// Initialize with a ModelMetadataIdentity
        /// </summary>
        /// <param name="identity"></param>
        void Init(ModelMetadataIdentity identity);
    }

    /// <summary>
    /// provides a faked model metadata
    /// </summary>
    public class FakeModelMetadata : ModelMetadata, IFakeModelMetadata
    {
        /// <inheritdoc />
        public FakeModelMetadata(ModelMetadataIdentity identity) : base(identity)
        {
            Init(identity);
        }

        /// <summary>
        /// Initialize with a ModelMetadataIdentity
        /// </summary>
        /// <param name="identity"></param>
        public void Init(ModelMetadataIdentity identity)
        {
            _AdditionalValues = new Dictionary<object, object>();
            _Properties = new ModelPropertyCollection(Array.Empty<ModelMetadata>());
            _BinderModelName = "";
            _BinderType = typeof(ParameterBinder);
            _BindingSource = new BindingSource(GetRandomString(), identity.Name, isGreedy: true, isFromRequest: true);
            _DataTypeName = identity.ModelType.Name;
            _Description = identity.Name;
            _DisplayFormatString = "";
            _DisplayName = identity.Name;
            _EditFormatString = "";
            _ElementMetadata = null; // can't set to another FakeModelMetadata without exploding things
            _EnumGroupedDisplayNamesAndValues = Array.Empty<KeyValuePair<EnumGroupAndName, string>>();
            _EnumNamesAndValues = new Dictionary<string, string>();
            _ModelBindingMessageProvider = new DefaultModelBindingMessageProvider();
            _Placeholder = "";
            _NullDisplayText = "";
            _PropertyFilterProvider = new FakePropertyFilterProvider();
            _ValidatorMetadata = new List<object>();
            _PropertyGetter = _ => new object();
            _PropertySetter = (_, _) =>
            {
            };
        }

        /// <inheritdoc />
        public override IReadOnlyDictionary<object, object> AdditionalValues =>
            _AdditionalValues;

        /// <summary>
        /// Sets the AdditionalValues collection
        /// </summary>
        public IReadOnlyDictionary<object, object> _AdditionalValues { get; set; }

        /// <inheritdoc />
        public override ModelPropertyCollection Properties =>
            _Properties;

        /// <summary>
        /// Sets the ModelPropertyCollection
        /// </summary>
        public ModelPropertyCollection _Properties { get; set; }

        /// <inheritdoc />
        public override string BinderModelName =>
            _BinderModelName;

        /// <summary>
        /// Sets the BinderModelName
        /// </summary>
        public string _BinderModelName { get; set; }

        /// <inheritdoc />
        public override Type BinderType => _BinderType;

        /// <summary>
        /// Sets the Binder Type
        /// </summary>
        public Type _BinderType { get; set; }

        /// <inheritdoc />
        public override BindingSource BindingSource => _BindingSource;

        /// <summary>
        /// Sets the BindingSource
        /// </summary>
        public BindingSource _BindingSource { get; set; }

        /// <inheritdoc />
        public override bool ConvertEmptyStringToNull =>
            _ConvertEmptyStringToNull;

        /// <summary>
        /// Sets the ConvertEmptyStringToNull flag
        /// </summary>
        public bool _ConvertEmptyStringToNull { get; set; }

        /// <inheritdoc />
        public override string DataTypeName => _DataTypeName;

        /// <summary>
        /// Sets the DataTimeName
        /// </summary>
        public string _DataTypeName { get; set; }

        /// <inheritdoc />
        public override string Description => _Description;

        /// <summary>
        /// Sets the description
        /// </summary>
        public string _Description { get; set; }

        /// <inheritdoc />
        public override string DisplayFormatString => _DisplayFormatString;

        /// <summary>
        /// Sets the DisplayFormatString
        /// </summary>
        public string _DisplayFormatString { get; set; }

        /// <inheritdoc />
        public override string DisplayName => _DisplayName;

        /// <summary>
        /// Sets the DisplayName
        /// </summary>
        public string _DisplayName { get; set; }

        /// <inheritdoc />
        public override string EditFormatString => _EditFormatString;

        /// <summary>
        /// Sets the EditFormatString
        /// </summary>
        public string _EditFormatString { get; set; }

        /// <inheritdoc />
        public override ModelMetadata ElementMetadata => _ElementMetadata;

        /// <summary>
        /// Sets the Element Metadata
        /// </summary>
        public ModelMetadata _ElementMetadata { get; set; }

        /// <inheritdoc />
        public override IEnumerable<KeyValuePair<EnumGroupAndName, string>> EnumGroupedDisplayNamesAndValues
            => _EnumGroupedDisplayNamesAndValues;

        /// <summary>
        /// Sets the ENumGroupedDisplayNamesAndValues
        /// </summary>
        public IEnumerable<KeyValuePair<EnumGroupAndName, string>> _EnumGroupedDisplayNamesAndValues { get; set; }

        /// <inheritdoc />
        public override IReadOnlyDictionary<string, string> EnumNamesAndValues => _EnumNamesAndValues;

        /// <summary>
        /// Sets the EnumNamesAndValues
        /// </summary>
        public IReadOnlyDictionary<string, string> _EnumNamesAndValues { get; set; }

        /// <inheritdoc />
        public override bool HasNonDefaultEditFormat => _HasNonDefaultEditFormat;

        /// <summary>
        /// Sets the HasNonDefaultEditFormat flag
        /// </summary>
        public bool _HasNonDefaultEditFormat { get; set; }

        /// <inheritdoc />
        public override bool HtmlEncode => _HtmlEncode;

        /// <summary>
        /// Sets the HtmlEncode flag
        /// </summary>
        public bool _HtmlEncode { get; set; }

        /// <inheritdoc />
        public override bool HideSurroundingHtml => _HideSurroundingHtml;

        /// <summary>
        /// Sets the HideSurroundingHtml flag
        /// </summary>
        public bool _HideSurroundingHtml { get; set; }

        /// <inheritdoc />
        public override bool IsBindingAllowed => _IsBindingAllowed;

        /// <summary>
        /// Sets the IsBindingAllowed flag
        /// </summary>
        public bool _IsBindingAllowed { get; set; }

        /// <inheritdoc />
        public override bool IsBindingRequired => _IsBindingRequired;

        /// <summary>
        /// Sets the IsBindingRequired flag
        /// </summary>
        public bool _IsBindingRequired { get; set; }

        /// <inheritdoc />
        public override bool IsEnum => _IsEnum;

        /// <summary>
        /// Sets the IsEnum flag
        /// </summary>
        public bool _IsEnum { get; set; }

        /// <inheritdoc />
        public override bool IsFlagsEnum => _IsFlagsEnum;

        /// <summary>
        /// Sets the IsFlagsEnum flag
        /// </summary>
        public bool _IsFlagsEnum { get; set; }

        /// <inheritdoc />
        public override bool IsReadOnly => _IsReadOnly;

        /// <summary>
        /// Sets the IsReadOnly flag
        /// </summary>
        public bool _IsReadOnly { get; set; }

        /// <inheritdoc />
        public override bool IsRequired => _IsRequired;

        /// <summary>
        /// Sets the IsRequired flag
        /// </summary>
        public bool _IsRequired { get; set; }

        /// <inheritdoc />
        public override ModelBindingMessageProvider ModelBindingMessageProvider =>
            _ModelBindingMessageProvider;

        /// <summary>
        /// Sets the ModelBindingMessageProvider
        /// </summary>
        public ModelBindingMessageProvider _ModelBindingMessageProvider { get; set; }

        /// <inheritdoc />
        public override int Order => _Order;

        /// <summary>
        /// Sets the Order
        /// </summary>
        public int _Order { get; set; }

        /// <inheritdoc />
        public override string Placeholder => _Placeholder;

        /// <summary>
        /// Sets the Placeholder text
        /// </summary>
        public string _Placeholder { get; set; }

        /// <inheritdoc />
        public override string NullDisplayText => _NullDisplayText;

        /// <summary>
        /// Sets the NullDisplayText
        /// </summary>
        public string _NullDisplayText { get; set; }

        /// <inheritdoc />
        public override IPropertyFilterProvider PropertyFilterProvider => _PropertyFilterProvider;

        /// <summary>
        /// Sets the PropertyFilterProvider
        /// </summary>
        public IPropertyFilterProvider _PropertyFilterProvider { get; set; }

        /// <inheritdoc />
        public override bool ShowForDisplay => _ShowForDisplay;

        /// <summary>
        /// Sets the ShowForDisplay flag
        /// </summary>
        public bool _ShowForDisplay { get; set; }

        /// <inheritdoc />
        public override bool ShowForEdit => _ShowForEdit;

        /// <summary>
        /// Sets the ShowForEdit flag
        /// </summary>
        public bool _ShowForEdit { get; set; }

        /// <inheritdoc />
        public override string SimpleDisplayProperty => _SimpleDisplayProperty;

        /// <summary>
        /// Sets the SimpleDisplayProperty
        /// </summary>
        public string _SimpleDisplayProperty { get; set; }

        /// <inheritdoc />
        public override string TemplateHint => _TemplateHint;

        /// <summary>
        /// Sets the TemplateHint
        /// </summary>
        public string _TemplateHint { get; set; }

        /// <inheritdoc />
        public override bool ValidateChildren => _ValidateChildren;

        /// <summary>
        /// Sets the ValidateChildren flag
        /// </summary>
        public bool _ValidateChildren { get; set; }

        /// <inheritdoc />
        public override IReadOnlyList<object> ValidatorMetadata => _ValidatorMetadata;

        /// <summary>
        /// Sets the ValidatorMetadata
        /// </summary>
        public IReadOnlyList<object> _ValidatorMetadata { get; set; }

        /// <inheritdoc />
        public override Func<object, object> PropertyGetter => _PropertyGetter;

        /// <summary>
        /// Sets the PropertyGetter func
        /// </summary>
        public Func<object, object> _PropertyGetter { get; set; }

        /// <inheritdoc />
        public override Action<object, object> PropertySetter => _PropertySetter;

        /// <summary>
        /// Sets the PropertySetter action
        /// </summary>
        public Action<object, object> _PropertySetter { get; set; }

        /// <summary>
        /// Produces metadata for all properties on the provided type
        /// </summary>
        /// <param name="modelType"></param>
        /// <returns></returns>
        public override IEnumerable<ModelMetadata> GetMetadataForProperties(Type modelType)
        {
            if (modelType is null)
            {
                yield break;
            }

            foreach (var prop in modelType.GetProperties())
            {
                yield return GetMetadataForType(prop.PropertyType);
            }
        }

        /// <summary>
        /// Produces metadata for the provided typ
        /// </summary>
        /// <param name="modelType"></param>
        /// <returns></returns>
        public override ModelMetadata GetMetadataForType(Type modelType)
        {
            return new FakeModelMetadata(
                ModelMetadataIdentity.ForType(modelType)
            );
        }
    }

    /// <summary>
    /// Provides a fake property filter which filters nothing
    /// </summary>
    public class FakePropertyFilterProvider
        : IPropertyFilterProvider
    {
        /// <inheritdoc />
        public Func<ModelMetadata, bool> PropertyFilter => _ => true;
    }
}