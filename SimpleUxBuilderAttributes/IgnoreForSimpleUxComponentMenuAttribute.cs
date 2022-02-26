namespace Simple.Ux.XBam.Components {

  /// <summary>
  /// Hide a field  with a public get and any setter from the simple ux component menu
  /// </summary>
  [System.AttributeUsage(
    validOn: System.AttributeTargets.Field | System.AttributeTargets.Property,
    Inherited = true
  )] public class IgnoreForSimpleUxComponentMenuAttribute : System.Attribute {}
}
