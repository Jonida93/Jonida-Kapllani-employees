# Employees - Longest Common Period

## What it does
- Reads a CSV file with columns: `EmpID, ProjectID, DateFrom, DateTo`
- Treats `DateTo = NULL` as today's date
- Finds the pair of employees that worked together the longest across all common projects
- Shows:
  - Best pair and total overlap days
  - Breakdown per project (Employee #1, Employee #2, ProjectID, Days worked)

## How to run
1. Open `EmployeesOverlap.sln` in Visual Studio
2. Set `EmployeesOverlap.Wpf` as Startup Project
3. Run, then click **Pick CSV file** and select your CSV

## CSV notes
- Accepts multiple date formats such as:
  - `yyyy-MM-dd`, `yyyy/MM/dd`
  - `dd/MM/yyyy`, `MM/dd/yyyy`
  - plus several common variants
- `NULL` (case-insensitive) is accepted in `DateTo`


## API + UI setup
This solution contains:
- EmployeesOverlap.Api (ASP.NET Core Web API)
- EmployeesOverlap.Wpf (WPF client)
- EmployeesOverlap.Core (shared logic)

### Run order
1. Start **EmployeesOverlap.Api** (it opens Swagger at http://localhost:5000/swagger)
2. Start **EmployeesOverlap.Wpf**
3. In the WPF app click **Pick CSV file** and upload a .csv file (the UI posts it to the API)

If you change the API port, update it in `src/EmployeesOverlap.Wpf/MainWindow.xaml.cs` (apiBase).
