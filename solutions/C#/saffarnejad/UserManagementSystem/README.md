# User Document Background Processing System

A user management system with background job processing for document handling, notifications, and cleanup operations.

## Features

- User registration with document upload
- Background job processing using Hangfire
- Delayed document processing (30-second delay)
- Automatic retry policies (max 2 retries: 5min, 10min)
- Nightly cleanup job (runs at 00:00 daily)
- Real-time job monitoring via Hangfire Dashboard
- Modular and testable architecture

## Prerequisites

- .NET 6+ SDK
- SQL Server (or SQLite for development)
- Visual Studio 2022+ or VS Code

## Setup Instructions

### 1. Clone and Navigate
```bash
git clone https://github.com/Saffarnejad/user-document-background-processing.git
cd solutions/C#/saffarnejad/UserManagementSystem