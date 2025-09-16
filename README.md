# ReportDemo

ReportDemo is a simple ASP.NET MVC project that demonstrates a secure login system and CRUD operations for managing student data. The backend uses PostgreSQL as the database and pgAdmin4 for database management.

## Features

- **Secure Login:** Modern login page with styled input and password visibility toggle.
- **Student Management:** Add, edit, view, and delete student records.
- **RDLC Reporting:** Generate reports for student data in RDLC format.
- **PostgreSQL Integration:** All data is stored in a PostgreSQL database.
- **MVC Pattern:** Built using ASP.NET MVC for clear separation of concerns.

## Screenshots

### Secure Login
![Secure Login](assets/login.png)<img width="505" height="772" alt="image" src="https://github.com/user-attachments/assets/875eec12-fe12-4fbf-9410-753ad556166a" />


### Students List & Actions
![Students List](assets/students-list.png)<img width="1920" height="1020" alt="image" src="https://github.com/user-attachments/assets/8379dfa3-f963-4781-8dc2-33d9fae4ba0c" />


## How to Run (Manual Steps)

1. **Clone this repository**
   - Download or clone the repository from GitHub:  
     `https://github.com/ShahFaisal9351/ReportDemo.git`

2. **Install Dependencies**
   - Ensure you have [.NET SDK](https://dotnet.microsoft.com/download) installed.
   - Download and install [PostgreSQL](https://www.postgresql.org/download/) and [pgAdmin4](https://www.pgadmin.org/download/).

3. **Configure the Database**
   - Open pgAdmin4 and create a new PostgreSQL database (for example, `reportdemo_db`).
   - Note the database name, username, and password.
   - Open the project’s `appsettings.json` or `Web.config` file and update the connection string:
     ```json
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=reportdemo_db;Username=YOUR_USER;Password=YOUR_PASSWORD"
     }
     ```
   - Create the necessary tables according to the models in your project. You can do this by running migrations if already set up, or by executing SQL scripts manually via pgAdmin4.

4. **Build and Run the Application**
   - Open the project in Visual Studio or your preferred IDE.
   - Restore NuGet packages if prompted.
   - Build the solution to ensure all dependencies are resolved.
   - Start the application (usually by pressing `F5` or clicking the "Start" button in Visual Studio).
   - The application should launch in your browser, typically at [http://localhost:7072](http://localhost:7072).

5. **Login and Usage**
   - Use your email and password to log in through the secure login page.
   - Once logged in, you can add, edit, view, and delete student records and generate RDLC reports.

## Project Structure

- `Controllers/` — MVC controllers for handling requests.
- `Models/` — Entity and view models.
- `Views/` — Razor views for UI.
- `assets/` — Images/screenshots for documentation.

## Technologies Used

- ASP.NET MVC
- PostgreSQL
- pgAdmin4
- RDLC Reporting

## About

This project is for demo purposes and showcases a basic student management system with secure login and reporting features. It is useful for beginners learning ASP.NET MVC and PostgreSQL integration.

---

**Images:**  
- `assets/login.png` — Login page screenshot  
- `assets/students-list.png` — Students list screenshot

> _Please make sure the image files are placed in the `assets/` folder as referenced above for proper rendering in GitHub._
