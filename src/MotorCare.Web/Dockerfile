FROM node:24-alpine AS build
WORKDIR /app

COPY MotorCare.Web/package*.json ./
RUN npm ci

COPY MotorCare.Web/ ./
RUN npm run build

FROM nginx:1.27-alpine
COPY MotorCare.Web/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/motorcare-web/browser /usr/share/nginx/html
EXPOSE 8080
HEALTHCHECK --interval=20s --timeout=5s --retries=10 CMD wget -qO- http://127.0.0.1:8080/ >/dev/null 2>&1 || exit 1
