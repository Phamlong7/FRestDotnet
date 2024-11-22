<!DOCTYPE html>
<html>
<head>
  <title>Project README</title>
</head>
<body>
  <h1 align="center">🚀 .NET Project Setup Guide</h1>
  <p align="center">
    A modern and streamlined guide to set up and run your .NET application with ease.
  </p>

  <hr>

  <h2>📋 Prerequisites</h2>
  <ul>
    <li>Ensure you have <b>.NET SDK</b> installed. (<a href="https://dotnet.microsoft.com/download" target="_blank">Download here</a>)</li>
    <li>Have <b>SQL Server</b> installed and running locally or remotely.</li>
    <li>An editor like <a href="https://code.visualstudio.com/" target="_blank">Visual Studio Code</a> or <a href="https://visualstudio.microsoft.com/" target="_blank">Visual Studio</a>.</li>
    <li>Git for version control (<a href="https://git-scm.com/" target="_blank">Download Git</a>).</li>
  </ul>

  <h2>📂 Project Setup</h2>
  <ol>
    <li>Clone the repository to your local machine:
      <pre><code>git clone https://github.com/Phamlong7/FRestDotnet.git</code></pre>
    </li>
    <li>Open the project in your preferred editor.</li>
    <li>Go to the <code>appsettings.json</code> file:
      <ul>
        <li>Insert your <b>secret keys</b> and <b>database connection string</b>.</li>
        <li>Example:
          <pre><code>
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your_server_name;Database=your_database_name;User Id=your_user;Password=your_password;"
  },
  "SecretKeys": {
    "JWTSecret": "your_jwt_secret_key",
    "AnotherSecret": "your_other_secret"
  }
            .......
}
          </code></pre>
        </li>
      </ul>
    </li>
    <li>Run migrations to generate the database schema:
      <pre><code>dotnet ef database update</code></pre>
    </li>
    <li>Verify that the connection string in <code>appsettings.json</code> matches your repository configuration.</li>
  </ol>

  <h2>🛠 Configuration</h2>
  <ul>
    <li>Navigate to the <b>Utility</b> folder.</li>
    <li>Inside the <b>ConstantHelper</b> file:
      <ul>
        <li>Enter your development email and password.</li>
        <li>Example:
          <pre><code>
public static class ConstantHelper {
    public const string Email = "youremail@example.com";
    public const string Password = "yourpassword";
}
          </code></pre>
        </li>
      </ul>
    </li>
  </ul>

  <h2>👤 Create an Admin Account</h2>
  <ol>
    <li>Run the application and create a user account.</li>
    <li>Open your database and locate the following tables:
      <ul>
        <li><b>ASPNETUSERROLE</b>: Assign the user a role of <code>Admin</code>.</li>
        <li><b>ASPNETUSER</b>: Ensure the user is properly linked.</li>
      </ul>
    </li>
  </ol>

  <h2>🎉 You're Ready!</h2>
  <p>Start the application and enjoy! 🚀</p>

  <hr>

  <h2>🤝 Contributing</h2>
  <p>If you'd like to contribute, please fork the repository and use a feature branch. Pull requests are warmly welcome!</p>

  <h2>📧 Contact</h2>
  <p>If you have any questions, feel free to contact us at <a href="mailto:phamlong070704@gmail.com">phamlong070704@gmail.com</a>.</p>
</body>
</html>
