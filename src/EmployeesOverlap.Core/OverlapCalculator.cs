namespace EmployeesOverlap.Core;

public static class OverlapCalculator
{
    public static PairResult FindBestPair(IReadOnlyList<WorkRecord> records)
    {
        if (records == null) throw new ArgumentNullException(nameof(records));

        if (records.Count == 0)
            return new PairResult(0, 0, 0, Array.Empty<ProjectOverlapRow>());

        var totalDaysByPair = new Dictionary<(int e1, int e2), int>();
        var breakdownByPair = new Dictionary<(int e1, int e2), List<ProjectOverlapRow>>();

        foreach (var projectGroup in records.GroupBy(r => r.ProjectId))
        {
            var list = projectGroup.ToList();

            for (int i = 0; i < list.Count; i++)
            {
                for (int j = i + 1; j < list.Count; j++)
                {
                    var a = list[i];
                    var b = list[j];

                    if (a.EmpId == b.EmpId)
                        continue;

                    var overlapDays = ComputeOverlapDaysInclusive(a.DateFrom, a.DateTo, b.DateFrom, b.DateTo);
                    if (overlapDays <= 0)
                        continue;

                    var (e1, e2) = NormalizePair(a.EmpId, b.EmpId);

                    totalDaysByPair.TryGetValue((e1, e2), out var currentTotal);
                    totalDaysByPair[(e1, e2)] = currentTotal + overlapDays;

                    if (!breakdownByPair.TryGetValue((e1, e2), out var rows))
                    {
                        rows = new List<ProjectOverlapRow>();
                        breakdownByPair[(e1, e2)] = rows;
                    }

                    rows.Add(new ProjectOverlapRow(e1, e2, projectGroup.Key, overlapDays));
                }
            }
        }

        if (totalDaysByPair.Count == 0)
            return new PairResult(0, 0, 0, Array.Empty<ProjectOverlapRow>());

        var best = totalDaysByPair
            .OrderByDescending(kvp => kvp.Value)
            .ThenBy(kvp => kvp.Key.e1)
            .ThenBy(kvp => kvp.Key.e2)
            .First();

        var bestPair = best.Key;
        var bestRows = breakdownByPair.TryGetValue(bestPair, out var rowList)
            ? rowList.OrderByDescending(r => r.DaysWorked).ThenBy(r => r.ProjectId).ToList()
            : new List<ProjectOverlapRow>();

        return new PairResult(bestPair.e1, bestPair.e2, best.Value, bestRows);
    }

    private static (int e1, int e2) NormalizePair(int a, int b) => a < b ? (a, b) : (b, a);

    private static int ComputeOverlapDaysInclusive(DateTime from1, DateTime to1, DateTime from2, DateTime to2)
    {
        var start = from1 > from2 ? from1 : from2;
        var end = to1 < to2 ? to1 : to2;

        if (end < start) return 0;

        return (end.Date - start.Date).Days + 1;
    }
}
