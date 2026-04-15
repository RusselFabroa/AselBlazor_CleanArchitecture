using AselDevBlazor.Application.Common.Interfaces.AI;
using AselDevBlazor.Domain.Entities.AIModels;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using System.Text.Json;

namespace AselDevBlazor.Application.Features.AI;

public class AIChatService : IAIChatService
{
    private readonly OpenAISettings _settings;
    private readonly ChatClient _client;

    public AIChatService( IOptions<OpenAISettings> options)
    {
            _settings = options.Value;
            _client = new ChatClient(_settings.Model, _settings.ApiKey);
    }

    public List<ChatMessage> InitializeConversation(string? systemOverride = null)
    {
        return new List<ChatMessage>
        {
            new SystemChatMessage(systemOverride ?? _settings.SystemMessage)
        };
    }

    public async IAsyncEnumerable<string> StreamResponseAsync(List<ChatMessage> messages)
    {
        var stream = _client.CompleteChatStreamingAsync(messages);

        await foreach (var response in stream)
        {
            foreach (var content in response.ContentUpdate)
            {
                if (!string.IsNullOrEmpty(content.Text))
                {
                    yield return content.Text;
                }
            }
        }
    }

    public async Task<ChatIntent> DetectIntentAsync(
     string userMessage,
     List<ChatMessage>? conversationHistory = null)
    {
        var intentPrompt = @"
You are an intent classifier for an HR Attendance Management System.
Analyze the user message AND the conversation history to understand the full context.

IMPORTANT CONTEXT RULES:
- If the user refers to 'him', 'her', 'his', 'their', 'that employee', 'the same person'
  — look back at the conversation history to find who they are referring to
- If the user gives a short follow-up like 'Department', 'Leave balance', 'His ID'
  — treat it as a continuation of the previous question
- If an employee ID or name was mentioned earlier in the conversation,
  carry it forward into the current parameters if still relevant
- Never treat a follow-up message as unrelated just because it is short

Available intents:
- attendance   → attendance logs, absences, tardiness, overtime
- leave        → leave balance, leave filing, leave status
- ehr          → employee details, profile, department, position
- manhour      → working hours, total hours rendered
- general_hr   → HR related but no specific data needed
               → Also used when user requests to calculate, summarize, or analyze
                 HR data already provided in conversation history
               → Examples: 'calculate attendance rate', 'what is his total hours',
                 'summarize the data', 'compute overtime', 'how many days absent'
               → If attendance or HR data exists in conversation history and user
                 asks to calculate or analyze it — always classify as general_hr
               → Do NOT mark as unknown just because the word 'calculate' is used
- unknown      → not HR related at all

Parameter extraction rules:
- Extract EmployeeId if a code or ID number is mentioned (current or from history)
- Extract EmployeeName if a person's name is mentioned (current or from history)
- When extracting EmployeeName, ALWAYS strip honorifics and titles — extract the name only
- Honorifics to strip: Mr., Mrs., Ms., Miss, Sir, Ma'am, Madam, Dr., Prof., Engr., Atty.,
  -san, -kun, -chan, -sama (Japanese), Bro, Sis, Boss, Kuya, Ate (Filipino informal)
- Examples of stripping:
  → 'Mr. Juan dela Cruz' → EmployeeName: 'Juan dela Cruz'
  → 'Sir John'           → EmployeeName: 'John'
  → 'Ms. Santos'         → EmployeeName: 'Santos'
  → 'Ma'am Reyes'        → EmployeeName: 'Reyes'
  → 'Tanaka-san'         → EmployeeName: 'Tanaka'
  → 'Kuya Ramon'         → EmployeeName: 'Ramon'
  → 'Ate Maria'          → EmployeeName: 'Maria'
  → 'Dr. Cruz'           → EmployeeName: 'Cruz'
- EmployeeId and EmployeeName are mutually exclusive — extract only one per message
- Extract Department, DateFrom, DateTo if mentioned
- DateFrom and DateTo must be in YYYY-MM-DD format

Intent-specific clarification rules:
- attendance intent → EmployeeId is REQUIRED. If missing, set NeedsClarification to true and ask for Employee ID.
                    → Do NOT accept a name as substitute for EmployeeId in attendance.
                    → DateFrom and DateTo are optional — the Orchestrator handles date confirmation.
                    → If user says 'yes', 'yes today', 'today', 'just today' as a follow-up
                      to a date confirmation — set DateFrom and DateTo to today's date.
                    → If user says 'this week' — DateFrom = Monday of current week, DateTo = today.
                    → If user says 'last week' — DateFrom = last Monday, DateTo = last Sunday.
                    → If user says 'this month' — DateFrom = 1st of current month, DateTo = today.
                    → If user says 'last month' — DateFrom = 1st of last month, DateTo = last day of last month.
                    → If user provides a specific range like 'April 1 to April 7' — extract as DateFrom and DateTo.
                    → If user provides a specific month like 'April' — DateFrom = April 1, DateTo = April 30.
- ehr intent       → EmployeeId OR EmployeeName OR Department is enough. No strict requirement.
- leave intent     → EmployeeId or EmployeeName is enough.
- manhour intent   → EmployeeId or Department is enough.
- general_hr       → No parameters required. Never set NeedsClarification to true for general_hr.

General clarification rule:
- If a required parameter is still missing after checking history, set NeedsClarification to true
- Respond ONLY in valid JSON, no explanation, no markdown

JSON format:
{
  ""Intents"": [""ehr""],
  ""Parameters"": {
    ""EmployeeId"": null,
    ""EmployeeName"": null,
    ""Department"": null,
    ""DateFrom"": null,
    ""DateTo"": null,
    ""Keyword"": null
  },
  ""IsHRRelated"": true,
  ""NeedsClarification"": false,
  ""ClarificationQuestion"": null
}

Examples:
- 'Show attendance of 01026791' → EmployeeId: '01026791', Intent: attendance, NeedsClarification: false
- 'Check attendance of Juan' → Intent: attendance, NeedsClarification: true, ClarificationQuestion: 'Please provide the Employee ID to check attendance records.'
- 'Show me details of Juan' → EmployeeName: 'Juan', Intent: ehr, NeedsClarification: false
- 'What department is EMP001 in?' → EmployeeId: 'EMP001', Intent: ehr, NeedsClarification: false
- 'List all IT employees' → Department: 'IT', Intent: ehr, NeedsClarification: false
- 'What is the leave policy?' → Intent: general_hr, NeedsClarification: false
- 'Can you calculate his attendance rate' → Intent: general_hr, IsHRRelated: true, NeedsClarification: false
- 'Please calculate attendance rate based on the data you gave' → Intent: general_hr, IsHRRelated: true, NeedsClarification: false
- 'Summarize the attendance data' → Intent: general_hr, IsHRRelated: true, NeedsClarification: false
- 'Show details of Sir Juan' → EmployeeName: 'Juan', Intent: ehr, NeedsClarification: false
- 'Check details of Ms. Santos' → EmployeeName: 'Santos', Intent: ehr, NeedsClarification: false
- 'Who is Tanaka-san?' → EmployeeName: 'Tanaka', Intent: ehr, NeedsClarification: false
";

        // ── Build messages: system prompt + last 6 history + current ─────
        // We only send the last 6 messages to keep token usage low
        var messages = new List<ChatMessage>
    {
        new SystemChatMessage(intentPrompt)
    };

        // Inject recent conversation history so AI understands context
        if (conversationHistory is not null && conversationHistory.Count > 0)
        {
            var recentHistory = conversationHistory
                .Skip(Math.Max(0, conversationHistory.Count - 6))
                .ToList();

            messages.AddRange(recentHistory);
        }

        // Add current user message
        messages.Add(new UserChatMessage(userMessage));

        var response = await _client.CompleteChatAsync(messages);
        var rawJson = response.Value.Content[0].Text;

        try
        {
            var intent = JsonSerializer.Deserialize<ChatIntent>(rawJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (intent is null) return FallbackIntent(userMessage);

            intent.RawUserMessage = userMessage;
            return intent;
        }
        catch
        {
            return FallbackIntent(userMessage);
        }
    }

    // 🆕 Safe fallback if intent detection fails
    private static ChatIntent FallbackIntent(string userMessage) => new()
    {
        Intents = new List<string> { "unknown" },
        Parameters = new ChatIntentParameter(),
        IsHrRelated = false,
        NeedsClarification = false,
        ClarificationQuestion = null,
        RawUserMessage = userMessage
    };



    // ── Final response using real DB data ────────────────────────────

    public async IAsyncEnumerable<string> BuildResponseAsync(
        List<ChatMessage> history,
        ChatContextPayload payload)
    {
        var dataContext = BuildDataContext(payload);

        // Inject data as a system message just before responding
        var messages = new List<ChatMessage>(history)
        {
            new SystemChatMessage(dataContext),
            new UserChatMessage(payload.RawUserMessage)
        };

        var stream = _client.CompleteChatStreamingAsync(messages);
        await foreach (var response in stream)
        {
            foreach (var content in response.ContentUpdate)
            {
                if (!string.IsNullOrEmpty(content.Text))
                    yield return content.Text;
            }
        }
    }

    // ── Private helpers ──────────────────────────────────────────────

    private static string BuildDataContext(ChatContextPayload payload)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("The following is real data retrieved from the HR system.");
        sb.AppendLine("Base your response strictly on this data. Do not fabricate any information.");
        sb.AppendLine();

        // ── eHR data ─────────────────────────────────────────────────────
        if (payload.Employees?.Any() == true)
        {
            sb.AppendLine("EMPLOYEE DATA:");
            sb.AppendLine(JsonSerializer.Serialize(payload.Employees, new JsonSerializerOptions
            {
                WriteIndented = false
            }));
            sb.AppendLine();
        }

        // ── Attendance summary ────────────────────────────────────────────
        if (payload.AttendanceSummaryF is not null)
        {
            var s = payload.AttendanceSummaryF;
            sb.AppendLine("ATTENDANCE SUMMARY:");
            sb.AppendLine($"Period: {s.DateFrom} to {s.DateTo}");
            sb.AppendLine($"Total Days: {s.TotalDays}");
            sb.AppendLine($"Present: {s.PresentDays} | Absent: {s.AbsentDays} | Late: {s.LateDays} | On Leave: {s.OnLeaveDays}");
            sb.AppendLine($"Total Regular Hours: {s.TotalRegularHours} | Total Overtime Hours: {s.TotalOvertimeHours}");
            if (s.HasMoreRecords)
                sb.AppendLine($"Note: Only showing first 10 of {s.TotalRecordCount} records.");
            sb.AppendLine();
            sb.AppendLine("IMPORTANT INSTRUCTION: The daily breakdown is already displayed as a table in the UI.");
            sb.AppendLine("DO NOT repeat or list the daily breakdown in your text response.");
            sb.AppendLine("Only provide a brief summary paragraph based on the numbers above.");
        }

        // ── Leave data ────────────────────────────────────────────────────
        // if (payload.LeaveRecords?.Any() == true) { ... }

        // ── No data fallback ─────────────────────────────────────────────
        if (!payload.HasData)
        {
            sb.AppendLine("No data was found matching the user's query.");
            sb.AppendLine("Inform the user politely that no matching records were found.");
        }

        return sb.ToString();
    }
}
