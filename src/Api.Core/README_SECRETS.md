# Setup local database credentials securely

Run these commands in your terminal in this directory to store your local database credentials securely:

```bash
dotnet user-secrets set "Database:Username" "your_postgres_username"
dotnet user-secrets set "Database:Password" "your_postgres_password"
```

Replace the username and password with your postgres credentials.

---

To verify the keys were written successfully, run:

```bash
dotnet user-secrets list
```
