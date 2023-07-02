### STAGE 1: Build ###
FROM node:16-alpine as builder

COPY package.json ./
RUN npm set progress=false && npm config set depth 0 && npm cache clean --force
RUN npm i --force && mkdir /ng-app && cp -R ./node_modules ./ng-app
WORKDIR /ng-app
COPY . .

RUN $(npm bin)/ng build --configuration production --aot=true
### STAGE 2: Setup ###
FROM nginx:1.21

COPY nginx/default.conf /etc/nginx/conf.d/
WORKDIR /usr/share/nginx/html/
RUN rm -rf /usr/share/nginx/html/*
COPY --from=builder /ng-app/dist/angular /usr/share/nginx/html

EXPOSE 80

