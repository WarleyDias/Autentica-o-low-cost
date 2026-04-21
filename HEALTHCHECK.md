# Health Check Documentation

The system now has three endpoints for monitoring application health:

## Complete Health Status

```http
GET /api/healthcheck
```

Returns detailed information about application health.

**Response (200 OK):**
```json
{
  "status": "Ok",
  "timestamp": "2024-01-15T10:30:45.123Z",
  "version": "1.0.0",
  "details": {
    "databaseHealthy": true,
    "authServiceHealthy": true,
    "uptimeMilliseconds": 125430,
    "activeUsers": 0,
    "environment": "Development"
  }
}
```

## Liveness Probe (Kubernetes)

```http
GET /api/healthcheck/live
```

Checks if the application is alive. Simple and fast.

**Response (200 OK):**
```json
{
  "status": "alive"
}
```

## Readiness Probe (Kubernetes)

```http
GET /api/healthcheck/ready
```

Checks if the application is ready to receive requests.

**Response (200 OK - Ready):**
```json
{
  "status": "ready"
}
```

**Response (503 Service Unavailable - Not Ready):**
```json
{
  "status": "not ready",
  "details": {
    "databaseHealthy": false,
    "authServiceHealthy": false,
    "uptimeMilliseconds": 1000,
    "activeUsers": 0,
    "environment": "Development"
  }
}
```

## Kubernetes Configuration

### Liveness Probe

```yaml
livenessProbe:
  httpGet:
    path: /api/healthcheck/live
    port: 80
  initialDelaySeconds: 10
  periodSeconds: 10
```

### Readiness Probe

```yaml
readinessProbe:
  httpGet:
    path: /api/healthcheck/ready
    port: 80
  initialDelaySeconds: 5
  periodSeconds: 5
```

## Docker Compose Configuration

```yaml
healthcheck:
  test: ["CMD", "curl", "-f", "http://localhost:80/api/healthcheck"]
  interval: 30s
  timeout: 10s
  retries: 3
  start_period: 40s
```

## Testing with cURL

```bash
# Complete health check
curl -X GET http://localhost:5000/api/healthcheck

# Liveness check
curl -X GET http://localhost:5000/api/healthcheck/live

# Readiness check
curl -X GET http://localhost:5000/api/healthcheck/ready
```

## Recommended Alerts

- If status != "Ok"
- If uptimeMilliseconds < 0 (application restarted)
- If DatabaseHealthy = false
- If AuthServiceHealthy = false
- If readiness returns 503 for more than 1 minute

## Log Aggregation

Aggregate `/api/healthcheck` logs for monitoring:

```bash
# Check every minute
*/1 * * * * curl -s http://localhost:5000/api/healthcheck >> /var/log/health-check.log
```

## Future Improvements

For future versions, enhance the health check:

1. **Database**: Verify actual database connection
2. **Cache**: Check Redis/Memcached status
3. **External APIs**: Verify external dependencies
4. **Disk Space**: Monitor available disk space
5. **Memory**: Monitor memory usage
6. **CPU**: Monitor CPU load

Extended implementation example:

```csharp
public class HealthCheckDetails
{
    public bool DatabaseHealthy { get; set; }
    public bool RedisHealthy { get; set; }
    public bool ExternalApiHealthy { get; set; }
    public double DiskUsagePercent { get; set; }
    public double MemoryUsageMB { get; set; }
    public double CpuUsagePercent { get; set; }
    public List<string> Warnings { get; set; }
    public List<string> Errors { get; set; }
}
```

