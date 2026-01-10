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
  - Get all courses / get course by id

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

> ✅ Add your ERD/schema image here:
> 
> ``
>
> (Replace the path with your actual image path.)

---

## Prerequisites

- SQL Server installed and running
- .NET SDK installed
- Visual Studio (recommended)

---

## Database Setup

1. Create a database (example):
   - `OnlineExaminationSystem`

2. Run your SQL scripts:
   - Tables + constraints
   - Stored Procedures
   - Seed/sample data (optional)

3. Confirm your stored procedures exist:
   - `sp_LoginWithProfile`
   - `sp_AddStudent`, `sp_StudentUpdateMyProfile`
   - `sp_AdminAddInstructor`, `sp_UpdateInstructor`, `sp_DeleteInstructor`, `sp_GetAllInstructors`, `sp_GetInstructorById`, `sp_GetInstructorsByBranch`, `sp_GetInstructorsByTrack`, `sp_GetInstructorsCourses`
   - `sp_AddCourse`, `sp_UpdateCourse`, `sp_DeleteCourse`, `sp_GetAllCourses`, `sp_GetCourseById`

---

## Configuration

Update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=OnlineExaminationSystem;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "YOUR_SECRET_KEY_HERE_very_long",
    "Issuer": "OnlineExaminationSystem",
    "Audience": "OnlineExaminationSystem.Client",
    "ExpiresMinutes": "60"
  }

---

## Collaborators
  - `Eman Shehata — emanshehata258@gmail.com`
  - `Ash Rawda — ashrawda@gmail.com`
 

