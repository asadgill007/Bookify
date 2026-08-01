using Bookify.Application.Common;
using Bookify.Application.Interfaces;
using Bookify.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bookify.Infrastructure.Services;

/// <summary>
/// Rule-based customer chatbot with a clear extension point for real AI
/// providers. When <see cref="ChatAiSettings.ApiKey"/> is empty (demo/dev mode)
/// the service answers FAQ-type questions, helps find businesses/services and
/// checks booking status using the live database. When an API key is
/// configured the same interface can be backed by OpenAI/Groq/Gemini — the
/// request shape (full conversation history) is provider-agnostic.
/// </summary>
public class RuleBasedChatAiService : IChatAiService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RuleBasedChatAiService> _logger;
    private readonly ChatAiSettings _settings;

    public RuleBasedChatAiService(
        IUnitOfWork unitOfWork,
        ILogger<RuleBasedChatAiService> logger,
        IOptions<ChatAiSettings> settings)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<Result<ChatAiReply>> AskAsync(ChatAiRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var latest = request.Messages.LastOrDefault();
            if (latest == null || string.IsNullOrWhiteSpace(latest.Content))
            {
                return Result<ChatAiReply>.Failure("Message cannot be empty.", "EMPTY_MESSAGE");
            }

            // Extension point: a real AI provider would be invoked here when an
            // API key is configured. The conversation history is already in a
            // provider-neutral shape (role + content turns).
            if (!string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                _logger.LogWarning(
                    "[CHAT AI] External provider configured ({Provider}) but not wired; using rule-based fallback. " +
                    "Implement the provider call in RuleBasedChatAiService.AskAsync to enable.",
                    _settings.Provider);
            }

            var reply = await GenerateReplyAsync(request, latest.Content, cancellationToken);

            return Result<ChatAiReply>.Success(new ChatAiReply
            {
                Content = reply,
                UsedExternalProvider = false
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI chat failed: {Message}", ex.Message);
            return Result<ChatAiReply>.Failure("Chat assistant is temporarily unavailable. Please try again.");
        }
    }

    private async Task<string> GenerateReplyAsync(
        ChatAiRequest request,
        string message,
        CancellationToken cancellationToken)
    {
        var text = message.ToLowerInvariant();

        // ── Booking status (needs a logged-in user) ──
        if ((text.Contains("booking") || text.Contains("appointment") || text.Contains("status") ||
             text.Contains("upcoming") || text.Contains("next")) &&
            (text.Contains("my") || text.Contains("status") || text.Contains("check") || text.Contains("where")))
        {
            return await BookingStatusReplyAsync(request, text, cancellationToken);
        }

        // ── Find business / service ──
        if (text.Contains("find") || text.Contains("search") || text.Contains("looking for") ||
            text.Contains("recommend") || text.Contains("near me") || text.Contains("help me find"))
        {
            return await FindBusinessReplyAsync(text, cancellationToken);
        }

        // ── Cancellation / reschedule ──
        if (text.Contains("cancel") || text.Contains("reschedule") || text.Contains("reschedule my"))
        {
            return "You can cancel or reschedule an appointment from the Appointments tab up to 24 hours " +
                   "before the scheduled time (some providers allow less — check the business's cancellation " +
                   "policy). Tap the appointment, then choose Cancel or Reschedule. If you need more help, " +
                   "open Contact Support from Settings.";
        }

        // ── Payments / refund ──
        if (text.Contains("pay") || text.Contains("payment") || text.Contains("refund") || text.Contains("charge"))
        {
            return "Bookify handles payments securely when you confirm a booking. Refunds follow each " +
                   "business's cancellation policy and are usually processed back to your original payment " +
                   "method within 5–10 business days. For a specific charge, use Report a Problem on that " +
                   "appointment or contact support.";
        }

        // ── Account / password ──
        if (text.Contains("password") || text.Contains("login") || text.Contains("sign in") || text.Contains("account"))
        {
            return "To reset your password: tap \"Forgot Password?\" on the login screen and follow the " +
                   "email link. You can update your profile details and language/currency preferences in " +
                   "Settings → Edit Profile.";
        }

        // ── Contact support ──
        if (text.Contains("support") || text.Contains("contact") || text.Contains("human") || text.Contains("agent") || text.Contains("help"))
        {
            return "You can reach our team through Contact Support in Settings (we usually reply within " +
                   "24 hours), by email at support@bookify.app, or by phone at +1 (555) 010-2030 on " +
                   "weekdays 9am–6pm.";
        }

        // ── Provider / list your business ──
        if (text.Contains("provider") || text.Contains("list my business") || text.Contains("own business") ||
            text.Contains("become") || text.Contains("sell"))
        {
            return "To list your business: create an account and choose \"List your business\", then follow " +
                   "the onboarding steps (category, hours, services, staff, photos). Once your listing is " +
                   "complete it goes live automatically and appears in customer search.";
        }

        // ── Greeting ──
        if (text.Contains("hi") || text.Contains("hello") || text.Contains("hey") || text.Contains("salam") || text.Contains("salaam"))
        {
            return "Hello! 👋 I'm Bookify's assistant. I can help you find businesses and services, check " +
                   "your booking status, or answer questions about bookings, payments, and your account. " +
                   "What would you like to know?";
        }

        // ── Thanks ──
        if (text.Contains("thank") || text.Contains("thanks") || text.Contains("shukria") || text.Contains("shukriya"))
        {
            return "You're welcome! 😊 Is there anything else I can help you with?";
        }

        // ── Default ──
        return "I can help with:\n" +
               "• Finding businesses & services (e.g. \"find a salon near me\")\n" +
               "• Checking your booking status (e.g. \"check my booking status\")\n" +
               "• Cancelling or rescheduling appointments\n" +
               "• Payments, refunds, passwords & account questions\n\n" +
               "Just ask in your own words, or tap Contact Support in Settings if you need a human.";
    }

    private async Task<string> BookingStatusReplyAsync(
        ChatAiRequest request,
        string text,
        CancellationToken cancellationToken)
    {
        if (!request.UserId.HasValue)
        {
            return "To check your booking status you'll need to sign in first. Once logged in, I can tell " +
                   "you your upcoming appointments.";
        }

        var appointments = await _unitOfWork.Appointments.GetUserAppointmentsAsync(
            request.UserId.Value,
            isCustomer: true,
            statusFilter: null,
            from: null,
            to: null,
            page: 1,
            pageSize: 10,
            cancellationToken);

        if (appointments.Count == 0)
        {
            return "You don't have any bookings yet. Search for a business and book your first appointment!";
        }

        var upcoming = appointments
            .Where(a => a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Pending)
            .OrderBy(a => a.StartTime)
            .Take(3)
            .ToList();

        if (upcoming.Count == 0)
        {
            return $"You have {appointments.Count} past bookings. Your latest one is in " +
                   $"{appointments.OrderByDescending(a => a.StartTime).First().StartTime:MMM d} — " +
                   "you can leave a review for completed appointments from the Appointments tab.";
        }

        var lines = upcoming.Select(a =>
            $"• {a.StartTime:ddd, MMM d 'at' HH:mm} — booking {a.BookingReference} ({a.Status})").ToList();
        return $"Here are your next bookings:\n{string.Join("\n", lines)}\n\n" +
               "You can view or manage these from the Appointments tab.";
    }

    private async Task<string> FindBusinessReplyAsync(string text, CancellationToken cancellationToken)
    {
        // Extract a search keyword from common phrasing.
        var keyword = text
            .Replace("find", "")
            .Replace("search", "")
            .Replace("looking for", "")
            .Replace("help me", "")
            .Replace("recommend", "")
            .Replace("a", "")
            .Replace("an", "")
            .Replace("the", "")
            .Replace("near me", "")
            .Replace("please", "")
            .Trim();

        if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 3)
        {
            keyword = text.Contains("salon") || text.Contains("hair") || text.Contains("barber")
                ? "salon"
                : text.Contains("spa") || text.Contains("massage")
                    ? "spa"
                    : text.Contains("dentist") || text.Contains("dental")
                        ? "dental"
                        : text.Contains("gym") || text.Contains("fitness") || text.Contains("train")
                            ? "fitness"
                            : null;
        }

        if (string.IsNullOrWhiteSpace(keyword))
        {
            return "Tell me what you're looking for and I'll find it — for example \"find a salon near me\" " +
                   "or \"recommend a spa\".";
        }

        var businesses = await _unitOfWork.Businesses.SearchAsync(
            keyword,
            categoryId: null,
            latitude: null,
            longitude: null,
            radiusKm: null,
            minRating: null,
            minPrice: null,
            maxPrice: null,
            isVerified: true,
            sortBy: "rating",
            sortDirection: "desc",
            page: 1,
            pageSize: 3,
            cancellationToken);

        if (businesses.Count == 0)
        {
            return $"I couldn't find any businesses matching \"{keyword}\" right now. Try a different " +
                   "search term or check the Categories tab.";
        }

        var lines = businesses.Select(b =>
            $"• {b.Name} — {b.City} (★ {b.AverageRating:0.0}, {b.TotalReviews} reviews)").ToList();
        return $"Here are top matches for \"{keyword}\":\n{string.Join("\n", lines)}\n\n" +
               "Tap one in Search to see services and book.";
    }
}

public class ChatAiSettings
{
    public string Provider { get; set; } = "OpenAI";
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4o-mini";
}
