namespace LdifHelper
{
    /// <summary>
    /// Represents the current LDIF record type being processed by the reader.
    /// </summary>
    public enum ChangeType
    {
        /// <summary>
        /// Indicates no open record is currently being processed.
        /// </summary>
        None,

        /// <summary>
        /// Indicates a change-add record is currently being processed.
        /// </summary>
        Add,

        /// <summary>
        /// Indicates a change-delete record is currently being processed.
        /// </summary>
        Delete,

        /// <summary>
        /// Indicates a change-moddn record is currently being processed.
        /// </summary>
        ModDn,

        /// <summary>
        /// Indicates a change-moddn record is currently being processed.
        /// </summary>
        ModRdn,

        /// <summary>
        /// Indicates a change-modify record is currently being processed.
        /// </summary>
        Modify
    }
}