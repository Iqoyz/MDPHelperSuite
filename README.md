This backend service is built using .NET and MongoDB. It provides an API for fetching firmware files stored in MongoDB. It supports various operations related to firmware management, such as retrieving firmware information and fetching firmware files.
- Fetch firmware information from the MongoDB database
- Retrieve firmware files via API endpoints
- Supports JSON format for API responses

- .NET 6.0 or later
- MongoDB 4.4 or later
- MongoDB.Driver package

1. Open `appsettings.json` and set your MongoDB connection string:

```json
{
  "ConnectionStrings": {
    "MongoDb": "mongodb://localhost:27017"
  }
}
````


#### Build and Run
```markdown
1. Restore the project dependencies:
````

```sh
dotnet restore

dotnet build

dotnet run
````

**GET** `/api/firmware/file/{id}`

- **Description**: Retrieves the firmware file by its ID.
- **Parameters**: 
  - `id` (string): The ID of the firmware file to retrieve.
- **Response**: The firmware file in binary format.


You can use tools like cURL to test the API endpoints. Example cURL commands:

```sh
curl -X GET http://localhost:5000/api/firmware
curl -X GET http://localhost:5000/api/firmware/name/{firmwareName}
````

or just run `http://localhost:5000/swagger/index.html` in your browser after `dotnet run`

### Troubleshooting
```markdown
- **Issue**: MongoDB connection error
  - **Solution**: Check the connection string in `appsettings.json` and ensure MongoDB is running.

- **Issue**: API not responding
  - **Solution**: Verify that the .NET application is running and check for any errors in the console logs.
```
