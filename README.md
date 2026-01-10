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

![Database Schema](./onlineExaminationSystem%20(1).png)

*Database schema diagram showing all tables and relationships*

---

## Prerequisites

- SQL Server installed and running
- .NET SDK installed
- Visual Studio (recommended)

---

## Database Setup

1. Create a database named `OnlineExaminationSystem` in SQL Server
2. Run the SQL scripts to create tables, relationships, and stored procedures
3. Execute seed data scripts for initial setup (optional)

Key stored procedures to verify:
- `sp_LoginWithProfile` - Authentication
- `sp_AddStudent`, `sp_StudentUpdateMyProfile` - Student management
- `sp_AdminAddInstructor`, `sp_UpdateInstructor`, `sp_GetAllInstructors` - Instructor management
- `sp_AddCourse`, `sp_UpdateCourse`, `sp_GetAllCourses`, `sp_GetCourseById` - Course management

---

## Collaborators

**Eman Shehata**  
📧 Email: emanshehata258@gmail.com  
🔗 LinkedIn: https://www.linkedin.com/in/emanshehata  

**Ash Rawda**  
📧 Email: ashrawda@gmail.com  
🔗 LinkedIn: https://www.linkedin.com/in/rawda-ashor-abdelhady-168250304  

---

## Notes

- This project follows **DB First** architecture.
- Stored Procedures are the main source of business rules.
- JWT is used for authentication and role-based authorization.
- Developed for **educational purposes (ITI)**.



