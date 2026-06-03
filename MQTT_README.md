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

## Nota de seguridad (producción)

En desarrollo el broker acepta conexiones anónimas. El docx (RNF de seguridad) exige
**MQTT sobre TLS** con dispositivos autenticados. Para producción: configurar credenciales
y certificados en `mosquitto.conf`, abrir el puerto `8883` y completar `MQTT_USERNAME` /
`MQTT_PASSWORD`.
