### STAGE 1: Build ###
FROM node:20-alpine AS build

WORKDIR /usr/src/app

COPY frontend/absence_manager_frontend/package.json frontend/absence_manager_frontend/package-lock.json ./
RUN npm ci

COPY frontend/absence_manager_frontend/. .
RUN npm run build -- --configuration production


### STAGE 2: Run ###
FROM nginx:1.27-alpine

RUN apk add --no-cache gettext

COPY frontend/absence_manager_frontend/config.example.json /usr/share/nginx/html/assets/config.example.json
COPY frontend/absence_manager_frontend/entrypoint.sh /entrypoint.sh

RUN chmod +x /entrypoint.sh

ENTRYPOINT ["/entrypoint.sh"]
CMD ["nginx", "-g", "daemon off;"]