FROM node:22-alpine AS build
WORKDIR /app

COPY src/digital-signature-web/package*.json ./
RUN npm ci --prefer-offline

COPY src/digital-signature-web/ .
RUN npm run build -- --configuration production

FROM nginx:alpine AS runtime
COPY --from=build /app/dist/digital-signature-web/browser /usr/share/nginx/html
COPY deploy/docker/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
