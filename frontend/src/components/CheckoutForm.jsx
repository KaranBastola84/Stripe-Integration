import React, { useState } from "react";
import { CardElement, useStripe, useElements } from "@stripe/react-stripe-js";
import axios from "axios";

const CARD_ELEMENT_OPTIONS = {
  style: {
    base: {
      color: "#32325d",
      fontFamily: '"Helvetica Neue", Helvetica, sans-serif',
      fontSmoothing: "antialiased",
      fontSize: "16px",
      "::placeholder": {
        color: "#aab7c4",
      },
    },
    invalid: {
      color: "#fa755a",
      iconColor: "#fa755a",
    },
  },
};

function CheckoutForm() {
  const stripe = useStripe();
  const elements = useElements();

  const [amount, setAmount] = useState("20.00");
  const [currency, setCurrency] = useState("usd");
  const [isProcessing, setIsProcessing] = useState(false);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(false);
  const [paymentIntentId, setPaymentIntentId] = useState(null);

  const handleSubmit = async (event) => {
    event.preventDefault();

    if (!stripe || !elements) {
      return;
    }

    setIsProcessing(true);
    setError(null);

    try {
      // Convert amount to cents
      const amountInCents = Math.round(parseFloat(amount) * 100);

      if (amountInCents < 50) {
        throw new Error("Amount must be at least $0.50 USD");
      }

      // Create payment intent
      const { data } = await axios.post(
        `${import.meta.env.VITE_API_URL}/api/payment/create-payment-intent`,
        {
          amount: amountInCents,
          currency: currency,
        },
      );

      // Confirm card payment
      const { error: stripeError, paymentIntent } =
        await stripe.confirmCardPayment(data.clientSecret, {
          payment_method: {
            card: elements.getElement(CardElement),
          },
        });

      if (stripeError) {
        setError(stripeError.message);
      } else if (paymentIntent.status === "succeeded") {
        setSuccess(true);
        setPaymentIntentId(paymentIntent.id);
      }
    } catch (err) {
      setError(err.response?.data?.error || err.message || "Payment failed");
    } finally {
      setIsProcessing(false);
    }
  };

  const handleNewPayment = () => {
    setSuccess(false);
    setPaymentIntentId(null);
    setAmount("20.00");
    setError(null);
  };

  if (success) {
    return (
      <div className="payment-status success-message">
        <h2>Payment Successful!</h2>
        <p>Your payment has been processed successfully.</p>
        <p>
          <strong>Payment ID:</strong> <code>{paymentIntentId}</code>
        </p>
        <p>
          <strong>Amount:</strong> ${amount} {currency.toUpperCase()}
        </p>
        <button onClick={handleNewPayment} className="new-payment-button">
          Make Another Payment
        </button>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit} className="payment-form">
      <div className="form-group">
        <label htmlFor="amount">Amount (USD)</label>
        <input
          type="number"
          id="amount"
          step="0.01"
          min="0.50"
          value={amount}
          onChange={(e) => setAmount(e.target.value)}
          placeholder="20.00"
          required
        />
      </div>

      <div className="form-group">
        <label htmlFor="currency">Currency</label>
        <input
          type="text"
          id="currency"
          value={currency}
          onChange={(e) => setCurrency(e.target.value)}
          placeholder="usd"
          required
        />
      </div>

      <div className="form-group">
        <label htmlFor="card-element">Card Details</label>
        <div className="card-element">
          <CardElement id="card-element" options={CARD_ELEMENT_OPTIONS} />
        </div>
      </div>

      <button
        type="submit"
        disabled={!stripe || isProcessing}
        className="pay-button"
      >
        {isProcessing ? "Processing..." : `Pay $${amount}`}
      </button>

      {error && <div className="error-message">{error}</div>}

      <div style={{ marginTop: "20px", fontSize: "12px", color: "#666" }}>
        <p>
          <strong>Test Cards:</strong>
        </p>
        <p>Success: 4242 4242 4242 4242</p>
        <p>Declined: 4000 0000 0000 9995</p>
        <p>Use any future date, any 3-digit CVC, and any ZIP</p>
      </div>
    </form>
  );
}

export default CheckoutForm;
