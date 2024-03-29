To run you need to update your Postgres database:
<pre>
$ dotnet ef database update -p CatTok.Api  
</pre>

Create google auth credentials and add these urls to the Authorized redirect URIs:
<pre>
https://localhost:7003
https://localhost:7003/signin-google
https://localhost:7003/api/auth/google/login
https://localhost:7003/api/auth/google/callback
</pre>

Then open appsettings.json and specify google client id, secret and jwt security key
<pre>
"GoogleOAuthOptions": {
    "ClientId": "*",
    "ClientSecret": "*"
  },

  "JwtSettings": {
    "Key": "*",
    "HashKey": "*"    <-- key for hashing passwords
    ...               <-- Optional (for production)
</pre>

Then start web app

<pre>
$ dotnet run --project CatTok.Api
</pre>

To see swagger navigate to <strong>https://localhost:7003/swagger</strong>