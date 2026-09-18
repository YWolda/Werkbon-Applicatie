using System.Globalization;
using UrenRegistratie.Models;

namespace UrenRegistratie.Services;

/// <summary>
/// Bevat alle logica voor opdrachten en gewerkte uren, inclusief de automatische
/// overuren-berekening: de eerste 8 uur (reizen + werken samen) per dag telt als
/// normale tijd, alles daarna telt automatisch als overuren - ongeacht of je op
/// dat moment aan het reizen of werken was.
/// </summary>
public class TimeTrackingService
{
    private const string ProjectsKey = "uren_projects";
    private const string SegmentsKey = "uren_segments";
    private const double NormalDayMinutes = 8 * 60;

    private readonly LocalStorageService _storage;
    private bool _isInitialized = false;

    private List<Project> _projects = new();
    private List<WorkSegment> _segments = new();

    public event Action? OnChange;

    public TimeTrackingService(LocalStorageService storage)
    {
        _storage = storage;
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        _projects = await _storage.GetListAsync<Project>(ProjectsKey);
        _segments = await _storage.GetListAsync<WorkSegment>(SegmentsKey);
        _isInitialized = true;
    }

    public IReadOnlyList<Project> Projects => _projects.Where(p => !p.IsArchived).ToList();
    public IReadOnlyList<Project> AllProjects => _projects;
    public IReadOnlyList<WorkSegment> Segments => _segments;

    public WorkSegment? ActiveSegment => _segments.FirstOrDefault(s => s.IsRunning);

    // ---------- Opdrachten ----------

    public async Task AddProjectAsync(Project project)
    {
        _projects.Add(project);
        await _storage.SetListAsync(ProjectsKey, _projects);
        NotifyChange();
    }

    public async Task UpdateProjectAsync(Project project)
    {
        var index = _projects.FindIndex(p => p.Id == project.Id);
        if (index == -1) return;

        _projects[index] = project;
        await _storage.SetListAsync(ProjectsKey, _projects);
        NotifyChange();
    }

    public async Task ArchiveProjectAsync(Guid projectId)
    {
        var project = _projects.FirstOrDefault(p => p.Id == projectId);
        if (project is null) return;

        project.IsArchived = true;
        await _storage.SetListAsync(ProjectsKey, _projects);
        NotifyChange();
    }

    public Project? GetProject(Guid projectId) => _projects.FirstOrDefault(p => p.Id == projectId);

    public string GetProjectName(Guid projectId) => GetProject(projectId)?.Name ?? "Onbekende opdracht";

    // ---------- Werkdag starten / wisselen / stoppen ----------

    public async Task<bool> StartDayAsync(Guid projectId, SegmentType startType)
    {
        if (ActiveSegment is not null) return false;

        _segments.Add(new WorkSegment
        {
            ProjectId = projectId,
            Type = startType,
            Start = DateTime.Now
        });

        await _storage.SetListAsync(SegmentsKey, _segments);
        NotifyChange();
        return true;
    }

    /// <summary>
    /// Wisselt tussen "onderweg", "aan het werk" en "pauze". Sluit het huidige
    /// segment af en start meteen een nieuw segment van het andere type.
    /// </summary>
    public async Task<bool> SwitchTypeAsync(SegmentType newType)
    {
        var active = ActiveSegment;
        if (active is null) return false;
        if (active.Type == newType) return false;

        active.End = DateTime.Now;

        _segments.Add(new WorkSegment
        {
            ProjectId = active.ProjectId,
            Type = newType,
            Start = DateTime.Now
        });

        await _storage.SetListAsync(SegmentsKey, _segments);
        NotifyChange();
        return true;
    }

    public async Task<bool> StopDayAsync(string? note = null)
    {
        var active = ActiveSegment;
        if (active is null) return false;

        active.End = DateTime.Now;
        active.Note = note;

        await _storage.SetListAsync(SegmentsKey, _segments);
        NotifyChange();
        return true;
    }

    // ---------- Berekeningen: dag- en weekoverzichten met automatische overuren ----------

    /// <summary>
    /// Berekent voor één kalenderdag, per segment, hoeveel minuten normale tijd en
    /// hoeveel minuten overuren dat segment bijdroeg. Segmenten worden chronologisch
    /// verwerkt; zodra de 8-uursgrens van de dag bereikt is, valt de rest in overuren.
    /// Reistijd vóór het eerste werk-segment telt als "heen", erna als "terug".
    /// </summary>
    public List<SegmentBreakdown> GetDayBreakdown(DateOnly date)
    {
        var daySegments = _segments
            .Where(s => DateOnly.FromDateTime(s.Start) == date && !s.IsRunning)
            .OrderBy(s => s.Start)
            .ToList();

        var result = new List<SegmentBreakdown>();
        double cumulativeNormal = 0;
        bool workStarted = false;

        foreach (var segment in daySegments)
        {
            var durationMinutes = segment.Duration.TotalMinutes;

            if (segment.Type == SegmentType.Break)
            {
                // Pauzetijd is onbetaalde, "neutrale" tijd: telt niet mee voor de
                // 8-uursgrens en dus ook niet voor overuren.
                result.Add(new SegmentBreakdown
                {
                    Segment = segment,
                    Category = "Break",
                    NormalMinutes = durationMinutes,
                    OvertimeMinutes = 0
                });
                continue;
            }

            var normalCapacityLeft = Math.Max(0, NormalDayMinutes - cumulativeNormal);
            var normalPortion = Math.Min(durationMinutes, normalCapacityLeft);
            var overtimePortion = durationMinutes - normalPortion;
            cumulativeNormal += normalPortion;

            string category;
            if (segment.Type == SegmentType.Travel)
            {
                category = workStarted ? "TravelBack" : "TravelTo";
            }
            else
            {
                workStarted = true;
                category = "Work";
            }

            result.Add(new SegmentBreakdown
            {
                Segment = segment,
                Category = category,
                NormalMinutes = normalPortion,
                OvertimeMinutes = overtimePortion
            });
        }

        return result;
    }

    /// <summary>
    /// Samenvatting van één dag, optioneel gefilterd op één opdracht (voor het
    /// weekrapport). Zonder projectId wordt over alle opdrachten samen berekend -
    /// zo blijft de 8-uursgrens correct, ook als je op één dag aan meerdere
    /// opdrachten werkt.
    /// </summary>
    public DayHoursSummary GetDaySummary(DateOnly date, Guid? projectId = null)
    {
        var breakdown = GetDayBreakdown(date);
        if (projectId is not null)
        {
            breakdown = breakdown.Where(b => b.Segment.ProjectId == projectId).ToList();
        }

        var summary = new DayHoursSummary { Date = date };
        foreach (var b in breakdown)
        {
            switch (b.Category)
            {
                case "TravelTo":
                    summary.TravelTo += TimeSpan.FromMinutes(b.NormalMinutes);
                    break;
                case "TravelBack":
                    summary.TravelBack += TimeSpan.FromMinutes(b.NormalMinutes);
                    break;
                case "Work":
                    summary.Work += TimeSpan.FromMinutes(b.NormalMinutes);
                    break;
                case "Break":
                    summary.Break += TimeSpan.FromMinutes(b.NormalMinutes);
                    break;
            }
            summary.Overtime += TimeSpan.FromMinutes(b.OvertimeMinutes);
        }

        return summary;
    }

    /// <summary>
    /// Alle 7 dagen (maandag t/m zondag) van de week waarin de opgegeven datum valt,
    /// voor gebruik in het weekrapport.
    /// </summary>
    public List<DayHoursSummary> GetWeekSummary(DateOnly anyDateInWeek, Guid? projectId = null)
    {
        var monday = anyDateInWeek.AddDays(-((int)anyDateInWeek.DayOfWeek == 0 ? 6 : (int)anyDateInWeek.DayOfWeek - 1));
        var days = new List<DayHoursSummary>();

        for (int i = 0; i < 7; i++)
        {
            days.Add(GetDaySummary(monday.AddDays(i), projectId));
        }

        return days;
    }

    public static int GetIsoWeekNumber(DateOnly date) =>
        ISOWeek.GetWeekOfYear(date.ToDateTime(TimeOnly.MinValue));

    /// <summary>Live samenvatting van vandaag, inclusief het nog lopende segment.</summary>
    public DayHoursSummary GetTodaySummary(Guid? projectId = null) =>
        GetDaySummaryIncludingRunning(DateOnly.FromDateTime(DateTime.Now), projectId);

    private DayHoursSummary GetDaySummaryIncludingRunning(DateOnly date, Guid? projectId)
    {
        // Voor live weergave: doe alsof het lopende segment nu stopt, zonder het echt te stoppen.
        var runningSegment = _segments.FirstOrDefault(s => s.IsRunning && DateOnly.FromDateTime(s.Start) == date);
        if (runningSegment is null)
        {
            return GetDaySummary(date, projectId);
        }

        var originalEnd = runningSegment.End;
        runningSegment.End = DateTime.Now;
        var summary = GetDaySummary(date, projectId);
        runningSegment.End = originalEnd;
        return summary;
    }

    /// <summary>
    /// Aantal minuten reis-/werktijd sinds de laatst afgeronde pauze vandaag
    /// (of sinds het begin van de werkdag als er nog geen pauze is geweest).
    /// Gebruikt om te waarschuwen zodra het bedrijfsreglement (elke 4 uur een
    /// half uur pauze) dreigt te worden overschreden.
    /// </summary>
    public double GetMinutesSinceLastBreak()
    {
        var active = ActiveSegment;
        if (active is null) return 0;

        var today = DateOnly.FromDateTime(active.Start);
        var todaysSegments = _segments
            .Where(s => DateOnly.FromDateTime(s.Start) == today)
            .OrderBy(s => s.Start)
            .ToList();

        var lastBreakEnd = todaysSegments
            .Where(s => s.Type == SegmentType.Break && !s.IsRunning)
            .Select(s => s.End!.Value)
            .OrderByDescending(d => d)
            .FirstOrDefault();

        var since = lastBreakEnd == default ? todaysSegments.First().Start : lastBreakEnd;

        double minutes = 0;
        foreach (var s in todaysSegments.Where(s => s.Type != SegmentType.Break))
        {
            var start = s.Start > since ? s.Start : since;
            var end = s.End ?? DateTime.Now;
            if (end > start)
            {
                minutes += (end - start).TotalMinutes;
            }
        }

        return minutes;
    }

    /// <summary>True zodra er 4 uur (240 min) gereisd/gewerkt is zonder pauze.</summary>
    public bool IsBreakDue() => GetMinutesSinceLastBreak() >= 240;

    private void NotifyChange() => OnChange?.Invoke();
}
