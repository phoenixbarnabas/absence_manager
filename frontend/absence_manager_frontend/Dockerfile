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

# töröljük az nginx default tartalmat
RUN rm -rf /usr/share/nginx/html/*

# nginx config
COPY frontend/absence_manager_frontend/nginx.conf /etc/nginx/nginx.conf

# Angular build bemásolása (EZ HIÁNYZOTT!)
COPY --from=build /usr/src/app/dist/absence_manager_frontend/browser/ /usr/share/nginx/html/

# runtime config template (rootba!)
COPY frontend/absence_manager_frontend/public/config.example.json /usr/share/nginx/html/config.example.json

# entrypoint
COPY frontend/absence_manager_frontend/entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

ENTRYPOINT ["/entrypoint.sh"]
CMD ["nginx", "-g", "daemon off;"]