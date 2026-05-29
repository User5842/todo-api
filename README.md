# Todo API

# Database Entities

## User

Email: string
Id: int
Name: string
PasswordHash: string
Tasks: ICollection<TodoItem>

## TodoItem

Details: string
Id: int
Status: string
User: User
UserId: int

# DTOs

## RegisterUserRequest

Email: string
Name: string
Password: string

## RegisterUserResponse

Email: string
Name: string

## LoginUserRequest

Email: string
Name: string
Password: string

# LoginUserResponse

Email: string
Name: string
Token: string

# CreateTaskRequest

Details: string

# CreatedTaskResponse

Details: string
Id: string
Status: string

# API Endpoints

## Register user

POST /register HTTP/1.1
Accept: application/json

HTTP/1.1 201 Created
Content-Type: application/json
Location: /users/me

## Get user profile

GET /users/me HTTP/1.1
Accept: application/json
Host: localhost

HTTP/1.1 200 OK
Content-Type: application/json

{
    "Email": "negron.a.rafael@gmail.com",
    "Id": 1,
    "Name": "Rafael Negron"
}

HTTP/1.1 401 Unauthorized
Content-Type: application/json

{
    "Message": "Unauthorized from accessing this resource"
}

## Create a new task

POST /tasks HTTP/1.1
Accept: application/json
Host: localhost

{
    "Details": "Grab milk at the store",
    "Status": "Not started",
    // "User": (does this come from token claims?)
}

HTTP/1.1 201 Created
Content-Type: application/json
Location: /tasks/1

{
    "Details": "Grab milk at the store",
    "Id": 1,
    "Status": "Not started"
}

B. Receive CreateTaskRequest
C. Read UserId from claims
G. If UserId claim is missing/invalid, return 401 Unauthorized
D. Create TodoItem entity
E. Set TodoItem.UserId to the claim user ID
A. Save task to database
F. Return 201 Created with task response



