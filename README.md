# pwm-fan-control

A PWM fan control, primarily intended for a setup of a raspberry pi and a Noctua fan

## Modalità di avvio FanCommander

### Modalità Development (console interattiva con grafico e gauge)

```sh
export ASPNETCORE_ENVIRONMENT=Development
cd FanCommander/FanCommander
# Avvio diretto
 dotnet run
# Oppure build e avvio
 dotnet build
 dotnet FanCommander.dll
```

In questa modalità, la console mostrerà un grafico ASCII della temperatura e una barra gauge della velocità della ventola, aggiornati ad ogni ciclo.

### Modalità Production (solo logging, nessuna visualizzazione interattiva)

```sh
export ASPNETCORE_ENVIRONMENT=Production
cd FanCommander/FanCommander
# Avvio diretto
 dotnet run
# Oppure build e avvio
 dotnet build
 dotnet FanCommander.dll
```

In questa modalità, il servizio scrive solo nei log e non mostra la visualizzazione interattiva.

## Acknowledgments

This project includes code from [Michael Klements](https://github.com/mklements/PWMFanControl). Many thanks for sharing!
