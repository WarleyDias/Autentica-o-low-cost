# Documentação do Health Check

## Endpoints de Health Check

O sistema agora possui três endpoints para monitoramento da saúde da aplicação:

### 1. Health Status Completo

```http
GET /api/healthcheck
```

Retorna informações detalhadas sobre a saúde da aplicação.

**Resposta (200 OK):**
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

### 2. Liveness Probe (Kubernetes)

```http
GET /api/healthcheck/live
```

Verifica se a aplicação está viva. Simples e rápido.

**Resposta (200 OK):**
```json
{
  "status": "alive"
}
```

### 3. Readiness Probe (Kubernetes)

```http
GET /api/healthcheck/ready
```

Verifica se a aplicação está pronta para receber requisições.

**Resposta (200 OK - Pronta):**
```json
{
  "status": "ready"
}
```

**Resposta (503 Service Unavailable - Não Pronta):**
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

## Uso em Kubernetes

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

## Uso em Docker Compose

```yaml
healthcheck:
  test: ["CMD", "curl", "-f", "http://localhost:80/api/healthcheck"]
  interval: 30s
  timeout: 10s
  retries: 3
  start_period: 40s
```

## Testes com cURL

```bash
# Health check completo
curl -X GET http://localhost:5000/api/healthcheck

# Liveness
curl -X GET http://localhost:5000/api/healthcheck/live

# Readiness
curl -X GET http://localhost:5000/api/healthcheck/ready
```

## Monitoramento

### Alertas Recomendados

- Se status != "Ok"
- Se uptimeMilliseconds < 0 (aplicação reiniciou)
- Se DatabaseHealthy = false
- Se AuthServiceHealthy = false
- Se readiness retornar 503 por mais de 1 minuto

### Agregação de Logs

Você pode agregar os logs de `/api/healthcheck` para monitoramento:

```bash
# Verificar a cada minuto
*/1 * * * * curl -s http://localhost:5000/api/healthcheck >> /var/log/health-check.log
```

## Implementação Futura

Para versões futuras, melhorar o health check:

1. **Database**: Verificar conexão real ao banco
2. **Cache**: Verificar status do Redis/Memcached
3. **External APIs**: Verificar dependências externas
4. **Disk Space**: Monitorar espaço em disco
5. **Memory**: Monitorar uso de memória
6. **CPU**: Monitorar carga de CPU

Exemplo de implementação estendida:

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
