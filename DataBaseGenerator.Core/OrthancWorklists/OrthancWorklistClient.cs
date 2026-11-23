using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DataBaseGenerator.Core.OrthancWorklists;

public sealed class OrthancWorklistClient : IDisposable
{
    private readonly HttpClient _http;

    private OrthancWorklistClient(HttpClient http)
    {
        _http = http;
    }


    public static OrthancWorklistClient Open(string baseUrl, string username, string password)
    {
        var root = (baseUrl ?? string.Empty).Trim().TrimEnd('/');
        if (!Uri.TryCreate(root + "/", UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new OrthancWorklistException("Некорректный адрес Orthanc.");
        }

        var http = new HttpClient { BaseAddress = uri, Timeout = TimeSpan.FromSeconds(60) };
        if (!string.IsNullOrEmpty(username))
        {
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password ?? string.Empty}"));
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
        }

        return new OrthancWorklistClient(http);
    }


    public async Task PingAsync(CancellationToken cancellationToken = default)
    {
        await SendAsync(HttpMethod.Get, "system", null, cancellationToken);
    }

    public async Task<string> CreateAsync(string jsonBody, CancellationToken cancellationToken = default)
    {
        var body = await SendAsync(HttpMethod.Post, "worklists/create", jsonBody, cancellationToken);
        using var document = JsonDocument.Parse(body);
        if (!TryGet(document.RootElement, "ID", out var id))
            throw new OrthancWorklistException("Orthanc не вернул ID задания.");

        var value = ReadText(id);
        if (string.IsNullOrWhiteSpace(value))
            throw new OrthancWorklistException("Orthanc не вернул ID задания.");

        return value;
    }

    public async Task<IReadOnlyList<OrthancWorklistEntry>> ListAsync(CancellationToken cancellationToken = default)
    {
        var body = await SendAsync(HttpMethod.Get, "worklists?format=Simplify", null, cancellationToken);
        using var document = JsonDocument.Parse(body);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
            throw new OrthancWorklistException("Orthanc вернул список заданий в неожиданном виде.");

        var entries = new List<OrthancWorklistEntry>();
        var pendingIds = new List<string>();
        foreach (var item in document.RootElement.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.String)
            {
                var id = item.GetString();
                if (!string.IsNullOrWhiteSpace(id))
                    pendingIds.Add(id);
                continue;
            }

            if (item.ValueKind != JsonValueKind.Object)
                continue;

            if (HasTags(item))
                entries.Add(ParseObject(item));
            else if (TryGet(item, "ID", out var idElement))
            {
                var id = ReadText(idElement);
                if (!string.IsNullOrWhiteSpace(id))
                    pendingIds.Add(id);
            }
        }

        foreach (var id in pendingIds)
        {
            var one = await SendAsync(
                HttpMethod.Get,
                "worklists/" + Uri.EscapeDataString(id) + "?format=Simplify",
                null,
                cancellationToken);
            using var oneDocument = JsonDocument.Parse(one);
            entries.Add(ParseObject(oneDocument.RootElement, id));
        }

        return entries;
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new OrthancWorklistException("У задания нет идентификатора Orthanc.");

        return SendAsync(HttpMethod.Delete, "worklists/" + Uri.EscapeDataString(id.Trim()), null, cancellationToken);
    }

    public void Dispose() => _http.Dispose();

    private async Task<string> SendAsync(
        HttpMethod method,
        string relativeUrl,
        string jsonBody,
        CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(method, relativeUrl);
            if (jsonBody != null)
                request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            using var response = await _http.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (response.IsSuccessStatusCode)
                return body ?? string.Empty;

            throw new OrthancWorklistException(FormatError(response.StatusCode, body));
        }
        catch (OrthancWorklistException)
        {
            throw;
        }
        catch (TaskCanceledException)
        {
            throw new OrthancWorklistException("Orthanc не ответил вовремя.");
        }
        catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException or UriFormatException)
        {
            throw new OrthancWorklistException($"Нет соединения с Orthanc: {ex.Message}");
        }
    }

    private static bool HasTags(JsonElement item) =>
        TryGet(item, "Tags", out _)
        || TryGet(item, "PatientID", out _)
        || TryGet(item, "AccessionNumber", out _);

    private static OrthancWorklistEntry ParseObject(JsonElement item, string fallbackId = null)
    {
        var tags = item;
        if (TryGet(item, "Tags", out var tagsElement) && tagsElement.ValueKind == JsonValueKind.Object)
            tags = tagsElement;

        var id = TryGet(item, "ID", out var idElement) ? ReadText(idElement) : fallbackId ?? string.Empty;
        var step = FirstStep(tags);
        var date = ReadTag(step, "ScheduledProcedureStepStartDate");
        var time = ReadTag(step, "ScheduledProcedureStepStartTime");

        return new OrthancWorklistEntry
        {
            Id = id,
            PatientId = ReadTag(tags, "PatientID"),
            PatientName = ReadTag(tags, "PatientName").Replace('^', ' '),
            PatientSex = ReadTag(tags, "PatientSex"),
            PatientAddress = ReadTag(tags, "PatientAddress"),
            PatientTelephone = ReadTag(tags, "PatientTelephoneNumbers"),
            PatientComments = ReadTag(tags, "PatientComments"),
            PatientBirthDate = FormatSchedule(ReadTag(tags, "PatientBirthDate"), string.Empty),
            AccessionNumber = ReadTag(tags, "AccessionNumber"),
            RequestedProcedureId = ReadTag(tags, "RequestedProcedureID"),
            RequestedProcedureDescription = ReadTag(tags, "RequestedProcedureDescription"),
            ScheduledProcedureStepId = ReadTag(step, "ScheduledProcedureStepID"),
            ReferringPhysicianName = ReadTag(tags, "ReferringPhysicianName").Replace('^', ' '),
            Modality = ReadTag(step, "Modality"),
            StationAeTitle = ReadTag(step, "ScheduledStationAETitle"),
            ScheduledStart = FormatSchedule(date, time),
            Description = ReadTag(step, "ScheduledProcedureStepDescription")
        };
    }

    private static JsonElement FirstStep(JsonElement tags)
    {
        if (!TryGet(tags, "ScheduledProcedureStepSequence", out var sequence))
            return default;

        if (sequence.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in sequence.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Object)
                    return item;
            }
        }

        return sequence.ValueKind == JsonValueKind.Object ? sequence : default;
    }

    private static string ReadTag(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object || !TryGet(element, name, out var value))
            return string.Empty;

        return ReadText(value);
    }

    private static string ReadText(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                return element.GetString() ?? string.Empty;
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
                return element.GetRawText();
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    var text = ReadText(item);
                    if (text.Length > 0)
                        return text;
                }

                return string.Empty;
            case JsonValueKind.Object:
                if (TryGet(element, "Value", out var value))
                    return ReadText(value);
                return string.Empty;
            default:
                return string.Empty;
        }
    }

    private static bool TryGet(JsonElement element, string name, out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            if (element.TryGetProperty(name, out value))
                return true;

            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    private static string FormatSchedule(string date, string time)
    {
        if (date.Length >= 8
            && DateTime.TryParseExact(date.Substring(0, 8), "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var day))
        {
            var hour = 0;
            var minute = 0;
            if (time.Length >= 4
                && int.TryParse(time.Substring(0, 2), out hour)
                && int.TryParse(time.Substring(2, 2), out minute))
            {
                return new DateTime(day.Year, day.Month, day.Day, hour, minute, 0).ToString("dd.MM.yyyy HH:mm");
            }

            return day.ToString("dd.MM.yyyy");
        }

        return string.Join(" ", new[] { date, time }).Trim();
    }

    private static string FormatError(HttpStatusCode code, string body)
    {
        body = (body ?? string.Empty).Trim();
        if (body.Length > 300)
            body = body.Substring(0, 300);

        if (body.Length == 0)
            return $"Orthanc ответил {(int)code}.";

        try
        {
            using var document = JsonDocument.Parse(body);
            if (TryGet(document.RootElement, "Message", out var message))
            {
                var text = ReadText(message);
                if (text.Length > 0)
                    return $"Orthanc ответил {(int)code}: {text}";
            }
        }
        catch (JsonException)
        {
        }

        return $"Orthanc ответил {(int)code}: {body}";
    }
}
