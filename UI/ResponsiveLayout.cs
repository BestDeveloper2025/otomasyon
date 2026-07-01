namespace otomasyon.UI;

internal static class ResponsiveLayout
{
    public static void ApplyHorizontalSplit(
        SplitContainer split,
        double panel1Ratio,
        int panel1Min,
        int panel2Min)
    {
        int total = split.Orientation == Orientation.Vertical
            ? split.ClientSize.Width
            : split.ClientSize.Height;

        if (total <= panel1Min + panel2Min + split.SplitterWidth)
            return;

        try
        {
            int max = total - split.SplitterWidth - panel2Min;
            int min = panel1Min;
            int desired = (int)Math.Round(total * panel1Ratio);
            split.SplitterDistance = Math.Clamp(desired, min, Math.Max(min, max));
        }
        catch (InvalidOperationException)
        {
            // İlk yüklemede boyut henüz hazır olmayabilir.
        }
    }

    public static void ApplyVerticalSplit(
        SplitContainer split,
        double panel1Ratio,
        int panel1Min,
        int panel2Min)
    {
        int total = split.ClientSize.Height;
        if (total <= panel1Min + panel2Min + split.SplitterWidth)
            return;

        try
        {
            int max = total - split.SplitterWidth - panel2Min;
            int min = panel1Min;
            int desired = (int)Math.Round(total * panel1Ratio);
            split.SplitterDistance = Math.Clamp(desired, min, Math.Max(min, max));
        }
        catch (InvalidOperationException)
        {
            // İlk yüklemede boyut henüz hazır olmayabilir.
        }
    }

    public static void SyncHorizontalRatio(SplitContainer split, ref double ratio)
    {
        int total = split.ClientSize.Width;
        if (total > 0)
            ratio = (double)split.SplitterDistance / total;
    }

    public static void SyncVerticalRatio(SplitContainer split, ref double ratio)
    {
        int total = split.ClientSize.Height;
        if (total > 0)
            ratio = (double)split.SplitterDistance / total;
    }

    public static void ApplyRecipeListColumns(ListView listView)
    {
        if (listView.Columns.Count < 6)
            return;

        int total = listView.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 4;
        if (total < 320)
            return;

        const int indexW = 36;
        const int edgeW = 52;
        const int thicknessW = 84;
        const int qtyW = 48;
        const int sourceW = 76;
        int fileW = total - indexW - edgeW - thicknessW - qtyW - sourceW;
        if (fileW < 80)
            fileW = 80;

        listView.Columns[0].Width = indexW;
        listView.Columns[1].Width = fileW;
        listView.Columns[2].Width = edgeW;
        listView.Columns[3].Width = thicknessW;
        listView.Columns[4].Width = qtyW;
        listView.Columns[5].Width = Math.Max(sourceW, total - indexW - fileW - edgeW - thicknessW - qtyW);
    }

    public static int MeasureWrappedFlowHeight(FlowLayoutPanel flow, int availableWidth)
    {
        if (availableWidth < 40)
            return flow.PreferredSize.Height;

        int previousWidth = flow.Width;
        try
        {
            flow.Width = availableWidth;
            return flow.PreferredSize.Height;
        }
        finally
        {
            flow.Width = previousWidth;
        }
    }
}
