using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace StripeBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(IConfiguration configuration, ILogger<PaymentController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Get Stripe configuration including publishable key
    /// </summary>
    [HttpGet("config")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetConfig()
    {
        return Ok(new
        {
            publishableKey = _configuration["Stripe:PublishableKey"]
        });
    }

    /// <summary>
    /// Create a payment intent
    /// </summary>
    /// <param name="request">Payment request containing amount and currency</param>
    [HttpPost("create-payment-intent")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentIntentRequest request)
    {
        try
        {
            if (request.Amount <= 0)
            {
                return BadRequest(new { error = "Invalid amount" });
            }

            var options = new PaymentIntentCreateOptions
            {
                Amount = request.Amount, // Amount in cents (e.g., 1000 = $10.00)
                Currency = request.Currency ?? "usd",
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            _logger.LogInformation("Payment intent created: {PaymentIntentId}", paymentIntent.Id);

            return Ok(new
            {
                clientSecret = paymentIntent.ClientSecret,
                paymentIntentId = paymentIntent.Id
            });
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error occurred");
            return StatusCode(500, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment intent");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Webhook endpoint for Stripe events
    /// </summary>
    [HttpPost("webhook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Webhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var stripeSignature = Request.Headers["Stripe-Signature"].ToString();
        var webhookSecret = _configuration["Stripe:WebhookSecret"];

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                stripeSignature,
                webhookSecret
            );

            _logger.LogInformation("Webhook received: {EventType}", stripeEvent.Type);

            // Handle the event
            switch (stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    _logger.LogInformation("✅ Payment succeeded: {PaymentIntentId}", paymentIntent?.Id);
                    // TODO: Handle successful payment (e.g., update database, send confirmation email)
                    break;

                case "payment_intent.payment_failed":
                    var failedPayment = stripeEvent.Data.Object as PaymentIntent;
                    _logger.LogWarning("❌ Payment failed: {PaymentIntentId}", failedPayment?.Id);
                    // TODO: Handle failed payment
                    break;

                case "payment_intent.canceled":
                    var canceledPayment = stripeEvent.Data.Object as PaymentIntent;
                    _logger.LogInformation("Payment canceled: {PaymentIntentId}", canceledPayment?.Id);
                    break;

                default:
                    _logger.LogInformation("Unhandled event type: {EventType}", stripeEvent.Type);
                    break;
            }

            return Ok(new { received = true });
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Webhook signature verification failed");
            return BadRequest(new { error = $"Webhook Error: {ex.Message}" });
        }
    }

    /// <summary>
    /// Get payment intent details by ID
    /// </summary>
    [HttpGet("payment-intent/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPaymentIntent(string id)
    {
        try
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(id);

            return Ok(new
            {
                id = paymentIntent.Id,
                amount = paymentIntent.Amount,
                currency = paymentIntent.Currency,
                status = paymentIntent.Status,
                created = paymentIntent.Created
            });
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error retrieving payment intent");
            return NotFound(new { error = ex.Message });
        }
    }
}

public record CreatePaymentIntentRequest(long Amount, string? Currency = "usd");
