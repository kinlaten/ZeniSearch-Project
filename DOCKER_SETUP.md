# Docker Setup Guide for Zeni Search Backend

## Prerequisites

- Docker Desktop installed ([https://www.docker.com/products/docker-desktop](https://www.docker.com/products/docker-desktop))
- Docker Compose (included with Docker Desktop)

## Quick Start

### 1. Clone/Navigate to the project:

```bash
cd /home/ed/Documents/gitRepo/zeni-search
```

### 2. Create environment file (optional):

```bash
# Copy the example to .env for custom settings
cp .env.example .env

# Edit .env if needed (passwords, ports, etc.)
nano .env
```

### 3. Build and start services:

```bash
# Build images and start containers
docker-compose up -d

# Or rebuild and start (if code changed)
docker-compose up -d --build
```

### 4. Run database migrations (first time only):

```bash
# Enter backend container
docker-compose exec backend bash

# Inside container, run migrations
dotnet ef database update

# Exit container
exit
```

### 5. Verify services are running:

```bash
# Check container status
docker-compose ps

# Check logs
docker-compose logs -f backend    # Backend logs
docker-compose logs -f postgres   # Database logs
```

### 6. Access the backend:

- **API Base URL:** `http://localhost:5000`
- **Health Check:** `http://localhost:5000/health`

## Common Commands

### View logs

```bash
# All services
docker-compose logs

# Specific service
docker-compose logs -f backend
docker-compose logs -f postgres

# Last 50 lines
docker-compose logs --tail=50
```

### Stop services

```bash
# Stop all services
docker-compose stop

# Stop specific service
docker-compose stop backend
```

### Start services

```bash
# Start all services
docker-compose start

# Start specific service
docker-compose start backend
```

### Remove everything

```bash
# Stop and remove containers, networks
docker-compose down

# Also remove volumes (WARNING: deletes database data)
docker-compose down -v
```

### Database access

```bash
# Connect to PostgreSQL from host
psql -h localhost -U zeni_user -d zeni_search_db

# Or from Docker
docker-compose exec postgres psql -U zeni_user -d zeni_search_db
```

### Rebuild backend

```bash
# Rebuild backend image after code changes
docker-compose build backend

# Rebuild and restart
docker-compose up -d --build backend
```

## Configuration

### Environment Variables

Edit `.env` file to customize:

```env
# Database
DB_USER=zeni_user
DB_PASSWORD=zeni_password
DB_NAME=zeni_search_db
DB_PORT=5432

# Backend
BACKEND_PORT=5000
ASPNETCORE_ENVIRONMENT=Development
LOG_LEVEL=Information
```

### Change Default Ports

To use different ports, edit `.env`:

```env
BACKEND_PORT=8000      # Backend on port 8000
DB_PORT=5433           # PostgreSQL on port 5433
```

## Troubleshooting

### Backend container fails to start

```bash
# Check logs
docker-compose logs backend

# Common issues:
# 1. Port already in use: Change BACKEND_PORT in .env
# 2. Database not ready: Check postgres logs
docker-compose logs postgres
```

### Database connection errors

```bash
# Verify PostgreSQL is running
docker-compose logs postgres

# Verify connection string is correct
# Format: Host=postgres;Port=5432;Database=zeni_search_db;Username=zeni_user;Password=zeni_password

# Test connection from host
psql -h localhost -U zeni_user -d zeni_search_db
```

### Can't find containers

```bash
# List all containers (including stopped)
docker ps -a

# List images
docker images
```

### Need to clear everything and start fresh

```bash
# Stop and remove all containers, volumes, networks
docker-compose down -v

# Remove images
docker-compose down -v --rmi all

# Start fresh
docker-compose up -d --build
```

## Production Considerations

For production deployment, consider:

1. **Use secrets management** (Docker Secrets or environment files outside repo)
2. **Set `ASPNETCORE_ENVIRONMENT=Production`** in `.env`
3. **Remove health check `stdin_open`** and `tty` settings
4. **Use strong passwords** for database
5. **Configure proper logging** and monitoring
6. **Use persistent volumes** for database data
7. **Set up backup strategy** for PostgreSQL data
8. **Consider using Docker Stack** or Kubernetes for orchestration

## Next Steps

When ready to add the frontend:

1. Add a `frontend` service to `docker-compose.yml`
2. Configure CORS in backend to allow frontend origin
3. Update backend API URLs in frontend to point to backend container
4. Add frontend container health checks

Example frontend service addition:

```yaml
frontend:
  build:
    context: ./frontend
    dockerfile: Dockerfile
  container_name: zeni-search-frontend
  ports:
    - "3000:3000"
  environment:
    REACT_APP_API_URL: http://localhost:5000
  depends_on:
    - backend
  networks:
    - zeni-network
  restart: unless-stopped
```

---

**Questions or issues?** Check the main [README.md](./README.md) or project documentation.
