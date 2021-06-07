# Auth

## Registration

### Error 01 

#### Payload

`https://localhost:5001/api/auth/register`

```json
{
  "username": "Test 01",
  "email": "test01@test.copm",
  "password": "secret"
}
```

#### Response

```json
{
  "token": null,
  "success": false,
  "errors": [
    "Passwords must have at least one non alphanumeric character.",
    "Passwords must have at least one digit ('0'-'9').",
    "Passwords must have at least one uppercase ('A'-'Z')."
  ]
}
```

### Error 02

#### Payload


`https://localhost:5001/api/auth/register`

```json
{
  "username": "TestOne",
  "email": "test01@test.copm"
}
```

### Response

#### Error

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "traceId": "00-c0b6975b55239b468932cd45f9cbad3d-86b926b1db4a794b-00",
  "errors": {
    "Password": [
      "The Password field is required."
    ]
  }
}
```

### Success

#### Payload

```json
{
  "username": "TestOne",
  "email": "test01@test.copm",
  "password": "Secret123456!"
}
```

#### Response

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJJZCI6IjQ2ZGRmNWQzLWMzNTMtNGM4Zi1hODRjLWNkOTdjZjE2YWNmZiIsImVtYWlsIjoidGVzdDAxQHRlc3QuY29wbSIsInN1YiI6InRlc3QwMUB0ZXN0LmNvcG0iLCJqdGkiOiIxZWU1ZDIzMC0xNjAzLTQzNDYtYmQ0My1hNGQ1N2M3MTk3YTkiLCJuYmYiOjE2MjMwODE2NjEsImV4cCI6MTYyMzEwMzI2MSwiaWF0IjoxNjIzMDgxNjYxfQ.hlPHZBfDXYUREKjaSBHZdK7gt6TLt7EwGlwvOogv5uo",
  "success": true,
  "errors": null
}
```

## Login

`https://localhost:5001/api/auth/login`

### Success

#### Payload

```json
{
  "email": "test01@test.copm",
  "password": "Secret123456!"
}
```

#### Response

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJJZCI6IjQ2ZGRmNWQzLWMzNTMtNGM4Zi1hODRjLWNkOTdjZjE2YWNmZiIsImVtYWlsIjoidGVzdDAxQHRlc3QuY29wbSIsInN1YiI6InRlc3QwMUB0ZXN0LmNvcG0iLCJqdGkiOiJkMWU4YzU0NS1jYTE3LTRiMWQtODY5Mi1lZjdjYTFjOGQ2ZWIiLCJuYmYiOjE2MjMwODQ3NTIsImV4cCI6MTYyMzEwNjM1MiwiaWF0IjoxNjIzMDg0NzUyfQ.GCsm7SSKU6jZm_EZrhB_8qMq-D6jLxYsmL45wuBq6Rc",
  "success": true,
  "errors": null
}
```

### With Errors

#### Payload

```json
{
  "email": "test01@test.copm",
  "password": "Secret123456!a"
}
```

#### Error Response

```json
{
  "token": null,
  "success": false,
  "errors": [
    "Wrong Password."
  ]
}
```

# Get Todos
``https://localhost:5001/api/todo``
```
// Headers
Authorization = Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJJZCI6IjQ2ZGRmNWQzLWMzNTMtNGM4Zi1hODRjLWNkOTdjZjE2YWNmZiIsImVtYWlsIjoidGVzdDAxQHRlc3QuY29wbSIsInN1YiI6InRlc3QwMUB0ZXN0LmNvcG0iLCJqdGkiOiJmYTRmODRlZC05MzVmLTRiN2MtYTNjMS0zMmVkYjZhNDJlNzEiLCJuYmYiOjE2MjMwODU1MDksImV4cCI6MTYyMzEwNzEwOSwiaWF0IjoxNjIzMDg1NTA5fQ.H5MCdyuTUUo0C9cwBwS8wcYq3QBzxGUWbwTAaMgeKFY
```
