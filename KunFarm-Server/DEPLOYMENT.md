# KunFarm Backend Deployment Guide

## 🔧 Prerequisites

### Server Requirements
- Ubuntu/CentOS server with Docker & Docker Compose installed
- Minimum 2GB RAM, 1 CPU core, 10GB storage
- Ports 5270 (API) and 3306 (MySQL) available

### GitHub Repository Setup
1. **Secrets** (Repository Settings → Secrets and variables → Actions):
   ```
   DOCKER_PASSWORD=your-docker-hub-token
   SECRET_KEY=your-server-ssh-private-key
   MYSQL_ROOT_PASSWORD=your-secure-mysql-password
   ```

2. **Variables** (Repository Settings → Secrets and variables → Actions):
   ```
   DOCKER_USERNAME=your-docker-hub-username
   HOST_IP=your-server-ip-address
   ```

3. **Environment** (Repository Settings → Environments):
   - Create environment named `TEST`
   - Add protection rules if needed

## 🚀 Automatic Deployment

### CI/CD Pipeline
The pipeline automatically triggers on:
- Push to `main`, `develop`, or `feature/shop` branches
- Manual workflow dispatch

### Pipeline Steps
1. **Build Stage:**
   - Checkout code
   - Setup .NET 8 SDK
   - Cache NuGet packages
   - Restore dependencies
   - Build application
   - Run tests
   - Build & push Docker image to Docker Hub

2. **Deploy Stage:**
   - SSH to production server
   - Download production docker-compose.yml
   - Pull latest images
   - Stop old containers
   - Start new containers
   - Health check API endpoint
   - Clean up unused images

## 🖥️ Manual Deployment

### Server Setup
1. **Create deployment directory:**
   ```bash
   mkdir -p /var/docker/kunfarm-app
   cd /var/docker/kunfarm-app
   ```

2. **Download production files:**
   ```bash
   curl -O https://raw.githubusercontent.com/your-username/Kun-Farm/main/KunFarm-Server/docker-compose.prod.yml
   mv docker-compose.prod.yml docker-compose.yml
   
   curl -O https://raw.githubusercontent.com/your-username/Kun-Farm/main/KunFarm-Server/env.example
   cp env.example .env
   ```

3. **Configure environment:**
   ```bash
   nano .env
   # Update the values:
   DOCKER_USERNAME=your-dockerhub-username
   MYSQL_ROOT_PASSWORD=your-secure-password
   ```

4. **Deploy:**
   ```bash
   docker-compose pull
   docker-compose up -d
   ```

### Health Check
- **API Swagger:** http://your-server-ip:5270/swagger
- **API Base:** http://your-server-ip:5270
- **Database:** mysql://your-server-ip:3306

### Default Admin Account
```
Username: admin
Password: admin
Email: admin@kunfarm.com
```

## 🔍 Troubleshooting

### Check Container Status
```bash
docker-compose ps
docker-compose logs app
docker-compose logs mysql
```

### Database Connection Issues
```bash
# Check MySQL is ready
docker-compose exec mysql mysqladmin ping -u root -p

# Check database exists
docker-compose exec mysql mysql -u root -p -e "SHOW DATABASES;"
```

### Application Issues
```bash
# Check app logs
docker-compose logs -f app

# Restart services
docker-compose restart

# Full recreate
docker-compose down && docker-compose up -d
```

### Update Deployment
```bash
# Pull latest images
docker-compose pull

# Recreate containers
docker-compose up -d --force-recreate

# Clean unused images
docker image prune -f
```

## 📊 Monitoring

### Container Logs
```bash
# Follow all logs
docker-compose logs -f

# Follow specific service
docker-compose logs -f app
docker-compose logs -f mysql
```

### Resource Usage
```bash
# Check container stats
docker stats

# Check disk usage
docker system df
```

### Database Backup
```bash
# Backup database
docker-compose exec mysql mysqldump -u root -p Kun_Farm > backup.sql

# Restore database
docker-compose exec -T mysql mysql -u root -p Kun_Farm < backup.sql
```

## 🔐 Security Notes

- Change default admin password after first login
- Use strong MySQL root password
- Consider setting up firewall rules
- Keep Docker and system updated
- Monitor logs for suspicious activity

## 📞 Support

If you encounter issues:
1. Check the troubleshooting section above
2. Review container logs
3. Check GitHub Actions pipeline logs
4. Ensure all secrets/variables are set correctly 