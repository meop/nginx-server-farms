# About

This project serves as a replacement for IIS ARR + URL Rewrite aka Web Farms aka Server Farms.

It uses Nginx instead.

It can control a service that hosts Nginx, but these wrapper services are not fully reliable, nor is Nginx itself.

Therefore, its fastest and safest mode is to run standalone, or as a service, controlling Nginx directly.

# Architecture

ASP.NET Core -> SignalR -> React

# Tools

You can use either VS Code or VS..

The project was developed mostly in VS Code and has an omnisharp.json C# style format file.

VS Code has better JS intellisense, but VS has better C# and Nuget intellisense, in general.

# Setup

```powershell
push-location ./NginxServerFarms
```

## Prepare HTTPS Certificate

```powershell
mkdir -p D:\nginx -ErrorAction Ignore
push-location D:\nginx

dotnet dev-certs https -t
dotnet dev-certs https -ep server-farms.pfx -p 'password'

pop-location
```

## Prepare Service

```powershell
mkdir -p D:\nginx\server-farms -ErrorAction Ignore

New-Service -Name NginxServerFarms `
    -BinaryPathName='D:\nginx\server-farms\NginxServerFarms.exe' `
    -StartupType Automatic
```

# Publish

```powershell
Stop-Service -Name NginxServerFarms

dotnet publish -o D:\nginx\server-farms\ -c Debug
robocopy ClientApp D:\nginx\server-farms\ClientApp /mir /z | Out-Null

Start-Service -Name NginxServerFarms
```