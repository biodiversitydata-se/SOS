#!/bin/sh
cp  /var/share/nginx/html/assets/_configs/${SPAENVIRONMENT}/config.js /var/share/nginx/html/assets/configv1.js
/usr/sbin/nginx -g "daemon off;"
