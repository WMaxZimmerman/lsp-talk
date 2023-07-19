namespace System;

/// <summary>Represents the method that will handle an event that has no event data.</summary>
/// <param name="sender">The source of the event.</param>
/// <param name="e">An object that contains no event data.</param>
public delegate void EventHandler(object? sender, EventArgs e);
