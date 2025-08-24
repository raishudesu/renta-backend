# Renta

**Book, Scan, Drive — The simplest way to rent vehicles.**  
Owners list their cars, renters book instantly, and QR codes make pickup seamless.  
_No paperwork, no hassle._

> **Note:** This project is still in active development. Features and documentation are subject to change.

---

## How Renta Works

A seamless experience for both **vehicle owners** and **renters**.

### For Renters

1. **Browse & Book**  
   Find the perfect vehicle, check availability, and book instantly with just a few clicks.

2. **Receive QR Code**  
   Get your unique QR code confirmation immediately after booking. No waiting, no paperwork.

3. **Quick Pickup**  
   Visit the owner’s location, show your QR code, and drive away. It’s that simple!

### For Vehicle Owners

1. **List Your Vehicle**  
   Create your account, add vehicle details, photos, and set your availability and pricing.

2. **Receive Bookings**  
   Get notified when someone books your vehicle. View all booking details in your dashboard.

3. **Scan & Confirm**  
   Simply scan the renter’s QR code to confirm the booking. Instant verification and peace of mind.

---

## Powerful Features

- **QR Code Technology**  
  Instant booking confirmation and seamless pickup process with QR codes.

- **Owner Dashboard**  
  Vehicle, and Booking management in one place.

- **Smart Scheduling _(Soon)_** \
  Automated availability management and booking conflict prevention.

- **Open Source**  
  Fully transparent and community-driven. Contribute, customize, and deploy your own rental platform with our open-source codebase.

- **Owner Location Tracking**  
  GPS integration for easy vehicle location and pickup coordination.

- **Instant Notifications _(Soon)_**  
  Real-time updates for bookings, confirmations, and important alerts.

---

## Tech Stack

- **Frontend:** React / Next.js
- **Backend:** ASP.NET Core Web API
- **Database:** PostgreSQL
- **Hosting:** Amazon Web Services (EC2, S3 & RDS)
- **Other:** QR Code generation

---

## 🚀 Deploying Renta's ASP.NET Core Web API on AWS EC2 with Nginx

This guide explains how to deploy an ASP.NET Core Web API application to an **AWS EC2 Amazon Linux 2023 instance** and serve it through **Nginx**.

---

## 📌 Prerequisites

- AWS EC2 instance (Amazon Linux 2023).
- Domain name (optional).
- .NET SDK (locally for publishing).
- .NET Runtime installed on EC2.
- Nginx installed.
- Published ASP.NET Core Web API project.

---

## ⚙️ 1. Connect to EC2

```sh
ssh -i your-key.pem ec2-user@your-ec2-public-ip
```

---

## 📦 2. Install Dependencies

### Update system

```sh
sudo dnf update -y
```

### Install .NET runtime

```sh
wget https://builds.dotnet.microsoft.com/dotnet/aspnetcore/Runtime/8.0.19/aspnetcore-runtime-8.0.19-linux-x64.tar.gz
```

_(Replace `8.0` with your target runtime if needed)_

Append these lines into your **\~/.bash_profile**, you can run this:

```bash
echo 'export DOTNET_ROOT=$HOME/dotnet' >> ~/.bash_profile
echo 'export PATH=$PATH:$HOME/dotnet' >> ~/.bash_profile
echo 'export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=True' >> ~/.bash_profile
```

Then reload your profile so the changes apply immediately:

```bash
source ~/.bash_profile
```

### Install Nginx

```sh
sudo dnf install -y nginx
```

Enable and start Nginx:

```sh
sudo systemctl enable nginx
sudo systemctl start nginx
```

---

## 📂 3. Deploy the ASP.NET Core App

### On local machine, publish the app

```sh
dotnet publish --configuration Release --output ./publish
```

### Create a zip file of the published application

```sh
tar -czf renta.tar.gz -C publish .
```

### Copy to EC2 instance using SCP

```sh
scp -i /path/to/your-key.pem renta.tar.gz ec2-user@your-ec2-ip:/home/ec2-user/
```

### Create application directory

```sh
sudo mkdir -p /opt/renta
sudo chown $USER:$USER /opt/renta
```

### Extract the application

```sh
cd /opt/renta
tar -xzf /home/ec2-user/renta.tar.gz
```

### Make sure the main DLL is executable

```sh
chmod +x backend.dll
```

### Test the application

```sh
dotnet backend.dll
```

### Create the environment file

```sh
sudo nano /opt/renta/.env
```

### Add your environment variables (replace with your actual values):

```
AWS_ACCESS_KEY_ID=AKIA1234567890EXAMPLE
AWS_SECRET_ACCESS_KEY=wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY
CONNECTION_STRING=Host=your-rds-endpoint.amazonaws.com;Database=yourdb;Username=youruser;Password=yourpassword;Port=5432;
JWT_SECRET_KEY=your-super-long-random-jwt-secret-key-at-least-32-characters-long
JWT_ISSUER=your-app-name
JWT_AUDIENCE=your-app-users
```

### Save and exit (Ctrl+X, then Y, then Enter)

### Secure the environment file (IMPORTANT!)

```sh
sudo chmod 600 /opt/renta/.env
sudo chown root:root /opt/renta/.env
```

## 🛠️ 4. Setup Systemd Service

---

Create service file:

```sh
sudo nano /etc/systemd/system/renta.service
```

Paste:

```ini
[Unit]
Description=Renta .NET Web API
After=network.target

[Service]
Type=simple
User=ec2-user
Group=ec2-user
WorkingDirectory=/opt/renta
ExecStart=/home/ec2-user/dotnet/dotnet /opt/renta/backend.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=renta
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000
Environment=DOTNET_ROOT=/home/ec2-user/dotnet
Environment=PATH=/home/ec2-user/dotnet:/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin
Environment=DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
EnvironmentFile=/opt/renta/.env

[Install]
WantedBy=multi-user.target
```

### Make sure dotnet is executable

```sh
chmod +x /home/ec2-user/dotnet/dotnet
```

### Configure ownership of the application directory

```sh
sudo chown -R ec2-user:ec2-user /opt/renta
```

### Configure .env file permissions

```sh
sudo chown ec2-user:ec2-user /opt/renta/.env
sudo chmod 600 /opt/renta/.env
```

### Verify the files exist and have correct permissions

```sh
ls -la /home/ec2-user/dotnet/dotnet
ls -la /opt/renta/backend.dll
ls -la /opt/renta/.env
```

Reload and start:

```sh
sudo systemctl daemon-reload
sudo systemctl enable renta
sudo systemctl start renta
sudo systemctl status renta
```

---

## 🌐 5. Configure Nginx Reverse Proxy

Create config:

```sh
sudo nano /etc/nginx/conf.d/renta.conf
```

Paste:

```nginx
server {
    listen 80 default_server;
    listen [::]:80 default_server;

    server_name _;

    location / {
        proxy_pass         http://127.0.0.1:5000;
        proxy_http_version 1.1;

        proxy_set_header   Upgrade $http_upgrade;
        proxy_set_header   Connection keep-alive;
        proxy_set_header   Host $host;
        proxy_cache_bypass $http_upgrade;

        proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
    }
}
```

Test and restart:

```sh
sudo nginx -t
sudo systemctl restart nginx
```

---

## 🔐 6. AWS Security Group

- Allow **port 80** (HTTP) and/or **443** (HTTPS).
- Restrict other ports.
- Port **5000** remains internal only.

---

## 🔄 7. Verify Setup

Check API directly on EC2:

```sh
curl http://127.0.0.1:5000/api/Vehicle
```

Check via Nginx (public):

```sh
curl http://your-ec2-public-ip/api/Vehicle
```

---

## 🔑 8. (Optional) HTTPS with Certbot

If using a domain:

```sh
sudo dnf install -y certbot python3-certbot-nginx
sudo certbot --nginx -d yourdomain.com -d www.yourdomain.com
```

---

## 🧰 9. Maintenance Commands

Restart API:

```sh
sudo systemctl restart renta
```

View logs:

```sh
journalctl -u renta -f
```

Restart Nginx:

```sh
sudo systemctl restart nginx
```

---

✅ Your ASP.NET Core API is now deployed and served through **Nginx reverse proxy** on AWS EC2.
