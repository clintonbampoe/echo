// Generates an RSA key pair for RS256 JWT signing (Echo auth-5).
//
// Run with:
//   dotnet run GenerateJwtKeys.cs
//
// Writes private.pem and public.pem to the current directory.
// DO NOT commit these files. Set them via user-secrets and delete the local copies:
//
//   dotnet user-secrets set "Jwt:PrivateKey" "$(cat private.pem)" --project Echo.Api
//   dotnet user-secrets set "Jwt:PublicKey" "$(cat public.pem)" --project Echo.Api
//   rm private.pem public.pem

using System.Security.Cryptography;

using var rsa = RSA.Create(2048);

var privatePem = rsa.ExportRSAPrivateKeyPem();
var publicPem = rsa.ExportSubjectPublicKeyInfoPem();

File.WriteAllText("private.pem", privatePem);
File.WriteAllText("public.pem", publicPem);

Console.WriteLine("Wrote private.pem and public.pem to the current directory.");
Console.WriteLine();
Console.WriteLine("Next steps:");
Console.WriteLine("  dotnet user-secrets set \"Jwt:PrivateKey\" \"$(cat private.pem)\" --project Echo.Api");
Console.WriteLine("  dotnet user-secrets set \"Jwt:PublicKey\" \"$(cat public.pem)\" --project Echo.Api");
Console.WriteLine("  rm private.pem public.pem");
