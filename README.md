# PokemonApp

## Overview

This project consists of two parts:
- **Backend API** built using ASP.NET Core Minimal APIs
- **Frontend UI** built with Vue.js

The app allows users to log in and search for Pokémon by ID or name. Results are displayed on the UI.

---

## Getting Started

### Prerequisites

- [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- [Node.js and npm](https://nodejs.org/en/download/) (for Vue.js frontend)
- Visual Studio 2022 or Visual Studio Code

---

### Running the Application

#### 1. Run the Backend API

- Open the **Backend API** project in Visual Studio.
- Build and run the project.
- On first run, the SQLite database will be created and migrations applied automatically.
- The user with username `test` and password `test123` is pre-populated. (Note: technically this should be MFA secured, but omitted here for brevity.)
- The API runs locally and exposes endpoints for login and Pokémon search.

#### 2. Run the Frontend UI

- Open the **UI** project in a separate instance of Visual Studio or your preferred code editor.
- Navigate to the frontend folder and run:
 npm install npm run serve

- The Vue.js app will start on a local development server.
- Access the UI in your browser, log in with username `test` and password `test123`.
- Enter the Pokémon ID or name to search.
- Results will be displayed after a successful query.

---

### Notes

- The user data and database migrations are automatically handled on first API run. If you encounter issues, please report them.
- CORS is enabled for all origins **only** for development/testing purposes. This is **not** recommended for production.

---

## Design Decisions and Technologies

- **Minimal APIs** were chosen for the backend because the project is small and has minimal bounded contexts. This allows for quick iteration and a simple codebase.
- Business logic is segregated into Services to maintain separation of concerns.
- This setup also allows easy refactoring to Controllers if the API complexity grows.
- **SQLite** is used as the database to keep setup simple and avoid external dependencies.
- The **frontend** is built in **Vue.js** as per requirements.
- The current UI is functional but could be improved by modularizing components and cleaning up generated boilerplate.
- **CORS** is enabled broadly to facilitate local frontend-backend communication during development.

---

If you have any questions or issues setting up or running the app, please feel free to reach out!



