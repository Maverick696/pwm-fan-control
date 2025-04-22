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

## Containerizzazione e pubblicazione su Docker Hub

Per buildare e pubblicare l'immagine FanCommander su Docker Hub usando Podman:

1. Modifica il file `Deploy/push-to-dockerhub.sh` se necessario.
2. Esegui lo script passando il tuo username Docker Hub:

```sh
cd Deploy
chmod +x push-to-dockerhub.sh
./push-to-dockerhub.sh <tuo-username>
```

Lo script esegue:
- Build dell'immagine tramite Podman
- Login a Docker Hub
- Tag dell'immagine per il tuo repository
- Push dell'immagine su Docker Hub

Dopo la push, potrai eseguire il pull e lanciare il container su qualsiasi host compatibile con Docker/Podman:

```sh
podman pull docker.io/<tuo-username>/fancommander:latest
```

## Acknowledgments

This project includes code from [Michael Klements](https://github.com/mklements/PWMFanControl). Many thanks for sharing!
