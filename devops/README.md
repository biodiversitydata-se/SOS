## Förutsättningar för att köra i lokalt kuberneteskluster (via skaffold)
Du behöver installera:  
**[Docker Desktop](https://www.docker.com/products/docker-desktop) och **aktivera Kubernetes** (Settings => Kubernetes => Enable kubernetes)** 

**[Skaffold](https://github.com/GoogleContainerTools/skaffold/releases/tag/v2.5.1)**  **OBS mallen är kompatibel med version v2.5.1 på Windows: https://github.com/GoogleContainerTools/skaffold/releases/tag/v2.5.1**  
(ladda hem .exe filen, döp den till ``skaffold`` och lägg i PATH).  

**[Helm](https://helm.sh/docs/intro/install/)**
(se till att du har v3.12.2 eller senare genom att köra ```helm version```, behöver du uppdatera så kör ```choco upgrade kubernetes-helm``` eller motsvarande för annat pakethanteringssystem)   

## *Första gången (en gång!)*
lägg till helm repo för redis genom kommando:
``helm repo add bitnami https://charts.bitnami.com/bitnami``   

## De olika skaffold-filerna

### skaffold.administration.api.yaml   
ställ dig i reporoten (där filerna ligger), kör kommando: ```skaffold run -f skaffold.administration.api.yaml```  
gå till ```http://localhost:5005/swagger``` i din webbläsare   
*kom ihåg!* kommando: ```skaffold delete -f skaffold.administration.api.yaml``` tar bort allt när du är klar, annars blir det en rörig blandning av containers när du kör igång nästa projekt, och vissa portar kommer vara låsta!

### skaffold.administration.gui.yaml  
kommando: ```skaffold run -f skaffold.administration.gui.yaml```  
nu kör hela lösningen inne i lokalt k8s-kluster dvs
dvs:
``` 
web
api
```
gå till ```http://localhost:4200``` i din webbläsare   
api finns på ```http://localhost:5005```   
*kom ihåg!* kommando: ```skaffold delete -f skaffold.administration.gui.yaml``` för att ta bort allt som du nyss snurrat upp  

###  skaffold.analysis.yaml  
kommando: ```skaffold run -f skaffold.analysis.yaml```  
gå till ```http://localhost:5005/swagger``` i din webbläsare   
*kom ihåg!* kommando: ```skaffold delete -f skaffold.analysis.yaml``` för att ta bort rätt container   

###  skaffold.datastewardship.yaml  
kommando: ```skaffold run -f skaffold.datastewardship.yaml```  
gå till ```http://localhost:5005/swagger``` i din webbläsare   
*kom ihåg!* kommando: ```skaffold delete -f skaffold.datastewardship.yaml``` för att ta bort rätt container   

###  skaffold.elasticsearch.proxy.yaml  
kommando: ```skaffold run -f skaffold.elasticsearch.proxy.yaml```  
gå till ```http://localhost:5005``` i din webbläsare   
*kom ihåg!* kommando: ```skaffold delete -f skaffold.elasticsearch.proxy.yaml``` för att ta bort rätt container   

###  skaffold.observations.yaml  
kommando: ```skaffold run -f skaffold.observations.yaml```  
gå till ```http://localhost:5005/swagger``` i din webbläsare   
*kom ihåg!* kommando: ```skaffold delete -f skaffold.observations.yaml``` för att ta bort rätt container   

## Hemligheter (när du kör skaffold run)

Om vi har hemligheter (API nycklar, connectionstring till central databas etc m.m.) som vi måste använda vid lokal utveckling - men inte vill ska hamna i git - så lägger vi dessa i en fil som heter ```base.secret.yaml``` i devops/k8s/local/base/api.

Du kan kopiera hela filinnehållet från **[https://vault-test.artdata.slu.se](https://vault-test.artdata.slu.se)**, gå in på secret path ```kv2/sos-observations```, mappen ```skaffold```. Kopiera och klistra in i en fil enligt instruktionen ovan.   
```*.secret.yaml``` är med i vår .gitignore.

Såhär ser innehållet ut i en *.secret.yaml fil:
```
apiVersion: v1
kind: Secret
metadata:
  name: api-key-st
type: Opaque
stringData:
  apikey: TheActualApiKeyGoesHere

```

sen ser vi till att hemligheten tillgängligörs som en miljövariabel genom följande konfiguration i API manifestet:
Där vi också konfigurerar vanliga klartext miljövariabler för vanliga icke-hemliga settings (dvs container -> env sektionen)  
```
            - name: API_KEY_ST
              valueFrom:
                secretKeyRef:
                  name: api-key-st
                  key: apikey
```

Detta gör att vårt api (i det här exemplet) har vår hemliga API_KEY_ST tillgänglig som en miljövariabel när vi kör via skaffold. Utanför skaffold (ctrl f5 i Visual Studio) kommer vi gå på aktuell appsettings.json-fil och lokala user secrets (Microsofts lösning i dotnet).