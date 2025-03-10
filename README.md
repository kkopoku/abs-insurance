# ABS InsuranceService

## Overview
The ABS InsuranceService is designed to manage insurance policies and their components. It allows users to create, update, and retrieve policy details while enforcing business rules such as percentage and flat value constraints. The service uses MongoDB for storage and .NET 8 for backend development.

## Tools
- .NET 8
- NoSQL (MongoDB)
- Postman for API testing

## Current Implementation
The following features have been implemented:

### Policy API
- RESTful API for managing insurance policies, supporting operations like adding, updating, and retrieving policies and components.
- Business logic to ensure that when the percentage value is updated, the flat value is set to 0 and vice versa.
- Secure handling of policy data.

### Business Logic
- Policies can have multiple components.
- A component can have either a percentage or flat value but not both simultaneously.
- Policies are uniquely identified and stored in MongoDB.

### Authentication
- JWT authentication is used to secure endpoints and ensure only authorized users can access policy data.

### Tests
- Unit tests for the Policy API and business logic have been implemented using xUnit and Moq.

## Project Structure
### Insurance API
A web service built using ASP.NET Core that exposes endpoints for managing policies and components while communicating with MongoDB for data persistence.

#### Key Features:
- Add a policy.
- Update policy details.
- Update policy components with business logic enforcement.
- Retrieve a specific policy by ID.
- List all policies.
- Delete a policy.
- Authenticate users with JWT.

### Directory Overview
```
src/InsuranceService
├── Controllers           // API controllers for handling HTTP requests
├── Constants             // Constants for fetching non-changing values
├── Models                // Entity models (Policy, PolicyComponent)
├── Enums                 // Enumeration types
├── Services              // Business logic for processing API requests
├── DTOs                  // Data Transfer Objects for structured input/output
├── Repositories          // Data access interfaces and implementations
├── Configurations        // Configuration files for JWT, MongoDB, etc.
├── Program.cs            // Entry point for the API
└── appsettings.json      // Configuration settings (e.g., MongoDB, JWT)

InsuranceService.Tests
A test project that contains unit tests for the API and business logic.
```

## Workflow
### Add a Policy
1. User sends a request to add a policy.
2. The policy details are validated and stored in MongoDB.

### Update a Policy
1. User sends a request to update a policy.
2. If a policy component's percentage is updated, its flat value is set to 0 (and vice versa).
3. The policy is updated in MongoDB.

### Retrieve a Policy
1. User sends a request to retrieve a policy by ID.
2. The service checks for authorization and fetches the policy from MongoDB.

### List All Policies
1. User sends a request to list all policies.
2. The service returns a list of available policies.

### Delete a Policies
1. User sends a request to delete a policy.
2. The service a successful response that the policy is deleted, along with its corresponding components.

## Project Setup
### Clone the repository
```sh
git clone git@github.com:kkopoku/insurance-service.git
```

### Navigate to the project directory
```sh
cd Hubtel.Insurance/Hubtel.Insurance.API
```

### Install dependencies
```sh
dotnet restore
```

### Build the project
```sh
dotnet build
```

### Run the API
```sh
dotnet run
```

### Run the tests
```sh
cd ../InsuranceService.Tests
dotnet test
```

## Access the API
The API will be running on `http://localhost:5258` by default or the port you have specified. You can access the API using Postman or any other HTTP client.

## API Documentation
The API documentation can be found [here](https://documenter.getpostman.com/view/23488284/2sAYdoGTev).

