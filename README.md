# Weather Monitoring Application

This application displays wind speed and temperature for various cities. It also provides the trend for these values for the last 2 hours.

## How to Run the Application

To run the application, follow these steps:

1. **If running for the first time**:
   - Run migrations using `Update-Database` in the `WeatherMonitoringApp` directory:
     ```bash
     cd WeatherMonitoringApp
     Update-Database
     ```
   - Run `npm install` in the `ClientApp` directory to install dependencies:
     ```bash
     cd ClientApp
     npm install
     ```
   - Set the API key in WeatherMonitoringApp/appsettings.json.
     

2. **For subsequent runs**:
   - Start the frontend (React) application:
     ```bash
     cd ClientApp
     npm start
     ```
   - Start the backend (ASP.NET Core) application:
     ```bash
     cd WeatherMonitoringApp
     dotnet run
     ```

## Features
- Displays the wind speed and temperature for selected cities.
- Shows a trend chart for the last 2 hours of wind speed and temperature data.

## Technology Stack
- **Frontend**: React
- **Backend**: ASP.NET Core
- **Database**: SQL Server

## Views
![image](https://github.com/user-attachments/assets/a0c2c89d-3104-4016-8bdb-a5cbad117532)
![image](https://github.com/user-attachments/assets/90a693ef-794f-4551-9742-f7e63625a74a)


