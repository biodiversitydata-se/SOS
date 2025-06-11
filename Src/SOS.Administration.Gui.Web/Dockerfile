FROM reg.app.slu.se/hub/node:20.11 AS build
WORKDIR /usr/src/app
COPY package.json package-lock.json ./
RUN npm install
COPY . .

RUN npm run build

# make sure entrypoint.sh has correct line endings
RUN apt-get update && apt-get install -y dos2unix && dos2unix ./entrypoint.sh && chmod +x entrypoint.sh

FROM reg.app.slu.se/hub/nginx:1.25.3-alpine as runtime
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /usr/src/app/dist /var/share/nginx/html

# create and use non-root user with UID 1000.. (requires us to use a port number higher than 1024)
RUN adduser -u 1000 \
  --disabled-password \
  --home /webrunner \
  --gecos '' webrunner \
  && chown -R webrunner /webrunner

WORKDIR /webrunner
COPY --from=build /usr/src/app/entrypoint.sh ./entrypoint.sh

## add necessary permissions
RUN chown -R webrunner:webrunner /var/share/nginx/html && chmod -R 755 /var/share/nginx/html && \
    chown -R webrunner:webrunner /var/cache/nginx /var/log/nginx /etc/nginx/conf.d && \
    touch /var/run/nginx.pid && chown -R webrunner:webrunner /var/run/nginx.pid

USER webrunner

EXPOSE 4200

# entrypoint.sh makes sure the right config.js is replacing the config.js served from assets...
ENTRYPOINT ["/webrunner/entrypoint.sh"]
