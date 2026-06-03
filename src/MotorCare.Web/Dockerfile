FROM node:24-alpine AS builder
WORKDIR /app

COPY MotorCare.Web/package*.json ./
RUN npm ci

COPY MotorCare.Web/ ./
ENV NEXT_TELEMETRY_DISABLED=1

ARG NEXT_PUBLIC_API_BASE_URL=
ARG NEXT_PUBLIC_APP_URL=https://staging.bakimsuite.com
ARG NEXT_PUBLIC_PUBLIC_APP_URL=
ARG NEXT_PUBLIC_APP_NAME=GarajPass
ARG API_BASE_URL=http://localhost:5102
ENV NEXT_PUBLIC_API_BASE_URL=$NEXT_PUBLIC_API_BASE_URL
ENV NEXT_PUBLIC_APP_URL=$NEXT_PUBLIC_APP_URL
ENV NEXT_PUBLIC_PUBLIC_APP_URL=$NEXT_PUBLIC_PUBLIC_APP_URL
ENV NEXT_PUBLIC_APP_NAME=$NEXT_PUBLIC_APP_NAME
ENV API_BASE_URL=$API_BASE_URL

RUN npm run build

FROM node:24-alpine AS runner
WORKDIR /app

ENV NODE_ENV=production
ENV NEXT_TELEMETRY_DISABLED=1
ENV PORT=8080
ENV HOSTNAME=0.0.0.0

RUN apk add --no-cache curl && \
    addgroup --system --gid 1001 nodejs && \
    adduser --system --uid 1001 nextjs

COPY --from=builder /app/.next/standalone ./
COPY --from=builder /app/.next/static ./.next/static
COPY --from=builder /app/public ./public

RUN chown -R nextjs:nodejs /app
USER nextjs

EXPOSE 8080
CMD ["node", "server.js"]
