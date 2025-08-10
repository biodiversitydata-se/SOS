# SOS.Status.Web


## Bygga image med docker
```bash
# Om du står i SOS.Status.Web mappen
docker build --no-cache -t sos-status-web:latest -f Dockerfile ../../

# Om du står i repots rot
docker build --no-cache -t sos-status-web:latest -f src/SOS.Status.Web/Dockerfile .
```

## Köra image med docker
```bash
docker run -p 5006:5006 -e ASPNETCORE_ENVIRONMENT=ST -e ASPNETCORE_URLS=http://+:5006 sos-status-web:latest
```