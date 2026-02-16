namespace EmployeesOverlap.Core;

public sealed record WorkRecord(
    int EmpId,
    int ProjectId,
    DateTime DateFrom,
    DateTime DateTo
);

public sealed record ProjectOverlapRow(
    int Employee1,
    int Employee2,
    int ProjectId,
    int DaysWorked
);

public sealed record PairResult(
    int Employee1,
    int Employee2,
    int TotalDays,
    IReadOnlyList<ProjectOverlapRow> ProjectRows
);
