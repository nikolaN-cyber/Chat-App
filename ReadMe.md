# Chat App

Chat app is a web application that allows users to chat in real time, either in private or group conversations. It is implemented in modern technologies such as **Angular v21** (signals, signalStore, RxJS, Angulat Materials, Bootstrap) for client side, **.NET 10** (EF Core 10, Clean architecture) for server side and **SQL Server** as a database, that's running as a docker container.

# Technologies used

## Frontend

- Angular (Standalone Components, Signals)
- NgRx SignalStore (State Management)
- Angular Materials (UI Components)
- Bootstrap (Grid system and spacing)

## Backend

- ASP.NET Core WebApi (WebApi Application)
- Class Libraries (Clean Architecture: Infrastructure, Domain, Application)
- EF Core (SQL Server ORM Tool)
- JWT (Bearer token)

# Installation and Running

## Database

1. Option A - With Docker Desktop, open services folder, add .env file with DB_PASSWORD=your_password and run "docker-compose up" from services folder.
2. Option B - Install wsl and Ubuntu system, go to Linux and Home folder in file explorer, copy services folder there. Open Ubuntu terminal, write "cd services" command, and then "docker compose up"

## Backend

1. Enter root server folder 'event-management-server'.
2. Right click on webapi project folder, open Manage User Secrets add DefaultConnection for ConnectionStrings ex: "Server=(localdb)\\mssqllocaldb;Database=your_db_name;Trusted_Connection=True;MultipleActiveResultSets=true", add key for jwt longer than 32 characters.
3. Apply migrations, dotnet ef database update --project Infrastructure --startup-project event-management-server.

## Frontend

1. Enter root client folder 'event-management-server'
2. run npm install
3. Run application with command ng serve

# Key functionalities

- User registration and login
- User profile check
- All conversations listing, both private and groups
- Sending messages
- Creating group chats, add users, delete chat (only for admins)
- Sending email notifications after message received
- Group chat invitations
- Show status of users

