# Online Examination System (ITI) — Backend API (DB First)

A .NET Web API for an ITI Online Examination System using **DB-First** (SQL Server + Stored Procedures) and **JWT Authentication**.  
The database is the source of truth: business logic is implemented mainly through **Stored Procedures**, and the API calls them using **Dapper**.

---

## Features

- **Single Login** for Admin / Instructor / Student
- **JWT Authentication + Role-based Authorization**
- **Students**
  - Register (creates account only, no token)
  - Login (returns token)
  - Update own profile (FullName / Email / Password) using token
- **Instructors**
  - Admin adds instructors
  - Instructor can fetch own courses by token
  - Admin can fetch instructor courses by instructor id
- **Courses**
  - Admin add / update / delete
  - Get all courses
  - Get course by id

---

## Tech Stack

- **.NET Web API**
- **SQL Server**
- **Dapper**
- **JWT Bearer Authentication**
- **Swagger (OpenAPI)**

---

## Project Architecture (DB First)

- Database tables and relationships are defined in SQL Server.
- Core operations are handled by Stored Procedures (SPs).
- API endpoints call SPs via Dapper.
- DTOs are created per API request/response.

### Entity Relationship Diagram (ERD)

![Database Schema]((/OnlineExaminationSystem(1))

> 📌 Place the ERD image inside the project at:  
> `(/OnlineExaminationSystem(1)`

---

## Prerequisites

- SQL Server installed and running
- .NET SDK installed
- Visual Studio 

---

 }


