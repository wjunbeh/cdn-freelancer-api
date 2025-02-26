# cdn-freelancer-api

Technologies Used
- ASP.NET Core
- Entity Framework Core
- SQL Server
- IMemoryCache (for caching)
- Pagination
- Error Handling


API Endpoints
User Endpoints:
1. GET
-> /api/users
-> Get paginated list of users

2. GET
-> /api/users/{id}
-> Get user by ID

3. POST
-> /api/users
-> Register a new user

4. PUT
-> /api/users/{id}
-> Update user details

5. DELETE
-> /api/users/{id}
-> Soft delete a user

6. PATCH
-> /api/users/{id}/change-password
-> Change user password


Caching Implementation:
- The API implements in-memory caching for optimized performance:
 1. User List (GET /api/users)
 2. Single User (GET /api/users/{id})

- Cache expires in 5 minutes.
- Cache is cleared automatically on user updates, deletions, or registrations.