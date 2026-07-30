// Generates an RSA key pair for RS256 JWT signing (Echo auth-5).

// Generates an RSA key pair for RS256 JWT signing (Echo auth-5), base64-encoded.
// Invoked by setup-jwt-keys.sh — not meant to be run standalone.

using System.Security.Cryptography;

using var rsa = RSA.Create(2048);

var privateB64 = Convert.ToBase64String(
    System.Text.Encoding.UTF8.GetBytes(rsa.ExportRSAPrivateKeyPem())
);
var publicB64 = Convert.ToBase64String(
    System.Text.Encoding.UTF8.GetBytes(rsa.ExportSubjectPublicKeyInfoPem())
);

File.WriteAllText("private.b64", privateB64);
File.WriteAllText("public.b64", publicB64);
