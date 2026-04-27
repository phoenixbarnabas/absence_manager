### STAGE 1: Build ###
FROM node:latest AS build

WORKDIR /usr/src/app

# csak a dependency fájlok
COPY frontend/absence_manager_frontend/package.json frontend/absence_manager_frontend/package-lock.json ./
RUN npm install

# teljes Angular app
COPY frontend/absence_manager_frontend/. .
RUN npm run build -- --configuration production


### STAGE 2: Run ###
FROM nginx:1.17.1-alpine

# nginx config (feltételezve, hogy itt van)
COPY frontend/absence_manager_frontend/nginx.conf /etc/nginx/nginx.conf

# a buildelt Angular app bemásolása az nginx rootba
COPY --from=build /usr/src/app/dist/absence_manager_frontend/browser /usr/share/nginx/html
COPY frontend/absence_manager_frontend/entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh
ENV BACKEND_URL=""
ENTRYPOINT ["/entrypoint.sh"]
CMD ["nginx", "-g", "daemon off;"]