using UrenRegistratie.Models;

namespace UrenRegistratie.Services;

/// <summary>
/// Houdt een lijst medewerkers bij zodat je die op de werkbon kan kiezen
/// in plaats van elke keer opnieuw te typen.
/// </summary>
public class EmployeeService
{
    private const string EmployeesKey = "uren_employees";

    private readonly LocalStorageService _storage;
    private List<Employee> _employees = new();
    private bool _initialized = false;

    public event Action? OnChange;

    public EmployeeService(LocalStorageService storage)
    {
        _storage = storage;
    }

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        _employees = await _storage.GetListAsync<Employee>(EmployeesKey);

        if (_employees.Count == 0)
        {
            _employees.Add(new Employee { Name = "Youri Wolda" });
            await _storage.SetListAsync(EmployeesKey, _employees);
        }

        _initialized = true;
    }

    public IReadOnlyList<Employee> Employees => _employees.OrderBy(e => e.Name).ToList();

    public async Task<Employee> AddEmployeeAsync(string name)
    {
        var trimmed = name.Trim();

        var existing = _employees.FirstOrDefault(e => e.Name.Equals(trimmed, StringComparison.OrdinalIgnoreCase));
        if (existing is not null) return existing;

        var employee = new Employee { Name = trimmed };
        _employees.Add(employee);
        await _storage.SetListAsync(EmployeesKey, _employees);
        OnChange?.Invoke();
        return employee;
    }

    public async Task RemoveEmployeeAsync(Guid id)
    {
        _employees.RemoveAll(e => e.Id == id);
        await _storage.SetListAsync(EmployeesKey, _employees);
        OnChange?.Invoke();
    }
}
