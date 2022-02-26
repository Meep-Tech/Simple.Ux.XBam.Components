using Meep.Tech.Data;
using Simple.Ux.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Simple.Ux.XBam.Components {

  /// <summary>
  /// If this component has a built in simple ux menu that can edit it's values.
  /// </summary>
  public interface IHasSimpleUxComponentEditMenu
      : Meep.Tech.Data.IComponent {

    internal static Dictionary<string, View> _compiledViews
      = new();

    string SimpleUxMenuTitle
      => Regex.Replace(GetType().Name, "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])", " $1");

    /// <summary>
    /// logic to build the editor ux from an empty builder.
    /// </summary>
    ViewBuilder BuildSimpleUxMenu(ViewBuilder builder, IComponent.IBuilderFactory componentFactory);

    /// <summary>
    /// Get the base editor UX for this type of component.
    /// </summary>
    View GetSimpleUxEditMenu()
      => _compiledViews.TryGetValue(SimpleUxMenuTitle, out View found)
        ? found
        : _compiledViews[SimpleUxMenuTitle] = BuildSimpleUxMenu(new ViewBuilder(SimpleUxMenuTitle), Factory)
          .Build();
  }

  ///<summary><inheritdoc/></summary>
  public interface IHasSimpleUxComponentEditMenu<TComponentBase>
      : IHasSimpleUxComponentEditMenu,
        IComponent<TComponentBase>
    where TComponentBase : IComponent<TComponentBase> {

    ///<summary><inheritdoc/></summary>
    ViewBuilder IHasSimpleUxComponentEditMenu.BuildSimpleUxMenu(ViewBuilder builder, IComponent.IBuilderFactory componentFactory) {
      System.Type ComponentBase = typeof(TComponentBase);
      foreach (MemberInfo member in ComponentBase.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        .Cast<MemberInfo>().Concat(ComponentBase.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
      ) {
        // ignore attribute
        if (member.GetCustomAttribute<IgnoreForSimpleUxComponentMenuAttribute>() != null) {
          continue;
        }

        /// Check for fields we should include by default:
        DisplayInSimpleUxComponentMenuAttribute includeAttribute
          = member.GetCustomAttribute<DisplayInSimpleUxComponentMenuAttribute>();

        DataField builtField;
        if (member is FieldInfo field && (field.IsPublic || includeAttribute is not null)) {
          builtField
            = ViewBuilder.BuildDefaultField(field, includeAttribute is not null ? new Dictionary<System.Type, Attribute> {
                  { typeof(ReadOnlyAttribute),  new ReadOnlyAttribute(includeAttribute.IsReadOnly) },
                  { typeof(DisplayNameAttribute),  new DisplayNameAttribute(includeAttribute.Name ?? field.Name) }
            } : null);
          if (builtField is not null) {
            builder.AddField(builtField);
          }
        } else if (member is PropertyInfo prop && (prop.GetMethod.IsPublic || includeAttribute is not null)) {
          builtField
            = ViewBuilder.BuildDefaultField(prop, includeAttribute is not null ? new Dictionary<System.Type, Attribute> {
                  { typeof(ReadOnlyAttribute),  new ReadOnlyAttribute(includeAttribute.IsReadOnly) },
                  { typeof(DisplayNameAttribute),  new DisplayNameAttribute(includeAttribute.Name ?? prop.Name) }
            } : null);
          if (builtField is not null) {
            builder.AddField(builtField);
          }
        } else
          continue;

        /// Add the callback listener for public fields:
        // if it's for a sub field, it's a bit different
        if (builtField.DataKey.Contains("::")) {
          string[] compoundKeyParts = builtField.DataKey.Split("::");
          builtField.AddValueChangeListener(
            "OnSimpleUxComponentMenuSubValueChanged",
            (updatedField, _) =>
              ((GetType().GetMember(compoundKeyParts[0]).First() as PropertyInfo).GetValue(this) as IDictionary)[compoundKeyParts[0]]
                = updatedField.Value
          );
        } else {
          builtField.AddValueChangeListener(
            "OnSimpleUxComponentMenuValueChanged",
            (updatedField, _) =>
              (GetType().GetMember(updatedField.DataKey).First() as PropertyInfo)
                .SetValue(this, updatedField.Value)
          );
        }
      }

      return builder;
    }

    /// <summary>
    /// Update from a ux.
    /// </summary>
    /// <param name="ux">The ux for this component</param>
    /// <param name="updatedFieldKey">(optional) a field that was changed.</param>
    internal void _updateFromFieldChange(Simple.Ux.Data.View ux, string updatedFieldKey = null) {
      if (updatedFieldKey is not null && ux.TryToGetField(updatedFieldKey, out var found)) {
        if (updatedFieldKey.Contains("::")) {
          var parts = updatedFieldKey.Split("::");
          ((GetType().GetMember(parts[0]).First() as PropertyInfo).GetValue(this) as IDictionary)[parts[0]]
            = found.Value;
        } else
          (GetType().GetMember(updatedFieldKey).First() as PropertyInfo)
            .SetValue(this, found.Value);
      }
    }
  }
}
