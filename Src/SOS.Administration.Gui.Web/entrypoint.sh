#!/bin/sh
cp  /var/share/nginx/html/assets/_configs/${SPAENVIRONMENT}/config.js /var/share/nginx/html/assets/
/usr/sbin/nginx -g "daemon off;"
