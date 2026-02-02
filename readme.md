# Stripe Payment Integration

A full-stack payment integration application using Stripe, built with .NET 9.0 backend and React + Vite frontend.

## 🚀 Features

- **Secure Payment Processing**: Integration with Stripe Payment Intents API
- **Modern Frontend**: React 19 with Stripe Elements for PCI-compliant card handling
- **RESTful API**: ASP.NET Core Web API with Swagger documentation
- **CORS Enabled**: Ready for development and production deployments
- **Environment Configuration**: Secure API key management

## 📋 Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js](https://nodejs.org/) (v18 or higher)
- [Stripe Account](https://stripe.com/) with API keys

## 🛠️ Project Structure

```
Stripe-Integration/
├── backend/                    # .NET 9.0 Web API
│   ├── Controllers/
│   │   └── PaymentController.cs
│   ├── Program.cs
│   ├── StripeBackend.csproj
│   ├── appsettings.json
│   └── appsettings.Development.json
├── frontend/                   # React + Vite
│   ├── src/
│   │   ├── components/
│   │   │   └── CheckoutForm.jsx
│   │   ├── App.jsx
│   │   └── main.jsx
│   ├── package.json
│   └── vite.config.js
└── readme.md
```

## ⚙️ Setup Instructions

### Backend Setup

1. **Navigate to the backend directory:**

   ```bash
   cd backend
   ```

2. **Configure Stripe API Keys:**

   Create or update `appsettings.Development.json`:

   ```json
   {
     "Stripe": {
       "SecretKey": "sk_test_your_secret_key_here",
       "PublishableKey": "pk_test_your_publishable_key_here"
     }
   }
   ```

3. **Restore dependencies and run:**

   ```bash
   dotnet restore
   dotnet run
   ```

4. **Access Swagger UI:**
   Navigate to `https://localhost:5001/swagger` to view API documentation

### Frontend Setup

1. **Navigate to the frontend directory:**

   ```bash
   cd frontend
   ```

2. **Install dependencies:**

   ```bash
   npm install
   ```

3. **Configure environment variables:**

   Create a `.env` file in the frontend directory:

   ```env
   VITE_API_URL=https://localhost:5001
   ```

4. **Run the development server:**

   ```bash
   npm run dev
   ```

   The frontend will be available at `http://localhost:5173`

## 🔌 API Endpoints

### `GET /api/payment/config`

Retrieves the Stripe publishable key for frontend initialization.

**Response:**

```json
{
  "publishableKey": "pk_test_..."
}
```

### `POST /api/payment/create-payment-intent`

Creates a payment intent for processing a payment.

**Request Body:**

```json
{
  "amount": 2000,
  "currency": "usd"
}
```

**Response:**

```json
{
  "clientSecret": "pi_..."
}
```

## 🧪 Testing

Use Stripe's [test card numbers](https://stripe.com/docs/testing) for testing:

- **Success**: `4242 4242 4242 4242`
- **Decline**: `4000 0000 0000 0002`
- Use any future expiration date and any 3-digit CVC

## 📦 Technologies Used

### Backend

- ASP.NET Core 9.0
- Stripe.NET SDK
- Swagger/OpenAPI

### Frontend

- React 19
- Vite
- @stripe/stripe-js
- @stripe/react-stripe-js
- Axios

## 🚀 Deployment

### Backend

Configure production Stripe keys in your hosting environment variables and update CORS policy in [backend/Program.cs](backend/Program.cs) to restrict allowed origins.

### Frontend

Build the production bundle:

```bash
cd frontend
npm run build
```

Deploy the `dist` folder to your preferred hosting service and update the `VITE_API_URL` environment variable.

## 📝 License

This project is for educational and development purposes.

## 🤝 Contributing

Feel free to submit issues and enhancement requests!
