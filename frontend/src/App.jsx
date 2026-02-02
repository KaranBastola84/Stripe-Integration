import React, { useState, useEffect } from "react";
import { loadStripe } from "@stripe/stripe-js";
import { Elements } from "@stripe/react-stripe-js";
import CheckoutForm from "./components/CheckoutForm";
import "./App.css";

function App() {
  const [stripePromise, setStripePromise] = useState(null);

  useEffect(() => {
    // Fetch publishable key from backend
    fetch(`${import.meta.env.VITE_API_URL}/api/payment/config`)
      .then((res) => res.json())
      .then((data) => {
        setStripePromise(loadStripe(data.publishableKey));
      })
      .catch((error) => {
        console.error("Error fetching Stripe config:", error);
      });
  }, []);

  return (
    <div className="App">
      <div className="container">
        <h1>Stripe Payment Integration</h1>
        <p className="subtitle">Complete your payment securely</p>

        {stripePromise ? (
          <Elements stripe={stripePromise}>
            <CheckoutForm />
          </Elements>
        ) : (
          <div className="loading">Loading payment form...</div>
        )}
      </div>
    </div>
  );
}

export default App;
