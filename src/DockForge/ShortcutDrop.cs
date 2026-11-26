namespace DockForge;

internal static class ShortcutDrop
{
    public static void Handle(DragEventArgs e, List<Item> items, bool enabled, Action changed)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
        e.Handled = true;
        // Explorer owns the files. Pinning must never report a move to the drag source.
        e.Effects = enabled ? ReferenceEffect(e.AllowedEffects) : DragDropEffects.None;
        if (e.RoutedEvent != DragDrop.DropEvent || e.Effects == DragDropEffects.None) return;
        if (e.Data.GetData(DataFormats.FileDrop) is not string[] { Length: > 0 } paths) return;
        items.AddRange(paths.Select(ShellService.FromDroppedPath));
        changed();
    }

    public static DragDropEffects ReferenceEffect(DragDropEffects allowed) =>
        allowed.HasFlag(DragDropEffects.Link) ? DragDropEffects.Link : allowed & DragDropEffects.Copy;
}
