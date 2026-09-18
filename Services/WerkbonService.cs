using UrenRegistratie.Models;

namespace UrenRegistratie.Services;

/// <summary>
/// Slaat afgeronde werkbonnen op (lokaal, net als de rest van de app) en houdt
/// ze beschikbaar voor het Overzicht.
/// </summary>
public class WerkbonService
{
    private const string WerkbonnenKey = "uren_werkbonnen";

    private readonly LocalStorageService _storage;
    private List<WerkbonRecord> _records = new();
    private bool _initialized = false;

    public event Action? OnChange;

    public WerkbonService(LocalStorageService storage)
    {
        _storage = storage;
    }

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        _records = await _storage.GetListAsync<WerkbonRecord>(WerkbonnenKey);
        _initialized = true;
    }

    public IReadOnlyList<WerkbonRecord> Records => _records.OrderByDescending(r => r.CreatedAt).ToList();

    public async Task SaveAsync(WerkbonRecord record)
    {
        var index = _records.FindIndex(r => r.Id == record.Id);
        if (index >= 0)
        {
            _records[index] = record;
        }
        else
        {
            _records.Add(record);
        }

        await _storage.SetListAsync(WerkbonnenKey, _records);
        OnChange?.Invoke();
    }

    /// <summary>Laatst gebruikte medewerkernaam, zodat je 'm niet elke keer opnieuw hoeft te typen.</summary>
    public string? GetLastUsedEmployeeName() =>
        _records.OrderByDescending(r => r.CreatedAt).FirstOrDefault()?.EmployeeName;
}
