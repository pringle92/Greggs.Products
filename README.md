# Greggs.Products

## Introduction
Hello and welcome to the Greggs Products repository!

## The Solution
This solution provides a scalable, robust, and high-performance API for Greggs products.

It is built using a clean, decoupled architecture adhering to SOLID principles:
* **API (Controller):** Handles request validation and delegation.
* **Service Layer:** Contains core business logic, decoupled via interfaces.
* **Data Access Layer:** Manages data retrieval, decoupled via a generic interface.

### Features Implemented:
* **Asynchronous Pipeline:** The entire stack uses `async/await` for high scalability.
* **Strategy Pattern:** Currency conversion is implemented using the Strategy Pattern, making it fully extensible (Open/Closed Principle).
* **Paged Results:** The API returns rich `PagedResult<T>` objects with metadata.
* **Global Error Handling:** A custom middleware provides robust, centralised exception handling.
* **Caching:** `IMemoryCache` is used to cache data access results for improved performance.

---

## API Documentation

The API exposes a single endpoint for retrieving products.

### Get Products

Retrieves a paginated list of products, with prices converted to a specified currency.

* **URL:** `GET /product`
* **Method:** `GET`
* **Produces:** `application/json`

### Query Parameters

| Parameter | Type | Required | Default | Description |
| :--- | :--- | :--- | :--- | :--- |
| `pageStart` | `integer` | No | `0` | The 0-based index of the page to retrieve. |
| `pageSize` | `integer` | No | `5` | The number of items to retrieve per page. (Max: 100) |
| `currency` | `string` | No | `GBP` | The target currency. Supported values: `GBP`, `EUR`. |

### Example Request

`GET /product?pageStart=0&pageSize=2&currency=EUR`

### Example Success Response (200 OK)

The response is wrapped in a `PagedResult` object, providing full pagination metadata.

```json
{
  "items": [
    {
      "name": "Sausage Roll",
      "price": 1.11,
      "currency": "EUR"
    },
    {
      "name": "Vegan Sausage Roll",
      "price": 1.22,
      "currency": "EUR"
    }
  ],
  "totalCount": 8,
  "pageStart": 0,
  "pageSize": 2
}

### Example Error Response (400 Bad Request)

Returned if any of the query parameters fail validation.

```json
"Returned if any of the query parameters fail validation."

Example Error Response (500 Internal Server Error)

Returned by the global exception handler if an unexpected error occurs.

```json
{
  "title": "An internal server error occurred.",
  "detail": null,
  "status": 500
}

---

# ***Original Challenge (Completed)***
The user stories below have been implemented and enhanced.

### User Story 1 (*** COMPLETED ***)
**As a** Greggs Fanatic<br/>
**I want to** be able to get the latest menu of products rather than the random static products it returns now<br/>
**So that** I get the most recently available products.

**Given** a previously implemented data access layer<br/>
**When** I hit a specified endpoint to get a list of products<br/>
**Then** a list or products is returned that uses the data access implementation rather than the static list it current utilises

### User Story 2 (*** COMPLETED ***)
**As a** Greggs Entrepreneur<br/>
**I want to** get the price of the products returned to me in Euros<br/>
**So that** I can set up a shop in Europe as part of our expansion

**Given** an exchange rate of 1GBP to 1.11EUR<br/>
**When** I hit a specified endpoint to get a list of products<br/>
**Then** I will get the products and their price(s) returned
