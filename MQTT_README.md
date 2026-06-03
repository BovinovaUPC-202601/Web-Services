# VacApp — Telemetría IoT por MQTT

La telemetría del collar **no entra por HTTP**. Según el constraint **CON2** del docx,
el collar se comunica **exclusivamente por MQTT**. El backend consume del broker mediante
un `BackgroundService` (`MqttTelemetryConsumer`) y reutiliza el mismo pipeline de dominio
que antes usaba el endpoint HTTP.

## Flujo

```
[Collar / Simulador] --publica--> [Broker MQTT] --consume--> [Backend IoTMonitoring]
       vacapp/telemetry                                       BovineHealthRecordCommandService
                                                                     |
                                                       aggregate evalúa rangos
                                                                     |
                                              si anómalo -> AbnormalTelemetryDetectedEvent
                                                                     |
                                                       AlertManagement crea la alerta
                                                                     |
[Collar] <--respuesta-- [Broker] <--publica-- vacapp/telemetry/response/{deviceId}
   (prende LED)                                  { id, isAlert, message }
```

## 1. Levantar el broker (Mosquitto local)

```bash
docker compose -f docker-compose.mqtt.yml up -d
```

Broker disponible en `localhost:1883` (acceso anónimo en desarrollo).

## 2. Variables de entorno del backend (.env)

```env
MQTT_HOST=localhost
MQTT_PORT=1883
MQTT_USERNAME=                 # vacío => conexión anónima
MQTT_PASSWORD=
MQTT_CLIENT_ID=vacapp-backend
MQTT_TELEMETRY_TOPIC=vacapp/telemetry
MQTT_RESPONSE_TOPIC_PREFIX=vacapp/telemetry/response
MQTT_USE_TLS=false            # true cuando el broker usa 8883 (TLS)
MQTT_TLS_ALLOW_UNTRUSTED=false # true solo con certificados self-signed (dev)
```

Si no se definen, el backend usa estos valores por defecto.

## 3. Contrato de mensajes

**Telemetría (collar → `vacapp/telemetry`):**

```json
{
  "deviceId": "esp32-001",
  "bovineId": 1,
  "userId": 1,
  "temperature": 39.2,
  "heartRate": 72,
  "batteryLevel": 85,
  "timestamp": "2026-06-03T12:00:00Z"
}
```

**Respuesta (backend → `vacapp/telemetry/response/{deviceId}`):**

```json
{ "id": 123, "isAlert": true, "message": "ALERT: vital signs outside normal bovine range." }
```

> `timestamp` es informativo; el backend sella `RecordedAt` en el servidor.

## 4. Demo sin hardware

No hay collar físico (el proyecto es teórico). Para la demo se usa el **simulador Python**
del repo `ESP32` (`simulator/`), que publica telemetría real al broker. Para el backend y el
broker, el simulador es indistinguible de un ESP32 real.

## Migración de base de datos (batteryLevel)

Se agregó la columna `battery_level` al modelo `BovineHealthRecord`. El proyecto usa
`Database.EnsureCreated()` (no migraciones), por lo que **una base existente NO recibe la
columna automáticamente**. Para aplicar el cambio en desarrollo, borrá la tabla
`bovine_health_records` (o la base) y dejá que el backend la recree al arrancar.

## Despliegue: Mosquitto en una VM de Azure (todo en la nube, TLS)

Cuando el backend está desplegado (Azure App Service), no puede llegar a un broker
local. El broker corre en una **VM de Azure** (B1s, cubierta por el crédito de
Azure for Students) con IP/DNS público y **MQTT sobre TLS** en el puerto 8883.

### 1. VM + firewall
- Crear VM Ubuntu B1s. Anotar su DNS público (ej. `vacapp-mqtt.eastus.cloudapp.azure.com`).
- NSG → abrir el puerto **8883** (TCP) entrante.

### 2. Certificado + Mosquitto (en la VM)
```bash
sudo apt update && sudo apt install -y docker.io
# Certificado: usar Let's Encrypt (recomendado) o self-signed para la demo.
# Self-signed rápido:
mkdir -p certs && cd certs
openssl req -new -x509 -days 365 -nodes -out ca.crt -keyout ca.key -subj "/CN=VacApp-CA"
openssl genrsa -out server.key 2048
openssl req -new -out server.csr -key server.key -subj "/CN=<DNS-de-tu-VM>"
openssl x509 -req -in server.csr -CA ca.crt -CAkey ca.key -CAcreateserial -out server.crt -days 365
```
Activar el listener TLS en `mosquitto.conf` (ver el bloque al final del archivo),
crear el usuario y levantar el contenedor montando `certs/` y `passwd`.

### 3. Variables de entorno del backend (Azure App Service → Configuration)
```
MQTT_HOST=<DNS-de-tu-VM>
MQTT_PORT=8883
MQTT_USE_TLS=true
MQTT_TLS_ALLOW_UNTRUSTED=true   # solo si el cert es self-signed; false con Let's Encrypt
MQTT_USERNAME=vacapp
MQTT_PASSWORD=<tu-pass>
```

### 4. Simulador (desde tu PC, apuntando a la VM)
```bash
python simulator.py --host <DNS-de-tu-VM> --port 8883 --tls --tls-insecure \
  --username vacapp --password <tu-pass>
```
(`--tls-insecure` solo con certificado self-signed; con Let's Encrypt se omite.)

> Recomendación de costo: apagá (deallocate) la VM cuando no la uses para no
> consumir el crédito de Azure for Students.
