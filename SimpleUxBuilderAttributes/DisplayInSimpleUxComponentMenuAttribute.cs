namespace Simple.Ux.XBam.Components {

  /// <summary>
  /// Can be used to show a private/protected component field in the simple ux component menu
  /// </summary>
  [System.AttributeUsage(
    validOn: System.AttributeTargets.Field | System.AttributeTargets.Property,
    Inherited = true
  )]

  public class DisplayInSimpleUxComponentMenuAttribute : System.Attribute {
    internal bool IsReadOnly {
      get;
    } = false;

    internal string Name {
      get;
    }

    /// <summary>
    /// Used to mark a private field in a component as "should be shown" in the editor.
    /// </summary>
    public DisplayInSimpleUxComponentMenuAttribute(string Name = null, bool IsReadOnly = false) {
      this.Name = Name;
      this.IsReadOnly = IsReadOnly;
    }
  }
}
